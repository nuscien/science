using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Trivial.Collection;
using Trivial.Data;
using Trivial.Reflection;
using Trivial.Tasks;
using Trivial.Users;
using Trivial.Web;

namespace Trivial.Text;

/// <summary>
/// The chat message record.
/// </summary>
[JsonConverter(typeof(ExtendedChatMessageConverter.InternalConverter))]
[Guid("CD8B6B09-1423-4A83-A707-9FD0A9830C70")]
public class ExtendedChatMessage : RelatedResourceEntityInfo
{
    /// <summary>
    /// Initializes a new instance of the ExtendedChatMessage class.
    /// </summary>
    /// <param name="sender">The nickname of the sender.</param>
    /// <param name="message">The message text.</param>
    /// <param name="creation">The creation date time; or null if use now.</param>
    /// <param name="info">The additional information; or null if create a new one.</param>
    public ExtendedChatMessage(BaseUserItemInfo sender, string message, DateTime? creation = null, JsonObjectNode info = null)
        : this(Guid.NewGuid(), null as BaseResourceEntityInfo, sender, new(message, ExtendedChatMessageFormats.Text, info), creation)
    {
    }

    /// <summary>
    /// Initializes a new instance of the ExtendedChatMessage class.
    /// </summary>
    /// <param name="conversation">The conversation instance as owner of this chat message.</param>
    /// <param name="sender">The nickname of the sender.</param>
    /// <param name="message">The message text.</param>
    /// <param name="format">The message format.</param>
    /// <param name="creation">The creation date time; or null if use now.</param>
    /// <param name="info">The additional information; or null if create a new one.</param>
    public ExtendedChatMessage(ExtendedChatConversation conversation, BaseUserItemInfo sender, string message, ExtendedChatMessageFormats format = ExtendedChatMessageFormats.Text, DateTime? creation = null, JsonObjectNode info = null)
        : this(Guid.NewGuid(), conversation?.Source, sender, new(message, format, info), creation)
    {
    }

    /// <summary>
    /// Initializes a new instance of the ExtendedChatMessage class.
    /// </summary>
    /// <param name="conversation">The conversation instance as owner of this chat message.</param>
    /// <param name="sender">The nickname of the sender.</param>
    /// <param name="message">The message text.</param>
    /// <param name="format">The message format.</param>
    /// <param name="creation">The creation date time; or null if use now.</param>
    /// <param name="info">The additional information; or null if create a new one.</param>
    public ExtendedChatMessage(BaseResourceEntityInfo conversation, BaseUserItemInfo sender, string message, ExtendedChatMessageFormats format = ExtendedChatMessageFormats.Text, DateTime? creation = null, JsonObjectNode info = null)
        : this(Guid.NewGuid(), conversation, sender, new(message, format, info), creation)
    {
    }

    /// <summary>
    /// Initializes a new instance of the ExtendedChatMessage class.
    /// </summary>
    /// <param name="conversation">The conversation instance as owner of this chat message.</param>
    /// <param name="sender">The nickname of the sender.</param>
    /// <param name="content">The message content.</param>
    /// <param name="creation">The creation date time; or null if use now.</param>
    public ExtendedChatMessage(ExtendedChatConversation conversation, BaseUserItemInfo sender, ExtendedChatMessageContent content, DateTime? creation = null)
        : this(Guid.NewGuid(), conversation?.Id, sender, content, creation)
    {
    }

    /// <summary>
    /// Initializes a new instance of the ExtendedChatMessage class.
    /// </summary>
    /// <param name="conversation">The conversation instance as owner of this chat message.</param>
    /// <param name="sender">The nickname of the sender.</param>
    /// <param name="content">The message content.</param>
    /// <param name="creation">The creation date time; or null if use now.</param>
    public ExtendedChatMessage(BaseResourceEntityInfo conversation, BaseUserItemInfo sender, ExtendedChatMessageContent content, DateTime? creation = null)
        : this(Guid.NewGuid(), conversation?.Id, sender, content, creation)
    {
    }

