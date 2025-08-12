using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Trivial.Text;
using Trivial.Users;

namespace Trivial.Tasks;

/// <summary>
/// The provider for responsive chat conversations.
/// </summary>
/// <param name="provider">The provider.</param>
internal class ResponsiveChatConversationProvider(BaseResponsiveChatProvider provider) : BaseExtendedChatConversationProvider(provider?.Profile)
{
    /// <summary>
    /// Gets the responsive chat provider.
    /// </summary>
    public BaseResponsiveChatProvider Provider { get; } = provider;

    /// <inheritdoc />
    public override Task<ExtendedChatMessageAvailability> CanSendAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(Provider.IsDisabled ? ExtendedChatMessageAvailability.Disabled : ExtendedChatMessageAvailability.Allowed);

    /// <inheritdoc />
    public override Task<ExtendedChatMessageSendResult> DeleteAsync(ExtendedChatMessageContext context, CancellationToken cancellationToken = default)
        => Provider.DeleteMessageAsync(context.Message, new ResponsiveChatContext(Provider, context), cancellationToken);

    /// <inheritdoc />
    public override Task<ExtendedChatMessageSendResult> SendAsync(ExtendedChatMessageContext context, CancellationToken cancellationToken = default)
        => SendAsync(Provider, context, CreateTopicAsync, cancellationToken);

    /// <inheritdoc />
    public override async Task<ExtendedChatMessageSendResult> UpdateAsync(ExtendedChatMessageContext context, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        throw new ExtendedChatMessageAvailabilityException(ExtendedChatMessageAvailability.Disabled, "Update is not supported.", new UnauthorizedAccessException("No permission to update message."));
    }

    private async Task<bool> CreateTopicAsync(BaseResponsiveChatProvider provider, ExtendedChatConversation conversation, bool skipIfExists)
        => await provider.CreateTopicAsync(skipIfExists);

    internal static async Task<ExtendedChatMessageSendResult> SendAsync(BaseResponsiveChatProvider provider, ExtendedChatMessageContext context, Func<BaseResponsiveChatProvider, ExtendedChatConversation, bool, Task<bool>> createTopicAsync, CancellationToken cancellationToken = default)
    {
        var token = context.CanSend(ExtendedChatMessageAvailability.Throttle);
        try
        {
            if (string.IsNullOrWhiteSpace(context?.Message?.Message)) throw new ArgumentException("The question message content should not be empty.", nameof(context));
            await createTopicAsync(provider, context.Conversation, true);
            var c = new ResponsiveChatContext(provider, context);
            if (!context.ParameterIs(out ResponsiveChatSendingLifecycle monitor)) monitor = null;
            var result = await provider.SendMessageAsync(c, monitor ?? new(), cancellationToken);
            var t = token;
            token = null;
            var response = provider.GetResponse(c, result, monitor, () =>
            {
                context.CanSend(t, ExtendedChatMessageAvailability.Allowed, out _);
            }, cancellationToken);
            context.SetDetails(response);
            return result;
        }
        finally
        {
            if (token != null) context.CanSend(token, ExtendedChatMessageAvailability.Allowed, out _);
        }
    }
}
