﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trivial.Collection;
using Trivial.Data;
using Trivial.Users;
using Trivial.Text;

namespace Trivial.Tasks;

/// <summary>
/// The context of the command guidance for the chat bot.
/// </summary>
public class ChatCommandGuidanceContext
{
    private readonly JsonObjectNode client;
    private readonly JsonObjectNode infos;
    private readonly Dictionary<string, JsonObjectNode> nextData = new();
    private readonly Dictionary<string, JsonObjectNode> nextInfo = new();

    /// <summary>
    /// Initializes a new instance of the ChatCommandGuidanceContext class.
    /// </summary>
    /// <param name="request">The request.</param>
    public ChatCommandGuidanceContext(ChatCommandGuidanceRequest request)
    {
        NextInfo = new();
        nextInfo["_"] = NextInfo;
        if (request == null)
        {
            TrackingId = Guid.NewGuid();
            History = new();
            Info = new();
            infos = new();
            client = new();
            return;
        }

        UserMessage = request.Message;
        UserMessageData = request.Data;
        TrackingId = request.TrackingId;
        var history = request.History;
        History = history == null ? new() : new(history);
        UserNickname = request.UserNickname;
        UserId = request.UserId;
        Gender = request.Gender;
        Info = request.Info?.TryGetObjectValue("_");
        infos = request.Info;
        client = request.ClientContextData;
    }

    /// <summary>
    /// Gets the instance identifier.
    /// </summary>
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>
    /// Gets the tracking identifier.
    /// </summary>
    public Guid TrackingId { get; }

    /// <summary>
    /// Gets the creation date time.
    /// </summary>
    public DateTime CreationTime { get; } = DateTime.Now;

    /// <summary>
    /// Gets the replied date time.
    /// </summary>
    public DateTime? RepliedTime { get; private set; }

    /// <summary>
    /// Gets the chat message from sender.
    /// </summary>
    public string UserMessage { get; }

    /// <summary>
    /// Gets the chat message data from sender.
    /// </summary>
    public JsonObjectNode UserMessageData { get; }

    /// <summary>
    /// Gets the original chat message result.
    /// </summary>
    public string OriginalAnswerMessage { get; private set; }

    /// <summary>
    /// Gets the chat message result.
    /// </summary>
    public string AnswerMessage { get; private set; }

    /// <summary>
    /// Gets the user identifier.
    /// </summary>
    public string UserId { get; }

    /// <summary>
    /// Gets the user nickname.
    /// </summary>
    public string UserNickname { get; }

    /// <summary>
    /// Gets the user gender.
    /// </summary>
    public Genders Gender { get; }

    /// <summary>
    /// Gets the history.
    /// </summary>
    public List<SimpleChatMessage> History { get; }

    /// <summary>
    /// Gets the context information.
    /// The command guidance can access this to store useful data during this round.
    /// </summary>
    public JsonObjectNode Info { get; }

    /// <summary>
    /// Gets next context information for context.
    /// </summary>
    public JsonObjectNode NextInfo { get; }

    /// <summary>
    /// Gets the data of the business rich output.
    /// </summary>
    public JsonObjectNode AnswerData { get; } = new();

    /// <summary>
    /// Gets the prompt collection generated by each command guidance.
    /// </summary>
    internal SynchronizedList<string> PromptCollection { get; } = new();

    /// <summary>
    /// Rewrites the answer message.
    /// </summary>
    /// <param name="value">The message.</param>
    public void RewriteAnswerMessage(string value)
        => AnswerMessage = value;

    /// <summary>
    /// Rewrites the answer message.
    /// </summary>
    /// <param name="value">The message.</param>
    /// <param name="additionalNewLine">true if append an empty line before original message.</param>
    public void AppendAnswerMessage(string value, bool additionalNewLine = false)
        => AnswerMessage = string.IsNullOrWhiteSpace(OriginalAnswerMessage) ? value : string.Concat(AnswerMessage, Environment.NewLine, additionalNewLine ? Environment.NewLine : string.Empty, value);

