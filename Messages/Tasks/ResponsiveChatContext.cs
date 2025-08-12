using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Trivial.Data;
using Trivial.Reflection;
using Trivial.Text;
using Trivial.Users;

namespace Trivial.Tasks;

/// <summary>
/// The context of handling the responsive chat message by provider.
/// </summary>
[Guid("DD9D50FF-531C-4A15-822B-E72648190800")]
public sealed class ResponsiveChatContext
{
    private readonly ExtendedChatMessage notification;
    private readonly JsonObjectNode questionContext;
    private readonly JsonObjectNode intentContainer;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResponsiveChatContext"/> class.
    /// </summary>
    /// <param name="conversation">The chat conversation.</param>
    /// <param name="context">The chat message context.</param>
    internal ResponsiveChatContext(BaseResponsiveChatConversation conversation, ExtendedChatMessageContext context)
    {
        Topic = conversation.CurrentTopic;
        var topicId = Topic?.Id;
        Conversation = context?.Conversation;
        Parameter = context?.Parameter;
        ChangingMethod = context?.ChangingMethod ?? ChangeMethods.Unknown;
        intentContainer = new();
        var q = context?.Message ?? new(Guid.NewGuid(), null as string, null, new ExtendedChatMessageContent());
        var answer = new ExtendedChatMessage(Guid.NewGuid(), q.OwnerId, conversation.Profile, new ExtendedChatMessageContent(null, ExtendedChatMessageFormats.Markdown, new()
        {
            { "context", new JsonObjectNode
            {
                { "interact", "turn-based" },
                { "topic", topicId },
                { "provider", "round-chat-service" },
                { "kind", "answer" },
                { "reply", q.Id },
            } }
        }));
        Model = new(q, answer);
        notification = new(Guid.NewGuid(), q.OwnerId, conversation.NotificationProfile ?? conversation.Profile, new ExtendedChatMessageContent(null, ExtendedChatMessageFormats.Markdown, new()
        {
            { "context", new JsonObjectNode
            {
                { "interact", "turn-based" },
                { "topic", topicId },
                { "provider", "round-chat-service" },
                { "kind", "error" },
                { "reply", q.Id },
            } }
        }));
        questionContext = new()
        {
            { "interact", "turn-based" },
            { "topic", topicId },
            { "provider", "round-chat-service" },
            { "kind", "question" },
            { "answer", answer.Id },
        };
        q.Info.SetValue("context", questionContext);
        intentContainer.SetValue("request", new JsonObjectNode());
        answer.Info.SetValue("intent", intentContainer);
        notification.Info.SetValue("intent", intentContainer);
    }

    /// <summary>
    /// Adds or removes the event handler occurred on the progress state is updated.
    /// </summary>
    public event DataEventHandler<ResponsiveChatMessageStates> StateUpdated;

    /// <summary>
    /// Gets the model.
    /// </summary>
    internal ResponsiveChatMessageModel Model { get; }

    /// <summary>
    /// Gets the chat conversation.
    /// </summary>
    internal ExtendedChatConversation Conversation { get; }

    /// <summary>
    /// Gets the topic.
    /// </summary>
    public ResponsiveChatMessageTopic Topic { get; }

    /// <summary>
    /// Gets the changing method.
    /// </summary>
    public ChangeMethods ChangingMethod { get; }

    /// <summary>
    /// Gets the additional parameter.
    /// </summary>
    public object Parameter { get; }

    /// <summary>
    /// Gets the intent request info.
    /// </summary>
    public JsonObjectNode Intent
    {
        get => intentContainer.TryGetObjectValue("request");
        set => intentContainer.SetValue("request", value ?? new());
    }

    /// <summary>
    /// Gets the result data processed by intent.
    /// </summary>
    public JsonObjectNode Data { get; internal set; }

    /// <summary>
    /// Gets the question message content.
    /// </summary>
    public string QuestionMessage => Model.Question.Message;

    /// <summary>
    /// Gets the answer message content.
    /// </summary>
    public string AnswerMessage => Model.Answer.Message;

    /// <summary>
    /// Gets the error message.
    /// </summary>
    public string ErrorMessage => notification.Message;

    /// <summary>
    /// Gets the length of answer message content.
    /// </summary>
    public int AnswerMessageLength => Model.Answer.Message?.Length ?? 0;

    /// <summary>
    /// Gets the sender.
    /// </summary>
    public BaseUserItemInfo Sender => Model.Question.Sender;

    /// <summary>
    /// Gets the profile of the chat service.
    /// </summary>
    public BaseUserItemInfo Profile => Model.Answer.Sender;

    /// <summary>
    /// Gets the notification profile of the chat service.
    /// </summary>
    public BaseUserItemInfo NotificationProfile => notification.Sender;

