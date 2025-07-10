using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Trivial.Data;
using Trivial.Net;
using Trivial.Reflection;
using Trivial.Text;

namespace Trivial.Tasks;

/// <summary>
/// The change record of the turn-based chat message.
/// </summary>
/// <param name="state">The change state of the message.</param>
/// <param name="start">The date time that the record creates.</param>
/// <param name="note">The additional change note.</param>
public sealed class RoundChatMessageStateRecord(RoundChatMessageStates state, DateTime start, string note)
{
    /// <summary>
    /// Gets the date time that the record creates.
    /// </summary>
    public DateTime StartTime { get; } = start;

    /// <summary>
    /// Gets the progress state of the message.
    /// </summary>
    public RoundChatMessageStates State { get; } = state;

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
/// The model of turn-based chat message, including request, response and additional data.
/// </summary>
[JsonConverter(typeof(JsonValueNodeConverter))]
public sealed class RoundChatMessageModel : IJsonObjectHost
{
    private string answer;
    private List<RoundChatMessageStateRecord> records = new();

    /// <summary>
    /// Initializing an instance of the RoundChatMessageModel class.
    /// </summary>
    /// <param name="question">The request message.</param>
    public RoundChatMessageModel(string question)
    {
        Id = Guid.NewGuid();
        Question = question;
        SendTime = DateTime.Now;
        Records = records.AsReadOnly();
    }

    /// <summary>
    /// Initializing an instance of the RoundChatMessageModel class.
    /// </summary>
    /// <param name="question">The request message.</param>
    public RoundChatMessageModel(ExtendedChatMessage question)
        : this(question?.Message)
    {
    }

    /// <summary>
    /// Initializing an instance of the RoundChatMessageModel class.
    /// </summary>
    /// <param name="json">The JSON object to convert.</param>
    public RoundChatMessageModel(JsonObjectNode json)
    {
        if (json == null) return;
        Id = json.TryGetGuidValue("id") ?? Guid.NewGuid();
        Question = json.TryGetStringValue("question");
        SendTime = json.TryGetDateTimeValue("sent") ?? DateTime.Now;
        State = json.TryGetEnumValue<RoundChatMessageStates>("state") ?? RoundChatMessageStates.Initialized;
        Answer = json.TryGetStringValue("answer");
        ReceiveStartTime = json.TryGetDateTimeValue("receiving");
        ReceiveCompleteTime = json.TryGetDateTimeValue("received");
        ErrorCode = json.TryGetStringValue("code");
        ErrorMessage = json.TryGetStringValue("error");
        Intent = json.TryGetObjectValue("intent");
        Data = json.TryGetObjectValue("data");
        var records = json.TryGetObjectListValue("records", true);
        if (records == null) return;
        foreach (var item in records)
        {
            var state = item.TryGetEnumValue<RoundChatMessageStates>("state");
            var creation = item.TryGetDateTimeValue("create");
            if (!state.HasValue || !creation.HasValue) continue;
            var record = new RoundChatMessageStateRecord(state.Value, creation.Value, item.TryGetStringTrimmedValue("note", true));
            this.records.Add(record);
        }
    }

    /// <summary>
    /// Initializing an instance of the RoundChatMessageModel class.
    /// </summary>
    /// <param name="id">The identifier of this message model.</param>
    /// <param name="question">The request message.</param>
    /// <param name="send">The date time that the message is sending.</param>
    /// <param name="state">The progress state of this message.</param>
    /// <param name="answer">The response message.</param>
    /// <param name="receiveStartTime">The date time that the message is starting to receive.</param>
    /// <param name="receiveCompleteTime">The data time that the message is received completed.</param>
    /// <param name="records">The history records of state.</param>
    public RoundChatMessageModel(Guid id, string question, DateTime send, RoundChatMessageStates state, string answer, DateTime? receiveStartTime, DateTime? receiveCompleteTime, List<RoundChatMessageStateRecord> records = null)
    {
        Id = id;
        Question = question;
        SendTime = send;
        State = state;
        Answer = answer;
        ReceiveStartTime = receiveStartTime;
        ReceiveCompleteTime = receiveCompleteTime;
        this.records = records;
        Records = records.AsReadOnly();
    }

    /// <summary>
    /// Adds or removes the event handler occurred on the value text of answer is updated.
    /// </summary>
    public event DataEventHandler<string> AnswerUpdated;