    /// <summary>
    /// Initializes a new instance of the ExtendedChatMessage class.
    /// </summary>
    /// <param name="id">The message identifier.</param>
    /// <param name="conversation">The conversation instance as owner of this chat message.</param>
    /// <param name="sender">The nickname of the sender.</param>
    /// <param name="content">The message content.</param>
    /// <param name="creation">The creation date time; or null if use now.</param>
    public ExtendedChatMessage(Guid id, ExtendedChatConversation conversation, BaseUserItemInfo sender, ExtendedChatMessageContent content, DateTime? creation = null)
        : this(ExtendedChatMessages.ToIdString(id), conversation?.Id, sender, content, creation)
    {
    }

    /// <summary>
    /// Initializes a new instance of the ExtendedChatMessage class.
    /// </summary>
    /// <param name="id">The message identifier.</param>
    /// <param name="conversation">The conversation instance as owner of this chat message.</param>
    /// <param name="sender">The nickname of the sender.</param>
    /// <param name="content">The message content.</param>
    /// <param name="creation">The creation date time; or null if use now.</param>
    public ExtendedChatMessage(Guid id, BaseResourceEntityInfo conversation, BaseUserItemInfo sender, ExtendedChatMessageContent content, DateTime? creation = null)
        : this(ExtendedChatMessages.ToIdString(id), conversation?.Id, sender, content, creation)
    {
    }

    /// <summary>
    /// Initializes a new instance of the ExtendedChatMessage class.
    /// </summary>
    /// <param name="id">The message identifier.</param>
    /// <param name="conversation">The conversation instance as owner of this chat message.</param>
    /// <param name="sender">The nickname of the sender.</param>
    /// <param name="content">The message content.</param>
    /// <param name="creation">The creation date time; or null if use now.</param>
    public ExtendedChatMessage(Guid id, string conversation, BaseUserItemInfo sender, ExtendedChatMessageContent content, DateTime? creation = null)
        : this(ExtendedChatMessages.ToIdString(id), conversation, sender, content, creation)
    {
    }

    /// <summary>
    /// Initializes a new instance of the ExtendedChatMessage class.
    /// </summary>
    /// <param name="id">The message identifier.</param>
    /// <param name="conversation">The conversation instance as owner of this chat message.</param>
    /// <param name="sender">The nickname of the sender.</param>
    /// <param name="content">The message content.</param>
    /// <param name="creation">The creation date time; or null if use now.</param>
    public ExtendedChatMessage(string id, ExtendedChatConversation conversation, BaseUserItemInfo sender, ExtendedChatMessageContent content, DateTime? creation = null)
        : this(id, conversation?.Id, sender, content, creation)
    {
    }

    /// <summary>
    /// Initializes a new instance of the ExtendedChatMessage class.
    /// </summary>
    /// <param name="id">The message identifier.</param>
    /// <param name="conversation">The conversation instance as owner of this chat message.</param>
    /// <param name="sender">The nickname of the sender.</param>
    /// <param name="content">The message content.</param>
    /// <param name="creation">The creation date time; or null if use now.</param>
    public ExtendedChatMessage(string id, BaseResourceEntityInfo conversation, BaseUserItemInfo sender, ExtendedChatMessageContent content, DateTime? creation = null)
        : this(id, conversation?.Id, sender, content, creation)
    {
    }

    /// <summary>
    /// Initializes a new instance of the ExtendedChatMessage class.
    /// </summary>
    /// <param name="id">The message identifier.</param>
    /// <param name="conversation">The conversation instance as owner of this chat message.</param>
    /// <param name="sender">The nickname of the sender.</param>
    /// <param name="content">The message content.</param>
    /// <param name="creation">The creation date time; or null if use now.</param>
    public ExtendedChatMessage(string id, string conversation, BaseUserItemInfo sender, ExtendedChatMessageContent content, DateTime? creation = null)
        : base(id, conversation, creation)
    {
        SetProperty(nameof(Sender), sender);
        if (content == null)
        {
            Info = new();
            return;
        }

        SetProperty(nameof(Message), content.Message);
        SetProperty(nameof(MessageFormat), content.MessageFormat);
        SetProperty(nameof(Category), content.Category);
        SetProperty(nameof(Priority), content.Priority);
        if (!string.IsNullOrEmpty(content.MessageType)) SetProperty(nameof(MessageType), content.MessageType);
        Info = content.Info ?? new();
    }

