namespace LolSpector.Core.Models;

/// <summary>
/// One player's stat line from one ranked match — the reduced shape both the
/// recent-matches list and the champion-stats aggregator consume.
/// </summary>
public class MatchParticipantSample
{
    public string MatchId { get; set; } = "";
    public int QueueId { get; set; }
    public DateTimeOffset GameStart { get; set; }
    public TimeSpan GameDuration { get; set; }
    public bool Win { get; set; }
    public int ChampionId { get; set; }
    public string ChampionName { get; set; } = "";
    public string Position { get; set; } = "";
    public int Kills { get; set; }
    public int Deaths { get; set; }
    public int Assists { get; set; }
    public int Cs { get; set; }
    public int GoldEarned { get; set; }
    public int DamageToChampions { get; set; }
    public int DamageTaken { get; set; }
    public int VisionScore { get; set; }

    /// <summary>Total kills by this player's team in the match (for kill participation).</summary>
    public int TeamKills { get; set; }
}
