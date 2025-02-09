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
/// The handler of sequence level configuration.
/// </summary>
public abstract class SequenceLevelConfigHandler
{
    /// <summary>
    /// Initializes a new instance of the SequenceLevelConfigHandler class.
    /// </summary>
    internal SequenceLevelConfigHandler()
    {
    }

    /// <summary>
    /// Fills output by input.
    /// </summary>
    /// <param name="input">The input data.</param>
    /// <param name="propertyInfo">The property info</param>
    /// <param name="output">The output result.</param>
    internal abstract void Fill(IEnumerable<JsonObjectNode> input, JsonObjectNode propertyInfo, JsonObjectNode output);
}

/// <summary>
/// The handler of sequence level configuration.
/// </summary>
public abstract class SequenceLevelConfigHandler<TValue, TOperator> : SequenceLevelConfigHandler
     where TOperator : struct, Enum
{
    /// <summary>
    /// Gets or sets the fallback default value.
    /// </summary>
    protected TValue FallbackDefaultValue { get; set; }

    /// <summary>
    /// Gets or sets the default operator.
    /// </summary>
    protected TOperator DefaultOperator { get; set; }

    /// <summary>
    /// Gets the result.
    /// </summary>
    /// <param name="input">The input data.</param>
    /// <param name="propertyInfo">The property info.</param>
    /// <returns>The result.</returns>
    public TValue GetResult(IEnumerable<JsonObjectNode> input, SequenceLevelConfigPropertyInfo<TValue, TOperator> propertyInfo)
    {
        var p = JsonObjectNode.ConvertFrom(propertyInfo);
        return GetResult(input, p, out _, out var result) ? result : default;
    }

    /// <summary>
    /// Gets the result.
    /// </summary>
    /// <param name="op">The operator.</param>
    /// <param name="input">The input values.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>The value of result.</returns>
    /// <exception cref="NotSupportedException">The op is not supported.</exception>
    protected abstract TValue GetResult(TOperator op, IEnumerable<TValue> input, TValue defaultValue);

    /// <summary>
    /// Tries to get the property value.
    /// </summary>
    /// <param name="json">The JSON instance.</param>
    /// <param name="propertyKey">The property key.</param>
    /// <param name="result">The result output.</param>
    /// <returns>true if get succeeded; otherwise, false.</returns>
    protected abstract bool TryGetValue(JsonObjectNode json, string propertyKey, out TValue result);

    /// <summary>
    /// Sets the property.
    /// </summary>
    /// <param name="json">The JSON instance.</param>
    /// <param name="propertyKey">The property key.</param>
    /// <param name="value">The value of property to set.</param>
    protected abstract void SetValue(JsonObjectNode json, string propertyKey, TValue value);

    /// <inheritdoc />
    internal override void Fill(IEnumerable<JsonObjectNode> input, JsonObjectNode propertyInfo, JsonObjectNode output)
    {
        if (!GetResult(input, propertyInfo, out var key, out var result)) return;
        SetValue(output, key, result);
    }

    /// <summary>
    /// Gets the result.
    /// </summary>
    /// <param name="input">The input data.</param>
    /// <param name="propertyInfo">The property info.</param>
    /// <param name="key">The property key.</param>
    /// <param name="result">The result.</param>
    /// <returns>true if get succeeded; otherwise, false.</returns>
    private bool GetResult(IEnumerable<JsonObjectNode> input, JsonObjectNode propertyInfo, out string key, out TValue result)
    {
        key = propertyInfo.TryGetStringTrimmedValue("key", true);
        if (key == null)
        {
            result = default;
            return false;
        }

        var col = GetCollection(input, key);
        if (!TryGetValue(propertyInfo, "default", out var defaultValue)) defaultValue = FallbackDefaultValue;
        var op = propertyInfo.TryGetEnumValue<TOperator>("op") ?? DefaultOperator;
        try
        {
            result = GetResult(op, col, defaultValue);
            return true;
        }
        catch (NotSupportedException)
        {
        }
        catch (InvalidOperationException)
        {
        }
        catch (NotImplementedException)
        {
        }

        result = default;
        return false;
    }

    /// <summary>
    /// Gets the collection of value from the input data.
    /// </summary>
    /// <param name="input">The input data.</param>
    /// <param name="propertyKey">The property key.</param>
    /// <returns>The collection of value.</returns>
    private IEnumerable<TValue> GetCollection(IEnumerable<JsonObjectNode> input, string propertyKey)
    {
        foreach (var item in input)
        {
            if (item != null && TryGetValue(item, propertyKey, out var v) == true) yield return v;
        }
    }
}

