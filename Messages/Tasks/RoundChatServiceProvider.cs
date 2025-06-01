using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Trivial.Data;
using Trivial.Net;
using Trivial.Reflection;
using Trivial.Text;

namespace Trivial.Tasks;

/// <summary>
/// The client provider of turn-based chat message web server.
/// </summary>
/// <param name="profile">The profile of the chat bot.</param>
public abstract class BaseRoundChatServiceProvider(Users.BaseUserItemInfo profile)
{
    /// <summary>
    /// Gets the bot info.
    /// </summary>
    public Users.BaseUserItemInfo Profile { get; } = profile;

    /// <summary>
    /// Gets a value indicating whether enables server-sent event mode.
    /// </summary>
    public bool IsStreaming { get; protected set; }

    /// <summary>
    /// Sends a message and gets the response.
    /// </summary>
    /// <param name="thread">The chat message thread.</param>
    /// <param name="message">The request message.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The turn-based message model.</returns>
    public RoundChatMessageModel Send(RoundChatMessageThread thread, string message, CancellationToken cancellationToken = default)
    {
        if (thread == null) return null;
        var context = new RoundChatMessageContext(thread, message);
        _ = SendAsync(thread, message, new InternalRoundChatMessageLifecycle(), cancellationToken);
        return context.Model;
    }

    /// <summary>
    /// Sends a message and gets the response.
    /// </summary>
    /// <param name="thread">The chat message thread.</param>
    /// <param name="message">The request message.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The turn-based message model.</returns>
    public Task<ExtendedChatMessage> SendAsync(RoundChatMessageThread thread, ExtendedChatMessage message, CancellationToken cancellationToken = default)
        => SendAsync(thread, message, null, new InternalRoundChatMessageLifecycle(), cancellationToken);

    /// <summary>
    /// Sends a message and gets the response.
    /// </summary>
    /// <param name="thread">The chat message thread.</param>
    /// <param name="message">The request message.</param>
    /// <param name="monitor">A flow lifecycle of turn-based chat message transferring.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The turn-based message model.</returns>
    public Task<ExtendedChatMessage> SendAsync(RoundChatMessageThread thread, ExtendedChatMessage message, RoundChatMessageLifecycle monitor, CancellationToken cancellationToken = default)
        => SendAsync(thread, message, null, new InternalRoundChatMessageLifecycle
        {
            Parent = monitor,
        }, cancellationToken);

    /// <summary>
    /// Sends a message and gets the response.
    /// </summary>
    /// <typeparam name="T">The type of the flow lifecycle context.</typeparam>
    /// <param name="thread">The chat message thread.</param>
    /// <param name="message">The request message.</param>
    /// <param name="monitor">A flow lifecycle of turn-based chat message transferring.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The turn-based message model.</returns>
    public Task<ExtendedChatMessage> SendAsync<T>(RoundChatMessageThread thread, ExtendedChatMessage message, RoundChatMessageLifecycle<T> monitor, CancellationToken cancellationToken = default)
        => SendAsync(thread, message, null, monitor, cancellationToken);

    /// <summary>
    /// Sends a message and gets the response.
    /// </summary>
    /// <param name="thread">The chat message thread.</param>
    /// <param name="message">The request message.</param>
    /// <param name="callback">A handler occurs on the turn-based message model is created.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The turn-based message model.</returns>
    public Task<ExtendedChatMessage> SendAsync(RoundChatMessageThread thread, ExtendedChatMessage message, Action<ExtendedChatMessage> callback, CancellationToken cancellationToken = default)
        => SendAsync(thread, message, callback, EmptyRoundChatMessageLifecycle.Instance, cancellationToken);

    /// <summary>
    /// Sends a message and gets the response.
    /// </summary>
    /// <typeparam name="T">The type of the flow lifecycle context.</typeparam>
    /// <param name="thread">The chat message thread.</param>
    /// <param name="message">The request message.</param>
    /// <param name="callback">A handler occurs on the turn-based message model is created.</param>
    /// <param name="monitor">A flow lifecycle of turn-based chat message transferring.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The turn-based message model.</returns>
    public async Task<ExtendedChatMessage> SendAsync<T>(RoundChatMessageThread thread, ExtendedChatMessage message, Action<ExtendedChatMessage> callback, RoundChatMessageLifecycle<T> monitor, CancellationToken cancellationToken = default)
    {
        if (thread == null || string.IsNullOrWhiteSpace(message?.Message)) return null;
        var context = new RoundChatMessageContext(thread, message.Message);
        var c = new ExtendedRoundChatMessageLifecycle<T>
        {
            Parent = monitor,
            Profile = Profile,
            Callback = callback
        };
        await SendAsync(context, c, cancellationToken);
        return c.Message;
    }

