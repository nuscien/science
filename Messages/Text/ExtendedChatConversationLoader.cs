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
/// The resolver for extended chat conversations.
/// </summary>
public interface IExtendedChatConversationLoader
{
    /// <summary>
    /// Lists the chat conversation provider of all or earlier conversations.
    /// </summary>
    /// <param name="client">The chat client.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>All conversations loaded.</returns>
    public IAsyncEnumerable<BaseExtendedChatConversationProvider> LoadEarlierConversationsAsync(BaseExtendedChatClient client, CancellationToken cancellationToken = default);
}

/// <summary>
/// The history resolver of extended chat conversation.
/// </summary>
public interface IExtendedChatConversationHistoryLoader
{
    /// <summary>
    /// Loads the history of the conversation.
    /// </summary>
    /// <param name="context">The context to load history.</param>
    /// <param name="client">The chat client.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the completion of history loading.</returns>
    public Task LoadHistoryAsync(ExtendedChatMessageHistoryContext context, BaseExtendedChatClient client, CancellationToken cancellationToken = default);
}

/// <summary>
/// The options for the extended chat client.
/// </summary>
/// <param name="sender">The message sender.</param>
/// <param name="list">The conversations resolver.</param>
/// <param name="history">The conversation history resolver.</param>
[Guid("54BEF093-D6B4-4D8D-8390-9780385BBC7A")]
public class LiteExtendedChatClientOptions(BaseUserItemInfo sender, IExtendedChatConversationLoader list, IExtendedChatConversationHistoryLoader history)
{
    /// <summary>
    /// Initializes a new instance of the LiteExtendedChatClient class with the specified sender, conversation list resolver, and history resolver.
    /// </summary>
    /// <param name="sender">The message sender.</param>
    /// <param name="resolver">The conversations resolver and history resolver.</param>
    public LiteExtendedChatClientOptions(BaseUserItemInfo sender, IExtendedChatConversationLoader resolver)
        : this(sender, resolver, resolver as IExtendedChatConversationHistoryLoader)
    {
    }

    /// <summary>
    /// Gets the conversations resolver.
    /// </summary>
    public IExtendedChatConversationLoader List { get; } = list;

    /// <summary>
    /// Gets the conversation history resolver.
    /// </summary>
    public IExtendedChatConversationHistoryLoader History { get; } = history;

    /// <summary>
    /// Gets the message sender.
    /// </summary>
    public BaseUserItemInfo Sender { get; } = sender;
}

/// <summary>
/// The extended chat client.
/// </summary>
/// <param name="sender">The sender.</param>
/// <param name="list">The conversations resolver.</param>
/// <param name="history">The conversation history resolver.</param>
[Guid("E83F59D6-A839-4B33-ABB7-629D0883B8B4")]
public class LiteExtendedChatClient(BaseUserItemInfo sender, IExtendedChatConversationLoader list, IExtendedChatConversationHistoryLoader history) : BaseExtendedChatClient(sender)
{
    private readonly IExtendedChatConversationLoader list = list;
    private readonly IExtendedChatConversationHistoryLoader history = history;
    private readonly List<BaseExtendedChatConversationCache<BaseExtendedChatConversationProvider>> providers = new();

    /// <summary>
    /// Initializes a new instance of the LiteExtendedChatClient class with the specified sender, conversation list resolver, and history resolver.
    /// </summary>
    /// <param name="options">The options.</param>
    public LiteExtendedChatClient(LiteExtendedChatClientOptions options)
        : this(options?.Sender, options?.List, options?.History)
    {
    }

    /// <summary>
    /// Registers a provider.
    /// </summary>
    /// <param name="provider">The provider to register.</param>
    public void Register(BaseExtendedChatConversationProvider provider)
        => providers.Add(new(this, provider?.Source), provider);

    /// <summary>
    /// Registers a provider.
    /// </summary>
    /// <param name="provider">The provider to register.</param>
    public void Register(BaseResponsiveChatProvider provider)
        => providers.Add(new(this, provider?.Profile), new ResponsiveChatConversationProvider(provider));

