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
/// The collection of chat conversation.
/// </summary>
/// <param name="sender">The sender.</param>
[Guid("CA2B708D-C85E-4E8E-9C4D-55899BD7DA85")]
public class ExtendedChatConversationCollection(BaseUserItemInfo sender)
{
    /// <summary>
    /// Gets or sets the status to load conversations.
    /// </summary>
    protected object Status { get; set; }

    /// <summary>
    /// Gets the sender.
    /// </summary>
    public BaseUserItemInfo Sender { get; } = sender;

    /// <summary>
    /// Gets the collection of conversation loaded.
    /// </summary>
    public ObservableCollection<ExtendedChatConversation> List { get; } = new();

    /// <summary>
    /// Gets or sets the additional tag.
    /// </summary>
    public object Tag { get; set; }

    /// <summary>
    /// Lists the chat conversation provider of all or earlier conversations.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>All conversations loaded.</returns>
    public virtual Task LoadEarlierConversationsAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <summary>
    /// Adds and sorts a given conversation in the collection cache.
    /// </summary>
    /// <param name="conversation">The conversation item to add.</param>
    public void Add(ExtendedChatConversation conversation)
    {
        if (!ReferenceEquals(Sender, conversation.Sender) || Sender is null) return;
        TextHelper.Add(List, conversation);
    }

    /// <summary>
    /// Adds a collection of conversation at the end of the collection cache.
    /// </summary>
    /// <param name="conversations">The conversation item collection to append.</param>
    /// <returns>The count of item to add.</returns>
    public int Add(IEnumerable<ExtendedChatConversation> conversations)
    {
        var i = 0;
        if (conversations is null || Sender is null) return i;
        foreach (var conversation in conversations)
        {
            if (conversation?.Id == null || List.Contains(conversation)) continue;
            i++;
            List.Add(conversation);
        }

        return i;
    }

    /// <summary>
    /// Removes a set of convsersation from the cache.
    /// </summary>
    /// <param name="conversations">The conversations to remove.</param>
    /// <returns>The count of item removed.</returns>
    public int Remove(IEnumerable<ExtendedChatConversation> conversations)
    {
        var i = 0;
        if (conversations == null) return i;
        foreach (var conversation in conversations)
        {
            if (conversation == null) continue;
            if (List.Remove(conversation)) i++;
        }

        return i;
    }

    /// <summary>
    /// Removes a convsersation from the cache.
    /// </summary>
    /// <param name="conversation">The conversation to remove.</param>
    /// <returns>true if item is successfully removed; otherwise, false. This method also returns false if item was not found in the cache.</returns>
    public bool Remove(ExtendedChatConversation conversation)
    {
        if (conversation == null) return false;
        return List.Remove(conversation);
    }

    /// <summary>
    /// Gets the specific conversation in cache.
    /// </summary>
    /// <param name="id">The conversation identifer.</param>
    /// <returns>The conversation; or null, if not found.</returns>
    public ExtendedChatConversation Get(string id)
    {
        foreach (var conversation in List)
        {
            if (conversation?.Id == id) return conversation;
        }

        return null;
    }

    /// <summary>
    /// Gets the specific conversation in cache.
    /// </summary>
    /// <param name="id">The conversation identifer.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The conversation; or null, if not found.</returns>
    public async Task<ExtendedChatConversation> GetAsync(string id, CancellationToken cancellationToken)
    {
        var conversation = Get(id);
        if (conversation != null) return conversation;
        var item = await GetFurtherAsync(id, cancellationToken);
        if (item is null) return null;
        Add(item);
        return item;
    }

