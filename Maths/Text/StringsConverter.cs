﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trivial.Text;

namespace Trivial.Maths;

public static partial class CollectionOperation
{
    /// <summary>
    /// Generates a string collection.
    /// </summary>
    /// <param name="count">The count.</param>
    /// <param name="value">The default value to fill.</param>
    /// <returns>A new string list.</returns>
    public static List<string> ToStrings(int count, string value = null)
    {
        var arr = new List<string>();
        for (int i = 0; i < count; i++)
        {
            arr.Add(value);
        }

        return arr;
    }

    /// <summary>
    /// Converts a collection of boolean to strings.
    /// </summary>
    /// <param name="input">The input collection.</param>
    /// <returns>A new string list.</returns>
    public static IEnumerable<string> ToStrings(IEnumerable<JsonStringNode> input)
    {
        foreach (var item in input)
        {
            yield return item?.StringValue;
        }
    }

    /// <summary>
    /// Converts a collection of boolean to strings.
    /// </summary>
    /// <param name="input">The input collection.</param>
    /// <param name="trueValue">The value for true.</param>
    /// <param name="falseValue">The value for false.</param>
    /// <returns>The string collection converted.</returns>
    public static IEnumerable<string> ToStrings(IEnumerable<bool> input, string trueValue, string falseValue)
    {
        if (input == null) yield break;
        foreach (var item in input)
        {
            yield return item ? trueValue : falseValue;
        }
    }

    /// <summary>
    /// Converts a collection of boolean to strings.
    /// </summary>
    /// <param name="input">The input collection.</param>
    /// <param name="trueValue">The value for true.</param>
    /// <param name="falseValue">The value for false.</param>
    /// <returns>The string collection converted.</returns>
    public static string[] ToStrings(bool[] input, string trueValue, string falseValue)
    {
        if (input == null) return null;
        var arr = new string[input.Length];
        for (var i = 0; i < input.Length; i++)
        {
            arr[i] = input[i] ? trueValue : falseValue;
        }

        return arr;
    }

    /// <summary>
    /// Converts a collection of boolean to strings.
    /// </summary>
    /// <param name="input">The input collection.</param>
    /// <param name="provider">An optional format provider.</param>
    /// <returns>The string collection converted.</returns>
    public static IEnumerable<string> ToStrings(IEnumerable<bool> input, IFormatProvider provider = null)
    {
        if (input == null) yield break;
        if (provider == null)
        {
            foreach (var item in input)
            {
                yield return item.ToString();
            }
        }
        else
        {
            foreach (var item in input)
            {
                yield return item.ToString(provider);
            }
        }
    }

    /// <summary>
    /// Converts a collection of boolean to strings.
    /// </summary>
    /// <param name="input">The input collection.</param>
    /// <param name="provider">An optional format provider.</param>
    /// <returns>The string collection converted.</returns>
    public static string[] ToStrings(bool[] input, IFormatProvider provider = null)
    {
        if (input == null) return null;
        var arr = new string[input.Length];
        if (provider == null)
        {
            for (var i = 0; i < input.Length; i++)
            {
                arr[i] = input[i].ToString();
            }
        }
        else
        {
            for (var i = 0; i < input.Length; i++)
            {
                arr[i] = input[i].ToString(provider);
            }
        }

        return arr;
    }

    /// <summary>
    /// Converts a collection of boolean to strings.
    /// </summary>
    /// <param name="input">The input collection.</param>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <param name="provider">An optional format provider.</param>
    /// <returns>The string collection converted.</returns>
    public static IEnumerable<string> ToStrings(IEnumerable<int> input, string format = null, IFormatProvider provider = null)
    {
        if (input == null) yield break;
        if (format == null)
        {
            if (provider == null)
            {
                foreach (var item in input)
                {
                    yield return item.ToString();
                }
            }
            else
            {
                foreach (var item in input)
                {
                    yield return item.ToString(provider);
                }
            }

            yield break;
        }

        if (provider == null)
        {
            foreach (var item in input)
            {
                yield return item.ToString(format);
            }

            yield break;
        }

        foreach (var item in input)
        {
            yield return item.ToString(format, provider);
        }
    }

    /// <summary>
    /// Converts a collection of boolean to strings.
    /// </summary>
    /// <param name="input">The input collection.</param>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <param name="provider">An optional format provider.</param>
    /// <returns>The string collection converted.</returns>
    public static string[] ToStrings(int[] input, string format = null, IFormatProvider provider = null)
    {
        if (input == null) return null;
        var arr = new string[input.Length];
        if (format == null)
        {
            if (provider == null)
            {
                for (var i = 0; i < input.Length; i++)
                {
                    arr[i] = input[i].ToString();
                }
            }
            else
            {
                for (var i = 0; i < input.Length; i++)
                {
                    arr[i] = input[i].ToString(provider);
                }
            }

            return arr;
        }

        if (provider == null)
        {
            for (var i = 0; i < input.Length; i++)
            {
                arr[i] = input[i].ToString(format);
            }
        }
        else
        {
            for (var i = 0; i < input.Length; i++)
            {
                arr[i] = input[i].ToString(format, provider);
            }
        }

        return arr;
    }