    /// <summary>
    /// Sends a message and gets the response.
    /// </summary>
    /// <param name="thread">The chat message thread.</param>
    /// <param name="message">The request message.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The turn-based message model.</returns>
    public Task<RoundChatMessageModel> SendAsync(RoundChatMessageThread thread, string message, CancellationToken cancellationToken = default)
        => SendAsync(thread, message, EmptyRoundChatMessageLifecycle.Instance, cancellationToken);

    /// <summary>
    /// Sends a message and gets the response.
    /// </summary>
    /// <param name="thread">The chat message thread.</param>
    /// <param name="message">The request message.</param>
    /// <param name="callback">A handler occurs on the turn-based message model is created.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The turn-based message model.</returns>
    public Task<RoundChatMessageModel> SendAsync(RoundChatMessageThread thread, string message, Action<RoundChatMessageModel> callback, CancellationToken cancellationToken = default)
    {
        if (thread == null || string.IsNullOrWhiteSpace(message)) return Task.FromResult<RoundChatMessageModel>(null);
        var context = new RoundChatMessageContext(thread, message);
        callback?.Invoke(context.Model);
        return SendAsync(context, EmptyRoundChatMessageLifecycle.Instance, cancellationToken);
    }

    /// <summary>
    /// Sends a message and gets the response.
    /// </summary>
    /// <param name="thread">The chat message thread.</param>
    /// <param name="message">The request message.</param>
    /// <param name="monitor">A flow lifecycle of turn-based chat message transferring.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The turn-based message model.</returns>
    public Task<RoundChatMessageModel> SendAsync(RoundChatMessageThread thread, string message, RoundChatMessageLifecycle monitor, CancellationToken cancellationToken = default)
        => SendAsync(thread, message, new InternalRoundChatMessageLifecycle {
            Parent = monitor
        }, cancellationToken);

    /// <summary>
    /// Sends a message and gets the response.
    /// </summary>
    /// <typeparam name="T">The type of the flow lifecycle context.</typeparam>
    /// <param name="thread">The chat message thread.</param>
    /// <param name="message">The request message.</param>
    /// <param name="monitor">A flow lifecycle of turn-based chat message transferring.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The turn-based message model.</returns>
    public Task<RoundChatMessageModel> SendAsync<T>(RoundChatMessageThread thread, string message, RoundChatMessageLifecycle<T> monitor, CancellationToken cancellationToken = default)
    {
        if (thread == null || string.IsNullOrWhiteSpace(message)) return Task.FromResult<RoundChatMessageModel>(null);
        var context = new RoundChatMessageContext(thread, message);
        return SendAsync(context, monitor, cancellationToken);
    }

