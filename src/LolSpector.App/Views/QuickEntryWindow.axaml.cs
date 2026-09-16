using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using LolSpector.App.ViewModels;

namespace LolSpector.App.Views;

public partial class QuickEntryWindow : Window
{
    public bool Submitted { get; private set; }

    public QuickEntryWindow()
    {
        InitializeComponent();
        Opened += (_, _) => Dispatcher.UIThread.Post(() => Row1.Focus());
    }

    public QuickEntryWindow(IReadOnlyList<string> currentIds) : this()
    {
        DataContext = new QuickEntryViewModel(currentIds);
    }

    public List<string> GetIds() => ((QuickEntryViewModel)DataContext!).ToList();

    private void OnQueryClick(object? sender, RoutedEventArgs e)
    {
        Submitted = true;
        Close();
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        Submitted = false;
        Close();
    }

    private void OnClearClick(object? sender, RoutedEventArgs e)
    {
        ((QuickEntryViewModel)DataContext!).Clear();
        Row1.Focus();
    }
}
