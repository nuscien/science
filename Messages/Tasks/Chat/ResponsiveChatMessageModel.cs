using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Trivial.Data;
using Trivial.Net;
using Trivial.Reflection;
using Trivial.Text;
using Trivial.Users;

namespace Trivial.Tasks;

/// <summary>
/// The change record of the turn-based chat message.
/// </summary>
/// <param name="state">The change state of the message.</param>
/// <param name="start">The date time that the record creates.</param>
/// <param name="note">The additional change note.</param>
[Guid("EC2F71A6-D222-4F93-A195-D0300086A50F")]
public sealed class ResponsiveChatMessageStateRecord(ResponsiveChatMessageStates state, DateTime start, string note)
{
    /// <summary>
    /// Gets the date time that the record creates.
    /// </summary>
    public DateTime StartTime { get; } = start;

    /// <summary>
    /// Gets the progress state of the message.
    /// </summary>
    public ResponsiveChatMessageStates State { get; } = state;

    /// <summary>
    /// Gets the additional change note.
    /// </summary>
    public string Note { get; } = note;

    /// <summary>
    /// Converts to JSON object.
    /// </summary>
    /// <returns>The JSON object node.</returns>
    public JsonObjectNode ToJson()
        => new()
        {
            { "create", StartTime },
            { "state", State.ToString() },
            { "note", Note }
        };
}

/// <summary>
/// The model of responsive chat message.
/// </summary>
[Guid("4E32DA41-A5F8-41C2-BA49-625C3A97ED54")]
public class ResponsiveChatMessageModel
{
    private readonly List<ResponsiveChatMessageStateRecord> records = new();

    /// <summary>
    /// Initializes a new instance of the ResponsiveChatMessageModel class.
    /// </summary>
    /// <param name="question">The question message.</param>
    /// <param name="answer">The answer message.</param>
    internal ResponsiveChatMessageModel(ExtendedChatMessage question, ExtendedChatMessage answer)
    {
        Question = question;
        Answer = answer;
        Records = records.AsReadOnly();
        SendTime = DateTime.Now;
    }

    /// <summary>
    /// Gets the question message.
    /// </summary>
    internal ExtendedChatMessage Question { get; }

    /// <summary>
    /// Gets or sets the answer message.
    /// </summary>
    internal ExtendedChatMessage Answer { get; }

    /// <summary>
    /// Gets the question message content.
    /// </summary>
    public string QuestionMessage => Question.Message;

    /// <summary>
    /// Gets the answer message content.
    /// </summary>
    public string AnswerMessage => Answer.Message;

    /// <summary>
    /// Gets or sets an additional tag.
    /// </summary>
    public object Tag { get; set; }

    /// <summary>
    /// Gets the date time that the message is sending.
    /// </summary>
    public DateTime SendTime { get; }

    /// <summary>
    /// Gets the date time that the message is starting to receive.
    /// </summary>
    public DateTime? ReceiveStartTime { get; private set; }

    /// <summary>
    /// Gets the date time that the message is received completed.
    /// </summary>
    public DateTime? ReceiveCompleteTime { get; private set; }

    /// <summary>
    /// Gets the action records.
    /// </summary>
    public IReadOnlyList<ResponsiveChatMessageStateRecord> Records { get; }

    /// <summary>
    /// Updates the state.
    /// </summary>
    /// <param name="value">The new state.</param>
    /// <param name="note">The additional note of the state.</param>
    /// <returns>The state change record.</returns>
    internal ResponsiveChatMessageStateRecord UpdateState(ResponsiveChatMessageStates value, string note = null)
    {
        var now = DateTime.Now;
        var record = new ResponsiveChatMessageStateRecord(value, DateTime.Now, note);
        records.Add(record);
        switch (value)
        {
            case ResponsiveChatMessageStates.Done:
            case ResponsiveChatMessageStates.Reject:
            case ResponsiveChatMessageStates.Abort:
                if (!ReceiveCompleteTime.HasValue) ReceiveCompleteTime = now;
                break;
            case ResponsiveChatMessageStates.IntentError:
            case ResponsiveChatMessageStates.ProcessFailure:
            case ResponsiveChatMessageStates.ResponseError:
                if (!ReceiveCompleteTime.HasValue) ReceiveCompleteTime = now;
                break;
            case ResponsiveChatMessageStates.Receiving:
                if (!ReceiveStartTime.HasValue) ReceiveStartTime = now;
                break;
        }

        return record;
    }
}

