using CommunityToolkit.Mvvm.ComponentModel;

namespace LolSpector.App.ViewModels;

/// <summary>Backing values for the 5-row quick-entry dialog used to fill in all Riot IDs at once.</summary>
public partial class QuickEntryViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string Id1 { get; set; } = "";

    [ObservableProperty]
    public partial string Id2 { get; set; } = "";

    [ObservableProperty]
    public partial string Id3 { get; set; } = "";

    [ObservableProperty]
    public partial string Id4 { get; set; } = "";

    [ObservableProperty]
    public partial string Id5 { get; set; } = "";

    public QuickEntryViewModel()
    {
    }

    public QuickEntryViewModel(IReadOnlyList<string> current)
    {
        if (current.Count > 0) Id1 = current[0];
        if (current.Count > 1) Id2 = current[1];
        if (current.Count > 2) Id3 = current[2];
        if (current.Count > 3) Id4 = current[3];
        if (current.Count > 4) Id5 = current[4];
    }

    public List<string> ToList() => [Id1, Id2, Id3, Id4, Id5];
}
