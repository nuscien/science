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
    /// Gets the current message.
    /// </summary>
    public ExtendedChatMessage Message { get; }

    /// <summary>
    /// Gets the conversation.
    /// </summary>
    public ExtendedChatConversation Conversation { get; }

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
    public object Tag { get; }

    /// <summary>
    /// Gets the chat message history in the conversation.
    /// </summary>
    public ObservableCollection<ExtendedChatMessage> History => Conversation.History;

    /// <summary>
    /// Tests if the conversation is available to send message.
    /// </summary>
    /// <returns>true if it allows to send message; otherwise, false.</returns>
    public bool CanSend()
        => Conversation.CanSend;

    /// <summary>
    /// Sets the flag about if the conversation is available to send message.
    /// </summary>
    /// <param name="value">true if it allows to send message; otherwise, false.</param>
    /// <returns>The token.</returns>
    public object CanSend(bool value)
    {
        stateToken = new();
        Conversation.SetValueOfCanSendTemp(value);
        return stateToken;
    }

    /// <summary>
    /// Sets the flag about if the conversation is available to send message.
    /// </summary>
    /// <param name="oldStateToken">The state token to check. It continues to set only if this equals the one recorded.</param>
    /// <param name="value">true if it allows to send message; otherwise, false.</param>
    /// <param name="newStateToken">The new state token.</param>
    /// <returns>true if set a value indicating whether enables sending capability succeeded; otherwise, false.</returns>
    public bool CanSend(object oldStateToken, bool value, out object newStateToken)
    {
        if (oldStateToken != stateToken)
        {
            newStateToken = stateToken;
            return false;
        }

        newStateToken = CanSend(value);
        return true;
    }
}

/// <summary>
/// The sending result of chat message.
/// </summary>
[Guid("34C8EAE2-D8AC-413C-BA5B-61AEF4B90EE3")]
public class ExtendedChatMessageSendResult : ResourceEntitySavingStatus
{
    /// <summary>
    /// Initializes a new instance of the ExtendedChatMessageSendResult class.
    /// </summary>
    public ExtendedChatMessageSendResult()
    {
        Info = new();
        SendStatus = ExtendedChatMessageSendResultStates.Success;
    }

    /// <summary>
    /// Initializes a new instance of the ExtendedChatMessageSendResult class.
    /// </summary>
    /// <param name="sendStatus">The new status.</param>
    /// <param name="message">The additional message about this saving status.</param>
    public ExtendedChatMessageSendResult(ExtendedChatMessageSendResultStates sendStatus, string message = null)
    {
        Update(sendStatus, message);
    }

    /// <summary>
    /// Initializes a new instance of the ExtendedChatMessageSendResult class.
    /// </summary>
    /// <param name="code">The HTTP status code.</param>
    /// <param name="message">The additional message about this saving status.</param>
    public ExtendedChatMessageSendResult(HttpStatusCode code, string message = null)
    {
        Update(code, message);
    }

    /// <summary>
    /// Initializes a new instance of the ExtendedChatMessageSendResult class.
    /// </summary>
    /// <param name="ex">The exception.</param>
    /// <param name="message">The additional message about this saving status.</param>
    public ExtendedChatMessageSendResult(Exception ex, string message = null)
    {
        Update(ex, message);
    }