    /// <summary>
    /// Initializes a new instance of the ExtendedChatMessage class.
    /// </summary>
    /// <param name="json">The JSON object to parse.</param>
    public ExtendedChatMessage(JsonObjectNode json)
        : base(json)
    {
    }

    /// <summary>
    /// Initializes a new instance of the ExtendedChatMessage class.
    /// </summary>
    /// <param name="json">The JSON object to parse.</param>
    /// <param name="profiles">All profiles to find the sender.</param>
    public ExtendedChatMessage(JsonObjectNode json, IEnumerable<BaseAccountEntityInfo> profiles)
        : base(json)
    {
        FillSender(json, profiles);
    }

    /// <summary>
    /// Gets the sender.
    /// </summary>
    public BaseUserItemInfo Sender => GetCurrentProperty<BaseUserItemInfo>();

    /// <summary>
    /// Gets the markdown text of the message.
    /// </summary>
    public string Message
    {
        get
        {
            return GetCurrentProperty<string>();
        }

        set
        {
            var s = GetCurrentProperty<string>();
            if (!SetCurrentProperty(value) || s == null) return;
            if (GetProperty<ChatMessageModificationKinds>(nameof(ModificationKind)) != ChatMessageModificationKinds.Original)
                SetProperty(nameof(ModificationKind), ChatMessageModificationKinds.Modified);
            SetProperty(nameof(LastModificationTime), DateTime.Now);
        }
    }

