using Avalonia.Media.Imaging;

namespace LolSpector.App.Utils;

/// <summary>
/// Downloads and caches champion square icons from Riot's public Data Dragon CDN,
/// keyed by the same championName ("MonkeyKing" etc.) used elsewhere. Icons are
/// cached to disk per patch version and kept in memory once decoded, so repeat
/// champions (across panels, across queries) never re-download. Returns null on
/// any failure (offline, unknown champion) — callers just skip showing an icon.
/// </summary>
public static class ChampionIconProvider
{
    private static readonly HttpClient Http = new();
    private static readonly Dictionary<string, Bitmap> Cache = new();
    private static readonly string CacheDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "lolSpector", "championIcons");

    public static async Task<Bitmap?> GetIconAsync(string championName)
    {
        if (Cache.TryGetValue(championName, out var cached)) return cached;

        try
        {
            var version = await DataDragonVersion.GetAsync();
            var filePath = Path.Combine(CacheDir, version, $"{championName}.png");

            if (!File.Exists(filePath))
            {
                var bytes = await Http.GetByteArrayAsync($"https://ddragon.leagueoflegends.com/cdn/{version}/img/champion/{championName}.png");
                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                await File.WriteAllBytesAsync(filePath, bytes);
            }

            await using var stream = File.OpenRead(filePath);
            var bitmap = new Bitmap(stream);
            Cache[championName] = bitmap;
            return bitmap;
        }
        catch
        {
            return null;
        }
    }
}