    /// <summary>
    /// Converts a collection of boolean to strings.
    /// </summary>
    /// <param name="input">The input collection.</param>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <param name="provider">An optional format provider.</param>
    /// <returns>The string collection converted.</returns>
    public static IEnumerable<string> ToStrings(IEnumerable<long> input, string format = null, IFormatProvider provider = null)
    {
        if (input == null) yield break;
        if (format == null)
        {
            if (provider == null)
            {
                foreach (var item in input)
                {
                    yield return item.ToString();
                }
            }
            else
            {
                foreach (var item in input)
                {
                    yield return item.ToString(provider);
                }
            }

            yield break;
        }

        if (provider == null)
        {
            foreach (var item in input)
            {
                yield return item.ToString(format);
            }

            yield break;
        }

        foreach (var item in input)
        {
            yield return item.ToString(format, provider);
        }
    }

    /// <summary>
    /// Converts a collection of boolean to strings.
    /// </summary>
    /// <param name="input">The input collection.</param>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <param name="provider">An optional format provider.</param>
    /// <returns>The string collection converted.</returns>
    public static string[] ToStrings(long[] input, string format = null, IFormatProvider provider = null)
    {
        if (input == null) return null;
        var arr = new string[input.Length];
        if (format == null)
        {
            if (provider == null)
            {
                for (var i = 0; i < input.Length; i++)
                {
                    arr[i] = input[i].ToString();
                }
            }
            else
            {
                for (var i = 0; i < input.Length; i++)
                {
                    arr[i] = input[i].ToString(provider);
                }
            }

            return arr;
        }

        if (provider == null)
        {
            for (var i = 0; i < input.Length; i++)
            {
                arr[i] = input[i].ToString(format);
            }
        }
        else
        {
            for (var i = 0; i < input.Length; i++)
            {
                arr[i] = input[i].ToString(format, provider);
            }
        }

        return arr;
    }

    /// <summary>
    /// Converts a collection of boolean to strings.
    /// </summary>
    /// <param name="input">The input collection.</param>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <param name="provider">An optional format provider.</param>
    /// <returns>The string collection converted.</returns>
    public static IEnumerable<string> ToStrings(IEnumerable<float> input, string format = null, IFormatProvider provider = null)
    {
        if (input == null) yield break;
        if (format == null)
        {
            if (provider == null)
            {
                foreach (var item in input)
                {
                    yield return item.ToString();
                }
            }
            else
            {
                foreach (var item in input)
                {
                    yield return item.ToString(provider);
                }
            }

            yield break;
        }

        if (provider == null)
        {
            foreach (var item in input)
            {
                yield return item.ToString(format);
            }

            yield break;
        }

        foreach (var item in input)
        {
            yield return item.ToString(format, provider);
        }
    }

    /// <summary>
    /// Converts a collection of boolean to strings.
    /// </summary>
    /// <param name="input">The input collection.</param>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <param name="provider">An optional format provider.</param>
    /// <returns>The string collection converted.</returns>
    public static string[] ToStrings(float[] input, string format = null, IFormatProvider provider = null)
    {
        if (input == null) return null;
        var arr = new string[input.Length];
        if (format == null)
        {
            if (provider == null)
            {
                for (var i = 0; i < input.Length; i++)
                {
                    arr[i] = input[i].ToString();
                }
            }
            else
            {
                for (var i = 0; i < input.Length; i++)
                {
                    arr[i] = input[i].ToString(provider);
                }
            }

            return arr;
        }

        if (provider == null)
        {
            for (var i = 0; i < input.Length; i++)
            {
                arr[i] = input[i].ToString(format);
            }
        }
        else
        {
            for (var i = 0; i < input.Length; i++)
            {
                arr[i] = input[i].ToString(format, provider);
            }
        }

        return arr;
    }

    /// <summary>
    /// Converts a collection of boolean to strings.
    /// </summary>
    /// <param name="input">The input collection.</param>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <param name="provider">An optional format provider.</param>
    /// <returns>The string collection converted.</returns>
    public static IEnumerable<string> ToStrings(IEnumerable<double> input, string format = null, IFormatProvider provider = null)
    {
        if (input == null) yield break;
        if (format == null)
        {
            if (provider == null)
            {
                foreach (var item in input)
                {
                    yield return item.ToString();
                }
            }
            else
            {
                foreach (var item in input)
                {
                    yield return item.ToString(provider);
                }
            }

            yield break;
        }

        if (provider == null)
        {
            foreach (var item in input)
            {
                yield return item.ToString(format);
            }

            yield break;
        }

        foreach (var item in input)
        {
            yield return item.ToString(format, provider);
        }
    }

