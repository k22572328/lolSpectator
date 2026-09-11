namespace LolSpector.Core.Config;

public class AppSettings
{
    public string ApiKey { get; set; } = "";

    /// <summary>Platform routing host, e.g. "tw2" for the Taiwan server. Regional routing (account-v1 vs. match-v5 use different hosts) is derived from this — see RiotRouting.</summary>
    public string Platform { get; set; } = "tw2";

    /// <summary>How many recent ranked matches per queue to sample for champion stats (upper bound — see SampleWindowMonths).</summary>
    public int SampleSize { get; set; } = 20;

    /// <summary>Only sample matches from within this many months — even if that yields fewer than SampleSize games.</summary>
    public int SampleWindowMonths { get; set; } = 3;

    /// <summary>How many top champions to show per queue.</summary>
    public int TopChampionCount { get; set; } = 5;

    /// <summary>How many recent matches per queue to show in the simple recent-matches list.</summary>
    public int RecentMatchCount { get; set; } = 5;

    /// <summary>The last 5 Riot IDs the user queried, restored on next launch.</summary>
    public List<string> RecentRiotIds { get; set; } = [];
}
