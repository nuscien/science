using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Trivial.Users;
using Trivial.Web;

namespace Trivial.Text;

internal static class TextHelper
{
    public static bool IsCjkv(char c)
        => (c >= 0x3400 && c < 0xA000) || (c >= 0x20000 && c < 0x2EF00) || (c >= 0x30000 && c < 0x323B0);

    public static bool IsCjkv(string s, bool all)
    {
        if (s == null) return false;
        if (all)
        {
            foreach (var c in s)
            {
                if (!IsCjkv(c)) return false;
            }

            return true;
        }
        else
        {
            foreach (var c in s)
            {
                if (IsCjkv(c)) return true;
            }

            return false;
        }
    }

    public static bool IsNotWhiteSpace(string s)
        => !string.IsNullOrWhiteSpace(s);

    public static IBasicPublisherInfo ToPublisherInfo(JsonObjectNode json)
    {
        if (json == null) return null;
        var type = json.TryGetStringTrimmedValue("type", true);
        if (type == null) return json.Deserialize<PublisherBasicInfo>();
        return type.ToLowerInvariant() switch
        {
            "organization" or "org" => json.Deserialize<OrgAccountItemInfo>(),
            _ => json.Deserialize<PublisherBasicInfo>()
        };
    }

    public static IBasicPublisherInfo ToPublisherInfo(JsonObjectNode json, string propertyKey)
        => ToPublisherInfo(json?.TryGetObjectValue(propertyKey));
}
