using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Trivial.Collection;
using Trivial.Data;
using Trivial.Reflection;
using Trivial.Text;
using Trivial.Users;

namespace Trivial.Tasks;

/// <summary>
/// The responsive chat message client.
/// </summary>
[Guid("43BA313B-B204-498F-A324-1CD2BAEA9BDD")]
public class ResponsiveChatClient(BaseUserItemInfo sender) : BaseExtendedChatClient(sender)
{
    private readonly List<BaseExtendedChatConversationCache<BaseResponsiveChatProvider>> providers = new();

    /// <summary>
    /// Adds or removes an event handler occurred when a new topic is created in the conversation.
    /// </summary>
    public event DataEventHandler<SelectionRelationship<ExtendedChatConversation, ResponsiveChatMessageTopic>> TopicCreated;

    /// <summary>
    /// Registers a provider.
    /// </summary>
    /// <param name="provider">The provider to register.</param>
    /// <returns>The conversation; or null, if fails.</returns>
    public ExtendedChatConversation Register(BaseResponsiveChatProvider provider)
    {
        if (provider?.Profile == null) return null;
        var conversation = new ExtendedChatConversation(this, provider.Profile);
        providers.Add(conversation, provider);
        return conversation;
    }

    /// <summary>
    /// Registers a set of provider.
    /// </summary>
    /// <param name="providers">The providers to register.</param>
    public int Register(IEnumerable<BaseResponsiveChatProvider> providers)
        => this.providers.AddRange(providers, CreateConversation);

    /// <summary>
    /// Registers a set of provider.
    /// </summary>
    /// <param name="providers">The providers to register.</param>
    public int Register(ReadOnlySpan<BaseResponsiveChatProvider> providers)
        => this.providers.AddRange(providers, CreateConversation);

    /// <summary>
    /// Removes a specific provider from the registry.
    /// </summary>
    /// <param name="provider">The provider to remove.</param>
    /// <returns>true if item is successfully removed; otherwise, false. This method also returns false if item was not found in the provider registry.</returns>
    public bool Remove(BaseResponsiveChatProvider provider)
        => providers.Remove(provider);

    /// <summary>
    /// Removes a specific provider from the registry.
    /// </summary>
    /// <param name="provider">The profile identifier.</param>
    /// <returns>true if item is successfully removed; otherwise, false. This method also returns false if item was not found in the provider registry.</returns>
    public bool Remove(string provider)
        => providers.Remove(provider);

    /// <summary>
    /// Creates a new round chat message topic.
    /// </summary>
    /// <param name="conversation">The conversation.</param>
    /// <param name="skipIfExists">true if skip creating if already has one; otherwise, false.</param>
    /// <returns>true if create succeeded; otherwise, false.</returns>
    /// <exception cref="InvalidOperationException">Send the message failed.</exception>
    /// <exception cref="ArgumentNullException">conversation was null.</exception>
    public async Task<bool> CreateTopicAsync(ExtendedChatConversation conversation, bool skipIfExists = false)
    {
        var provider = GetProvider(conversation, true);
        return await CreateTopicAsync(provider, conversation, skipIfExists);
    }

    /// <inheritdoc />
    public override Task<ExtendedChatMessageAvailability> CanSendAsync(ExtendedChatConversation conversation, CancellationToken cancellationToken = default)
    {
        var provider = GetProvider(conversation);
        return Task.FromResult(provider != null && !provider.IsDisabled ? ExtendedChatMessageAvailability.Allowed : ExtendedChatMessageAvailability.Disabled);
    }

