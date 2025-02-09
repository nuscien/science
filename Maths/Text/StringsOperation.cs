using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trivial.Collection;
using Trivial.Text;

namespace Trivial.Maths;

public static partial class CollectionOperations
{
    /// <summary>
    /// Gets one item by specific operator.
    /// </summary>
    /// <param name="op">The operator.</param>
    /// <param name="input">The input collection to compute.</param>
    /// <param name="options">The options.</param>
    /// <returns>The result.</returns>
    /// <exception cref="NotSupportedException">The operator is not supported.</exception>
    public static string Merge(StringCollectionOperators op, IEnumerable<string> input, StringCollectionMergeOptions options)
    {
        if (input == null) return null;
        if (input is string[] arr)
        {
            if (arr.Length < 1) return null;
        }
        else if (input is ICollection<string> col)
        {
            if (col.Count < 1) return null;
        }
        else
        {
            col = input?.ToList();
            input = col;
            if (col.Count < 1) return null;
        }

        if (options?.SkipNullOrEmpty == true) input = input.Where(ele => !string.IsNullOrEmpty(ele));
        switch (op)
        {
            case StringCollectionOperators.Empty:
                return string.Empty;
            case StringCollectionOperators.Join:
                return string.Join(string.Empty, input);
            case StringCollectionOperators.Lines:
                return string.Join(options?.NewLineUseN == true ? Environment.NewLine : "\n", input);
            case StringCollectionOperators.Tabs:
                return Join('\t', input);
            case StringCollectionOperators.Tags:
                return options?.AppendWhiteSpaceAfterComma == true ? string.Join("; ", input) : Join(';', input);
            case StringCollectionOperators.Commas:
                return options?.AppendWhiteSpaceAfterComma == true ? string.Join(", ", input) : Join(',', input);
            case StringCollectionOperators.Dots:
                return Join('.', input);
            case StringCollectionOperators.Slashes:
                return Join('/', input);
            case StringCollectionOperators.VerticalLines:
                return Join('|', input);
            case StringCollectionOperators.VerticalLineSeparators:
                return string.Join(" | ", input);
            case StringCollectionOperators.WhiteSpaces:
                return Join(' ', input);
            case StringCollectionOperators.DoubleWhiteSpaces:
                return string.Join("  ", input);
            case StringCollectionOperators.TripleWhiteSpaces:
                return string.Join("   ", input);
            case StringCollectionOperators.QuadrupleWhiteSpaces:
                return string.Join("    ", input);
            case StringCollectionOperators.And:
                return string.Join(" & ", input);
            case StringCollectionOperators.SplitPoints:
                return string.Join(" · ", input);
            case StringCollectionOperators.JsonArray:
                {
                    var json = new JsonArrayNode();
                    json.AddRange(input);
                    return json.ToString();
                }
            case StringCollectionOperators.Bullet:
                input = input.Select(ele => string.Concat("· ", ele));
                return string.Join(options?.NewLineUseN == true ? Environment.NewLine : "\n", input);
            case StringCollectionOperators.Numbering:
                {
                    input = input.Select((ele, i) => string.Concat(i, ' ', '\t', ele));
                    return string.Join(options?.NewLineUseN == true ? Environment.NewLine : "\n", input);
                }
            case StringCollectionOperators.First:
                return input.First();
            case StringCollectionOperators.Last:
                return input.Last();
            case StringCollectionOperators.Longest:
                {
                    int i = 0;
                    string s = null;
                    foreach (var item in input)
                    {
                        if (item == null || item.Length < i) continue;
                        if (item.Length == i)
                        {
                            if (i == 0) s = string.Empty;
                            continue;
                        }

                        s = item;
                        i = item.Length;
                    }

                    return s;
                }
            case StringCollectionOperators.LastLongest:
                {
                    int i = 0;
                    string s = null;
                    foreach (var item in input)
                    {
                        if (item == null || item.Length < i) continue;
                        if (i == 0) s = string.Empty;
                        s = item;
                        i = item.Length;
                    }

                    return s;
                }
            case StringCollectionOperators.Shortest:
                {
                    int i = -1;
                    string s = null;
                    foreach (var item in input)
                    {
                        if (item == null) return null;
                        if (i >= 0 && item.Length >= i) continue;
                        s = item;
                        i = item.Length;
                    }

                    return s;
                }
            case StringCollectionOperators.LastShortest:
                {
                    int i = -1;
                    string s = null;
                    foreach (var item in input)
                    {
                        if (item == null) return null;
                        if (i >= 0 && item.Length > i) continue;
                        s = item;
                        i = item.Length;
                    }

                    return s;
                }
            case StringCollectionOperators.AscBinaryEncode:
                {
                    string s = null;
                    foreach (var item in input)
                    {
                        if (item == null) continue;
                        if (s == null || item.CompareTo(s) < 0) s = item;
                    }

                    return s;
                }
            case StringCollectionOperators.DescBinaryEncode:
                {
                    string s = null;
                    foreach (var item in input)
                    {
                        if (item == null) continue;
                        if (s == null || item.CompareTo(s) > 0) s = item;
                    }

                    return s;
                }
            default:
                throw NotSupported(op);
        }
    }

    /// <summary>
    /// Gets one item by specific operator.
    /// </summary>
    /// <param name="op">The operator.</param>
    /// <param name="input">The input collection to compute.</param>
    /// <returns>The result.</returns>
    /// <exception cref="NotSupportedException">The operator is not supported.</exception>
    public static string Merge(StringCollectionOperators op, IEnumerable<string> input)
        => Merge(op, input, null);

    /// <summary>
    /// Gets one item by specific operator.
    /// </summary>
    /// <param name="op">The operator.</param>
    /// <param name="input">The input collection to compute.</param>
    /// <param name="options">The options.</param>
    /// <returns>The result.</returns>
    /// <exception cref="NotSupportedException">The operator is not supported.</exception>
    public static string Merge(StringCollectionOperators op, StringCollectionMergeOptions options, params string[] input)
        => Merge(op, input, options);

    /// <summary>
    /// Gets one item by specific operator.
    /// </summary>
    /// <param name="op">The operator.</param>
    /// <param name="input">The input collection to compute.</param>
    /// <returns>The result.</returns>
    /// <exception cref="NotSupportedException">The operator is not supported.</exception>
    public static string Merge(StringCollectionOperators op, params string[] input)
        => Merge(op, input, null);

    private static string Join(char seperator, IEnumerable<string> input)
#if NET6_0_OR_GREATER
        => string.Join(seperator, input);
#else
        => string.Join(seperator.ToString(), input);
#endif
}
