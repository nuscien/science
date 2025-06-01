using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Trivial.Data;
using Trivial.Net;
using Trivial.Reflection;
using Trivial.Text;

namespace Trivial.Tasks;

/// <summary>
/// The context of turn-based chat message.
/// </summary>
public class RoundChatMessageContext
{
    /// <summary>
    /// Initializes a new instance of the RoundChatMessageContext class.
    /// </summary>
    /// <param name="thread">The chat message thread.</param>
    /// <param name="message">The request message sent by user.</param>
    internal RoundChatMessageContext(RoundChatMessageThread thread, string message)
    {
        Thread = thread ?? new(null);
        Model = new(message);
        Thread.Add(Model);
    }

    /// <summary>
    /// Gets the chat thread instance.
    /// </summary>
    public RoundChatMessageThread Thread { get; }

    /// <summary>
    /// Gets or sets the additional tag.
    /// </summary>
    public object Tag { get; set; }

    /// <summary>
    /// Gets the message model of this turn.
    /// </summary>
    public RoundChatMessageModel Model { get; }

    /// <summary>
    /// Gets the chat message history.
    /// </summary>
    /// <param name="onlySuccess">true if returns successful ones only; otherwise, false.</param>
    public IEnumerable<RoundChatMessageModel> GetHistory(bool onlySuccess = false)
    {
        var col = Thread.GetHistory(onlySuccess);
        foreach (var item in col)
        {
            if (item != Model) yield return item;
        }
    }

    /// <summary>
    /// Tests if this thread is available to send a new message.
    /// </summary>
    /// <returns>true if there is no working job any more, or it is empty; otherwise, false.</returns>
    public bool CanSend()
        => Thread.CanSend();
}

/// <summary>
/// The turn-based chat message thread.
/// </summary>
[JsonConverter(typeof(JsonValueNodeConverter))]
public class RoundChatMessageThread : IJsonObjectHost
{
    /// <summary>
    /// The history record of message.
    /// </summary>
    private readonly List<RoundChatMessageModel> history;

    /// <summary>
    /// The topic name of this chat message thread.
    /// </summary>
    private string name;

    /// <summary>
    /// A flag indicating whether the thread is enabled.
    /// </summary>
    private bool isEnabled;

    /// <summary>
    /// Initializes a new instance of the RoundChatMessageThread class.
    /// </summary>
    /// <param name="threadId">The chat message thread identifier.</param>
    /// <param name="history">The history record of message.</param>
    /// <param name="info">The additional information.</param>
    public RoundChatMessageThread(string threadId, IEnumerable<RoundChatMessageModel> history = null, JsonObjectNode info = null)
    {
        Id = threadId;
        AdditionalInfo = info ?? new();
        if (history == null)
        {
            this.history = new();
        }
        if (history is List<RoundChatMessageModel> list)
        {
            this.history = list;
        }
        else
        {
            this.history = history.ToList();
        }
    }

    /// <summary>
    /// Initializing an instance of the RoundChatMessageModel class.
    /// </summary>
    /// <param name="json">The JSON object to convert.</param>
    public RoundChatMessageThread(JsonObjectNode json)
    {
        if (json == null) return;
        Id = json.TryGetStringTrimmedValue("id", true);
        Name = json.TryGetStringTrimmedValue("name", true);
        AdditionalInfo = json.TryGetObjectValue("info") ?? new();
        var items = json.TryGetObjectListValue("items", true);
        foreach (var item in items)
        {
            var record = new RoundChatMessageModel(item);
            if (string.IsNullOrWhiteSpace(record.Question)) continue;
            history.Add(record);
        }
    }

    /// <summary>
    /// Adds or removes a hanlder occurs on the new turn-based chat message model is added.
    /// </summary>
    public event DataEventHandler<RoundChatMessageModel> Added;

    /// <summary>
    /// Adds or removes a hanlder occurs on the topic name is changed.
    /// </summary>
    public event DataEventHandler<string> NameChanged;

    /// <summary>
    /// Adds or removes a hanlder occurs on the thread is enabled.
    /// </summary>
    public event EventHandler Enabled;

    /// <summary>
    /// Adds or removes a hanlder occurs on the thread is disabled.
    /// </summary>
    public event EventHandler Disabled;

    /// <summary>
    /// Gets or sets the topic name.
    /// </summary>
    public string Name
    {
        get
        {
            return name;
        }

        set
        {
            if (name == value) return;
            name = value;
            NameChanged?.Invoke(this, value);
        }
    }

    /// <summary>
    /// Gets the chat thread identifier.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the chat message thread is enabled.
    /// </summary>
    public bool IsEnabled
    {
        get
        {
            return isEnabled;
        }

        set
        {
            if (isEnabled == value) return;
            isEnabled = value;
            if (value) Enabled?.Invoke(this, new EventArgs());
            else Disabled?.Invoke(this, new EventArgs());
        }
    }

    /// <summary>
    /// Gets the additional information of this chat message thread.
    /// </summary>
    public JsonObjectNode AdditionalInfo { get; }

    /// <summary>
    /// Converts history to the chat message JSON array.
    /// </summary>
    /// <param name="current">The handler to format the current user request string.</param>
    /// <param name="systemPrompt">The optional system prompt.</param>
    /// <param name="maxHistoryRecord">The maximum record count of history.</param>
    /// <returns>The JSON array about the history.</returns>
    public JsonArrayNode ToHistoryPromptJsonArray(Func<string, string> current, string systemPrompt = null, int? maxHistoryRecord = null)
    {
        var arr = ToHistoryPromptJsonArray(systemPrompt, maxHistoryRecord, out var q);
        if (q == null) return arr;
        if (current != null)
        {
            q = current(q);
            if (string.IsNullOrWhiteSpace(q)) return arr;
        }

        arr.Add(new JsonObjectNode
        {
            { "role", "user" },
            { "message", q }
        });
        return arr;
    }

