using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Trivial.Collection;
using Trivial.Data;
using Trivial.Reflection;
using Trivial.Tasks;
using Trivial.Users;

namespace Trivial.Text;

/// <summary>
/// The base client of extended chat service.
/// </summary>
/// <param name="sender">The sender.</param>
public abstract class BaseExtendedChatClient(BaseUserItemInfo sender)
{
    /// <summary>
    /// Adds or removes an event handler occurred on sending.
    /// </summary>
    public event DataEventHandler<ExtendedChatMessage> Sending;

    /// <summary>
    /// Adds or removes an event handler occurred on the message sents succeeded.
    /// </summary>
    public event DataEventHandler<ExtendedChatMessage> Sent;

    /// <summary>
    /// Adds or removes an event handler occurred on the message sents failed.
    /// </summary>
    public event DataEventHandler<ExtendedChatMessage> SendFailed;

    /// <summary>
    /// Adds or removes an event handler occurred on the message sents canceled.
    /// </summary>
    public event DataEventHandler<ExtendedChatMessage> SendCanceled;

    /// <summary>
    /// Adds or removes an event handler occurred on modifying.
    /// </summary>
    public event DataEventHandler<ExtendedChatMessage> Modifying;

    /// <summary>
    /// Adds or removes an event handler occurred on the message is modified.
    /// </summary>
    public event DataEventHandler<ExtendedChatMessage> Modified;

    /// <summary>
    /// Adds or removes an event handler occurred on the message modifies failed.
    /// </summary>
    public event DataEventHandler<ExtendedChatMessage> ModifyFailed;

    /// <summary>
    /// Adds or removes an event handler occurred on the message modifies canceled.
    /// </summary>
    public event DataEventHandler<ExtendedChatMessage> ModifyCanceled;

    /// <summary>
    /// Adds or removes an event handler occurred on deleting.
    /// </summary>
    public event DataEventHandler<ExtendedChatMessage> Deleting;

    /// <summary>
    /// Adds or removes an event handler occurred on the message is deleted.
    /// </summary>
    public event DataEventHandler<ExtendedChatMessage> Deleted;

    /// <summary>
    /// Adds or removes an event handler occurred on the message deletes failed.
    /// </summary>
    public event DataEventHandler<ExtendedChatMessage> DeleteFailed;

    /// <summary>
    /// Adds or removes an event handler occurred on the message deletes canceled.
    /// </summary>
    public event DataEventHandler<ExtendedChatMessage> DeleteCanceled;

    /// <summary>
    /// Gets all conversations in cache.
    /// </summary>
    public ObservableCollection<ExtendedChatConversation> Conversations { get; }

    /// <summary>
    /// Gets the sender.
    /// </summary>
    public BaseUserItemInfo Sender { get; } = sender;

    /// <summary>
    /// Sends a message.
    /// </summary>
    /// <param name="conversation">The chat conversation.</param>
    /// <param name="message">The message content.</param>
    /// <param name="parameter">The additional parameter.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The message sent.</returns>
    /// <exception cref="InvalidOperationException">Cannot send the message.</exception>
    /// <exception cref="NotSupportedException">Sending action is not available.</exception>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    /// <exception cref="ArgumentNullException">conversation was null.</exception>
    /// <exception cref="ArgumentException">The request message is not valid.</exception>
    public async Task<ExtendedChatMessage> SendAsync(ExtendedChatConversation conversation, ExtendedChatMessageContent message, ExtendedChatMessageParameter parameter, CancellationToken cancellationToken = default)
    {
        if (message == null) return null;
        if (conversation == null) throw new ArgumentNullException(nameof(conversation), "conversation was null.");
        conversation.ThrowIfCannotSend();
        parameter ??= new();
        var msg = new ExtendedChatMessage(Guid.NewGuid(), conversation, Sender, message, null)
        {
            State = ResourceEntityStates.Publishing
        };
        conversation.History.Add(msg);
        parameter.SendStatus = ExtendedChatMessageSendResultStates.NotSend;
        parameter.Begin(msg, ChangeMethods.Add);
        msg.UpdateSavingStatus(ResourceEntitySavingStates.Saving);
        ExtendedChatMessageSendResult resp;
        var context = CreateContext(msg, conversation, ChangeMethods.Add, parameter.Parameter) ?? new(msg, conversation, ChangeMethods.Add, parameter.Parameter);
        Sending?.Invoke(this, new(msg));
        msg.State = ResourceEntityStates.Normal;
        conversation.ThrowIfCannotSend(msg);
        context.CanSetDetails = true;
        try
        {
            resp = await SendAsync(context, cancellationToken) ?? new();
            context.CanSetDetails = false;
        }
        catch (OperationCanceledException ex)
        {
            context.CanSetDetails = false;
            try
            {
                msg.UpdateSavingStatus(ex, out resp);
                parameter.SendStatus = resp?.SendStatus ?? ExtendedChatMessageSendResultStates.Aborted;
                conversation.History.Remove(msg);
            }
            finally
            {
                OnSendStateUpdateError(context);
            }

            parameter.Details = context.Details;
            parameter.Cancel();
            OnSendCanceled(context, resp);
            SendCanceled?.Invoke(this, new(msg));
            throw;
        }
        catch (Exception ex)
        {
            context.CanSetDetails = false;
            try
            {
                msg.UpdateSavingStatus(ex, out resp);
                parameter.SendStatus = resp?.SendStatus ?? ExtendedChatMessageSendResultStates.OtherError;
                conversation.History.Remove(msg);
            }
            catch (Exception)
            {
                OnSendStateUpdateError(context);
                throw;
            }

            parameter.Details = context.Details;
            parameter.End(false);
            OnSendFailed(context, resp);
            _ = OnSendFailedAsync(context, resp);
            SendFailed?.Invoke(this, new(msg));
            if (resp.ShouldThrowException) throw;
            return msg;
        }

        parameter.Details = context.Details;
        try
        {
            msg.UpdateSavingStatus(resp);
            parameter.SendStatus = resp.SendStatus;
        }
        catch (Exception)
        {
            OnSendStateUpdateError(context);
            throw;
        }

        if (resp.IsSuccessful)
        {
            parameter.End(true);
            OnSendSucceeded(context, resp);
            _ = OnSendSucceededAsync(context, resp);
            Sent?.Invoke(this, new(msg));
        }
        else
        {
            parameter.End(false);
            OnSendFailed(context, resp);
            _ = OnSendFailedAsync(context, resp);
            SendFailed?.Invoke(this, new(msg));
        }

        return msg;
    }

