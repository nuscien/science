using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Authentication;
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
    private readonly IExtendedChatConversationProvider provider;

    /// <summary>
    /// Initializes a new instance of the ExtendedChatConversation class.
    /// </summary>
    /// <param name="provider">The chat conversation provider.</param>
    /// <param name="sender">The sender.</param>
    /// <param name="source">The conversation source.</param>
    /// <param name="parameter">The parameter used by the client to store something.</param>
    public ExtendedChatConversation(IExtendedChatConversationProvider provider, BaseUserItemInfo sender, BaseResourceEntityInfo source, object parameter = null)
    {
        this.provider = provider;
        SetProperty(nameof(Availability), ExtendedChatMessageAvailability.Testing);
        SetProperty(nameof(Source), source);
        SetProperty(nameof(Sender), sender);
        Parameter = parameter;
        _ = GetAvailabilityAsync();
    }

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
    /// Gets or sets the history status.
    /// </summary>
    internal object HistoryStatus { get; set; }

    /// <summary>
    /// Gets or sets the parameter.
    /// </summary>
    protected internal object Parameter { get; }

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
    public BaseUserItemInfo Sender => GetCurrentProperty<BaseUserItemInfo>();

    /// <summary>
    /// Gets the state about if the user can send message.
    /// </summary>
    public ExtendedChatMessageAvailability Availability
    {
        get
        {
            var b = GetProperty<ExtendedChatMessageAvailability>("ServiceAvailability");
            if (b != ExtendedChatMessageAvailability.Allowed) return b;
            return GetProperty<ExtendedChatMessageAvailability>("TempAvailability");
        }
    }

    /// <summary>
    /// Gets a value indicating whether current conversation is avalaible to send message.
    /// </summary>
    public bool CanSend => GetCurrentProperty<bool>();

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
    /// <param name="parameter">The additional parameter.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The message sent.</returns>
    /// <exception cref="ExtendedChatMessageAvailabilityException">Sending action is not available.</exception>
    /// <exception cref="ExtendedChatMessageException">Send the message failed.</exception>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    /// <exception cref="ArgumentNullException">conversation was null.</exception>
    public async Task<ExtendedChatMessage> SendAsync(ExtendedChatMessageContent message, ExtendedChatMessageParameter parameter, CancellationToken cancellationToken = default)
    {
        if (message == null) return null;
        ThrowIfCannotSend();
        parameter ??= new();
        var msg = new ExtendedChatMessage(Guid.NewGuid(), this, Sender, message, null)
        {
            State = ResourceEntityStates.Publishing
        };
        History.Add(msg);
        parameter.SendStatus = ExtendedChatMessageSendResultStates.NotSend;
        parameter.Begin(msg, ChangeMethods.Add);
        msg.UpdateSavingStatus(ResourceEntitySavingStates.Saving);
        ExtendedChatMessageSendResult resp;
        var context = CreateContextInternal(msg, ChangeMethods.Add, parameter.Parameter) ?? new(msg, this, ChangeMethods.Add, parameter.Parameter);
        Sending?.Invoke(this, new(msg));
        msg.State = ResourceEntityStates.Normal;
        ThrowIfCannotSend(msg);
        context.CanSetDetails = true;
        try
        {
            resp = await SendInternalAsync(context, cancellationToken) ?? new();
            context.CanSetDetails = false;
        }
        catch (OperationCanceledException ex)
        {
            context.CanSetDetails = false;
            UpdateSavingStatus(ex, context, parameter, ExtendedChatMessageSendResultStates.Aborted, out resp);
            History.Remove(msg);
            parameter.Details = context.Details;
            parameter.Cancel();
            OnSendCanceled(context, resp);
            SendCanceled?.Invoke(this, new(msg));
            throw;
        }
        catch (Exception ex)
        {
            context.CanSetDetails = false;
            UpdateSavingStatus(ex, context, parameter, ExtendedChatMessageSendResultStates.OtherError, out resp);
            History.Remove(msg);
            parameter.Details = context.Details;
            parameter.End(false);
            OnSendFailed(context, resp);
            _ = OnSendFailedAsync(context, resp);
            SendFailed?.Invoke(this, new(msg));
            if (resp.ThrowException(ex, context)) throw;
            return msg;
        }

        parameter.Details = context.Details;
        UpdateSavingStatus(context, parameter, resp);
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
    /// <param name="message">The message content.</param>
    /// <param name="parameter">The additional parameter.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The message sent.</returns>
    /// <exception cref="ExtendedChatMessageAvailabilityException">Sending action is not available.</exception>
    /// <exception cref="ExtendedChatMessageException">Send the message failed.</exception>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    /// <exception cref="ArgumentNullException">conversation was null.</exception>
    public Task<ExtendedChatMessage> SendAsync(ExtendedChatMessageContent message, object parameter, CancellationToken cancellationToken = default)
        => SendAsync(message, TextHelper.ToParameter(parameter), cancellationToken);

    /// <summary>
    /// Sends a message.
    /// </summary>
    /// <param name="message">The message content.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The message sent.</returns>
    /// <exception cref="ExtendedChatMessageAvailabilityException">Sending action is not available.</exception>
    /// <exception cref="ExtendedChatMessageException">Send the message failed.</exception>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    /// <exception cref="ArgumentNullException">conversation was null.</exception>
    public Task<ExtendedChatMessage> SendAsync(ExtendedChatMessageContent message, CancellationToken cancellationToken = default)
        => SendAsync(message, new ExtendedChatMessageParameter(), cancellationToken);

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
    public async Task<ExtendedChatMessageSendResult> ModifyAsync(ExtendedChatMessage original, string message, ExtendedChatMessageParameter parameter, CancellationToken cancellationToken = default)
    {
        AssertSender();
        var level = GetManageLevel(original);
        if (!level.HasFlag(ExtendedChatMessageManagementLevels.Modification)) throw new ExtendedChatMessageAvailabilityException(ExtendedChatMessageAvailability.Disabled, "Update is not supported.", new UnauthorizedAccessException("No permission to update message."));
        parameter ??= new();
        var old = original.Message;
        original.Message = message;
        ExtendedChatMessageSendResult resp;
        original.UpdateSavingStatus(ResourceEntitySavingStates.Saving);
        parameter.SendStatus = ExtendedChatMessageSendResultStates.Pending;
        parameter.Begin(original, ChangeMethods.Remove);
        Modifying?.Invoke(this, new(original));
        var context = CreateContextInternal(original, ChangeMethods.MemberModify, parameter.Parameter) ?? new(original, this, ChangeMethods.MemberModify, parameter.Parameter);
        context.CanSetDetails = true;
        try
        {
            resp = await UpdateInternalAsync(context, cancellationToken) ?? new();
            context.CanSetDetails = false;
        }
        catch (OperationCanceledException ex)
        {
            context.CanSetDetails = false;
            original.Message = old;
            UpdateSavingStatus(ex, context, parameter, ExtendedChatMessageSendResultStates.Aborted, out resp);
            parameter.Details = context.Details;
            parameter.Cancel();
            ModifyCanceled?.Invoke(this, new(original));
            throw;
        }
        catch (Exception ex)
        {
            context.CanSetDetails = false;
            original.Message = old;
            UpdateSavingStatus(ex, context, parameter, ExtendedChatMessageSendResultStates.OtherError, out resp);
            parameter.Details = context.Details;
            parameter.End(false);
            ModifyFailed?.Invoke(this, new(original));
            if (resp.ThrowException(ex, context)) throw;
            return resp;
        }

        parameter.Details = context.Details;
        if (resp.IsSuccessful) original.Message = old;
        UpdateSavingStatus(context, parameter, resp);
        parameter.End(resp.IsSuccessful);
        if (resp.IsSuccessful) Modified?.Invoke(this, new(original));
        else ModifyFailed?.Invoke(this, new(original));
        return resp;
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
        => ModifyAsync(original, message, TextHelper.ToParameter(parameter), cancellationToken);

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
        var entity = GetMessage(id);
        if (entity != null) return await DeleteAsync(entity, parameter, cancellationToken);
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
    /// <param name="message">The message to delete.</param>
    /// <param name="parameter">The additional parameter.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The result of saving action.</returns>
    /// <exception cref="NotSupportedException">The deletion action is not supported.</exception>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    public async Task<ExtendedChatMessageSendResult> DeleteAsync(ExtendedChatMessage message, ExtendedChatMessageParameter parameter, CancellationToken cancellationToken = default)
    {
        AssertSender();
        var level = GetManageLevel(message);
        if (!level.HasFlag(ExtendedChatMessageManagementLevels.Deletion)) throw new ExtendedChatMessageAvailabilityException(ExtendedChatMessageAvailability.Disabled, "Cannot delete the message.", new UnauthorizedAccessException("No permission to delete the message."));
        parameter ??= new();
        var history = History;
        ExtendedChatMessageSendResult resp;
        message.UpdateSavingStatus(ResourceEntitySavingStates.Saving);
        parameter.SendStatus = ExtendedChatMessageSendResultStates.Pending;
        parameter.Begin(message, ChangeMethods.Remove);
        Deleting?.Invoke(this, new(message));
        var context = CreateContextInternal(message, ChangeMethods.Remove, parameter.Parameter) ?? new(message, this, ChangeMethods.Remove, parameter.Parameter);
        context.CanSetDetails = true;
        try
        {
            resp = await DeleteInternalAsync(context, cancellationToken) ?? new();
            context.CanSetDetails = false;
        }
        catch (OperationCanceledException ex)
        {
            context.CanSetDetails = false;
            UpdateSavingStatus(ex, context, parameter, ExtendedChatMessageSendResultStates.Aborted, out resp);
            parameter.Details = context.Details;
            parameter.Cancel();
            DeleteCanceled?.Invoke(this, new(message));
            throw;
        }
        catch (Exception ex)
        {
            context.CanSetDetails = false;
            UpdateSavingStatus(ex, context, parameter, ExtendedChatMessageSendResultStates.OtherError, out resp);
            parameter.Details = context.Details;
            parameter.End(false);
            DeleteFailed?.Invoke(this, new(message));
            if (resp.ThrowException(ex, context)) throw;
            return resp;
        }

        parameter.Details = context.Details;
        if (resp.IsSuccessful)
        {
            history.Remove(message);
            if (message.State == ResourceEntityStates.Normal) message.State = ResourceEntityStates.Deleted;
        }

        UpdateSavingStatus(context, parameter, resp);
        parameter.End(resp.IsSuccessful);
        if (resp.IsSuccessful) Deleted?.Invoke(this, new(message));
        else DeleteFailed?.Invoke(this, new(message));
        return resp;
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
        => DeleteAsync(message, TextHelper.ToParameter(parameter), cancellationToken);

    /// <summary>
    /// Deletes a message.
    /// </summary>
    /// <param name="message">The message to delete.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The result of saving action.</returns>
    /// <exception cref="NotSupportedException">The deletion action is not supported.</exception>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    public Task<ExtendedChatMessageSendResult> DeleteAsync(ExtendedChatMessage message, CancellationToken cancellationToken = default)
        => DeleteAsync(message, new ExtendedChatMessageParameter(), cancellationToken);

    /// <summary>
    /// Tests if current conversation is avalaible to send message.
    /// </summary>
    /// <returns>The state about if the user can send message.</returns>
    public async Task<ExtendedChatMessageAvailability> GetAvailabilityAsync()
    {
        if (Sender is null) return ExtendedChatMessageAvailability.Unauthorized;
        try
        {
            var b = await GetServiceAvailabilityInternalAsync();
            SetServiceAvailability(b);
            return b;
        }
        catch (ArgumentException)
        {
        }
        catch (UnauthorizedAccessException)
        {
            SetServiceAvailability(ExtendedChatMessageAvailability.Unauthorized);
            return ExtendedChatMessageAvailability.Unauthorized;
        }
        catch (AuthenticationException)
        {
            SetServiceAvailability(ExtendedChatMessageAvailability.Unauthorized);
            return ExtendedChatMessageAvailability.Unauthorized;
        }
        catch (NullReferenceException)
        {
        }
        catch (InvalidOperationException)
        {
        }
        catch (NotSupportedException)
        {
        }
        catch (NotImplementedException)
        {
        }
        catch (ApplicationException)
        {
        }
        catch (ExternalException)
        {
        }

        SetServiceAvailability(ExtendedChatMessageAvailability.NotSupported);
        return ExtendedChatMessageAvailability.NotSupported;
    }

    /// <summary>
    /// Loads the history of the conversation.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the completion of history loading.</returns>
    public async Task LoadHistoryAsync(CancellationToken cancellationToken = default)
    {
        if (Sender is null) return;
        var context = new ExtendedChatMessageHistoryContext(this);
        await LoadHistoryAsync(context, cancellationToken);
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
    /// Sets the service availability.
    /// </summary>
    /// <param name="value">The availability about if the service is available to send message.</param>
    protected void SetServiceAvailability(ExtendedChatMessageAvailability value)
    {
        SetProperty("ServiceAvailability", value);
        UpdateIfIsAllowedToSend();
    }

    /// <summary>
    /// Sets the service availability.
    /// </summary>
    /// <param name="value">true if the service is available to send message; otherwise, false.</param>
    protected void SetServiceAvailability(bool value)
        => SetServiceAvailability(value ? ExtendedChatMessageAvailability.Allowed : ExtendedChatMessageAvailability.Disabled);

    /// <summary>
    /// Sends a question message and gets the details information.
    /// </summary>
    /// <param name="message">The message content.</param>
    /// <param name="parameter">The additional parameter.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The details information of processing and response.</returns>
    /// <exception cref="ExtendedChatMessageAvailabilityException">Sending action is not available.</exception>
    /// <exception cref="ExtendedChatMessageException">Send the message failed.</exception>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    /// <exception cref="ArgumentNullException">conversation was null.</exception>
    /// <exception cref="InvalidCastException">Get details information failed.</exception>
    protected async Task<T> SendForDetailsAsync<T>(ExtendedChatMessageContent message, ExtendedChatMessageParameter parameter, CancellationToken cancellationToken = default)
    {
        parameter ??= new();
        var hasDetails = parameter.Details is not null;
        try
        {
            await SendAsync(message, parameter, cancellationToken);
        }
        catch (ExtendedChatMessageAvailabilityException)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (OutOfMemoryException)
        {
            throw;
        }
        catch (Exception)
        {
            if (hasDetails || parameter.Details is null) throw;
        }

        if (ObjectConvert.TryGet(parameter.Details, out T response)) return response;
        throw new InvalidCastException(parameter.Details is null
            ? "Get details information failed because it is null."
            : $"Get details information failed because its type is not the expected one. Its actual type is {parameter.Details.GetType().Name}.");
    }

    /// <summary>
    /// Sends a question message and gets the details information.
    /// </summary>
    /// <param name="message">The message content.</param>
    /// <param name="parameter">The additional parameter.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The details information of processing and response.</returns>
    /// <exception cref="ExtendedChatMessageAvailabilityException">Sending action is not available.</exception>
    /// <exception cref="ExtendedChatMessageException">Send the message failed.</exception>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    /// <exception cref="ArgumentNullException">conversation was null.</exception>
    /// <exception cref="InvalidCastException">Get details information failed.</exception>
    protected Task<T> SendForDetailsAsync<T>(ExtendedChatMessageContent message, object parameter, CancellationToken cancellationToken = default)
        => SendForDetailsAsync<T>(message, TextHelper.ToParameter(parameter), cancellationToken);

    /// <summary>
    /// Sends a question message and gets the details information.
    /// </summary>
    /// <param name="message">The message content.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The details information of processing and response.</returns>
    /// <exception cref="ExtendedChatMessageAvailabilityException">Sending action is not available.</exception>
    /// <exception cref="ExtendedChatMessageException">Send the message failed.</exception>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    /// <exception cref="ArgumentNullException">conversation was null.</exception>
    /// <exception cref="InvalidCastException">Get details information failed.</exception>
    protected Task<T> SendForDetailsAsync<T>(ExtendedChatMessageContent message, CancellationToken cancellationToken = default)
        => SendForDetailsAsync<T>(message, new ExtendedChatMessageParameter(), cancellationToken);

    /// <summary>
    /// Tests if the specific message is just sent by the sender within given duration.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="duration">The durtion.</param>
    /// <returns>true if it was sent by the sender within the given duration; otherwise, false.</returns>
    protected bool IsLatestMessageBySender(ExtendedChatMessage message, TimeSpan duration)
        => message != null && message?.Sender == Sender && (DateTime.Now - message.CreationTime) <= duration;

    /// <summary>
    /// Sends a message.
    /// </summary>
    /// <param name="context">The current chat message context.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The message sent.</returns>
    /// <exception cref="ExtendedChatMessageAvailabilityException">Sending action is not available.</exception>
    /// <exception cref="ExtendedChatMessageException">Send the message failed.</exception>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    internal virtual Task<ExtendedChatMessageSendResult> SendInternalAsync(ExtendedChatMessageContext context, CancellationToken cancellationToken = default)
    {
        if (provider is null) return Task.FromResult(new ExtendedChatMessageSendResult(ExtendedChatMessageSendResultStates.ClientError, "No implementation."));
        return provider.SendAsync(context, cancellationToken);
    }

    /// <summary>
    /// Modifies a message.
    /// </summary>
    /// <param name="context">The current chat message context.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The result of saving action.</returns>
    /// <exception cref="ExtendedChatMessageAvailabilityException">Sending action is not available.</exception>
    /// <exception cref="ExtendedChatMessageException">Send the message failed.</exception>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    internal virtual Task<ExtendedChatMessageSendResult> UpdateInternalAsync(ExtendedChatMessageContext context, CancellationToken cancellationToken = default)
    {
        if (provider is null) return Task.FromResult(new ExtendedChatMessageSendResult(ExtendedChatMessageSendResultStates.ClientError, "No implementation."));
        return provider.UpdateAsync(context, cancellationToken);
    }

    /// <summary>
    /// Deletes a message.
    /// </summary>
    /// <param name="context">The current chat message context.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The result of saving action.</returns>
    /// <exception cref="ExtendedChatMessageAvailabilityException">Sending action is not available.</exception>
    /// <exception cref="ExtendedChatMessageException">Send the message failed.</exception>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    internal virtual Task<ExtendedChatMessageSendResult> DeleteInternalAsync(ExtendedChatMessageContext context, CancellationToken cancellationToken = default)
    {
        if (provider is null) return Task.FromResult(new ExtendedChatMessageSendResult(ExtendedChatMessageSendResultStates.ClientError, "No implementation."));
        return provider.DeleteAsync(context, cancellationToken);
    }

    /// <summary>
    /// Tests if current conversation is avalaible to send message.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The state about if the user can send message.</returns>
    internal virtual Task<ExtendedChatMessageAvailability> GetServiceAvailabilityInternalAsync(CancellationToken cancellationToken = default)
        => provider is null ? Task.FromResult(ExtendedChatMessageAvailability.NotSupported) : provider.GetServiceAvailabilityAsync(this, cancellationToken);

    /// <summary>
    /// Creates the chat message context.
    /// </summary>
    /// <param name="message">The chat message.</param>
    /// <param name="changing">The changing action type.</param>
    /// <param name="parameter">The additional parameter.</param>
    /// <returns>The chat message context.</returns>
    internal virtual ExtendedChatMessageContext CreateContextInternal(ExtendedChatMessage message, ChangeMethods changing, object parameter)
        => new(message, this, changing, parameter);

    /// <summary>
    /// Gets nagement level of a message in the conversation.
    /// </summary>
    /// <param name="message">The message</param>
    /// <returns>The management level.</returns>
    public virtual ExtendedChatMessageManagementLevels GetManageLevel(ExtendedChatMessage message)
        => provider is null ? ExtendedChatMessageManagementLevels.None : provider.GetManageLevel(message);

    /// <summary>
    /// Loads the history of the conversation.
    /// </summary>
    /// <param name="context">The context to load history of the conversation.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the completion of history loading.</returns>
    protected virtual Task LoadHistoryAsync(ExtendedChatMessageHistoryContext context, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

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
    protected virtual void OnStateUpdateError(ExtendedChatMessageContext context)
    {
    }

    /// <summary>
    /// Adds a message to the specific conversation.
    /// </summary>
    /// <param name="message">The message to add.</param>
    /// <exception cref="ExtendedChatMessageAvailabilityException">conversation is not found.</exception>
    /// <exception cref="ArgumentNullException">conversation was null.</exception>
    protected void AddMessage(ExtendedChatMessage message)
        => AddMessage(message, false, null);

    /// <summary>
    /// Adds a message to the specific conversation.
    /// </summary>
    /// <param name="message">The message to add.</param>
    /// <param name="last">true if append at last; otherwise, false.</param>
    /// <param name="conversations">All conversations.</param>
    protected internal void AddMessage(ExtendedChatMessage message, bool last, ObservableCollection<ExtendedChatConversation> conversations = null)
    {
        if (message?.OwnerId == null) return;
        if (History.Contains(message)) return;
        if (History.Count < 1 || last)
        {
            History.Add(message);
            TextHelper.Add(conversations, this);
            return;
        }

        var latest = History.LastOrDefault();
        if (latest != null && latest.CreationTime <= message.CreationTime)
        {
            History.Add(message);
            TextHelper.Add(conversations, this);
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
    protected internal int AddHistory(IEnumerable<ExtendedChatMessage> messages)
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
    protected internal async Task<int> AddHistoryAsync(IAsyncEnumerable<ExtendedChatMessage> messages)
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

    /// <summary>
    /// Sets the flag of whether can send messages by service.
    /// </summary>
    /// <param name="state">The state about if the user can send message.</param>
    internal void SetTempAvailability(ExtendedChatMessageAvailability state)
    {
        SetProperty("TempAvailability", state);
        UpdateIfIsAllowedToSend();
    }

    /// <summary>
    /// Throws if the current state is not allowed to send message.
    /// </summary>
    /// <param name="msg">The message to send.</param>
    /// <exception cref="ExtendedChatMessageAvailabilityException">Cannot send the message.</exception>
    internal void ThrowIfCannotSend(ExtendedChatMessage msg = null)
    {
        AssertSender();
        var state = Availability;
        if (state == ExtendedChatMessageAvailability.Allowed) return;
        if (msg != null)
        {
            History.Remove(msg);
            msg.UpdateSavingStatus(ExtendedChatMessageSendResultStates.NotSend);
        }

        throw state switch
        {
            ExtendedChatMessageAvailability.Disabled => new ExtendedChatMessageAvailabilityException(state, "Cannot send message.", new UnauthorizedAccessException("Not allowed to send message.")),
            ExtendedChatMessageAvailability.Unauthorized => new ExtendedChatMessageAvailabilityException(state, "No permission to send message.", new UnauthorizedAccessException("Requires login by an authorized account.")),
            ExtendedChatMessageAvailability.ReadOnly => new ExtendedChatMessageAvailabilityException(state, "The conversation is read-only.", new InvalidOperationException("The conversation is read-only.")),
            ExtendedChatMessageAvailability.Disconnnected => new ExtendedChatMessageAvailabilityException(state, "The connection is not available."),
            ExtendedChatMessageAvailability.Throttle => new ExtendedChatMessageAvailabilityException(state, "Traffic limitation for connection.", new TaskCanceledException("Sending is canceled because traffic limitation for connection.")),
            ExtendedChatMessageAvailability.NotSupported => new ExtendedChatMessageAvailabilityException(state, "The conversation is not supported."),
            _ => new ExtendedChatMessageAvailabilityException(state, "Cannot send message.", new UnauthorizedAccessException("Not allowed to send message."))
        };
    }

    private void AssertSender()
    {
        if (Sender is null) throw new ExtendedChatMessageAvailabilityException(ExtendedChatMessageAvailability.Unauthorized, "Requires login.", new UnauthorizedAccessException("Requires login."));
    }

    private void UpdateIfIsAllowedToSend()
        => SetProperty(nameof(CanSend), Availability == ExtendedChatMessageAvailability.Allowed);

    private void UpdateSavingStatus(Exception ex, ExtendedChatMessageContext context, ExtendedChatMessageParameter parameter, ExtendedChatMessageSendResultStates fallbackState, out ExtendedChatMessageSendResult resp)
    {
        try
        {
            var message = context.Message;
            message.UpdateSavingStatus(ex, out resp);
            parameter.SendStatus = resp?.SendStatus ?? fallbackState;
        }
        catch (Exception)
        {
            OnStateUpdateError(context);
            throw;
        }
    }

    private void UpdateSavingStatus(ExtendedChatMessageContext context, ExtendedChatMessageParameter parameter, ExtendedChatMessageSendResult resp)
    {
        try
        {
            context.Message.UpdateSavingStatus(resp);
            parameter.SendStatus = resp.SendStatus;
        }
        catch (Exception)
        {
            OnStateUpdateError(context);
            throw;
        }
    }
}

/// <summary>
/// The chat message conversation, may be a user, group, bot or topic.
/// </summary>
/// <param name="sender">The sender.</param>
/// <param name="source">The conversation source.</param>
/// <param name="parameter">The parameter used by the client to store something.</param>
[Guid("386F1526-B6D5-4B5B-B59F-DF3ECA4C203A")]
public abstract class BaseExtendedChatConversation(BaseUserItemInfo sender, BaseResourceEntityInfo source, object parameter = null) : ExtendedChatConversation(null, sender, source, parameter)
{
    /// <inheritdoc />
    internal override Task<ExtendedChatMessageSendResult> SendInternalAsync(ExtendedChatMessageContext context, CancellationToken cancellationToken = default)
        => SendAsync(context, cancellationToken);

    /// <inheritdoc />
    internal override Task<ExtendedChatMessageSendResult> UpdateInternalAsync(ExtendedChatMessageContext context, CancellationToken cancellationToken = default)
        => UpdateInternalAsync(context, cancellationToken);

    /// <inheritdoc />
    internal override Task<ExtendedChatMessageSendResult> DeleteInternalAsync(ExtendedChatMessageContext context, CancellationToken cancellationToken = default)
        => DeleteInternalAsync(context, cancellationToken);

    /// <inheritdoc />
    internal override Task<ExtendedChatMessageAvailability> GetServiceAvailabilityInternalAsync(CancellationToken cancellationToken = default)
        => GetServiceAvailabilityAsync(cancellationToken);

    /// <summary>
    /// Sends a message.
    /// </summary>
    /// <param name="context">The current chat message context.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The message sent.</returns>
    /// <exception cref="ExtendedChatMessageAvailabilityException">Sending action is not available.</exception>
    /// <exception cref="ExtendedChatMessageException">Send the message failed.</exception>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    protected abstract Task<ExtendedChatMessageSendResult> SendAsync(ExtendedChatMessageContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Modifies a message.
    /// </summary>
    /// <param name="context">The current chat message context.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The result of saving action.</returns>
    /// <exception cref="ExtendedChatMessageAvailabilityException">Sending action is not available.</exception>
    /// <exception cref="ExtendedChatMessageException">Send the message failed.</exception>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    protected abstract Task<ExtendedChatMessageSendResult> UpdateAsync(ExtendedChatMessageContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a message.
    /// </summary>
    /// <param name="context">The current chat message context.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The result of saving action.</returns>
    /// <exception cref="ExtendedChatMessageAvailabilityException">Sending action is not available.</exception>
    /// <exception cref="ExtendedChatMessageException">Send the message failed.</exception>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    protected abstract Task<ExtendedChatMessageSendResult> DeleteAsync(ExtendedChatMessageContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests if current conversation is avalaible to send message.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The state about if the user can send message.</returns>
    protected abstract Task<ExtendedChatMessageAvailability> GetServiceAvailabilityAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// The chat message conversation, may be a user, group, bot or topic.
/// </summary>
/// <param name="sender">The sender.</param>
/// <param name="source">The conversation source.</param>
/// <param name="parameter">The parameter used by the client to store something.</param>
[Guid("C7D4A31D-7032-4708-AFBE-FF6F938FFB54")]
public abstract class BaseExtendedChatConversation<T>(BaseUserItemInfo sender, BaseResourceEntityInfo source, object parameter = null) : ExtendedChatConversation(null, sender, source, parameter)
    where T : ExtendedChatMessageContext
{
    /// <inheritdoc />
    internal override Task<ExtendedChatMessageSendResult> SendInternalAsync(ExtendedChatMessageContext context, CancellationToken cancellationToken = default)
        => SendAsync(context as T, cancellationToken);

    /// <inheritdoc />
    internal override Task<ExtendedChatMessageSendResult> UpdateInternalAsync(ExtendedChatMessageContext context, CancellationToken cancellationToken = default)
        => UpdateInternalAsync(context as T, cancellationToken);

    /// <inheritdoc />
    internal override Task<ExtendedChatMessageSendResult> DeleteInternalAsync(ExtendedChatMessageContext context, CancellationToken cancellationToken = default)
        => DeleteInternalAsync(context as T, cancellationToken);

    /// <inheritdoc />
    internal override Task<ExtendedChatMessageAvailability> GetServiceAvailabilityInternalAsync(CancellationToken cancellationToken = default)
        => GetServiceAvailabilityAsync(cancellationToken);

    /// <inheritdoc />
    internal override ExtendedChatMessageContext CreateContextInternal(ExtendedChatMessage message, ChangeMethods changing, object parameter)
        => CreateContext(message, changing, parameter);

    /// <summary>
    /// Sends a message.
    /// </summary>
    /// <param name="context">The current chat message context.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The message sent.</returns>
    /// <exception cref="ExtendedChatMessageAvailabilityException">Sending action is not available.</exception>
    /// <exception cref="ExtendedChatMessageException">Send the message failed.</exception>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    protected abstract Task<ExtendedChatMessageSendResult> SendAsync(T context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Modifies a message.
    /// </summary>
    /// <param name="context">The current chat message context.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The result of saving action.</returns>
    /// <exception cref="ExtendedChatMessageAvailabilityException">Sending action is not available.</exception>
    /// <exception cref="ExtendedChatMessageException">Send the message failed.</exception>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    protected abstract Task<ExtendedChatMessageSendResult> UpdateAsync(T context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a message.
    /// </summary>
    /// <param name="context">The current chat message context.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The result of saving action.</returns>
    /// <exception cref="ExtendedChatMessageAvailabilityException">Sending action is not available.</exception>
    /// <exception cref="ExtendedChatMessageException">Send the message failed.</exception>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    protected abstract Task<ExtendedChatMessageSendResult> DeleteAsync(T context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests if current conversation is avalaible to send message.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The state about if the user can send message.</returns>
    protected abstract Task<ExtendedChatMessageAvailability> GetServiceAvailabilityAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates the chat message context.
    /// </summary>
    /// <param name="message">The chat message.</param>
    /// <param name="changing">The changing action type.</param>
    /// <param name="parameter">The additional parameter.</param>
    /// <returns>The chat message context.</returns>
    internal abstract T CreateContext(ExtendedChatMessage message, ChangeMethods changing, object parameter);
}
