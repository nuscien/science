using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
/// The flow lifecycle of turn-based chat message transferring.
/// </summary>
public abstract class RoundChatMessageLifecycle
{
    /// <summary>
    /// Occurs on initialization.
    /// </summary>
    /// <param name="model">The current chat message model.</param>
    /// <param name="thread">The chat message thread.</param>
    public virtual void OnInit(RoundChatMessageModel model, RoundChatMessageTopic thread)
    {
    }

    /// <summary>
    /// Occurs on the message is sending.
    /// </summary>
    /// <param name="model">The current chat message model.</param>
    /// <param name="isStreaming"></param>
    /// <param name="thread">The chat message thread.</param>
    public virtual void OnSend(RoundChatMessageModel model, bool isStreaming, RoundChatMessageTopic thread)
    {
    }

    /// <summary>
    /// Occurs on the message has received.
    /// </summary>
    /// <param name="model">The current chat message model.</param>
    /// <param name="thread">The chat message thread.</param>
    public virtual void OnReceive(RoundChatMessageModel model, RoundChatMessageTopic thread)
    {
    }

    /// <summary>
    /// Occurs on exception thrown.
    /// </summary>
    /// <param name="ex">The exception catched.</param>
    /// <param name="model">The current chat message model.</param>
    /// <param name="thread">The chat message thread.</param>
    public virtual void OnError(Exception ex, RoundChatMessageModel model, RoundChatMessageTopic thread)
    {
    }

    /// <summary>
    /// Occurs on the response text is changed.
    /// </summary>
    /// <param name="value">The message value text upddted.</param>
    /// <param name="model">The current chat message model.</param>
    /// <param name="thread">The chat message thread.</param>
    public virtual void OnTextChange(string value, RoundChatMessageModel model, RoundChatMessageTopic thread)
    {
    }

    /// <summary>
    /// Occurs on the progress state is changed.
    /// </summary>
    /// <param name="value">The new state.</param>
    /// <param name="model">The current chat message model.</param>
    /// <param name="thread">The chat message thread.</param>
    public virtual void OnStateChange(RoundChatMessageStates value, RoundChatMessageModel model, RoundChatMessageTopic thread)
    {
    }
}

/// <summary>
/// The flow lifecycle of turn-based chat message transferring.
/// </summary>
/// <typeparam name="T">The type of the flow context.</typeparam>
public abstract class RoundChatMessageLifecycle<T>
{
    /// <summary>
    /// Initializes by a turn-based chat message starting.
    /// </summary>
    /// <param name="thread">The chat message thread.</param>
    /// <returns>A context used by sub-sequent step of the flow.</returns>
    public virtual T Init(RoundChatMessageTopic thread)
        => default;

    /// <summary>
    /// Occurs on initialization.
    /// </summary>
    /// <param name="model">The current chat message model.</param>
    /// <param name="thread">The chat message thread.</param>
    /// <param name="context">The context of this flow lifecycle of turn-based chat message transferring.</param>
    public virtual void OnInit(RoundChatMessageModel model, RoundChatMessageTopic thread, T context)
    {
    }

    /// <summary>
    /// Occurs on the message is sending.
    /// </summary>
    /// <param name="model">The current chat message model.</param>
    /// <param name="isStreaming"></param>
    /// <param name="thread">The chat message thread.</param>
    /// <param name="context">The context of this flow lifecycle of turn-based chat message transferring.</param>
    public virtual void OnSend(RoundChatMessageModel model, bool isStreaming, RoundChatMessageTopic thread, T context)
    {
    }

    /// <summary>
    /// Occurs on the message has received.
    /// </summary>
    /// <param name="model">The current chat message model.</param>
    /// <param name="thread">The chat message thread.</param>
    /// <param name="context">The context of this flow lifecycle of turn-based chat message transferring.</param>
    public virtual void OnReceive(RoundChatMessageModel model, RoundChatMessageTopic thread, T context)
    {
    }

    /// <summary>
    /// Occurs on exception thrown.
    /// </summary>
    /// <param name="ex">The exception catched.</param>
    /// <param name="model">The current chat message model.</param>
    /// <param name="thread">The chat message thread.</param>
    /// <param name="context">The context of this flow lifecycle of turn-based chat message transferring.</param>
    public virtual void OnError(Exception ex, RoundChatMessageModel model, RoundChatMessageTopic thread, T context)
    {
    }

    /// <summary>
    /// Occurs on the response text is changed.
    /// </summary>
    /// <param name="value">The message value text upddted.</param>
    /// <param name="model">The current chat message model.</param>
    /// <param name="thread">The chat message thread.</param>
    /// <param name="context">The context of this flow lifecycle of turn-based chat message transferring.</param>
    public virtual void OnTextChange(string value, RoundChatMessageModel model, RoundChatMessageTopic thread, T context)
    {
    }

    /// <summary>
    /// Occurs on the progress state is changed.
    /// </summary>
    /// <param name="value">The new state.</param>
    /// <param name="model">The current chat message model.</param>
    /// <param name="thread">The chat message thread.</param>
    /// <param name="context">The context of this flow lifecycle of turn-based chat message transferring.</param>
    public virtual void OnStateChange(RoundChatMessageStates value, RoundChatMessageModel model, RoundChatMessageTopic thread, T context)
    {
    }
}

