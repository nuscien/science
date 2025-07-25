﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Trivial.Collection;
using Trivial.Data;
using Trivial.IO;
using Trivial.Reflection;
using Trivial.Tasks;
using Trivial.Users;

namespace Trivial.Text;

/// <summary>
/// The helper of extended chat message.
/// </summary>
public static class ExtendedChatMessages
{
    internal const string AttachmentLinkItemKey = "attachment\\item";
    internal const string AttachmentLinkSetKey = "attachment\\list";
    internal const string MarkdownKey = "text\\markdown";

    /// <summary>
    /// Creates a chat message record.
    /// </summary>
    /// <param name="sender">The nickname of the sender.</param>
    /// <param name="data">The message data.</param>
    /// <param name="message">The message text.</param>
    /// <param name="format">The message format.</param>
    /// <param name="creation">The creation date time; or null if use now.</param>
    /// <param name="info">The additional information; or null if create a new one.</param>
    /// <returns>The chat message.</returns>
    public static ExtendedChatMessage<AttachmentLinkItem> Create(UserItemInfo sender, AttachmentLinkItem data, string message, ExtendedChatMessageFormats format = ExtendedChatMessageFormats.Text, DateTime? creation = null, JsonObjectNode info = null)
        => Create(Guid.NewGuid(), sender, data, message, format, creation, info);

    /// <summary>
    /// Creates a chat message record.
    /// </summary>
    /// <param name="id">The message identifier.</param>
    /// <param name="sender">The nickname of the sender.</param>
    /// <param name="data">The message data.</param>
    /// <param name="message">The message text.</param>
    /// <param name="format">The message format.</param>
    /// <param name="creation">The creation date time; or null if use now.</param>
    /// <param name="info">The additional information; or null if create a new one.</param>
    /// <returns>The chat message.</returns>
    public static ExtendedChatMessage<AttachmentLinkItem> Create(Guid id, UserItemInfo sender, AttachmentLinkItem data, string message, ExtendedChatMessageFormats format = ExtendedChatMessageFormats.Text, DateTime? creation = null, JsonObjectNode info = null)
        => Create(ToIdString(id), sender, data, message, format, creation, info);

    /// <summary>
    /// Creates a chat message record.
    /// </summary>
    /// <param name="id">The message identifier.</param>
    /// <param name="sender">The nickname of the sender.</param>
    /// <param name="data">The message data.</param>
    /// <param name="message">The message text.</param>
    /// <param name="format">The message format.</param>
    /// <param name="creation">The creation date time; or null if use now.</param>
    /// <param name="info">The additional information; or null if create a new one.</param>
    /// <returns>The chat message.</returns>
    public static ExtendedChatMessage<AttachmentLinkItem> Create(string id, UserItemInfo sender, AttachmentLinkItem data, string message, ExtendedChatMessageFormats format = ExtendedChatMessageFormats.Text, DateTime? creation = null, JsonObjectNode info = null)
        => new(AttachmentLinkItemKey, id, sender, data, message, format, creation, info);

    /// <summary>
    /// Creates a chat message record.
    /// </summary>
    /// <param name="json">The JSON object to parse.</param>
    /// <returns>The chat message.</returns>
    public static ExtendedChatMessage<AttachmentLinkItem> CreateAttachmentLinkItem(JsonObjectNode json)
        => new(json, data => new AttachmentLinkItem(data), AttachmentLinkItemKey);

    /// <summary>
    /// Creates a chat message record.
    /// </summary>
    /// <param name="sender">The nickname of the sender.</param>
    /// <param name="data">The message data.</param>
    /// <param name="message">The message text.</param>
    /// <param name="format">The message format.</param>
    /// <param name="creation">The creation date time; or null if use now.</param>
    /// <param name="info">The additional information; or null if create a new one.</param>
    /// <returns>The chat message.</returns>
    public static ExtendedChatMessage<AttachmentLinkSet> Create(UserItemInfo sender, AttachmentLinkSet data, string message, ExtendedChatMessageFormats format = ExtendedChatMessageFormats.Text, DateTime? creation = null, JsonObjectNode info = null)
        => Create(Guid.NewGuid(), sender, data ?? new(), message, format, creation, info);

