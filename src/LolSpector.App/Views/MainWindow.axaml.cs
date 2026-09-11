using Avalonia.Controls;
using LolSpector.App.ViewModels;

namespace LolSpector.App.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.SettingsRequested += () => _ = OpenSettingsAsync(vm);
            vm.QuickEntryRequested += () => _ = OpenQuickEntryAsync(vm);
        }
    }

    private async Task OpenSettingsAsync(MainWindowViewModel vm)
    {
        var window = new SettingsWindow(vm.CurrentSettings);
        await window.ShowDialog(this);
        if (window.Saved)
        {
            vm.ApplySettings(window.GetUpdatedSettings(vm.CurrentSettings));
        }
    }

    private async Task OpenQuickEntryAsync(MainWindowViewModel vm)
    {
        var window = new QuickEntryWindow(vm.CurrentRiotIds);
        await window.ShowDialog(this);
        if (window.Submitted)
        {
            await vm.ApplyQuickEntryAndQueryAsync(window.GetIds());
        }
    }
}
