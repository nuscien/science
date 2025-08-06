﻿using System;
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
[JsonConverter(typeof(AttachmentLinkSetConverter))]
public class AttachmentLinkSet : IJsonObjectHost, IExtendedChatMessageDataDescription
{
    /// <summary>
    /// Initializes a new instance of the AttachmentLinkSet class.
    /// </summary>
    public AttachmentLinkSet()
    {
        Items = new();
        Info = new();
    }

    /// <summary>
    /// Initializes a new instance of the AttachmentLinkSet class.
    /// </summary>
    /// <param name="item">The attachment item.</param>
    public AttachmentLinkSet(AttachmentLinkItem item)
        : this()
    {
        if (item.Link != null) Items.Add(item);
    }

    /// <summary>
    /// Initializes a new instance of the AttachmentLinkSet class.
    /// </summary>
    /// <param name="items">The attachment items.</param>
    public AttachmentLinkSet(IEnumerable<AttachmentLinkItem> items)
        : this()
    {
        if (items == null) return;
        foreach (var item in items)
        {
            if (item?.Link != null) Items.Add(item);
        }
    }

    /// <summary>
    /// Initializes a new instance of the AttachmentLinkSet class.
    /// </summary>
    /// <param name="json">The JSON input.</param>
    public AttachmentLinkSet(JsonObjectNode json)
    {
        if (json == null) return;
        var arr = json.TryGetObjectListValue("list");
        Items = [.. arr];
        Info = json.TryGetObjectValue("info") ?? new();
    }

    string IExtendedChatMessageDataDescription.MessageType => ExtendedChatMessages.AttachmentLinkSetKey;

    /// <summary>
    /// Gets the count of attachment.
    /// </summary>
    public int Count => Items.Count;

    /// <summary>
    /// Gets all attachment items.
    /// </summary>
    public ObservableCollection<AttachmentLinkItem> Items { get; }

    /// <summary>
    /// Gets the additional information.
    /// </summary>
    public JsonObjectNode Info { get; }

    /// <summary>
    /// Gets or sets the item.
    /// </summary>
    /// <param name="index">The zero-based index.</param>
    /// <returns>The attachment item.</returns>
    /// <exception cref="ArgumentOutOfRangeException">index is not valid.</exception>
    public AttachmentLinkItem this[int index]
    {
        get => Items[index];
        set => Items[index] = value;
    }

    /// <summary>
    /// Tests if contains the specific item.
    /// </summary>
    /// <param name="item">The attachment item.</param>
    /// <returns>true if contains; otherwise, false.</returns>
    public bool Contains(AttachmentLinkItem item)
        => item != null && Items.Contains(item);

    /// <summary>
    /// Tests if contains the specific item.
    /// </summary>
    /// <param name="link">The attachment link.</param>
    /// <returns>true if contains; otherwise, false.</returns>
    public bool Contains(Uri link)
        => link != null && Items.Any(ele => ele?.Link == link);

    /// <summary>
    /// Tries to get the specific item.
    /// </summary>
    /// <param name="index">The zero-base index.</param>
    /// <returns>The attachment item; or null, if the index is not valid.</returns>
    public AttachmentLinkItem TryGet(int index)
    {
        if (index < 0 || index >= Items.Count) return null;
        try
        {
            return Items[index];
        }
        catch (ArgumentException)
        {
        }
        catch (InvalidOperationException)
        {
        }

        return null;
    }

    /// <summary>
    /// Adds an attachment.
    /// </summary>
    /// <param name="item">The attachment to add.</param>
    public void Add(AttachmentLinkItem item)
    {
        if (item?.Link == null) return;
        Items.Add(item);
    }

    /// <summary>
    /// Adds an attachment.
    /// </summary>
    /// <param name="link">The URI of the attachment.</param>
    /// <param name="mime">The MIME value of the attachment.</param>
    public AttachmentLinkItem Add(Uri link, string mime)
    {
        if (link == null) return null;
        var item = new AttachmentLinkItem(link, mime);
        Items.Add(item);
        return item;
    }

