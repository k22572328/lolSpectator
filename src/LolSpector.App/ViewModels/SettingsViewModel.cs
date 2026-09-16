using CommunityToolkit.Mvvm.ComponentModel;
using LolSpector.Core.Config;

namespace LolSpector.App.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string ApiKey { get; set; } = "";

    [ObservableProperty]
    public partial int SampleSize { get; set; } = 20;

    [ObservableProperty]
    public partial int SampleWindowMonths { get; set; } = 3;

    [ObservableProperty]
    public partial int TopChampionCount { get; set; } = 5;

    public SettingsViewModel()
    {
    }

    public SettingsViewModel(AppSettings current)
    {
        LoadFrom(current);
    }

    public void LoadFrom(AppSettings current)
    {
        ApiKey = current.ApiKey;
        SampleSize = current.SampleSize;
        SampleWindowMonths = current.SampleWindowMonths;
        TopChampionCount = current.TopChampionCount;
    }

    /// <summary>Writes the edited values back onto <paramref name="target"/> (clamped to sane ranges) and returns it.</summary>
    public AppSettings ApplyTo(AppSettings target)
    {
        target.ApiKey = ApiKey.Trim();
        target.SampleSize = Math.Clamp(SampleSize, 1, 100);
        target.SampleWindowMonths = Math.Clamp(SampleWindowMonths, 1, 24);
        target.TopChampionCount = Math.Clamp(TopChampionCount, 1, 20);
        return target;
    }
}
