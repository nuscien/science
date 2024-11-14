using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trivial.Collection;

namespace Trivial.Maths;

/// <summary>
/// The operators for number collection.
/// </summary>
public enum NumberCollectionOperators : byte
{
    /// <summary>
    /// Always returns zero (0).
    /// </summary>
    Zero = 0,

    /// <summary>
    /// The smallest number.
    /// </summary>
    Min = 1,

    /// <summary>
    /// The largest number.
    /// </summary>
    Max = 2,

    /// <summary>
    /// The first number.
    /// </summary>
    First = 3,

    /// <summary>
    /// The last number.
    /// </summary>
    Last = 4,

    /// <summary>
    /// Sum of the numbers.
    /// </summary>
    Sum = 5,

    /// <summary>
    /// Average mean of the numbers.
    /// </summary>
    Mean = 6,

    /// <summary>
    /// Average median of the numbers.
    /// </summary>
    Median = 7,

    /// <summary>
    /// Count of the numbers.
    /// </summary>
    Count = 15,
}

public static partial class CollectionOperations
{
    /// <summary>
    /// Gets one item by specific operator.
    /// </summary>
    /// <param name="op">The operator.</param>
    /// <param name="input">The input collection to compute.</param>
    /// <param name="defaultValue">The default value for empty.</param>
    /// <returns>The result.</returns>
    /// <exception cref="NotSupportedException">The operator is not supported.</exception>
    public static int Merge(NumberCollectionOperators op, IEnumerable<int> input, int defaultValue)
        => Merge(op, input) ?? defaultValue;

    /// <summary>
    /// Gets one item by specific operator.
    /// </summary>
    /// <param name="op">The operator.</param>
    /// <param name="input">The input collection to compute.</param>
    /// <param name="defaultValue">The default value for empty.</param>
    /// <returns>The result.</returns>
    /// <exception cref="NotSupportedException">The operator is not supported.</exception>
    public static double Merge(NumberCollectionOperators op, IEnumerable<int> input, double defaultValue)
        => op switch
        {
            NumberCollectionOperators.Zero => 0d,
            NumberCollectionOperators.Sum => Merge(input, Enumerable.Sum) ?? defaultValue,
            NumberCollectionOperators.Mean => Merge(input, Enumerable.Average) ?? defaultValue,
            NumberCollectionOperators.Median => Median(input) ?? defaultValue,
            NumberCollectionOperators.Min => Min(input) ?? defaultValue,
            NumberCollectionOperators.Max => Max(input) ?? defaultValue,
            NumberCollectionOperators.First => ListExtensions.FirstOrNull(input) ?? defaultValue,
            NumberCollectionOperators.Last => ListExtensions.LastOrNull(input) ?? defaultValue,
            NumberCollectionOperators.Count => input.Count(),
            _ => throw NotSupported(op),
        };

    /// <summary>
    /// Gets one item by specific operator.
    /// </summary>
    /// <param name="op">The operator.</param>
    /// <param name="input">The input collection to compute.</param>
    /// <returns>The result.</returns>
    /// <exception cref="NotSupportedException">The operator is not supported.</exception>
    public static int? Merge(NumberCollectionOperators op, IEnumerable<int> input)
        => op switch
        {
            NumberCollectionOperators.Zero => 0,
            NumberCollectionOperators.Sum => Merge(input, Enumerable.Sum) ?? 0,
            NumberCollectionOperators.Mean => MergeToInt32(input, Enumerable.Average),
            NumberCollectionOperators.Median => Median(input),
            NumberCollectionOperators.Min => StatisticalMethod.Min(input),
            NumberCollectionOperators.Max => StatisticalMethod.Max(input),
            NumberCollectionOperators.First => ListExtensions.FirstOrNull(input),
            NumberCollectionOperators.Last => ListExtensions.LastOrNull(input),
            NumberCollectionOperators.Count => input?.Count() ?? 0,
            _ => throw NotSupported(op),
        };

