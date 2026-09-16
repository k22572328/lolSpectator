using CommunityToolkit.Mvvm.ComponentModel;

namespace LolSpector.App.ViewModels;

/// <summary>
/// One entry in a player panel's own position-filter dropdown, carrying a live
/// game count for that position. Each panel owns its own set of instances
/// (created via <see cref="CreateDefaultSet"/>) so counts stay per-player.
/// </summary>
public partial class PositionFilterOption : ObservableObject
{
    public string BaseLabel { get; }

    /// <summary>Raw teamPosition ("TOP"/"JUNGLE"/...) this option filters to; null means "no filter — all positions".</summary>
    public string? Value { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Label))]
    public partial int Count { get; set; }

    public string Label => $"{BaseLabel}（{Count}）";

    public PositionFilterOption(string baseLabel, string? value)
    {
        BaseLabel = baseLabel;
        Value = value;
    }

    public static List<PositionFilterOption> CreateDefaultSet() =>
    [
        new("全部位置", null),
        new("上路", "TOP"),
        new("打野", "JUNGLE"),
        new("中路", "MIDDLE"),
        new("下路", "BOTTOM"),
        new("輔助", "UTILITY"),
    ];
}