    /// <summary>
    /// Converts history to the chat message JSON array.
    /// </summary>
    /// <param name="excludeCurrent">true if exclude the new one if available; otherwise, false.</param>
    /// <param name="systemPrompt">The optional system prompt.</param>
    /// <param name="maxHistoryRecord">The maximum record count of history.</param>
    /// <returns>The JSON array about the history.</returns>
    public JsonArrayNode ToHistoryPromptJsonArray(bool excludeCurrent, string systemPrompt = null, int? maxHistoryRecord = null)
    {
        var arr = ToHistoryPromptJsonArray(systemPrompt, maxHistoryRecord, out var q);
        if (!excludeCurrent && q != null)
            arr.Add(new JsonObjectNode
            {
                { "role", "user" },
                { "message", q }
            });
        return arr;
    }

    /// <summary>
    /// Converts history to the chat message JSON array.
    /// </summary>
    /// <param name="currentQuestion">The current user request string.</param>
    /// <param name="systemPrompt">The optional system prompt.</param>
    /// <param name="maxHistoryRecord">The maximum record count of history.</param>
    /// <returns>The JSON array about the history.</returns>
    public JsonArrayNode ToHistoryPromptJsonArray(string currentQuestion, string systemPrompt, int? maxHistoryRecord = null)
    {
        var arr = ToHistoryPromptJsonArray(systemPrompt, maxHistoryRecord, out var q);
        if (!string.IsNullOrWhiteSpace(currentQuestion))
            arr.Add(new JsonObjectNode
            {
                { "role", "user" },
                { "message", currentQuestion }
            });
        return arr;
    }

    /// <summary>
    /// Converts history to the chat message JSON array.
    /// </summary>
    /// <param name="systemPrompt">The optional system prompt.</param>
    /// <param name="maxHistoryRecord">The maximum record count of history.</param>
    /// <returns>The JSON array about the history.</returns>
    public JsonArrayNode ToHistoryPromptJsonArray(string systemPrompt = null, int? maxHistoryRecord = null)
        => ToHistoryPromptJsonArray(systemPrompt, maxHistoryRecord, out _);

    /// <summary>
    /// Tests if this thread is available to send a new message.
    /// </summary>
    /// <returns>true if there is no working job any more, or it is empty; otherwise, false.</returns>
    public bool CanSend()
    {
        if (!IsEnabled) return false;
        var history = this.history.ToList();
        for (var i = history.Count - 1; i >= 0; i--)
        {
            if (history[i] == null || history[i].IsError) continue;
            return !history[i].IsWorking;
        }

        return true;
    }

    /// <summary>
    /// Gets the chat message history.
    /// </summary>
    /// <param name="onlySuccess">true if returns successful ones only; otherwise, false.</param>
    public IEnumerable<RoundChatMessageModel> GetHistory(bool onlySuccess = false)
    {
        var history = this.history.ToList();
        foreach (var model in history)
        {
            if (model == null) continue;
            if (!model.IsSuccessful && onlySuccess) continue;
            yield return model;
        }
    }

    /// <summary>
    /// Converts to JSON object.
    /// </summary>
    /// <returns>The JSON object node.</returns>
    public JsonObjectNode ToJson()
        => new()
        {
            { "id", Id },
            { "items", history.ToJsonObjectNodes() },
            { "name", Name },
            { "info", AdditionalInfo }
        };

    /// <summary>
    /// Adds a message model at the end of this thread.
    /// </summary>
    /// <param name="item">The new message model to append to this thread.</param>
    internal void Add(RoundChatMessageModel item)
    {
        if (history.Contains(item)) return;
        history.Add(item);
    }

    /// <summary>
    /// Converts history to the chat message JSON array.
    /// </summary>
    /// <param name="systemPrompt">The optional system prompt.</param>
    /// <param name="maxHistoryRecord">The maximum record count of history.</param>
    /// <param name="q">The question in current.</param>
    /// <returns>The JSON array about the history.</returns>
    private JsonArrayNode ToHistoryPromptJsonArray(string systemPrompt, int? maxHistoryRecord, out string q)
    {
        var arr = new JsonArrayNode();
        if (!string.IsNullOrWhiteSpace(systemPrompt))
            arr.Add(new JsonObjectNode
            {
                { "role", "system" },
                { "message", systemPrompt }
            });
        var col = history.Where(ele => ele.IsSuccessful);
        if (maxHistoryRecord.HasValue)
        {
            var skip = history.Count - maxHistoryRecord.Value;
            if (skip > 0) col = col.Skip(skip);
        }

        foreach (var item in col)
        {
            if (item == null || !item.IsSuccessful) continue;
            arr.Add(new JsonObjectNode
            {
                { "role", "user" },
                { "message", item.Question }
            });
            arr.Add(new JsonObjectNode
            {
                { "role", "assistant" },
                { "message", item.Answer }
            });
        }

        var last = history.LastOrDefault();
        q = !string.IsNullOrWhiteSpace(last?.Question) && last.IsPreparing ? last.Question : null;
        return arr;
    }
}
