using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Trivial.Maths;
using Trivial.Tasks;
using Trivial.Text;

namespace Trivial.Data;

/// <summary>
/// The property info of sequence level configuration.
/// </summary>
/// <typeparam name="TValue">The type of value.</typeparam>
/// <typeparam name="TOperator">The type of merge operator.</typeparam>
public abstract class SequenceLevelConfigPropertyInfo<TValue, TOperator>
    where TOperator : struct
{
    /// <summary>
    /// Initializes a new instance of the SequenceLevelConfigPropertyInfo class.
    /// </summary>
    public SequenceLevelConfigPropertyInfo()
    {
    }

    /// <summary>
    /// Initializes a new instance of the SequenceLevelConfigPropertyInfo class.
    /// </summary>
    /// <param name="propertyKey">The property key.</param>
    /// <param name="title">The title.</param>
    /// <param name="op">The operator.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="link">The URI of web page.</param>
    public SequenceLevelConfigPropertyInfo(string propertyKey, string title, TOperator op, TValue defaultValue = default, Uri link = null)
    {
        PropertyKey = propertyKey;
        Title = title;
        Operator = op;
        DefaultValue = defaultValue;
        var type = typeof(TValue);
        if (type.IsValueType)
        {
            if (type == typeof(int) || type == typeof(long) || type == typeof(short)) Kind = "integer";
            else if (type == typeof(float) || type == typeof(double) || type == typeof(decimal)) Kind = "number";
            else if (type == typeof(bool)) Kind = "boolean";
            else if (type == typeof(int?) || type == typeof(long?) || type == typeof(short?)) Kind = "integer";
            else if (type == typeof(float?) || type == typeof(double?) || type == typeof(decimal?)) Kind = "number";
            else if (type == typeof(bool?)) Kind = "boolean";
        }
        else if (type == typeof(string))
        {
            Kind = "string";
        }
    }

    /// <summary>
    /// Initializes a new instance of the SequenceLevelConfigPropertyInfo class.
    /// </summary>
    /// <param name="propertyKey">The property key.</param>
    /// <param name="op">The operator.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="link">The URI of web page.</param>
    public SequenceLevelConfigPropertyInfo(string propertyKey, TOperator op, TValue defaultValue = default, Uri link = null)
        : this(propertyKey, null, op, defaultValue)
    {
    }

    /// <summary>
    /// The kind of value.
    /// </summary>
    [JsonPropertyName("kind")]
    public string Kind { get; set; }

    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the property key.
    /// </summary>
    [JsonPropertyName("key")]
    public string PropertyKey { get; set; }

    /// <summary>
    /// Gets or sets an optional HTTP URI of desciption web page.
    /// </summary>
    [JsonPropertyName("url")]
    public Uri Link { get; set; }

    /// <summary>
    /// Gets or sets the default value.
    /// </summary>
    [JsonPropertyName("default")]
    public TValue DefaultValue { get; set; }

    /// <summary>
    /// Gets or sets the operator to merge the values.
    /// </summary>
    [JsonPropertyName("op")]
    [JsonConverter(typeof(JsonStringEnumCompatibleConverter))]
    public TOperator Operator { get; set; }

    /// <summary>
    /// Gets or sets the additional information.
    /// </summary>
    [JsonPropertyName("info")]
    public JsonObjectNode Info { get; set; }
}
