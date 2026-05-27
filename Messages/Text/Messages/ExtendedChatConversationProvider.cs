using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Trivial.Data;
using Trivial.Reflection;
using Trivial.Users;

namespace Trivial.Text;

/// <summary>
/// The chat message conversation, may be a user, group, bot or topic.
/// </summary>
public interface IExtendedChatConversationProvider
{
    /// <summary>
    /// Gets nagement level of a message in the conversation.
    /// </summary>
    /// <param name="message">The message</param>
    /// <returns>The management level.</returns>
    public ExtendedChatMessageManagementLevels GetManageLevel(ExtendedChatMessage message);

    /// <summary>
    /// Sends a message.
    /// </summary>
    /// <param name="context">The current chat message context.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The message sent.</returns>
    /// <exception cref="ExtendedChatMessageAvailabilityException">Sending action is not available.</exception>
    /// <exception cref="ExtendedChatMessageException">Send the message failed.</exception>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    public Task<ExtendedChatMessageSendResult> SendAsync(ExtendedChatMessageContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Modifies a message.
    /// </summary>
    /// <param name="context">The current chat message context.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The result of saving action.</returns>
    /// <exception cref="ExtendedChatMessageAvailabilityException">Sending action is not available.</exception>
    /// <exception cref="ExtendedChatMessageException">Send the message failed.</exception>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    public Task<ExtendedChatMessageSendResult> UpdateAsync(ExtendedChatMessageContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a message.
    /// </summary>
    /// <param name="context">The current chat message context.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The result of saving action.</returns>
    /// <exception cref="ExtendedChatMessageAvailabilityException">Sending action is not available.</exception>
    /// <exception cref="ExtendedChatMessageException">Send the message failed.</exception>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    public Task<ExtendedChatMessageSendResult> DeleteAsync(ExtendedChatMessageContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests if current conversation is avalaible to send message.
    /// </summary>
    /// <param name="conversation">The chat conversation.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The state about if the user can send message.</returns>
    public Task<ExtendedChatMessageAvailability> GetServiceAvailabilityAsync(ExtendedChatConversation conversation, CancellationToken cancellationToken = default);
}
