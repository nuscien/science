using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
    /// <param name="client">The client.</param>
    /// <param name="source">The conversation source.</param>
    internal protected ExtendedChatConversation(BaseExtendedChatClient client, BaseResourceEntityInfo source)
    {
        if (string.IsNullOrEmpty(source?.Id)) client = null;
        Client = client ?? new InternalExtendedChatClient(null, false);
        SetProperty(nameof(CanSend), ExtendedChatMessageAvailability.Testing);
        SetProperty(nameof(Source), source);
        _ = CanSendAsync();
    }

    /// <summary>
    /// Initializes a new instance of the ExtendedChatConversation class.
    /// </summary>
    /// <param name="client">The client.</param>
    /// <param name="source">The conversation source.</param>
    /// <param name="parameter">The parameter used by the client to store something.</param>
    internal protected ExtendedChatConversation(BaseExtendedChatClient client, BaseResourceEntityInfo source, object parameter)
    {
        if (string.IsNullOrEmpty(source?.Id)) client = null;
        Client = client ?? new InternalExtendedChatClient(null, false);
        SetProperty(nameof(CanSend), ExtendedChatMessageAvailability.Testing);
        SetProperty(nameof(Source), source);
        _ = CanSendAsync();
    }

    /// <summary>
    /// Gets or sets the parameter.
    /// </summary>
    internal object Parameter { get; set; }

    /// <summary>
    /// Gets or sets the history status.
    /// </summary>
    internal object HistoryStatus { get; set; }

    /// <summary>
    /// Gets the client to send and receive messages for this conversation.
    /// </summary>
    protected internal BaseExtendedChatClient Client { get; }

    /// <summary>
    /// Gets or sets the display name of the conversation.
    /// </summary>
    public string DisplayName
    {
        get => GetCurrentProperty<string>() ?? Source?.GetProperty(ResourceEntitySpecialProperties.DisplayName);
        set => SetCurrentProperty(value);
    }

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
    public BaseUserItemInfo Sender => Client.Sender;

    /// <summary>
    /// Gets the state about if the user can send message.
    /// </summary>
    public ExtendedChatMessageAvailability CanSend
    {
        get
        {
            var b = GetProperty<ExtendedChatMessageAvailability>("CanSendByService");
            if (b != ExtendedChatMessageAvailability.Allowed) return b;
            return GetProperty<ExtendedChatMessageAvailability>("CanSendByTemp");
        }
    }

    /// <summary>
    /// Gets a value indicating whether current conversation is avalaible to send message.
    /// </summary>
    public bool IsAllowedToSendMessage
    {
        get => GetCurrentProperty<bool>();
    }

    /// <summary>
    /// Gets the earlist message date in the conversation history.
    /// </summary>
    /// <returns>The date time of earlist message; or null, if no message.</returns>
    public DateTime? GetEarliestMessageDate()
    {
        if (History.Count < 1) return null;
        return History.Where(ele => ele != null).Select(ele => ele.CreationTime).Min();
    }

    /// <summary>
    /// Gets the latest message date in the conversation history.
    /// </summary>
    /// <returns>The date time of latest message; or null, if no message.</returns>
    public DateTime? GetLatestMessageDate()
    {
        if (History.Count < 1) return null;
        return History.Where(ele => ele != null).Select(ele => ele.CreationTime).Max();
    }

    /// <summary>
    /// Gets or sets the additional tag.
    /// </summary>
    public object Tag { get; set; }

    /// <summary>
    /// Sends a message.
    /// </summary>
    /// <param name="message">The message content.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The message sent.</returns>
    public Task<ExtendedChatMessage> SendAsync(ExtendedChatMessageContent message, CancellationToken cancellationToken = default)
        => Client.SendAsync(this, message, cancellationToken);

    /// <summary>
    /// Modifies a message.
    /// </summary>
    /// <param name="original">The original message.</param>
    /// <param name="message">The new message content.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The result of saving action.</returns>
    public Task<ExtendedChatMessageSendResult> ModifyAsync(ExtendedChatMessage original, string message, CancellationToken cancellationToken = default)
        => Client.ModifyAsync(this, original, message, cancellationToken);

    /// <summary>
    /// Deletes a message.
    /// </summary>
    /// <param name="message">The message to delete.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The result of saving action.</returns>
    public Task<ExtendedChatMessageSendResult> DeleteAsync(ExtendedChatMessage message, CancellationToken cancellationToken = default)
        => Client.DeleteAsync(this, message, cancellationToken);

    /// <summary>
    /// Deletes a message.
    /// </summary>
    /// <param name="id">The identifier of the message to delete.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The result of saving action.</returns>
    public Task<ExtendedChatMessageSendResult> DeleteAsync(string id, CancellationToken cancellationToken = default)
        => Client.DeleteAsync(this, GetMessage(id), cancellationToken);

    /// <summary>
    /// Tests if current conversation is avalaible to send message.
    /// </summary>
    /// <returns>The state about if the user can send message.</returns>
    public async Task<ExtendedChatMessageAvailability> CanSendAsync()
    {
        var b = await Client.CanSendAsync(this);
        SetProperty("CanSendByService", b);
        UpdateIfIsAllowedToSend();
        return b;
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
    /// <param name="state">The state about if the user can send message.</param>
    internal void SetValueOfCanSendTemp(ExtendedChatMessageAvailability state)
    {
        SetProperty("CanSendByTemp", state);
        UpdateIfIsAllowedToSend();
    }

    /// <summary>
    /// Throws if the current state is not allowed to send message.
    /// </summary>
    /// <param name="msg">The message to send.</param>
    /// <exception cref="ExtendedChatMessageAvailabilityException">Cannot send the message.</exception>
    internal void ThrowIfCannotSend(ExtendedChatMessage msg = null)
    {
        var state = CanSend;
        if (state == ExtendedChatMessageAvailability.Allowed) return;
        if (msg != null)
        {
            History.Remove(msg);
            msg.UpdateSavingStatus(ExtendedChatMessageSendResultStates.NotSend);
        }

        throw state switch
        {
            ExtendedChatMessageAvailability.Disabled => new ExtendedChatMessageAvailabilityException(state, "Cannot send message.", new UnauthorizedAccessException("Not allowed to send message.")),
            ExtendedChatMessageAvailability.Unauthorized => new ExtendedChatMessageAvailabilityException(state, "No permission to send message.", new UnauthorizedAccessException("Requires login by authorized account.")),
            ExtendedChatMessageAvailability.ReadOnly => new ExtendedChatMessageAvailabilityException(state, "The conversation is read-only.", new InvalidOperationException("The conversation is read-only.")),
            ExtendedChatMessageAvailability.Disconnnected => new ExtendedChatMessageAvailabilityException(state, "The connection is not available."),
            ExtendedChatMessageAvailability.Throttle => new ExtendedChatMessageAvailabilityException(state, "Traffic limitation for connection.", new TaskCanceledException("Sending is canceled because traffic limitation for connection.")),
            ExtendedChatMessageAvailability.NotSupported => new ExtendedChatMessageAvailabilityException(state, "The conversation is not supported."),
            _ => new ExtendedChatMessageAvailabilityException(state, "Cannot send message.", new UnauthorizedAccessException("Not allowed to send message."))
        };
    }

    /// <summary>
    /// Adds a message to the specific conversation.
    /// </summary>
    /// <param name="message">The message to add.</param>
    /// <param name="last">true if append at last; otherwise, false.</param>
    /// <param name="conversations">All conversations.</param>
    internal void AddMessage(ExtendedChatMessage message, bool last, ObservableCollection<ExtendedChatConversation> conversations = null)
    {
        if (message?.OwnerId == null) return;
        if (History.Contains(message)) return;
        if (History.Count < 1 || last)
        {
            History.Add(message);
            TextHelper.Sort(conversations, this);
            return;
        }

        var latest = History.LastOrDefault();
        if (latest != null && latest.CreationTime <= message.CreationTime)
        {
            History.Add(message);
            TextHelper.Sort(conversations, this);
            return;
        }

        for (var i = History.Count - 2; i >= 0; i--)
        {
            if (History[i] == null) continue;
            if (History[i].CreationTime <= message.CreationTime) break;
            History.Insert(i + 1, message);
            return;
        }

        History.Insert(0, message);
    }

    /// <summary>
    /// Adds a collection of messages to the conversation history.
    /// All messages in the collection will be added to the beginning of the history.
    /// </summary>
    /// <param name="messages">The message collection.</param>
    /// <returns>The count of message added.</returns>
    internal int AddHistory(IEnumerable<ExtendedChatMessage> messages)
    {
        if (messages == null) return 0;
        var i = 0;
        if (History.Count < 1)
        {
            foreach (var item in messages)
            {
                if (item == null || History.Contains(item)) continue;
                History.Add(item);
                i++;
            }
        }
        else
        {
            foreach (var item in messages)
            {
                if (item == null || History.Contains(item)) continue;
                History.Insert(i, item);
                i++;
            }
        }

        return i;
    }

    /// <summary>
    /// Adds a collection of messages to the conversation history.
    /// All messages in the collection will be added to the beginning of the history.
    /// </summary>
    /// <param name="messages">The message collection.</param>
    /// <returns>The count of message added.</returns>
    internal async Task<int> AddHistoryAsync(IAsyncEnumerable<ExtendedChatMessage> messages)
    {
        if (messages == null) return 0;
        var i = 0;
        if (History.Count < 1)
        {
            await foreach (var item in messages)
            {
                if (item == null || History.Contains(item)) continue;
                History.Add(item);
                i++;
            }
        }
        else
        {
            await foreach (var item in messages)
            {
                if (item == null || History.Contains(item)) continue;
                History.Insert(i, item);
                i++;
            }
        }

        return i;
    }

    private void UpdateIfIsAllowedToSend()
        => SetProperty(nameof(IsAllowedToSendMessage), CanSend == ExtendedChatMessageAvailability.Allowed);
}

internal class BaseExtendedChatConversationCache<T>(ExtendedChatConversation conversation, T provider)
{
    /// <summary>
    /// Gets the provider.
    /// </summary>
    public T Provider { get; } = provider;

    /// <summary>
    /// Gets the conversation.
    /// </summary>
    public ExtendedChatConversation Conversation { get; } = conversation;
}
