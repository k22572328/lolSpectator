using Avalonia.Media;
using LolSpector.App.Utils;
using LolSpector.Core.Models;

namespace LolSpector.App.ViewModels;

/// <summary>Presentation wrapper around one sampled match — display strings only, no mutable state.</summary>
public class RecentMatchViewModel(MatchParticipantSample sample)
{
    public bool Win => sample.Win;
    public string ResultText => sample.Win ? "勝利" : "落敗";
    public IBrush ResultBackground { get; } = new SolidColorBrush(sample.Win ? Colors.SeaGreen : Colors.IndianRed, 0.18);
    public string QueueLabel => DisplayFormat.QueueLabel(sample.QueueId);
    public string ChampionName => sample.ChampionName;
    public string Position => DisplayFormat.Position(sample.Position);
    public string Kda => $"{sample.Kills}/{sample.Deaths}/{sample.Assists}";
    public string CsLine => $"{sample.Cs} 補刀 ({PerMinute(sample.Cs):0.0}/分)";
    public string DamageLine => $"{sample.DamageToChampions:N0} 傷害";
    public string GoldLine => $"{sample.GoldEarned:N0} 金錢";
    public string VisionLine => $"視野 {sample.VisionScore}";
    public string DurationText => DisplayFormat.Duration(sample.GameDuration);
    public string RelativeTime => DisplayFormat.RelativeTime(sample.GameStart);

    private double PerMinute(int value) => sample.GameDuration.TotalMinutes <= 0 ? 0 : value / sample.GameDuration.TotalMinutes;
}