    /// <summary>
    /// Adds or removes the event handler occurred on the progress state is updated.
    /// </summary>
    public event DataEventHandler<RoundChatMessageStates> StateUpdated;

    /// <summary>
    /// Gets the identifier of this message model.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the request message.
    /// </summary>
    public string Question { get; }

    /// <summary>
    /// Gets the response message.
    /// </summary>
    public string Answer
    {
        get
        {
            return answer ?? string.Empty;
        }

        internal set
        {
            if (answer == value) return;
            answer = value;
            AnswerUpdated?.Invoke(this, answer);
        }
    }

    /// <summary>
    /// Gets or sets an additional tag.
    /// </summary>
    public object Tag { get; set; }

    /// <summary>
    /// Gets the progress state of this message.
    /// </summary>
    public RoundChatMessageStates State { get; private set; }

    /// <summary>
    /// Gets the intent data.
    /// </summary>
    public JsonObjectNode Intent { get; internal set; }

    /// <summary>
    /// Gets the business data.
    /// </summary>
    public JsonObjectNode Data { get; internal set; }

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
    /// Gets the error code.
    /// </summary>
    public string ErrorCode { get; set; }

    /// <summary>
    /// Gets a value indicating whether the message has an error code.
    /// </summary>
    public bool HasErrorCode => !string.IsNullOrWhiteSpace(ErrorCode);

    /// <summary>
    /// Gets the error message.
    /// </summary>
    public string ErrorMessage { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the message has an error message.
    /// </summary>
    public bool HasErrorMessage => !string.IsNullOrWhiteSpace(ErrorMessage);

    /// <summary>
    /// Gets a value indicating whether the message is pending to get final result.
    /// </summary>
    public bool IsWorking => State switch
    {
        RoundChatMessageStates.SendingIntent or RoundChatMessageStates.Intent or RoundChatMessageStates.Processing or RoundChatMessageStates.SendingMessage or RoundChatMessageStates.Waiting or RoundChatMessageStates.Receiving => true,
        _ => false
    };

    /// <summary>
    /// Gets a value indicating whether the message is received succeeded.
    /// </summary>
    public bool IsSuccessful => State == RoundChatMessageStates.Done;

    /// <summary>
    /// Gets a value indicating whether there is an error during getting the result.
    /// </summary>
    public bool IsError => State switch
    {
        RoundChatMessageStates.Reject or RoundChatMessageStates.Abort or RoundChatMessageStates.IntentError or RoundChatMessageStates.ProcessFailure or RoundChatMessageStates.ResponseError or RoundChatMessageStates.OtherError => true,
        _ => false
    };

    /// <summary>
    /// Gets a value indicating whether the message is before sending.
    /// </summary>
    public bool IsPreparing => State switch
    {
        RoundChatMessageStates.Initialized or RoundChatMessageStates.SendingIntent or RoundChatMessageStates.Intent or RoundChatMessageStates.Processing => true,
        _ => false
    };

    /// <summary>
    /// Gets a value indicating whether it is intent data resolving or action processing.
    /// </summary>
    public bool IsInIntentProgress => State switch
    {
        RoundChatMessageStates.SendingIntent or RoundChatMessageStates.Intent or RoundChatMessageStates.Processing => true,
        _ => false
    };

    /// <summary>
    /// Gets a value indicating whether it is in the progress that the client is sending data to or receiving data from web server.
    /// </summary>
    public bool IsInAgentProgress => State switch
    {
        RoundChatMessageStates.SendingIntent or RoundChatMessageStates.Intent or RoundChatMessageStates.SendingMessage or RoundChatMessageStates.Waiting or RoundChatMessageStates.Receiving => true,
        _ => false
    };

    /// <summary>
    /// Gets the history records of state.
    /// </summary>
    public IReadOnlyList<RoundChatMessageStateRecord> Records { get; }

    /// <summary>
    /// Converts to JSON object.
    /// </summary>
    /// <returns>The JSON object node.</returns>
    public JsonObjectNode ToJson()
        => new()
        {
            { "id", Id },
            { "question", Question },
            { "sent", SendTime },
            { "state", State.ToString() },
            { "answer", Answer },
            { "receiving", ReceiveStartTime },
            { "received", ReceiveCompleteTime },
            { "records", records.Select(i => i.ToJson()) },
            { "code", ErrorCode },
            { "error", ErrorMessage },
            { "intent", Intent },
            { "data", Data }
        };

    /// <summary>
    /// Updates the state.
    /// </summary>
    /// <param name="value">The new state.</param>
    /// <param name="note">The additional note of the state.</param>
    /// <returns>The state change record.</returns>
    internal RoundChatMessageStateRecord UpdateState(RoundChatMessageStates value, string note = null)
    {
        if (State == value && string.IsNullOrWhiteSpace(note)) return null;
        var now = DateTime.Now;
        State = value;
        var record = new RoundChatMessageStateRecord(value, DateTime.Now, note);
        records.Add(record);
        switch (State)
        {
            case RoundChatMessageStates.Done:
            case RoundChatMessageStates.Reject:
            case RoundChatMessageStates.Abort:
                if (!ReceiveCompleteTime.HasValue) ReceiveCompleteTime = now;
                break;
            case RoundChatMessageStates.IntentError:
            case RoundChatMessageStates.ProcessFailure:
            case RoundChatMessageStates.ResponseError:
                if (!ReceiveCompleteTime.HasValue) ReceiveCompleteTime = now;
                ErrorMessage = note;
                break;
            case RoundChatMessageStates.Receiving:
                if (!ReceiveStartTime.HasValue) ReceiveStartTime = now;
                break;
        }

        StateUpdated?.Invoke(this, State);
        return record;
    }
}

/// <summary>
/// The intent info of the round chat message.
/// </summary>
public class RoundChatMessageIntentInfo
{
    /// <summary>
    /// Initializes an instance of the RoundChatMessageIntentInfo class.
    /// </summary>
    /// <param name="info">The intent info.</param>
    public RoundChatMessageIntentInfo(JsonObjectNode info)
    {
        Info = info;
    }

