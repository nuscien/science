﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trivial.Drawing
{
    /// <summary>
    /// Color calculator.
    /// </summary>
    public static partial class ColorCalculator
    {
        /// <summary>
        /// Calculates to get the alpha color.
        /// </summary>
        /// <param name="value">The source color value.</param>
        /// <param name="alpha">The alpha channel. Value is from 0 to 1.</param>
        /// <param name="resetOriginalAlpha">true if use new alpha channel directly instead base the current one; otherwise, false.</param>
        /// <returns></returns>
        public static Color Alpha(Color value, double alpha, bool resetOriginalAlpha = false)
            => Color.FromArgb(resetOriginalAlpha ? ToChannel(alpha * 255) : ToChannel(value.A * alpha), value.R, value.G, value.B);

        /// <summary>
        /// Calculates to get the alpha color.
        /// </summary>
        /// <param name="value">The source color value.</param>
        /// <param name="alpha">The alpha channel. Value is from 0 to 1.</param>
        /// <param name="resetOriginalAlpha">true if use new alpha channel directly instead base the current one; otherwise, false.</param>
        /// <returns></returns>
        public static Color Alpha(Color value, float alpha, bool resetOriginalAlpha = false)
            => Color.FromArgb(resetOriginalAlpha ? ToChannel(alpha * 255) : ToChannel(value.A * alpha), value.R, value.G, value.B);

        /// <summary>
        /// Calculates to get the alpha color.
        /// </summary>
        /// <param name="value">The source color value.</param>
        /// <param name="alpha">The alpha channel. Value is from 0 to 255.</param>
        /// <param name="resetOriginalAlpha">true if use new alpha channel directly instead base the current one; otherwise, false.</param>
        /// <returns></returns>
        public static Color Alpha(Color value, byte alpha, bool resetOriginalAlpha = false)
            => Color.FromArgb(resetOriginalAlpha ? ToChannel(alpha) : ToChannel(alpha / 255d * value.A), value.R, value.G, value.B);

        /// <summary>
        /// Increases brighness.
        /// </summary>
        /// <param name="value">The source color value.</param>
        /// <param name="amount">The degree of brightness to increase. Value is from -1 to 1.</param>
        /// <returns>The color after lighten.</returns>
        public static Color Lighten(Color value, double amount)
        {
            if (amount == 0) return value;
            if (amount > 1) return Color.FromArgb(value.A, 255, 255, 255);
            if (amount < -1) return Color.FromArgb(value.A, 0, 0, 0);
            var bg = amount > 0 ? 255 : 0;
            amount = Math.Abs(amount);
            return Color.FromArgb(
                value.A,
                ToChannel((bg - value.R) * amount + value.R),
                ToChannel((bg - value.G) * amount + value.G),
                ToChannel((bg - value.B) * amount) + value.B);
        }

        /// <summary>
        /// Increases brighness.
        /// </summary>
        /// <param name="value">The source color value.</param>
        /// <param name="amount">The degree of brightness to increase. Value is from -1 to 1.</param>
        /// <returns>The color after lighten.</returns>
        public static Color Lighten(Color value, float amount)
        {
            if (amount == 0) return value;
            if (amount > 1) return Color.FromArgb(value.A, 255, 255, 255);
            if (amount < -1) return Color.FromArgb(value.A, 0, 0, 0);
            var bg = amount > 0 ? 255 : 0;
            amount = Math.Abs(amount);
            return Color.FromArgb(
                value.A,
                ToChannel((bg - value.R) * amount + value.R),
                ToChannel((bg - value.G) * amount + value.G),
                ToChannel((bg - value.B) * amount) + value.B);
        }

        /// <summary>
        /// Increases brighness.
        /// </summary>
        /// <param name="value">The source color value collection.</param>
        /// <param name="amount">The degree of brightness to decrease. Value is from -1 to 1.</param>
        /// <returns>The color after lighten.</returns>
        public static IEnumerable<Color> Lighten(IEnumerable<Color> value, double amount)
            => value?.Select(ele => Lighten(ele, amount));

        /// <summary>
        /// Increases brighness.
        /// </summary>
        /// <param name="value">The source color value collection.</param>
        /// <param name="amount">The degree of brightness to decrease. Value is from -1 to 1.</param>
        /// <returns>The color after lighten.</returns>
        public static IEnumerable<Color> Lighten(IEnumerable<Color> value, float amount)
            => value?.Select(ele => Lighten(ele, amount));

        /// <summary>
        /// Decreases brighness.
        /// </summary>
        /// <param name="value">The source color value.</param>
        /// <param name="amount">The degree of brightness to decrease. Value is from -1 to 1.</param>
        /// <returns>The color after darken.</returns>
        public static Color Darken(Color value, double amount)
            => Lighten(value, -amount);

        /// <summary>
        /// Decreases brighness.
        /// </summary>
        /// <param name="value">The source color value.</param>
        /// <param name="amount">The degree of brightness to decrease. Value is from -1 to 1.</param>
        /// <returns>The color after darken.</returns>
        public static Color Darken(Color value, float amount)
            => Lighten(value, -amount);

        /// <summary>
        /// Decreases brighness.
        /// </summary>
        /// <param name="value">The source color value collection.</param>
        /// <param name="amount">The degree of brightness to decrease. Value is from -1 to 1.</param>
        /// <returns>The color after darken.</returns>
        public static IEnumerable<Color> Darken(IEnumerable<Color> value, double amount)
            => value?.Select(ele => Darken(ele, amount));

        /// <summary>
        /// Decreases brighness.
        /// </summary>
        /// <param name="value">The source color value collection.</param>
        /// <param name="amount">The degree of brightness to decrease. Value is from -1 to 1.</param>
        /// <returns>The color after darken.</returns>
        public static IEnumerable<Color> Darken(IEnumerable<Color> value, float amount)
            => value?.Select(ele => Darken(ele, amount));

        /// <summary>
        /// Toggles brightness between light mode and dark mode.
        /// </summary>
        /// <param name="value">The source color value.</param>
        /// <returns>The color toggled.</returns>
        public static Color ToggleBrightness(Color value)
        {
            var delta = 255
                - Maths.Arithmetic.Max(value.R, value.G, value.B)
                - Maths.Arithmetic.Min(value.R, value.G, value.B);
            return Color.FromArgb(value.A, value.R + delta, value.G + delta, value.B + delta);
        }

        /// <summary>
        /// Toggles brightness between light mode and dark mode.
        /// </summary>
        /// <param name="value">The source color value collection.</param>
        /// <returns>The color toggled.</returns>
        public static IEnumerable<Color> ToggleBrightness(IEnumerable<Color> value)
            => value?.Select(ele => ToggleBrightness(ele));

        /// <summary>
        /// Reverses RGB.
        /// </summary>
        /// <param name="value">The source color value.</param>
        /// <returns>The color to reverse.</returns>
        public static Color Reverse(Color value)
            => Color.FromArgb(value.A, 255 - value.R, 255 - value.G, 255 - value.B);

        /// <summary>
        /// Reverses RGB.
        /// </summary>
        /// <param name="value">The source color value collection.</param>
        /// <returns>The color to reverse.</returns>
        public static IEnumerable<Color> Reverse(IEnumerable<Color> value)
            => value?.Select(ele => Reverse(ele));

        /// <summary>
        /// Adds saturate filter.
        /// </summary>
        /// <param name="value">The source color value.</param>
        /// <param name="amount">The saturate to add filter. Value is from 0 to 1.</param>
        /// <returns>A new color with additional saturate.</returns>
        public static Color SaturateFilter(Color value, double amount)
        {
            if (double.IsNaN(amount)) return value;
            var hsl = ToHSL(value);
            var saturate = hsl.Item2 * amount;
            if (saturate < 0) saturate = 0;
            else if (saturate > 1) saturate = 1;
            return FromHSL(hsl.Item1, saturate, hsl.Item3);
        }

        /// <summary>
        /// Adds saturate filter.
        /// </summary>
        /// <param name="value">The source color value.</param>
        /// <param name="amount">The saturate to add filter. Value is from 0 to 1.</param>
        /// <returns>A new color with additional saturate.</returns>
        public static Color SaturateFilter(Color value, float amount)
        {
            if (float.IsNaN(amount)) return value;
            var hsl = ToSingleHSL(value);
            var saturate = hsl.Item2 * amount;
            if (saturate < 0) saturate = 0;
            else if (saturate > 1) saturate = 1;
            return FromHSL(hsl.Item1, saturate, hsl.Item3);
        }

        /// <summary>
        /// Adds saturate filter.
        /// </summary>
        /// <param name="value">The source color value collection.</param>
        /// <param name="amount">The hue to rotate. Value is from 0 to 360.</param>
        /// <returns>A new color with additional saturate.</returns>
        public static IEnumerable<Color> SaturateFilter(IEnumerable<Color> value, double amount)
            => value?.Select(ele => SaturateFilter(ele, amount));

        /// <summary>
        /// Adds saturate filter.
        /// </summary>
        /// <param name="value">The source color value collection.</param>
        /// <param name="amount">The hue to rotate. Value is from 0 to 360.</param>
        /// <returns>A new color with additional saturate.</returns>
        public static IEnumerable<Color> SaturateFilter(IEnumerable<Color> value, float amount)
            => value?.Select(ele => SaturateFilter(ele, amount));

        /// <summary>
        /// Rotates hue.
        /// </summary>
        /// <param name="value">The source color value.</param>
        /// <param name="amount">The hue to rotate. Value is from 0 to 360.</param>
        /// <returns>A new color with hue rotation.</returns>
        public static Color RotateHue(Color value, double amount)
        {
            if (double.IsNaN(amount)) return value;
            var hsl = ToHSL(value);
            if (amount > 360 || amount < 0) amount %= 360;
            var hue = hsl.Item1 + amount;
            if (hue < 0) hue += 360;
            else if (hue > 360) hue -= 360;
            return FromHSL(hue, hsl.Item2, hsl.Item3);
        }

        /// <summary>
        /// Rotates hue.
        /// </summary>
        /// <param name="value">The source color value.</param>
        /// <param name="amount">The hue to rotate. Value is from 0 to 360.</param>
        /// <returns>A new color with hue rotation.</returns>
        public static Color RotateHue(Color value, float amount)
        {
            if (float.IsNaN(amount)) return value;
            var hsl = ToSingleHSL(value);
            if (amount > 360 || amount < 0) amount %= 360;
            var hue = hsl.Item1 + amount;
            if (hue < 0) hue += 360;
            else if (hue > 360) hue -= 360;
            return FromHSL(hue, hsl.Item2, hsl.Item3);
        }

        /// <summary>
        /// Rotates hue.
        /// </summary>
        /// <param name="value">The source color value collection.</param>
        /// <param name="amount">The hue to rotate. Value is from 0 to 360.</param>
        /// <returns>A new color with hue rotation.</returns>
        public static IEnumerable<Color> RotateHue(IEnumerable<Color> value, double amount)
            => value?.Select(ele => RotateHue(ele, amount));

        /// <summary>
        /// Rotates hue.
        /// </summary>
        /// <param name="value">The source color value collection.</param>
        /// <param name="amount">The hue to rotate. Value is from 0 to 360.</param>
        /// <returns>A new color with hue rotation.</returns>
        public static IEnumerable<Color> RotateHue(IEnumerable<Color> value, float amount)
            => value?.Select(ele => RotateHue(ele, amount));

        /// <summary>
        /// Gets a collection.
        /// </summary>
        /// <param name="from">The color from.</param>
        /// <param name="to">The color to.</param>
        /// <param name="count">The count of color to return.</param>
        /// <returns>A collection of console text.</returns>
        public static IEnumerable<Color> LinearGradient(Color from, Color to, int count)
        {
            if (count < 1) yield break;
            if (count == 1)
            {
                yield return Color.FromArgb((from.A + to.A) / 2, (from.R + to.R) / 2, (from.G + to.G) / 2, (from.B + to.B) / 2);
                yield break;
            }

            yield return from;
            var steps = count - 1;
            var deltaA = (to.A - from.A) * 1f / steps;
            var deltaR = (to.R - from.R) * 1f / steps;
            var deltaG = (to.G - from.B) * 1f / steps;
            var deltaB = (to.B - from.B) * 1f / steps;
            float a = from.A;
            float r = from.R;
            float g = from.G;
            float b = from.B;
            for (var i = 1; i < steps; i++)
            {
                var c = Color.FromArgb(
                    PlusChannel(ref a, deltaA),
                    PlusChannel(ref r, deltaR),
                    PlusChannel(ref g, deltaG),
                    PlusChannel(ref b, deltaB));
                yield return c;
            }

            yield return to;
        }

        private static int PlusChannel(ref float c, float delta)
        {
#if NETOLDVER
            var r = (int)Math.Round(c + delta);
#else
            var r = (int)MathF.Round(c + delta);
#endif
            if (r < 0) return 0;
            else if (r > 255) return 255;
            return r;
        }

        private static int ToChannel(float c)
        {
#if NETOLDVER
            var r = (int)Math.Round(c);
#else
            var r = (int)MathF.Round(c);
#endif
            if (r < 0) return 0;
            else if (r > 255) return 255;
            return r;
        }

        private static int ToChannel(double c)
        {
            var r = (int)Math.Round(c);
            if (r < 0) return 0;
            else if (r > 255) return 255;
            return r;
        }
    }
}