using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Trivial.CommandLine;
using Trivial.Data;
using Trivial.Net;
using Trivial.Reflection;
using Trivial.Text;
using Trivial.Users;

namespace Trivial.Tasks;

/// <summary>
/// The base provider of responsive chat message service.
/// </summary>
/// <param name="profile">The provider profile of chat message service.</param>
public abstract class BaseResponsiveChatProvider(BaseUserItemInfo profile)
{
    private bool isDisabled;

    /// <summary>
    /// Adds or removes the event handler raised on property changed.
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// Adds or removes the event handler occurred on the answer state is changed.
    /// </summary>
    public event DataEventHandler<ResponsiveChatMessageModel> AnswerStateChanged;

    /// <summary>
    /// Adds or removes the event handler occurred on a message is waiting to send.
    /// </summary>
    public event DataEventHandler<ResponsiveChatMessageModel> Sending;

    /// <summary>
    /// Adds or removes the event handler occurred on a message has already sent and got the response.
    /// </summary>
    public event DataEventHandler<ResponsiveChatMessageModel> Sent;

    /// <summary>
    /// Adds or removes the event handler occurred on a message is failed to get response.
    /// </summary>
    public event DataEventHandler<ResponsiveChatMessageModel> SendFailed;

    /// <summary>
    /// Adds or removes the event handler occurred on a message is canceled to get response.
    /// </summary>
    public event DataEventHandler<ResponsiveChatMessageModel> SendCanceled;

    /// <summary>
    /// Gets the bot info.
    /// </summary>
    public BaseUserItemInfo Profile { get; } = profile;

    /// <summary>
    /// Gets or sets the notification profile.
    /// </summary>
    public BaseUserItemInfo NotificationProfile { get; set; }

    /// <summary>
    /// Gets a value indicating whether enables server-sent event mode.
    /// </summary>
    public bool IsStreaming { get; protected set; }

    /// <summary>
    /// Gets the current chat message topic.
    /// </summary>
    public ResponsiveChatMessageTopic CurrentTopic { get; private set; }

    /// <summary>
    /// Gets the identifier of the chat service.
    /// </summary>
    public virtual string Id => Profile?.Id ?? Guid.Empty.ToString("N");

    /// <summary>
    /// Gets a value indicating whether throw the original exception during sending.
    /// </summary>
    public bool ThrowOriginalOnSending { get; set; }

