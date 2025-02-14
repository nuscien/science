using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Trivial.Maths;
using Trivial.Tasks;
using Trivial.Text;

namespace Trivial.Data;

/// <summary>
/// The merge operator of JSON object and array.
/// </summary>
public enum JsonObjectMergeOperators : byte
{
    /// <summary>
    /// The first item.
    /// </summary>
    First = 0,

    /// <summary>
    /// The last item.
    /// </summary>
    Last = 1,

    /// <summary>
    /// Appending the members.
    /// </summary>
    Append = 2,

    /// <summary>
    /// Overridding the members.
    /// </summary>
    Override = 3,

    /// <summary>
    /// Returns empty.
    /// </summary>
    Empty = 7
}

/// <summary>
/// The handler of sequence level configuration.
/// </summary>
internal class Int32SequenceLevelConfigHandler : Int32SequenceLevelConfigHandler<NumberCollectionOperators>
{
    /// <inheritdoc />
    protected override int GetResult(NumberCollectionOperators op, IEnumerable<int> value, int defaultValue)
        => CollectionOperations.Merge(op, value) ?? defaultValue;
}

/// <summary>
/// The handler of sequence level configuration.
/// </summary>
internal class Int64SequenceLevelConfigHandler : Int64SequenceLevelConfigHandler<NumberCollectionOperators>
{
    /// <inheritdoc />
    protected override long GetResult(NumberCollectionOperators op, IEnumerable<long> value, long defaultValue)
        => CollectionOperations.Merge(op, value) ?? defaultValue;
}

/// <summary>
/// The handler of sequence level configuration.
/// </summary>
internal class DoubleSequenceLevelConfigHandler : DoubleSequenceLevelConfigHandler<NumberCollectionOperators>
{
    /// <inheritdoc />
    protected override double GetResult(NumberCollectionOperators op, IEnumerable<double> value, double defaultValue)
        => CollectionOperations.Merge(op, value) ?? defaultValue;
}

/// <summary>
/// The handler of sequence level configuration.
/// </summary>
internal class DecimalSequenceLevelConfigHandler : DecimalSequenceLevelConfigHandler<NumberCollectionOperators>
{
    /// <inheritdoc />
    protected override decimal GetResult(NumberCollectionOperators op, IEnumerable<decimal> value, decimal defaultValue)
        => CollectionOperations.Merge(op, value) ?? defaultValue;
}

/// <summary>
/// The handler of sequence level configuration.
/// </summary>
internal class StringSequenceLevelConfigHandler : StringSequenceLevelConfigHandler<StringCollectionOperators>
{
    /// <inheritdoc />
    protected override string GetResult(StringCollectionOperators op, IEnumerable<string> value, string defaultValue)
        => CollectionOperations.Merge(op, value) ?? defaultValue;
}

/// <summary>
/// The handler of sequence level configuration.
/// </summary>
internal class BooleanSequenceLevelConfigHandler : BooleanSequenceLevelConfigHandler<SequenceBooleanOperator>
{
    /// <inheritdoc />
    protected override bool GetResult(SequenceBooleanOperator op, IEnumerable<bool> value, bool defaultValue)
        => BooleanOperations.Calculate(op, value) ?? defaultValue;
}

/// <summary>
/// The handler of sequence level configuration.
/// </summary>
internal class JsonObjectSequenceLevelConfigHandler : JsonObjectSequenceLevelConfigHandler<JsonObjectMergeOperators>
{
    /// <inheritdoc />
    protected override JsonObjectNode GetResult(JsonObjectMergeOperators op, IEnumerable<JsonObjectNode> value, JsonObjectNode defaultValue)
    {
        if (value == null) return defaultValue;
        switch (op)
        {
            case JsonObjectMergeOperators.First:
                return value.FirstOrDefault() ?? defaultValue;
            case JsonObjectMergeOperators.Last:
                return value.LastOrDefault() ?? defaultValue;
            case JsonObjectMergeOperators.Append:
                {
                    var obj = defaultValue?.Clone() ?? new();
                    foreach (var item in value)
                    {
                        obj.SetRange(item, true);
                    }

                    return obj;
                }
            case JsonObjectMergeOperators.Override:
                {
                    var obj = defaultValue?.Clone() ?? new();
                    foreach (var item in value)
                    {
                        obj.SetRange(item, false);
                    }

                    return obj;
                }
            case JsonObjectMergeOperators.Empty:
                return new();
            default:
                throw new NotSupportedException($"The op is not supported.");
        }
    }
}

/// <summary>
/// The handler of sequence level configuration.
/// </summary>
internal class JsonArraySequenceLevelConfigHandler : JsonArraySequenceLevelConfigHandler<JsonObjectMergeOperators>
{
    /// <inheritdoc />
    protected override JsonArrayNode GetResult(JsonObjectMergeOperators op, IEnumerable<JsonArrayNode> value, JsonArrayNode defaultValue)
    {
        if (value == null) return defaultValue;
        switch (op)
        {
            case JsonObjectMergeOperators.First:
                return value.FirstOrDefault() ?? defaultValue;
            case JsonObjectMergeOperators.Last:
                return value.LastOrDefault() ?? defaultValue;
            case JsonObjectMergeOperators.Append:
                {
                    var obj = defaultValue?.Clone() ?? new();
                    foreach (var item in value)
                    {
                        obj.AddRange(item);
                    }

                    return obj;
                }
            case JsonObjectMergeOperators.Override:
                {
                    var obj = defaultValue?.Clone() ?? new();
                    foreach (var item in value)
                    {
                        if (item == null) continue;
                        obj.SetRange(item);
                    }

                    return obj;
                }
            case JsonObjectMergeOperators.Empty:
                return new();
            default:
                throw new NotSupportedException($"The op is not supported.");
        }
    }
}

/// <summary>
/// The handler of sequence level configuration.
/// </summary>
internal class TagsSequenceLevelConfigHandler : TagsSequenceLevelConfigHandler<JsonObjectMergeOperators>
{
    /// <inheritdoc />
    protected override List<string> GetResult(JsonObjectMergeOperators op, IEnumerable<List<string>> value, List<string> defaultValue)
    {
        if (value == null) return defaultValue;
        switch (op)
        {
            case JsonObjectMergeOperators.First:
                return value.FirstOrDefault() ?? defaultValue;
            case JsonObjectMergeOperators.Last:
                return value.LastOrDefault() ?? defaultValue;
            case JsonObjectMergeOperators.Append:
                {
                    var obj = defaultValue is null ? new List<string>() : new(defaultValue);
                    foreach (var item in value)
                    {
                        obj.AddRange(item);
                    }

                    return obj;
                }
            case JsonObjectMergeOperators.Override:
                {
                    var obj = defaultValue is null ? new List<string>() : new(defaultValue);
                    foreach (var item in value)
                    {
                        if (item == null) continue;
                        for (var i = 0; i < item.Count; i++)
                        {
                            if (obj.Count == i) obj.Add(item[i]);
                            else obj[i] = item[i];
                        }
                    }

                    return obj;
                }
            case JsonObjectMergeOperators.Empty:
                return new();
            default:
                throw new NotSupportedException($"The op is not supported.");
        }
    }
}