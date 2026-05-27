using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Trivial.Data;
using Trivial.Users;

namespace Trivial.Text;

/// <summary>
/// The exception thrown when the extended chat message sending is not supported.
/// </summary>
public class ExtendedChatMessageException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExtendedChatMessageException"/> class with a specified state and message, and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    public ExtendedChatMessageException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtendedChatMessageException"/> class with a specified state and message, and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="context">The chat message context.</param>
    /// <param name="message">The message that describes the error.</param>
    public ExtendedChatMessageException(ExtendedChatMessageContext context, string message = null)
       : base(message)
    {
        if (context == null) return;
        Conversation = context.Conversation;
        TargetMessage = context.Message;
        ChangingMethod = context.ChangingMethod;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtendedChatMessageException"/> class with a specified state and message, and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="conversation">The chat conversation.</param>
    /// <param name="chatMessage">The chat message.</param>
    /// <param name="changing">The changing method.</param>
    /// <param name="exceptionMessage">The message that describes the error.</param>
    /// <param name="innerException">The inner exception.</param>
    public ExtendedChatMessageException(ExtendedChatConversation conversation, ExtendedChatMessage chatMessage, ChangeMethods changing, string exceptionMessage = null, Exception innerException = null)
       : base(exceptionMessage, innerException)
    {
        Conversation = conversation;
        TargetMessage = chatMessage;
        ChangingMethod = changing;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtendedChatMessageException"/> class with a specified state and message, and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="context">The chat message context.</param>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The inner exception.</param>
    public ExtendedChatMessageException(ExtendedChatMessageContext context, string message, Exception innerException)
        : base(message, innerException)
    {
        if (context == null) return;
        Conversation = context.Conversation;
        TargetMessage = context.Message;
        ChangingMethod = context.ChangingMethod;
    }

    /// <summary>
    /// Gets the current message.
    /// </summary>
    public ExtendedChatMessage TargetMessage { get; }

    /// <summary>
    /// Gets the conversation.
    /// </summary>
    public ExtendedChatConversation Conversation { get; }

    /// <summary>
    /// Gets the sender.
    /// </summary>
    public BaseUserItemInfo Sender => TargetMessage?.Sender ?? Conversation?.Sender;

    /// <summary>
    /// Gets the changing method.
    /// </summary>
    public ChangeMethods ChangingMethod { get; }

    /// <summary>
    /// Gets the chat message history in the conversation.
    /// </summary>
    public ObservableCollection<ExtendedChatMessage> ChatHistory => Conversation?.History;
}

/// <summary>
/// The exception thrown when the extended chat message sending is not supported.
/// </summary>
public class ExtendedChatMessageAvailabilityException : NotSupportedException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExtendedChatMessageAvailabilityException"/> class with a specified state and message, and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    public ExtendedChatMessageAvailabilityException()
       : this(ExtendedChatMessageAvailability.Disabled)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtendedChatMessageAvailabilityException"/> class with a specified state and message, and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ExtendedChatMessageAvailabilityException(string message)
       : this(ExtendedChatMessageAvailability.Disabled, message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtendedChatMessageAvailabilityException"/> class with a specified state and message, and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="state">The state about if the user can send message.</param>
    /// <param name="message">The message that describes the error.</param>
    public ExtendedChatMessageAvailabilityException(ExtendedChatMessageAvailability state, string message = null)
        : base(message)
    {
        State = state;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtendedChatMessageAvailabilityException"/> class with a specified state and message, and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="state">The state about if the user can send message.</param>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The inner exception.</param>
    public ExtendedChatMessageAvailabilityException(ExtendedChatMessageAvailability state, string message, Exception innerException)
        : base(message, innerException)
    {
        State = state;
    }

    /// <summary>
    /// Gets the state about if the user can send message.
    /// </summary>
    public ExtendedChatMessageAvailability State { get; }

    /// <summary>
    /// Gets a value indicating whether the user is allowed to send message.
    /// </summary>
    public bool IsAllowed => State == ExtendedChatMessageAvailability.Allowed;
}
