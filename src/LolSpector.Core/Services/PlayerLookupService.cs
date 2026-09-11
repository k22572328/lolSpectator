using System.Collections.Concurrent;
using System.Text.Json;
using LolSpector.Core.Caching;
using LolSpector.Core.Models;
using LolSpector.Core.RiotApi;

namespace LolSpector.Core.Services;

/// <summary>
/// Orchestrates the full Riot API call chain for one player: Riot ID → puuid →
/// league entries → ranked match samples (both queues).
/// </summary>
public class PlayerLookupService(RiotApiClient api, MatchCacheStore cache)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// <paramref name="sharedMatchCache"/> is shared across all players in one "查詢" run so that
    /// a match shared by several of the 5 looked-up players (e.g. a premade group) is only fetched once.
    /// </summary>
    public async Task<PlayerResult> GetPlayerAsync(
        string riotIdInput,
        int sampleSize,
        int sampleWindowMonths,
        ConcurrentDictionary<string, MatchDto> sharedMatchCache,
        CancellationToken ct = default)
    {
        var (gameName, tagLine) = ParseRiotId(riotIdInput);
        var startTime = DateTimeOffset.UtcNow.AddMonths(-sampleWindowMonths).ToUnixTimeSeconds();

        var account = await api.GetAccountByRiotIdAsync(gameName, tagLine, ct);
        var leagueEntries = await api.GetLeagueEntriesByPuuidAsync(account.Puuid, ct);

        var soloSamples = await FetchQueueSamplesAsync(account.Puuid, RankedQueue.SoloDuoId, sampleSize, startTime, sharedMatchCache, ct);
        var flexSamples = await FetchQueueSamplesAsync(account.Puuid, RankedQueue.FlexId, sampleSize, startTime, sharedMatchCache, ct);

        return new PlayerResult
        {
            RiotId = $"{account.GameName}#{account.TagLine}",
            Puuid = account.Puuid,
            LeagueEntries = leagueEntries,
            SoloSamples = soloSamples,
            FlexSamples = flexSamples,
        };
    }

    private async Task<List<MatchParticipantSample>> FetchQueueSamplesAsync(
        string puuid,
        int queueId,
        int count,
        long startTimeEpochSeconds,
        ConcurrentDictionary<string, MatchDto> sharedMatchCache,
        CancellationToken ct)
    {
        var matchIds = await api.GetMatchIdsByPuuidAsync(puuid, queueId, count, startTimeEpochSeconds, ct);
        var samples = new List<MatchParticipantSample>(matchIds.Count);

        foreach (var matchId in matchIds)
        {
            var match = await GetMatchAsync(matchId, sharedMatchCache, ct);
            var sample = BuildSample(matchId, match, puuid);
            if (sample is not null) samples.Add(sample);
        }

        return [.. samples.OrderByDescending(s => s.GameStart)];
    }

    private async Task<MatchDto> GetMatchAsync(string matchId, ConcurrentDictionary<string, MatchDto> sharedMatchCache, CancellationToken ct)
    {
        if (sharedMatchCache.TryGetValue(matchId, out var cached))
            return cached;

        var rawFromDisk = cache.TryGetRaw(matchId);
        if (rawFromDisk is not null)
        {
            var fromDisk = JsonSerializer.Deserialize<MatchDto>(rawFromDisk, JsonOptions);
            if (fromDisk is not null)
            {
                sharedMatchCache[matchId] = fromDisk;
                return fromDisk;
            }
        }

        var (dto, raw) = await api.GetMatchByIdRawAsync(matchId, ct);
        cache.SaveRaw(matchId, raw);
        sharedMatchCache[matchId] = dto;
        return dto;
    }

    private static MatchParticipantSample? BuildSample(string matchId, MatchDto match, string puuid)
    {
        var participant = match.Info.Participants.FirstOrDefault(p => p.Puuid == puuid);
        if (participant is null) return null;

        var team = match.Info.Teams.FirstOrDefault(t => t.TeamId == participant.TeamId);
        var teamKills = team?.Objectives.Champion.Kills ?? participant.Kills + participant.Assists;

        return new MatchParticipantSample
        {
            MatchId = matchId,
            QueueId = match.Info.QueueId,
            GameStart = DateTimeOffset.FromUnixTimeMilliseconds(match.Info.GameStartTimestamp),
            GameDuration = TimeSpan.FromSeconds(match.Info.GameDuration),
            Win = participant.Win,
            ChampionId = participant.ChampionId,
            ChampionName = participant.ChampionName,
            Position = participant.TeamPosition,
            Kills = participant.Kills,
            Deaths = participant.Deaths,
            Assists = participant.Assists,
            Cs = participant.TotalMinionsKilled + participant.NeutralMinionsKilled,
            GoldEarned = participant.GoldEarned,
            DamageToChampions = participant.TotalDamageDealtToChampions,
            DamageTaken = participant.TotalDamageTaken,
            VisionScore = participant.VisionScore,
            TeamKills = teamKills,
        };
    }

    public static (string GameName, string TagLine) ParseRiotId(string input)
    {
        var trimmed = input.Trim();
        var idx = trimmed.LastIndexOf('#');
        if (idx <= 0 || idx == trimmed.Length - 1)
            throw new FormatException("Riot ID 格式錯誤,需為「遊戲名稱#TAG」,例如 Hide on bush#KR1");
        return (trimmed[..idx].Trim(), trimmed[(idx + 1)..].Trim());
    }
}
