using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
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
public abstract class BaseRoundChatServiceProvider(Users.BaseUserItemInfo profile) : IExtendedChatTopic, INotifyPropertyChanged
{
    private bool isDisabled;

    /// <summary>
    /// Adds or removes the event handler occurred on the answer state is changed.
    /// </summary>
    public event DataEventHandler<RoundChatMessageModel> AnswerStateChanged;

    /// <summary>
    /// Adds or removes the event handler occurred on a message is waiting to send.
    /// </summary>
    public event DataEventHandler<RoundChatMessageModel> Sending;

    /// <summary>
    /// Adds or removes the event handler occurred on a message has already sent and got the response.
    /// </summary>
    public event DataEventHandler<RoundChatMessageModel> Sent;

    /// <summary>
    /// Adds or removes the event handler occurred on a message is failed to get response.
    /// </summary>
    public event DataEventHandler<RoundChatMessageModel> SendFailed;

    /// <summary>
    /// Adds or removes the event handler occurred on a message is canceled to get response.
    /// </summary>
    public event DataEventHandler<RoundChatMessageModel> SendCanceled;

    /// <summary>
    /// Gets the bot info.
    /// </summary>
    public Users.BaseUserItemInfo Profile { get; } = profile;

    /// <summary>
    /// Gets a value indicating whether enables server-sent event mode.
    /// </summary>
    public bool IsStreaming { get; protected set; }

    /// <summary>
    /// Gets the history of chat messages.
    /// </summary>
    public ObservableCollection<ExtendedChatMessage> History { get; } = new();

    /// <summary>
    /// Adds or removes the event handler raised on property changed.
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// Gets the current chat message topic.
    /// </summary>
    public RoundChatMessageTopic CurrentTopic { get; private set; } = null;

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

    string IExtendedChatTopic.Nickname => Profile?.Nickname;
    Uri IExtendedChatTopic.AvatarUri => Profile?.AvatarUri;
    bool IExtendedChatTopic.IsMultipleParticipatorsMode => false;
    bool IExtendedChatTopic.IsReadOnly => IsDisabled;
    bool IExtendedChatTopic.IsRoundMode => true;

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
    /// Sends a message and gets the response.
    /// </summary>
    /// <param name="message">The request message.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The turn-based message model.</returns>
    public RoundChatMessageModel Send(string message, CancellationToken cancellationToken = default)
    {
        var topic = CurrentTopic;
        if (topic == null) return null;
        var context = new RoundChatMessageContext(topic, message);
        _ = SendAsync(message, new InternalRoundChatMessageLifecycle(), cancellationToken);
        return context.Model;
    }

    /// <summary>
    /// Sends a message and gets the response.
    /// </summary>
    /// <param name="message">The request message.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The turn-based message model.</returns>
    public Task<ExtendedChatMessage> SendAsync(ExtendedChatMessage message, CancellationToken cancellationToken = default)
        => SendAsync(message, null, new InternalRoundChatMessageLifecycle(), cancellationToken);

    /// <summary>
    /// Sends a message and gets the response.
    /// </summary>
    /// <param name="message">The request message.</param>
    /// <param name="monitor">A flow lifecycle of turn-based chat message transferring.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The turn-based message model.</returns>
    public Task<ExtendedChatMessage> SendAsync(ExtendedChatMessage message, RoundChatMessageLifecycle monitor, CancellationToken cancellationToken = default)
        => SendAsync(message, null, new InternalRoundChatMessageLifecycle
        {
            Parent = monitor,
        }, cancellationToken);

    /// <summary>
    /// Sends a message and gets the response.
    /// </summary>
    /// <typeparam name="T">The type of the flow lifecycle context.</typeparam>
    /// <param name="message">The request message.</param>
    /// <param name="monitor">A flow lifecycle of turn-based chat message transferring.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The turn-based message model.</returns>
    public Task<ExtendedChatMessage> SendAsync<T>(ExtendedChatMessage message, RoundChatMessageLifecycle<T> monitor, CancellationToken cancellationToken = default)
        => SendAsync(message, null, monitor, cancellationToken);

    /// <summary>
    /// Sends a message and gets the response.
    /// </summary>
    /// <param name="message">The request message.</param>
    /// <param name="callback">A handler occurs on the turn-based message model is created.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The turn-based message model.</returns>
    public Task<ExtendedChatMessage> SendAsync(ExtendedChatMessage message, Action<ExtendedChatMessage> callback, CancellationToken cancellationToken = default)
        => SendAsync(message, callback, EmptyRoundChatMessageLifecycle.Instance, cancellationToken);

