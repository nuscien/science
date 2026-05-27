using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Trivial.Data;
using Trivial.Net;

namespace Trivial.Text;

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
        else if (ex is JsonException || ex is IOException || ex is InvalidCastException castEx || ex is FormatException formatEx)
            Update(ExtendedChatMessageSendResultStates.ClientError, message ?? ex.Message);
        else if (ex is OperationCanceledException)
            Update(ExtendedChatMessageSendResultStates.Aborted, message ?? ex.Message);
        else if (ex is UnauthorizedAccessException || ex is SecurityException)
            Update(ExtendedChatMessageSendResultStates.Forbidden, message ?? ex.Message);
        else
            Update(ExtendedChatMessageSendResultStates.OtherError, message ?? ex?.Message);
    }

    internal bool ThrowException(Exception ex, ExtendedChatMessageContext context)
    {
        if (ex is ExtendedChatMessageException || ex is NotSupportedException || ex is InvalidOperationException || ex is OutOfMemoryException) return true;
        if (ex is FailedHttpException || ex is JsonException || ex is HttpRequestException || ex is IOException || ex is FormatException || ex is InvalidCastException)
            throw new ExtendedChatMessageException(context, ex.Message, ex);
        return SendStatus != ExtendedChatMessageSendResultStates.Success;
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
