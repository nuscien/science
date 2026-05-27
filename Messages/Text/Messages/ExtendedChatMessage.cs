using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
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
[Guid("CD8B6B09-1423-4A83-A707-9FD0A9830C70")]
public class ExtendedChatMessage : RelatedResourceEntityInfo
{
    /// <summary>
    /// Initializes a new instance of the ExtendedChatMessage class.
    /// </summary>
    /// <param name="args">The initialization arguments.</param>
    /// <param name="sender">The nickname of the sender.</param>
    /// <param name="content">The message content.</param>
    public ExtendedChatMessage(RelatedResourceEntityArgs args, BaseUserItemInfo sender, ExtendedChatMessageContent content)
        : base(args)
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
        SetProperty(nameof(ReplyId), content.ReplyId);
        SetProperty(nameof(RootReplyId), content.RootReplyId ?? content.ReplyId);
        Info = content.Info ?? new();
    }

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
        SetProperty(nameof(ReplyId), content.ReplyId);
        SetProperty(nameof(RootReplyId), content.RootReplyId ?? content.ReplyId);
        Info = content.Info ?? new();
    }

    /// <summary>
    /// Gets the sender.
    /// </summary>
    public BaseUserItemInfo Sender => GetCurrentProperty<BaseUserItemInfo>();

    /// <summary>
    /// Gets or sets the method of sender.
    /// </summary>
    [JsonPropertyName("method")]
#if NETCOREAPP
    [NotMapped]