    /// <summary>
    /// Initializes an instance of the RoundChatMessageIntentInfo class.
    /// </summary>
    /// <param name="message">The message to return to user.</param>
    public RoundChatMessageIntentInfo(string message)
        : this(true, message)
    {
    }

    /// <summary>
    /// Initializes an instance of the RoundChatMessageIntentInfo class.
    /// </summary>
    /// <param name="skipIntent">true if skip intent; otherwise, false.</param>
    /// <param name="message">The message to return to user.</param>
    public RoundChatMessageIntentInfo(bool skipIntent, string message)
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
    public static implicit operator RoundChatMessageIntentInfo(JsonObjectNode json)
    {
        if (json == null) return null;
        return new RoundChatMessageIntentInfo(json);
    }
}

/// <summary>
/// The error during the round chat message sending.
/// </summary>
public class RoundChatMessageException : Exception
{
    /// <summary>
    /// Initializes an instance of the RoundChatMessageException class.
    /// </summary>
    public RoundChatMessageException()
    {
    }

    /// <summary>
    /// Initializes an instance of the RoundChatMessageException class.
    /// </summary>
    /// <param name="model">The model of the round chat message.</param>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The inner exception.</param>
    public RoundChatMessageException(RoundChatMessageModel model, string message, Exception innerException = null)
        : base(message, innerException)
    {
        Model = model;
        if (innerException is FailedHttpException httpEx)
            HttpStatusCode = httpEx.StatusCode;
    }

    /// <summary>
    /// Initializes an instance of the RoundChatMessageException class.
    /// </summary>
    /// <param name="model">The model of the round chat message.</param>
    /// <param name="innerException">The inner exception.</param>
    public RoundChatMessageException(RoundChatMessageModel model, Exception innerException = null)
        : this(model, model?.ErrorMessage ?? innerException?.Message ?? (model?.State != null ? $"Send message failed on state {model.State}." : "Send message failed."), innerException)
    {
    }

    /// <summary>
    /// Gets the model of the round chat message.
    /// </summary>
    public RoundChatMessageModel Model { get; }

    /// <summary>
    /// Gets the state of the round chat message.
    /// </summary>
    public RoundChatMessageStates State => Model?.State ?? RoundChatMessageStates.Unknown;

    /// <summary>
    /// Gets the status code of the HTTP response; or null, if it is not an HTTP networking error.
    /// </summary>
    public HttpStatusCode? HttpStatusCode { get; }
}
