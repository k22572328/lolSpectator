using LolSpector.Core.Models;

namespace LolSpector.App.Utils;

/// <summary>Chinese display labels for Riot's English enum-ish strings. Presentation-only — kept out of Core.</summary>
public static class DisplayFormat
{
    public static string Position(string teamPosition) => teamPosition switch
    {
        "TOP" => "上路",
        "JUNGLE" => "打野",
        "MIDDLE" => "中路",
        "BOTTOM" => "下路",
        "UTILITY" => "輔助",
        _ => "未知",
    };

    public static string QueueLabel(int queueId) => queueId switch
    {
        RankedQueue.SoloDuoId => "單雙排",
        RankedQueue.FlexId => "彈性排位",
        _ => "其他",
    };

    public static string QueueLabel(string queueType) => queueType switch
    {
        RankedQueue.SoloDuoType => "單雙排",
        RankedQueue.FlexType => "彈性排位",
        _ => queueType,
    };

    public static string Tier(string tier) => tier.ToUpperInvariant() switch
    {
        "IRON" => "鋼鐵",
        "BRONZE" => "青銅",
        "SILVER" => "白銀",
        "GOLD" => "黃金",
        "PLATINUM" => "白金",
        "EMERALD" => "翡翠",
        "DIAMOND" => "鑽石",
        "MASTER" => "大師",
        "GRANDMASTER" => "宗師",
        "CHALLENGER" => "最強王者",
        _ => tier,
    };

    public static string RelativeTime(DateTimeOffset time)
    {
        var delta = DateTimeOffset.UtcNow - time;
        if (delta < TimeSpan.FromMinutes(1)) return "剛剛";
        if (delta < TimeSpan.FromHours(1)) return $"{(int)delta.TotalMinutes} 分鐘前";
        if (delta < TimeSpan.FromDays(1)) return $"{(int)delta.TotalHours} 小時前";
        if (delta < TimeSpan.FromDays(30)) return $"{(int)delta.TotalDays} 天前";
        return time.LocalDateTime.ToString("yyyy/MM/dd");
    }

    public static string Duration(TimeSpan duration) => $"{(int)duration.TotalMinutes}:{duration.Seconds:D2}";
}