    /// <summary>
    /// Tests if the cache contains the specific conversation.
    /// </summary>
    /// <param name="id">The conversation identifer.</param>
    /// <param name="result">The conversation.</param>
    /// <returns>true if contains; otherwise false.</returns>
    public bool Contains(string id, out ExtendedChatConversation result)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            result = null;
            return false;
        }

        foreach (var conversation in List)
        {
            if (conversation?.Id == id)
            {
                result = conversation;
                return true;
            }
        }

        result = null;
        return false;
    }

    /// <summary>
    /// Tests if the cache contains the specific conversation.
    /// </summary>
    /// <param name="id">The conversation identifer.</param>
    /// <returns>true if contains; otherwise false.</returns>
    public bool Contains(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return false;
        foreach (var conversation in List)
        {
            if (conversation?.Id == id) return true;
        }

        return false;
    }

    /// <summary>
    /// Tests if the cache contains the specific conversation.
    /// </summary>
    /// <param name="conversation">The conversation to test.</param>
    /// <returns>true if contains; otherwise false.</returns>
    public bool Contains(ExtendedChatConversation conversation)
    {
        if (conversation is null) return false;
        foreach (var item in List)
        {
            if (ReferenceEquals(item, conversation)) return true;
        }

        return false;
    }

    /// <summary>
    /// Tests if the cache contains the specific conversation.
    /// </summary>
    /// <param name="conversation">The conversation identifier.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>true if contains; otherwise false.</returns>
    public async Task<bool> ContainsAsync(string conversation, CancellationToken cancellationToken = default)
    {
        if (Contains(conversation)) return true;
        var item = await GetFurtherAsync(conversation, cancellationToken);
        if (item is null) return false;
        Add(item);
        return true;
    }

    /// <summary>
    /// Sends a message.
    /// </summary>
    /// <param name="conversation">The conversation identifier.</param>
    /// <param name="message">The message content.</param>
    /// <param name="parameter">The additional parameter.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The message sent.</returns>
    /// <exception cref="ExtendedChatMessageAvailabilityException">Sending action is not available.</exception>
    /// <exception cref="ExtendedChatMessageException">Send the message failed.</exception>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    /// <exception cref="ArgumentNullException">conversation was null.</exception>
    /// <exception cref="ArgumentException">The conversation identifier should not be empty or consist only of white-space characters.</exception>
    public async Task<ExtendedChatMessage> SendAsync(string conversation, ExtendedChatMessageContent message, ExtendedChatMessageParameter parameter, CancellationToken cancellationToken = default)
    {
        var c = GetOrThrowException(conversation);
        return await c.SendAsync(message, parameter, cancellationToken);
    }

    /// <summary>
    /// Sends a message.
    /// </summary>
    /// <param name="conversation">The conversation identifier.</param>
    /// <param name="message">The message content.</param>
    /// <param name="parameter">The additional parameter.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The message sent.</returns>
    /// <exception cref="ExtendedChatMessageAvailabilityException">Sending action is not available.</exception>
    /// <exception cref="ExtendedChatMessageException">Send the message failed.</exception>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    /// <exception cref="ArgumentNullException">conversation was null.</exception>
    /// <exception cref="ArgumentException">The conversation identifier should not be empty or consist only of white-space characters.</exception>
    public async Task<ExtendedChatMessage> SendAsync(string conversation, ExtendedChatMessageContent message, object parameter, CancellationToken cancellationToken = default)
    {
        var c = GetOrThrowException(conversation);
        return await c.SendAsync(message, parameter, cancellationToken);
    }

    /// <summary>
    /// Sends a message.
    /// </summary>
    /// <param name="conversation">The conversation identifier.</param>
    /// <param name="message">The message content.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The message sent.</returns>
    /// <exception cref="ExtendedChatMessageAvailabilityException">Sending action is not available.</exception>
    /// <exception cref="ExtendedChatMessageException">Send the message failed.</exception>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    /// <exception cref="ArgumentNullException">conversation was null.</exception>
    /// <exception cref="ArgumentException">The conversation identifier should not be empty or consist only of white-space characters.</exception>
    public async Task<ExtendedChatMessage> SendAsync(string conversation, ExtendedChatMessageContent message, CancellationToken cancellationToken = default)
    {
        var c = GetOrThrowException(conversation);
        return await c.SendAsync(message, cancellationToken);
    }

    /// <summary>
    /// Modifies a message.
    /// </summary>
    /// <param name="original">The original message.</param>
    /// <param name="message">The new message content.</param>
    /// <param name="parameter">The additional parameter.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The result of saving action.</returns>
    /// <exception cref="ExtendedChatMessageException">Send the message failed.</exception>
    /// <exception cref="NotSupportedException">The modification action is not supported.</exception>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    /// <exception cref="ArgumentNullException">conversation was null.</exception>
    /// <exception cref="ArgumentException">The conversation identifier should not be empty or consist only of white-space characters.</exception>
    public async Task<ExtendedChatMessageSendResult> ModifyAsync(ExtendedChatMessage original, string message, ExtendedChatMessageParameter parameter, CancellationToken cancellationToken = default)
    {
        var c = GetOrThrowException(original);
        return await c.ModifyAsync(original, message, parameter, cancellationToken);
    }

    /// <summary>
    /// Modifies a message.
    /// </summary>
    /// <param name="original">The original message.</param>
    /// <param name="message">The new message content.</param>
    /// <param name="parameter">The additional parameter.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The result of saving action.</returns>
    /// <exception cref="NotSupportedException">The modification action is not supported.</exception>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    public async Task<ExtendedChatMessageSendResult> ModifyAsync(ExtendedChatMessage original, string message, object parameter, CancellationToken cancellationToken = default)
    {
        var c = GetOrThrowException(original);
        return await c.ModifyAsync(original, message, parameter, cancellationToken);
    }

    /// <summary>
    /// Modifies a message.
    /// </summary>
    /// <param name="original">The original message.</param>
    /// <param name="message">The new message content.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The result of saving action.</returns>
    /// <exception cref="NotSupportedException">The modification action is not supported.</exception>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    public async Task<ExtendedChatMessageSendResult> ModifyAsync(ExtendedChatMessage original, string message, CancellationToken cancellationToken = default)
    {
        var c = GetOrThrowException(original);
        return await c.ModifyAsync(original, message, cancellationToken);
    }

    /// <summary>
    /// Deletes a message.
    /// </summary>
    /// <param name="message">The message to delete.</param>
    /// <param name="parameter">The additional parameter.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The result of saving action.</returns>
    /// <exception cref="NotSupportedException">The deletion action is not supported.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Cannot find the message by the specific identifier.</exception>
    /// <exception cref="ArgumentNullException">The message identifier cannot be null.</exception>
    /// <exception cref="ArgumentException">The message identifier should not be empty or consist only of white-space characters.</exception>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    public async Task<ExtendedChatMessageSendResult> DeleteAsync(ExtendedChatMessage message, ExtendedChatMessageParameter parameter, CancellationToken cancellationToken = default)
    {
        var c = GetOrThrowException(message);
        return await c.DeleteAsync(message, parameter, cancellationToken);
    }

    /// <summary>
    /// Deletes a message.
    /// </summary>
    /// <param name="message">The message to delete.</param>
    /// <param name="parameter">The additional parameter.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The result of saving action.</returns>
    /// <exception cref="NotSupportedException">The deletion action is not supported.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Cannot find the message by the specific identifier.</exception>
    /// <exception cref="ArgumentNullException">The message identifier cannot be null.</exception>
    /// <exception cref="ArgumentException">The message identifier should not be empty or consist only of white-space characters.</exception>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    public async Task<ExtendedChatMessageSendResult> DeleteAsync(ExtendedChatMessage message, object parameter, CancellationToken cancellationToken = default)
    {
        var c = GetOrThrowException(message);
        return await c.DeleteAsync(message, parameter, cancellationToken);
    }

    /// <summary>
    /// Deletes a message.
    /// </summary>
    /// <param name="message">The message to delete.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The result of saving action.</returns>
    /// <exception cref="NotSupportedException">The deletion action is not supported.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Cannot find the message by the specific identifier.</exception>
    /// <exception cref="ArgumentNullException">The message identifier cannot be null.</exception>
    /// <exception cref="ArgumentException">The message identifier should not be empty or consist only of white-space characters.</exception>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    public async Task<ExtendedChatMessageSendResult> DeleteAsync(ExtendedChatMessage message, CancellationToken cancellationToken = default)
    {
        var c = GetOrThrowException(message);
        return await c.DeleteAsync(message, cancellationToken);
    }

    /// <summary>
    /// Gets the conversation which is not loaded yet.
    /// </summary>
    /// <param name="conversation">The conversation identifier.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The conversation instance; or null, if not found.</returns>
    protected virtual Task<ExtendedChatConversation> GetFurtherAsync(string conversation, CancellationToken cancellationToken = default)
        => Task.FromResult<ExtendedChatConversation>(null);

    private ExtendedChatConversation GetOrThrowException(string conversation)
    {
        if (conversation == null) throw new ArgumentNullException(nameof(conversation), "conversation was null.");
        if (string.IsNullOrWhiteSpace(conversation)) throw new ArgumentException("The conversation identifier should not be empty or consist only of white-space characters.", nameof(conversation));
        return Get(conversation) ?? throw new ExtendedChatMessageAvailabilityException(ExtendedChatMessageAvailability.NotSupported, "The conversation is not maintained by this client.", new ArgumentException("The conversation is not supported by this client.", nameof(conversation)));
    }

    private ExtendedChatConversation GetOrThrowException(ExtendedChatMessage message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message), "message was null.");
        if (string.IsNullOrWhiteSpace(message.OwnerId)) throw new ArgumentException("The conversation is not found in the message.", nameof(message));
        return Get(message.OwnerId) ?? throw new ExtendedChatMessageAvailabilityException(ExtendedChatMessageAvailability.NotSupported, "The conversation is not maintained by this client.", new ArgumentException("The conversation is not supported by this client.", nameof(message)));
    }

    //private void AssertContains(ExtendedChatConversation conversation)
    //{
    //    if (conversation == null) throw new ArgumentNullException(nameof(conversation), "conversation was null.");
    //    if (!Contains(conversation)) throw new ExtendedChatMessageAvailabilityException(ExtendedChatMessageAvailability.NotSupported, "The conversation is not maintained by this client.", new ArgumentException("The conversation is not supported by this client.", nameof(conversation)));
    //}
}
