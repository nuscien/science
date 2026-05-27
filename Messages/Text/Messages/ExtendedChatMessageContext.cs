using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Trivial.Data;
using Trivial.Net;
using Trivial.Reflection;
using Trivial.Users;

namespace Trivial.Text;

/// <summary>
/// The context of chat message action.
/// </summary>
[Guid("CB9AFA18-569E-4BFD-BF37-9E0EE0171AF8")]
public class ExtendedChatMessageContext
{
    private object stateToken;

    /// <summary>
    /// Initializes a new instance of the ExtendedChatMessageContext class.
    /// </summary>
    /// <param name="message">The current message.</param>
    /// <param name="conversation">The conversation.</param>
    /// <param name="changing">The changing method.</param>
    /// <param name="parameter">The additional parameter.</param>
    internal ExtendedChatMessageContext(ExtendedChatMessage message, ExtendedChatConversation conversation, ChangeMethods changing, object parameter)
    {
        Message = message;
        Conversation = conversation;
        ChangingMethod = changing;
        Parameter = parameter;
    }

    /// <summary>
    /// Gets the additional parameter.
    /// </summary>
    public object Parameter { get; }

    /// <summary>
    /// Gets the details information of processing and response.
    /// </summary>
    public object Details { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the details information is avalaible to set.
    /// </summary>
    public bool CanSetDetails { get; internal set; }

    /// <summary>
    /// Gets the current message.
    /// </summary>
    public ExtendedChatMessage Message { get; }

    /// <summary>
    /// Gets the conversation.
    /// </summary>
    public ExtendedChatConversation Conversation { get; }

    /// <summary>
    /// Gets the conversation parameter.
    /// </summary>
    public object ConversationParameter => Conversation?.Parameter;

    /// <summary>
    /// Gets the sender.
    /// </summary>
    public BaseUserItemInfo Sender => Message?.Sender ?? Conversation?.Sender;

    /// <summary>
    /// Gets the changing method.
    /// </summary>
    public ChangeMethods ChangingMethod { get; }

    /// <summary>
    /// Gets or sets the additional information object.
    /// </summary>
    public object Tag { get; set; }

    /// <summary>
    /// Gets the chat message history in the conversation.
    /// </summary>
    public ObservableCollection<ExtendedChatMessage> History => Conversation.History;

    /// <summary>
    /// Gets the state about if the user can send message.
    /// </summary>
    public ExtendedChatMessageAvailability Availability => Conversation.Availability;

    /// <summary>
    /// Tests if the conversation is available to send message.
    /// </summary>
    /// <returns>true if it allows to send message; otherwise, false.</returns>
    public bool CanSend => Conversation.CanSend;

    /// <summary>
    /// Sets the details information of processing and response.
    /// Only during the sending is occurring the provider can set.
    /// </summary>
    /// <param name="value">The details information of processing and response.</param>
    /// <returns>true if set succeeded; otherwise, false.</returns>
    public bool SetDetails(object value)
    {
        var b = CanSetDetails;
        if (b) Details = value;
        return b;
    }

    /// <summary>
    /// Sets the flag about if the conversation is available to send message.
    /// </summary>
    /// <param name="state">true if the user can send message; otherwise, false.</param>
    /// <returns>The token.</returns>
    public object SetAvailability(bool state)
        => SetAvailability(state ? ExtendedChatMessageAvailability.Allowed : ExtendedChatMessageAvailability.Disabled);

    /// <summary>
    /// Sets the flag about if the conversation is available to send message.
    /// </summary>
    /// <param name="state">The state about if the user can send message.</param>
    /// <returns>The token.</returns>
    public object SetAvailability(ExtendedChatMessageAvailability state)
    {
        stateToken = new();
        Conversation.SetTempAvailability(state);
        return stateToken;
    }

    /// <summary>
    /// Sets the flag about if the conversation is available to send message.
    /// </summary>
    /// <param name="oldStateToken">The state token to check. It continues to set only if this equals the one recorded.</param>
    /// <param name="state">The state about if the user can send message.</param>
    /// <param name="newStateToken">The new state token.</param>
    /// <returns>true if set a value indicating whether enables sending capability succeeded; otherwise, false.</returns>
    public bool SetAvailability(object oldStateToken, ExtendedChatMessageAvailability state, out object newStateToken)
    {
        if (oldStateToken != stateToken)
        {
            newStateToken = stateToken;
            return false;
        }

        newStateToken = SetAvailability(state);
        return true;
    }

    /// <summary>
    /// Tries to convert the value in a specific type.
    /// </summary>
    /// <typeparam name="T">The type of value.</typeparam>
    /// <param name="value">The value converted.</param>
    /// <returns>true if the type is the specific one; otherwise, false.</returns>
    public bool ParameterIs<T>(out T value)
        => ObjectConvert.TryGet(Parameter, out value);
}

/// <summary>
/// The context to load chat message history.
/// </summary>
/// <param name="conversation">The chat conversation.</param>
[Guid("57127736-14A9-4E89-9CBA-A8946E64AFDB")]
public class ExtendedChatMessageHistoryContext(ExtendedChatConversation conversation)
{
    /// <summary>
    /// Gets or sets the status of the history.
    /// </summary>
    public object Status
    {
        get => Conversation.HistoryStatus;
        set => Conversation.HistoryStatus = value;
    }

