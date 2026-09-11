using Avalonia.Controls;
using Avalonia.Interactivity;
using LolSpector.App.ViewModels;
using LolSpector.Core.Config;

namespace LolSpector.App.Views;

public partial class SettingsWindow : Window
{
    public bool Saved { get; private set; }

    public SettingsWindow()
    {
        InitializeComponent();
    }

    public SettingsWindow(AppSettings current) : this()
    {
        DataContext = new SettingsViewModel(current);
    }

    public AppSettings GetUpdatedSettings(AppSettings target) =>
        ((SettingsViewModel)DataContext!).ApplyTo(target);

    private void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        Saved = true;
        Close();
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        Saved = false;
        Close();
    }
}
