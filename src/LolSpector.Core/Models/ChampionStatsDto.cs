namespace LolSpector.Core.Models;

/// <summary>Aggregated performance on one champion, within one ranked queue, over the sampled matches.</summary>
public class ChampionStatsDto
{
    public int ChampionId { get; set; }
    public string ChampionName { get; set; } = "";
    public string QueueType { get; set; } = "";

    public int Games { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public double WinRatePercent { get; set; }

    public double AvgKills { get; set; }
    public double AvgDeaths { get; set; }
    public double AvgAssists { get; set; }
    public double Kda { get; set; }
    public double KillParticipationPercent { get; set; }

    public double AvgCs { get; set; }
    public double CsPerMin { get; set; }
    public double AvgDamageToChampions { get; set; }
    public double AvgDamageTaken { get; set; }
    public double AvgGoldEarned { get; set; }
    public double GoldPerMin { get; set; }
    public double AvgVisionScore { get; set; }

    public string MostCommonPosition { get; set; } = "";
    public DateTimeOffset LastPlayed { get; set; }
}
