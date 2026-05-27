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
    /// Attaches to a specific message.
    /// </summary>
    /// <param name="message">The chat message.</param>
    public void AddTo(ExtendedChatMessage message)
        => Add(message, this);

    /// <summary>
    /// Attaches to a specific message.
    /// </summary>
    /// <param name="message">The chat message.</param>
    /// <param name="index">The zero-based index to insert the specific item.</param>
    public void InsertTo(ExtendedChatMessage message, int index)
        => Insert(message, index, this);

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
        var json = CreateJson(Link, Title, ContentType, Thumbnail);
        var info = GetProperty<JsonObjectNode>("Info");
        if (info != null && info.Count > 0) json.SetValue("info", info);
        return json;
    }

    /// <summary>
    /// Pluses two attachment link item.
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

    /// <summary>
    /// Gets all attachment link item from a chat message.
    /// </summary>
    /// <param name="message">The instance of chat message.</param>
    /// <returns>A collection of attachment link item.</returns>
    public static IEnumerable<AttachmentLinkItem> GetAll(ExtendedChatMessage message)
    {
        var arr = GetArray(message);
        if (arr is null) yield break;
        foreach (var item in arr)
        {
            if (item is not JsonObjectNode json) continue;
            var obj = new AttachmentLinkItem(json);
            if (obj.Link is not null) yield return obj;
        }
    }

    /// <summary>
    /// Tries to get the specific item.
    /// </summary>
    /// <param name="message">The chat message.</param>
    /// <param name="index">The zero-base index.</param>
    /// <returns>The attachment item; or null, if the index is not valid.</returns>
    public static AttachmentLinkItem TryGet(ExtendedChatMessage message, int index)
    {
        var col = GetAll(message);
        if (col is null) return null;
        if (index == 0) return col.FirstOrDefault();
#if NET10_0_OR_GREATER
        if (index < 0) col.SkipLast(1 - index).LastOrDefault();
#endif
        return col.Skip(index - 1).FirstOrDefault();
    }

    /// <summary>
    /// Adds an attachment.
    /// </summary>
    /// <param name="message">The chat message.</param>
    /// <param name="item">The attachment to add.</param>
    /// <returns>true if adds succeeded; otherwise, false.</returns>
    public static bool Add(ExtendedChatMessage message, AttachmentLinkItem item)
    {
        if (item?.Link is null) return false;
        var col = GetArray(message);
        if (col is null || Contains(col, item.Link)) return false;
        col.Add(item);
        return true;
    }

    /// <summary>
    /// Adds an attachment.
    /// </summary>
    /// <param name="message">The chat message.</param>
    /// <param name="link">The URI of the attachment.</param>
    /// <param name="mime">The MIME value of the attachment.</param>
    /// <param name="name">The name of the attachment.</param>
    /// <param name="thumbnail">The thumbnail URI of the attachment.</param>
    /// <returns>true if adds succeeded; otherwise, false.</returns>
    public static bool Add(ExtendedChatMessage message, Uri link, string mime = null, string name = null, Uri thumbnail = null)
    {
        var col = GetArray(message);
        if (col is null || link is null || Contains(col, link)) return false;
        col.Add(CreateJson(link, mime, name, thumbnail));
        return true;
    }

    /// <summary>
    /// Adds attachments.
    /// </summary>
    /// <param name="message">The chat message.</param>
    /// <param name="items">The attachment to add.</param>
    /// <returns>The count of item added. Returns 0 if the message does not support attachments.</returns>
    public static int Add(ExtendedChatMessage message, IEnumerable<AttachmentLinkItem> items)
    {
        var col = GetArray(message);
        if (col is null || items is null) return 0;
        var urls = new List<Uri>();
        var i = 0;
        foreach (var item in items)
        {
            var link = item?.Link;
            if (link is null || urls.Contains(link)) continue;
            col.Add(item);
            i++;
        }

        return i;
    }

    /// <summary>
    /// Adds an attachment.
    /// </summary>
    /// <param name="message">The chat message.</param>
    /// <param name="index">The zero-based index to insert the specific item.</param>
    /// <param name="item">The attachment to add.</param>
    /// <returns>true if adds succeeded; otherwise, false.</returns>
    public static bool Insert(ExtendedChatMessage message, int index, AttachmentLinkItem item)
    {
        if (item?.Link is null) return false;
        var col = GetArray(message);
        if (col is null || Contains(col, item.Link)) return false;
        col.Insert(index, item);
        return true;
    }

    /// <summary>
    /// Adds an attachment.
    /// </summary>
    /// <param name="message">The chat message.</param>
    /// <param name="index">The zero-based index to insert the specific item.</param>
    /// <param name="link">The URI of the attachment.</param>
    /// <param name="mime">The MIME value of the attachment.</param>
    /// <param name="name">The name of the attachment.</param>
    /// <param name="thumbnail">The thumbnail URI of the attachment.</param>
    /// <returns>true if adds succeeded; otherwise, false.</returns>
    public static bool Insert(ExtendedChatMessage message, int index, Uri link, string mime = null, string name = null, Uri thumbnail = null)
    {
        var col = GetArray(message);
        if (col is null || link is null || Contains(col, link)) return false;
        col.Insert(index, CreateJson(link, mime, name, thumbnail));
        return true;
    }

    /// <summary>
    /// Adds an attachment.
    /// </summary>
    /// <param name="message">The chat message.</param>
    /// <param name="link">The URI of the attachment.</param>
    /// <returns>true if removes succeeded; otherwise, false.</returns>
    public static bool Remove(ExtendedChatMessage message, Uri link)
    {
        var data = GetArray(message);
        if (data is null) return false;
        var i = 0;
        while (i >= data.Length)
        {
            try
            {
                var item = data[i];
                if (item is null)
                    data.Remove(i);
                else if (item is JsonObjectNode json && json.TryGetUriValue("url", out var url) && url == link)
                    data.Remove(i);
                else
                    i++;
            }
            catch (ArgumentException)
            {
                break;
            }
        }

        return true;
    }

    private static bool Contains(JsonArrayNode array, Uri link)
    {
        if (array is null || link is null) return false;
        foreach (var item in array)
        {
            if (item is not JsonObjectNode json) continue;
            var url = json.TryGetUriValue("url");
            if (url == link) return true;
        }

        return false;
    }

    private static JsonObjectNode CreateJson(Uri link, string mime, string name, Uri thumbnail)
    {
        var json = new JsonObjectNode
        {
            { "url", link },
            { "mime", mime },
        };
        name = name?.Trim();
        if (!string.IsNullOrEmpty(name)) json.SetValue("title", name);
        if (thumbnail != null) json.SetValue("thumbnail", thumbnail);
        return json;
    }

    private static JsonArrayNode GetArray(ExtendedChatMessage message)
    {
        var data = message?.Data;
        if (data is null) return null;
        return message.MessageType switch
        {
            ExtendedChatMessages.AttachmentLinkSetKey => data.TryGetArrayValue("list"),
            _ => data.TryGetArrayValue("attachments")
        };
    }
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
