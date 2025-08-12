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
/// The chat message provider for a conversation.
/// </summary>
/// <param name="source"></param>
public abstract class BaseExtendedChatConversationProvider(BaseResourceEntityInfo source)
{
    /// <summary>
    /// Gets the resource source which is the owner of the messages.
    /// </summary>
    public BaseResourceEntityInfo Source { get; } = source;

    /// <summary>
    /// Gets the identifier of the chat service.
    /// </summary>
    public virtual string Id => Source?.Id ?? Guid.Empty.ToString("N");

    /// <summary>
    /// Sends a message.
    /// </summary>
    /// <param name="context">The current chat message context.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The message sent.</returns>
    /// <exception cref="ExtendedChatMessageAvailabilityException">Sending action is not available.</exception>
    /// <exception cref="ExtendedChatMessageException">Send the message failed.</exception>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    public abstract Task<ExtendedChatMessageSendResult> SendAsync(ExtendedChatMessageContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Modifies a message.
    /// </summary>
    /// <param name="context">The current chat message context.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The result of saving action.</returns>
    /// <exception cref="ExtendedChatMessageAvailabilityException">Sending action is not available.</exception>
    /// <exception cref="ExtendedChatMessageException">Send the message failed.</exception>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    public abstract Task<ExtendedChatMessageSendResult> UpdateAsync(ExtendedChatMessageContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a message.
    /// </summary>
    /// <param name="context">The current chat message context.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The result of saving action.</returns>
    /// <exception cref="ExtendedChatMessageAvailabilityException">Sending action is not available.</exception>
    /// <exception cref="ExtendedChatMessageException">Send the message failed.</exception>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    public abstract Task<ExtendedChatMessageSendResult> DeleteAsync(ExtendedChatMessageContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests if this conversation is available to send message or not.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The state about if the user can send message.</returns>
    public abstract Task<ExtendedChatMessageAvailability> CanSendAsync(CancellationToken cancellationToken = default);
}