    /// <summary>
    /// Gets the JSON object of response.
    /// </summary>
    /// <returns>The response JSON object.</returns>
    public ChatCommandGuidanceResponse GetResponse()
    {
        var resp = new ChatCommandGuidanceResponse()
        {
            Id = Id,
            TrackingId = TrackingId,
            Message = AnswerMessage,
            Data = AnswerData,
            Info = Info,
            ClientContextData = client
        };
        foreach (var item in nextData)
        {
            resp.Details[item.Key] = item.Value;
        }

        return resp;
    }

    /// <summary>
    /// Sets the answer message.
    /// </summary>
    /// <param name="value">The message.</param>
    internal void SetAnswerMessage(string value)
    {
        RepliedTime = DateTime.Now;
        AnswerMessage = value;
        OriginalAnswerMessage = value;
    }

    /// <summary>
    /// Gets the information data.
    /// </summary>
    /// <param name="key">The command key.</param>
    /// <returns>The information data.</returns>
    internal JsonObjectNode GetInfo(string key)
        => key == null ? Info : infos?.TryGetObjectValue(key);

    /// <summary>
    /// Gets the next information data of a specific command.
    /// </summary>
    /// <param name="command">The command key.</param>
    /// <param name="init">true if ensures; otherwise, false.</param>
    /// <returns>The next information data for context.</returns>
    internal JsonObjectNode GetNextInfo(string command, bool init = false)
    {
        if (command == null) return NextInfo;
        command = command.Trim().ToLowerInvariant();
        if (nextInfo.TryGetValue(command, out var result)) return result;
        if (!init) return null;
        result = new();
        nextInfo[command] = result;
        return result;
    }

    /// <summary>
    /// Gets the answer data of a specific command.
    /// </summary>
    /// <param name="command">The command key.</param>
    /// <param name="init">true if ensures; otherwise, false.</param>
    /// <returns>The data for rich output.</returns>
    internal JsonObjectNode GetAnswerData(string command, bool init = false)
    {
        if (command == null) return AnswerData;
        command = command.Trim().ToLowerInvariant();
        if (nextData.TryGetValue(command, out var result)) return result;
        if (!init) return null;
        result = new();
        nextData[command] = result;
        return result;
    }
}

/// <summary>
/// The request of chat command guidance.
/// </summary>
public class ChatCommandGuidanceRequest
{
    /// <summary>
    /// Gets or sets the nickname of the sender.
    /// </summary>
    public string UserNickname { get; set; }

    /// <summary>
    /// Gets or sets the sender identifier.
    /// </summary>
    public string UserId { get; set; }

    /// <summary>
    /// Gets or sets the gender of the sender.
    /// </summary>
    public Genders Gender { get; set; }

    /// <summary>
    /// Gets or sets the tracking identifier.
    /// </summary>
    public Guid TrackingId { get; set; }

    /// <summary>
    /// Gets or sets the message.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Gets or sets the data.
    /// </summary>
    public JsonObjectNode Data { get; set; }

    /// <summary>
    /// Gets or sets the information.
    /// </summary>
    public JsonObjectNode Info { get; set; }

    /// <summary>
    /// Gets or sets the chat history.
    /// </summary>
    public IEnumerable<SimpleChatMessage> History { get; set; }

    /// <summary>
    /// Gets or sets the context data from client.
    /// </summary>
    public JsonObjectNode ClientContextData { get; set; }

