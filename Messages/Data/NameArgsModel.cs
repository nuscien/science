using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Trivial.Reflection;
using Trivial.Text;

namespace Trivial.Data;

/// <summary>
/// The observable model of the name and arguments.
/// </summary>
[DataContract]
public class NameArgsObservableModel : ObservableProperties
{
    /// <summary>
    /// Initializes a new instance of the NameArgsObservableModel class.
    /// </summary>
    public NameArgsObservableModel()
    {
    }

    /// <summary>
    /// Initializes a new instance of the NameArgsObservableModel class.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="value">The value.</param>
    public NameArgsObservableModel(string name, JsonObjectNode value = null)
    {
        Name = name;
        Arguments = value;
    }

    /// <summary>
    /// Initializes a new instance of the NameArgsObservableModel class.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="value">The value.</param>
    public NameArgsObservableModel(string name, Dictionary<string, string> value = null)
    {
        Name = name;
        if (value == null) return;
        Arguments = new();
        Arguments.SetRange(value);
    }

    /// <summary>
    /// Initializes a new instance of the NameArgsObservableModel class.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="value">The value.</param>
    public NameArgsObservableModel(string name, Dictionary<string, JsonObjectNode> value = null)
    {
        Name = name;
        if (value == null) return;
        Arguments = new();
        Arguments.SetRange(value);
    }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    [DataMember(Name = "name")]
    [JsonPropertyName("name")]
    [Description("The name.")]
    public string Name

    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value, OnChange);
    }

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    [DataMember(Name = "arguments")]
    [JsonPropertyName("arguments")]
    [Description("The arguments.")]
    public JsonObjectNode Arguments
    {
        get => GetCurrentProperty<JsonObjectNode>();
        set => SetCurrentProperty(value, OnChange);
    }

    /// <summary>
    /// Occurs on the property name is changed.
    /// </summary>
    /// <param name="newValue">The new value of the property.</param>
    /// <param name="exist">true if the old value of the property exists; otherwise, false.</param>
    /// <param name="oldValue">The old value of the property.</param>
    protected virtual void OnNameChange(object newValue, bool exist, object oldValue)
    {
    }

    /// <summary>
    /// Occurs on the property value is changed.
    /// </summary>
    /// <param name="newValue">The new value of the property.</param>
    /// <param name="exist">true if the old value of the property exists; otherwise, false.</param>
    /// <param name="oldValue">The old value of the property.</param>
    protected virtual void OnArgumentsChange(object newValue, bool exist, object oldValue)
    {
    }

    private void OnChange(string key, object newValue, bool exist, object oldValue)
    {
        if (string.IsNullOrEmpty(key)) return;
        switch (key)
        {
            case nameof(Name):
                OnNameChange(newValue, exist, oldValue);
                break;
            case nameof(Arguments):
                OnArgumentsChange(newValue, exist, oldValue);
                break;
        }
    }

    /// <summary>
    /// Parses.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <returns>The instance parsed; or null, if failed.</returns>
    public static NameArgsObservableModel Parse(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        if (s.StartsWith('`'))
        {
            s = s.Trim('`');
            if (s.StartsWith("json", StringComparison.OrdinalIgnoreCase)) s = s.Substring(4).Trim();
        }

        if (s.StartsWith('{'))
        {
            var json = JsonObjectNode.TryParse(s);
            if (json == null) json = JsonObjectNode.TryParse(string.Concat(s, '}'));
            if (json == null) return null;
            return new(json.TryGetStringTrimmedValue("name"), json.TryGetObjectValue("arguments") ?? json.TryGetObjectValue("args"));
        }

        var lines = StringExtensions.ReadLines(s).ToList();
        if (lines.Count == 4 && lines[3] == "```")
        {
            if (lines[1] == "```python")
            {
                var key = lines[0]?.Trim();
                if (string.IsNullOrEmpty(key)) return null;
                var cmd = lines[2]?.Trim();
                if (string.IsNullOrEmpty(cmd) || !cmd.StartsWith("tool_call(")) return null;
                cmd = cmd.Substring(10).TrimEnd(')');
                var args = cmd.Split(',');
                var json = new JsonObjectNode();
                foreach (var kvp in args)
                {
                    try
                    {
                        var i = kvp.IndexOf('=');
                        if (i < 1) continue;
                        var item = kvp.Substring(i + 1).Trim();
                        if (item.StartsWith('"')) json.SetValue(kvp.Substring(0, i), item.Trim('"'));
                        else if (item.StartsWith('\'')) json.SetValue(kvp.Substring(0, i), item.Trim('\''));
                        else if (item.IndexOf('.') > 0 && double.TryParse(item, out var d)) json.SetValue(kvp.Substring(0, i), d);
                        else if (int.TryParse(item, out var j)) json.SetValue(kvp.Substring(0, i), j);
                        else json.SetValue(kvp.Substring(0, i), item);
                    }
                    catch (ArgumentException)
                    {
                    }
                }

                return new(json.TryGetStringTrimmedValue("name"), json.TryGetObjectValue("arguments") ?? json.TryGetObjectValue("args"));
            }
            else if (lines[1] == "```json")
            {
                var key = lines[0]?.Trim();
                var cmd = JsonObjectNode.TryParse(lines[2]?.Trim());
                if (cmd == null) return new();
                var json = cmd.TryGetObjectValue("arguments");
                if (json != null)
                {
                    cmd = json;
                    key = cmd.TryGetStringTrimmedValue("name", true) ?? key;
                }

                if (string.IsNullOrEmpty(key)) return null;
                return new(key, cmd);
            }
        }
        else if (lines.Count == 2 && !string.IsNullOrEmpty(lines[1]))
        {
            if (lines[1].Trim().Trim('`').StartsWith('{'))
            {
                var cmd = JsonObjectNode.TryParse(lines[1]?.Trim());
                if (cmd != null)
                {
                    var key = cmd.TryGetStringTrimmedValue("name", true);
                    var args = cmd.TryGetObjectValue("arguments");
                    var firstLine = lines[0].Length > 1 && !lines[0].Contains(' ') ? lines[0].Trim() : null;
                    if (key != null)
                    {
                        if (args != null || cmd.Count < 2) return new(key, args);
                    }
                    else if (args == null && firstLine != null)
                    {
                        return new(firstLine, cmd);
                    }
                }
            }
        }

        return null;
    }
}