    /// <summary>
    /// Gets one item by specific operator.
    /// </summary>
    /// <param name="op">The operator.</param>
    /// <param name="input">The input collection to compute.</param>
    /// <returns>The result.</returns>
    /// <exception cref="NotSupportedException">The operator is not supported.</exception>
    public static int? Merge(NumberCollectionOperators op, params int[] input)
        => Merge(op, input as IEnumerable<int>);

    /// <summary>
    /// Gets one item by specific operator.
    /// </summary>
    /// <param name="op">The operator.</param>
    /// <param name="input">The input collection to compute.</param>
    /// <param name="defaultValue">The default value for empty.</param>
    /// <returns>The result.</returns>
    /// <exception cref="NotSupportedException">The operator is not supported.</exception>
    public static long Merge(NumberCollectionOperators op, IEnumerable<long> input, long defaultValue)
        => Merge(op, input) ?? defaultValue;

    /// <summary>
    /// Gets one item by specific operator.
    /// </summary>
    /// <param name="op">The operator.</param>
    /// <param name="input">The input collection to compute.</param>
    /// <param name="defaultValue">The default value for empty.</param>
    /// <returns>The result.</returns>
    /// <exception cref="NotSupportedException">The operator is not supported.</exception>
    public static double Merge(NumberCollectionOperators op, IEnumerable<long> input, double defaultValue)
        => op switch
        {
            NumberCollectionOperators.Zero => 0d,
            NumberCollectionOperators.Sum => Merge(input, Enumerable.Sum) ?? defaultValue,
            NumberCollectionOperators.Mean => Merge(input, Enumerable.Average) ?? defaultValue,
            NumberCollectionOperators.Median => Median(input) ?? defaultValue,
            NumberCollectionOperators.Min => Min(input) ?? defaultValue,
            NumberCollectionOperators.Max => Max(input) ?? defaultValue,
            NumberCollectionOperators.First => ListExtensions.FirstOrNull(input) ?? defaultValue,
            NumberCollectionOperators.Last => ListExtensions.LastOrNull(input) ?? defaultValue,
            NumberCollectionOperators.Count => input.Count(),
            _ => throw NotSupported(op),
        };

    /// <summary>
    /// Gets one item by specific operator.
    /// </summary>
    /// <param name="op">The operator.</param>
    /// <param name="input">The input collection to compute.</param>
    /// <returns>The result.</returns>
    /// <exception cref="NotSupportedException">The operator is not supported.</exception>
    public static long? Merge(NumberCollectionOperators op, IEnumerable<long> input)
        => op switch
        {
            NumberCollectionOperators.Zero => 0L,
            NumberCollectionOperators.Sum => Merge(input, Enumerable.Sum) ?? 0L,
            NumberCollectionOperators.Mean => MergeToInt64(input, Enumerable.Average),
            NumberCollectionOperators.Median => Median(input),
            NumberCollectionOperators.Min => StatisticalMethod.Min(input),
            NumberCollectionOperators.Max => StatisticalMethod.Max(input),
            NumberCollectionOperators.First => ListExtensions.FirstOrNull(input),
            NumberCollectionOperators.Last => ListExtensions.LastOrNull(input),
            NumberCollectionOperators.Count => input?.Count() ?? 0L,
            _ => throw NotSupported(op),
        };

    /// <summary>
    /// Gets one item by specific operator.
    /// </summary>
    /// <param name="op">The operator.</param>
    /// <param name="input">The input collection to compute.</param>
    /// <returns>The result.</returns>
    /// <exception cref="NotSupportedException">The operator is not supported.</exception>
    public static long? Merge(NumberCollectionOperators op, params long[] input)
        => Merge(op, input as IEnumerable<long>);

    /// <summary>
    /// Gets one item by specific operator.
    /// </summary>
    /// <param name="op">The operator.</param>
    /// <param name="input">The input collection to compute.</param>
    /// <param name="defaultValue">The default value for empty.</param>
    /// <returns>The result.</returns>
    /// <exception cref="NotSupportedException">The operator is not supported.</exception>
    public static float Merge(NumberCollectionOperators op, IEnumerable<float> input, float defaultValue)
        => Merge(op, input) ?? defaultValue;

