using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using LolSpector.App.Utils;
using LolSpector.Core.Models;

namespace LolSpector.App.ViewModels;

/// <summary>Presentation wrapper around one champion's aggregated stats. Each stat is split into Label/Value so the view can line them up in a two-column grid.</summary>
public partial class ChampionStatCardViewModel : ViewModelBase
{
    private readonly ChampionStatsDto _stats;

    public ChampionStatCardViewModel(ChampionStatsDto stats)
    {
        _stats = stats;
        _ = LoadIconAsync();
    }

    public string ChampionName => ChampionNameLocalizer.Localize(_stats.ChampionName);
    public string GamesRecord => $"{_stats.Games} 場 · {_stats.Wins} 勝 {_stats.Losses} 敗";
    public string WinRate => $"{_stats.WinRatePercent:0.#}% 勝率";
    public IBrush WinRateColor => DisplayFormat.WinRateColor(_stats.WinRatePercent);

    public string KdaLabel => "KDA";
    public string KdaValue => $"{_stats.AvgKills:0.#} / {_stats.AvgDeaths:0.#} / {_stats.AvgAssists:0.#}（{_stats.Kda:0.##}）";

    public string KillParticipationLabel => "擊殺參與";
    public string KillParticipationValue => $"{_stats.KillParticipationPercent:0}%";

    public string CsLabel => "補刀";
    public string CsValue => $"{_stats.AvgCs:0.#}（{_stats.CsPerMin:0.#}/分）";

    public string DamageShareLabel => "輸出佔比";
    public string DamageShareValue => $"{_stats.DamageSharePercent:0}%";

    public string GoldLabel => "分鐘金錢";
    public string GoldValue => $"{_stats.GoldPerMin:0.#}";

    public string VisionLabel => "視野分數";
    public string VisionValue => $"{_stats.AvgVisionScore:0.#}";

    public string PositionLabel => "主要位置";
    public string PositionValue => DisplayFormat.Position(_stats.MostCommonPosition);

    public string LastPlayedLabel => "最近遊玩";
    public string LastPlayedValue => DisplayFormat.RelativeTime(_stats.LastPlayed);

    /// <summary>Null until the icon finishes downloading/loading from cache — the view just shows nothing until then.</summary>
    [ObservableProperty]
    public partial Bitmap? Icon { get; set; }

    private async Task LoadIconAsync()
    {
        var bitmap = await ChampionIconProvider.GetIconAsync(_stats.ChampionName);
        if (bitmap is not null)
        {
            Dispatcher.UIThread.Post(() => Icon = bitmap);
        }
    }
}
