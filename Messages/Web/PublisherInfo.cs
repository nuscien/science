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
[JsonConverter(typeof(PublisherInfoConverter))]
public class PublisherBasicInfo : IBasicPublisherInfo, IJsonObjectHost
{
    /// <summary>
    /// Initializes a new instance of the PublisherBasicInfo class.
    /// </summary>
    /// <param name="id">The identifier of the publisher.</param>
    /// <param name="name">The display name of the publisher.</param>
    /// <param name="uri">The URI of official website.</param>
    public PublisherBasicInfo(string id, string name, Uri uri = null)
    {
        Id = id;
        DisplayName = name;
        Website = uri;
    }

    /// <summary>
    /// Initializes a new instance of the BotAccountItemInfo class.
    /// </summary>
    /// <param name="json">The JSON object to parse.</param>
    public PublisherBasicInfo(JsonObjectNode json)
    {
        if (json == null) return;
        Id = json.TryGetStringTrimmedValue("id", true);
        DisplayName = json.TryGetStringTrimmedValue("name", true);
        Website = json.TryGetUriValue("website");
    }

    /// <summary>
    /// Gets the identifier of the publisher.
    /// </summary>
    public virtual string Id { get; }

    /// <summary>
    /// Gets the display name of the publisher.
    /// </summary>
    public virtual string DisplayName { get; }

    /// <summary>
    /// Gets the URI of official website.
    /// </summary>
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
            { "website", Website },
        };
}

/// <summary>
/// JSON value node converter.
/// </summary>
internal sealed class PublisherInfoConverter : JsonObjectHostConverter<PublisherBasicInfo>
{
    /// <inheritdoc />
    protected override PublisherBasicInfo Create(JsonObjectNode json)
        => new(json);
}
