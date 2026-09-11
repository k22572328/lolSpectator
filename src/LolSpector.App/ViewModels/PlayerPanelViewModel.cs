using System.Collections.ObjectModel;
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

    public ObservableCollection<RecentMatchViewModel> RecentMatches { get; } = [];
    public ObservableCollection<ChampionStatCardViewModel> SoloChampions { get; } = [];
    public ObservableCollection<ChampionStatCardViewModel> FlexChampions { get; } = [];

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
        RecentMatches.Clear();
        SoloChampions.Clear();
        FlexChampions.Clear();
    }

    public void ApplyResult(PlayerResult result, AppSettings settings)
    {
        IsLoading = false;
        ErrorMessage = null;
        DisplayName = result.RiotId;

        SoloRankSummary = FormatRank(result.LeagueEntries, RankedQueue.SoloDuoType);
        FlexRankSummary = FormatRank(result.LeagueEntries, RankedQueue.FlexType);

        var recentSolo = result.SoloSamples.Take(settings.RecentMatchCount);
        var recentFlex = result.FlexSamples.Take(settings.RecentMatchCount);
        var recent = recentSolo.Concat(recentFlex)
            .OrderByDescending(s => s.GameStart)
            .Select(s => new RecentMatchViewModel(s));
        RecentMatches.Clear();
        foreach (var match in recent) RecentMatches.Add(match);

        var soloStats = ChampionStatsAggregator.Aggregate(result.SoloSamples, RankedQueue.SoloDuoType, settings.TopChampionCount);
        SoloChampions.Clear();
        foreach (var champ in soloStats) SoloChampions.Add(new ChampionStatCardViewModel(champ));

        var flexStats = ChampionStatsAggregator.Aggregate(result.FlexSamples, RankedQueue.FlexType, settings.TopChampionCount);
        FlexChampions.Clear();
        foreach (var champ in flexStats) FlexChampions.Add(new ChampionStatCardViewModel(champ));
    }

    private static string FormatRank(List<LeagueEntryDto> entries, string queueType)
    {
        var entry = entries.FirstOrDefault(e => e.QueueType == queueType);
        if (entry is null) return "未定位";
        return $"{DisplayFormat.Tier(entry.Tier)} {entry.Rank} · {entry.LeaguePoints} LP · {entry.Wins} 勝 {entry.Losses} 敗";
    }
}