    /// <summary>
    /// Converts a collection of boolean to strings.
    /// </summary>
    /// <param name="input">The input collection.</param>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <param name="provider">An optional format provider.</param>
    /// <returns>The string collection converted.</returns>
    public static string[] ToStrings(double[] input, string format = null, IFormatProvider provider = null)
    {
        if (input == null) return null;
        var arr = new string[input.Length];
        if (format == null)
        {
            if (provider == null)
            {
                for (var i = 0; i < input.Length; i++)
                {
                    arr[i] = input[i].ToString();
                }
            }
            else
            {
                for (var i = 0; i < input.Length; i++)
                {
                    arr[i] = input[i].ToString(provider);
                }
            }

            return arr;
        }

        if (provider == null)
        {
            for (var i = 0; i < input.Length; i++)
            {
                arr[i] = input[i].ToString(format);
            }
        }
        else
        {
            for (var i = 0; i < input.Length; i++)
            {
                arr[i] = input[i].ToString(format, provider);
            }
        }

        return arr;
    }

    /// <summary>
    /// Converts a collection of boolean to strings.
    /// </summary>
    /// <param name="input">The input collection.</param>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <param name="provider">An optional format provider.</param>
    /// <returns>The string collection converted.</returns>
    public static IEnumerable<string> ToStrings(IEnumerable<decimal> input, string format = null, IFormatProvider provider = null)
    {
        if (input == null) yield break;
        if (format == null)
        {
            if (provider == null)
            {
                foreach (var item in input)
                {
                    yield return item.ToString();
                }
            }
            else
            {
                foreach (var item in input)
                {
                    yield return item.ToString(provider);
                }
            }

            yield break;
        }

        if (provider == null)
        {
            foreach (var item in input)
            {
                yield return item.ToString(format);
            }

            yield break;
        }

        foreach (var item in input)
        {
            yield return item.ToString(format, provider);
        }
    }

    /// <summary>
    /// Converts a collection of boolean to strings.
    /// </summary>
    /// <param name="input">The input collection.</param>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <param name="provider">An optional format provider.</param>
    /// <returns>The string collection converted.</returns>
    public static string[] ToStrings(decimal[] input, string format = null, IFormatProvider provider = null)
    {
        if (input == null) return null;
        var arr = new string[input.Length];
        if (format == null)
        {
            if (provider == null)
            {
                for (var i = 0; i < input.Length; i++)
                {
                    arr[i] = input[i].ToString();
                }
            }
            else
            {
                for (var i = 0; i < input.Length; i++)
                {
                    arr[i] = input[i].ToString(provider);
                }
            }

            return arr;
        }

        if (provider == null)
        {
            for (var i = 0; i < input.Length; i++)
            {
                arr[i] = input[i].ToString(format);
            }
        }
        else
        {
            for (var i = 0; i < input.Length; i++)
            {
                arr[i] = input[i].ToString(format, provider);
            }
        }

        return arr;
    }

    /// <summary>
    /// Converts a collection of boolean to strings.
    /// </summary>
    /// <param name="input">The input collection.</param>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <returns>The string collection converted.</returns>
    public static IEnumerable<string> ToStrings(IEnumerable<Guid> input, string format = null)
    {
        if (input == null) yield break;
        if (format == null)
        {
            foreach (var item in input)
            {
                yield return item.ToString();
            }
        }
        else
        {
            foreach (var item in input)
            {
                yield return item.ToString(format);
            }
        }
    }

    /// <summary>
    /// Converts a collection of boolean to strings.
    /// </summary>
    /// <param name="input">The input collection.</param>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <returns>The string collection converted.</returns>
    public static string[] ToStrings(DateTime[] input, string format = null)
    {
        if (input == null) return null;
        var arr = new string[input.Length];
        if (format == null)
        {
            for (var i = 0; i < input.Length; i++)
            {
                arr[i] = input[i].ToString();
            }
        }
        else
        {
            for (var i = 0; i < input.Length; i++)
            {
                arr[i] = input[i].ToString(format);
            }
        }

        return arr;
    }

    /// <summary>
    /// Converts a collection of boolean to strings.
    /// </summary>
    /// <param name="input">The input collection.</param>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <param name="provider">An optional format provider.</param>
    /// <returns>The string collection converted.</returns>
    public static IEnumerable<string> ToStrings(IEnumerable<TimeSpan> input, string format = null, IFormatProvider provider = null)
    {
        if (input == null) yield break;
        if (format == null)
        {
            foreach (var item in input)
            {
                yield return item.ToString();
            }
        }
        else
        {
            foreach (var item in input)
            {
                yield return item.ToString(format, provider);
            }
        }
    }

