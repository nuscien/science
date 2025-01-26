using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
/// The rich text data for chat message.
/// </summary>
[JsonConverter(typeof(ExtendedChatMessageTextDataConverter))]
public class ExtendedChatMessageTextData : BaseObservableProperties, IJsonObjectHost
{
    /// <summary>
    /// Initializes a new instance of the ExtendedChatMessageTextData class.
    /// </summary>
    public ExtendedChatMessageTextData()
    {
        Info = new();
    }

    /// <summary>
    /// Initializes a new instance of the ExtendedChatMessageTextData class.
    /// </summary>
    /// <param name="value">The rich text.</param>
    public ExtendedChatMessageTextData(string value)
    {
        Value = value;
        Info = new();
    }

    /// <summary>
    /// Initializes a new instance of the ExtendedChatMessageTextData class.
    /// </summary>
    /// <param name="json">The JSON object to parse.</param>
    public ExtendedChatMessageTextData(JsonObjectNode json)
    {
        if (json == null) return;
        Value = json.TryGetStringValue("value");
        Info = json.TryGetObjectValue("info") ?? new();
    }

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    public string Value
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets the additional information.
    /// </summary>
    public JsonObjectNode Info { get; }

    /// <summary>
    /// Converts to JSON object.
    /// </summary>
    /// <returns>A JSON object.</returns>
    public virtual JsonObjectNode ToJson()
    {
        var json = new JsonObjectNode
        {
            { "value", Value }
        };
        if (Info.Count > 0) json.SetValue("info", Info);
        return json;
    }

    /// <summary>
    /// Returns a string which represents the object instance.
    /// </summary>
    /// <returns>A string which represents the object instance.</returns>
    public override string ToString()
        => Value;

    /// <summary>
    /// Converts the JSON raw back.
    /// </summary>
    /// <param name="value">The source value.</param>
    /// <returns>A model of the message.</returns>
    public static implicit operator ExtendedChatMessageTextData(JsonObjectNode value)
        => new(value);

    /// <summary>
    /// Converts to JSON object.
    /// </summary>
    /// <param name="value">The JSON value.</param>
    /// <returns>A JSON object.</returns>
    public static explicit operator JsonObjectNode(ExtendedChatMessageTextData value)
        => value?.ToJson();
}

/// <summary>
/// The factory of the extended chat message data.
/// </summary>
/// <typeparam name="T">The type of data.</typeparam>
public abstract class BaseExtendedChatMessageDataFactory<T> where T : class
{
    /// <summary>
    /// Gets the message type.
    /// </summary>
    public string MessageType { get; }

    /// <summary>
    /// Generates the data instance from a JSON object.
    /// </summary>
    /// <param name="json">The JSON object node input.</param>
    /// <returns>The instance of the data.</returns>
    public virtual T Create(JsonObjectNode json)
        => json == null ? default : json.Deserialize<T>();

    /// <summary>
    /// Occurs on the message is created.
    /// </summary>
    /// <param name="message">The chat message created.</param>
    protected virtual void OnCreateMessage(ExtendedChatMessage<T> message)
    {
    }

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
    public ExtendedChatMessage<T> CreateMessage(UserItemInfo sender, T data, string message, ExtendedChatMessageFormats format = ExtendedChatMessageFormats.Text, DateTime? creation = null, JsonObjectNode info = null)
        => CreateMessage(Guid.NewGuid(), sender, data, message, format, creation, info);

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
    public ExtendedChatMessage<T> CreateMessage(Guid id, UserItemInfo sender, T data, string message, ExtendedChatMessageFormats format = ExtendedChatMessageFormats.Text, DateTime? creation = null, JsonObjectNode info = null)
        => CreateMessage(ExtendedChatMessages.ToIdString(id), sender, data, message, format, creation, info);

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
    public ExtendedChatMessage<T> CreateMessage(string id, UserItemInfo sender, T data, string message, ExtendedChatMessageFormats format = ExtendedChatMessageFormats.Text, DateTime? creation = null, JsonObjectNode info = null)
    {
        if (string.IsNullOrWhiteSpace(MessageType)) return null;
        var obj = new ExtendedChatMessage<T>(MessageType, id, sender, data, message, format, creation, info);
        OnCreateMessage(obj);
        return obj;
    }

    /// <summary>
    /// Creates a chat message record.
    /// </summary>
    /// <param name="json">The JSON object to parse.</param>
    /// <returns>The chat message.</returns>
    public ExtendedChatMessage<T> CreateMessage(JsonObjectNode json)
    {
        var obj = new ExtendedChatMessage<T>(json, Create, MessageType);
        if (string.IsNullOrWhiteSpace(obj.MessageType)) return null;
        OnCreateMessage(obj);
        return obj;
    }
}

/// <summary>
/// JSON value node converter.
/// </summary>
internal sealed class ExtendedChatMessageTextDataConverter : JsonObjectHostConverter<ExtendedChatMessageTextData>
{
    /// <inheritdoc />
    protected override ExtendedChatMessageTextData Create(JsonObjectNode json)
        => new(json);
}
