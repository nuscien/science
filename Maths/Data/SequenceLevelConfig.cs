using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Trivial.Maths;
using Trivial.Tasks;
using Trivial.Text;

namespace Trivial.Data;

/// <summary>
/// The sequnce level configuraion.
/// </summary>
public class SequenceLevelConfig
{
    private Dictionary<string, SequenceLevelConfigHandler> handlers = new();

    /// <summary>
    /// Calculates.
    /// </summary>
    /// <param name="input">The input data.</param>
    /// <param name="properties">The collection of property info.</param>
    /// <returns>The result.</returns>
    public JsonObjectNode Calculate(IEnumerable<JsonObjectNode> input, IEnumerable<JsonObjectNode> properties)
    {
        var result = new JsonObjectNode();
        if (input == null || properties == null) return result;
        foreach (var prop in properties)
        {
            if (prop is not JsonObjectNode json) continue;
            var kind = json.TryGetStringTrimmedValue("kind", true) ?? json.GetValueKind("default") switch
            {
                JsonValueKind.String => "string",
                JsonValueKind.Number => "number",
                JsonValueKind.True => "boolean",
                JsonValueKind.False => "boolean",
                _ => null
            };
            if (kind == null) continue;
            if (!handlers.TryGetValue(kind, out var h)) h = kind.ToLowerInvariant() switch
            {
                "string" or "s" => new StringSequenceLevelConfigHandler(),
                "number" or "n" => new DoubleSequenceLevelConfigHandler(),
                "integer" or "i" => new Int64SequenceLevelConfigHandler(),
                "boolean" or "b" => new BooleanSequenceLevelConfigHandler(),
                _ => null,
            };
            if (h == null) continue;
            h.Fill(input, json, result);
        }

        return result;
    }

    /// <summary>
    /// Calculates.
    /// </summary>
    /// <param name="input">The input data.</param>
    /// <param name="properties">The collection of property info.</param>
    /// <returns>The result.</returns>
    public JsonObjectNode Calculate(IEnumerable<JsonObjectNode> input, JsonObjectNode properties)
    {
        if (properties == null) return new();
        var arr = new List<JsonObjectNode>();
        foreach (var prop in properties)
        {
            if (prop.Value is not JsonObjectNode json || string.IsNullOrWhiteSpace(prop.Key)) continue;
            var key = json.TryGetStringTrimmedValue("key");
            if (key == null)
            {
                json = json.Clone();
                json.SetValue("key", prop.Key);
            }

            arr.Add(json);
        }

        return Calculate(input, arr);
    }

    /// <summary>
    /// Registers a handler.
    /// </summary>
    /// <param name="kind">The kind.</param>
    /// <param name="handler">The handler.</param>
    public void Register(string kind, SequenceLevelConfigHandler handler)
    {
        if (string.IsNullOrWhiteSpace(kind)) return;
        if (handler == null) handlers.Remove(kind);
        else handlers[kind] = handler;
    }

    /// <summary>
    /// Removes a handler.
    /// </summary>
    /// <param name="kind"></param>
    /// <returns></returns>
    public bool Remove(string kind)
        => handlers.Remove(kind);
}
