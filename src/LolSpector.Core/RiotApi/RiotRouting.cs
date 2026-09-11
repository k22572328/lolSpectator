namespace LolSpector.Core.RiotApi;

/// <summary>
/// Maps a platform routing value (e.g. "tw2") to the two *different* regional
/// routing hosts Riot's APIs actually require:
///
/// - account-v1 only recognizes AMERICAS / ASIA / EUROPE — it does not accept
///   "sea", so SEA-cluster platforms (TW2/VN2/SG2/TH2/PH2, migrated from Garena
///   in 2023) must still route account-v1 calls through "asia".
/// - match-v5 (and other newer match/game data APIs) DOES support a dedicated
///   "sea" regional route for that same platform group, and using "asia" there
///   instead doesn't error — it silently returns an empty match list.
///
/// Verified directly against the live API (2026-09) for tw2; see the other
/// platforms' groupings in Riot's routing docs.
/// </summary>
public static class RiotRouting
{
    public static string AccountRegionFor(string platform) => platform.ToLowerInvariant() switch
    {
        "na1" or "br1" or "la1" or "la2" or "oc1" => "americas",
        "kr" or "jp1" => "asia",
        "eun1" or "euw1" or "tr1" or "ru" or "me1" => "europe",
        "tw2" or "vn2" or "sg2" or "th2" or "ph2" => "asia",
        _ => "americas",
    };

    public static string MatchRegionFor(string platform) => platform.ToLowerInvariant() switch
    {
        "na1" or "br1" or "la1" or "la2" => "americas",
        "kr" or "jp1" => "asia",
        "eun1" or "euw1" or "tr1" or "ru" or "me1" => "europe",
        "oc1" or "tw2" or "vn2" or "sg2" or "th2" or "ph2" => "sea",
        _ => "americas",
    };
}