    /// <summary>
    /// Converts the JSON raw back.
    /// </summary>
    /// <param name="value">The source value.</param>
    /// <returns>The request instance.</returns>
    public static implicit operator ChatCommandGuidanceRequest(JsonObjectNode value)
    {
        if (value is null) return null;
        var sender = value.TryGetObjectValue("sender");
        return new()
        {
            UserId = sender?.TryGetStringTrimmedValue("id", true) ?? value.TryGetStringTrimmedValue("senderId", true),
            UserNickname = sender?.TryGetStringTrimmedValue("nickname", true) ?? value.TryGetStringTrimmedValue("senderName", true),
            Gender = sender?.TryGetEnumValue<Genders>("gender") ?? value.TryGetEnumValue<Genders>("senderGender") ?? Genders.Unknown,
            TrackingId = value.TryGetGuidValue("tracking") ?? Guid.NewGuid(),
            Message = value.TryGetStringValue("text") ?? value.TryGetStringValue("message"),
            Data = value.TryGetObjectValue("data"),
            Info = value.TryGetObjectValue("info"),
            History = ChatCommandGuidanceHelper.DeserializeChatMessages(value.TryGetArrayValue("history")),
            ClientContextData = value.TryGetObjectValue("ref"),
        };
    }

    /// <summary>
    /// Converts to JSON object.
    /// </summary>
    /// <param name="value">The JSON value.</param>
    /// <returns>A JSON object.</returns>
    public static explicit operator JsonObjectNode(ChatCommandGuidanceRequest value)
    {
        if (value is null) return null;
        return new()
        {
            { "sender", new JsonObjectNode
            {
                { "id", value.UserId },
                { "nickname", value.UserNickname },
                { "gender", value.Gender.ToString() },
            } },
            { "trackig", value.TrackingId },
            { "text", value.Message },
            { "data", value.Data },
            { "info", value.Info},
            { "history", ChatCommandGuidanceHelper.Serizalize(value.History) },
            { "ref", value.ClientContextData }
        };
    }
}

/// <summary>
/// The request of chat command guidance.
/// </summary>
public class ChatCommandGuidanceResponse
{
    /// <summary>
    /// Gets or sets the message identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the tracking identifier.
    /// </summary>
    public Guid TrackingId { get; set; }

    /// <summary>
    /// Gets or sets the message.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Gets or sets the response data.
    /// </summary>
    public JsonObjectNode Data { get; set; }

    /// <summary>
    /// Gets or sets the details.
    /// </summary>
    public Dictionary<string, JsonObjectNode> Details { get; } = new();

    /// <summary>
    /// Gets or sets the information.
    /// </summary>
    public JsonObjectNode Info { get; set; }

    /// <summary>
    /// Gets or sets the context data from client.
    /// </summary>
    public JsonObjectNode ClientContextData { get; set; }

    /// <summary>
    /// Converts the JSON raw back.
    /// </summary>
    /// <param name="value">The source value.</param>
    /// <returns>The response instance.</returns>
    public static implicit operator ChatCommandGuidanceResponse(JsonObjectNode value)
    {
        if (value is null) return null;
        var result = new ChatCommandGuidanceResponse()
        {
            Id = value.TryGetGuidValue("id") ?? Guid.NewGuid(),
            TrackingId = value.TryGetGuidValue("tracking") ?? Guid.NewGuid(),
            Message = value.TryGetStringValue("text") ?? value.TryGetStringValue("message"),
            Data = value.TryGetObjectValue("data"),
            Info = value.TryGetObjectValue("info"),
            ClientContextData = value.TryGetObjectValue("ref"),
        };
        var detailsArr = value.TryGetObjectValue("details") ?? new();
        foreach (var item in detailsArr)
        {
            if (item.Value is not JsonObjectNode json) continue;
            result.Details[item.Key] = json;
        }

        return result;
    }

    /// <summary>
    /// Converts to JSON object.
    /// </summary>
    /// <param name="value">The JSON value.</param>
    /// <returns>A JSON object.</returns>
    public static explicit operator JsonObjectNode(ChatCommandGuidanceResponse value)
    {
        if (value is null) return null;
        return new()
        {
            { "id", value.Id },
            { "trackig", value.TrackingId },
            { "text", value.Message },
            { "data", value.Data },
            { "details", ChatCommandGuidanceHelper.ToJson(value.Details) },
            { "info", value.Info},
            { "ref", value.ClientContextData }
        };
    }
}
