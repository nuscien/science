using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
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
/// The data for chat message with message type.
/// </summary>
public interface IExtendedChatMessageDataDescription
{
    /// <summary>
    /// Gets the message type.
    /// </summary>
    public string MessageType { get; }
}

/// <summary>
/// The programming code snippet and its information.
/// </summary>
public class ProgrammingCodeSnippetInfo : BaseObservableProperties, IExtendedChatMessageDataDescription
{
    [JsonIgnore]
    string IExtendedChatMessageDataDescription.MessageType => ExtendedChatMessages.ProgrammingCodeKey;

    /// <summary>
    /// Gets or sets the programming language name or its content type.
    /// </summary>
    [JsonPropertyName("language")]
    public string Language
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the code snippet.
    /// </summary>
    [JsonPropertyName("value")]
    public string Value
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the optional title.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    [JsonPropertyName("title")]
    public string Title
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }
}
