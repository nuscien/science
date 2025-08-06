using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Trivial.Reflection;
using Trivial.Tasks;

namespace Trivial.Text;

/// <summary>
/// The chat message content.
/// </summary>
[Guid("FEC3B850-D7C6-4FD4-BE84-91DE279A3DFF")]
public class ExtendedChatMessageContent
{
    /// <summary>
    /// Initializes a new instance of the ExtendedChatMessageContent class.
    /// </summary>
    public ExtendedChatMessageContent()
    {
        Info = new();
    }

    /// <summary>
    /// Initializes a new instance of the ExtendedChatMessageContent class.
    /// </summary>
    /// <param name="message">The message content.</param>
    /// <param name="format">The message format.</param>
    /// <param name="info">The additional info.</param>
    public ExtendedChatMessageContent(string message, ExtendedChatMessageFormats format = ExtendedChatMessageFormats.Text, JsonObjectNode info = null)
        : this(message, format, null, info)
    {
    }

    /// <summary>
    /// Initializes a new instance of the ExtendedChatMessageContent class.
    /// </summary>
    /// <param name="message">The message content.</param>
    /// <param name="format">The message format.</param>
    /// <param name="category">The optional category.</param>
    /// <param name="info">The additional info.</param>
    public ExtendedChatMessageContent(string message, ExtendedChatMessageFormats format, string category, JsonObjectNode info = null)
    {
        Message = message;
        MessageFormat = format;
        Category = category;
        Info = info ?? new();
    }

    /// <summary>
    /// Gets the markdown text of the message.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Gets or sets the modification kind.
    /// </summary>
    public ChatMessageModificationKinds ModificationKind { get; set; }

    /// <summary>
    /// Gets or sets the message type.
    /// </summary>
    public string MessageType { get; set; }

    /// <summary>
    /// Gets or sets the message format.
    /// </summary>
    public ExtendedChatMessageFormats MessageFormat { get; set; }

    /// <summary>
    /// Gets or sets the category.
    /// </summary>
    public string Category { get; set; }

    /// <summary>
    /// Gets or sets the priority.
    /// </summary>
    public BasicPriorities Priority { get; set; } = BasicPriorities.Normal;

    /// <summary>
    /// Gets the additional data.
    /// </summary>
    public JsonObjectNode Info { get; }
}

/// <summary>
/// The chat message content.
/// </summary>
public class ExtendedChatMessageContent<T> : ExtendedChatMessageContent
{
    /// <summary>
    /// Initializes a new instance of the ExtendedChatMessageContent class.
    /// </summary>
    public ExtendedChatMessageContent()
        : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the ExtendedChatMessageContent class.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <param name="message">The message content.</param>
    /// <param name="format">The message format.</param>
    /// <param name="info">The additional info.</param>
    public ExtendedChatMessageContent(T data, string message, ExtendedChatMessageFormats format = ExtendedChatMessageFormats.Text, JsonObjectNode info = null)
        : this(data, message, format, null, info)
    {
    }

    /// <summary>
    /// Initializes a new instance of the ExtendedChatMessageContent class.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <param name="message">The message content.</param>
    /// <param name="format">The message format.</param>
    /// <param name="category">The optional category.</param>
    /// <param name="info">The additional info.</param>
    public ExtendedChatMessageContent(T data, string message, ExtendedChatMessageFormats format, string category, JsonObjectNode info = null)
        : base(message, format, category, info)
    {
        if (data == null) return;
        Data = data;
        if (data is IExtendedChatMessageDataDescription desc)
        {
            MessageType = desc.MessageType;
        }
        else
        {
            var id = ObjectConvert.GetGuid(data.GetType()) ?? ObjectConvert.GetGuid(typeof(T));
            if (id.HasValue) MessageType = ExtendedChatMessages.ToIdString(id.Value);
        }
    }

    /// <summary>
    /// Initializes a new instance of the ExtendedChatMessageContent class.
    /// </summary>
    /// <param name="type">The message type.</param>
    /// <param name="data">The data.</param>
    /// <param name="message">The message content.</param>
    /// <param name="format">The message format.</param>
    /// <param name="info">The additional info.</param>
    public ExtendedChatMessageContent(string type, T data, string message, ExtendedChatMessageFormats format = ExtendedChatMessageFormats.Text, JsonObjectNode info = null)
        : this(type, data, message, format, null, info)
    {
    }

    /// <summary>
    /// Initializes a new instance of the ExtendedChatMessageContent class.
    /// </summary>
    /// <param name="type">The message type.</param>
    /// <param name="data">The data.</param>
    /// <param name="message">The message content.</param>
    /// <param name="format">The message format.</param>
    /// <param name="category">The optional category.</param>
    /// <param name="info">The additional info.</param>
    public ExtendedChatMessageContent(string type, T data, string message, ExtendedChatMessageFormats format, string category, JsonObjectNode info = null)
        : base(message, format, category, info)
    {
        MessageType = type;
        Data = data;
    }

    /// <summary>
    /// Gets the data of customized message details.
    /// </summary>
    public T Data { get; set; }
}
