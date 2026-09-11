using System.Text.Json;

namespace LolSpector.Core.Config;

/// <summary>Loads/saves AppSettings as JSON under the OS's per-user app-data folder.</summary>
public class SettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    private readonly string _filePath;

    public SettingsStore(string? filePath = null)
    {
        _filePath = filePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "lolSpector",
            "config.json");
    }

    public string CacheDirectory => Path.Combine(Path.GetDirectoryName(_filePath)!, "matchCache");

    public AppSettings Load()
    {
        try
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
                if (settings is not null) return settings;
            }
        }
        catch (Exception)
        {
            // 設定檔損毀或無法讀取時,退回預設值,不讓 App 無法啟動
        }
        return new AppSettings();
    }

    public void Save(AppSettings settings)
    {
        var dir = Path.GetDirectoryName(_filePath)!;
        Directory.CreateDirectory(dir);
        var json = JsonSerializer.Serialize(settings, JsonOptions);
        File.WriteAllText(_filePath, json);
    }
}