    /// <summary>
    /// Gets one item by specific operator.
    /// </summary>
    /// <param name="op">The operator.</param>
    /// <param name="input">The input collection to compute.</param>
    /// <param name="defaultIsZero">true if the default value is zero; otherwise, false, double.NaN.</param>
    /// <returns>The result.</returns>
    /// <exception cref="NotSupportedException">The operator is not supported.</exception>
    public static float Merge(NumberCollectionOperators op, IEnumerable<float> input, bool defaultIsZero)
        => Merge(op, input) ?? (defaultIsZero ? 0f : float.NaN);

    /// <summary>
    /// Gets one item by specific operator.
    /// </summary>
    /// <param name="op">The operator.</param>
    /// <param name="input">The input collection to compute.</param>
    /// <returns>The result.</returns>
    /// <exception cref="NotSupportedException">The operator is not supported.</exception>
    public static float? Merge(NumberCollectionOperators op, IEnumerable<float> input)
        => op switch
        {
            NumberCollectionOperators.Zero => 0f,
            NumberCollectionOperators.Sum => Merge(input, Enumerable.Sum) ?? 0f,
            NumberCollectionOperators.Mean => Merge(input, Enumerable.Average),
            NumberCollectionOperators.Median => Median(input),
            NumberCollectionOperators.Min => StatisticalMethod.Min(input),
            NumberCollectionOperators.Max => StatisticalMethod.Max(input),
            NumberCollectionOperators.First => ListExtensions.FirstOrNull(input),
            NumberCollectionOperators.Last => ListExtensions.LastOrNull(input),
            NumberCollectionOperators.Count => input?.Count() ?? 0f,
            _ => throw NotSupported(op),
        };

    /// <summary>
    /// Gets one item by specific operator.
    /// </summary>
    /// <param name="op">The operator.</param>
    /// <param name="input">The input collection to compute.</param>
    /// <returns>The result.</returns>
    /// <exception cref="NotSupportedException">The operator is not supported.</exception>
    public static float? Merge(NumberCollectionOperators op, params float[] input)
        => Merge(op, input as IEnumerable<float>);

    /// <summary>
    /// Gets one item by specific operator.
    /// </summary>
    /// <param name="op">The operator.</param>
    /// <param name="input">The input collection to compute.</param>
    /// <param name="defaultValue">The default value for empty.</param>
    /// <returns>The result.</returns>
    /// <exception cref="NotSupportedException">The operator is not supported.</exception>
    public static double Merge(NumberCollectionOperators op, IEnumerable<double> input, double defaultValue)
        => Merge(op, input) ?? defaultValue;

    /// <summary>
    /// Gets one item by specific operator.
    /// </summary>
    /// <param name="op">The operator.</param>
    /// <param name="input">The input collection to compute.</param>
    /// <param name="defaultIsZero">true if the default value is zero; otherwise, false, double.NaN.</param>
    /// <returns>The result.</returns>
    /// <exception cref="NotSupportedException">The operator is not supported.</exception>
    public static double Merge(NumberCollectionOperators op, IEnumerable<double> input, bool defaultIsZero)
        => Merge(op, input) ?? (defaultIsZero ? 0d : double.NaN);

    /// <summary>
    /// Gets one item by specific operator.
    /// </summary>
    /// <param name="op">The operator.</param>
    /// <param name="input">The input collection to compute.</param>
    /// <returns>The result.</returns>
    /// <exception cref="NotSupportedException">The operator is not supported.</exception>
    public static double? Merge(NumberCollectionOperators op, IEnumerable<double> input)
        => op switch
        {
            NumberCollectionOperators.Zero => 0d,
            NumberCollectionOperators.Sum => Merge(input, Enumerable.Sum) ?? 0d,
            NumberCollectionOperators.Mean => Merge(input, Enumerable.Average),
            NumberCollectionOperators.Median => Median(input),
            NumberCollectionOperators.Min => StatisticalMethod.Min(input),
            NumberCollectionOperators.Max => StatisticalMethod.Max(input),
            NumberCollectionOperators.First => ListExtensions.FirstOrNull(input),
            NumberCollectionOperators.Last => ListExtensions.LastOrNull(input),
            NumberCollectionOperators.Count => input?.Count() ?? 0d,
            _ => throw NotSupported(op),
        };

