﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Arithmetic\Basic.cs" company="Nanchang Jinchen Software Co., Ltd.">
//   Copyright (c) 2010 Nanchang Jinchen Software Co., Ltd. All rights reserved.
// </copyright>
// <summary>
//   The basic arithmetic functions.
// </summary>
// <author>Kingcean Tuan</author>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trivial.Maths
{
    /// <summary>
    /// The utility for arithmetic.
    /// </summary>
    public static partial class Arithmetic
    {
        private const string num36 = "0123456789abcdefghijklmnopqrstuvwxyz";

        /// <summary>
        /// Gets a result of factorial for a specific number.
        /// </summary>
        /// <param name="value">A number to calculate.</param>
        /// <returns>A number of result.</returns>
        /// <example>
        /// <code>
        /// var factorialNum = Arithmetic.Factorial(20); // => 2432902008176640000
        /// </code>
        /// </example>
        public static long Factorial(uint value)
        {
            if (value < 2) return 1;
            var resultNum = (long)value;
            for (uint step = 2; step < value; step++)
            {
                resultNum *= step;
            }

            return resultNum;
        }

        /// <summary>
        /// Gets a result of factorial for a specific number.
        /// </summary>
        /// <param name="value">A number to calculate.</param>
        /// <returns>A number of result.</returns>
        /// <example>
        /// <code>
        /// var factorialNum = Arithmetic.FactorialApproximate(100); // 9.33262154439442e+157
        /// </code>
        /// </example>
        public static double FactorialApproximate(uint value)
        {
            if (value < 2) return 1;
            var resultNum = (double)value;
            for (double step = 2; step < value; step++)
            {
                resultNum *= step;
            }

            return resultNum;
        }

        /// <summary>
        /// Calculates the value times 1024 of the specific power.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="e">The exponential.</param>
        /// <returns>The result calculated.</returns>
        /// <remarks>You can use this to calculate such as 80K or 4M.</remarks>
        public static long Times1024(int value, int e = 1)
        {
            return value * (long)Math.Pow(1024, e);
        }

        /// <summary>
        /// Calculates the value times 1024 of the specific power.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="e">The exponential.</param>
        /// <returns>The result calculated.</returns>
        /// <remarks>You can use this to calculate such as 80K or 4M.</remarks>
        public static double Times1024(long value, int e = 1)
        {
            return value * Math.Pow(1024, e);
        }

        /// <summary>
        /// Converts a number to a specific positional notation format string.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <param name="radix">The positional notation. Should be an integer in 2-36.</param>
        /// <returns>A string of the number in the specific positional notation.</returns>
        /// <exception cref="ArgumentOutOfRangeException">radix was less than 2 or greater than 36.</exception>
        public static string ToPositionalNotationString(short value, int radix)
        {
            return ToPositionalNotationString((long)value, radix);
        }

        /// <summary>
        /// Converts a number to a specific positional notation format string.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <param name="radix">The positional notation. Should be an integer in 2-36.</param>
        /// <returns>A string of the number in the specific positional notation.</returns>
        /// <exception cref="ArgumentOutOfRangeException">radix was less than 2 or greater than 36.</exception>
        public static string ToPositionalNotationString(int value, int radix)
        {
            return ToPositionalNotationString((long)value, radix);
        }

        /// <summary>
        /// Converts a number to a specific positional notation format string.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <param name="radix">The positional notation. Should be an integer in 2-36.</param>
        /// <returns>A string of the number in the specific positional notation.</returns>
        /// <exception cref="ArgumentOutOfRangeException">radix was less than 2 or greater than 36.</exception>
        public static string ToPositionalNotationString(uint value, int radix)
        {
            return ToPositionalNotationString((long)value, radix);
        }

        /// <summary>
        /// Converts a number to a specific positional notation format string.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <param name="radix">The positional notation. Should be an integer in 2-36.</param>
        /// <returns>A string of the number in the specific positional notation.</returns>
        /// <exception cref="ArgumentOutOfRangeException">radix was less than 2 or greater than 36.</exception>
        public static string ToPositionalNotationString(long value, int radix)
        {
            if (radix < 2 || radix > 36) throw new ArgumentOutOfRangeException(nameof(radix), "radix should be in 2-36.");
            var integerStr = string.Empty;
            var integerPart = Math.Abs(value);
            if (integerPart == 0) return "0";
            while (integerPart != 0)
            {
                integerStr = num36[(int)(integerPart % radix)] + integerStr;
                integerPart /= radix;
            }

            if (value < 0) return "-" + integerStr;
            return integerStr;
        }

        /// <summary>
        /// Converts a number to a specific positional notation format string.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <param name="radix">The positional notation. Should be an integer in 2-36.</param>
        /// <returns>A string of the number in the specific positional notation.</returns>
        /// <exception cref="ArgumentOutOfRangeException">radix was less than 2 or greater than 36.</exception>
        public static string ToPositionalNotationString(float value, int radix)
        {
            return ToPositionalNotationString((double)value, radix);
        }

        /// <summary>
        /// Converts a number to a specific positional notation format string.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <param name="radix">The positional notation. Should be an integer in 2-36.</param>
        /// <returns>A string of the number in the specific positional notation.</returns>
        /// <exception cref="ArgumentOutOfRangeException">radix was less than 2 or greater than 36.</exception>
        public static string ToPositionalNotationString(double value, int radix)
        {
            if (radix < 2 || radix > 36) throw new ArgumentOutOfRangeException(nameof(radix), "radix should be in 2-36.");
            var integerStr = string.Empty;
            var fractionalStr = string.Empty;
            var integerPart = Math.Abs((long)value);
            var fractionalPart = Math.Abs(value) - integerPart;
            if (integerPart == 0)
            {
                integerStr = "0";
            }

            while (integerPart != 0)
            {
                integerStr = num36[(int)(integerPart % radix)] + integerStr;
                integerPart /= radix;
            }

            for (int i = 0; i < 10; i++)
            {
                if (fractionalPart == 0)
                {
                    break;
                }

                var pos = (int)(fractionalPart * radix);
                if (pos < 35 && Math.Abs(pos + 1 - fractionalPart * radix) < 0.00000000001)
                {
                    fractionalStr += num36[pos + 1];
                    break;
                }

                fractionalStr += num36[pos];
                fractionalPart = fractionalPart * radix - pos;
            }

            while (fractionalStr.Length > 0 && fractionalStr.LastIndexOf('0') == (fractionalStr.Length - 1))
                fractionalStr = fractionalStr.Remove(fractionalStr.Length - 1);

            var str = new StringBuilder();
            if (value < 0) str.Append('-');
            str.Append(integerStr);
            if (!string.IsNullOrEmpty(fractionalStr))
            {
                str.Append('.');
                str.Append(fractionalStr);
            }

            return str.ToString();
        }

        /// <summary>
        /// Parses a string to a number.
        /// </summary>
        /// <param name="s">The input string.</param>
        /// <param name="radix">The positional notation. Should be an integer in 2-36.</param>
        /// <exception cref="ArgumentNullException">s was null.</exception>
        /// <exception cref="ArgumentException">s was empty or consists only of white-space characters..</exception>
        /// <exception cref="ArgumentOutOfRangeException">radix was less than 2 or greater than 36.</exception>
        /// <exception cref="FormatException">s was in an incorrect format.</exception>
        /// <returns>A number parsed.</returns>
        public static short ParseToInt16(string s, int radix)
        {
            var result = ParseToInt32(s, radix);
            if (result >= short.MinValue && result <= short.MaxValue)
                return (short)result;
            throw new FormatException("s was too small or too large.", new OverflowException("s was too small or too large."));
        }

        /// <summary>
        /// Parses a string to a number.
        /// </summary>
        /// <param name="s">The input string.</param>
        /// <param name="radix">The positional notation. Should be an integer in 2-36.</param>
        /// <exception cref="ArgumentNullException">s was null.</exception>
        /// <exception cref="ArgumentException">s was empty or consists only of white-space characters..</exception>
        /// <exception cref="ArgumentOutOfRangeException">radix was less than 2 or greater than 36.</exception>
        /// <exception cref="FormatException">s was in an incorrect format.</exception>
        /// <returns>A number parsed.</returns>
        public static int ParseToInt32(string s, int radix)
        {
            if (s == null) throw new ArgumentNullException(nameof(s), "s should not be null.");
            if (string.IsNullOrWhiteSpace(s)) throw new ArgumentException("s should not be empty or consists only of white-space characters.", nameof(s));
            if (radix < 2 || radix > 36) throw new ArgumentOutOfRangeException(nameof(radix), "radix should be in 2-36.");
            if (TryParseToInt32(s, radix, out var result)) return result;
            var message = $"{nameof(s)} is incorrect. It should be in base {radix} number format.";
            throw new FormatException(message, new ArgumentException(message, nameof(s)));
        }

        /// <summary>
        /// Parses a string to a number.
        /// </summary>
        /// <param name="s">The input string.</param>
        /// <param name="radix">The positional notation. Should be an integer in 2-36.</param>
        /// <exception cref="ArgumentNullException">s was null.</exception>
        /// <exception cref="ArgumentException">s was empty or consists only of white-space characters..</exception>
        /// <exception cref="ArgumentOutOfRangeException">radix was less than 2 or greater than 36.</exception>
        /// <exception cref="FormatException">s was in an incorrect format.</exception>
        /// <returns>A number parsed.</returns>
        public static long ParseToInt64(string s, int radix)
        {
            if (s == null) throw new ArgumentNullException(nameof(s), "s should not be null.");
            if (string.IsNullOrWhiteSpace(s)) throw new ArgumentException("s should not be empty or consists only of white-space characters.", nameof(s));
            if (radix < 2 || radix > 36) throw new ArgumentOutOfRangeException(nameof(radix), "radix should be in 2-36.");
            if (TryParseToInt64(s, radix, out var result)) return result;
            var message = $"{nameof(s)} is incorrect. It should be in base {radix} number format.";
            throw new FormatException(message, new ArgumentException(message, nameof(s)));
        }

        /// <summary>
        /// Tries to parse a string to a number.
        /// </summary>
        /// <param name="s">The input string.</param>
        /// <param name="radix">The positional notation. Should be an integer in 2-36.</param>
        /// <param name="result">The result.</param>
        /// <returns>true if parse succeeded; otherwise, false.</returns>
        public static bool TryParseToInt16(string s, int radix, out short result)
        {
            if (TryParseToInt32(s, radix, out var i) && i >= short.MinValue && i <= short.MaxValue)
            {
                result = (short)i;
                return true;
            }

            result = default;
            return false;
        }

        /// <summary>
        /// Tries to parse a string to a number.
        /// </summary>
        /// <param name="s">The input string.</param>
        /// <param name="radix">The positional notation. Should be an integer in 2-36.</param>
        /// <param name="result">The result.</param>
        /// <returns>true if parse succeeded; otherwise, false.</returns>
        public static bool TryParseToInt32(string s, int radix, out int result)
        {
            if (string.IsNullOrEmpty(s))
            {
                result = 0;
                return false;
            }

            s = s.Trim().ToLowerInvariant();
            if (radix == 10)
            {
                if (int.TryParse(s, out result)) return true;
                var i = TryParseNumericWord(s);
                if (i.HasValue)
                {
                    result = i.Value;
                    return true;
                }
            }

            if (radix < 2 || radix > 36 || string.IsNullOrEmpty(s))
            {
                result = default;
                return false;
            }

            var num = 0;
            var pos = 0;
            var neg = false;
            if (radix == 16)
            {
                if (s.StartsWith("0x-") || s.StartsWith("-0x") || s.StartsWith("&h-") || s.StartsWith("-&h"))
                {
                    pos += 3;
                    neg = true;
                }
                else if (s.StartsWith("0x") || s.StartsWith("&h"))
                {
                    pos += 2;
                }
                else if (s.StartsWith("x-") || s.StartsWith("-x"))
                {
                    pos += 2;
                    neg = true;
                }
                else if (s.StartsWith("x"))
                {
                    pos++;
                }
            }
            else if (s[0] == '-')
            {
                neg = true;
                pos++;
            }

            for (; pos < s.Length; pos++)
            {
                var c = s[pos];
                num *= radix;
                var i = num36.IndexOf(c);
                if (i < 0)
                {
                    if (c == ' ' || c == '_' || c == ',') continue;
                    if (c == '.' || c == '\t' || c == '\r' || c == '\n' || c == '\0') break;
                    result = default;
                    return false;
                }
                else if (i >= radix || num < 0)
                {
                    result = default;
                    return false;
                }

                num += i;
            }

            result = neg ? -num : num;
            return true;
        }

        /// <summary>
        /// Tries to parse a string to a number.
        /// </summary>
        /// <param name="s">The input string.</param>
        /// <param name="radix">The positional notation. Should be an integer in 2-36.</param>
        /// <param name="result">The result.</param>
        /// <returns>true if parse succeeded; otherwise, false.</returns>
        public static bool TryParseToInt64(string s, int radix, out long result)
        {
            if (string.IsNullOrEmpty(s))
            {
                result = 0;
                return false;
            }

            s = s.Trim().ToLowerInvariant();
            if (radix == 10)
            {
                if (long.TryParse(s, out result)) return true;
                var i = TryParseNumericWord64(s);
                if (i.HasValue)
                {
                    result = i.Value;
                    return true;
                }
            }

            if (radix < 2 || radix > 36 || string.IsNullOrEmpty(s))
            {
                result = default;
                return false;
            }

            var num = 0L;
            var pos = 0;
            var neg = false;
            if (radix == 16)
            {
                if (s.StartsWith("0x-") || s.StartsWith("-0x") || s.StartsWith("&h-") || s.StartsWith("-&h"))
                {
                    pos += 3;
                    neg = true;
                }
                else if (s.StartsWith("0x") || s.StartsWith("&h"))
                {
                    pos += 2;
                }
                else if (s.StartsWith("x-") || s.StartsWith("-x"))
                {
                    pos += 2;
                    neg = true;
                }
                else if (s.StartsWith("x"))
                {
                    pos++;
                }
            }
            else if (s[0] == '-')
            {
                neg = true;
                pos++;
            }

            for (; pos < s.Length; pos++)
            {
                var c = s[pos];
                num *= radix;
                var i = num36.IndexOf(c);
                if (i < 0)
                {
                    if (c == ' ' || c == '_' || c == ',') continue;
                    if (c == '.' || c == '\t' || c == '\r' || c == '\n' || c == '\0') break;
                    result = default;
                    return false;
                }
                else if (i >= radix || num < 0)
                {
                    result = default;
                    return false;
                }

                num += i;
            }

            result = neg ? -num : num;
            return true;
        }

        private static int? TryParseNumericWord(string word)
        {
            if (word.Length == 1) return TryParseToInt32(word[0]);
            var magnitude = word[word.Length - 1];
            if (magnitude == '整') word = word.Substring(0, word.Length - 1);
            var level = magnitude switch
            {
                'k' or 'K' or '千' or '仟' => 1000,
                '万' or '萬' or 'w' => 10000,
                'M' => 1_000_000,
                '亿' or '億' => 100_000_000,
                'G' or 'B' or '吉' => 1_000_000_000,
                '百' or '佰' => 100,
                '十' or '拾' => 10,
                _ => 1
            };
            if (level > 1)
            {
                var s = word.Substring(0, word.Length - 1);
                if (level == 10000 && s.Length > 0 && (s[s.Length - 1] == '百' || s[s.Length - 1] == '佰'))
                {
                    level = 1_000_000;
                    s = s.Substring(0, s.Length - 1);
                    if (s.Length < 1) return level;
                }

                if (int.TryParse(s, out var i))
                {
                    var j = i * level;
                    if (j % level > 0) return null;
                    return j;
                }
            }

            return TryParseNumericWordInternal(word);
        }

        /// <summary>
        /// Parses a character to an integer.
        /// </summary>
        /// <param name="c">A character</param>
        /// <returns>An integer parsed.</returns>
        private static int? TryParseToInt32(char c)
            => c switch
            {
                '0' => 0,
                '1' => 1,
                '2' => 2,
                '3' => 3,
                '4' => 4,
                '5' => 5,
                '6' => 6,
                '7' => 7,
                '8' => 8,
                '9' => 9,
                '零' or '〇' or '凌' or '空' or '无' or '無' or 'O' or '０' or 'ｏ' or 'ｏ' or '₀' or '⁰' or '○' or '蛋' or '圈' or '栋' or '영' or '공' => 0,
                '一' or '壹' or '①' or 'I' or 'i' or 'Ⅰ' or 'ⅰ' or '⒈' or '１' or '㈠' or '⑴' or '¹' or '₁' or 'a' or '单' or '幺' or '奇' or '独' or '甲' or '일' => 1,
                '二' or '贰' or '②' or 'Ⅱ' or 'ⅱ' or '⒉' or '２' or '㈡' or '⑵' or '²' or '₂' or 'に' or '两' or '兩' or '俩' or '倆' or '双' or '乙' or '이' or '둘' => 2,
                '三' or '叄' or '③' or 'Ⅲ' or 'ⅲ' or '⒊' or '３' or '㈢' or '⑶' or '³' or '₃' or '仨' or '丙' or '삼' or '셋' => 3,
                '四' or '肆' or '④' or 'Ⅳ' or 'ⅳ' or '⒋' or '４' or '㈣' or '⑷' or '⁴' or '₄' or '亖' or '丁' or '罒' or 'し' or '사' or '넷' => 4,
                '五' or '伍' or '⑤' or 'V' or 'v' or 'Ⅴ' or 'ⅴ' or '⒌' or '５' or '㈤' or '⑸' or '⁵' or '₅' or '戊' or 'ご' or '오' => 5,
                '六' or '陆' or '⑥' or 'Ⅵ' or 'ⅵ' or '⒍' or '６' or '㈥' or '⑹' or '⁶' or '₆' or '顺' or '己' or '육' => 6,
                '七' or '柒' or '⑦' or 'Ⅶ' or 'ⅶ' or '⒎' or '７' or '㈦' or '⑺' or '⁷' or '₇' or '拐' or '庚' or '칠' => 7,
                '八' or '捌' or '⑧' or 'Ⅷ' or 'ⅷ' or '⒏' or '８' or '㈧' or '⑻' or '⁸' or '₈' or '发' or '發' or '辛' or '팔' => 8,
                '九' or '玖' or '⑨' or 'Ⅸ' or 'ⅸ' or '⒐' or '９' or '㈨' or '⑼' or '⁹' or '₉' or '酒' or '壬' or '구' => 9,
                '十' or '拾' or '⑩' or 'Ⅹ' or 'X' or 'ⅹ' or '⒑' or '㈩' or '⑽' or '癸' or '십' or '열' => 10,
                'Ⅺ' or 'ⅺ' or '⒒' or '⑾' => 11,
                'Ⅻ' or 'ⅻ' or '⒓' or '⑿' => 12,
                '⒔' or '⒀' => 13,
                '⒕' or '⒁' => 14,
                '⒖' or '⒂' => 15,
                '⒗' or '⒃' => 16,
                '⒘' or '⒄' => 17,
                '⒙' or '⒅' => 18,
                '⒚' or '⒆' => 19,
                '廿' or '⒛' or '⒇' => 20,
                '卅' => 30,
                '卌' => 40,
                '圩' or 'ⅼ' or 'Ⅼ' => 50,
                '圆' => 60,
                '进' => 70,
                '枯' => 80,
                '枠' => 90,
                '百' or '佰' or 'Ⅽ' or 'ⅽ' => 100,
                '皕' => 200,
                'Ⅾ' or 'ⅾ' => 500,
                '千' or '仟' or 'K' or 'k' or 'Ⅿ' or 'ⅿ' => 1000,
                '万' or '萬' => 10000,
                '亿' or '億' => 100000000,
                'G' => 1000000000,
                _ => null
            };

        private static long? TryParseNumericWord64(string word)
        {
            if (word.Length == 1) return TryParseToInt32(word[0]);
            var magnitude = word[word.Length - 1];
            if (magnitude == '整') word = word.Substring(0, word.Length - 1);
            var level = magnitude switch
            {
                'k' or 'K' or '千' or '仟' => 1000L,
                '万' or '萬' or 'w' => 10000L,
                'M' => 1_000_000L,
                '亿' or '億' => 100_000_000L,
                'G' or 'B' or '吉' => 1_000_000_000L,
                'T' or '太' => 1_000_000_000_000L,
                'P' => 1_000_000_000_000_000L,
                '百' or '佰' => 100L,
                '十' or '拾' => 10L,
                _ => 1L
            };
            if (level > 1)
            {
                var s = word.Substring(0, word.Length - 1);
                if (level == 10_000L && s.Length > 0 && (s[s.Length - 1] == '百' || s[s.Length - 1] == '佰'))
                {
                    level = 1_000_000L;
                    s = s.Substring(0, s.Length - 1);
                    if (s.Length < 1) return level;
                }
                else if (level == 100_000_000L && s.Length > 0 && (s[s.Length - 1] == '十' || s[s.Length - 1] == '拾'))
                {
                    level = 1_000_000_000L;
                    s = s.Substring(0, s.Length - 1);
                    if (s.Length < 1) return level;
                }

                if (long.TryParse(s, out var i))
                {
                    var j = i * level;
                    if (j % level > 0) return null;
                    return j;
                }
            }

            return TryParseNumericWordInternal(word);
        }

        private static int? TryParseNumericWordInternal(string word)
            => word.ToLowerInvariant() switch
            {
                "zero" or "nought" or "nill" or "れい" or "zéro" or "nul" => 0,
                "one" or "first" or "いち" or "하나" or "un" or "une" or "unu" => 1,
                "two" or "second" or "ii" or "deux" or "du" => 2,
                "three" or "third" or "iii" or "さん" or "trois" or "tri" => 3,
                "four" or "fourth" or "forth" or "iv" or "quatre" or "kvar" => 4,
                "five" or "fifth" or "다섯" or "cinq" or "kvin" => 5,
                "six" or "sixth" or "vi" or "half dozen" or "半打" or "ろく" or "여섯" or "ses" => 6,
                "seven" or "seventh" or "vii" or "しち" or "일곱" or "sept" or "sep" => 7,
                "eight" or "eighth" or "viii" or "はち" or "여덟" or "huit" => 8,
                "nine" or "ninth" or "ix" or "きゅう" or "아홉" or "neuf" or "naŭ" => 9,
                "ten" or "tenth" or "一十" or "壹拾" or "一零" or "一〇" or "１０" or "じゅう" or "dix" or "dek" => 10,
                "eleven" or "eleventh" or "xi" or "一十一" or "十一" or "一一" or "１１" or "onze" => 11,
                "twelve" or "twelfth" or "xii" or "dozen" or "a dozen" or "一十二" or "十二" or "一二" or "一打" or "１２" or "douze" => 12,
                "thirteen" or "xiii" or "一十三" or "十三" or "一三" or "１３" or "treize" => 13,
                "fourteen" or "xiv" or "一十四" or "十四" or "一四" or "１４" or "quatorze" => 14,
                "fifteen" or "xv" or "一十五" or "十五" or "一五" or "１５" or "quinze" => 15,
                "sixteen" or "xvi" or "一十六" or "十六" or "一六" or "１６" or "seize" => 16,
                "seventeen" or "xvii" or "一十七" or "十七" or "一七" or "１７" or "dix-sept" => 17,
                "eighteen" or "xviii" or "一十八" or "十八" or "一八" or "１８" or "dix-huit" => 18,
                "ninteen" or "xix" or "一十九" or "十九" or "一九" or "１９" or "dix-neuf" => 19,
                "twenty" or "xx" or "二十" or "贰拾" or "二零" or "二〇" or "２０" or "vingt" => 20,
                "twenty-one" or "xxi" or "二十一" or "廿一" or "二一" or "２１" or "vingt et un" => 21,
                "twenty-two" or "xxii" or "二十二" or "廿二" or "二二" or "２２" or "vingt-deux" => 22,
                "twenty-three" or "xxiii" or "二十三" or "廿三" or "二三" or "２３" or "vingt-trois" => 23,
                "twenty-four" or "xxiv" or "二十四" or "廿四" or "二四" or "２４" or "vingt-quatre" => 24,
                "twenty-five" or "xxv" or "二十五" or "廿五" or "二五" or "２５" or "vingt-cinq" => 25,
                "twenty-six" or "xxvi" or "二十六" or "廿六" or "二六" or "２６" or "vingt-six" => 26,
                "twenty-seven" or "xxvii" or "二十七" or "廿七" or "二七" or "２７" or "vingt-sept" => 27,
                "twenty-eight" or "xxviii" or "二十八" or "廿八" or "二八" or "２８" or "vingt-huit" => 28,
                "twenty-nine" or "xxix" or "二十九" or "廿九" or "二九" or "２９" or "vingt-neuf" => 29,
                "thirty" or "xxx" or "三十" or "叄拾" or "三零" or "三〇" or "３０" or "trente" => 30,
                "forty" or "xl" or "四十" or "肆拾" or "四零" or "四〇" or "４０" or "quarante" => 40,
                "fifty" or "五十" or "伍拾" or "五零" or "五〇" or "半百" or "５０" or "cinquante" => 50,
                "sixty" or "六十" or "陆拾" or "六零" or "六〇" or "６０" or "soixante" => 60,
                "seventy" or "七十" or "柒拾" or "七零" or "七〇" or "７０" or "soixante-dix" => 70,
                "eighty" or "八十" or "捌拾" or "八零" or "八〇" or "８０" or "quatre-vingts" => 80,
                "ninty" or "九十" or "玖拾" or "九零" or "九〇" or "９０" or "quatre-vingt-dix" => 90,
                "一百" or "壹佰" or "一零零" or "一〇〇" or "１００" or "hundred" or "a hundred" or "one hundred" or "ひゃく" or "いちひゃく" or "cent" => 100,
                "两百" or "二百" or "贰佰" or "二零零" or "二〇〇" or "２００" or "two hundred" or "cc" or "deux cents" => 200,
                "三百" or "叄佰" or "三零零" or "三〇〇" or "３００" or "three hundred" or "ccc" or "trois cents" => 300,
                "四百" or "肆佰" or "四零零" or "四〇〇" or "４００" or "four hundred" or "cd" or "quatre cents" => 400,
                "五百" or "伍佰" or "五零零" or "五〇〇" or "５００" or "five hundred" or "cinq cents" => 500,
                "六百" or "陆佰" or "六零零" or "六〇〇" or "６００" or "six hundred" or "six cents" => 600,
                "七百" or "柒佰" or "七零零" or "七〇〇" or "７００" or "seven hundred" or "sept cents" => 700,
                "八百" or "捌佰" or "八零零" or "八〇〇" or "８００" or "eight hundred" or "huit cents" => 800,
                "九百" or "玖佰" or "九零零" or "九〇〇" or "９００" or "nine hundred" or "neuf cents" => 900,
                "kilo" or "a kilo" or "thousand" or "a thousand" or "one thousand" or "一千" or "壹仟" or "１０００" or "せん" or "いちせん" or "mille" or "millennium" or "1e3" => 1000,
                "一万" or "壹萬" or "壹万" or "１００００" or "ten thousand" or "ten kilo" or "まん" or "いちまん" or "1e4" => 10000,
                "a million" or "one million" or "million" or "mega" or "1 mega" or "一百万" or "壹佰萬" or "壹佰万" or "１００００００" or "1e6" => 1000000,
                _ => null
            };

    }
}
