using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Trivial.Collection;
using Trivial.Data;
using Trivial.Reflection;
using Trivial.Tasks;
using Trivial.Users;

namespace Trivial.Text;

/// <summary>
/// The modification kinds of the message.
/// </summary>
public enum ChatMessageModificationKinds : byte
{
    /// <summary>
    /// The message is never modified.
    /// </summary>
    Original = 0,

    /// <summary>
    /// The message is currently streaming.
    /// It means the message is transferring by continious updating for this while.
    /// It will be original or other one if it streams completed.
    /// </summary>
    Streaming = 1,

    /// <summary>
    /// The message has been modified by sender.
    /// </summary>
    Modified = 2,

    /// <summary>
    /// The message has been modified and is open to update by others.
    /// </summary>
    Collaborative = 3,

    /// <summary>
    /// The message has been removed by sender or admin.
    /// </summary>
    Removed = 5,

    /// <summary>
    /// The message is banned by system.
    /// </summary>
    Ban = 9,

    /// <summary>
    /// Others.
    /// </summary>
    Others = 15
}

/// <summary>
/// The content formats of the chat message.
/// </summary>
public enum ExtendedChatMessageFormats : byte
{
    /// <summary>
    /// The plain text.
    /// </summary>
    Text = 0,

    /// <summary>
    /// The markdown format string.
    /// </summary>
    Markdown = 2,

    /// <summary>
    /// The JSON object.
    /// </summary>
    Json = 5,

    /// <summary>
    /// The text is hidden but shows in the details view if available.
    /// </summary>
    Hidden = 7,
}

/// <summary>
/// The states of send result for chat message.
/// </summary>
public enum ExtendedChatMessageSendResultStates : byte
{
    /// <summary>
    /// Not send.
    /// </summary>
    NotSend = 0,

    /// <summary>
    /// Success.
    /// </summary>
    Success = 1,

    /// <summary>
    /// Network issue or timeout.
    /// </summary>
    NetworkIssue = 2,

    /// <summary>
    /// Unauthorized or fobidden to send.
    /// </summary>
    Forbidden = 3,

    /// <summary>
    /// Traffic limitation for connection.
    /// </summary>
    Throttle = 4,

    /// <summary>
    /// Format error or invalid request.
    /// </summary>
    RequestError = 5,

    /// <summary>
    /// Client error.
    /// </summary>
    ClientError = 6,

    /// <summary>
    /// Unknown server-side error.
    /// </summary>
    ServerError = 7,

    /// <summary>
    /// Unsupported operation or feature.
    /// </summary>
    Unsupported = 8,

    /// <summary>
    /// Canceled.
    /// </summary>
    Aborted = 9,

    /// <summary>
    /// Other error.
    /// </summary>
    OtherError = 10,
}

/// <summary>
/// The basic management levels of the chat message, e.g. can delete, modify.
/// </summary>
[Flags]
public enum ExtendedChatMessageManagementLevels : byte
{
    /// <summary>
    /// No permission.
    /// </summary>
    None = 0,

    /// <summary>
    /// Markable.
    /// </summary>
    Mark = 1,

    /// <summary>
    /// Modifiable.
    /// </summary>
    Modification = 2,

    /// <summary>
    /// Removable.
    /// </summary>
    Deletion = 4,

    /// <summary>
    /// Modification and deletion permissions.
    /// </summary>
    All = 15
}
