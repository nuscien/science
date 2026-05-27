using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using Trivial.Web;

namespace Trivial.Text;

/// <summary>
/// The plugin for chat message with message type.
/// </summary>
public abstract class BaseExtendedChatMessagePlugin
{
    /// <summary>
    /// Gets the message type.
    /// </summary>
    public abstract string MessageType { get; }
}

/// <summary>
/// The base chat message data for common-used information.
/// </summary>
public abstract class BaseCommonChatMessageData : BaseObservableProperties
{
    /// <summary>
    /// Gets the type of message.
    /// </summary>
    [JsonIgnore]
    public abstract string MessageType { get; }

    /// <summary>
    /// Gets or sets the attachment links.
    /// </summary>
    [JsonPropertyName("attachments")]
    [Description("The attachment links.")]
    public List<AttachmentLinkItem> Attachments
    {
        get => GetCurrentProperty<List<AttachmentLinkItem>>();
        set => GetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the location where the message sends from and is about.
    /// </summary>
    [JsonPropertyName("location")]
    [Description("The location where the message sends from and is about.")]
    public Geography.Geolocation.Model Location
    {
        get => GetCurrentProperty<Geography.Geolocation.Model>();
        set => GetCurrentProperty(value);
    }

}

/// <summary>
/// The programming code snippet and its information.
/// </summary>
public class ProgrammingCodeSnippetInfo : BaseObservableProperties
{
    /// <summary>
    /// Gets or sets the programming language name or its content type.
    /// </summary>
    [JsonPropertyName("language")]
    public string Language
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(FormatLanguageName(value));
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

    /// <summary>
    /// Sets the language name.
    /// </summary>
    /// <param name="languageName">The language name.</param>
    /// <param name="skipFormat">true if skip formatting the language name; otherwise, false.</param>
    public void SetLanguageName(string languageName, bool skipFormat = false)
        => SetProperty(nameof(Language), skipFormat ? languageName : FormatLanguageName(languageName));

    private static string FormatLanguageName(string value)
    {
        value = value.Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(value)) return null;
        if (value.StartsWith("text/") || value.StartsWith("text\\")) value = value.Substring(5);
        else if (value.StartsWith('.')) value = value.Substring(1);
        return value switch
        {
            JsonValues.JsonMIME or "json" => "json",
            "x-csharp" or "csharp" or "c#" or "cs" => "csharp",
            WebFormat.JavaScriptMIME or "javascript" or "ecmascript" or "js" or "esm" => "javascript",
            WebFormat.YamlMIME or "x-yaml" or "yaml" => "yaml",
            "markdown" or "md" or ".md" => "markdown",
            WebFormat.XmlMIME or "xml" or "settings" => "xml",
            WebFormat.SvgMIME or "svg" => "svg",
            "x-python" or "python" or "py" or "py2" or "py3" or "pyw" => "python",
            "x-chdr" or "x-c" or "c" or "cc" or "cxx" or "dic" or "h" => "c",
            "cpp" or "c++" or "hh" or "hpp" => "cpp",
            "x-golang" or "golang" or "go" => "go",
            "x-java-source" or "java" => "java",
            "x-qsharp" or "qsharp" or "q#" or "qs" => "qsharp",
            JsonValues.JsonlMIME or "jsonl" => "jsonl",
            _ => null
        };
    }
}
