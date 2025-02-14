using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Trivial.Text;

namespace Trivial.Data;

/// <summary>
/// The utilities and extensions of resource entity info.
/// </summary>
public static class ResourceEntityUtils
{
    /// <summary>
    /// Deserializes to a JSON format string.
    /// </summary>
    /// <param name="entity">The entity to deserialize.</param>
    /// <returns>A JSON format string.</returns>
    public static string ToJsonString(BaseResourceEntityInfo entity)
        => entity?.ToJson()?.ToString() ?? JsonValues.NullString;

    /// <summary>
    /// Deserializes to a JSON format string.
    /// </summary>
    /// <param name="entity">The entity to deserialize.</param>
    /// <param name="indentStyle">The ident style.</param>
    /// <returns>A JSON format string.</returns>
    public static string ToJsonString(BaseResourceEntityInfo entity, IndentStyles indentStyle)
        => entity?.ToJson()?.ToString(indentStyle) ?? JsonValues.NullString;

    /// <summary>
    /// Converts to a JSON node.
    /// </summary>
    /// <param name="entity">The entity to convert.</param>
    /// <returns>A JSON node; or null, if entity is null.</returns>
    public static JsonObjectNode ToJson(BaseResourceEntityInfo entity)
        => entity?.ToJson();

    /// <summary>
    /// Writes the entity to the specified writer as a JSON value.
    /// </summary>
    /// <param name="entity">The entity to write.</param>
    /// <param name="writer">The writer to which to write this instance.</param>
    public static void WriteTo(BaseResourceEntityInfo entity, Utf8JsonWriter writer)
    {
        if (writer == null) return;
        var json = entity?.ToJson();
        if (json is null) writer.WriteNullValue();
        else json.WriteTo(writer);
    }
}