    /// <summary>
    /// Sends a message and gets the response.
    /// </summary>
    /// <typeparam name="T">The type of the flow lifecycle context.</typeparam>
    /// <param name="context">The message context.</param>
    /// <param name="monitor">A flow lifecycle of turn-based chat message transferring.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The turn-based message model.</returns>
    private async Task<RoundChatMessageModel> SendAsync<T>(RoundChatMessageContext context, RoundChatMessageLifecycle<T> monitor, CancellationToken cancellationToken = default)
    {
        var c = monitor is not null ? (monitor.Init(context.Thread) ?? default) : default;
        if (!context.CanSend()) throw new InvalidOperationException("The message is not available to send.");
        var m = context.Model;
        var t = context.Thread;
        monitor?.OnInit(m, t, c);
        m.UpdateState(RoundChatMessageStates.Intent);
        monitor?.OnStateChange(m.State, m, t, c);
        try
        {
            m.Intent = await GetIntentDataAsync(m.Question, context, cancellationToken);
        }
        catch (OperationCanceledException ex)
        {
            m.UpdateState(RoundChatMessageStates.Abort, ex.Message);
            monitor?.OnStateChange(m.State, m, t, c);
            OnTaskCancel(ex, context);
            monitor?.OnError(ex, m, t, c);
            throw;
        }
        catch (Exception ex)
        {
            m.UpdateState(RoundChatMessageStates.IntentError, ex.Message);
            monitor?.OnStateChange(m.State, m, t, c);
            OnGetIntentDataError(ex, context);
            monitor?.OnError(ex, m, t, c);
            throw;
        }

        m.UpdateState(RoundChatMessageStates.Processing);
        monitor?.OnStateChange(m.State, m, t, c);
        try
        {
            m.Data = await ProcessIntentAsync(context.Model.Intent ?? new(), context, cancellationToken);
        }
        catch (OperationCanceledException ex)
        {
            m.UpdateState(RoundChatMessageStates.Abort, ex.Message);
            monitor?.OnStateChange(m.State, m, t, c);
            OnTaskCancel(ex, context);
            monitor?.OnError(ex, m, t, c);
            throw;
        }
        catch (Exception ex)
        {
            m.UpdateState(RoundChatMessageStates.ProcessFailure, ex.Message);
            monitor?.OnStateChange(m.State, m, t, c);
            OnProcessIntentError(ex, context);
            monitor?.OnError(ex, m, t, c);
            throw;
        }

        m.UpdateState(RoundChatMessageStates.Waiting);
        monitor?.OnStateChange(m.State, m, t, c);
        if (IsStreaming)
        {
            var state = 0;
            RoundChatMessageWriter writer;
            try
            {
                writer = CreateResponseWriter(context) ?? new();
            }
            catch (Exception ex)
            {
                m.UpdateState(RoundChatMessageStates.ProcessFailure, ex.Message);
                monitor?.OnStateChange(m.State, m, t, c);
                OnGetResponseError(ex, context);
                monitor?.OnError(ex, m, t, c);
                throw;
            }

            try
            {
                monitor?.OnSend(m, true, t, c);
                var resp = StreamAsync(m.Question, context.Model.Data ?? new(), context, cancellationToken);
                writer.Model = m;
                await foreach (var item in resp)
                {
                    if (state == 0)
                    {
                        state = 1;
                        m.UpdateState(RoundChatMessageStates.Receiving);
                        monitor?.OnStateChange(m.State, m, t, c);
                    }

                    cancellationToken.ThrowIfCancellationRequested();
                    ConvertSeverSentEventMessage(writer, item, context);
                    if (writer.IsEnd) break;
                    if (state == 1 && writer.Length > 0)
                    {
                        state = 2;
                        m.UpdateState(RoundChatMessageStates.Receiving, "First text snippet is received.");
                        monitor?.OnStateChange(m.State, m, t, c);
                    }
                }

                if (state == 0)
                    m.UpdateState(RoundChatMessageStates.ResponseError, "No response.");
                else
                    m.UpdateState(RoundChatMessageStates.Done);
                monitor?.OnStateChange(m.State, m, t, c);
            }
            catch (OperationCanceledException ex)
            {
                m.UpdateState(RoundChatMessageStates.Abort, ex.Message);
                monitor?.OnStateChange(m.State, m, t, c);
                OnTaskCancel(ex, context);
                monitor?.OnError(ex, m, t, c);
                throw;
            }
            catch (Exception ex)
            {
                m.UpdateState(RoundChatMessageStates.ResponseError, ex.Message);
                monitor?.OnStateChange(m.State, m, t, c);
                OnGetResponseError(ex, context);
                monitor?.OnError(ex, m, t, c);
                throw;
            }
            finally
            {
                writer.Model = null;
                writer.IsEnd = true;
            }
        }
        else
        {
            try
            {
                monitor?.OnSend(m, false, t, c);
                m.Answer = await GetResponseAsync(m.Question, context.Model.Data ?? new(), context, cancellationToken);
                if (string.IsNullOrWhiteSpace(m.Answer))
                    m.UpdateState(RoundChatMessageStates.ResponseError, "No response.");
                else
                    m.UpdateState(RoundChatMessageStates.Done);
                monitor?.OnStateChange(m.State, m, t, c);
            }
            catch (OperationCanceledException ex)
            {
                m.UpdateState(RoundChatMessageStates.Abort, ex.Message);
                monitor?.OnStateChange(m.State, m, t, c);
                OnTaskCancel(ex, context);
                monitor?.OnError(ex, m, t, c);
                throw;
            }
            catch (Exception ex)
            {
                m.UpdateState(RoundChatMessageStates.ResponseError, ex.Message);
                monitor?.OnStateChange(m.State, m, t, c);
                OnGetResponseError(ex, context);
                throw;
            }
        }

        OnGetResponse(m.Answer, context);
        monitor?.OnReceive(m, t, c);
        return m;
    }

