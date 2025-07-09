using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trivial.Globalization;

/// <summary>
/// The utilities of date and time.
/// </summary>
public static class DateTimeUtilities
{
    private static readonly string[] clockEmojiItems = new[] { "🕛", "🕐", "🕑", "🕒", "🕓", "🕔", "🕕", "🕖", "🕗", "🕘", "🕙", "🕚", "🕧", "🕜", "🕝", "🕞", "🕟", "🕠", "🕡", "🕢", "🕣", "🕤", "🕥", "🕦" };

    /// <summary>
    /// Gets the emoji of the time.
    /// </summary>
    /// <param name="time">The time to get the emoji.</param>
    /// <returns>A char of the time. Only o'clock and half.</returns>
    public static string ToEmoji(DateTime time)
    {
        var h = time.Hour % 12;
        if (time.Minute <= 15)
            return clockEmojiItems[h];
        if (time.Minute > 45)
        {
            h++;
            if (h > 12) h -= 12;
            return clockEmojiItems[h];
        }

        return clockEmojiItems[h + 12];
    }

    /// <summary>
    /// Gets the date time for social network message.
    /// Returns time if the date is today; month, day and time if the year is the year of today; or full short date and time.
    /// </summary>
    /// <param name="time">The date time.</param>
    /// <param name="culture">The optional culture information.</param>
    /// <returns>A string about the date time.</returns>
    public static string ToMessageTimeString(DateTime time, CultureInfo culture = null)
    {
        culture ??= CultureInfo.CurrentUICulture ?? CultureInfo.CurrentCulture;
        var now = new DateTime();
        if (now.Year != time.Year)
        {
            var key = GetCultureFamilyName(culture);
            if (key == "zh")
            {
                var year = now.Year - time.Year;
                switch (year)
                {
                    case 1:
                        return time.ToString("'去年'M'月'd'日'HH:mm");
                    case -1:
                        return time.ToString("'明年'M'月'd'日'HH:mm");
                }
            }
            else if (key == "ja")
            {
                var year = now.Year - time.Year;
                switch (year)
                {
                    case 1:
                        return time.ToString("'昨年'M'月'd'日'HH:mm");
                    case -1:
                        return time.ToString("'明年'M'月'd'日'HH:mm");
                }
            }

            return time.ToString("g");
        }

        var days = (int)(now.Date - time.Date).TotalDays;
        switch (days)
        {
            case 0:
                {
                    return time.ToString("T");
                }
            case 1:
                {
                    var abbr = GetYesterdayAbbreviation(culture);
                    if (abbr != null) return string.Concat(abbr, ' ', time.ToString("t"));
                    break;
                }
            case -1:
                {
                    var abbr = GetTomorrowAbbreviation(culture);
                    if (abbr != null) return string.Concat(abbr, ' ', time.ToString("t"));
                    break;
                }
            case 2:
                {
                    var key = GetCultureFamilyName(culture);
                    if (key == "zh") return string.Concat("前天 ", time.ToString("t"));
                    break;
                }
            case -2:
                {
                    var key = GetCultureFamilyName(culture);
                    if (key == "zh") return string.Concat("后天 ", time.ToString("t"));
                    break;
                }
        }

        var pattern = culture?.DateTimeFormat?.MonthDayPattern ?? "MMM dd";
        var cultureKey = GetCultureFamilyName(culture);
        if (cultureKey == "zh") return time.ToString("M'月'd'日'HH:mm");
        pattern = pattern.Replace("MMMM", "MMM");
        return string.Concat(time.ToString(pattern), ' ', time.ToString("t"));
    }

    private static string GetCultureFamilyName(CultureInfo culture)
    {
        var key = culture?.Name;
        if (string.IsNullOrWhiteSpace(key)) return null;
        var i = key.IndexOf('-');
        if (i > 0) key = key.Substring(0, i);
        return key;
    }

    private static string GetYesterdayAbbreviation(CultureInfo culture)
    {
        var key = GetCultureFamilyName(culture);
        return key switch
        {
            "en" => "YTD",
            "fr" => "hier",
            "zh" => "昨天",
            "ja" => "昨日",
            "ko" => "어제",
            //"la" => "hesterno",
            "es" => "ayer",
            "pt" => "ontem",
            "de" => "gestern",
            "el" => "εχθές",
            _ => null
        };
    }

    private static string GetTomorrowAbbreviation(CultureInfo culture)
    {
        var key = GetCultureFamilyName(culture);
        return key switch
        {
            "en" => "TMR",
            "fr" => "demain",
            "zh" => "明天",
            "ja" => "明日",
            "ko" => "내일",
            "la" => "cras",
            "es" => "mañana",
            "pt" => "amanhã",
            "de" => "morgen",
            "el" => "αύριο",
            _ => null
        };
    }
}
