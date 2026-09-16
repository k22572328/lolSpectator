using System.Text.Json;

namespace LolSpector.App.Utils;

/// <summary>Resolves (once, shared) the latest Data Dragon patch version string used by both champion-name and champion-icon lookups.</summary>
internal static class DataDragonVersion
{
    private static readonly HttpClient Http = new();
    private static Task<string>? _task;

    public static Task<string> GetAsync() => _task ??= FetchAsync();

    private static async Task<string> FetchAsync()
    {
        try
        {
            var json = await Http.GetStringAsync("https://ddragon.leagueoflegends.com/api/versions.json");
            var version = JsonSerializer.Deserialize<List<string>>(json)?.FirstOrDefault();
            if (version is not null) return version;
        }
        catch
        {
            // 抓不到最新版號就退回一個合理的舊版號,圖片/中文名稱對照可能會缺少最新角色
        }
        return "14.1.1";
    }
}