    /// <summary>
    /// Creates a chat message record.
    /// </summary>
    /// <param name="id">The message identifier.</param>
    /// <param name="sender">The nickname of the sender.</param>
    /// <param name="data">The message data.</param>
    /// <param name="message">The message text.</param>
    /// <param name="format">The message format.</param>
    /// <param name="creation">The creation date time; or null if use now.</param>
    /// <param name="info">The additional information; or null if create a new one.</param>
    /// <returns>The chat message.</returns>
    public static ExtendedChatMessage<AttachmentLinkSet> Create(Guid id, UserItemInfo sender, AttachmentLinkSet data, string message, ExtendedChatMessageFormats format = ExtendedChatMessageFormats.Text, DateTime? creation = null, JsonObjectNode info = null)
        => Create(ToIdString(id), sender, data ?? new(), message, format, creation, info);

    /// <summary>
    /// Creates a chat message record.
    /// </summary>
    /// <param name="id">The message identifier.</param>
    /// <param name="sender">The nickname of the sender.</param>
    /// <param name="data">The message data.</param>
    /// <param name="message">The message text.</param>
    /// <param name="format">The message format.</param>
    /// <param name="creation">The creation date time; or null if use now.</param>
    /// <param name="info">The additional information; or null if create a new one.</param>
    /// <returns>The chat message.</returns>
    public static ExtendedChatMessage<AttachmentLinkSet> Create(string id, UserItemInfo sender, AttachmentLinkSet data, string message, ExtendedChatMessageFormats format = ExtendedChatMessageFormats.Text, DateTime? creation = null, JsonObjectNode info = null)
        => new(AttachmentLinkSetKey, id, sender, data ?? new(), message, format, creation, info);

    /// <summary>
    /// Creates a chat message record.
    /// </summary>
    /// <param name="json">The JSON object to parse.</param>
    /// <returns>The chat message.</returns>
    public static ExtendedChatMessage<AttachmentLinkSet> CreateAttachmentLinkSet(JsonObjectNode json)
        => new(json, data => new AttachmentLinkSet(data), AttachmentLinkSetKey);

    /// <summary>
    /// Creates a chat message record.
    /// </summary>
    /// <param name="factory">The data factory.</param>
    /// <param name="sender">The nickname of the sender.</param>
    /// <param name="data">The message data.</param>
    /// <param name="message">The message text.</param>
    /// <param name="format">The message format.</param>
    /// <param name="creation">The creation date time; or null if use now.</param>
    /// <param name="info">The additional information; or null if create a new one.</param>
    /// <returns>The chat message.</returns>
    public static ExtendedChatMessage<T> Create<T>(BaseExtendedChatMessageDataFactory<T> factory, UserItemInfo sender, T data, string message, ExtendedChatMessageFormats format = ExtendedChatMessageFormats.Text, DateTime? creation = null, JsonObjectNode info = null) where T : class
        => factory?.CreateMessage(Guid.NewGuid(), sender, data, message, format, creation, info);

    /// <summary>
    /// Creates a chat message record.
    /// </summary>
    /// <param name="factory">The data factory.</param>
    /// <param name="id">The message identifier.</param>
    /// <param name="sender">The nickname of the sender.</param>
    /// <param name="data">The message data.</param>
    /// <param name="message">The message text.</param>
    /// <param name="format">The message format.</param>
    /// <param name="creation">The creation date time; or null if use now.</param>
    /// <param name="info">The additional information; or null if create a new one.</param>
    /// <returns>The chat message.</returns>
    public static ExtendedChatMessage<T> Create<T>(BaseExtendedChatMessageDataFactory<T> factory, Guid id, UserItemInfo sender, T data, string message, ExtendedChatMessageFormats format = ExtendedChatMessageFormats.Text, DateTime? creation = null, JsonObjectNode info = null) where T : class
        => factory?.CreateMessage(ToIdString(id), sender, data, message, format, creation, info);

