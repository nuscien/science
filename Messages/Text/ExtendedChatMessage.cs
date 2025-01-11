using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Trivial.Collection;
using Trivial.Data;
using Trivial.Reflection;
using Trivial.Tasks;
using Trivial.Users;

namespace Trivial.Text;

/// <summary>
/// The modification kinds of the message.
/// </summary>
public enum ChatMessageModificationKinds : byte
{
    /// <summary>
    /// The message is neve modified.
    /// </summary>
    Original = 0,

    /// <summary>
    /// The streaming message which means the message is transferring by continious updating.
    /// </summary>
    Streaming = 1,

    /// <summary>
    /// The message has been modified by sender.
    /// </summary>
    Modified = 2,

    /// <summary>
    /// The message has been modified and is open to update by others.
    /// </summary>
    Collaborative = 3,

    /// <summary>
    /// The message has been removed by sender.
    /// </summary>
    Removed = 5,

    /// <summary>
    /// The message is banned by system.
    /// </summary>
    Ban = 9,

    /// <summary>
    /// Others.
    /// </summary>
    Others = 15
}

/// <summary>
/// The chat message record.
/// </summary>
[JsonConverter(typeof(ExtendedChatMessageConverter.InternalConverter))]
public class ExtendedChatMessage : BaseResourceEntityInfo
{
    /// <summary>
    /// Initializes a new instance of the ExtendedChatMessage class.
    /// </summary>
    /// <param name="sender">The nickname of the sender.</param>
    /// <param name="message">The message text.</param>
    /// <param name="creation">The creation date time; or null if use now.</param>
    /// <param name="info">The additional information; or null if create a new one.</param>
    public ExtendedChatMessage(UserItemInfo sender, string message, DateTime? creation = null, JsonObjectNode info = null)
        : this(Guid.NewGuid(), sender, message, creation, info, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the ExtendedChatMessage class.
    /// </summary>
    /// <param name="id">The message identifier.</param>
    /// <param name="sender">The nickname of the sender.</param>
    /// <param name="message">The message text.</param>
    /// <param name="creation">The creation date time; or null if use now.</param>
    /// <param name="info">The additional information; or null if create a new one.</param>
    /// <param name="type">The message type.</param>
    public ExtendedChatMessage(Guid id, UserItemInfo sender, string message, DateTime? creation = null, JsonObjectNode info = null, string type = null)
        : this(ExtendedChatMessages.ToIdString(id), sender, message, creation, info, type)
    {
    }

    /// <summary>
    /// Initializes a new instance of the ExtendedChatMessage class.
    /// </summary>
    /// <param name="id">The message identifier.</param>
    /// <param name="sender">The nickname of the sender.</param>
    /// <param name="message">The message text.</param>
    /// <param name="creation">The creation date time; or null if use now.</param>
    /// <param name="info">The additional information; or null if create a new one.</param>
    /// <param name="type">The message type.</param>
    public ExtendedChatMessage(string id, UserItemInfo sender, string message, DateTime? creation = null, JsonObjectNode info = null, string type = null)
        : base(id)
    {
        var time = creation ?? DateTime.Now;
        SetProperty(nameof(Sender), sender);
        SetProperty(nameof(Message), message);
        if (!string.IsNullOrEmpty(type)) SetProperty(nameof(MessageType), type);
        SetProperty(nameof(CreationTime), time);
        SetProperty(nameof(LastModificationTime), time);
        Info = info ?? new();
    }

    /// <summary>
    /// Initializes a new instance of the ExtendedChatMessage class.
    /// </summary>
    /// <param name="json">The JSON object to parse.</param>
    public ExtendedChatMessage(JsonObjectNode json)
    {
        if (json is null) return;
        Id = json.TryGetStringTrimmedValue("id", true) ?? json.Id;
        SetProperty(nameof(Sender), (UserItemInfo)json.TryGetObjectValue("sender"));
        SetProperty(nameof(Message), json.TryGetStringValue("text") ?? json.TryGetStringValue("message"));
        SetProperty(nameof(MessageType), json.TryGetStringTrimmedValue("type", true));
        SetProperty(nameof(CreationTime), json.TryGetDateTimeValue("created") ?? DateTime.Now);
        SetProperty(nameof(LastModificationTime), json.TryGetDateTimeValue("modified") ?? DateTime.Now);
        Info = json.TryGetObjectValue("info") ?? new();
        Category = json.TryGetStringTrimmedValue("category", true);
        SetProperty("Data", json.TryGetObjectValue("data"));
    }

    /// <summary>
    /// Gets the sender.
    /// </summary>
    public UserItemInfo Sender => GetCurrentProperty<UserItemInfo>();

    /// <summary>
    /// Gets the plain text of the message.
    /// </summary>
    public string Message
    {
        get
        {
            return GetCurrentProperty<string>();
        }

        set
        {
            if (!SetCurrentProperty(value)) return;
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
    /// Gets the creation date time.
    /// </summary>
    public DateTime CreationTime => GetCurrentProperty<DateTime>();

    /// <summary>
    /// Gets the creation date time.
    /// </summary>
    public DateTime LastModificationTime
    {
        get => GetCurrentProperty<DateTime>();
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
    /// Gets the additional data.
    /// </summary>
    public JsonObjectNode Info { get; }

    /// <summary>
    /// Converts to JSON object.
    /// </summary>
    /// <returns>A JSON object.</returns>
    public override JsonObjectNode ToJson()
    {
        var json = base.ToJson();
        json.SetValue("sender", Sender);
        json.SetValue("text", Message);
        json.SetValue("created", CreationTime);
        json.SetValue("modified", LastModificationTime);
        json.SetValue("type", MessageType);
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
    /// Converts the JSON raw back.
    /// </summary>
    /// <param name="value">The source value.</param>
    /// <returns>A model of the message.</returns>
    public static implicit operator ExtendedChatMessage(JsonObjectNode value)
    {
        if (value is null) return null;
        var type = value.TryGetStringTrimmedValue("type", true)?.ToLowerInvariant();
        if (type == null) return new(value);
        return type switch
        {
            ExtendedChatMessages.AttachmentLinkItemKey => new ExtendedChatMessage<AttachmentLinkItem>(value, json => new AttachmentLinkItem(json), type),
            ExtendedChatMessages.AttachmentLinkSetKey => new ExtendedChatMessage<AttachmentLinkSet>(value, json => new AttachmentLinkSet(json), type),
            ExtendedChatMessages.MarkdownKey or "text\\md" or "text/markdown" or "markdown" => new ExtendedChatMessage<ExtendedChatMessageTextData>(value, json => new ExtendedChatMessageTextData(json), ExtendedChatMessages.MarkdownKey),
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
[JsonConverter(typeof(ExtendedChatMessageConverter))]
public class ExtendedChatMessage<T> : ExtendedChatMessage where T : class
{
    /// <summary>
    /// Initializes a new instance of the ExtendedChatMessage class.
    /// </summary>
    /// <param name="type">The message type.</param>
    /// <param name="sender">The nickname of the sender.</param>
    /// <param name="data">The message data.</param>
    /// <param name="message">The message text.</param>
    /// <param name="creation">The creation date time; or null if use now.</param>
    /// <param name="info">The additional information; or null if create a new one.</param>
    protected ExtendedChatMessage(string type, UserItemInfo sender, T data, string message, DateTime? creation = null, JsonObjectNode info = null)
        : this(type, Guid.NewGuid(), sender, data, message, creation, info)
    {
    }

    /// <summary>
    /// Initializes a new instance of the ExtendedChatMessage class.
    /// </summary>
    /// <param name="type">The message type.</param>
    /// <param name="id">The message identifier.</param>
    /// <param name="sender">The nickname of the sender.</param>
    /// <param name="data">The message data.</param>
    /// <param name="message">The message text.</param>
    /// <param name="creation">The creation date time; or null if use now.</param>
    /// <param name="info">The additional information; or null if create a new one.</param>
    protected ExtendedChatMessage(string type, Guid id, UserItemInfo sender, T data, string message, DateTime? creation = null, JsonObjectNode info = null)
        : this(type, ExtendedChatMessages.ToIdString(id), sender, data, message, creation, info)
    {
    }

    /// <summary>
    /// Initializes a new instance of the ExtendedChatMessage class.
    /// </summary>
    /// <param name="type">The message type.</param>
    /// <param name="id">The message identifier.</param>
    /// <param name="sender">The nickname of the sender.</param>
    /// <param name="data">The message data.</param>
    /// <param name="message">The message text.</param>
    /// <param name="creation">The creation date time; or null if use now.</param>
    /// <param name="info">The additional information; or null if create a new one.</param>
    protected internal ExtendedChatMessage(string type, string id, UserItemInfo sender, T data, string message, DateTime? creation = null, JsonObjectNode info = null)
        : base(id, sender, message, creation, info, type)
    {
        Data = data;
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
            if (s == null || dataConverter == null) return;
            if (s.StartsWith("{") && s.StartsWith("}")) data = JsonObjectNode.TryParse(s);
            else if (s.StartsWith("[") || s.StartsWith("<")) return;
            else data = new()
            {
                { "value", s }
            };
        }

        if (data == null) return;
        Data = dataConverter == null ? data.Deserialize<T>() : dataConverter(data);
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