    /// <summary>
    /// Sends a message and gets the response.
    /// </summary>
    /// <typeparam name="T">The type of the flow lifecycle context.</typeparam>
    /// <param name="message">The request message.</param>
    /// <param name="callback">A handler occurs on the turn-based message model is created.</param>
    /// <param name="monitor">A flow lifecycle of turn-based chat message transferring.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The turn-based message model.</returns>
    public async Task<ExtendedChatMessage> SendAsync<T>(ExtendedChatMessage message, Action<ExtendedChatMessage> callback, RoundChatMessageLifecycle<T> monitor, CancellationToken cancellationToken = default)
    {
        await CreateTopicAsync(true);
        var topic = CurrentTopic;
        if (topic == null || string.IsNullOrWhiteSpace(message?.Message)) return null;
        var context = new RoundChatMessageContext(topic, message.Message);
        History.Add(message);
        var c = new ExtendedRoundChatMessageLifecycle<T>
        {
            Parent = monitor,
            Profile = Profile,
            Callback = callback,
            History = History,
        };
        var resp = await SendAsync(context, c, cancellationToken);
        if (c.Message == null) return null;
        var round = new JsonObjectNode
        {
            { "interact", "turn-based" },
            { "topic", topic.Id },
            { "provider", "round-chat-service" },
            { "kind", "answer" },
            { "reply", message.Id },
        };
        c.Message.Info.SetValue("context", round);
        var intent = new JsonObjectNode();
        intent.SetValue("request", resp.Intent);
        intent.SetValue("response", GetDataSummary(resp.Data));
        c.Message.Info.SetValue("intent", intent);
        var records = resp.Records;
        if (records != null)
        {
            var arr = new List<JsonObjectNode>();
            foreach (var rec in records)
            {
                if (rec != null) arr.Add(rec.ToJson());
            }

            intent.SetValue("records", arr);
        }

        return c.Message;
    }

    /// <summary>
    /// Sends a message and gets the response.
    /// </summary>
    /// <param name="message">The request message.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The turn-based message model.</returns>
    public Task<RoundChatMessageModel> SendAsync(string message, CancellationToken cancellationToken = default)
        => SendAsync(message, EmptyRoundChatMessageLifecycle.Instance, cancellationToken);

    /// <summary>
    /// Sends a message and gets the response.
    /// </summary>
    /// <param name="message">The request message.</param>
    /// <param name="callback">A handler occurs on the turn-based message model is created.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The turn-based message model.</returns>
    public async Task<RoundChatMessageModel> SendAsync(string message, Action<RoundChatMessageModel> callback, CancellationToken cancellationToken = default)
    {
        await CreateTopicAsync(true);
        var topic = CurrentTopic;
        if (topic == null || string.IsNullOrWhiteSpace(message)) return null;
        var context = new RoundChatMessageContext(topic, message);
        callback?.Invoke(context.Model);
        return await SendAsync(context, EmptyRoundChatMessageLifecycle.Instance, cancellationToken);
    }

    /// <summary>
    /// Sends a message and gets the response.
    /// </summary>
    /// <param name="message">The request message.</param>
    /// <param name="monitor">A flow lifecycle of turn-based chat message transferring.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The turn-based message model.</returns>
    public Task<RoundChatMessageModel> SendAsync(string message, RoundChatMessageLifecycle monitor, CancellationToken cancellationToken = default)
        => SendAsync(message, new InternalRoundChatMessageLifecycle {
            Parent = monitor
        }, cancellationToken);

    /// <summary>
    /// Sends a message and gets the response.
    /// </summary>
    /// <typeparam name="T">The type of the flow lifecycle context.</typeparam>
    /// <param name="message">The request message.</param>
    /// <param name="monitor">A flow lifecycle of turn-based chat message transferring.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The turn-based message model.</returns>
    public async Task<RoundChatMessageModel> SendAsync<T>(string message, RoundChatMessageLifecycle<T> monitor, CancellationToken cancellationToken = default)
    {
        await CreateTopicAsync(true);
        var topic = CurrentTopic;
        if (topic == null || string.IsNullOrWhiteSpace(message)) return null;
        var context = new RoundChatMessageContext(topic, message);
        return await SendAsync(context, monitor, cancellationToken);
    }