    /// <summary>
    /// Sends a question message and receives answer.
    /// </summary>
    /// <param name="conversation">The chat conversation.</param>
    /// <param name="message">The message content.</param>
    /// <param name="parameter">The additional parameter.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The message sent.</returns>
    /// <exception cref="InvalidOperationException">Send the message failed.</exception>
    /// <exception cref="NotSupportedException">Sending action is not available.</exception>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    /// <exception cref="ArgumentNullException">conversation was null.</exception>
    /// <exception cref="ArgumentException">The request message is not valid.</exception>
    public Task<ResponsiveChatMessageResponse> SendForAnswerAsync(ExtendedChatConversation conversation, ExtendedChatMessageContent message, ExtendedChatMessageParameter parameter, CancellationToken cancellationToken = default)
        => SendForDetailsAsync<ResponsiveChatMessageResponse>(conversation, message, parameter, cancellationToken);

    /// <summary>
    /// Sends a question message and receives answer.
    /// </summary>
    /// <param name="conversation">The chat conversation.</param>
    /// <param name="message">The message content.</param>
    /// <param name="parameter">The additional parameter.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The message sent.</returns>
    /// <exception cref="InvalidOperationException">Send the message failed.</exception>
    /// <exception cref="NotSupportedException">Sending action is not available.</exception>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    /// <exception cref="ArgumentNullException">conversation was null.</exception>
    /// <exception cref="ArgumentException">The request message is not valid.</exception>
    public Task<ResponsiveChatMessageResponse> SendForAnswerAsync(ExtendedChatConversation conversation, ExtendedChatMessageContent message, object parameter, CancellationToken cancellationToken = default)
        => SendForDetailsAsync<ResponsiveChatMessageResponse>(conversation, message, TextHelper.ToParameter(parameter), cancellationToken);

    /// <summary>
    /// Sends a question message and receives answer.
    /// </summary>
    /// <param name="conversation">The chat conversation.</param>
    /// <param name="message">The message content.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The message sent.</returns>
    /// <exception cref="InvalidOperationException">Cannot send the message.</exception>
    /// <exception cref="NotSupportedException">Sending action is not available.</exception>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    /// <exception cref="ArgumentNullException">conversation was null.</exception>
    /// <exception cref="ArgumentException">The request message is not valid.</exception>
    public Task<ResponsiveChatMessageResponse> SendForAnswerAsync(ExtendedChatConversation conversation, ExtendedChatMessageContent message, CancellationToken cancellationToken = default)
        => SendForDetailsAsync<ResponsiveChatMessageResponse>(conversation, message, new ExtendedChatMessageParameter(), cancellationToken);

    /// <summary>
    /// Gets the conversation by the provider.
    /// </summary>
    /// <param name="provider">The chat provider.</param>
    /// <returns>The chat conversation; or null, if not found.</returns>
    public ExtendedChatConversation GetConversation(BaseResponsiveChatProvider provider)
    {
        if (provider == null) return null;
        foreach (var item in providers)
        {
            if (item.Provider == provider) return item.Conversation;
        }

        return null;
    }

    /// <summary>
    /// Adds a notification message to the chat history.
    /// </summary>
    /// <param name="conversation">The chat conversation.</param>
    /// <param name="message">The notification message.</param>
    /// <param name="category">An optional category.</param>
    /// <param name="notes">The additional notes.</param>
    /// <returns>The message instance.</returns>
    /// <exception cref="InvalidOperationException">Send the message failed.</exception>
    /// <exception cref="ArgumentNullException">conversation was null.</exception>
    public ExtendedChatMessage AddNotification(ExtendedChatConversation conversation, string message, string category = null, string notes = null)
    {
        if (string.IsNullOrWhiteSpace(message)) return null;
        var provider = GetProvider(conversation, true);
        var msg = new ExtendedChatMessage(conversation, provider.NotificationProfile ?? provider.Profile, new ExtendedChatMessageContent(message, ExtendedChatMessageFormats.Markdown))
        {
            Category = category
        };
        conversation.History.Add(msg);
        msg.Info.SetValueIfNotEmpty("notes", notes);
        return msg;
    }

