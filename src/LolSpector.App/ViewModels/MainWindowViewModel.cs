using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LolSpector.Core.Caching;
using LolSpector.Core.Config;
using LolSpector.Core.Models;
using LolSpector.Core.RiotApi;
using LolSpector.Core.Services;

namespace LolSpector.App.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly SettingsStore _settingsStore;
    private readonly HttpClient _httpClient = new();
    private readonly RiotRateLimiter _rateLimiter = new();

    private AppSettings _settings;
    private RiotApiClient _apiClient;
    private MatchCacheStore _matchCache;
    private PlayerLookupService _lookupService;

    public ObservableCollection<PlayerPanelViewModel> Players { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(OpenQuickEntryCommand))]
    public partial bool IsQuerying { get; set; }

    [ObservableProperty]
    public partial string StatusText { get; set; } = "點選「輸入名單」一次填入 5 組 Riot ID(格式:遊戲名稱#TAG)。";

    /// <summary>Raised when the user asks to open Settings; the View owns showing the dialog.</summary>
    public event Action? SettingsRequested;

    /// <summary>Raised when the user asks to open the 5-row quick-entry dialog; the View owns showing it.</summary>
    public event Action? QuickEntryRequested;

    public MainWindowViewModel()
    {
        _settingsStore = new SettingsStore();
        _settings = _settingsStore.Load();
        _matchCache = new MatchCacheStore(_settingsStore.CacheDirectory);
        _apiClient = new RiotApiClient(_httpClient, _rateLimiter, _settings.Platform) { ApiKey = _settings.ApiKey };
        _lookupService = new PlayerLookupService(_apiClient, _matchCache);

        for (var i = 0; i < 5; i++)
        {
            var panel = new PlayerPanelViewModel();
            if (i < _settings.RecentRiotIds.Count) panel.RiotIdInput = _settings.RecentRiotIds[i];
            Players.Add(panel);
        }
    }

    public AppSettings CurrentSettings => _settings;

    public void ApplySettings(AppSettings updated)
    {
        _settings = updated;
        _settingsStore.Save(_settings);
        _apiClient.ApiKey = _settings.ApiKey;
    }

    [RelayCommand]
    private void OpenSettings() => SettingsRequested?.Invoke();

    [RelayCommand(CanExecute = nameof(CanOpenQuickEntry))]
    private void OpenQuickEntry() => QuickEntryRequested?.Invoke();

    private bool CanOpenQuickEntry() => !IsQuerying;

    /// <summary>Current Riot ID inputs, in panel order — used to prefill the quick-entry dialog.</summary>
    public IReadOnlyList<string> CurrentRiotIds => [.. Players.Select(p => p.RiotIdInput)];

    /// <summary>Called by the View once the quick-entry dialog is submitted: writes the 5 IDs back onto the panels and runs the query.</summary>
    public async Task ApplyQuickEntryAndQueryAsync(IReadOnlyList<string> ids)
    {
        for (var i = 0; i < Players.Count && i < ids.Count; i++)
        {
            Players[i].RiotIdInput = ids[i];
        }
        await RunQueryAsync();
    }

    private async Task RunQueryAsync()
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            StatusText = "尚未設定 API Key,請先點選右上角「設定」輸入。";
            SettingsRequested?.Invoke();
            return;
        }

        var activePlayers = Players.Where(p => !string.IsNullOrWhiteSpace(p.RiotIdInput)).ToList();
        if (activePlayers.Count == 0)
        {
            StatusText = "請至少輸入一組 Riot ID。";
            return;
        }

        IsQuerying = true;
        StatusText = "查詢中...";
        foreach (var panel in activePlayers) panel.BeginLoading();

        var sharedMatchCache = new ConcurrentDictionary<string, MatchDto>();
        var tasks = activePlayers.Select(panel => QueryOneAsync(panel, sharedMatchCache));
        await Task.WhenAll(tasks);

        _settings.RecentRiotIds = [.. activePlayers.Select(p => p.RiotIdInput)];
        _settingsStore.Save(_settings);

        IsQuerying = false;
        var failed = activePlayers.Count(p => p.HasError);
        StatusText = failed == 0
            ? "查詢完成。"
            : $"查詢完成,其中 {failed} 位查詢失敗,詳見個別面板錯誤訊息。";
    }

    private async Task QueryOneAsync(PlayerPanelViewModel panel, ConcurrentDictionary<string, MatchDto> sharedMatchCache)
    {
        try
        {
            var result = await _lookupService.GetPlayerAsync(panel.RiotIdInput, _settings.SampleSize, _settings.SampleWindowMonths, sharedMatchCache);
            panel.ApplyResult(result, _settings);
        }
        catch (FormatException ex)
        {
            panel.ApplyError(ex.Message);
        }
        catch (RiotApiException ex)
        {
            panel.ApplyError(ex.Message);
        }
        catch (Exception ex)
        {
            panel.ApplyError($"查詢失敗:{ex.Message}");
        }
    }
}
