using System.Text.Json;

namespace LolSpector.App.Utils;

/// <summary>
/// Maps a match-v5 championName (e.g. "MonkeyKing") to its Traditional Chinese
/// display name via Riot's public Data Dragon static data, so lolSpector can show
/// 中文角色名稱 instead of the raw English key. Falls back to the English key
/// when offline or for a champion Data Dragon doesn't know about yet — never blocks
/// the app on failure.
/// </summary>
public static class ChampionNameLocalizer
{
    private static readonly HttpClient Http = new();
    private static readonly Dictionary<string, string> Names = new();
    private static readonly string CacheFile = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "lolSpector", "championNames.zh-TW.json");

    private static Task? _loadTask;

    public static Task EnsureLoadedAsync() => _loadTask ??= LoadAsync();

    public static string Localize(string championName) =>
        Names.TryGetValue(championName, out var zh) ? zh : championName;

    private static async Task LoadAsync()
    {
        if (TryLoadFromDiskCache()) return;

        try
        {
            var version = await DataDragonVersion.GetAsync();
            var championJson = await Http.GetStringAsync($"https://ddragon.leagueoflegends.com/cdn/{version}/data/zh_TW/champion.json");
            ParseInto(championJson);

            Directory.CreateDirectory(Path.GetDirectoryName(CacheFile)!);
            await File.WriteAllTextAsync(CacheFile, championJson);
        }
        catch
        {
            // 抓不到中文對照表就沿用英文角色名稱,不影響查詢主要功能
        }
    }

    private static bool TryLoadFromDiskCache()
    {
        if (!File.Exists(CacheFile)) return false;
        try
        {
            ParseInto(File.ReadAllText(CacheFile));
            return Names.Count > 0;
        }
        catch
        {
            return false;
        }
    }

    private static void ParseInto(string championJson)
    {
        using var doc = JsonDocument.Parse(championJson);
        foreach (var champion in doc.RootElement.GetProperty("data").EnumerateObject())
        {
            var zhName = champion.Value.GetProperty("name").GetString();
            if (zhName is not null) Names[champion.Name] = zhName;
        }
    }
}