internal class InternalRoundChatMessageLifecycle : RoundChatMessageLifecycle<object>
{
    /// <summary>
    /// Gets the parent instance.
    /// </summary>
    public RoundChatMessageLifecycle Parent { get; set; }

    /// <inheritdoc />
    public override object Init(RoundChatMessageTopic thread)
        => null;

    /// <inheritdoc />
    public override void OnInit(RoundChatMessageModel model, RoundChatMessageTopic thread, object context)
        => Parent?.OnInit(model, thread);

    /// <inheritdoc />
    public override void OnSend(RoundChatMessageModel model, bool isStreaming, RoundChatMessageTopic thread, object context)
        => Parent?.OnSend(model, isStreaming, thread);

    /// <inheritdoc />
    public override void OnReceive(RoundChatMessageModel model, RoundChatMessageTopic thread, object context)
        => Parent?.OnReceive(model, thread);

    /// <inheritdoc />
    public override void OnError(Exception ex, RoundChatMessageModel model, RoundChatMessageTopic thread, object context)
        => Parent?.OnError(ex, model, thread);

    /// <inheritdoc />
    public override void OnTextChange(string value, RoundChatMessageModel model, RoundChatMessageTopic thread, object context)
        => Parent?.OnTextChange(value, model, thread);

    /// <inheritdoc />
    public override void OnStateChange(RoundChatMessageStates value, RoundChatMessageModel model, RoundChatMessageTopic thread, object context)
        => Parent?.OnStateChange(value, model, thread);
}

internal class EmptyRoundChatMessageLifecycle : RoundChatMessageLifecycle<object>
{
    /// <summary>
    /// Gets the singleton of an empty flow lifecycle of turn-based chat message transferring.
    /// </summary>
    public static EmptyRoundChatMessageLifecycle Instance { get; } = new();

    /// <inheritdoc />
    public override object Init(RoundChatMessageTopic thread)
        => null;
}

internal class ExtendedRoundChatMessageLifecycle<T> : RoundChatMessageLifecycle<T>
{
    private bool isStreaming;

    /// <summary>
    /// Gets or sets the bot info.
    /// </summary>
    public Users.BaseUserItemInfo Profile { get; set; }

    /// <summary>
    /// Gets the response message.
    /// </summary>
    public ExtendedChatMessage Message { get; private set; }

    /// <summary>
    /// Gets or sets the callback that the message is created.
    /// </summary>
    public Action<ExtendedChatMessage> Callback { get; set; }

    /// <summary>
    /// Gets the parent instance.
    /// </summary>
    public RoundChatMessageLifecycle<T> Parent { get; set; }

    /// <summary>
    /// Gets the history of chat messages.
    /// </summary>
    public ObservableCollection<ExtendedChatMessage> History { get; set; }

    /// <inheritdoc />
    public override T Init(RoundChatMessageTopic thread)
        => Parent is not null ? (Parent.Init(thread) ?? default) : default;

    /// <inheritdoc />
    public override void OnInit(RoundChatMessageModel model, RoundChatMessageTopic thread, T context)
    {
        Parent?.OnInit(model, thread, context);
    }

    /// <inheritdoc />
    public override void OnSend(RoundChatMessageModel model, bool isStreaming, RoundChatMessageTopic thread, T context)
    {
        this.isStreaming = isStreaming;
        Parent?.OnSend(model, isStreaming, thread, context);
    }

    /// <inheritdoc />
    public override void OnReceive(RoundChatMessageModel model, RoundChatMessageTopic thread, T context)
    {
        UpdateMessage(model.Answer);
        Parent?.OnReceive(model, thread, context);
    }

    /// <inheritdoc />
    public override void OnError(Exception ex, RoundChatMessageModel model, RoundChatMessageTopic thread, T context)
    {
        Parent?.OnError(ex, model, thread, context);
    }

    /// <inheritdoc />
    public override void OnTextChange(string value, RoundChatMessageModel model, RoundChatMessageTopic thread, T context)
    {
        Parent?.OnTextChange(value, model, thread, context);
        UpdateMessage(model.Answer);
    }

    /// <inheritdoc />
    public override void OnStateChange(RoundChatMessageStates value, RoundChatMessageModel model, RoundChatMessageTopic thread, T context)
    {
        Parent?.OnStateChange(value, model, thread, context);
    }

    /// <summary>
    /// Updates the response message.
    /// </summary>
    /// <param name="message">The message text.</param>
    private void UpdateMessage(string message)
    {
        if (Message == null)
        {
            Message = new ExtendedChatMessage(Profile, message)
            {
                ModificationKind = isStreaming ? ChatMessageModificationKinds.Streaming : ChatMessageModificationKinds.Original,
                MessageFormat = ExtendedChatMessageFormats.Markdown,
            };
            History.Add(Message);
            Callback?.Invoke(Message);
        }
        else
        {
            Message.Message = message;
        }
    }
}