    /// <summary>
    /// Gets or sets the modification kind.
    /// </summary>
    public ChatMessageModificationKinds ModificationKind
    {
        get => GetCurrentProperty<ChatMessageModificationKinds>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets the message type.
    /// </summary>
    public string MessageType => GetCurrentProperty<string>();

    /// <summary>
    /// Gets or sets the message format.
    /// </summary>
    public ExtendedChatMessageFormats MessageFormat
    {
        get => GetCurrentProperty<ExtendedChatMessageFormats>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets the category.
    /// </summary>
    public string Category
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets the priority.
    /// </summary>
    public BasicPriorities Priority => GetCurrentProperty<BasicPriorities>();

    /// <summary>
    /// Gets the additional data.
    /// </summary>
    public JsonObjectNode Info { get; private set; }

    /// <inheritdoc />
    [JsonIgnore]
#if NETCOREAPP
    [NotMapped]
#endif
    protected override string Supertype => "message";

    /// <inheritdoc />
    [JsonIgnore]
#if NETCOREAPP
    [NotMapped]
#endif
    protected override string ResourceType => MessageType;

    /// <inheritdoc />
    protected override void Fill(JsonObjectNode json)
    {
        base.Fill(json);
        SetProperty(nameof(Sender), AccountEntityInfoConverter.Convert(json.TryGetObjectValue("sender")));
        SetProperty(nameof(Message), json.TryGetStringValue("text") ?? json.TryGetStringValue("message"));
        var messageType = json.TryGetStringTrimmedValue("type", true);
        SetProperty(nameof(MessageType), messageType);
        SetProperty(nameof(MessageFormat), json.TryGetEnumValue<ExtendedChatMessageFormats>("format") ?? ExtendedChatMessageFormats.Text);
        SetProperty(nameof(Priority), json.TryGetEnumValue<BasicPriorities>("priority") ?? BasicPriorities.Normal);
        Info = json.TryGetObjectValue("info") ?? new();
        Category = json.TryGetStringTrimmedValue("category", true);
        var data = json.TryGetObjectValue("data");
        if (data == null) return;
        SetProperty("Data", data);
        if (messageType != null) return;
        messageType = data.Schema;
        if (!string.IsNullOrWhiteSpace(messageType)) SetProperty(nameof(MessageType), messageType);
    }

    /// <summary>
    /// Fills sender from the profile collection.
    /// </summary>
    /// <param name="json">The message in JSON.</param>
    /// <param name="profiles">All profiles to find the sender.</param>
    /// <returns>true if found; otherwise, false.</returns>
    protected bool FillSender(JsonObjectNode json, IEnumerable<BaseAccountEntityInfo> profiles)
    {
        if (profiles == null) return false;
        var senderId = ExtendedChatMessages.GetSender(json, profiles)?.Id
            ?? json.TryGetStringTrimmedValue("sender", true);
        if (string.IsNullOrEmpty(senderId)) return false;
        var user = profiles.FirstOrDefault(ele => ele.Id == senderId);
        if (user == null) return false;
        SetProperty(nameof(Sender), user);
        return true;
    }

    /// <summary>
    /// Converts to JSON object.
    /// </summary>
    /// <returns>A JSON object.</returns>
    public override JsonObjectNode ToJson()
    {
        var json = base.ToJson();
        json.SetValue("sender", Sender);
        json.SetValue("text", Message);
        json.SetValue("format", (int)MessageFormat);
        if (Info.Count > 0) json.SetValue("info", Info);
        if (!string.IsNullOrWhiteSpace(Category)) json.SetValue("category", Category);
        var data = GetProperty<object>("Data");
        if (data == null) return json;
        if (data is JsonObjectNode j)
        {
            json.SetValue("data", j);
            return json;
        }

        if (data is IJsonObjectHost joh)
        {
            json.SetValue("data", joh);
            return json;
        }

        try
        {
            var d = JsonSerializer.Serialize(data);
            if (string.IsNullOrWhiteSpace(d)) return json;
            json.SetValue("data", JsonObjectNode.ConvertFrom(d));
            return json;
        }
        catch (ArgumentException)
        {
        }
        catch (JsonException)
        {
        }
        catch (NotSupportedException)
        {
        }
        catch (InvalidOperationException)
        {
        }

        json.SetValueIfNotNull("priority", (int)Priority);
        return json;
    }

    /// <summary>
    /// Returns a string which represents the object instance.
    /// </summary>
    /// <returns>A string which represents the object instance.</returns>
    public override string ToString()
    {
        var nickname = Sender?.Nickname?.Trim();
        if (string.IsNullOrEmpty(nickname)) nickname = "?";
        return $"{nickname} ({LastModificationTime}){Environment.NewLine}{Message}";
    }

    /// <summary>
    /// Updates the saving status of this entity.
    /// </summary>
    /// <param name="state">The new state.</param>
    /// <param name="message">The additional message about this saving status.</param>
    public void UpdateSavingStatus(ExtendedChatMessageSendResultStates state, string message = null)
        => GetSendResult().Update(state, message);

    /// <summary>
    /// Updates the saving status of this entity.
    /// </summary>
    /// <param name="ex">The exception thrown during saving.</param>
    public new void UpdateSavingStatus(Exception ex)
        => GetSendResult().Update(ex);

    /// <summary>
    /// Updates the saving status of this entity.
    /// </summary>
    /// <param name="ex">The exception thrown during saving.</param>
    /// <param name="result">The message send result.</param>
    internal void UpdateSavingStatus(Exception ex, out ExtendedChatMessageSendResult result)
    {
        result = GetSendResult();
        result.Update(ex);
    }

    /// <summary>
    /// Updates the saving status of this entity.
    /// </summary>
    /// <param name="resp">The action result.</param>
    internal void UpdateSavingStatus(ExtendedChatMessageSendResult resp)
    {
        if (resp == null || resp.State == ResourceEntitySavingStates.Ready || resp.SendStatus == ExtendedChatMessageSendResultStates.Success)
        {
            UpdateSavingStatus(ResourceEntitySavingStates.Ready);
            return;
        }

        var status = GetSendResult();
        status.Update(resp.SendStatus, resp.Message);
        status.Info.SetRange(resp.Info);
    }

    private ExtendedChatMessageSendResult GetSendResult()
    {
        if (LastSavingStatus is ExtendedChatMessageSendResult status) return status;
        status = new ExtendedChatMessageSendResult();
        var current = LastSavingStatus;
        if (current is not null) status.Update(current.State, current.Message);
        SetProperty(nameof(LastSavingStatus), status);
        return status;
    }

    /// <summary>
    /// Converts the JSON raw back.
    /// </summary>
    /// <param name="value">The source value.</param>
    /// <returns>A model of the message.</returns>
    public static implicit operator ExtendedChatMessage(JsonObjectNode value)
    {
        if (value is null) return null;
        var type = value.TryGetStringTrimmedValue("type", true)?.ToLowerInvariant();
        if (type == null) return new(value);
        if (type.StartsWith("text\\") || type.StartsWith("text/") || (!type.Contains("\\") && !type.Contains("/")))
        {
            var program = type switch
            {
                JsonValues.JsonMIME or "text\\json" or "json" or ".json" => "json",
                "text/x-csharp" or "text\\csharp" or "c#" or "csharp" or "c#" or ".cs" => "csharp",
                WebFormat.JavaScriptMIME or "text\\javascript" or "text\\ecmascript" or "javascript" or "ecmascript" or "js" or ".js" or ".esm" => "javascript",
                WebFormat.YamlMIME or "text\\yaml" or "yaml" => "yaml",
                WebFormat.MarkdownMIME or "text\\md" or "text\\markdown" or "markdown" or ".md" => "markdown",
                WebFormat.XmlMIME or "text\\xml" or "xml" or ".xml" or ".settings" => "xml",
                WebFormat.SvgMIME or "text\\svg" or "svg" or ".svg" => "svg",
                "text/x-python" or "text\\python" or "python" or ".py" or ".py2" or ".py3" or ".pyw" => "python",
                "text/x-chdr" or "text\\c" or "text/x-c" or ".c" or ".cc" or ".cxx" or ".dic" or ".h" => "c",
                "text\\cpp" or "c++" or "hh" or "hpp" or ".cpp" => "cpp",
                "text/x-golang" or "text\\golang" or "text\\go" or "golang" or ".go" => "go",
                "text/x-java-source" or "text\\java" or ".java" => "java",
                "text/x-qsharp" or "text\\qsharp" or "qsharp" or "q#" or ".qs" => "qsharp",
                JsonValues.JsonlMIME or "text\\jsonl" or "jsonl" => "jsonl",
                _ => null
            };
            if (program != null)
            {
                var msg = new ExtendedChatMessage<ProgrammingCodeSnippetInfo>(value, null, ExtendedChatMessages.ProgrammingCodeKey);
                if (msg.Data == null) return new(value);
                msg.Data.Language = program;
                return msg;
            }
        }
        else if (type.StartsWith("code\\") && type.Length > 5)
        {
            var program = type.Substring(5);
            var msg = new ExtendedChatMessage<ProgrammingCodeSnippetInfo>(value, null, ExtendedChatMessages.ProgrammingCodeKey);
            if (msg.Data == null) return new(value);
            msg.Data.Language = program;
            return msg;
        }

        if (type.StartsWith("text\\") || type.StartsWith("text/"))
        {
            if (type == ExtendedChatMessages.ProgrammingCodeKey) return new ExtendedChatMessage<AttachmentLinkSet>(value, null);
            var data = value.TryGetObjectValue("data");
            if (data is null) return new(value);
            var language = data.TryGetStringTrimmedValue("language", true);
            if (language != null) return new ExtendedChatMessage<ProgrammingCodeSnippetInfo>(value, null);
            var msg = new ExtendedChatMessage<ProgrammingCodeSnippetInfo>(value, null, ExtendedChatMessages.ProgrammingCodeKey);
            msg.Data.Language = language;
        }

        return type switch
        {
            ExtendedChatMessages.AttachmentLinkItemKey => new ExtendedChatMessage<AttachmentLinkItem>(value, json => new AttachmentLinkItem(json), type),
            ExtendedChatMessages.AttachmentLinkSetKey => new ExtendedChatMessage<AttachmentLinkSet>(value, json => new AttachmentLinkSet(json), type),
            ExtendedChatMessages.ProgrammingCodeKey => new ExtendedChatMessage<ProgrammingCodeSnippetInfo>(value, null),
            ExtendedChatMessages.MarkdownKey or "text\\md" or "text/markdown" or "markdown" or ".md" => new ExtendedChatMessage<ProgrammingCodeSnippetInfo>(value, null, ExtendedChatMessages.MarkdownKey),
            _ => new(value)
        };
    }

    /// <summary>
    /// Converts to JSON object.
    /// </summary>
    /// <param name="value">The JSON value.</param>
    /// <returns>A JSON object.</returns>
    public static explicit operator JsonObjectNode(ExtendedChatMessage value)
        => value?.ToJson();
}

/// <summary>
/// The chat message record.
/// </summary>
/// <typeparam name="T">The type of data.</typeparam>
[JsonConverter(typeof(ExtendedChatMessageConverter))]
public class ExtendedChatMessage<T> : ExtendedChatMessage where T : class
{
    /// <summary>
    /// Initializes a new instance of the ExtendedChatMessage class.
    /// </summary>
    /// <param name="type">The message type.</param>
    /// <param name="conversation">The conversation instance as owner of this chat message.</param>
    /// <param name="sender">The nickname of the sender.</param>
    /// <param name="data">The message data.</param>
    /// <param name="message">The message text.</param>
    /// <param name="format">The message format.</param>
    /// <param name="creation">The creation date time; or null if use now.</param>
    /// <param name="info">The additional information; or null if create a new one.</param>
    protected ExtendedChatMessage(string type, BaseResourceEntityInfo conversation, UserItemInfo sender, T data, string message, ExtendedChatMessageFormats format = ExtendedChatMessageFormats.Text, DateTime? creation = null, JsonObjectNode info = null)
        : this(Guid.NewGuid(), conversation, sender, new(type, data, message, format, info), creation)
    {
    }

    /// <summary>
    /// Initializes a new instance of the ExtendedChatMessage class.
    /// </summary>
    /// <param name="type">The message type.</param>
    /// <param name="conversation">The conversation instance as owner of this chat message.</param>
    /// <param name="sender">The nickname of the sender.</param>
    /// <param name="data">The message data.</param>
    /// <param name="message">The message text.</param>
    /// <param name="format">The message format.</param>
    /// <param name="creation">The creation date time; or null if use now.</param>
    /// <param name="info">The additional information; or null if create a new one.</param>
    protected ExtendedChatMessage(string type, ExtendedChatConversation conversation, UserItemInfo sender, T data, string message, ExtendedChatMessageFormats format = ExtendedChatMessageFormats.Text, DateTime? creation = null, JsonObjectNode info = null)
        : this(Guid.NewGuid(), conversation, sender, new(type, data, message, format, info), creation)
    {
    }

    /// <summary>
    /// Initializes a new instance of the ExtendedChatMessage class.
    /// </summary>
    /// <param name="id">The message identifier.</param>
    /// <param name="conversation">The conversation instance as owner of this chat message.</param>
    /// <param name="sender">The nickname of the sender.</param>
    /// <param name="content">The message content.</param>
    /// <param name="creation">The creation date time; or null if use now.</param>
    protected ExtendedChatMessage(Guid id, BaseResourceEntityInfo conversation, UserItemInfo sender, ExtendedChatMessageContent<T> content, DateTime? creation = null)
        : this(ExtendedChatMessages.ToIdString(id), conversation?.Id, sender, content, creation)
    {
    }

    /// <summary>
    /// Initializes a new instance of the ExtendedChatMessage class.
    /// </summary>
    /// <param name="id">The message identifier.</param>
    /// <param name="conversation">The conversation instance as owner of this chat message.</param>
    /// <param name="sender">The nickname of the sender.</param>
    /// <param name="content">The message content.</param>
    /// <param name="creation">The creation date time; or null if use now.</param>
    protected ExtendedChatMessage(Guid id, ExtendedChatConversation conversation, UserItemInfo sender, ExtendedChatMessageContent<T> content, DateTime? creation = null)
        : this(ExtendedChatMessages.ToIdString(id), conversation?.Id, sender, content, creation)
    {
    }

    /// <summary>
    /// Initializes a new instance of the ExtendedChatMessage class.
    /// </summary>
    /// <param name="id">The message identifier.</param>
    /// <param name="conversation">The conversation instance as owner of this chat message.</param>
    /// <param name="sender">The nickname of the sender.</param>
    /// <param name="content">The message content.</param>
    /// <param name="creation">The creation date time; or null if use now.</param>
    protected ExtendedChatMessage(Guid id, string conversation, UserItemInfo sender, ExtendedChatMessageContent<T> content, DateTime? creation = null)
        : this(ExtendedChatMessages.ToIdString(id), conversation, sender, content, creation)
    {
    }

    /// <summary>
    /// Initializes a new instance of the ExtendedChatMessage class.
    /// </summary>
    /// <param name="id">The message identifier.</param>
    /// <param name="conversation">The conversation instance as owner of this chat message.</param>
    /// <param name="sender">The nickname of the sender.</param>
    /// <param name="content">The message content.</param>
    /// <param name="creation">The creation date time; or null if use now.</param>
    protected internal ExtendedChatMessage(string id, BaseResourceEntityInfo conversation, UserItemInfo sender, ExtendedChatMessageContent<T> content, DateTime? creation = null)
        : this(id, conversation?.Id, sender, content, creation)
    {
    }

    /// <summary>
    /// Initializes a new instance of the ExtendedChatMessage class.
    /// </summary>
    /// <param name="id">The message identifier.</param>
    /// <param name="conversation">The conversation instance as owner of this chat message.</param>
    /// <param name="sender">The nickname of the sender.</param>
    /// <param name="content">The message content.</param>
    /// <param name="creation">The creation date time; or null if use now.</param>
    protected internal ExtendedChatMessage(string id, ExtendedChatConversation conversation, UserItemInfo sender, ExtendedChatMessageContent<T> content, DateTime? creation = null)
        : this(id, conversation?.Id, sender, content, creation)
    {
    }

    /// <summary>
    /// Initializes a new instance of the ExtendedChatMessage class.
    /// </summary>
    /// <param name="id">The message identifier.</param>
    /// <param name="conversation">The conversation instance as owner of this chat message.</param>
    /// <param name="sender">The nickname of the sender.</param>
    /// <param name="content">The message content.</param>
    /// <param name="creation">The creation date time; or null if use now.</param>
    protected internal ExtendedChatMessage(string id, string conversation, UserItemInfo sender, ExtendedChatMessageContent<T> content, DateTime? creation = null)
        : base(id, conversation, sender, content, creation)
    {
        if (content != null) Data = content.Data;
    }

    /// <summary>
    /// Initializes a new instance of the ExtendedChatMessage class.
    /// </summary>
    /// <param name="json">The JSON object to parse.</param>
    /// <param name="ignoreDataDeserialize">true if skip to deserialize the data; otherwise, false.</param>
    /// <param name="type">The message type to override; or null, if use the one in JSON input.</param>
    protected ExtendedChatMessage(JsonObjectNode json, bool ignoreDataDeserialize = false, string type = null)
        : base(json)
    {
        if (!string.IsNullOrWhiteSpace(type)) SetProperty(nameof(MessageType), type);
        if (ignoreDataDeserialize || json == null) return;
        var data = json.TryGetObjectValue("data");
        if (data == null) return;
        Data = data.Deserialize<T>();
    }

    /// <summary>
    /// Initializes a new instance of the ExtendedChatMessage class.
    /// </summary>
    /// <param name="json">The JSON object to parse.</param>
    /// <param name="dataConverter">The data converter.</param>
    /// <param name="type">The message type to override; or null, if use the one in JSON input.</param>
    protected internal ExtendedChatMessage(JsonObjectNode json, Func<JsonObjectNode, T> dataConverter, string type = null)
        : base(json)
    {
        if (!string.IsNullOrWhiteSpace(type)) SetProperty(nameof(MessageType), type);
        var data = json?.TryGetObjectValue("data");
        if (data == null)
        {
            var s = json.TryGetStringTrimmedValue("data", true);
            if (s == null) return;
            if (s.StartsWith("{") && s.StartsWith("}")) data = JsonObjectNode.TryParse(s);
            else if (s.StartsWith("[") || s.StartsWith("<")) return;
            else if (dataConverter != null) data = new()
            {
                { "value", s }
            };
        }

        if (data == null) return;
        Data = dataConverter == null ? data.Deserialize<T>() : dataConverter(data);
    }

    /// <summary>
    /// Initializes a new instance of the ExtendedChatMessage class.
    /// </summary>
    /// <param name="json">The JSON object to parse.</param>
    /// <param name="profiles">All profiles to find the sender.</param>
    /// <param name="ignoreDataDeserialize">true if skip to deserialize the data; otherwise, false.</param>
    /// <param name="type">The message type to override; or null, if use the one in JSON input.</param>
    protected ExtendedChatMessage(JsonObjectNode json, IEnumerable<BaseAccountEntityInfo> profiles, bool ignoreDataDeserialize = false, string type = null)
        : this(json, ignoreDataDeserialize, type)
    {
        FillSender(json, profiles);
    }

    /// <summary>
    /// Initializes a new instance of the ExtendedChatMessage class.
    /// </summary>
    /// <param name="json">The JSON object to parse.</param>
    /// <param name="profiles">All profiles to find the sender.</param>
    /// <param name="dataConverter">The data converter.</param>
    /// <param name="type">The message type to override; or null, if use the one in JSON input.</param>
    protected internal ExtendedChatMessage(JsonObjectNode json, IEnumerable<BaseAccountEntityInfo> profiles, Func<JsonObjectNode, T> dataConverter, string type = null)
        : this(json, dataConverter, type)
    {
        FillSender(json, profiles);
    }

    /// <summary>
    /// Gets the data of customized message details.
    /// </summary>
    public T Data
    {
        get => GetCurrentProperty<T>();
        protected set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Converts to JSON object.
    /// </summary>
    /// <param name="value">The JSON value.</param>
    /// <returns>A JSON object.</returns>
    public static explicit operator JsonObjectNode(ExtendedChatMessage<T> value)
        => value?.ToJson();
}

/// <summary>
/// JSON value node converter.
/// </summary>
public sealed class ExtendedChatMessageConverter : JsonConverterFactory
{
    /// <summary>
    /// JSON value node converter.
    /// </summary>
    internal sealed class InternalConverter : JsonObjectHostConverter<ExtendedChatMessage>
    {
        /// <inheritdoc />
        protected override ExtendedChatMessage Create(JsonObjectNode json)
            => (ExtendedChatMessage)json;
    }

    /// <summary>
    /// JSON value node converter.
    /// </summary>
    internal sealed class GenericConverter<T> : JsonObjectHostConverter<ExtendedChatMessage<T>> where T : class
    {
        /// <inheritdoc />
        protected override ExtendedChatMessage<T> Create(JsonObjectNode json)
            => new(json, null);
    }

    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert)
        => typeToConvert == typeof(ExtendedChatMessage) || typeof(ExtendedChatMessage).IsAssignableFrom(typeToConvert);

    /// <inheritdoc />
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        if (typeToConvert.IsGenericType)
        {
            var generic = typeToConvert.GetGenericTypeDefinition();
            if (generic == typeof(ExtendedChatMessage<>))
            {
                var genericType = typeToConvert.GetGenericArguments().FirstOrDefault();
                if (genericType != null)
                {
                    var type = typeof(GenericConverter<>).MakeGenericType(new[] { genericType });
                    return (JsonConverter)Activator.CreateInstance(type);
                }
            }
        }
        else
        {
            return new InternalConverter();
        }

        var converter = new JsonValueNodeConverter();
        return converter.CreateConverter(typeToConvert, options);
    }
}