    /// <summary>
    /// Gets the progress state of this message.
    /// </summary>
    public ResponsiveChatMessageStates State { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the message is pending to get final result.
    /// </summary>
    public bool IsWorking => State switch
    {
        ResponsiveChatMessageStates.SendingIntent or ResponsiveChatMessageStates.Intent or ResponsiveChatMessageStates.Processing or ResponsiveChatMessageStates.SendingMessage or ResponsiveChatMessageStates.Waiting or ResponsiveChatMessageStates.Receiving => true,
        _ => false
    };

    /// <summary>
    /// Gets a value indicating whether the message is received succeeded.
    /// </summary>
    public bool IsSuccessful => State == ResponsiveChatMessageStates.Done;

    /// <summary>
    /// Gets a value indicating whether there is an error during getting the result.
    /// </summary>
    public bool IsError => State switch
    {
        ResponsiveChatMessageStates.Reject or ResponsiveChatMessageStates.Abort or ResponsiveChatMessageStates.IntentError or ResponsiveChatMessageStates.ProcessFailure or ResponsiveChatMessageStates.ResponseError or ResponsiveChatMessageStates.OtherError => true,
        _ => false
    };

    /// <summary>
    /// Gets a value indicating whether the message is before sending.
    /// </summary>
    public bool IsPreparing => State switch
    {
        ResponsiveChatMessageStates.Initialized or ResponsiveChatMessageStates.SendingIntent or ResponsiveChatMessageStates.Intent or ResponsiveChatMessageStates.Processing => true,
        _ => false
    };

    /// <summary>
    /// Gets a value indicating whether it is intent data resolving or action processing.
    /// </summary>
    public bool IsInIntentProgress => State switch
    {
        ResponsiveChatMessageStates.SendingIntent or ResponsiveChatMessageStates.Intent or ResponsiveChatMessageStates.Processing => true,
        _ => false
    };

    /// <summary>
    /// Gets a value indicating whether it is in the progress that the client is sending data to or receiving data from web server.
    /// </summary>
    public bool IsInAgentProgress => State switch
    {
        ResponsiveChatMessageStates.SendingIntent or ResponsiveChatMessageStates.Intent or ResponsiveChatMessageStates.SendingMessage or ResponsiveChatMessageStates.Waiting or ResponsiveChatMessageStates.Receiving => true,
        _ => false
    };

    /// <summary>
    /// Updates the state.
    /// </summary>
    /// <param name="value">The new state.</param>
    /// <param name="note">The additional note of the state.</param>
    /// <returns>The state change record.</returns>
    internal ResponsiveChatMessageStateRecord UpdateState(ResponsiveChatMessageStates value, string note = null)
    {
        if (State == value && string.IsNullOrWhiteSpace(note)) return null;
        var now = DateTime.Now;
        State = value;
        var record = Model.UpdateState(value, note);
        StateUpdated?.Invoke(this, State);
        return record;
    }

    internal void SetDataSummary(JsonObjectNode json)
        => intentContainer.SetValue("response", json);

    internal void SetDataRecord()
    {
        var arr = new JsonArrayNode();
        foreach (var item in Model.Records)
        {
            if (item == null) continue;
            arr.Add(item.ToJson());
        }

        intentContainer.SetValue("records", arr);
        intentContainer.SetValue("state", State.ToString());
    }

    internal void SetAnswer(string message)
    {
        Model.Answer.Message = message;
        EnableAnswer();
    }

    internal void AppendAnswer(string value)
    {
        Model.Answer.Message += value;
        EnableAnswer();
    }

    internal void SetError(string message, Exception ex = null)
    {
        notification.Message = message;
        Model.UpdateState(ResponsiveChatMessageStates.ResponseError, "Streaming error.");
        if (ex != null) notification.Info.SetValue("error", TextHelper.ToJson(ex));
        var history = Conversation?.History;
        if (string.IsNullOrWhiteSpace(message) || history == null || history.Contains(notification)) return;
        history.Add(notification);
        questionContext.SetValue("answer", notification.Id);
    }

    internal void SetStreamingMode()
        => Model.Answer.ModificationKind = ChatMessageModificationKinds.Streaming;

    private void EnableAnswer()
    {
        var history = Conversation?.History;
        if (history == null || history.Contains(Model.Answer)) return;
        history.Add(Model.Answer);
        questionContext.SetValue("answer", Model.Answer.Id);
    }
}

/// <summary>
/// The flow lifecycle of responsive chat message transferring.
/// </summary>
public class ResponsiveChatSendingLifecycle
{
    /// <summary>
    /// Initializes a new instance of the ResponsiveChatSendingLifecycle class.
    /// </summary>
    internal protected ResponsiveChatSendingLifecycle()
    {
    }

    /// <summary>
    /// Occurs on initialization.
    /// </summary>
    /// <param name="context">The chat context.</param>
    public virtual void OnInit(ResponsiveChatContext context)
    {
    }

    /// <summary>
    /// Occurs on the message is sending.
    /// </summary>
    /// <param name="isStreaming"></param>
    public virtual void OnSend(bool isStreaming)
    {
    }

    /// <summary>
    /// Occurs on the message has received.
    /// </summary>
    public virtual void OnReceive()
    {
    }

    /// <summary>
    /// Occurs on exception thrown.
    /// </summary>
    /// <param name="ex">The exception catched.</param>
    public virtual void OnError(Exception ex)
    {
    }

    /// <summary>
    /// Occurs on the response text is changed.
    /// </summary>
    /// <param name="value">The message value text upddted.</param>
    public virtual void OnTextChange(string value)
    {
    }

    /// <summary>
    /// Occurs on the progress state is changed.
    /// </summary>
    /// <param name="value">The new state.</param>
    public virtual void OnStateChange(ResponsiveChatMessageStates value)
    {
    }
}