#endif
    public MessageSenderMethodInfo SenderMethod
    {
        get => GetCurrentProperty<MessageSenderMethodInfo>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets the markdown text of the message.
    /// </summary>
    [JsonPropertyName("message")]
#if NETCOREAPP
    [Column("message")]
#endif
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
    [JsonPropertyName("msgState")]
#if NETCOREAPP
    [Column("msgstate")]
#endif
    public ChatMessageModificationKinds ModificationKind
    {
        get => GetCurrentProperty<ChatMessageModificationKinds>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets the message type.
    /// </summary>
    [JsonIgnore]
#if NETCOREAPP
    [Column("kind")]
#endif
    public string MessageType
    {
        get
        {
            return GetCurrentProperty<string>();
        }

        set
        {
            SetCurrentProperty(value);
        }
    }

    /// <summary>
    /// Gets or sets the message format.
    /// </summary>
    [JsonPropertyName("format")]
#if NETCOREAPP
    [Column("format")]
#endif
    public ExtendedChatMessageFormats MessageFormat
    {
        get => GetCurrentProperty<ExtendedChatMessageFormats>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets the category.
    /// </summary>
    [JsonPropertyName("category")]
#if NETCOREAPP
    [Column("category")]
#endif
    public string Category
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets the priority.
    /// </summary>
    [JsonPropertyName("priority")]
#if NETCOREAPP
    [Column("priority")]
#endif
    public BasicPriorities Priority => GetCurrentProperty<BasicPriorities>();

    /// <summary>
    /// Gets the additional data.
    /// </summary>
    [JsonPropertyName("info")]
#if NETCOREAPP
    [Column("info")]
#endif
    public JsonObjectNode Info { get; private set; }

    /// <summary>
    /// Gets the message identifier of reply.
    /// </summary>
    [JsonPropertyName("reply")]
#if NETCOREAPP
    [Column("reply")]
#endif
    public string ReplyId => GetCurrentProperty<string>();

    /// <summary>
    /// Gets the message identifier of root reply.
    /// </summary>
    [JsonPropertyName("parent")]
#if NETCOREAPP
    [Column("parent")]
#endif
    public string RootReplyId => GetCurrentProperty<string>() ?? ReplyId;

    /// <summary>
    /// Gets the message identifier of conversation topic.
    /// </summary>
    [JsonPropertyName("topic")]
#if NETCOREAPP
    [Column("topic")]
#endif
    public string TopicId
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// The options value.
    /// </summary>
    [Description("The options value.")]
#if NETCOREAPP
    [Column("config")]
#endif
    public new JsonObjectNode ConfigInfo
    {
        get => base.ConfigInfo;
        set => base.ConfigInfo = value;
    }

    /// <summary>
    /// The message data.
    /// </summary>
    [JsonIgnore]
#if NETCOREAPP
    [NotMapped]
#endif
    public JsonObjectNode Data
    {
        get => GetCurrentProperty<JsonObjectNode>();
        set => SetCurrentProperty(value);
    }

    /// <inheritdoc />
    protected override string Supertype => "message";

    /// <inheritdoc />
    protected override string ResourceType => MessageType;

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
    /// Gets message data.
    /// </summary>
    /// <typeparam name="T">The type of data.</typeparam>
    /// <param name="options">Options to control the reader behavior during parsing.</param>
    /// <returns>The message data.</returns>
    /// <exception cref="JsonException">The message data or its property does not represent a valid single JSON object.</exception>
    /// <exception cref="NotSupportedException">There is no compatible JSON converter for the type of the message data or its serializable members.</exception>
    /// <exception cref="InvalidCastException">Convert failed.</exception>
    public T GetData<T>(JsonSerializerOptions options = default) where T : class
    {
        var data = Data;
        if (data is null) return null;
        var type = typeof(T);
        if (type == typeof(string))
            return (T)(object)data.ToString();
        if (type == typeof(JsonObjectNode))
            return (T)(object)data;
        if (type == typeof(StringBuilder))
            return (T)(object)new StringBuilder(data.ToString());
        if (type == typeof(SecureString))
            return (T)(object)Security.SecureStringExtensions.ToSecure(data.ToString());
        var creator = type.GetMethod("ConvertFrom", new[] { typeof(JsonObjectNode) });
        if (creator is not null && creator.IsStatic && creator.ReturnType == typeof(T))
        {
            var result = creator.Invoke(null, new object[] { data });
            return result is null ? null : (T)result;
        }

        creator = type.GetMethod("ConvertFrom", new[] { typeof(JsonObjectNode), typeof(JsonSerializerOptions) });
        if (creator is not null && creator.IsStatic && creator.ReturnType == typeof(T))
        {
            var result = creator.Invoke(null, new object[] { data, options });
            return result is null ? null : (T)result;
        }

        return Data.Deserialize<T>();
    }

    /// <summary>
    /// Creates a chat message content by replying this message.
    /// </summary>
    /// <param name="message">The message text.</param>
    /// <param name="info">The additional information.</param>
    /// <returns>The chat message content to reply this message.</returns>
    public ExtendedChatMessageContent Reply(string message, JsonObjectNode info = null)
        => new(message, MessageFormat, info)
        {
            ReplyId = Id,
            RootReplyId = RootReplyId ?? ReplyId ?? Id
        };

    /// <summary>
    /// Creates a chat message content by replying this message.
    /// </summary>
    /// <param name="message">The message text.</param>
    /// <param name="category">The optional category.</param>
    /// <param name="info">The additional information.</param>
    /// <returns>The chat message content to reply this message.</returns>
    public ExtendedChatMessageContent Reply(string message, string category, JsonObjectNode info = null)
        => new(message, MessageFormat, category, info)
        {
            ReplyId = Id,
            RootReplyId = RootReplyId ?? ReplyId ?? Id
        };

    /// <summary>
    /// Creates a chat message content by replying this message.
    /// </summary>
    /// <param name="message">The message text.</param>
    /// <param name="format">The message format.</param>
    /// <param name="category">The optional category.</param>
    /// <param name="info">The additional information.</param>
    /// <returns>The chat message content to reply this message.</returns>
    public ExtendedChatMessageContent Reply(string message, ExtendedChatMessageFormats format, string category = null, JsonObjectNode info = null)
        => new(message, format, category, info)
        {
            ReplyId = Id,
            RootReplyId = RootReplyId ?? ReplyId ?? Id
        };

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
}
