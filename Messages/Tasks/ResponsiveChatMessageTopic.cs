using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Trivial.Collection;
using Trivial.Data;
using Trivial.Reflection;
using Trivial.Text;

namespace Trivial.Tasks;

/// <summary>
/// The chat topic.
/// </summary>
[Guid("654A8667-7559-4247-AB98-28F4C4DEBAF6")]
public class ResponsiveChatMessageTopic
{
    /// <summary>
    /// The history record of message.
    /// </summary>
    private readonly List<ResponsiveChatMessageModel> history;

    /// <summary>
    /// The name of this chat message topic.
    /// </summary>
    private string name;

    /// <summary>
    /// A flag indicating whether the topic is enabled.
    /// </summary>
    private bool isEnabled;

    /// <summary>
    /// Initializes a new instance of the ResponsiveChatMessageTopic class.
    /// </summary>
    /// <param name="id">The chat message topic identifier.</param>
    /// <param name="history">The history record of message.</param>
    /// <param name="info">The additional information.</param>
    public ResponsiveChatMessageTopic(string id, IEnumerable<ResponsiveChatMessageModel> history = null, JsonObjectNode info = null)
    {
        Id = id;
        AdditionalInfo = info ?? new();
        if (history == null)
        {
            this.history = new();
        }
        else if (history is List<ResponsiveChatMessageModel> list)
        {
            this.history = list;
        }
        else
        {
            this.history = history.ToList();
        }
    }

    /// <summary>
    /// Adds or removes a hanlder occurs on the new turn-based chat message model is added.
    /// </summary>
    public event DataEventHandler<ResponsiveChatMessageModel> Added;

    /// <summary>
    /// Adds or removes a hanlder occurs on the topic name is changed.
    /// </summary>
    public event DataEventHandler<string> NameChanged;

    /// <summary>
    /// Adds or removes a hanlder occurs on the topic is enabled.
    /// </summary>
    public event EventHandler Enabled;

    /// <summary>
    /// Adds or removes a hanlder occurs on the topic is disabled.
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
    /// Gets the chat topic identifier.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the chat message topic is enabled.
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
    /// Gets the additional information of this chat message topic.
    /// </summary>
    public JsonObjectNode AdditionalInfo { get; }

    /// <summary>
    /// Converts history to the chat message JSON array.
    /// </summary>
    /// <param name="currentQuestion">The current user request string.</param>
    /// <param name="systemPrompt">The optional system prompt.</param>
    /// <param name="options">The options to generate history data.</param>
    /// <returns>The JSON array about the history.</returns>
    public JsonArrayNode ToHistoryPromptJsonArray(string currentQuestion, string systemPrompt, ResponsiveChatMessageHistoryOptions options)
    {
        var arr = new JsonArrayNode();
        options ??= ResponsiveChatMessageHistoryOptions.Default;
        var roleKey = TextHelper.GetIfNotEmpty(options.RoleKey, "role");
        var messageKey = TextHelper.GetIfNotEmpty(options.MessageKey, "content");
        var instructionRole = TextHelper.GetIfNotEmpty(options.InstructionRole, "system");
        var userRole = TextHelper.GetIfNotEmpty(options.UserRole, "user");
        var assistantRole = TextHelper.GetIfNotEmpty(options.AssistantRole, "assistant");
        if (!string.IsNullOrWhiteSpace(systemPrompt))
            arr.Add(new JsonObjectNode
            {
                { roleKey, instructionRole },
                { messageKey, systemPrompt }
            });
        IEnumerable<ResponsiveChatMessageModel> col = history;
        if (options.MaxRecord.HasValue)
        {
            var skip = history.Count - options.MaxRecord.Value;
            if (skip > 0) col = col.Skip(skip);
        }

        foreach (var item in col)
        {
            if (item == null) continue;
            arr.Add(new JsonObjectNode
            {
                { roleKey, userRole },
                { messageKey, item.Question?.Message }
            });
            arr.Add(new JsonObjectNode
            {
                { roleKey, assistantRole },
                { messageKey, item.Answer?.Message }
            });
        }

        if (!string.IsNullOrWhiteSpace(currentQuestion))
            arr.Add((options ?? ResponsiveChatMessageHistoryOptions.Default).CreateUserMessage(currentQuestion));
        return arr;
    }

    /// <summary>
    /// Gets the chat message history.
    /// </summary>
    public IEnumerable<ResponsiveChatMessageModel> GetHistory()
    {
        var history = this.history.ToList();
        foreach (var model in history)
        {
            if (model == null) continue;
            yield return model;
        }
    }

    /// <summary>
    /// Adds a message model at the end of this topic.
    /// </summary>
    /// <param name="item">The new message model to append to this topic.</param>
    internal void Add(ResponsiveChatMessageModel item)
    {
        if (history.Contains(item)) return;
        history.Add(item);
        Added?.Invoke(this, new(item));
    }
}

/// <summary>
/// The options used to generate message history of round chat.
/// </summary>
public class ResponsiveChatMessageHistoryOptions
{
    /// <summary>
    /// Gets the default instance.
    /// </summary>
    internal static ResponsiveChatMessageHistoryOptions Default { get; } = new();

    /// <summary>
    /// Gets or sets the optional maximum count of the message record.
    /// </summary>
    public int? MaxRecord { get; set; }

    /// <summary>
    /// Gets or sets the role of instruction (e.g. system, developer) used to introduce the context.
    /// </summary>
    public string InstructionRole { get; set; }

    /// <summary>
    /// Gets or sets the role of assistant used to answer the query by service.
    /// </summary>
    public string AssistantRole { get; set; }

    /// <summary>
    /// Gets or sets the role of user used to send question by user.
    /// </summary>
    public string UserRole { get; set; }

    /// <summary>
    /// Gets or sets the property key of role.
    /// </summary>
    public string RoleKey { get; set; }

    /// <summary>
    /// Gets or sets the property key of message content.
    /// </summary>
    public string MessageKey { get; set; }

    /// <summary>
    /// Creates a user message.
    /// </summary>
    /// <param name="message">The message content.</param>
    /// <returns>A JSON object about the message.</returns>
    public JsonObjectNode CreateUserMessage(string message)
        => new()
        {
            { TextHelper.GetIfNotEmpty(RoleKey, "role"), TextHelper.GetIfNotEmpty(UserRole, "user") },
            { TextHelper.GetIfNotEmpty(MessageKey, "content"), message }
        };

    /// <summary>
    /// Creates an assistant message.
    /// </summary>
    /// <param name="message">The message content.</param>
    /// <returns>A JSON object about the message.</returns>
    public JsonObjectNode CreateAssistantMessage(string message)
        => new()
        {
            { TextHelper.GetIfNotEmpty(RoleKey, "role"), TextHelper.GetIfNotEmpty(AssistantRole, "assistant") },
            { TextHelper.GetIfNotEmpty(MessageKey, "content"), message }
        };
}