    /// <summary>
    /// Adds a notification message to the chat history.
    /// </summary>
    /// <param name="conversation">The chat conversation.</param>
    /// <param name="message">The notification message.</param>
    /// <returns>The message instance.</returns>
    /// <exception cref="InvalidOperationException">Send the message failed.</exception>
    /// <exception cref="ArgumentNullException">conversation was null.</exception>
    public ExtendedChatMessage AddNotification(ExtendedChatConversation conversation, ExtendedChatMessageContent message)
    {
        if (message == null) return null;
        var provider = GetProvider(conversation, true);
        var msg = new ExtendedChatMessage(conversation, provider.NotificationProfile ?? provider.Profile, message);
        conversation.History.Add(msg);
        return msg;
    }

    /// <inheritdoc />
    protected override Task<ExtendedChatMessageSendResult> SendAsync(ExtendedChatMessageContext context, CancellationToken cancellationToken = default)
        => ResponsiveChatConversationProvider.SendAsync(GetProvider(context), context, CreateTopicAsync, cancellationToken);

    /// <inheritdoc />
    protected override async Task<ExtendedChatMessageSendResult> UpdateAsync(ExtendedChatMessageContext context, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        throw new ExtendedChatMessageAvailabilityException(ExtendedChatMessageAvailability.Disabled, "Update is not supported.", new UnauthorizedAccessException("No permission to update message."));
    }

    /// <inheritdoc />
    protected override async Task<ExtendedChatMessageSendResult> DeleteAsync(ExtendedChatMessageContext context, CancellationToken cancellationToken = default)
    {
        var provider = GetProvider(context);
        var c = new ResponsiveChatContext(provider, context);
        return await provider.DeleteMessageAsync(context.Message, c, cancellationToken);
    }

    /// <inheritdoc />
    protected override async IAsyncEnumerable<ExtendedChatConversation> LoadEarlierConversationsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        foreach (var item in providers)
        {
            if (item?.Conversation == null) continue;
            yield return item.Conversation;
        }
    }

    /// <summary>
    /// Occurs when a new topic is created in the conversation.
    /// </summary>
    /// <param name="conversation">The conversation.</param>
    /// <param name="topic">The new topic.</param>
    protected virtual void OnTopicCreate(ExtendedChatConversation conversation, ResponsiveChatMessageTopic topic)
    {
    }

    /// <summary>
    /// Gets the provider for the specified conversation.
    /// </summary>
    /// <param name="conversation">The chat conversation.</param>
    /// <param name="throwException">true if throw an exception if not found; otherwise, false.</param>
    /// <returns>The responsive chat provider bound by the conversation; or null, if not found.</returns>
    /// <exception cref="ArgumentNullException">The conversation was null.</exception>
    /// <exception cref="ExtendedChatMessageAvailabilityException">Not found.</exception>
    protected BaseResponsiveChatProvider GetProvider(ExtendedChatConversation conversation, bool throwException = false)
    {
        if (conversation == null)
        {
            if (throwException) throw new ArgumentNullException(nameof(conversation), "conversation was null.");
            return null;
        }

        foreach (var item in providers)
        {
            if (item.Conversation == conversation) return item.Provider;
        }

        if (throwException) throw new ExtendedChatMessageAvailabilityException(ExtendedChatMessageAvailability.NotSupported, "The conversation is not maintained by this client.", new ArgumentException("The conversation is not supported by this client.", nameof(conversation)));
        return null;
    }

    private BaseResponsiveChatProvider GetProvider(ExtendedChatMessageContext context)
        => GetProvider(context?.Conversation, true);

    private async Task<bool> CreateTopicAsync(BaseResponsiveChatProvider provider, ExtendedChatConversation conversation, bool skipIfExists)
    {
        if (!await provider.CreateTopicAsync(skipIfExists)) return false;
        OnTopicCreate(conversation, provider.CurrentTopic);
        TopicCreated?.Invoke(this, new(new(conversation, provider.CurrentTopic)));
        return true;
    }

    private ExtendedChatConversation CreateConversation(BaseResponsiveChatProvider provider)
        => new(this, provider?.Profile);
}