    /// <summary>
    /// Creates a chat message record.
    /// </summary>
    /// <param name="factory">The data factory.</param>
    /// <param name="id">The message identifier.</param>
    /// <param name="sender">The nickname of the sender.</param>
    /// <param name="data">The message data.</param>
    /// <param name="message">The message text.</param>
    /// <param name="format">The message format.</param>
    /// <param name="creation">The creation date time; or null if use now.</param>
    /// <param name="info">The additional information; or null if create a new one.</param>
    /// <returns>The chat message.</returns>
    public static ExtendedChatMessage<T> Create<T>(BaseExtendedChatMessageDataFactory<T> factory, string id, UserItemInfo sender, T data, string message, ExtendedChatMessageFormats format = ExtendedChatMessageFormats.Text, DateTime? creation = null, JsonObjectNode info = null) where T : class
        => factory?.CreateMessage(id, sender, data, message, format, creation, info);

    /// <summary>
    /// Creates a chat message record.
    /// </summary>
    /// <param name="factory">The data factory.</param>
    /// <param name="json">The JSON object to parse.</param>
    /// <returns>The chat message.</returns>
    public static ExtendedChatMessage<T> Create<T>(BaseExtendedChatMessageDataFactory<T> factory, JsonObjectNode json) where T : class
        => factory?.CreateMessage(json);

    /// <summary>
    /// Creates a chat message record.
    /// </summary>
    /// <param name="languageName">The lowercase name of the programming language without whitespace.</param>
    /// <param name="sender">The nickname of the sender.</param>
    /// <param name="codeSnippet">The code snippet.</param>
    /// <param name="creation">The creation date time; or null if use now.</param>
    /// <param name="info">The additional information; or null if create a new one.</param>
    /// <returns>The chat message.</returns>
    public static ExtendedChatMessage CreateCodeSnippet(string languageName, UserItemInfo sender, string codeSnippet, DateTime? creation = null, JsonObjectNode info = null)
        => new(Guid.NewGuid(), sender, codeSnippet, ExtendedChatMessageFormats.Code, creation, info, string.Concat("code\\", languageName));

    /// <summary>
    /// Creates a chat message record.
    /// </summary>
    /// <param name="languageName">The lowercase name of the programming language without whitespace.</param>
    /// <param name="id">The message identifier.</param>
    /// <param name="sender">The nickname of the sender.</param>
    /// <param name="codeSnippet">The code snippet.</param>
    /// <param name="creation">The creation date time; or null if use now.</param>
    /// <param name="info">The additional information; or null if create a new one.</param>
    /// <returns>The chat message.</returns>
    public static ExtendedChatMessage CreateCodeSnippet(string languageName, Guid id, UserItemInfo sender, string codeSnippet, DateTime? creation = null, JsonObjectNode info = null)
        => new(id, sender, codeSnippet, ExtendedChatMessageFormats.Code, creation, info, string.Concat("code\\", languageName));

    /// <summary>
    /// Creates a chat message record.
    /// </summary>
    /// <param name="languageName">The lowercase name of the programming language without whitespace.</param>
    /// <param name="id">The message identifier.</param>
    /// <param name="sender">The nickname of the sender.</param>
    /// <param name="codeSnippet">The code snippet.</param>
    /// <param name="creation">The creation date time; or null if use now.</param>
    /// <param name="info">The additional information; or null if create a new one.</param>
    /// <returns>The chat message.</returns>
    public static ExtendedChatMessage CreateCodeSnippet(string languageName, string id, UserItemInfo sender, string codeSnippet, DateTime? creation = null, JsonObjectNode info = null)
        => new(id, sender, codeSnippet, ExtendedChatMessageFormats.Code, creation, info, string.Concat("code\\", languageName));

