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
    private readonly List<ResponsiveChatProviderCache> providers = new();

    /// <summary>
    /// Registers a provider.
    /// </summary>
    /// <param name="provider">The provider to register.</param>
    public void Register(BaseResponsiveChatProvider provider)
    {
        if (provider.Profile == null) return;
        providers.Add(new()
        {
            Provider = provider,
            Conversation = new(this, provider.Profile)
        });
    }

    /// <summary>
    /// Registers a set of provider.
    /// </summary>
    /// <param name="providers">The providers to register.</param>
    public int Register(IEnumerable<BaseResponsiveChatProvider> providers)
    {
        var i = 0;
        if (providers == null) return i;
        foreach (var provider in providers)
        {
            if (provider.Profile == null) continue;
            this.providers.Add(new()
            {
                Provider = provider,
                Conversation = new(this, provider.Profile)
            });
            i++;
        }

        return i;
    }

    /// <summary>
    /// Registers a set of provider.
    /// </summary>
    /// <param name="providers">The providers to register.</param>
    public int Register(ReadOnlySpan<BaseResponsiveChatProvider> providers)
    {
        var i = 0;
        foreach (var provider in providers)
        {
            if (provider.Profile == null) continue;
            this.providers.Add(new()
            {
                Provider = provider,
                Conversation = new(this, provider.Profile)
            });
            i++;
        }

        return i;
    }

    /// <summary>
    /// Removes a specific provider from the registry.
    /// </summary>
    /// <param name="provider">The provider to remove.</param>
    /// <returns>true if item is successfully removed; otherwise, false. This method also returns false if item was not found in the provider registry.</returns>
    public bool Remove(BaseResponsiveChatProvider provider)
    {
        if (provider == null) return false;
        ResponsiveChatProviderCache cache = null;
        foreach (var item in providers)
        {
            if (item?.Provider != provider) continue;
            cache = item;
            break;
        }

        if (cache == null) return false;
        return providers.Remove(cache);
    }

    /// <summary>
    /// Removes a specific provider from the registry.
    /// </summary>
    /// <param name="provider">The profile identifier.</param>
    /// <returns>true if item is successfully removed; otherwise, false. This method also returns false if item was not found in the provider registry.</returns>
    public bool Remove(string provider)
    {
        if (string.IsNullOrWhiteSpace(provider)) return false;
        ResponsiveChatProviderCache cache = null;
        foreach (var item in providers)
        {
            if (item?.Provider?.Id != provider) continue;
            cache = item;
            break;
        }

        if (cache == null) return false;
        return providers.Remove(cache);
    }

    /// <summary>
    /// Gets the contact profile.
    /// </summary>
    /// <param name="id">The profile identifier.</param>
    /// <returns>The contact profile.</returns>
    public BaseUserItemInfo GetProfile(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        foreach (var item in providers)
        {
            if (item?.Provider?.Id == id) return item.Provider.Profile;
        }

        return null;
    }

    /// <inheritdoc />
    public override Task<bool> CanSendAsync(ExtendedChatConversation conversation, CancellationToken cancellationToken = default)
    {
        var provider = GetProvider(conversation);
        return Task.FromResult(provider != null && !provider.IsDisabled);
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

    /// <inheritdoc />
    protected override async Task<ExtendedChatMessageSendResult> SendAsync(ExtendedChatMessageContext context, CancellationToken cancellationToken = default)
    {
        var token = context.CanSend(false);
        try
        {
            var provider = GetProvider(context);
            var c = await provider.CreateContextAsync(context) ?? throw new ArgumentException("The question message content should not be empty.", nameof(context));
            if (!context.ParameterIs(out ResponsiveChatSendingLifecycle monitor)) monitor = null;
            var result = await provider.SendMessageAsync(c, monitor ?? new(), cancellationToken);
            var t = token;
            token = null;
            var response = provider.GetResponse(c, result, monitor, () =>
            {
                context.CanSend(t, true, out _);
            }, cancellationToken);
            context.SetDetails(response);
            return result;
        }
        finally
        {
            if (token != null) context.CanSend(token, true, out _);
        }
    }

    /// <inheritdoc />
    protected override async Task<ExtendedChatMessageSendResult> UpdateAsync(ExtendedChatMessageContext context, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        throw new NotSupportedException("Update is not supported.", new UnauthorizedAccessException("No permission to update message."));
    }

    /// <inheritdoc />
    protected override async Task<ExtendedChatMessageSendResult> DeleteAsync(ExtendedChatMessageContext context, CancellationToken cancellationToken = default)
    {
        var provider = GetProvider(context);
        return await provider.DeleteAsync(context, cancellationToken);
    }

    /// <inheritdoc />
    protected override async IAsyncEnumerable<ExtendedChatConversation> ListConversationsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        foreach (var item in providers)
        {
            if (item?.Conversation == null) continue;
            yield return item.Conversation;
        }
    }

    private BaseResponsiveChatProvider GetProvider(ExtendedChatConversation conversation, bool throwException = false)
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

        if (throwException) throw new InvalidOperationException("Not found.");
        return null;
    }

    private BaseResponsiveChatProvider GetProvider(ExtendedChatMessageContext context)
        => GetProvider(context?.Conversation, true);
}

internal class ResponsiveChatProviderCache
{
    public BaseResponsiveChatProvider Provider { get; set; }

    public ExtendedChatConversation Conversation { get; set; }
}
