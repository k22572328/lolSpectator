using System.Collections.ObjectModel;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using LolSpector.App.Utils;
using LolSpector.Core.Config;
using LolSpector.Core.Models;
using LolSpector.Core.Services;

namespace LolSpector.App.ViewModels;

public partial class PlayerPanelViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string RiotIdInput { get; set; } = "";

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial string DisplayName { get; set; } = "";

    [ObservableProperty]
    public partial string SoloRankSummary { get; set; } = "尚未查詢";

    [ObservableProperty]
    public partial string FlexRankSummary { get; set; } = "尚未查詢";

    /// <summary>常用角色清單 — 依目前選取的排位/位置篩選條件重新計算後的結果。</summary>
    public ObservableCollection<ChampionStatCardViewModel> Champions { get; } = [];

    /// <summary>這個玩家自己的位置篩選選項(每人各自獨立的實例,場次數字才不會互相干擾)。</summary>
    public List<PositionFilterOption> PositionOptions { get; } = PositionFilterOption.CreateDefaultSet();

    /// <summary>這個玩家自己的位置篩選(每人各自獨立,不是全域套用)。</summary>
    [ObservableProperty]
    public partial PositionFilterOption SelectedPosition { get; set; }

    // ── 整體數據(跨英雄合併,依目前的排位/位置篩選計算)──────────────────────
    private static readonly IBrush HighlightBrush = new SolidColorBrush(Color.FromRgb(255, 193, 7));
    private static readonly IBrush NormalBrush = Brushes.White;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OverallVisionText))]
    [NotifyPropertyChangedFor(nameof(OverallDamageShareText))]
    [NotifyPropertyChangedFor(nameof(OverallGoldText))]
    public partial bool HasOverallStats { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OverallVisionText))]
    public partial double OverallVisionScore { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OverallDamageShareText))]
    public partial double OverallDamageShare { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OverallGoldText))]
    public partial double OverallGoldPerMin { get; set; }

    /// <summary>Set by MainWindowViewModel after comparing all 5 panels' overall stats.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OverallVisionText))]
    [NotifyPropertyChangedFor(nameof(VisionHighlightBrush))]
    public partial bool IsLowestVision { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OverallDamageShareText))]
    [NotifyPropertyChangedFor(nameof(DamageShareHighlightBrush))]
    public partial bool IsHighestDamageShare { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OverallGoldText))]
    [NotifyPropertyChangedFor(nameof(GoldHighlightBrush))]
    public partial bool IsHighestGoldPerMin { get; set; }

    public string OverallVisionText => !HasOverallStats ? "—" : $"{OverallVisionScore:0.#}" + (IsLowestVision ? "（全場最低）" : "");
    public string OverallDamageShareText => !HasOverallStats ? "—" : $"{OverallDamageShare:0}%" + (IsHighestDamageShare ? "（全場最高）" : "");
    public string OverallGoldText => !HasOverallStats ? "—" : $"{OverallGoldPerMin:0.#}" + (IsHighestGoldPerMin ? "（全場最高）" : "");

    public IBrush VisionHighlightBrush => IsLowestVision ? HighlightBrush : NormalBrush;
    public IBrush DamageShareHighlightBrush => IsHighestDamageShare ? HighlightBrush : NormalBrush;
    public IBrush GoldHighlightBrush => IsHighestGoldPerMin ? HighlightBrush : NormalBrush;

    // 查詢到的原始樣本,以及上一次的排位勾選狀態 — 篩選條件變動時用來重新計算 Champions,不必重新打 API。
    private List<MatchParticipantSample> _soloSamples = [];
    private List<MatchParticipantSample> _flexSamples = [];
    private int _topChampionCount = 5;
    private bool _includeSolo = true;
    private bool _includeFlex = true;
    private bool _hasResult;

    public PlayerPanelViewModel()
    {
        SelectedPosition = PositionOptions[0];
    }

    partial void OnSelectedPositionChanged(PositionFilterOption value) => RecomputeChampionsCore();

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    /// <summary>What to show as the panel's title: the resolved Riot ID after a query, otherwise whatever was typed in, otherwise a placeholder.</summary>
    public string HeaderText => !string.IsNullOrWhiteSpace(DisplayName)
        ? DisplayName
        : !string.IsNullOrWhiteSpace(RiotIdInput)
            ? RiotIdInput
            : "(尚未輸入)";

    partial void OnErrorMessageChanged(string? value) => OnPropertyChanged(nameof(HasError));
    partial void OnDisplayNameChanged(string value) => OnPropertyChanged(nameof(HeaderText));
    partial void OnRiotIdInputChanged(string value) => OnPropertyChanged(nameof(HeaderText));

    public void BeginLoading()
    {
        IsLoading = true;
        ErrorMessage = null;
    }

    public void ApplyError(string message)
    {
        IsLoading = false;
        ErrorMessage = message;
        DisplayName = RiotIdInput;
        SoloRankSummary = "—";
        FlexRankSummary = "—";
        _hasResult = false;
        _soloSamples = [];
        _flexSamples = [];
        foreach (var option in PositionOptions) option.Count = 0;
        SelectedPosition = PositionOptions[0];
        Champions.Clear();
        HasOverallStats = false;
        OnPropertyChanged(string.Empty);
    }

    public void ApplyResult(PlayerResult result, AppSettings settings, bool includeSolo, bool includeFlex)
    {
        IsLoading = false;
        ErrorMessage = null;
        DisplayName = result.RiotId;

        SoloRankSummary = FormatRank(result.LeagueEntries, RankedQueue.SoloDuoType);
        FlexRankSummary = FormatRank(result.LeagueEntries, RankedQueue.FlexType);

        _soloSamples = result.SoloSamples;
        _flexSamples = result.FlexSamples;
        _hasResult = true;
        _topChampionCount = settings.TopChampionCount;
        _includeSolo = includeSolo;
        _includeFlex = includeFlex;

        UpdatePositionCounts();

        // 每次查完直接選場次最多的位置,而不是預設「全部位置」;完全沒有資料時才退回全部位置。
        var mostPlayed = PositionOptions
            .Where(o => o.Value is not null)
            .OrderByDescending(o => o.Count)
            .FirstOrDefault(o => o.Count > 0);
        SelectedPosition = mostPlayed ?? PositionOptions[0];

        RecomputeChampionsCore();
    }

    /// <summary>Called by the main window when the (global) queue checkboxes change — position stays this panel's own choice.</summary>
    public void RecomputeChampions(int topChampionCount, bool includeSolo, bool includeFlex)
    {
        _topChampionCount = topChampionCount;
        _includeSolo = includeSolo;
        _includeFlex = includeFlex;
        RecomputeChampionsCore();
    }

    /// <summary>Re-derives the champion list, the overall (cross-champion) summary, and each dropdown option's game count from the already-fetched samples — no API calls.</summary>
    private void RecomputeChampionsCore()
    {
        Champions.Clear();
        if (!_hasResult)
        {
            HasOverallStats = false;
            return;
        }

        UpdatePositionCounts();

        var pool = PooledSamples();
        var filtered = (SelectedPosition.Value is { } positionFilter ? pool.Where(s => s.Position == positionFilter) : pool).ToList();

        var stats = ChampionStatsAggregator.Aggregate(filtered, queueType: "", _topChampionCount);
        foreach (var champ in stats) Champions.Add(new ChampionStatCardViewModel(champ));

        var overall = ChampionStatsAggregator.AggregateOverall(filtered);
        HasOverallStats = overall is not null;
        OverallVisionScore = overall?.AvgVisionScore ?? 0;
        OverallDamageShare = overall?.DamageSharePercent ?? 0;
        OverallGoldPerMin = overall?.GoldPerMin ?? 0;

        // 保險起見強制刷新這個物件上所有綁定(避免任何一條 NotifyPropertyChangedFor 漏接而讓卡片顯示卡在舊值/空白)。
        OnPropertyChanged(string.Empty);
    }

    private void UpdatePositionCounts()
    {
        var pool = PooledSamples();
        foreach (var option in PositionOptions)
        {
            option.Count = option.Value is null ? pool.Count : pool.Count(s => s.Position == option.Value);
        }
    }

    private List<MatchParticipantSample> PooledSamples()
    {
        var pool = new List<MatchParticipantSample>();
        if (_includeSolo) pool.AddRange(_soloSamples);
        if (_includeFlex) pool.AddRange(_flexSamples);
        return pool;
    }

    private static string FormatRank(List<LeagueEntryDto> entries, string queueType)
    {
        var entry = entries.FirstOrDefault(e => e.QueueType == queueType);
        if (entry is null) return "未定位";
        return $"{DisplayFormat.Tier(entry.Tier)} {entry.Rank} · {entry.LeaguePoints} LP · {entry.Wins} 勝 {entry.Losses} 敗";
    }
}