    /// <summary>
    /// Adds an attachment.
    /// </summary>
    /// <param name="link">The URI of the attachment.</param>
    /// <param name="mime">The MIME value of the attachment.</param>
    /// <param name="name">The name of the attachment.</param>
    /// <param name="thumbnail">The thumbnail URI of the attachment.</param>
    public AttachmentLinkItem Add(Uri link, string mime, string name, Uri thumbnail)
    {
        if (link == null) return null;
        var item = new AttachmentLinkItem(link, mime, name, thumbnail);
        Items.Add(item);
        return item;
    }

    /// <summary>
    /// Adds an attachment.
    /// </summary>
    /// <param name="index">The zero-based index to insert the specific item.</param>
    /// <param name="item">The attachment to add.</param>
    /// <exception cref="ArgumentOutOfRangeException">The index is not valid.</exception>
    public void Insert(int index, AttachmentLinkItem item)
    {
        if (item?.Link == null) return;
        Items.Insert(index, item);
    }

    /// <summary>
    /// Adds an attachment.
    /// </summary>
    /// <param name="items">The attachment items to add.</param>
    /// <returns>The count of the item added.</returns>
    public int AddRange(IEnumerable<AttachmentLinkItem> items)
    {
        var i = 0;
        if (items == null) return i;
        foreach (var item in items)
        {
            if (item?.Link == null) continue;
            i++;
            Items.Add(item);
        }

        return i;
    }

    /// <summary>
    /// Adds an attachment.
    /// </summary>
    /// <param name="index">The zero-based index to insert the specific item.</param>
    /// <param name="link">The URI of the attachment.</param>
    /// <param name="mime">The MIME value of the attachment.</param>
    /// <exception cref="ArgumentOutOfRangeException">The index is not valid.</exception>
    public AttachmentLinkItem Insert(int index, Uri link, string mime)
    {
        if (link == null) return null;
        var item = new AttachmentLinkItem(link, mime);
        Items.Insert(index, item);
        return item;
    }

    /// <summary>
    /// Adds an attachment.
    /// </summary>
    /// <param name="index">The zero-based index to insert the specific item.</param>
    /// <param name="link">The URI of the attachment.</param>
    /// <param name="mime">The MIME value of the attachment.</param>
    /// <param name="name">The name of the attachment.</param>
    /// <param name="thumbnail">The thumbnail URI of the attachment.</param>
    /// <exception cref="ArgumentOutOfRangeException">The index is not valid.</exception>
    public AttachmentLinkItem Insert(int index, Uri link, string mime, string name, Uri thumbnail)
    {
        if (link == null) return null;
        var item = new AttachmentLinkItem(link, mime, name, thumbnail);
        Items.Insert(index, item);
        return item;
    }

    /// <summary>
    /// Removes a specific attachment.
    /// </summary>
    /// <param name="item">The attachment to remove.</param>
    /// <returns>true if item is found and successfully removed; otherwise, false.</returns>
    public bool Remove(AttachmentLinkItem item)
        => Items.Remove(item);

    /// <summary>
    /// Downloads the attachment.
    /// </summary>
    /// <param name="index">The zero-based index to insert the specific item.</param>
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
    public Task<FileInfo> DownloadItemAsync(int index, string fileName, IProgress<double> progress = null, CancellationToken cancellationToken = default)
    {
        var item = this[index];
        if (item?.Link == null) throw new InvalidOperationException("Cannot find the attachment item.");
        return item.DownloadAsync(fileName, progress, cancellationToken);
    }

    /// <summary>
    /// Checks if all items are the same content type.
    /// </summary>
    /// <param name="mime">The content type; or null, if they are not same.</param>
    /// <returns>true if all items are the same content type; otherwise, false.</returns>
    public bool AreSameContentType(out string mime)
    {
        mime = null;
        foreach (var item in Items)
        {
            if (item is null) continue;
            if (mime == null)
            {
                mime = item.ContentType;
                continue;
            }

            if (string.Equals(item.ContentType, mime, StringComparison.OrdinalIgnoreCase)) continue;
            mime = null;
            return false;
        }

        return mime != null;
    }