    /// <summary>
    /// Gets the result state.
    /// </summary>
    public ExtendedChatMessageSendResultStates SendStatus
    {
        get => GetCurrentProperty<ExtendedChatMessageSendResultStates>();
        private set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets the additional information.
    /// </summary>
    public JsonObjectNode Info { get; }

    /// <summary>
    /// Gets a value indicating whether the state is successful.
    /// </summary>
    public bool IsSuccessful => State == ResourceEntitySavingStates.Ready || SendStatus == ExtendedChatMessageSendResultStates.Success;

    /// <summary>
    /// Gets a value indicating whether need throw the exception.
    /// </summary>
    internal bool ShouldThrowException => SendStatus == ExtendedChatMessageSendResultStates.Aborted || SendStatus == ExtendedChatMessageSendResultStates.OtherError;

    /// <summary>
    /// Gets a value indicating whether the state is a retry-able one.
    /// </summary>
    internal bool CanRetry => SendStatus == ExtendedChatMessageSendResultStates.NotSend || SendStatus == ExtendedChatMessageSendResultStates.Throttle || SendStatus == ExtendedChatMessageSendResultStates.NetworkIssue;

    /// <inheritdoc />
    public override void Update(ResourceEntitySavingStates state, string message = null)
    {
        base.Update(state, message);
        Info.Clear();
        switch (state)
        {
            case ResourceEntitySavingStates.Local:
                SendStatus = ExtendedChatMessageSendResultStates.NotSend;
                break;
            case ResourceEntitySavingStates.Ready:
                SendStatus = ExtendedChatMessageSendResultStates.Success;
                break;
            case ResourceEntitySavingStates.Disabled:
                SendStatus = ExtendedChatMessageSendResultStates.Forbidden;
                break;
        }
    }

    /// <summary>
    /// Updates the saving status of this entity.
    /// </summary>
    /// <param name="sendStatus">The new status.</param>
    /// <param name="message">The additional message about this saving status.</param>
    public virtual void Update(ExtendedChatMessageSendResultStates sendStatus, string message = null)
    {
        var state = ExtendedChatMessages.ToSavingState(sendStatus);
        SendStatus = sendStatus;
        base.Update(state, message);
        Info.Clear();
    }

    /// <summary>
    /// Updates the saving status of this entity.
    /// </summary>
    /// <param name="ex">The exception thrown.</param>
    /// <param name="message">The additional message about this saving status.</param>
    public virtual void Update(Exception ex, string message = null)
    {
        Info.Clear();
        if (ex == null)
        {
            Update(ExtendedChatMessageSendResultStates.Success, message);
            return;
        }

        if (ex is FailedHttpException httpEx)
            Update(httpEx, message);
        else if (ex is HttpRequestException httpEx2)
            Update(httpEx2, message);
        else if (ex is ArgumentException argEx)
            Update(argEx, message);
        else if (ex is InvalidOperationException invalidEx)
            Update(invalidEx, message);
        else if (ex is JsonException || ex is IOException || ex is InvalidCastException castEx|| ex is FormatException formatEx)
            Update(ExtendedChatMessageSendResultStates.ClientError, message ?? ex.Message);
        else if (ex is OperationCanceledException)
            Update(ExtendedChatMessageSendResultStates.Aborted, message ?? ex.Message);
        else if (ex is UnauthorizedAccessException || ex is SecurityException)
            Update(ExtendedChatMessageSendResultStates.Forbidden, message ?? ex.Message);
        else
            Update(ExtendedChatMessageSendResultStates.OtherError, message ?? ex?.Message);
    }

    /// <summary>
    /// Updates the saving status of this entity.
    /// </summary>
    /// <param name="ex">The exception thrown.</param>
    /// <param name="message">The additional message about this saving status.</param>
    private void Update(FailedHttpException ex, string message = null)
    {
        if (!ex.StatusCode.HasValue)
        {
            Update(ExtendedChatMessageSendResultStates.NetworkIssue, message ?? ex.Message);
            return;
        }

        var code = (int)ex.StatusCode.Value;
        var httpStatus = Update(ex.StatusCode.Value, message ?? ex.Message);
        httpStatus.SetValue("reason", ex.ReasonPhrase);
        try
        {
            httpStatus.SetValueIfNotEmpty("host", ex.RequestMessage?.RequestUri?.Host);
        }
        catch (InvalidOperationException)
        {
        }
        catch (ArgumentException)
        {
        }
        catch (FormatException)
        {
        }
        catch (NotSupportedException)
        {
        }
        catch (NullReferenceException)
        {
        }
        catch (NotImplementedException)
        {
        }
    }

    /// <summary>
    /// Updates the saving status of this entity.
    /// </summary>
    /// <param name="ex">The exception thrown.</param>
    /// <param name="message">The additional message about this saving status.</param>
    private void Update(HttpRequestException ex, string message = null)
    {
#if NETCOREAPP
        if (ex.StatusCode.HasValue)
            Update(ex.StatusCode.Value, message ?? ex.Message);
        else
#endif
            Update(ExtendedChatMessageSendResultStates.NetworkIssue, message ?? ex.Message);
    }

    /// <summary>
    /// Updates the saving status of this entity.
    /// </summary>
    /// <param name="ex">The exception thrown.</param>
    /// <param name="message">The additional message about this saving status.</param>
    private void Update(ArgumentException ex, string message = null)
    {
        var argsInfo = new JsonObjectNode();
        argsInfo.SetValueIfNotEmpty("name", ex.ParamName);
        Info.SetValue("args", argsInfo);
        Update(ExtendedChatMessageSendResultStates.RequestError, message ?? ex.Message);
    }

    /// <summary>
    /// Updates the saving status of this entity.
    /// </summary>
    /// <param name="ex">The exception thrown.</param>
    /// <param name="message">The additional message about this saving status.</param>
    private void Update(InvalidOperationException ex, string message = null)
    {
        if (ex.InnerException is FailedHttpException httpEx)
        {
            Update(httpEx, message ?? ex.Message);
            return;
        }

        Update(ExtendedChatMessageSendResultStates.ClientError, message ?? ex.Message);
    }

    private JsonObjectNode Update(HttpStatusCode status, string message)
    {
        var code = (int)status;
        var httpStatus = new JsonObjectNode
        {
            { "status", code }
        };
        Info.SetValue("http", httpStatus);
        if (code < 400)
        {
            Update(ExtendedChatMessageSendResultStates.NetworkIssue, message);
        }
        else if (code < 500)
        {
            Update(code switch
            {
                401 or 402 or 403 => ExtendedChatMessageSendResultStates.Forbidden,
                429 => ExtendedChatMessageSendResultStates.Throttle,
                _ => ExtendedChatMessageSendResultStates.RequestError
            }, message);
        }
        else if (code < 700)
        {
            Update(ExtendedChatMessageSendResultStates.ServerError, message);
        }
        else
        {
            Update(ExtendedChatMessageSendResultStates.OtherError, message);
        }

        return httpStatus;
    }

    /// <summary>
    /// Converts to the result of chat message sending.
    /// </summary>
    /// <param name="sendStatus">The value</param>
    public static implicit operator ExtendedChatMessageSendResult(ExtendedChatMessageSendResultStates sendStatus)
        => new(sendStatus);

    /// <summary>
    /// Converts to the result of chat message sending.
    /// </summary>
    /// <param name="code">The value</param>
    public static implicit operator ExtendedChatMessageSendResult(HttpStatusCode code)
        => new(code);

    /// <summary>
    /// Converts to the result of chat message sending.
    /// </summary>
    /// <param name="success">The value</param>
    public static implicit operator ExtendedChatMessageSendResult(bool success)
        => new(success ? ExtendedChatMessageSendResultStates.Success : ExtendedChatMessageSendResultStates.OtherError);
}