    /// <summary>
    /// Gets the response message.
    /// </summary>
    /// <param name="message">The request message.</param>
    /// <param name="data">The data.</param>
    /// <param name="context">The message context.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The value text of response.</returns>
    protected abstract Task<string> GetResponseAsync(string message, JsonObjectNode data, RoundChatMessageContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the response message by streaming (SSE) mode.
    /// </summary>
    /// <param name="message">The request message.</param>
    /// <param name="data">The data.</param>
    /// <param name="context">The message context.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The server-sent events.</returns>
    protected abstract IAsyncEnumerable<ServerSentEventInfo> StreamAsync(string message, JsonObjectNode data, RoundChatMessageContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates an instance of response writer.
    /// </summary>
    /// <param name="context">The turn-based chat message context.</param>
    /// <returns>An instance of response writer.</returns>
    protected virtual RoundChatMessageWriter CreateResponseWriter(RoundChatMessageContext context)
        => null;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="record"></param>
    /// <param name="context"></param>
    protected virtual void ConvertSeverSentEventMessage(RoundChatMessageWriter writer, ServerSentEventInfo record, RoundChatMessageContext context)
    {
        var recJson = record?.TryGetJsonData();
        if (recJson == null || string.IsNullOrEmpty(record.EventName))
        {
            if (record?.TryGetValue("error", out var error) == true && !string.IsNullOrWhiteSpace(error))
            {
                var errorMessage = JsonObjectNode.TryParse(error)?.TryGetStringTrimmedValue("message", true);
                if (errorMessage != null) writer.SetError(errorMessage);
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
                writer.Append(piece);
                break;
            case "clear":
                writer.Clear();
                break;
            case "override":
            case "output":
                writer.Set(piece);
                break;
        }
    }

    /// <summary>
    /// Gets the intent data from web server.
    /// </summary>
    /// <param name="message">The request message.</param>
    /// <param name="context">The message context.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The intent description data.</returns>
    protected virtual Task<JsonObjectNode> GetIntentDataAsync(string message, RoundChatMessageContext context, CancellationToken cancellationToken = default)
        => Task.FromResult<JsonObjectNode>(null);

    /// <summary>
    /// Processes by the intent.
    /// </summary>
    /// <param name="intent">The intent description data.</param>
    /// <param name="context">The message context.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns></returns>
    protected virtual Task<JsonObjectNode> ProcessIntentAsync(JsonObjectNode intent, RoundChatMessageContext context, CancellationToken cancellationToken = default)
        => Task.FromResult<JsonObjectNode>(null);

    /// <summary>
    /// Occurs on the task is canceled.
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="context">The message context.</param>
    protected virtual void OnTaskCancel(OperationCanceledException ex, RoundChatMessageContext context)
    {
    }

    /// <summary>
    /// Occurs on the intent description data resolving failure.
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="context">The message context.</param>
    protected virtual void OnGetIntentDataError(Exception ex, RoundChatMessageContext context)
    {
    }

    /// <summary>
    /// Occurs on the intent description data processing failure.
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="context">The message context.</param>
    protected virtual void OnProcessIntentError(Exception ex, RoundChatMessageContext context)
    {
    }

    /// <summary>
    /// Occurs on the response getting failure.
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="context">The message context.</param>
    protected virtual void OnGetResponseError(Exception ex, RoundChatMessageContext context)
    {
    }

    /// <summary>
    /// Occurs on the response is successful to get.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="context">The message context.</param>
    protected virtual void OnGetResponse(string message, RoundChatMessageContext context)
    {
    }

    /// <summary>
    /// Creates a new chat thread.
    /// </summary>
    /// <returns>A new chat thread.</returns>
    protected virtual RoundChatMessageThread CreateThread()
        => new(null);
}
