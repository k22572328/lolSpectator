namespace LolSpector.Core.Caching;

/// <summary>
/// A finished match's data never changes, so successfully-fetched match-v5 responses
/// are cached to disk (one JSON file per matchId) to avoid re-fetching them on repeat
/// queries — especially valuable when the same 5 players are looked up again later.
/// </summary>
public class MatchCacheStore(string cacheDirectory)
{
    public string? TryGetRaw(string matchId)
    {
        var path = PathFor(matchId);
        return File.Exists(path) ? File.ReadAllText(path) : null;
    }

    public void SaveRaw(string matchId, string rawJson)
    {
        Directory.CreateDirectory(cacheDirectory);
        File.WriteAllText(PathFor(matchId), rawJson);
    }

    private string PathFor(string matchId) => Path.Combine(cacheDirectory, matchId + ".json");
}