    /// <summary>
    /// Sends a message and gets the response.
    /// </summary>
    /// <typeparam name="T">The type of the flow lifecycle context.</typeparam>
    /// <param name="context">The message context.</param>
    /// <param name="monitor">A flow lifecycle of turn-based chat message transferring.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The turn-based message model.</returns>
    /// <exception cref="InvalidOperationException">The message is not available; or, the processing is invalid.</exception>
    /// <exception cref="RoundChatMessageException">The message is failed to process.</exception>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>"
    private async Task<RoundChatMessageModel> SendAsync<T>(RoundChatMessageContext context, RoundChatMessageLifecycle<T> monitor, CancellationToken cancellationToken = default)
    {
        var c = monitor is not null ? (monitor.Init(context.Topic) ?? default) : default;
        if (!context.CanSend()) throw new InvalidOperationException("The message is not available to send.");
        var m = context.Model;
        var t = context.Topic;
        AnswerStateChanged?.Invoke(this, m);
        Sending?.Invoke(this, m);
        monitor?.OnInit(m, t, c);
        OnSendRequest(m.Question, context);
        m.UpdateState(RoundChatMessageStates.Intent);
        monitor?.OnStateChange(m.State, m, t, c);
        AnswerStateChanged?.Invoke(this, m);
        try
        {
            var intent = await GetIntentInfoAsync(m.Question, context, cancellationToken);
            if (intent != null)
            {
                m.Intent = intent.Info;
                m.Answer = intent.Message;
                if (intent.SkipIntent)
                {
                    if (string.IsNullOrWhiteSpace(m.Answer))
                        m.UpdateState(RoundChatMessageStates.ResponseError, "No response.");
                    else
                        m.UpdateState(RoundChatMessageStates.Done);
                    monitor?.OnStateChange(m.State, m, t, c);
                    AnswerStateChanged?.Invoke(this, m);
                    OnGetResponse(m.Answer, context);
                    monitor?.OnReceive(m, t, c);
                    Sent?.Invoke(this, m);
                    return m;
                }
            }
        }
        catch (OperationCanceledException ex)
        {
            m.UpdateState(RoundChatMessageStates.Abort, ex.Message);
            monitor?.OnStateChange(m.State, m, t, c);
            AnswerStateChanged?.Invoke(this, m);
            OnTaskCancel(ex, context);
            monitor?.OnError(ex, m, t, c);
            SendCanceled?.Invoke(this, m);
            throw;
        }
        catch (Exception ex)
        {
            m.UpdateState(RoundChatMessageStates.IntentError, ex.Message);
            monitor?.OnStateChange(m.State, m, t, c);
            AnswerStateChanged?.Invoke(this, m);
            OnGetIntentInfoError(ex, context);
            monitor?.OnError(ex, m, t, c);
            SendFailed?.Invoke(this, m);
            if (ThrowOriginalOnSending) throw;
            throw new RoundChatMessageException(m, ex);
        }

        m.UpdateState(RoundChatMessageStates.Processing);
        monitor?.OnStateChange(m.State, m, t, c);
        AnswerStateChanged?.Invoke(this, m);
        try
        {
            m.Data = await ProcessIntentAsync(context.Model.Intent ?? new(), context, cancellationToken);
        }
        catch (OperationCanceledException ex)
        {
            m.UpdateState(RoundChatMessageStates.Abort, ex.Message);
            monitor?.OnStateChange(m.State, m, t, c);
            AnswerStateChanged?.Invoke(this, m);
            OnTaskCancel(ex, context);
            monitor?.OnError(ex, m, t, c);
            SendCanceled?.Invoke(this, m);
            throw;
        }
        catch (Exception ex)
        {
            m.UpdateState(RoundChatMessageStates.ProcessFailure, ex.Message);
            monitor?.OnStateChange(m.State, m, t, c);
            AnswerStateChanged?.Invoke(this, m);
            OnProcessIntentError(ex, context);
            monitor?.OnError(ex, m, t, c);
            SendFailed?.Invoke(this, m);
            if (ThrowOriginalOnSending) throw;
            throw new RoundChatMessageException(m, ex);
        }

        m.UpdateState(RoundChatMessageStates.Waiting);
        monitor?.OnStateChange(m.State, m, t, c);
        AnswerStateChanged?.Invoke(this, m);
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
                AnswerStateChanged?.Invoke(this, m);
                OnGetResponseError(ex, context);
                monitor?.OnError(ex, m, t, c);
                SendFailed?.Invoke(this, m);
                if (ThrowOriginalOnSending) throw;
                throw new RoundChatMessageException(m, ex);
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
                        AnswerStateChanged?.Invoke(this, m);
                    }

