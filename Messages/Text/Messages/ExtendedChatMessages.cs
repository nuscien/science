using System;
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
using Trivial.Web;

namespace Trivial.Text;

/// <summary>
/// The helper of extended chat message.
/// </summary>
public static class ExtendedChatMessages
{
    internal const string AttachmentLinkSetKey = "attachment\\list";
    internal const string ProgrammingCodeKey = "text\\code";
    internal const string MarkdownKey = "text\\markdown";

    /// <summary>
    /// Converts to saving state.
    /// </summary>
    /// <param name="sendState">The status of sending.</param>
    /// <returns>The saving state.</returns>
    public static ResourceEntitySavingStates ToSavingState(ExtendedChatMessageSendResultStates sendState)
        => sendState switch
        {
            ExtendedChatMessageSendResultStates.NotSend => ResourceEntitySavingStates.Local,
            ExtendedChatMessageSendResultStates.Success => ResourceEntitySavingStates.Ready,
            _ => ResourceEntitySavingStates.Failure
        };

    /// <summary>
    /// Creates a chat message content to send.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <param name="message">The message content.</param>
    /// <param name="format">The message format.</param>
    /// <param name="info">The additional info.</param>
    /// <returns>The chat message content.</returns>
    public static ExtendedChatMessageContent<T> CreateMessageContent<T>(T data, string message, ExtendedChatMessageFormats format = ExtendedChatMessageFormats.Text, JsonObjectNode info = null)
        => new(data, message, format, info);

    /// <summary>
    /// Creates a chat message content to send.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <param name="message">The message content.</param>
    /// <param name="format">The message format.</param>
    /// <param name="category">The optional category.</param>
    /// <param name="info">The additional info.</param>
    /// <returns>The chat message content.</returns>
    public static ExtendedChatMessageContent<T> CreateMessageContent<T>(T data, string message, ExtendedChatMessageFormats format, string category, JsonObjectNode info = null)
        => new(data, message, format, category, info);

    /// <summary>
    /// Creates a chat message content to send.
    /// </summary>
    /// <param name="message">The message content.</param>
    /// <param name="info">The additional info.</param>
    /// <returns>The chat message content.</returns>
    public static ExtendedChatMessageContent CreateMarkdownMessageContent(string message, JsonObjectNode info = null)
        => new(message, ExtendedChatMessageFormats.Markdown, info);

    /// <summary>
    /// Creates a chat message content to send.
    /// </summary>
    /// <param name="message">The message content.</param>
    /// <param name="category">The optional category.</param>
    /// <param name="info">The additional info.</param>
    /// <returns>The chat message content.</returns>
    public static ExtendedChatMessageContent CreateMarkdownMessageContent(string message, string category, JsonObjectNode info = null)
        => new(message, ExtendedChatMessageFormats.Markdown, category, info);

    /// <summary>
    /// Gets the message which the specific one replies.
    /// </summary>
    /// <param name="messages">The message collection </param>
    /// <param name="message">The reply.</param>
    /// <returns>The message replied by the specific one.</returns>
    public static ExtendedChatMessage GetReplied(this IEnumerable<ExtendedChatMessage> messages, ExtendedChatMessage message)
        => ResourceEntityUtils.Get(messages, message?.ReplyId);

    /// <summary>
    /// Gets the specific message replies.
    /// </summary>
    /// <param name="messages">The message collection </param>
    /// <param name="source">The identifier of the source message to find its replies.</param>
    /// <param name="replyExactly">true if reply the given message exactly; otherwise, false, also including the replies to its replies.</param>
    /// <returns>The collection of the messages which reply the given one.</returns>
    public static IEnumerable<ExtendedChatMessage> GetReplies(this IEnumerable<ExtendedChatMessage> messages, string source, bool replyExactly = false)
    {
        if (messages == null || string.IsNullOrEmpty(source)) yield break;
        if (replyExactly)
        {
            foreach (var m in messages)
            {
                if (m?.Id == null) continue;
                if (m.ReplyId == source) yield return m;
            }
        }
        else
        {
            foreach (var m in messages)
            {
                if (m?.Id == null) continue;
                if (m.RootReplyId == source || m.ReplyId == source) yield return m;
            }
        }
    }

    /// <summary>
    /// Gets the specific message replies.
    /// </summary>
    /// <param name="messages">The message collection </param>
    /// <param name="source">The source message to find its replies.</param>
    /// <param name="replyExactly">true if reply the given message exactly; otherwise, false, also including the replies to its replies.</param>
    /// <returns>The collection of the messages which reply the given one.</returns>
    public static IEnumerable<ExtendedChatMessage> GetReplies(this IEnumerable<ExtendedChatMessage> messages, ExtendedChatMessage source, bool replyExactly = false)
        => GetReplies(messages, source?.Id, replyExactly);

    /// <summary>
    /// Gets the specific message with same topic.
    /// </summary>
    /// <param name="messages">The message collection </param>
    /// <param name="topic">The topic identifier.</param>
    /// <returns>The collection of the messages which reply the given one.</returns>
    public static IEnumerable<ExtendedChatMessage> GetTopicMessages(this IEnumerable<ExtendedChatMessage> messages, string topic)
    {
        if (messages == null) yield break;
        if (string.IsNullOrEmpty(topic))
        {
            if (topic != null) yield break;
            foreach (var m in messages)
            {
                if (m?.Id == null) continue;
                if (m.TopicId == null) yield return m;
            }
        }
        else
        {
            foreach (var m in messages)
            {
                if (m?.Id == null) continue;
                if (m.TopicId == topic) yield return m;
            }
        }
    }

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

        if (kind != JsonValueKind.Object)
        {
            found = false;
            return null;
        }

        var info = message.TryGetObjectValue("sender");
        if (info is null)
        {
            found = false;
            return null;
        }

        var id = info.Id ?? message.TryGetStringValue("sender");
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
        return AccountEntityFactory.Convert(info);
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
            var obj = JsonObjectNode.ConvertFrom(message);
            obj.SetValue("sender", senderId);
            arr.Add(obj);
        }

        var profileArr = new JsonArrayNode();
        foreach (var profile in profiles)
        {
            profileArr.Add(JsonObjectNode.ConvertFrom(profile));
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
