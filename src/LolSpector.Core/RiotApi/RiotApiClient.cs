using System.Net;
using System.Text.Json;
using LolSpector.Core.Models;

namespace LolSpector.Core.RiotApi;

/// <summary>
/// Thin wrapper over the Riot Games HTTP API: handles routing (platform vs.
/// regional hosts), the X-Riot-Token header, client-side rate limiting, and
/// 429/401/404 handling. Call sites get parsed DTOs (or raw JSON for caching).
/// </summary>
public class RiotApiClient(HttpClient http, RiotRateLimiter limiter, string platform)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly string _accountRegion = RiotRouting.AccountRegionFor(platform);
    private readonly string _matchRegion = RiotRouting.MatchRegionFor(platform);

    public string ApiKey { get; set; } = "";

    public Task<RiotAccountDto> GetAccountByRiotIdAsync(string gameName, string tagLine, CancellationToken ct = default) =>
        GetJsonAsync<RiotAccountDto>(_accountRegion,
            $"/riot/account/v1/accounts/by-riot-id/{Uri.EscapeDataString(gameName)}/{Uri.EscapeDataString(tagLine)}", ct);

    public Task<List<LeagueEntryDto>> GetLeagueEntriesByPuuidAsync(string puuid, CancellationToken ct = default) =>
        GetJsonAsync<List<LeagueEntryDto>>(platform, $"/lol/league/v4/entries/by-puuid/{puuid}", ct);

    /// <summary><paramref name="startTimeEpochSeconds"/>, when given, excludes matches older than that — Riot's own server-side time filter.</summary>
    public Task<List<string>> GetMatchIdsByPuuidAsync(string puuid, int queueId, int count, long? startTimeEpochSeconds = null, CancellationToken ct = default)
    {
        var startTimeQuery = startTimeEpochSeconds is { } startTime ? $"&startTime={startTime}" : "";
        return GetJsonAsync<List<string>>(_matchRegion,
            $"/lol/match/v5/matches/by-puuid/{puuid}/ids?queue={queueId}&start=0&count={count}{startTimeQuery}", ct);
    }

    /// <summary>Returns both the parsed DTO and the raw JSON, so callers can persist the raw text to the match cache.</summary>
    public async Task<(MatchDto Dto, string Raw)> GetMatchByIdRawAsync(string matchId, CancellationToken ct = default)
    {
        var raw = await GetRawAsync(_matchRegion, $"/lol/match/v5/matches/{matchId}", ct);
        var dto = JsonSerializer.Deserialize<MatchDto>(raw, JsonOptions)
                  ?? throw new InvalidOperationException($"無法解析對戰資料:{matchId}");
        return (dto, raw);
    }

    private async Task<T> GetJsonAsync<T>(string host, string path, CancellationToken ct)
    {
        var raw = await GetRawAsync(host, path, ct);
        return JsonSerializer.Deserialize<T>(raw, JsonOptions)
               ?? throw new InvalidOperationException($"無法解析回應:{path}");
    }

    private async Task<string> GetRawAsync(string host, string path, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(ApiKey))
            throw new RiotApiException("尚未設定 API Key,請至設定頁輸入。", HttpStatusCode.Unauthorized);

        const int maxRetries = 5;
        for (var attempt = 0; attempt <= maxRetries; attempt++)
        {
            await limiter.WaitAsync(ct);

            using var request = new HttpRequestMessage(HttpMethod.Get, $"https://{host}.api.riotgames.com{path}");
            request.Headers.Add("X-Riot-Token", ApiKey);
            using var response = await http.SendAsync(request, ct);

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                var retryAfter = response.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(2);
                await Task.Delay(retryAfter + TimeSpan.FromMilliseconds(200), ct);
                continue;
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var body = await SafeReadBodyAsync(response, ct);
                throw new RiotApiException($"API Key 未通過驗證(HTTP 401){body}\n請確認設定頁貼上的是完整的 Development API Key,沒有多餘空白或缺漏字元。", response.StatusCode);
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                var body = await SafeReadBodyAsync(response, ct);
                throw new RiotApiException($"API Key 遭拒絕存取(HTTP 403){body}\n常見原因:Key 已過期需重新產生、或該 Key 尚未核准存取此 API。", response.StatusCode);
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
                throw new RiotApiException("找不到玩家或對戰資料。", response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                var body = await SafeReadBodyAsync(response, ct);
                throw new RiotApiException($"Riot API 錯誤(HTTP {(int)response.StatusCode}){body}", response.StatusCode);
            }

            return await response.Content.ReadAsStringAsync(ct);
        }

        throw new RiotApiException("多次重試後仍遭速率限制,請稍後再試。", HttpStatusCode.TooManyRequests);
    }

    private static async Task<string> SafeReadBodyAsync(HttpResponseMessage response, CancellationToken ct)
    {
        try
        {
            var text = await response.Content.ReadAsStringAsync(ct);
            return string.IsNullOrWhiteSpace(text) ? "" : $"\nRiot 回應內容:{text.Trim()}";
        }
        catch
        {
            return "";
        }
    }
}