    /// <summary>
    /// Gets the chat conversation.
    /// </summary>
    public ExtendedChatConversation Conversation { get; } = conversation;

    /// <summary>
    /// Gets the conversation parameter.
    /// </summary>
    public object ConversationParameter => Conversation?.Parameter;

    /// <summary>
    /// Gets the conversation sender.
    /// </summary>
    public BaseUserItemInfo Sender => Conversation?.Sender;

    /// <summary>
    /// Tries to convert the status in a specific type.
    /// </summary>
    /// <typeparam name="T">The type of value.</typeparam>
    /// <param name="value">The status converted.</param>
    /// <returns>true if the type is the specific one; otherwise, false.</returns>
    public bool StatusIs<T>(out T value)
        => ObjectConvert.TryGet(Status, out value);

    /// <summary>
    /// Adds a message to the specific conversation.
    /// </summary>
    /// <param name="message">The message to add.</param>
    /// <param name="last">true if append at last; otherwise, false.</param>
    public void AddMessage(ExtendedChatMessage message, bool last)
        => Conversation?.AddMessage(message, last);

    /// <summary>
    /// Adds a message to the specific conversation.
    /// </summary>
    /// <param name="message">The message to add.</param>
    public void AddMessage(ExtendedChatMessage message)
        => AddMessage(message, false);

    /// <summary>
    /// Adds a collection of messages to the conversation history.
    /// All messages in the collection will be added to the beginning of the history.
    /// </summary>
    /// <param name="messages">The message collection.</param>
    /// <returns>The count of message added.</returns>
    public int AddHistory(IEnumerable<ExtendedChatMessage> messages)
        => Conversation?.AddHistory(messages) ?? 0;

    /// <summary>
    /// Adds a collection of messages to the conversation history.
    /// All messages in the collection will be added to the beginning of the history.
    /// </summary>
    /// <param name="messages">The message collection.</param>
    /// <returns>The count of message added.</returns>
    public Task<int> AddHistoryAsync(IAsyncEnumerable<ExtendedChatMessage> messages)
    {
        if (Conversation is null) return Task.FromResult(0);
        return Conversation.AddHistoryAsync(messages);
    }

    /// <summary>
    /// Gets the earlist message date in the conversation history.
    /// </summary>
    /// <returns>The date time of earlist message; or null, if no message.</returns>
    public DateTime? GetEarliestMessageDate()
        => Conversation?.GetEarliestMessageDate();

    /// <summary>
    /// Gets the latest message date in the conversation history.
    /// </summary>
    /// <returns>The date time of latest message; or null, if no message.</returns>
    public DateTime? GetLatestMessageDate()
        => Conversation?.GetLatestMessageDate();
}

/// <summary>
/// The parameter of chat message action.
/// </summary>
[Guid("FCEBF269-CA48-4B5F-85F3-EB04862030ED")]
public class ExtendedChatMessageParameter : ObservableProperties
{
    /// <summary>
    /// Initializes a new instance of the ExtendedChatMessageParameter class.
    /// </summary>
    public ExtendedChatMessageParameter()
    {
    }

    /// <summary>
    /// Initializes a new instance of the ExtendedChatMessageParameter class.
    /// </summary>
    /// <param name="parameter">The parameter.</param>
    public ExtendedChatMessageParameter(object parameter)
    {
        Parameter = parameter;
    }

    /// <summary>
    /// Gets or sets the message.
    /// </summary>
    public ExtendedChatMessage Message
    {
        get => GetCurrentProperty<ExtendedChatMessage>();
        private set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets the changing method.
    /// </summary>
    public ChangeMethods ChangingMethod
    {
        get => GetCurrentProperty<ChangeMethods>();
        private set => SetCurrentProperty(value);
    }


    /// <summary>
    /// Gets the result state.
    /// </summary>
    public ExtendedChatMessageSendResultStates SendStatus
    {
        get => GetCurrentProperty<ExtendedChatMessageSendResultStates>();
        internal set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the details information of processing and response.
    /// The object is defined by the provider of the chat message service.
    /// </summary>
    public object Details
    {
        get => GetCurrentProperty<object>();
        internal set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the parameter.
    /// </summary>
    public object Parameter
    {
        get => GetCurrentProperty<object>();
        private set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Tries to convert the value in a specific type.
    /// </summary>
    /// <typeparam name="T">The type of value.</typeparam>
    /// <param name="value">The value converted.</param>
    /// <returns>true if the type is the specific one; otherwise, false.</returns>
    public bool ParameterIs<T>(out T value)
        => ObjectConvert.TryGet(Parameter, out value);

    /// <summary>
    /// Occurs on begin.
    /// </summary>
    protected virtual void OnBegin()
    {
    }

    /// <summary>
    /// Occurs on succeed.
    /// </summary>
    protected virtual void OnSucceed()
    {
    }

    /// <summary>
    /// Occurs on fail.
    /// </summary>
    protected virtual void OnFail()
    {
    }

    /// <summary>
    /// Occurs on cancel.
    /// </summary>
    protected virtual void OnCancel()
    {
    }

    internal void Begin(ExtendedChatMessage message, ChangeMethods change)
    {
        Message = message;
        ChangingMethod = change;
        OnBegin();
    }

    internal void End(bool success)
    {
        if (success) OnSucceed();
        else OnFail();
    }

    internal void Cancel()
        => OnCancel();
}
