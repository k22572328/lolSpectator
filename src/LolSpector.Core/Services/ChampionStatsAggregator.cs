using LolSpector.Core.Models;

namespace LolSpector.Core.Services;

/// <summary>Groups a queue's sampled matches by champion and computes detailed per-champion stats.</summary>
public static class ChampionStatsAggregator
{
    public static List<ChampionStatsDto> Aggregate(IReadOnlyCollection<MatchParticipantSample> samples, string queueType, int topN)
    {
        return samples
            .GroupBy(s => (s.ChampionId, s.ChampionName))
            .Select(group => BuildStats(group.Key.ChampionId, group.Key.ChampionName, queueType, [.. group]))
            .OrderByDescending(c => c.Games)
            .ThenByDescending(c => c.LastPlayed)
            .Take(topN)
            .ToList();
    }

    /// <summary>Same math as <see cref="Aggregate"/>, but collapsed across every champion into one combined line — for "this player's overall numbers regardless of champion".</summary>
    public static ChampionStatsDto? AggregateOverall(IReadOnlyCollection<MatchParticipantSample> samples) =>
        samples.Count == 0 ? null : BuildStats(championId: 0, championName: "", queueType: "", [.. samples]);

    private static ChampionStatsDto BuildStats(int championId, string championName, string queueType, List<MatchParticipantSample> games)
    {
        var wins = games.Count(s => s.Win);
        var totalDeaths = games.Sum(s => s.Deaths);
        var totalKillsAssists = games.Sum(s => s.Kills + s.Assists);

        return new ChampionStatsDto
        {
            ChampionId = championId,
            ChampionName = championName,
            QueueType = queueType,
            Games = games.Count,
            Wins = wins,
            Losses = games.Count - wins,
            WinRatePercent = 100.0 * wins / games.Count,

            AvgKills = games.Average(s => s.Kills),
            AvgDeaths = games.Average(s => s.Deaths),
            AvgAssists = games.Average(s => s.Assists),
            Kda = totalDeaths == 0 ? totalKillsAssists : (double)totalKillsAssists / totalDeaths,
            KillParticipationPercent = 100.0 * games.Average(s => s.TeamKills == 0 ? 0 : (double)(s.Kills + s.Assists) / s.TeamKills),

            AvgCs = games.Average(s => s.Cs),
            CsPerMin = games.Average(PerMinute(s => s.Cs)),
            DamageSharePercent = 100.0 * games.Average(s => s.TeamDamageToChampions == 0 ? 0 : (double)s.DamageToChampions / s.TeamDamageToChampions),
            GoldPerMin = games.Average(PerMinute(s => s.GoldEarned)),
            AvgVisionScore = games.Average(s => s.VisionScore),

            MostCommonPosition = games
                .GroupBy(s => s.Position)
                .OrderByDescending(g => g.Count())
                .First().Key,
            LastPlayed = games.Max(s => s.GameStart),
        };

        static Func<MatchParticipantSample, double> PerMinute(Func<MatchParticipantSample, int> selector) =>
            s => s.GameDuration.TotalMinutes <= 0 ? 0 : selector(s) / s.GameDuration.TotalMinutes;
    }
}