    /// <summary>
    /// Converts a collection of boolean to strings.
    /// </summary>
    /// <param name="input">The input collection.</param>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <param name="provider">An optional format provider.</param>
    /// <returns>The string collection converted.</returns>
    public static string[] ToStrings(TimeSpan[] input, string format = null, IFormatProvider provider = null)
    {
        if (input == null) return null;
        var arr = new string[input.Length];
        if (format == null)
        {
            for (var i = 0; i < input.Length; i++)
            {
                arr[i] = input[i].ToString();
            }
        }
        else
        {
            for (var i = 0; i < input.Length; i++)
            {
                arr[i] = input[i].ToString(format, provider);
            }
        }

        return arr;
    }

    /// <summary>
    /// Converts a collection of boolean to strings.
    /// </summary>
    /// <param name="input">The input collection.</param>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <param name="provider">An optional format provider.</param>
    /// <returns>The string collection converted.</returns>
    public static IEnumerable<string> ToStrings(IEnumerable<DateTime> input, string format = null, IFormatProvider provider = null)
    {
        if (input == null) yield break;
        foreach (var item in input)
        {
            yield return item.ToString(format, provider);
        }
    }

    /// <summary>
    /// Converts a collection of boolean to strings.
    /// </summary>
    /// <param name="input">The input collection.</param>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <param name="provider">An optional format provider.</param>
    /// <returns>The string collection converted.</returns>
    public static string[] ToStrings(DateTime[] input, string format = null, IFormatProvider provider = null)
    {
        if (input == null) return null;
        var arr = new string[input.Length];
        for (var i = 0; i < input.Length; i++)
        {
            arr[i] = input[i].ToString(format, provider);
        }

        return arr;
    }

    /// <summary>
    /// Converts a collection of boolean to strings.
    /// </summary>
    /// <param name="input">The input collection.</param>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <param name="provider">An optional format provider.</param>
    /// <returns>The string collection converted.</returns>
    public static IEnumerable<string> ToStrings(IEnumerable<DateTimeOffset> input, string format = null, IFormatProvider provider = null)
    {
        if (input == null) yield break;
        foreach (var item in input)
        {
            yield return item.ToString(format, provider);
        }
    }

    /// <summary>
    /// Converts a collection of boolean to strings.
    /// </summary>
    /// <param name="input">The input collection.</param>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <param name="provider">An optional format provider.</param>
    /// <returns>The string collection converted.</returns>
    public static string[] ToStrings(DateTimeOffset[] input, string format = null, IFormatProvider provider = null)
    {
        if (input == null) return null;
        var arr = new string[input.Length];
        for (var i = 0; i < input.Length; i++)
        {
            arr[i] = input[i].ToString(format, provider);
        }

        return arr;
    }

#if NET6_0_OR_GREATER
    /// <summary>
    /// Converts a collection of boolean to strings.
    /// </summary>
    /// <param name="input">The input collection.</param>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <param name="provider">An optional format provider.</param>
    /// <returns>The string collection converted.</returns>
    public static IEnumerable<string> ToStrings(IEnumerable<DateOnly> input, string format = null, IFormatProvider provider = null)
    {
        if (input == null) yield break;
        foreach (var item in input)
        {
            yield return item.ToString(format, provider);
        }
    }

    /// <summary>
    /// Converts a collection of boolean to strings.
    /// </summary>
    /// <param name="input">The input collection.</param>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <param name="provider">An optional format provider.</param>
    /// <returns>The string collection converted.</returns>
    public static string[] ToStrings(DateOnly[] input, string format = null, IFormatProvider provider = null)
    {
        if (input == null) return null;
        var arr = new string[input.Length];
        for (var i = 0; i < input.Length; i++)
        {
            arr[i] = input[i].ToString(format, provider);
        }

        return arr;
    }

    /// <summary>
    /// Converts a collection of boolean to strings.
    /// </summary>
    /// <param name="input">The input collection.</param>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <param name="provider">An optional format provider.</param>
    /// <returns>The string collection converted.</returns>
    public static IEnumerable<string> ToStrings(IEnumerable<TimeOnly> input, string format = null, IFormatProvider provider = null)
    {
        if (input == null) yield break;
        foreach (var item in input)
        {
            yield return item.ToString(format, provider);
        }
    }

    /// <summary>
    /// Converts a collection of boolean to strings.
    /// </summary>
    /// <param name="input">The input collection.</param>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <param name="provider">An optional format provider.</param>
    /// <returns>The string collection converted.</returns>
    public static string[] ToStrings(TimeOnly[] input, string format = null, IFormatProvider provider = null)
    {
        if (input == null) return null;
        var arr = new string[input.Length];
        for (var i = 0; i < input.Length; i++)
        {
            arr[i] = input[i].ToString(format, provider);
        }
        
        return arr;
    }
#endif
}
