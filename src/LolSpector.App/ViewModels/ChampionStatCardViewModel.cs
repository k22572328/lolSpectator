using LolSpector.App.Utils;
using LolSpector.Core.Models;

namespace LolSpector.App.ViewModels;

/// <summary>Presentation wrapper around one champion's aggregated stats.</summary>
public class ChampionStatCardViewModel(ChampionStatsDto stats)
{
    public string ChampionName => stats.ChampionName;
    public string GamesRecord => $"{stats.Games} 場 · {stats.Wins} 勝 {stats.Losses} 敗";
    public string WinRate => $"{stats.WinRatePercent:0.#}% 勝率";
    public string KdaLine => $"{stats.AvgKills:0.#} / {stats.AvgDeaths:0.#} / {stats.AvgAssists:0.#}（KDA {stats.Kda:0.##}）";
    public string KillParticipation => $"擊殺參與 {stats.KillParticipationPercent:0.#}%";
    public string CsLine => $"補刀 {stats.AvgCs:0.#}（{stats.CsPerMin:0.#}/分）";
    public string DamageLine => $"對英雄傷害 {stats.AvgDamageToChampions:N0}";
    public string DamageTakenLine => $"承受傷害 {stats.AvgDamageTaken:N0}";
    public string GoldLine => $"金錢 {stats.AvgGoldEarned:N0}（{stats.GoldPerMin:0.#}/分）";
    public string VisionLine => $"視野分數 {stats.AvgVisionScore:0.#}";
    public string Position => $"主要位置:{DisplayFormat.Position(stats.MostCommonPosition)}";
    public string LastPlayed => $"最近遊玩:{DisplayFormat.RelativeTime(stats.LastPlayed)}";
}
