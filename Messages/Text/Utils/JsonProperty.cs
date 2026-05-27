using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Trivial.Text;

/// <summary>
/// The locale property resolver of JSON object.
/// </summary>
public class LocaleJsonPropertyResolver : IJsonPropertyResolver<string>
{
    /// <summary>
    /// Gets or sets the optional current market code.
    /// </summary>
    [JsonPropertyName("mkt")]
    public string Market { get; set; }

    /// <summary>
    /// Gets or sets the optional prefix of property key.
    /// </summary>
    [JsonPropertyName("prefix")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Prefix { get; set; }

    /// <summary>
    /// Gets or sets the optional suffix of property key.
    /// </summary>
    [JsonPropertyName("suffix")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Suffix { get; set; }

    /// <summary>
    /// Gets or sets the optional fallback market code.
    /// </summary>
    [JsonPropertyName("fallback")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Fallback { get; set; }

    /// <summary>
    /// Gets or sets the optional tag object.
    /// </summary>
    [JsonIgnore]
    public object Tag { get; set; }

    /// <summary>
    /// Gets the property value.
    /// </summary>
    /// <param name="node">The source node.</param>
    /// <param name="result">The value of the property.</param>
    /// <returns>true if has the property and the type is the one expected; otherwise, false.</returns>
    public bool TryGetValue(JsonObjectNode node, out string result)
    {
        var mkt = string.IsNullOrWhiteSpace(Market) ? Market.Trim() : CultureInfo.CurrentUICulture?.Name?.Trim();
        if (node == null || string.IsNullOrEmpty(mkt))
        {
            result = null;
            return false;
        }

        string key;
        string name;
        while (true)
        {
            key = $"{Prefix}{mkt}{Suffix}";
            name = node.TryGetStringValue(key)?.Trim();
            if (!string.IsNullOrEmpty(name))
            {
                result = name;
                return true;
            }

            key = $"{Prefix}{mkt.ToLowerInvariant()}{Suffix}";
            name = node.TryGetStringValue(key)?.Trim();
            if (!string.IsNullOrEmpty(name))
            {
                result = name;
                return true;
            }

            var i = mkt.LastIndexOf('-');
            #pragma warning disable IDE0057
            if (i > 0) mkt = mkt.Substring(0, i);
            #pragma warning restore IDE0057
            else break;
        }

        key = $"{Prefix}{Fallback ?? "ww"}{Suffix}";
        result = node.TryGetStringValue(key)?.Trim();
        return string.IsNullOrEmpty(result);
    }
}