    /// <summary>
    /// Gets one item by specific operator.
    /// </summary>
    /// <param name="op">The operator.</param>
    /// <param name="input">The input collection to compute.</param>
    /// <returns>The result.</returns>
    /// <exception cref="NotSupportedException">The operator is not supported.</exception>
    public static double? Merge(NumberCollectionOperators op, params double[] input)
        => Merge(op, input as IEnumerable<double>);

    /// <summary>
    /// Gets one item by specific operator.
    /// </summary>
    /// <param name="op">The operator.</param>
    /// <param name="input">The input collection to compute.</param>
    /// <param name="defaultValue">The default value for empty.</param>
    /// <returns>The result.</returns>
    /// <exception cref="NotSupportedException">The operator is not supported.</exception>
    public static decimal Merge(NumberCollectionOperators op, IEnumerable<decimal> input, decimal defaultValue)
        => Merge(op, input) ?? defaultValue;

    /// <summary>
    /// Gets one item by specific operator.
    /// </summary>
    /// <param name="op">The operator.</param>
    /// <param name="input">The input collection to compute.</param>
    /// <returns>The result.</returns>
    /// <exception cref="NotSupportedException">The operator is not supported.</exception>
    public static decimal? Merge(NumberCollectionOperators op, IEnumerable<decimal> input)
        => op switch
        {
            NumberCollectionOperators.Zero => decimal.Zero,
            NumberCollectionOperators.Sum => Merge(input, Enumerable.Sum) ?? decimal.Zero,
            NumberCollectionOperators.Mean => Merge(input, Enumerable.Average),
            NumberCollectionOperators.Median => Median(input),
            NumberCollectionOperators.Min => StatisticalMethod.Min(input),
            NumberCollectionOperators.Max => StatisticalMethod.Max(input),
            NumberCollectionOperators.First => ListExtensions.FirstOrNull(input),
            NumberCollectionOperators.Last => ListExtensions.LastOrNull(input),
            NumberCollectionOperators.Count => input?.Count() ?? decimal.Zero,
            _ => throw NotSupported(op),
        };

    /// <summary>
    /// Gets one item by specific operator.
    /// </summary>
    /// <param name="op">The operator.</param>
    /// <param name="input">The input collection to compute.</param>
    /// <returns>The result.</returns>
    /// <exception cref="NotSupportedException">The operator is not supported.</exception>
    public static decimal? Merge(NumberCollectionOperators op, params decimal[] input)
        => Merge(op, input as IEnumerable<decimal>);

    private static TResult? Merge<TItem, TResult>(IEnumerable<TItem> col, Func<IEnumerable<TItem>, TResult> func)
        where TItem : struct
        where TResult : struct
    {
        if (col is null) return null;
        if (col is ICollection<TItem> col2) return col2.Count > 0 ? func(col2) : null;
        if (col is TItem[] col3) return col3.Length > 0 ? func(col3) : null;
        try
        {
            return func(col);
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    private static int? MergeToInt32<TItem>(IEnumerable<TItem> col, Func<IEnumerable<TItem>, double> func) where TItem : struct
    {
        var result = Merge(col, func);
        return result.HasValue ? (int)Math.Round(result.Value) : null;
    }

    private static long? MergeToInt64<TItem>(IEnumerable<TItem> col, Func<IEnumerable<TItem>, double> func) where TItem : struct
    {
        var result = Merge(col, func);
        return result.HasValue ? (long)Math.Round(result.Value) : null;
    }

    private static T? Min<T>(IEnumerable<T> col) where T : struct, IComparable
    {
        var v = StatisticalMethod.Min(col, out var i);
        return i < 0 ? null : v;
    }

    private static T? Max<T>(IEnumerable<T> col) where T : struct, IComparable
    {
        var v = StatisticalMethod.Max(col, out var i);
        return i < 0 ? null : v;
    }

    private static T? Median<T>(IEnumerable<T> col) where T : struct
    {
        if (col is null) return null;
        return StatisticalMethod.Median(col, out var i);
    }
}
