using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Trivial.Data;
using Trivial.Net;
using Trivial.Reflection;
using Trivial.Text;

namespace Trivial.Tasks;

/// <summary>
/// The processing states of chat message.
/// </summary>
public enum ResponsiveChatMessageStates : byte
{
    /// <summary>
    /// The message is initialized.
    /// </summary>
    Initialized = 0,

    /// <summary>
    /// Sending to get intent data.
    /// </summary>
    SendingIntent = 1,

    /// <summary>
    /// Intent analysing.
    /// </summary>
    Intent = 2,

    /// <summary>
    /// Processing intent business logic.
    /// </summary>
    Processing = 3,

    /// <summary>
    /// Sending request message.
    /// </summary>
    SendingMessage = 4,

    /// <summary>
    /// Waiting for respond.
    /// </summary>
    Waiting = 5,

    /// <summary>
    /// Receiving the response message.
    /// </summary>
    Receiving = 6,

    /// <summary>
    /// Receive the message succeeded.
    /// </summary>
    Done = 7,

    /// <summary>
    /// The message is rejected to return before or during transferring back.
    /// </summary>
    Reject = 8,

    /// <summary>
    /// Canceled or aborted.
    /// </summary>
    Abort = 9,

    /// <summary>
    /// Error of intent request or response.
    /// </summary>
    IntentError = 10,

    /// <summary>
    /// Error of processing the intent.
    /// </summary>
    ProcessFailure = 11,

    /// <summary>
    /// Error of getting or receiving the response message.
    /// </summary>
    ResponseError = 12,

    /// <summary>
    /// Other error.
    /// </summary>
    OtherError = 13,

    /// <summary>
    /// Unknown state.
    /// </summary>
    Unknown = 15,
}