    /// <summary>
    /// Registers a set of provider.
    /// </summary>
    /// <param name="providers">The providers to register.</param>
    public int Register(IEnumerable<BaseExtendedChatConversationProvider> providers)
        => this.providers.AddRange(providers, CreateConversation);

    /// <summary>
    /// Registers a set of provider.
    /// </summary>
    /// <param name="providers">The providers to register.</param>
    public int Register(ReadOnlySpan<BaseExtendedChatConversationProvider> providers)
        => this.providers.AddRange(providers, CreateConversation);

    /// <summary>
    /// Removes a specific provider from the registry.
    /// </summary>
    /// <param name="provider">The provider to remove.</param>
    /// <returns>true if item is successfully removed; otherwise, false. This method also returns false if item was not found in the provider registry.</returns>
    public bool Remove(BaseExtendedChatConversationProvider provider)
        => providers.Remove(provider);

    /// <summary>
    /// Removes a specific provider from the registry.
    /// </summary>
    /// <param name="provider">The profile identifier.</param>
    /// <returns>true if item is successfully removed; otherwise, false. This method also returns false if item was not found in the provider registry.</returns>
    public bool Remove(string provider)
        => providers.Remove(provider);

    /// <inheritdoc />
    public override async Task<ExtendedChatMessageAvailability> CanSendAsync(ExtendedChatConversation conversation, CancellationToken cancellationToken = default)
    {
        var provider = GetProvider(conversation, true);
        return await provider.CanSendAsync(cancellationToken);
    }

    /// <inheritdoc />
    public override ExtendedChatMessageManagementLevels GetManageLevel(ExtendedChatConversation conversation, ExtendedChatMessage message)
        => message.Sender == conversation.Sender ? ExtendedChatMessageManagementLevels.All : ExtendedChatMessageManagementLevels.None;

    /// <inheritdoc />
    protected override async IAsyncEnumerable<ExtendedChatConversation> LoadEarlierConversationsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var list = this.list;
        if (list is null) yield break;
        var col = list.LoadEarlierConversationsAsync(this, cancellationToken);
        if (col == null) yield break;
        await foreach (var provider in col)
        {
            if (provider?.Source == null) continue;
            var conversation = new ExtendedChatConversation(this, provider.Source);
            providers.Add(conversation, provider);
            yield return conversation;
        }
    }

    /// <inheritdoc />
    protected override Task LoadHistoryAsync(ExtendedChatMessageHistoryContext context, CancellationToken cancellationToken = default)
    {
        var history = this.history;
        if (history is null) return Task.CompletedTask;
        return history.LoadHistoryAsync(context, this, cancellationToken);
    }

    /// <inheritdoc />
    protected override async Task<ExtendedChatMessageSendResult> SendAsync(ExtendedChatMessageContext context, CancellationToken cancellationToken = default)
    {
        var provider = GetProvider(context);
        return await provider.SendAsync(context, cancellationToken);
    }

    /// <inheritdoc />
    protected override async Task<ExtendedChatMessageSendResult> UpdateAsync(ExtendedChatMessageContext context, CancellationToken cancellationToken = default)
    {
        var provider = GetProvider(context);
        return await provider.UpdateAsync(context, cancellationToken);
    }

    /// <inheritdoc />
    protected override async Task<ExtendedChatMessageSendResult> DeleteAsync(ExtendedChatMessageContext context, CancellationToken cancellationToken = default)
    {
        var provider = GetProvider(context);
        return await provider.DeleteAsync(context, cancellationToken);
    }

    private BaseExtendedChatConversationProvider GetProvider(ExtendedChatConversation conversation, bool throwException = false)
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

    private BaseExtendedChatConversationProvider GetProvider(ExtendedChatMessageContext context)
        => GetProvider(context?.Conversation, true);

    private ExtendedChatConversation CreateConversation(BaseExtendedChatConversationProvider provider)
        => new(this, provider?.Source);
}