/// <summary>
/// The sending response of responsive chat message.
/// </summary>
[Guid("F1F8AAEE-AEC6-4210-9635-FF6FA8B5F6DD")]
public class ResponsiveChatMessageResponse
{
    private Task<ExtendedChatMessage> task;
    private Exception ex;
    private Action callback;

    /// <summary>
    /// Initializes a new instance of the ResponsiveChatMessageResponse class.
    /// </summary>
    /// <param name="task">The response receiving task.</param>
    /// <param name="question">The question message.</param>
    /// <param name="result">The sending result.</param>
    /// <param name="callback">The callback handler.</param>
    internal ResponsiveChatMessageResponse(Task<ExtendedChatMessage> task, ExtendedChatMessage question, ExtendedChatMessageSendResult result, Action callback)
    {
        this.task = task;
        this.callback = callback;
        Result = result;
        Question = question;
        _ = OnInitAsync();
    }

    /// <summary>
    /// Gets the sending result.
    /// </summary>
    public ExtendedChatMessageSendResult Result { get; }

    /// <summary>
    /// Gets the question message.
    /// </summary>
    public ExtendedChatMessage Question { get; }

    /// <summary>
    /// Gets the response.
    /// It returns null when pending, no response or error.
    /// </summary>
    public ExtendedChatMessage Answer { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the answer is received or error is catched.
    /// </summary>
    public bool IsReceived => task != null;

    /// <summary>
    /// Gets a value indicating whether an error is thrown during getting answer.
    /// </summary>
    public bool IsFailed => ex != null;

    /// <summary>
    /// Gets response.
    /// </summary>
    /// <returns>The response message.</returns>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    /// <exception cref="ResponsiveChatMessageException">Get repsonse failed.</exception>
    public async Task<ExtendedChatMessage> GetAnswerAsync()
    {
        var t = task;
        if (t == null)
        {
            if (ex != null) throw ex;
            return Answer;
        }

        try
        {
            Answer = await t;
            return Answer;
        }
        catch (Exception ex)
        {
            this.ex = ex;
            throw;
        }
        finally
        {
            task = null;
            var cb = callback;
            callback = null;
            cb?.Invoke();
        }
    }

    private async Task OnInitAsync()
    {
        await Task.CompletedTask;
        await GetAnswerAsync();
    }
}

/// <summary>
/// The intent info of the round chat message.
/// </summary>
[Guid("F3887EAE-9C80-487B-ACC3-D54C73689A40")]
public class ResponsiveChatMessageIntentInfo
{
    /// <summary>
    /// Initializes an instance of the ResponsiveChatMessageIntentInfo class.
    /// </summary>
    /// <param name="info">The intent info.</param>
    public ResponsiveChatMessageIntentInfo(JsonObjectNode info)
    {
        Info = info;
    }

    /// <summary>
    /// Initializes an instance of the ResponsiveChatMessageIntentInfo class.
    /// </summary>
    /// <param name="message">The message to return to user.</param>
    public ResponsiveChatMessageIntentInfo(string message)
        : this(true, message)
    {
    }

    /// <summary>
    /// Initializes an instance of the ResponsiveChatMessageIntentInfo class.
    /// </summary>
    /// <param name="skipIntent">true if skip intent; otherwise, false.</param>
    /// <param name="message">The message to return to user.</param>
    public ResponsiveChatMessageIntentInfo(bool skipIntent, string message)
    {
        SkipIntent = skipIntent;
        Message = message;
    }

    /// <summary>
    /// Gets a value indicating whether the intent should be skipped.
    /// </summary>
    public bool SkipIntent { get; }

    /// <summary>
    /// Gets the message instead of the intent request info.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the intent info.
    /// </summary>
    public JsonObjectNode Info { get; }

    /// <summary>
    /// Creates an instance with the info.
    /// </summary>
    /// <param name="json">The intent info.</param>
    public static implicit operator ResponsiveChatMessageIntentInfo(JsonObjectNode json)
    {
        if (json == null) return null;
        return new ResponsiveChatMessageIntentInfo(json);
    }
}
