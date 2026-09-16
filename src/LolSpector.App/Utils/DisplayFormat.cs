using Avalonia.Media;

namespace LolSpector.App.Utils;

/// <summary>Chinese display labels for Riot's English enum-ish strings. Presentation-only — kept out of Core.</summary>
public static class DisplayFormat
{
    private static readonly Color WinRateLow = Color.FromRgb(224, 76, 76);    // 低勝率:深紅
    private static readonly Color WinRateMid = Color.FromRgb(180, 180, 180); // 50%:中性灰
    private static readonly Color WinRateHigh = Color.FromRgb(56, 176, 92);  // 高勝率:深綠

    /// <summary>Red→gray→green gradient keyed on win rate, deeper toward either end. 50% sits at the neutral midpoint.</summary>
    public static IBrush WinRateColor(double winRatePercent)
    {
        var t = Math.Clamp(winRatePercent / 100.0, 0.0, 1.0);
        var color = t >= 0.5 ? Lerp(WinRateMid, WinRateHigh, (t - 0.5) * 2) : Lerp(WinRateLow, WinRateMid, t * 2);
        return new SolidColorBrush(color);
    }

    private static Color Lerp(Color from, Color to, double t)
    {
        t = Math.Clamp(t, 0.0, 1.0);
        return Color.FromRgb(
            (byte)(from.R + (to.R - from.R) * t),
            (byte)(from.G + (to.G - from.G) * t),
            (byte)(from.B + (to.B - from.B) * t));
    }

    public static string Position(string teamPosition) => teamPosition switch
    {
        "TOP" => "上路",
        "JUNGLE" => "打野",
        "MIDDLE" => "中路",
        "BOTTOM" => "下路",
        "UTILITY" => "輔助",
        _ => "未知",
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
}