/// <summary>
/// The handler of sequence level configuration.
/// </summary>
public abstract class Int32SequenceLevelConfigHandler<TOperator> : SequenceLevelConfigHandler<int, TOperator>
     where TOperator : struct, Enum
{
    /// <inheritdoc />
    protected override bool TryGetValue(JsonObjectNode json, string propertyKey, out int result)
        => json.TryGetInt32Value(propertyKey, out result);

    /// <inheritdoc />
    protected override void SetValue(JsonObjectNode json, string propertyKey, int value)
        => json.SetValue(propertyKey, value);
}

/// <summary>
/// The handler of sequence level configuration.
/// </summary>
public abstract class Int64SequenceLevelConfigHandler<TOperator> : SequenceLevelConfigHandler<long, TOperator>
     where TOperator : struct, Enum
{
    /// <inheritdoc />
    protected override bool TryGetValue(JsonObjectNode json, string propertyKey, out long result)
        => json.TryGetInt64Value(propertyKey, out result);

    /// <inheritdoc />
    protected override void SetValue(JsonObjectNode json, string propertyKey, long value)
        => json.SetValue(propertyKey, value);
}

/// <summary>
/// The handler of sequence level configuration.
/// </summary>
public abstract class DoubleSequenceLevelConfigHandler<TOperator> : SequenceLevelConfigHandler<double, TOperator>
     where TOperator : struct, Enum
{
    /// <inheritdoc />
    protected override bool TryGetValue(JsonObjectNode json, string propertyKey, out double result)
        => json.TryGetDoubleValue(propertyKey, out result);

    /// <inheritdoc />
    protected override void SetValue(JsonObjectNode json, string propertyKey, double value)
        => json.SetValue(propertyKey, value);
}

/// <summary>
/// The handler of sequence level configuration.
/// </summary>
public abstract class DecimalSequenceLevelConfigHandler<TOperator> : SequenceLevelConfigHandler<decimal, TOperator>
     where TOperator : struct, Enum
{
    /// <inheritdoc />
    protected override bool TryGetValue(JsonObjectNode json, string propertyKey, out decimal result)
        => json.TryGetDecimalValue(propertyKey, out result);

    /// <inheritdoc />
    protected override void SetValue(JsonObjectNode json, string propertyKey, decimal value)
        => json.SetValue(propertyKey, value);
}

/// <summary>
/// The handler of sequence level configuration.
/// </summary>
public abstract class StringSequenceLevelConfigHandler<TOperator> : SequenceLevelConfigHandler<string, TOperator>
     where TOperator : struct, Enum
{
    /// <inheritdoc />
    protected override bool TryGetValue(JsonObjectNode json, string propertyKey, out string result)
        => json.TryGetStringValue(propertyKey, out result);

    /// <inheritdoc />
    protected override void SetValue(JsonObjectNode json, string propertyKey, string value)
        => json.SetValue(propertyKey, value);
}

/// <summary>
/// The handler of sequence level configuration.
/// </summary>
public abstract class BooleanSequenceLevelConfigHandler<TOperator> : SequenceLevelConfigHandler<bool, TOperator>
     where TOperator : struct, Enum
{
    /// <inheritdoc />
    protected override bool TryGetValue(JsonObjectNode json, string propertyKey, out bool result)
        => json.TryGetBooleanValue(propertyKey, out result);

    /// <inheritdoc />
    protected override void SetValue(JsonObjectNode json, string propertyKey, bool value)
        => json.SetValue(propertyKey, value);
}

/// <summary>
/// The handler of sequence level configuration.
/// </summary>
public abstract class JsonObjectSequenceLevelConfigHandler<TOperator> : SequenceLevelConfigHandler<JsonObjectNode, TOperator>
     where TOperator : struct, Enum
{
    /// <inheritdoc />
    protected override bool TryGetValue(JsonObjectNode json, string propertyKey, out JsonObjectNode result)
    {
        result = json.TryGetObjectValue(propertyKey);
        return result is not null;
    }

    /// <inheritdoc />
    protected override void SetValue(JsonObjectNode json, string propertyKey, JsonObjectNode value)
        => json.SetValue(propertyKey, value);
}

/// <summary>
/// The handler of sequence level configuration.
/// </summary>
public abstract class JsonArraySequenceLevelConfigHandler<TOperator> : SequenceLevelConfigHandler<JsonArrayNode, TOperator>
     where TOperator : struct, Enum
{
    /// <inheritdoc />
    protected override bool TryGetValue(JsonObjectNode json, string propertyKey, out JsonArrayNode result)
    {
        result = json.TryGetArrayValue(propertyKey);
        return result is not null;
    }

    /// <inheritdoc />
    protected override void SetValue(JsonObjectNode json, string propertyKey, JsonArrayNode value)
        => json.SetValue(propertyKey, value);
}