    /// <summary>
    /// Gets a value indicating whether the chat service is disabled.
    /// </summary>
    public bool IsDisabled
    {
        get
        {
            return isDisabled || string.IsNullOrWhiteSpace(Profile?.Id);
        }

        protected set
        {
            if (string.IsNullOrWhiteSpace(Profile?.Id)) return;
            isDisabled = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDisabled)));
        }
    }

    /// <summary>
    /// Creates a new round chat message topic.
    /// </summary>
    /// <param name="skipIfExists">true if skip creating if already has one; otherwise, false.</param>
    /// <returns>true if create succeeded; otherwise, false.</returns>
    public async Task<bool> CreateTopicAsync(bool skipIfExists = false)
    {
        if (IsDisabled) return false;
        if (CurrentTopic != null && skipIfExists) return false;
        CurrentTopic = await CreateNewTopicAsync();
        return CurrentTopic != null;
    }

    /// <summary>
    /// Creates a new round chat message topic.
    /// </summary>
    /// <returns>The round chat message topic.</returns>
    protected virtual Task<ResponsiveChatMessageTopic> CreateNewTopicAsync()
        => Task.FromResult(new ResponsiveChatMessageTopic(Guid.NewGuid().ToString())
        {
            IsEnabled = true
        });

    /// <summary>
    /// Gets the summary of data for the response message.
    /// </summary>
    /// <param name="data">The data from business service.</param>
    /// <returns>A summary information.</returns>
    protected virtual JsonObjectNode GetDataSummary(JsonObjectNode data)
    {
        var json = new JsonObjectNode();
        if (data == null) return json;
        json.SetValueIfNotNull("action", data.TryGetObjectValue("action"));
        json.SetValueIfNotNull("intent", data.TryGetObjectValue("intent"));
        json.SetValueIfNotNull("message", data.TryGetStringTrimmedValue("message", true) ?? data.TryGetStringTrimmedValue("msg", true));
        json.SetValueIfNotNull("code", data.TryGetStringTrimmedValue("code", true));
        if (data.ContainsKey("error"))
        {
            var kind = data.GetValueKind("error");
            switch (kind)
            {
                case JsonValueKind.String:
                    json.SetValue("error", data.TryGetStringTrimmedValue("error", true));
                    break;
                case JsonValueKind.True:
                    json.SetValue("error", true);
                    break;
                case JsonValueKind.False:
                    json.SetValue("error", false);
                    break;
                case JsonValueKind.Number:
                    json.SetValue("error", data.TryGetDoubleValue("error", true));
                    break;
                case JsonValueKind.Object:
                    json.SetValue("error", data.TryGetObjectValue("error"));
                    break;
                case JsonValueKind.Array:
                    json.SetValue("error", data.TryGetArrayValue("error"));
                    break;
            }
        }

        return json;
    }

    /// <summary>
    /// Gets the response message.
    /// </summary>
    /// <param name="message">The request message.</param>
    /// <param name="data">The data.</param>
    /// <param name="context">The message context.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The value text of response.</returns>
    protected abstract Task<string> GetResponseAsync(string message, JsonObjectNode data, ResponsiveChatContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the response message by streaming (SSE) mode.
    /// </summary>
    /// <param name="message">The request message.</param>
    /// <param name="data">The data.</param>
    /// <param name="context">The message context.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The server-sent events.</returns>
    protected abstract IAsyncEnumerable<ServerSentEventInfo> StreamAsync(string message, JsonObjectNode data, ResponsiveChatContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Formats the message.
    /// </summary>
    /// <param name="answer">The original answer.</param>
    /// <param name="streaming">true if it is streaming; otherwise, false.</param>
    /// <returns>The message formatted.</returns>
    protected virtual string FormatMessage(string answer, bool streaming)
        => answer;

    /// <summary>
    /// Formats the message.
    /// </summary>
    /// <param name="exception">The exception.</param>
    /// <returns>The message formatted.</returns>
    protected virtual string FormatMessage(Exception exception)
        => exception?.Message ?? "Error!";

    /// <summary>
    /// Converts server-sent events to message.
    /// </summary>
    /// <param name="record">The record item of server-sent event.</param>
    /// <param name="context">The chat context.</param>
    protected virtual void ConvertSeverSentEventMessage(ServerSentEventInfo record, ResponsiveChatContext context)
    {
        var recJson = record?.TryGetJsonData();
        if (recJson == null || string.IsNullOrEmpty(record.EventName))
        {
            if (record?.TryGetValue("error", out var error) == true && !string.IsNullOrWhiteSpace(error))
            {
                var errorMessage = JsonObjectNode.TryParse(error)?.TryGetStringTrimmedValue("message", true);
                if (errorMessage != null) context.SetError(errorMessage);
            }

            return;
        }

        var piece = recJson.TryGetObjectListValue("choices")?.FirstOrDefault()?.TryGetObjectValue("delta")?.TryGetStringValue("content");
        if (string.IsNullOrWhiteSpace(piece)) piece = recJson.TryGetStringValue("msg");
        if (string.IsNullOrWhiteSpace(piece)) return;
        var eventName = record.EventName?.Trim().ToLowerInvariant();
        switch (eventName)
        {
            case "add":
            case "finish":
            case "message":
                context.AppendAnswer(piece);
                break;
            case "clear":
                context.SetAnswer(string.Empty);
                break;
            case "override":
            case "output":
                context.SetAnswer(piece);
                break;
        }
    }

    /// <summary>
    /// Gets the intent information from web server.
    /// </summary>
    /// <param name="message">The request message.</param>
    /// <param name="context">The message context.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The intent description data.</returns>
    protected virtual Task<ResponsiveChatMessageIntentInfo> GetIntentInfoAsync(string message, ResponsiveChatContext context, CancellationToken cancellationToken = default)
        => Task.FromResult<ResponsiveChatMessageIntentInfo>(null);

    /// <summary>
    /// Processes by the intent.
    /// </summary>
    /// <param name="intent">The intent description data.</param>
    /// <param name="context">The message context.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The result data by processing intent.</returns>
    protected virtual Task<JsonObjectNode> ProcessIntentAsync(JsonObjectNode intent, ResponsiveChatContext context, CancellationToken cancellationToken = default)
        => Task.FromResult<JsonObjectNode>(null);

    /// <summary>
    /// Occurs on the task is canceled.
    /// </summary>
    /// <param name="message">The request message.</param>
    /// <param name="context">The message context.</param>
    protected virtual void OnSendRequest(string message, ResponsiveChatContext context)
    {
    }

    /// <summary>
    /// Occurs on the task is canceled.
    /// </summary>
    /// <param name="ex">The exception thrown.</param>
    /// <param name="context">The message context.</param>
    protected virtual void OnTaskCancel(OperationCanceledException ex, ResponsiveChatContext context)
    {
    }

    /// <summary>
    /// Occurs on the intent info resolving failure.
    /// </summary>
    /// <param name="ex">The exception thrown.</param>
    /// <param name="context">The message context.</param>
    protected virtual void OnGetIntentInfoError(Exception ex, ResponsiveChatContext context)
    {
    }

    /// <summary>
    /// Occurs on the intent description data processing failure.
    /// </summary>
    /// <param name="ex">The exception thrown.</param>
    /// <param name="context">The message context.</param>
    protected virtual void OnProcessIntentError(Exception ex, ResponsiveChatContext context)
    {
    }

    /// <summary>
    /// Occurs on the response getting failure.
    /// </summary>
    /// <param name="ex">The exception thrown.</param>
    /// <param name="context">The message context.</param>
    protected virtual void OnGetResponseError(Exception ex, ResponsiveChatContext context)
    {
    }

    /// <summary>
    /// Occurs on the response is successful to get.
    /// </summary>
    /// <param name="message">The response message.</param>
    /// <param name="context">The message context.</param>
    protected virtual void OnGetResponse(string message, ResponsiveChatContext context)
    {
    }

    /// <summary>
    /// Deletes a message.
    /// </summary>
    /// <param name="context">The current chat message context.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The result of saving action.</returns>
    /// <exception cref="NotSupportedException">The deletion action is not supported.</exception>
    public async virtual Task<ExtendedChatMessageSendResult> DeleteAsync(ExtendedChatMessageContext context, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        throw new NotSupportedException("Cannot delete the message.", new UnauthorizedAccessException("No permission to delete the message."));
    }

    internal ResponsiveChatMessageResponse GetResponse(ResponsiveChatContext context, ExtendedChatMessageSendResult result, ResponsiveChatSendingLifecycle monitor, Action callback, CancellationToken cancellationToken)
        => new(GetResponseAsync(context, monitor, cancellationToken), context.Model.Question, result, callback);

    internal async Task<ResponsiveChatContext> CreateContextAsync(ExtendedChatMessageContext context)
    {
        if (string.IsNullOrWhiteSpace(context?.Message?.Message)) return null;
        await CreateTopicAsync(true);
        return new ResponsiveChatContext(this, context);
    }

    internal async Task<ExtendedChatMessageSendResult> SendMessageAsync(ResponsiveChatContext context, ResponsiveChatSendingLifecycle monitor, CancellationToken cancellationToken = default)
    {
        if (IsDisabled) throw new InvalidOperationException("The message is not available to send.");
        var m = context.Model;
        var q = context.QuestionMessage;
        if (string.IsNullOrWhiteSpace(q)) return new(ExtendedChatMessageSendResultStates.RequestError, "Request message is empty.");
        var t = context.Topic;
        AnswerStateChanged?.Invoke(this, m);
        Sending?.Invoke(this, m);
        monitor?.OnInit(context);
        OnSendRequest(q, context);
        context.UpdateState(ResponsiveChatMessageStates.Intent);
        monitor?.OnStateChange(context.State);
        AnswerStateChanged?.Invoke(this, m);
        try
        {
            var intent = await GetIntentInfoAsync(q, context, cancellationToken);
            var resp = new ExtendedChatMessageSendResult();
            if (intent != null)
            {
                context.Intent = intent.Info;
                if (intent.SkipIntent)
                {
                    if (string.IsNullOrWhiteSpace(intent.Message))
                    {
                        context.UpdateState(ResponsiveChatMessageStates.ResponseError, "No response.");
                        resp.Update(ExtendedChatMessageSendResultStates.ServerError, "No response.");
                    }
                    else
                    {
                        context.UpdateState(ResponsiveChatMessageStates.Done);
                        context.SetAnswer(intent.Message);
                    }

                    monitor?.OnStateChange(context.State);
                    AnswerStateChanged?.Invoke(this, m);
                    OnGetResponse(m.Answer.Message, context);
                    monitor?.OnReceive();
                    Sent?.Invoke(this, m);
                }
            }

            return resp;
        }
        catch (OperationCanceledException ex)
        {
            SetError(ResponsiveChatMessageStates.Abort, context, ex);
            monitor?.OnStateChange(context.State);
            AnswerStateChanged?.Invoke(this, m);
            OnTaskCancel(ex, context);
            monitor?.OnError(ex);
            SendCanceled?.Invoke(this, m);
            throw;
        }
        catch (Exception ex)
        {
            SetError(ResponsiveChatMessageStates.IntentError, context, ex);
            monitor?.OnStateChange(context.State);
            AnswerStateChanged?.Invoke(this, m);
            OnGetIntentInfoError(ex, context);
            monitor?.OnError(ex);
            SendFailed?.Invoke(this, m);
            if (ThrowOriginalOnSending) throw;
            throw new ResponsiveChatMessageException(context, ex);
        }
    }

    private async Task<ExtendedChatMessage> GetResponseAsync(ResponsiveChatContext context, ResponsiveChatSendingLifecycle monitor, CancellationToken cancellationToken = default)
    {
        var m = context.Model;
        var q = context.QuestionMessage;
        context.UpdateState(ResponsiveChatMessageStates.Processing);
        monitor?.OnStateChange(context.State);
        AnswerStateChanged?.Invoke(this, m);
        try
        {
            context.Data = await ProcessIntentAsync(context.Intent ?? new(), context, cancellationToken);
        }
        catch (OperationCanceledException ex)
        {
            SetError(ResponsiveChatMessageStates.Abort, context, ex);
            monitor?.OnStateChange(context.State);
            AnswerStateChanged?.Invoke(this, m);
            OnTaskCancel(ex, context);
            monitor?.OnError(ex);
            SendCanceled?.Invoke(this, m);
            throw;
        }
        catch (Exception ex)
        {
            SetError(ResponsiveChatMessageStates.ProcessFailure, context, ex);
            monitor?.OnStateChange(context.State);
            AnswerStateChanged?.Invoke(this, m);
            OnProcessIntentError(ex, context);
            monitor?.OnError(ex);
            SendFailed?.Invoke(this, m);
            if (ThrowOriginalOnSending) throw;
            throw new ResponsiveChatMessageException(context, ex);
        }

        context.SetDataSummary(GetDataSummary(context.Data));
        context.UpdateState(ResponsiveChatMessageStates.Waiting);
        monitor?.OnStateChange(context.State);
        AnswerStateChanged?.Invoke(this, m);
        if (IsStreaming)
        {
            var state = 0;
            try
            {
                monitor?.OnSend(true);
                var resp = StreamAsync(q, context.Data ?? new(), context, cancellationToken);
                context.SetStreamingMode();
                await foreach (var item in resp)
                {
                    if (state == 0)
                    {
                        state = 1;
                        context.UpdateState(ResponsiveChatMessageStates.Receiving);
                        monitor?.OnStateChange(context.State);
                        AnswerStateChanged?.Invoke(this, m);
                    }

                    cancellationToken.ThrowIfCancellationRequested();
                    ConvertSeverSentEventMessage(item, context);
                    monitor?.OnReceive();
                    if (context.IsError) break;
                    if (state == 1 && context.AnswerMessageLength > 0)
                    {
                        state = 2;
                        context.UpdateState(ResponsiveChatMessageStates.Receiving, "First text snippet is received.");
                        monitor?.OnStateChange(context.State);
                    }
                }

                if (state == 0)
                    context.UpdateState(ResponsiveChatMessageStates.ResponseError, "No response.");
                else
                    context.UpdateState(ResponsiveChatMessageStates.Done);
                monitor?.OnStateChange(context.State);
                AnswerStateChanged?.Invoke(this, m);
            }
            catch (OperationCanceledException ex)
            {
                SetError(ResponsiveChatMessageStates.Abort, context, ex);
                monitor?.OnStateChange(context.State);
                AnswerStateChanged?.Invoke(this, m);
                OnTaskCancel(ex, context);
                monitor?.OnError(ex);
                SendCanceled?.Invoke(this, m);
                throw;
            }
            catch (Exception ex)
            {
                SetError(ResponsiveChatMessageStates.ResponseError, context, ex);
                monitor?.OnStateChange(context.State);
                AnswerStateChanged?.Invoke(this, m);
                OnGetResponseError(ex, context);
                monitor?.OnError(ex);
                SendFailed?.Invoke(this, m);
                if (ThrowOriginalOnSending) throw;
                throw new ResponsiveChatMessageException(context, ex);
            }
        }
        else
        {
            try
            {
                monitor?.OnSend(false);
                var answer = await GetResponseAsync(q, context.Data ?? new(), context, cancellationToken);
                if (string.IsNullOrWhiteSpace(answer))
                {
                    context.UpdateState(ResponsiveChatMessageStates.ResponseError, "No response.");
                }
                else
                {
                    context.SetAnswer(answer);
                    context.UpdateState(ResponsiveChatMessageStates.Done);
                }

                monitor?.OnStateChange(context.State);
                AnswerStateChanged?.Invoke(this, m);
            }
            catch (OperationCanceledException ex)
            {
                SetError(ResponsiveChatMessageStates.Abort, context, ex);
                monitor?.OnStateChange(context.State);
                AnswerStateChanged?.Invoke(this, m);
                OnTaskCancel(ex, context);
                monitor?.OnError(ex);
                SendCanceled?.Invoke(this, m);
                throw;
            }
            catch (Exception ex)
            {
                SetError(ResponsiveChatMessageStates.ResponseError, context, ex);
                monitor?.OnStateChange(context.State);
                AnswerStateChanged?.Invoke(this, m);
                OnGetResponseError(ex, context);
                SendFailed?.Invoke(this, m);
                if (ThrowOriginalOnSending) throw;
                throw new ResponsiveChatMessageException(context, ex);
            }
        }

        context.SetDataRecord();
        if (context.IsSuccessful) context.Topic.Add(context.Model);
        OnGetResponse(m.Answer.Message, context);
        monitor?.OnReceive();
        Sent?.Invoke(this, m);
        return m.Answer;
    }

    private void SetError(ResponsiveChatMessageStates state, ResponsiveChatContext context, Exception ex)
    {
        context.SetError(FormatMessage(ex), ex is ResponsiveChatMessageException rcmEx ? (rcmEx.InnerException ?? ex) : ex);
        context.SetDataRecord();
        context.UpdateState(state, ex.Message);
    }
}