    /// <summary>
    /// Checks if all items are the same content type.
    /// </summary>
    /// <param name="mime">The content type; or null, if they are not same.</param>
    /// <returns>true if all items are the same content type; otherwise, false.</returns>
    public bool AreSameContentType(string mime)
        => AreSameContentType(out var m) && string.Equals(m, mime, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Checks if all items are the same content type.
    /// </summary>
    /// <returns>true if all items are the same content type; otherwise, false.</returns>
    public bool AreSameContentType()
        => AreSameContentType(out _);

    /// <summary>
    /// Checks if the items are any of the specific content type.
    /// </summary>
    /// <param name="mime">The expected content type collection.</param>
    /// <returns>true if the items are any of the specific content type; otherwise, false.</returns>
    public bool AnyOfContentType(IEnumerable<string> mime)
    {
        if (mime == null) return false;
        foreach (var item in Items)
        {
            if (item is null) continue;
            if (mime.Contains(item.ContentType, StringComparer.OrdinalIgnoreCase)) return true;
        }

        return false;
    }

    /// <summary>
    /// Filters by content type.
    /// </summary>
    /// <param name="mime">The MIME value or suffix.</param>
    /// <returns>The attachment collection.</returns>
    public IEnumerable<AttachmentLinkItem> FilterContentType(string mime)
    {
        mime = mime?.Trim()?.ToLower();
        if (string.IsNullOrEmpty(mime))
        {
            foreach (var item in Items)
            {
                if (item?.Link == null) continue;
                yield return item;
            }

            yield break;
        }

        if (!mime.Contains('/')) mime += '/';
#if NETFRAMEWORK
        if (mime.EndsWith("/"))
#else
        if (mime.EndsWith('/'))
#endif
        {
            foreach (var item in Items)
            {
                var contentType = item.ContentType?.ToLowerInvariant();
                if (item?.Link == null || string.IsNullOrEmpty(contentType) || contentType.StartsWith(mime)) continue;
                yield return item;
            }

            yield break;
        }

        if (mime == WebFormat.StreamMIME)
        {
            foreach (var item in Items)
            {
                if (item?.Link == null) continue;
                if (string.IsNullOrEmpty(item.ContentType)) yield return item;
                if (item.ContentType?.ToLowerInvariant() != mime) continue;
                yield return item;
            }

            yield break;
        }

        foreach (var item in Items)
        {
            if (item?.Link == null || item.ContentType?.ToLowerInvariant() != mime) continue;
            yield return item;
        }
    }

    /// <summary>
    /// Converts to JSON object.
    /// </summary>
    /// <returns>A JSON object.</returns>
    public virtual JsonObjectNode ToJson()
    {
        var json = new JsonObjectNode
        {
            { "list", Items.ToJsonObjectNodes() },
        };
        if (Info.Count > 0) json.SetValue("info", Info);
        return json;
    }

    /// <summary>
    /// Converts the JSON raw back.
    /// </summary>
    /// <param name="value">The source value.</param>
    /// <returns>A model of the message.</returns>
    public static implicit operator AttachmentLinkSet(JsonObjectNode value)
        => value is null ? null : new(value);

    /// <summary>
    /// Converts to JSON object.
    /// </summary>
    /// <param name="value">The JSON value.</param>
    /// <returns>A JSON object.</returns>
    public static explicit operator JsonObjectNode(AttachmentLinkSet value)
        => value?.ToJson();
}

/// <summary>
/// JSON value node converter.
/// </summary>
internal sealed class AttachmentLinkSetConverter : JsonObjectHostConverter<AttachmentLinkSet>
{
    /// <inheritdoc />
    protected override AttachmentLinkSet Create(JsonObjectNode json)
        => new(json);
}
