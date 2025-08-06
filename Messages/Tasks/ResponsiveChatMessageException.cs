using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Trivial.Net;
using Trivial.Text;

namespace Trivial.Tasks;

/// <summary>
/// The error during the round chat message sending.
/// </summary>
public class ResponsiveChatMessageException : Exception
{
    /// <summary>
    /// Initializes an instance of the ResponsiveChatMessageException class.
    /// </summary>
    public ResponsiveChatMessageException()
    {
    }

    /// <summary>
    /// Initializes an instance of the ResponsiveChatMessageException class.
    /// </summary>
    /// <param name="context">The context of the responsive chat message.</param>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The inner exception.</param>
    public ResponsiveChatMessageException(ResponsiveChatContext context, string message, Exception innerException = null)
        : base(message, innerException)
    {
        Question = context.Model.Question;
        Answer = context.Model.Answer;
        State = context.State;
        Records = context.Model.Records;
        TopicId = context.Topic?.Id;
        if (innerException is FailedHttpException httpEx)
            HttpStatusCode = httpEx.StatusCode;
#if NETCOREAPP
        else if (innerException is HttpRequestException reqEx)
            HttpStatusCode = reqEx.StatusCode;
#endif
    }

    /// <summary>
    /// Initializes an instance of the ResponsiveChatMessageException class.
    /// </summary>
    /// <param name="context">The context of the responsive chat message.</param>
    /// <param name="innerException">The inner exception.</param>
    public ResponsiveChatMessageException(ResponsiveChatContext context, Exception innerException = null)
        : this(context, context?.ErrorMessage ?? innerException?.Message ?? $"Send message failed on state {context.State}.", innerException)
    {
    }

    /// <summary>
    /// Gets the action records.
    /// </summary>
    public IReadOnlyList<ResponsiveChatMessageStateRecord> Records { get; }

    /// <summary>
    /// Gets the topic identifier.
    /// </summary>
    public string TopicId { get; }

    /// <summary>
    /// Gets the model of the question chat message.
    /// </summary>
    public ExtendedChatMessage Question { get; }

    /// <summary>
    /// Gets the text content of the question chat message.
    /// </summary>
    public string QuestionMessage => Question?.Message;

    /// <summary>
    /// Gets the model of the answer chat message.
    /// </summary>
    public ExtendedChatMessage Answer { get; }

    /// <summary>
    /// Gets the text content of the answer chat message.
    /// </summary>
    public string AnswerMessage => Answer?.Message;

    /// <summary>
    /// Gets the state of the round chat message.
    /// </summary>
    public ResponsiveChatMessageStates State { get; }

    /// <summary>
    /// Gets the status code of the HTTP response; or null, if it is not an HTTP networking error.
    /// </summary>
    public HttpStatusCode? HttpStatusCode { get; }
}
