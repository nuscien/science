using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Security;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Trivial.Collection;
using Trivial.Net;
using Trivial.Reflection;
using Trivial.Text;
using Trivial.Users;
using Trivial.Web;

namespace Trivial.Data;

/// <summary>
/// The attachment link item model.
/// </summary>
[JsonConverter(typeof(AttachmentLinkItemConverter))]
public class AttachmentLinkItem : BaseObservableProperties, IJsonObjectHost
{
    /// <summary>
    /// Initializes a new instance of the AttachmentLinkItem class.
    /// </summary>
    public AttachmentLinkItem()
    {
    }

    /// <summary>
    /// Initializes a new instance of the AttachmentLinkItem class.
    /// </summary>
    /// <param name="uri">The URI of the attachment.</param>
    /// <param name="mime">The MIME of the attachment.</param>
    public AttachmentLinkItem(Uri uri, string mime)
        : this(uri, mime, null, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the AttachmentLinkItem class.
    /// </summary>
    /// <param name="uri">The URI of the attachment.</param>
    /// <param name="mime">The MIME value of the attachment.</param>
    /// <param name="name">The name of the attachment.</param>
    /// <param name="thumbnail">The thumbnail URI of the attachment.</param>
    public AttachmentLinkItem(Uri uri, string mime, string name, Uri thumbnail)
    {
        SetProperty(nameof(Link), uri);
        mime = mime?.Trim();
        if (!string.IsNullOrEmpty(mime)) SetProperty(nameof(ContentType), mime);
        name = name?.Trim();
        if (!string.IsNullOrEmpty(name)) SetProperty(nameof(Title), name);
        SetProperty(nameof(Thumbnail), thumbnail);
        SetProperty("Info", new JsonObjectNode());
    }

    /// <summary>
    /// Initializes a new instance of the AttachmentLinkItem class.
    /// </summary>
    /// <param name="json">The JSON input.</param>
    public AttachmentLinkItem(JsonObjectNode json)
    {
        if (json == null) return;
        SetProperty(nameof(Link), json.TryGetUriValue("url"));
        SetProperty(nameof(ContentType), json.TryGetStringTrimmedValue("mime", true));
        SetProperty(nameof(Title), json.TryGetStringTrimmedValue("title", true) ?? json.TryGetStringTrimmedValue("name", true));
        SetProperty(nameof(Thumbnail), json.TryGetUriValue("thumbnail") ?? json.TryGetUriValue("thumb"));
        SetProperty("Info", json.TryGetObjectValue("info") ?? new());
    }

    /// <summary>
    /// Gets the URI of the attachment.
    /// </summary>
    public Uri Link => GetCurrentProperty<Uri>();

    /// <summary>
    /// Gets the MIME value of the attachment.
    /// </summary>
    public string ContentType => GetCurrentProperty<string>();

    /// <summary>
    /// Gets the optional title of the attachment; or null, if no title.
    /// </summary>
    public string Title => GetCurrentProperty<string>();

    /// <summary>
    /// Gets the optional thumbnail URI of the attachment; or null, if no thumbnail.
    /// </summary>
    public Uri Thumbnail => GetCurrentProperty<Uri>();

    /// <summary>
    /// Downloads the attachment.
    /// </summary>
    /// <param name="fileName">The file name.</param>
    /// <param name="progress">The progress to report, from 0 to 1.</param>
    /// <param name="cancellationToken">The optional cancellation token.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">The attachement does not exist.</exception>
    /// <exception cref="ArgumentNullException">The argument is null.</exception>
    /// <exception cref="ArgumentException">The argument is invalid.</exception>
    /// <exception cref="SecurityException">The caller does not have the required permission.</exception>
    /// <exception cref="IOException">An I/O error.</exception>
    /// <exception cref="DirectoryNotFoundException">The specified path is invalid, such as being on an unmapped drive.</exception>
    /// <exception cref="NotSupportedException">The path of the file refers to a non-file device, such as "con:", "com1:", "lpt1:".</exception>
    public virtual Task<FileInfo> DownloadAsync(string fileName, IProgress<double> progress = null, CancellationToken cancellationToken = default)
        => Link == null ? throw new InvalidOperationException("The attachment link URI should not be null.") : HttpClientExtensions.WriteFileAsync(Link, fileName, progress, cancellationToken);

    /// <summary>
    /// Converts to JSON object.
    /// </summary>
    /// <returns>A JSON object.</returns>
    public virtual JsonObjectNode ToJson()
    {
        var json = new JsonObjectNode
        {
            { "url", Link },
            { "mime", ContentType },
        };
        var name = Title?.Trim();
        if (!string.IsNullOrEmpty(name)) json.SetValue("title", Title);
        if (Thumbnail != null) json.SetValue("thumbnail", Thumbnail);
        var info = GetProperty<JsonObjectNode>("Info");
        if (info != null && info.Count > 0) json.SetValue("info", info);
        return json;
    }

    /// <summary>
    /// Creates a chat message record.
    /// </summary>
    /// <param name="sender">The nickname of the sender.</param>
    /// <param name="message">The message text.</param>
    /// <param name="format">The message format.</param>
    /// <param name="creation">The creation date time; or null if use now.</param>
    /// <param name="info">The additional information; or null if create a new one.</param>
    /// <returns>The chat message.</returns>
    public ExtendedChatMessage<AttachmentLinkItem> CreateMessage(UserItemInfo sender, string message, ExtendedChatMessageFormats format = ExtendedChatMessageFormats.Text, DateTime? creation = null, JsonObjectNode info = null)
        => CreateMessage(Guid.NewGuid(), sender, message, format, creation, info);

    /// <summary>
    /// Creates a chat message record.
    /// </summary>
    /// <param name="id">The message identifier.</param>
    /// <param name="sender">The nickname of the sender.</param>
    /// <param name="message">The message text.</param>
    /// <param name="format">The message format.</param>
    /// <param name="creation">The creation date time; or null if use now.</param>
    /// <param name="info">The additional information; or null if create a new one.</param>
    /// <returns>The chat message.</returns>
    public ExtendedChatMessage<AttachmentLinkItem> CreateMessage(Guid id, UserItemInfo sender, string message, ExtendedChatMessageFormats format = ExtendedChatMessageFormats.Text, DateTime? creation = null, JsonObjectNode info = null)
        => CreateMessage(ExtendedChatMessages.ToIdString(id), sender, message, format, creation, info);

    /// <summary>
    /// Creates a chat message record.
    /// </summary>
    /// <param name="id">The message identifier.</param>
    /// <param name="sender">The nickname of the sender.</param>
    /// <param name="message">The message text.</param>
    /// <param name="format">The message format.</param>
    /// <param name="creation">The creation date time; or null if use now.</param>
    /// <param name="info">The additional information; or null if create a new one.</param>
    /// <returns>The chat message.</returns>
    public ExtendedChatMessage<AttachmentLinkItem> CreateMessage(string id, UserItemInfo sender, string message, ExtendedChatMessageFormats format = ExtendedChatMessageFormats.Text, DateTime? creation = null, JsonObjectNode info = null)
        => new(ExtendedChatMessages.AttachmentLinkItemKey, id, sender, this, message, format, creation, info);

    /// <summary>
    /// Pluses two angles.
    /// leftValue + rightValue
    /// </summary>
    /// <param name="leftValue">The left value for addition operator.</param>
    /// <param name="rightValue">The right value for addition operator.</param>
    /// <returns>A result after addition.</returns>
    public static AttachmentLinkSet operator +(AttachmentLinkItem leftValue, AttachmentLinkItem rightValue)
    {
        var result = new AttachmentLinkSet(leftValue);
        result.Add(rightValue);
        return result;
    }

    /// <summary>
    /// Converts the JSON raw back.
    /// </summary>
    /// <param name="value">The source value.</param>
    /// <returns>A model of the message.</returns>
    public static implicit operator AttachmentLinkItem(JsonObjectNode value)
        => value is null ? null : new(value);

    /// <summary>
    /// Converts to JSON object.
    /// </summary>
    /// <param name="value">The JSON value.</param>
    /// <returns>A JSON object.</returns>
    public static explicit operator JsonObjectNode(AttachmentLinkItem value)
        => value?.ToJson();

    /// <summary>
    /// Converts to JSON object.
    /// </summary>
    /// <param name="value">The JSON value.</param>
    /// <returns>A JSON object.</returns>
    public static explicit operator AttachmentLinkSet(AttachmentLinkItem value)
        => new(value);
}

/// <summary>
/// JSON value node converter.
/// </summary>
internal sealed class AttachmentLinkItemConverter : JsonObjectHostConverter<AttachmentLinkItem>
{
    /// <inheritdoc />
    protected override AttachmentLinkItem Create(JsonObjectNode json)
        => new(json);
}