    /// <summary>
    /// Creates a chat message record.
    /// </summary>
    /// <param name="sender">The nickname of the sender.</param>
    /// <param name="markdown">The markdown text.</param>
    /// <param name="creation">The creation date time; or null if use now.</param>
    /// <param name="info">The additional information; or null if create a new one.</param>
    /// <returns>The chat message.</returns>
    public static ExtendedChatMessage CreateMarkdown(UserItemInfo sender, string markdown, DateTime? creation = null, JsonObjectNode info = null)
        => CreateMarkdown(Guid.NewGuid(), sender, markdown, creation, info);

    /// <summary>
    /// Creates a chat message record.
    /// </summary>
    /// <param name="id">The message identifier.</param>
    /// <param name="sender">The nickname of the sender.</param>
    /// <param name="markdown">The markdown text.</param>
    /// <param name="creation">The creation date time; or null if use now.</param>
    /// <param name="info">The additional information; or null if create a new one.</param>
    /// <returns>The chat message.</returns>
    public static ExtendedChatMessage CreateMarkdown(Guid id, UserItemInfo sender, string markdown, DateTime? creation = null, JsonObjectNode info = null)
        => new(id, sender, markdown, ExtendedChatMessageFormats.Markdown, creation, info);

    /// <summary>
    /// Creates a chat message record.
    /// </summary>
    /// <param name="id">The message identifier.</param>
    /// <param name="sender">The nickname of the sender.</param>
    /// <param name="markdown">The markdown text.</param>
    /// <param name="creation">The creation date time; or null if use now.</param>
    /// <param name="info">The additional information; or null if create a new one.</param>
    /// <returns>The chat message.</returns>
    public static ExtendedChatMessage CreateMarkdown(string id, UserItemInfo sender, string markdown, DateTime? creation = null, JsonObjectNode info = null)
        => new(id, sender, markdown, ExtendedChatMessageFormats.Markdown, creation, info);

    /// <summary>
    /// Tests if the message contains an attachment item.
    /// </summary>
    /// <param name="message">The chat message.</param>
    /// <returns>true if contains; otherwise, false.</returns>
    public static bool HasAttachment(this ExtendedChatMessage<AttachmentLinkItem> message)
        => message?.Data?.Link != null;

    /// <summary>
    /// Tests if contains the specific item.
    /// </summary>
    /// <param name="message">The chat message.</param>
    /// <param name="item">The attachment item.</param>
    /// <returns>true if contains; otherwise, false.</returns>
    public static bool ContainsItem(this ExtendedChatMessage<AttachmentLinkSet> message, AttachmentLinkItem item)
        => message != null && message.Data.Contains(item);

    /// <summary>
    /// Tests if contains the specific item.
    /// </summary>
    /// <param name="message">The chat message.</param>
    /// <param name="link">The attachment link.</param>
    /// <returns>true if contains; otherwise, false.</returns>
    public static bool ContainsItem(this ExtendedChatMessage<AttachmentLinkSet> message, Uri link)
        => message != null && message.Data.Contains(link);

    /// <summary>
    /// Tries to get the specific item.
    /// </summary>
    /// <param name="message">The chat message.</param>
    /// <param name="index">The zero-base index.</param>
    /// <returns>The attachment item; or null, if the index is not valid.</returns>
    public static AttachmentLinkItem TryGetItem(this ExtendedChatMessage<AttachmentLinkSet> message, int index)
        => message?.Data.TryGet(index);

    /// <summary>
    /// Adds an attachment.
    /// </summary>
    /// <param name="message">The chat message.</param>
    /// <param name="item">The attachment to add.</param>
    public static void AddItem(this ExtendedChatMessage<AttachmentLinkSet> message, AttachmentLinkItem item)
        => message?.Data.Add(item);