    /// <summary>
    /// Sends a message.
    /// </summary>
    /// <param name="conversation">The chat conversation.</param>
    /// <param name="message">The message content.</param>
    /// <param name="parameter">The additional parameter.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The message sent.</returns>
    /// <exception cref="InvalidOperationException">Cannot send the message.</exception>
    /// <exception cref="NotSupportedException">Sending action is not available.</exception>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    /// <exception cref="ArgumentNullException">conversation was null.</exception>
    /// <exception cref="ArgumentException">The request message is not valid.</exception>
    public Task<ExtendedChatMessage> SendAsync(ExtendedChatConversation conversation, ExtendedChatMessageContent message, object parameter, CancellationToken cancellationToken = default)
        => SendAsync(conversation, message, TextHelper.ToParameter(parameter), cancellationToken);

    /// <summary>
    /// Sends a message.
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
    public Task<ExtendedChatMessage> SendAsync(ExtendedChatConversation conversation, ExtendedChatMessageContent message, CancellationToken cancellationToken = default)
        => SendAsync(conversation, message, new ExtendedChatMessageParameter(), cancellationToken);

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
    public Task<ExtendedChatMessageSendResult> ModifyAsync(ExtendedChatMessage original, string message, ExtendedChatMessageParameter parameter, CancellationToken cancellationToken = default)
    {
        var conversation = GetConversation(original);
        return ModifyAsync(conversation, original, message, parameter, cancellationToken);
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
    public Task<ExtendedChatMessageSendResult> ModifyAsync(ExtendedChatMessage original, string message, object parameter, CancellationToken cancellationToken = default)
    {
        var conversation = GetConversation(original);
        return ModifyAsync(conversation, original, message, TextHelper.ToParameter(parameter), cancellationToken);
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
    public Task<ExtendedChatMessageSendResult> ModifyAsync(ExtendedChatMessage original, string message, CancellationToken cancellationToken = default)
        => ModifyAsync(original, message, new ExtendedChatMessageParameter(), cancellationToken);

    /// <summary>
    /// Modifies a message.
    /// </summary>
    /// <param name="conversation">The chat conversation.</param>
    /// <param name="original">The original message.</param>
    /// <param name="message">The new message content.</param>
    /// <param name="parameter">The additional parameter.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The result of saving action.</returns>
    /// <exception cref="NotSupportedException">The modification action is not supported.</exception>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    public async Task<ExtendedChatMessageSendResult> ModifyAsync(ExtendedChatConversation conversation, ExtendedChatMessage original, string message, ExtendedChatMessageParameter parameter, CancellationToken cancellationToken = default)
    {
        var level = GetManageLevel(conversation, original);
        if (!level.HasFlag(ExtendedChatMessageManagementLevels.Modification)) throw new NotSupportedException("Update is not supported.", new UnauthorizedAccessException("No permission to update message."));
        parameter ??= new();
        var old = original.Message;
        original.Message = message;
        ExtendedChatMessageSendResult resp;
        original.UpdateSavingStatus(ResourceEntitySavingStates.Saving);
        parameter.SendStatus = ExtendedChatMessageSendResultStates.Pending;
        parameter.Begin(original, ChangeMethods.Remove);
        Modifying?.Invoke(this, new(original));
        var context = CreateContext(original, conversation, ChangeMethods.MemberModify, parameter.Parameter) ?? new(original, conversation, ChangeMethods.MemberModify, parameter.Parameter);
        context.CanSetDetails = true;
        try
        {
            resp = await UpdateAsync(context, cancellationToken) ?? new();
            context.CanSetDetails = false;
        }
        catch (OperationCanceledException ex)
        {
            context.CanSetDetails = false;
            original.Message = old;
            original.UpdateSavingStatus(ex, out resp);
            parameter.SendStatus = resp?.SendStatus ?? ExtendedChatMessageSendResultStates.Aborted;
            conversation.History.Remove(original);
            parameter.Details = context.Details;
            parameter.Cancel();
            ModifyCanceled?.Invoke(this, new(original));
            throw;
        }
        catch (Exception ex)
        {
            context.CanSetDetails = false;
            original.Message = old;
            original.UpdateSavingStatus(ex, out resp);
            parameter.SendStatus = resp?.SendStatus ?? ExtendedChatMessageSendResultStates.OtherError;
            conversation.History.Remove(original);
            parameter.Details = context.Details;
            parameter.End(false);
            ModifyFailed?.Invoke(this, new(original));
            if (resp.ShouldThrowException) throw;
            return resp;
        }

        parameter.Details = context.Details;
        if (resp.IsSuccessful) original.Message = old;
        original.UpdateSavingStatus(resp);
        parameter.SendStatus = resp.SendStatus;
        parameter.End(resp.IsSuccessful);
        if (resp.IsSuccessful) Modified?.Invoke(this, new(original));
        else ModifyFailed?.Invoke(this, new(original));
        return resp;
    }

    /// <summary>
    /// Modifies a message.
    /// </summary>
    /// <param name="conversation">The chat conversation.</param>
    /// <param name="original">The original message.</param>
    /// <param name="message">The new message content.</param>
    /// <param name="parameter">The additional parameter.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The result of saving action.</returns>
    /// <exception cref="NotSupportedException">The modification action is not supported.</exception>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    public Task<ExtendedChatMessageSendResult> ModifyAsync(ExtendedChatConversation conversation, ExtendedChatMessage original, string message, object parameter, CancellationToken cancellationToken = default)
        => ModifyAsync(conversation, original, message, TextHelper.ToParameter(parameter), cancellationToken);

    /// <summary>
    /// Modifies a message.
    /// </summary>
    /// <param name="conversation">The chat conversation.</param>
    /// <param name="original">The original message.</param>
    /// <param name="message">The new message content.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The result of saving action.</returns>
    /// <exception cref="NotSupportedException">The modification action is not supported.</exception>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    public Task<ExtendedChatMessageSendResult> ModifyAsync(ExtendedChatConversation conversation, ExtendedChatMessage original, string message, CancellationToken cancellationToken = default)
        => ModifyAsync(conversation, original, message, new ExtendedChatMessageParameter(), cancellationToken);

    /// <summary>
    /// Deletes a message.
    /// </summary>
    /// <param name="message">The message to delete.</param>
    /// <param name="parameter">The additional parameter.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The result of saving action.</returns>
    /// <exception cref="NotSupportedException">The deletion action is not supported.</exception>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    public Task<ExtendedChatMessageSendResult> DeleteAsync(ExtendedChatMessage message, ExtendedChatMessageParameter parameter, CancellationToken cancellationToken = default)
    {
        var conversation = GetConversation(message);
        return DeleteAsync(conversation, message, parameter, cancellationToken);
    }

    /// <summary>
    /// Deletes a message.
    /// </summary>
    /// <param name="message">The message to delete.</param>
    /// <param name="parameter">The additional parameter.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The result of saving action.</returns>
    /// <exception cref="NotSupportedException">The deletion action is not supported.</exception>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    public Task<ExtendedChatMessageSendResult> DeleteAsync(ExtendedChatMessage message, object parameter, CancellationToken cancellationToken = default)
    {
        var conversation = GetConversation(message);
        return DeleteAsync(conversation, message, parameter, cancellationToken);
    }

    /// <summary>
    /// Deletes a message.
    /// </summary>
    /// <param name="message">The message to delete.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The result of saving action.</returns>
    /// <exception cref="NotSupportedException">The deletion action is not supported.</exception>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    public Task<ExtendedChatMessageSendResult> DeleteAsync(ExtendedChatMessage message, CancellationToken cancellationToken = default)
    {
        var conversation = GetConversation(message);
        return DeleteAsync(conversation, message, new ExtendedChatMessageParameter(), cancellationToken);
    }

    /// <summary>
    /// Deletes a message.
    /// </summary>
    /// <param name="id">The identifier of the message to delete.</param>
    /// <param name="parameter">The additional parameter.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The result of saving action.</returns>
    /// <exception cref="NotSupportedException">The deletion action is not supported.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Cannot find the message by the specific identifier.</exception>
    /// <exception cref="ArgumentNullException">The message identifier cannot be null.</exception>
    /// <exception cref="ArgumentException">The message identifier should not be empty or consist only of white-space characters.</exception>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    public async Task<ExtendedChatMessageSendResult> DeleteAsync(string id, ExtendedChatMessageParameter parameter, CancellationToken cancellationToken = default)
    {
        if (id == null) throw new ArgumentNullException(nameof(id), "The message identifier cannot be null.");
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("The message identifier should not be empty or consist only of white-space characters.", nameof(id));
        foreach (var conversation in Conversations)
        {
            var entity = conversation.GetMessage(id);
            if (entity == null) continue;
            return await DeleteAsync(conversation, entity, parameter, cancellationToken);
        }

        throw new ArgumentOutOfRangeException(nameof(id), "The message with the specific identifier does not exist in any conversation.");
    }

    /// <summary>
    /// Deletes a message.
    /// </summary>
    /// <param name="id">The identifier of the message to delete.</param>
    /// <param name="parameter">The additional parameter.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The result of saving action.</returns>
    /// <exception cref="NotSupportedException">The deletion action is not supported.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Cannot find the message by the specific identifier.</exception>
    /// <exception cref="ArgumentNullException">The message identifier cannot be null.</exception>
    /// <exception cref="ArgumentException">The message identifier should not be empty or consist only of white-space characters.</exception>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    public Task<ExtendedChatMessageSendResult> DeleteAsync(string id, object parameter, CancellationToken cancellationToken = default)
        => DeleteAsync(id, TextHelper.ToParameter(parameter), cancellationToken);

    /// <summary>
    /// Deletes a message.
    /// </summary>
    /// <param name="id">The identifier of the message to delete.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The result of saving action.</returns>
    /// <exception cref="NotSupportedException">The deletion action is not supported.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Cannot find the message by the specific identifier.</exception>
    /// <exception cref="ArgumentNullException">The message identifier cannot be null.</exception>
    /// <exception cref="ArgumentException">The message identifier should not be empty or consist only of white-space characters.</exception>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    public Task<ExtendedChatMessageSendResult> DeleteAsync(string id, CancellationToken cancellationToken = default)
        => DeleteAsync(id, new ExtendedChatMessageParameter(), cancellationToken);

    /// <summary>
    /// Deletes a message.
    /// </summary>
    /// <param name="conversation">The chat conversation.</param>
    /// <param name="message">The message to delete.</param>
    /// <param name="parameter">The additional parameter.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The result of saving action.</returns>
    /// <exception cref="NotSupportedException">The deletion action is not supported.</exception>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    public async Task<ExtendedChatMessageSendResult> DeleteAsync(ExtendedChatConversation conversation, ExtendedChatMessage message, ExtendedChatMessageParameter parameter, CancellationToken cancellationToken = default)
    {
        var level = GetManageLevel(conversation, message);
        if (!level.HasFlag(ExtendedChatMessageManagementLevels.Deletion)) throw new NotSupportedException("Cannot delete the message.", new UnauthorizedAccessException("No permission to delete the message."));
        parameter ??= new();
        var history = conversation.History;
        ExtendedChatMessageSendResult resp;
        message.UpdateSavingStatus(ResourceEntitySavingStates.Saving);
        parameter.SendStatus = ExtendedChatMessageSendResultStates.Pending;
        parameter.Begin(message, ChangeMethods.Remove);
        Deleting?.Invoke(this, new(message));
        var context = CreateContext(message, conversation, ChangeMethods.Remove, parameter.Parameter) ?? new(message, conversation, ChangeMethods.Remove, parameter.Parameter);
        context.CanSetDetails = true;
        try
        {
            resp = await DeleteAsync(context, cancellationToken) ?? new();
            context.CanSetDetails = false;
        }
        catch (OperationCanceledException ex)
        {
            context.CanSetDetails = false;
            message.UpdateSavingStatus(ex, out resp);
            parameter.SendStatus = resp?.SendStatus ?? ExtendedChatMessageSendResultStates.Aborted;
            conversation.History.Remove(message);
            parameter.Details = context.Details;
            parameter.Cancel();
            DeleteCanceled?.Invoke(this, new(message));
            throw;
        }
        catch (Exception ex)
        {
            context.CanSetDetails = false;
            message.UpdateSavingStatus(ex, out resp);
            parameter.SendStatus = resp?.SendStatus ?? ExtendedChatMessageSendResultStates.OtherError;
            conversation.History.Remove(message);
            parameter.Details = context.Details;
            parameter.End(false);
            DeleteFailed?.Invoke(this, new(message));
            if (resp.ShouldThrowException) throw;
            return resp;
        }

        parameter.Details = context.Details;
        if (resp.IsSuccessful)
        {
            history.Remove(message);
            if (message.State == ResourceEntityStates.Normal) message.State = ResourceEntityStates.Deleted;
        }

        message.UpdateSavingStatus(resp);
        parameter.SendStatus = resp.SendStatus;
        parameter.End(resp.IsSuccessful);
        if (resp.IsSuccessful) Deleted?.Invoke(this, new(message));
        else DeleteFailed?.Invoke(this, new(message));
        return resp;
    }

    /// <summary>
    /// Deletes a message.
    /// </summary>
    /// <param name="conversation">The chat conversation.</param>
    /// <param name="message">The message to delete.</param>
    /// <param name="parameter">The additional parameter.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The result of saving action.</returns>
    /// <exception cref="NotSupportedException">The deletion action is not supported.</exception>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    public Task<ExtendedChatMessageSendResult> DeleteAsync(ExtendedChatConversation conversation, ExtendedChatMessage message, object parameter, CancellationToken cancellationToken = default)
        => DeleteAsync(conversation, message, TextHelper.ToParameter(parameter), cancellationToken);

    /// <summary>
    /// Deletes a message.
    /// </summary>
    /// <param name="conversation">The chat conversation.</param>
    /// <param name="message">The message to delete.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The result of saving action.</returns>
    /// <exception cref="NotSupportedException">The deletion action is not supported.</exception>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    public Task<ExtendedChatMessageSendResult> DeleteAsync(ExtendedChatConversation conversation, ExtendedChatMessage message, CancellationToken cancellationToken = default)
        => DeleteAsync(conversation, message, new ExtendedChatMessageParameter(), cancellationToken);

    /// <summary>
    /// Gets the conversation.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <returns>The conversation; or null, if not found.</returns>
    public ExtendedChatConversation GetConversation(ExtendedChatMessage message)
    {
        foreach (var conversation in Conversations)
        {
            if (conversation.History.Contains(message)) return conversation;
        }

        return null;
    }

    /// <summary>
    /// Gets the conversation.
    /// </summary>
    /// <param name="id">The conversation identifier.</param>
    /// <returns>The conversation; or null, if not found.</returns>
    public ExtendedChatConversation GetConversation(string id)
    {
        foreach (var conversation in Conversations)
        {
            if (conversation.Id == id) return conversation;
        }

        return null;
    }

    /// <summary>
    /// Gets nagement level of a message in the conversation.
    /// </summary>
    /// <param name="conversation">The chat conversation.</param>
    /// <param name="message">The message</param>
    /// <returns>The management level.</returns>
    public virtual ExtendedChatMessageManagementLevels GetManageLevel(ExtendedChatConversation conversation, ExtendedChatMessage message)
        => ExtendedChatMessageManagementLevels.None;

    /// <summary>
    /// Gets nagement level of a message in the conversation.
    /// </summary>
    /// <param name="message">The message</param>
    /// <returns>The management level.</returns>
    public ExtendedChatMessageManagementLevels GetManageLevel(ExtendedChatMessage message)
        => GetManageLevel(GetConversation(message), message);

    /// <summary>
    /// Tests if this conversation is available to send message or not.
    /// </summary>
    /// <param name="conversation">The chat conversation.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>true if the conversation is available to send message; otherwise, false.</returns>
    public abstract Task<bool> CanSendAsync(ExtendedChatConversation conversation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all conversations.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The count of conversation loaded.</returns>
    public async Task<int> ListAsync(CancellationToken cancellationToken = default)
    {
        var i = 0;
        await foreach (var conversation in ListConversationsAsync(cancellationToken))
        {
            if (string.IsNullOrWhiteSpace(conversation?.Id)) continue;
            Conversations.Add(conversation);
            i++;
        }

        return i;
    }

    /// <summary>
    /// Loads the history of the conversation.
    /// </summary>
    /// <param name="conversation">The chat conversation.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the completion of history loading.</returns>
    public virtual Task LoadHistoryAsync(ExtendedChatConversation conversation, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <summary>
    /// Sends a question message and gets the details information.
    /// </summary>
    /// <param name="conversation">The chat conversation.</param>
    /// <param name="message">The message content.</param>
    /// <param name="parameter">The additional parameter.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The details information of processing and response.</returns>
    /// <exception cref="InvalidOperationException">Send the message failed.</exception>
    /// <exception cref="NotSupportedException">Sending action is not available.</exception>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    /// <exception cref="ArgumentNullException">conversation was null.</exception>
    /// <exception cref="ArgumentException">The request message is not valid.</exception>
    protected async Task<T> SendForDetailsAsync<T>(ExtendedChatConversation conversation, ExtendedChatMessageContent message, ExtendedChatMessageParameter parameter, CancellationToken cancellationToken = default)
    {
        parameter ??= new();
        await SendAsync(conversation, message, parameter, cancellationToken);
        if (ObjectConvert.TryGet(parameter.Details, out T response)) return response;
        throw new InvalidOperationException(parameter.Details is null
            ? "Get details information failed because it is null."
            : $"Get details information failed because its type is not the expected one. Its actual type is {parameter.Details.GetType().Name}.");
    }

    /// <summary>
    /// Sends a question message and gets the details information.
    /// </summary>
    /// <param name="conversation">The chat conversation.</param>
    /// <param name="message">The message content.</param>
    /// <param name="parameter">The additional parameter.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The details information of processing and response.</returns>
    /// <exception cref="InvalidOperationException">Send the message failed.</exception>
    /// <exception cref="NotSupportedException">Sending action is not available.</exception>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    /// <exception cref="ArgumentNullException">conversation was null.</exception>
    /// <exception cref="ArgumentException">The request message is not valid.</exception>
    protected Task<T> SendForDetailsAsync<T>(ExtendedChatConversation conversation, ExtendedChatMessageContent message, object parameter, CancellationToken cancellationToken = default)
        => SendForDetailsAsync<T>(conversation, message, TextHelper.ToParameter(parameter), cancellationToken);

    /// <summary>
    /// Sends a question message and gets the details information.
    /// </summary>
    /// <param name="conversation">The chat conversation.</param>
    /// <param name="message">The message content.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The details information of processing and response.</returns>
    /// <exception cref="InvalidOperationException">Cannot send the message.</exception>
    /// <exception cref="NotSupportedException">Sending action is not available.</exception>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    /// <exception cref="ArgumentNullException">conversation was null.</exception>
    /// <exception cref="ArgumentException">The request message is not valid.</exception>
    protected Task<T> SendForDetailsAsync<T>(ExtendedChatConversation conversation, ExtendedChatMessageContent message, CancellationToken cancellationToken = default)
        => SendForDetailsAsync<T>(conversation, message, new ExtendedChatMessageParameter(), cancellationToken);

    /// <summary>
    /// Creates the chat message context.
    /// </summary>
    /// <param name="message">The chat message.</param>
    /// <param name="conversation">The context.</param>
    /// <param name="changing">The changing action type.</param>
    /// <param name="parameter">The additional parameter.</param>
    /// <returns>The chat message context.</returns>
    protected virtual ExtendedChatMessageContext CreateContext(ExtendedChatMessage message, ExtendedChatConversation conversation, ChangeMethods changing, object parameter)
        => new(message, conversation, changing, parameter);

    /// <summary>
    /// Creates a chat conversation.
    /// </summary>
    /// <param name="entity">The entity bound to the conversation.</param>
    /// <returns>The conversation.</returns>
    protected ExtendedChatConversation CreateConversation(BaseResourceEntityInfo entity)
        => entity == null ? null : new(this, entity);

    /// <summary>
    /// Creates a chat conversation.
    /// </summary>
    /// <param name="entities">The entities bound to the conversation.</param>
    /// <returns>The conversation.</returns>
    protected IEnumerable<ExtendedChatConversation> CreateConversation(IEnumerable<BaseResourceEntityInfo> entities)
    {
        if (entities == null) yield break;
        foreach (var item in entities)
        {
            if (item != null) yield return new(this, item);
        }
    }

    /// <summary>
    /// Creates a chat conversation.
    /// </summary>
    /// <param name="entities">The entities bound to the conversation.</param>
    /// <returns>The conversation.</returns>
    protected async IAsyncEnumerable<ExtendedChatConversation> CreateConversation(IAsyncEnumerable<BaseResourceEntityInfo> entities)
    {
        if (entities == null) yield break;
        await foreach (var item in entities)
        {
            if (item != null) yield return new(this, item);
        }
    }

    /// <summary>
    /// Tests if the specific message is just sent by the sender within given duration.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="duration">The durtion.</param>
    /// <returns>true if it was sent by the sender within the given duration; otherwise, false.</returns>
    protected bool IsLatestMessageBySender(ExtendedChatMessage message, TimeSpan duration)
        => message != null && message?.Sender == Sender && (DateTime.Now - message.CreationTime) <= duration;

    /// <summary>
    /// Lists all conversations.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>All conversations loaded.</returns>
    protected abstract IAsyncEnumerable<ExtendedChatConversation> ListConversationsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a message.
    /// </summary>
    /// <param name="context">The current chat message context.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The message sent.</returns>
    /// <exception cref="NotSupportedException">The sending action is not supported.</exception>
    protected abstract Task<ExtendedChatMessageSendResult> SendAsync(ExtendedChatMessageContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Modifies a message.
    /// </summary>
    /// <param name="context">The current chat message context.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The result of saving action.</returns>
    /// <exception cref="NotSupportedException">The modification action is not supported.</exception>
    protected abstract Task<ExtendedChatMessageSendResult> UpdateAsync(ExtendedChatMessageContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a message.
    /// </summary>
    /// <param name="context">The current chat message context.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The result of saving action.</returns>
    /// <exception cref="NotSupportedException">The deletion action is not supported.</exception>
    protected abstract Task<ExtendedChatMessageSendResult> DeleteAsync(ExtendedChatMessageContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Occurs on send succeeded.
    /// </summary>
    /// <param name="context">The current chat message context.</param>
    /// <param name="resp">The result of saving action.</param>
    /// <returns></returns>
    protected virtual void OnSendSucceeded(ExtendedChatMessageContext context, ExtendedChatMessageSendResult resp)
    {
    }

    /// <summary>
    /// Occurs on send succeeded.
    /// </summary>
    /// <param name="context">The current chat message context.</param>
    /// <param name="resp">The result of saving action.</param>
    /// <returns></returns>
    protected virtual Task OnSendSucceededAsync(ExtendedChatMessageContext context, ExtendedChatMessageSendResult resp)
        => Task.CompletedTask;

    /// <summary>
    /// Occurs on send failed.
    /// </summary>
    /// <param name="context">The current chat message context.</param>
    /// <param name="resp">The result of saving action.</param>
    /// <returns></returns>
    protected virtual void OnSendFailed(ExtendedChatMessageContext context, ExtendedChatMessageSendResult resp)
    {
    }

    /// <summary>
    /// Occurs on send failed.
    /// </summary>
    /// <param name="context">The current chat message context.</param>
    /// <param name="resp">The result of saving action.</param>
    /// <returns></returns>
    protected virtual void OnSendCanceled(ExtendedChatMessageContext context, ExtendedChatMessageSendResult resp)
    {
    }

    /// <summary>
    /// Occurs on send failed.
    /// </summary>
    /// <param name="context">The current chat message context.</param>
    /// <param name="resp">The result of saving action.</param>
    /// <returns></returns>
    protected virtual Task OnSendFailedAsync(ExtendedChatMessageContext context, ExtendedChatMessageSendResult resp)
        => Task.CompletedTask;

    /// <summary>
    /// Occurs on cleaning up about the sending state updates failed.
    /// </summary>
    /// <param name="context">The current chat message context.</param>
    protected virtual void OnSendStateUpdateError(ExtendedChatMessageContext context)
    {
    }

    /// <summary>
    /// Adds a set of conversation to the cache.
    /// </summary>
    /// <param name="conversations">The conversations to add.</param>
    protected void AddConversation(IEnumerable<ExtendedChatConversation> conversations)
    {
        if (conversations == null) return;
        foreach (var conversation in conversations)
        {
            if (conversation?.Id == null || Conversations.Contains(conversation)) continue;
            Conversations.Add(conversation);
        }
    }

    /// <summary>
    /// Adds a conversation to the cache.
    /// </summary>
    /// <param name="conversation">The conversation to add.</param>
    protected void AddConversation(ExtendedChatConversation conversation)
    {
        if (conversation == null) return;
        if (conversation?.Id == null || Conversations.Contains(conversation)) return;
        Conversations.Add(conversation);
    }

    /// <summary>
    /// Removes a set of convsersation from the cache.
    /// </summary>
    /// <param name="conversations">The conversations to remove.</param>
    protected int RemoveConversation(IEnumerable<ExtendedChatConversation> conversations)
    {
        var i = 0;
        if (conversations == null) return i;
        foreach (var conversation in conversations)
        {
            if (conversation == null) continue;
            if (Conversations.Remove(conversation)) i++;
        }

        return i;
    }

    /// <summary>
    /// Removes a convsersation from the cache.
    /// </summary>
    /// <param name="conversation">The conversation to remove.</param>
    protected bool RemoveConversation(ExtendedChatConversation conversation)
    {
        if (conversation == null) return false;
        return Conversations.Remove(conversation);
    }

    /// <summary>
    /// Sorts a given conversation in the collection cache.
    /// </summary>
    /// <param name="conversation">The conversation item to sort its order in the collection cache.</param>
    protected void SortConversation(ExtendedChatConversation conversation)
    {
        var last = conversation?.History?.LastOrDefault();
        if (last == null) return;
        var i = 0;
        try
        {
            for (; i < Conversations.Count; i++)
            {
                var item = Conversations[i];
                var testItem = item?.History?.LastOrDefault();
                if (testItem == null) continue;
                if (testItem.CreationTime < last.CreationTime) break;
                if (testItem.CreationTime == last.CreationTime && item == conversation) return;
            }

            var j = Conversations.IndexOf(conversation);
            if (i == j) return;
            else if (j < 0) Conversations.Insert(i, conversation);
            else Conversations.Move(j, i);
        }
        catch (ArgumentException)
        {
        }
        catch (InvalidOperationException)
        {
        }
    }

    /// <summary>
    /// Adds a set of messages to the specific conversation.
    /// </summary>
    /// <param name="conversation">The chat conversation.</param>
    /// <param name="messages">The messages to add.</param>
    protected void AddHistory(ExtendedChatConversation conversation, IEnumerable<ExtendedChatMessage> messages)
    {
        if (messages == null || conversation == null) return;
        messages = messages.Where(m => m != null).OrderByDescending(m => m.CreationTime);
        foreach (var message in messages)
        {
            if (message == null || message.OwnerId != conversation.Id || conversation.History.Contains(message)) continue;
            conversation.History.Insert(0, message);
        }
    }

    /// <summary>
    /// Adds a set of messages to the specific conversation.
    /// </summary>
    /// <param name="messages">The messages to add.</param>
    protected void AddHistory(IEnumerable<ExtendedChatMessage> messages)
    {
        if (messages == null) return;
        messages = messages.Where(m => m != null).OrderByDescending(m => m.CreationTime);
        foreach (var message in messages)
        {
            AddHistory(message);
        }
    }

    /// <summary>
    /// Adds a message to the specific conversation.
    /// </summary>
    /// <param name="message">The message to add.</param>
    protected void AddHistory(ExtendedChatMessage message)
    {
        if (message?.OwnerId == null) return;
        var conversation = GetConversation(message);
        if (conversation == null || conversation.History.Contains(message)) return;
        if (conversation.History.Count < 1)
        {
            conversation.History.Add(message);
            return;
        }

        var latest = conversation.History.LastOrDefault();
        if (latest == null || latest.CreationTime > message.CreationTime) conversation.History.Insert(0, message);
        conversation.History.Add(message);
    }

    /// <summary>
    /// Adds a message to the specific conversation.
    /// </summary>
    /// <param name="message">The message to add.</param>
    /// <param name="last">true if append at last; otherwise, false.</param>
    protected void AddMessage(ExtendedChatMessage message, bool last)
    {
        if (message?.OwnerId == null) return;
        var conversation = GetConversation(message);
        if (conversation == null || conversation.History.Contains(message)) return;
        if (conversation.History.Count < 1 || last)
        {
            conversation.History.Add(message);
            SortConversation(conversation);
            return;
        }

        var latest = conversation.History.LastOrDefault();
        if (latest != null && latest.CreationTime <= message.CreationTime)
        {
            conversation.History.Add(message);
            return;
        }

        for (var i = conversation.History.Count - 2; i >= 0; i--)
        {
            if (conversation.History[i] == null) continue;
            if (conversation.History[i].CreationTime <= message.CreationTime) break;
            conversation.History.Insert(i + 1, message);
            SortConversation(conversation);
            return;
        }

        conversation.History.Insert(0, message);
        SortConversation(conversation);
    }

    /// <summary>
    /// Adds a message to the specific conversation.
    /// </summary>
    /// <param name="message">The message to add.</param>
    protected void AddMessage(ExtendedChatMessage message)
        => AddMessage(message, false);
}

/// <summary>
/// The internal extended chat client.
/// </summary>
/// <param name="sender">The sender.</param>
/// <param name="canSend">true if it is available to send message; otherwise, false.</param>
internal sealed class InternalExtendedChatClient(BaseUserItemInfo sender, bool canSend) : BaseExtendedChatClient(sender)
{
    /// <inheritdoc />
    public override Task<bool> CanSendAsync(ExtendedChatConversation conversation, CancellationToken cancellationToken = default)
        => Task.FromResult(canSend);

    /// <inheritdoc />
    public override ExtendedChatMessageManagementLevels GetManageLevel(ExtendedChatConversation conversation, ExtendedChatMessage message)
        => message.Sender == conversation.Sender ? ExtendedChatMessageManagementLevels.All : ExtendedChatMessageManagementLevels.None;

    /// <inheritdoc />
    protected override async IAsyncEnumerable<ExtendedChatConversation> ListConversationsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        yield break;
    }

    /// <inheritdoc />
    public override Task LoadHistoryAsync(ExtendedChatConversation conversation, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <inheritdoc />
    protected override Task<ExtendedChatMessageSendResult> SendAsync(ExtendedChatMessageContext context, CancellationToken cancellationToken = default)
        => GetSendResultAsync();

    /// <inheritdoc />
    protected override Task<ExtendedChatMessageSendResult> UpdateAsync(ExtendedChatMessageContext context, CancellationToken cancellationToken = default)
        => GetSendResultAsync();

    /// <inheritdoc />
    protected override Task<ExtendedChatMessageSendResult> DeleteAsync(ExtendedChatMessageContext context, CancellationToken cancellationToken = default)
        => GetSendResultAsync();

    private ExtendedChatMessageSendResult GetSendResult()
        => new(canSend ? ExtendedChatMessageSendResultStates.Success : ExtendedChatMessageSendResultStates.Unsupported);

    private Task<ExtendedChatMessageSendResult> GetSendResultAsync()
        => Task.FromResult(GetSendResult());
}