                    cancellationToken.ThrowIfCancellationRequested();
                    ConvertSeverSentEventMessage(writer, item, context);
                    monitor?.OnReceive(m, t, c);
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
                AnswerStateChanged?.Invoke(this, m);
            }
            catch (OperationCanceledException ex)
            {
                m.UpdateState(RoundChatMessageStates.Abort, ex.Message);
                monitor?.OnStateChange(m.State, m, t, c);
                AnswerStateChanged?.Invoke(this, m);
                OnTaskCancel(ex, context);
                monitor?.OnError(ex, m, t, c);
                SendCanceled?.Invoke(this, m);
                throw;
            }
            catch (Exception ex)
            {
                m.UpdateState(RoundChatMessageStates.ResponseError, ex.Message);
                monitor?.OnStateChange(m.State, m, t, c);
                AnswerStateChanged?.Invoke(this, m);
                OnGetResponseError(ex, context);
                monitor?.OnError(ex, m, t, c);
                SendFailed?.Invoke(this, m);
                if (ThrowOriginalOnSending) throw;
                throw new RoundChatMessageException(m, ex);
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
                AnswerStateChanged?.Invoke(this, m);
            }
            catch (OperationCanceledException ex)
            {
                m.UpdateState(RoundChatMessageStates.Abort, ex.Message);
                monitor?.OnStateChange(m.State, m, t, c);
                AnswerStateChanged?.Invoke(this, m);
                OnTaskCancel(ex, context);
                monitor?.OnError(ex, m, t, c);
                SendCanceled?.Invoke(this, m);
                throw;
            }
            catch (Exception ex)
            {
                m.UpdateState(RoundChatMessageStates.ResponseError, ex.Message);
                monitor?.OnStateChange(m.State, m, t, c);
                AnswerStateChanged?.Invoke(this, m);
                OnGetResponseError(ex, context);
                SendFailed?.Invoke(this, m);
                if (ThrowOriginalOnSending) throw;
                throw new RoundChatMessageException(m, ex);
            }
        }

        OnGetResponse(m.Answer, context);
        monitor?.OnReceive(m, t, c);
        Sent?.Invoke(this, m);
        return m;
    }

    /// <summary>
    /// Creates a new round chat message topic.
    /// </summary>
    /// <returns>The round chat message topic.</returns>
    protected virtual Task<RoundChatMessageTopic> CreateNewTopicAsync()
        => Task.FromResult(new RoundChatMessageTopic(Guid.NewGuid().ToString())
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
    /// Gets the intent information from web server.
    /// </summary>
    /// <param name="message">The request message.</param>
    /// <param name="context">The message context.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The intent description data.</returns>
    protected virtual Task<RoundChatMessageIntentInfo> GetIntentInfoAsync(string message, RoundChatMessageContext context, CancellationToken cancellationToken = default)
        => Task.FromResult<RoundChatMessageIntentInfo>(null);

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
    /// <param name="message">The request message.</param>
    /// <param name="context">The message context.</param>
    protected virtual void OnSendRequest(string message, RoundChatMessageContext context)
    {
    }

    /// <summary>
    /// Occurs on the task is canceled.
    /// </summary>
    /// <param name="ex">The exception thrown.</param>
    /// <param name="context">The message context.</param>
    protected virtual void OnTaskCancel(OperationCanceledException ex, RoundChatMessageContext context)
    {
    }

    /// <summary>
    /// Occurs on the intent info resolving failure.
    /// </summary>
    /// <param name="ex">The exception thrown.</param>
    /// <param name="context">The message context.</param>
    protected virtual void OnGetIntentInfoError(Exception ex, RoundChatMessageContext context)
    {
    }

    /// <summary>
    /// Occurs on the intent description data processing failure.
    /// </summary>
    /// <param name="ex">The exception thrown.</param>
    /// <param name="context">The message context.</param>
    protected virtual void OnProcessIntentError(Exception ex, RoundChatMessageContext context)
    {
    }

    /// <summary>
    /// Occurs on the response getting failure.
    /// </summary>
    /// <param name="ex">The exception thrown.</param>
    /// <param name="context">The message context.</param>
    protected virtual void OnGetResponseError(Exception ex, RoundChatMessageContext context)
    {
    }

    /// <summary>
    /// Occurs on the response is successful to get.
    /// </summary>
    /// <param name="message">The response message.</param>
    /// <param name="context">The message context.</param>
    protected virtual void OnGetResponse(string message, RoundChatMessageContext context)
    {
    }
}