    /// <summary>
    /// Adds an attachment.
    /// </summary>
    /// <param name="message">The chat message.</param>
    /// <param name="link">The URI of the attachment.</param>
    /// <param name="mime">The MIME value of the attachment.</param>
    public static AttachmentLinkItem AddItem(this ExtendedChatMessage<AttachmentLinkSet> message, Uri link, string mime)
        => message?.Data.Add(link, mime);

    /// <summary>
    /// Adds an attachment.
    /// </summary>
    /// <param name="message">The chat message.</param>
    /// <param name="link">The URI of the attachment.</param>
    /// <param name="mime">The MIME value of the attachment.</param>
    /// <param name="name">The name of the attachment.</param>
    /// <param name="thumbnail">The thumbnail URI of the attachment.</param>
    public static AttachmentLinkItem AddItem(this ExtendedChatMessage<AttachmentLinkSet> message, Uri link, string mime, string name, Uri thumbnail)
        => message?.Data.Add(link, mime, name, thumbnail);

    /// <summary>
    /// Adds an attachment.
    /// </summary>
    /// <param name="message">The chat message.</param>
    /// <param name="items">The attachment items to add.</param>
    /// <returns>The count of the item added.</returns>
    public static int AddItems(this ExtendedChatMessage<AttachmentLinkSet> message, IEnumerable<AttachmentLinkItem> items)
        => message?.Data.AddRange(items) ?? 0;

    /// <summary>
    /// Adds an attachment.
    /// </summary>
    /// <param name="message">The chat message.</param>
    /// <param name="index">The zero-based index to insert the specific item.</param>
    /// <param name="item">The attachment to add.</param>
    /// <exception cref="ArgumentOutOfRangeException">The index is not valid.</exception>
    public static void InsertItem(this ExtendedChatMessage<AttachmentLinkSet> message, int index, AttachmentLinkItem item)
        => message?.Data.Insert(index, item);

    /// <summary>
    /// Adds an attachment.
    /// </summary>
    /// <param name="message">The chat message.</param>
    /// <param name="index">The zero-based index to insert the specific item.</param>
    /// <param name="link">The URI of the attachment.</param>
    /// <param name="mime">The MIME value of the attachment.</param>
    /// <exception cref="ArgumentOutOfRangeException">The index is not valid.</exception>
    public static AttachmentLinkItem InsertItem(this ExtendedChatMessage<AttachmentLinkSet> message, int index, Uri link, string mime)
        => message?.Data.Insert(index, link, mime);

    /// <summary>
    /// Adds an attachment.
    /// </summary>
    /// <param name="message">The chat message.</param>
    /// <param name="index">The zero-based index to insert the specific item.</param>
    /// <param name="link">The URI of the attachment.</param>
    /// <param name="mime">The MIME value of the attachment.</param>
    /// <param name="name">The name of the attachment.</param>
    /// <param name="thumbnail">The thumbnail URI of the attachment.</param>
    /// <exception cref="ArgumentOutOfRangeException">The index is not valid.</exception>
    public static AttachmentLinkItem InsertItem(this ExtendedChatMessage<AttachmentLinkSet> message, int index, Uri link, string mime, string name, Uri thumbnail)
        => message?.Data.Insert(index, link, mime, name, thumbnail);

    /// <summary>
    /// Tests if the message is in plain text.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <returns>Try if the message body is plain text; otherwise, false.</returns>
    public static bool IsPlainTextMessage(ExtendedChatMessage message)
    {
        if (message == null) return false;
        var type = message.MessageType?.ToLowerInvariant();
        if (string.IsNullOrEmpty(type)) return true;
        return type == "text" || type == "txt" || type == "text\\plain" || type == "text/plain";
    }

