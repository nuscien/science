using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Trivial.Data;
using Trivial.Reflection;
using Trivial.Tasks;
using Trivial.Text;
using Trivial.Users;

namespace Trivial.Web;

/// <summary>
/// The basic information of publisher.
/// </summary>
public interface IBasicPublisherInfo
{
    /// <summary>
    /// Gets the identifier of the publisher.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the display name of the publisher.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Gets the URI of official website.
    /// </summary>
    public Uri Website { get; }
}

/// <summary>
/// The basic information of publisher.
/// </summary>
[JsonConverter(typeof(BasePublisherInfoConverter))]
public class BasicPublisherInfo : IBasicPublisherInfo, IJsonObjectHost
{
    /// <summary>
    /// Initializes a new instance of the BasicPublisherInfo class.
    /// </summary>
    /// <param name="id">The identifier of the publisher.</param>
    /// <param name="name">The display name of the publisher.</param>
    /// <param name="website">The URI of official website.</param>
    public BasicPublisherInfo(string id, string name, Uri website = null)
    {
        Id = id;
        DisplayName = name;
        Website = website;
    }

    /// <summary>
    /// Initializes a new instance of the BotAccountItemInfo class.
    /// </summary>
    /// <param name="json">The JSON object to parse.</param>
    public BasicPublisherInfo(JsonObjectNode json)
    {
        if (json == null) return;
        Id = json.TryGetStringTrimmedValue("id", true);
        DisplayName = json.TryGetStringTrimmedValue("name", true);
        Website = json.TryGetUriValue("url");
    }

    /// <summary>
    /// Gets the identifier of the publisher.
    /// </summary>
    [JsonPropertyName("id")]
    public virtual string Id { get; }

    /// <summary>
    /// Gets the display name of the publisher.
    /// </summary>
    [JsonPropertyName("name")]
    public virtual string DisplayName { get; }

    /// <summary>
    /// Gets the URI of official website.
    /// </summary>
    [JsonPropertyName("url")]
    public virtual Uri Website { get; }

    /// <summary>
    /// Converts to JSON object.
    /// </summary>
    /// <returns>A JSON object.</returns>
    public virtual JsonObjectNode ToJson()
        => new()
        {
            { "id", Id },
            { "name", DisplayName },
            { "url", Website },
        };

    /// <summary>
    /// Returns a string that represents this instance.
    /// </summary>
    /// <returns>A string that represents this instance.</returns>
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append(DisplayName ?? "?");
        sb.Append(" (Publisher ");
        sb.Append(Id);
        sb.Append(')');
        return sb.ToString();
    }
}

/// <summary>
/// JSON value node converter.
/// </summary>
internal sealed class BasePublisherInfoConverter : JsonObjectHostConverter<BasicPublisherInfo>
{
    /// <inheritdoc />
    protected override BasicPublisherInfo Create(JsonObjectNode json)
        => new(json);
}

/// <summary>
/// JSON value node converter.
/// </summary>
internal sealed class GenericPublisherInfoConverter : JsonConverter<IBasicPublisherInfo>
{
    /// <inheritdoc />
    public override IBasicPublisherInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
            case JsonTokenType.False:
                return null;
            case JsonTokenType.String:
                var s = reader.GetString()?.Trim();
                if (string.IsNullOrEmpty(s)) return null;
                if (!s.StartsWith("https://")) return new BasicPublisherInfo(null, s);
                var url = StringExtensions.TryCreateUri(s);
                s = s.Substring(8).Trim('/');
                var i = s.IndexOf("/");
                if (i > 0) s = s.Substring(0, i);
                return new BasicPublisherInfo(null, s, url);
            case JsonTokenType.StartObject:
                var json = JsonObjectNode.ParseValue(ref reader);
                return TextHelper.ToPublisherInfo(json);
            default:
                throw new NotSupportedException("Expect a JSON object.");
        }
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, IBasicPublisherInfo value, JsonSerializerOptions options)
    {
        var json = JsonObjectNode.ConvertFrom(value, options);
        if (json is null) writer.WriteNullValue();
        else json.WriteTo(writer);
    }

    /// <summary>
    /// Gets the string in JSON format with type information and raw data.
    /// </summary>
    /// <param name="value">The publisher information instance.</param>
    /// <returns>A string with the publisher information.</returns>
    public static string GetCacheString(IBasicPublisherInfo value)
    {
        if (value is null) return null;
        JsonObjectNode json = new()
        {
            Id = value.Id,
            TypeDiscriminator = null
        };
        json.SetValue("cache", new JsonObjectNode
        {
            { "name", value.DisplayName },
            { "url", value.Website }
        });
        if (value is OrgAccountItemInfo) json.TypeDiscriminator = "org";
        else if (value.GetType() == typeof(BasicPriorities)) json.TypeDiscriminator = "basic";
        else json.SetValue("raw", JsonObjectNode.ConvertFrom(value));
        return json.ToString();
    }
}
