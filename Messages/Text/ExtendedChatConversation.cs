using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Trivial.Data;
using Trivial.Reflection;
using Trivial.Tasks;
using Trivial.Users;

namespace Trivial.Text;

/// <summary>
/// The chat message conversation, may be a user, group, bot or topic.
/// </summary>
[Guid("6FBB2174-7BE0-4A51-BA0F-C6E856AB4E2E")]
public class ExtendedChatConversation : BaseObservableProperties
{
    /// <summary>
    /// Initializes a new instance of the ExtendedChatConversation class.
    /// </summary>
    /// <param name="provider">The provider.</param>
    /// <param name="source">The conversation source.</param>
    internal protected ExtendedChatConversation(BaseExtendedChatClient provider, BaseResourceEntityInfo source)
    {
        if (string.IsNullOrEmpty(source?.Id)) provider = null;
        Provider = provider ?? new InternalExtendedChatClient(null, false);
        SetProperty(nameof(Source), source);
        _ = CanSendAsync();
    }

    /// <summary>
    /// Gets the provider to send and receive messages for this conversation.
    /// </summary>
    protected internal BaseExtendedChatClient Provider { get; }

    /// <summary>
    /// Gets the identifier of the resource which is the owner of the messages.
    /// </summary>
    public string Id => Source?.Id;

    /// <summary>
    /// Gets the resource source which is the owner of the messages.
    /// </summary>
    public BaseResourceEntityInfo Source => GetCurrentProperty<BaseResourceEntityInfo>();

    /// <summary>
    /// Gets the history.
    /// </summary>
    public ObservableCollection<ExtendedChatMessage> History { get; } = new();

    /// <summary>
    /// Gets the sender.
    /// </summary>
    public BaseUserItemInfo Sender => Provider.Sender;

    /// <summary>
    /// Gets a value indicating whether current conversation is avalaible to send message.
    /// </summary>
    public bool CanSend => GetProperty<bool>("CanSendByService") && GetProperty<bool>("CanSendByTemp");

    /// <summary>
    /// Gets the earlist message date in the conversation history.
    /// </summary>
    /// <returns></returns>
    public DateTime? GetEarliestMessageDate()
    {
        if (History.Count < 1) return null;
        return History.Where(ele => ele != null).Select(ele => ele.CreationTime).Min();
    }

    /// <summary>
    /// Gets the earlist message date in the conversation history.
    /// </summary>
    /// <returns></returns>
    public DateTime? GetLatestMessageDate()
    {
        if (History.Count < 1) return null;
        return History.Where(ele => ele != null).Select(ele => ele.CreationTime).Max();
    }

    /// <summary>
    /// Sends a message.
    /// </summary>
    /// <param name="message">The message content.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The message sent.</returns>
    public Task<ExtendedChatMessage> SendAsync(ExtendedChatMessageContent message, CancellationToken cancellationToken = default)
        => Provider.SendAsync(this, message, cancellationToken);

    /// <summary>
    /// Modifies a message.
    /// </summary>
    /// <param name="original">The original message.</param>
    /// <param name="message">The new message content.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The result of saving action.</returns>
    public Task<ExtendedChatMessageSendResult> ModifyAsync(ExtendedChatMessage original, string message, CancellationToken cancellationToken = default)
        => Provider.ModifyAsync(this, original, message, cancellationToken);

    /// <summary>
    /// Deletes a message.
    /// </summary>
    /// <param name="message">The message to delete.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The result of saving action.</returns>
    public Task<ExtendedChatMessageSendResult> DeleteAsync(ExtendedChatMessage message, CancellationToken cancellationToken = default)
        => Provider.DeleteAsync(this, message, cancellationToken);

    /// <summary>
    /// Deletes a message.
    /// </summary>
    /// <param name="id">The identifier of the message to delete.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The result of saving action.</returns>
    public Task<ExtendedChatMessageSendResult> DeleteAsync(string id, CancellationToken cancellationToken = default)
        => Provider.DeleteAsync(this, GetMessage(id), cancellationToken);

    /// <summary>
    /// Tests if current conversation is avalaible to send message.
    /// </summary>
    /// <returns>true if can send; otherwise, false.</returns>
    public async Task<bool> CanSendAsync()
    {
        var b = await Provider.CanSendAsync(this);
        SetProperty("CanSendByService", b);
        return CanSend;
    }

    /// <summary>
    /// Gets the source.
    /// </summary>
    /// <param name="id">The identifier of the resource source.</param>
    /// <returns>The source info; or null, if not found.</returns>
    public ExtendedChatMessage GetMessage(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        foreach (var message in History)
        {
            if (message.Id == id) return message;
        }

        return null;
    }

    /// <summary>
    /// Sets the flag of whether can send messages by service.
    /// </summary>
    /// <param name="value">true if can sent message; otherwise, false.</param>
    internal void SetValueOfCanSendTemp(bool value)
        => SetProperty("CanSendByTemp", value);

    /// <summary>
    /// Throws if the current state is not allowed to send message.
    /// </summary>
    /// <param name="msg">The message to send.</param>
    /// <exception cref="InvalidOperationException">Cannot send the message.</exception>
    internal void ThrowIfCannotSend(ExtendedChatMessage msg = null)
    {
        if (CanSend) return;
        if (msg != null)
        {
            History.Remove(msg);
            msg.UpdateSavingStatus(ExtendedChatMessageSendResultStates.NotSend);
        }

        throw new InvalidOperationException("Cannot send message.");
    }
}