    /// <summary>
    /// Removes a specific attachment.
    /// </summary>
    /// <param name="message">The chat message.</param>
    /// <param name="item">The attachment to remove.</param>
    /// <returns>true if item is found and successfully removed; otherwise, false.</returns>
    public static bool RemoveItem(this ExtendedChatMessage<AttachmentLinkSet> message, AttachmentLinkItem item)
        => message != null && message.Data.Remove(item);

    /// <summary>
    /// Gets the sender from the message.
    /// </summary>
    /// <param name="message">The message in JSON object.</param>
    /// <param name="profiles">The profile collection.</param>
    /// <returns>The profile info.</returns>
    public static BaseAccountEntityInfo GetSender(JsonObjectNode message, IEnumerable<BaseAccountEntityInfo> profiles)
        => GetSender(message, profiles, out _);

    /// <summary>
    /// Gets the sender from the message.
    /// </summary>
    /// <param name="message">The message in JSON object.</param>
    /// <param name="profiles">The profile collection.</param>
    /// <param name="found">true if found in the profiles; otherwise, false.</param>
    /// <returns>The profile info.</returns>
    public static BaseAccountEntityInfo GetSender(JsonObjectNode message, IEnumerable<BaseAccountEntityInfo> profiles, out bool found)
    {
        if (message == null)
        {
            found = false;
            return null;
        }

        var kind = message.GetValueKind("sender");
        if (kind == JsonValueKind.Null || kind == JsonValueKind.Undefined)
        {
            found = false;
            return null;
        }

        if (kind == JsonValueKind.Array)
        {
            var first = message.TryGetObjectValue("sender", 0);
            if (first == null || first == message)
            {
                found = false;
                return null;
            }

            return GetSender(first, profiles, out found);
        }

        var info = kind == JsonValueKind.Object ? message.TryGetObjectValue("sender") : null;
        var id = info?.Id ?? message.TryGetStringValue("sender");
        if (string.IsNullOrWhiteSpace(id))
        {
            found = false;
            return null;
        }

        var user = profiles.FirstOrDefault(ele => ele.Id == id);
        if (user != null)
        {
            found = true;
            return user;
        }

        found = false;
        return info == null ? null : AccountEntityInfoConverter.Convert(info);
    }

    /// <summary>
    /// Converts the messages to a JSON object.
    /// </summary>
    /// <param name="to">The JSON object to set properties.</param>
    /// <param name="messages">The messages.</param>
    /// <param name="includeSaveInfo">true if include information about this saving action; otherwise, false.</param>
    public static void ToJson(JsonObjectNode to, IEnumerable<ExtendedChatMessage> messages, bool includeSaveInfo = false)
    {
        if (to == null) return;
        if (includeSaveInfo) to.SetValue("info", new JsonObjectNode
        {
            { "save", DateTime.Now },
            { "r", Guid.NewGuid() }
        });
        var arr = new JsonArrayNode();
        var profiles = new List<BaseAccountEntityInfo>();
        foreach (var message in messages)
        {
            var senderId = message?.Sender?.Id;
            if (string.IsNullOrWhiteSpace(senderId)) continue;
            if (!profiles.Any(p => p.Id == senderId))
                profiles.Add(message.Sender);
            var obj = message.ToJson();
            obj.SetValue("sender", senderId);
            arr.Add(obj);
        }

        var profileArr = new JsonArrayNode();
        foreach (var profile in profiles)
        {
            profileArr.Add(profile.ToJson());
        }

        to.SetValue("profiles", profileArr);
        to.SetValue("messages", arr);
    }

    /// <summary>
    /// Converts the messages to a JSON object.
    /// </summary>
    /// <param name="messages">The messages.</param>
    /// <returns>A JSON with all message history.</returns>
    public static JsonObjectNode ToJson(this IEnumerable<ExtendedChatMessage> messages)
    {
        var json = new JsonObjectNode();
        ToJson(json, messages);
        return json;
    }

    internal static string ToIdString(Guid id)
        => id.ToString("N");
}
