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
    /// The message is neve modified.
    /// </summary>
    Original = 0,

    /// <summary>
    /// The streaming message which means the message is transferring by continious updating.
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
    /// The message has been removed by sender.
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
    /// The code.
    /// </summary>
    Code = 5,

    /// <summary>
    /// The text is hidden but shows in the details view if available.
    /// </summary>
    Hidden = 7,
}
