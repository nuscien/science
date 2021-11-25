﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trivial.Drawing
{
    /// <summary>
    /// The types to mix colors.
    /// </summary>
    public enum ColorMixTypes
    {
        /// <summary>
        /// Average (mean).
        /// Like 2 pigments mix together.
        /// </summary>
        Normal = 0,

        /// <summary>
        /// Merge each channel by the maximum value.
        /// Like 2 lights shine the same place.
        /// </summary>
        Lighten = 1,

        /// <summary>
        /// Merge each channel by the minimum value.
        /// Like 2 optical filters overlap.
        /// </summary>
        Darken = 2,

        /// <summary>
        /// Merge each channel by the minimum value or maximum value.
        /// So the saturation value of the new color will be as higher as the one merged.
        /// </summary>
        Wetness = 3,

        /// <summary>
        /// Merge each channel by the middle value.
        /// So the saturation value of the new color will be as lower as the one merged.
        /// </summary>
        Dryness = 4,

        /// <summary>
        /// Color linear dodge.
        /// Like 2 lights increase each other.
        /// </summary>
        Weaken = 5,

        /// <summary>
        /// Color linear burn.
        /// Like 2 optical filters overlap with additional loss.
        /// </summary>
        Deepen = 6,

        /// <summary>
        /// Emphasize each channel.
        /// Color dodge if the channel in base color is greater than gray; otherwise, color burn.
        /// </summary>
        Emphasis = 7,

        /// <summary>
        /// Add each channel of the blend color and the base color.
        /// </summary>
        Accent = 8,

        /// <summary>
        /// Diff each channel of the blend color by the base color.
        /// </summary>
        Diff = 9,

        /// <summary>
        /// Remove each channel value of the blend color by the base color.
        /// </summary>
        Remove = 10,

        /// <summary>
        /// Symmetry each channel of the blend color by the base color.
        /// </summary>
        Symmetry = 11,
    }

    /// <summary>
    /// Color channels.
    /// </summary>
    [Flags]
    public enum ColorChannels
    {
        /// <summary>
        /// Red.
        /// </summary>
        Red = 1,

        /// <summary>
        /// Green.
        /// </summary>
        Green = 2,

        /// <summary>
        /// Blue.
        /// </summary>
        Blue = 4
    }

    /// <summary>
    /// Color calculator.
    /// </summary>
    public static partial class ColorCalculator
    {
        /// <summary>
        /// Adds a color on another one.
        /// </summary>
        /// <param name="top">The color on top.</param>
        /// <param name="bottom">The color on bottom.</param>
        /// <returns>A new color.</returns>
        public static Color Overlay(Color top, Color bottom)
        {
            if (top.A == 0) return bottom;
            if (bottom.A == 0 || top.A == 255) return top;
            var ratio = top.A * 1f / 255;
            if (bottom.A < 255) ratio += (255 - bottom.A) / 255f * (1 - ratio);
            if (ratio >= 1) return top;
            var negRatio = 1 - ratio;
#if NETOLDVER
            var a = 255 - (int)Math.Round((255d - top.A) * (255d - bottom.A) * 255);
#else
            var a = 255 - (int)MathF.Round((255f - top.A) * (255f - bottom.A) * 255);
#endif
            return Color.FromArgb(
                a,
                ToChannel(top.R * ratio + bottom.R * negRatio),
                ToChannel(top.G * ratio + bottom.G * negRatio),
                ToChannel(top.B * ratio + bottom.B * negRatio));
        }

        /// <summary>
        /// Adds a color on another one.
        /// </summary>
        /// <param name="top">The color on top.</param>
        /// <param name="alpha">Alpha for top color. Value is from 0 to 1.</param>
        /// <param name="bottom">The color on bottom.</param>
        /// <returns>A new color.</returns>
        public static Color Overlay(Color top, double alpha, Color bottom)
            => Overlay(Opacity(top, alpha), bottom);

        /// <summary>
        /// Adds a color on another one.
        /// </summary>
        /// <param name="top">The color on top.</param>
        /// <param name="alpha">Alpha for top color. Value is from 0 to 1.</param>
        /// <param name="bottom">The color on bottom.</param>
        /// <returns>A new color.</returns>
        public static Color Overlay(Color top, float alpha, Color bottom)
            => Overlay(Opacity(top, alpha), bottom);

        /// <summary>
        /// Adds a color on another one.
        /// </summary>
        /// <param name="top">The color on top.</param>
        /// <param name="alpha">Alpha for top color. Value is from 0 to 1.</param>
        /// <param name="bottom">The color on bottom.</param>
        /// <returns>A new color.</returns>
        public static Color Overlay(Color top, byte alpha, Color bottom)
            => Overlay(Opacity(top, alpha), bottom);

        /// <summary>
        /// Adds a color on another one.
        /// </summary>
        /// <param name="top">The color on top.</param>
        /// <param name="middle">The color in middle.</param>
        /// <param name="bottom">The color on bottom.</param>
        /// <returns>A new color.</returns>
        public static Color Overlay(Color top, Color middle, Color bottom)
            => Overlay(top, Overlay(middle, bottom));

        /// <summary>
        /// Mixes colors.
        /// </summary>
        /// <param name="type">The type to mix colors.</param>
        /// <param name="a">The blend color.</param>
        /// <param name="b">The base color.</param>
        /// <returns>A new color mixed.</returns>
        public static Color Mix(ColorMixTypes type, Color a, Color b)
            => type switch
            {
                ColorMixTypes.Lighten => MixByLighten(a, b),
                ColorMixTypes.Darken => MixByDarken(a, b),
                ColorMixTypes.Weaken => MixByWeaken(a, b),
                ColorMixTypes.Wetness => MixByWetness(a, b),
                ColorMixTypes.Dryness => MixByDryness(a, b),
                ColorMixTypes.Deepen => MixByDeepen(a, b),
                ColorMixTypes.Emphasis => MixByEmphasis(a, b),
                ColorMixTypes.Accent => MixByAccent(a, b),
                ColorMixTypes.Diff => MixByDiff(a, b),
                ColorMixTypes.Remove => MixByRemove(a, b),
                ColorMixTypes.Symmetry => MixBySymmetry(a, b),
                _ => MixByMean(a, b)
            };

        /// <summary>
        /// Mixes colors.
        /// </summary>
        /// <param name="merge">The handler to merge each channel.</param>
        /// <param name="a">The blend color.</param>
        /// <param name="b">The base color.</param>
        /// <returns>A new color mixed.</returns>
        public static Color Mix(Func<byte, byte, ColorChannels, byte> merge, Color a, Color b)
        {
            if (merge == null) return MixByMean(a, b);
            var red = merge(a.R, b.R, ColorChannels.Red);
            var green = merge(a.G, b.G, ColorChannels.Green);
            var blue = merge(a.B, b.B, ColorChannels.Blue);
            return MixWithAlpha(red, green, blue, a, b);
        }

        /// <summary>
        /// Mixes colors.
        /// </summary>
        /// <param name="type">The type to mix colors.</param>
        /// <param name="a">The collection of blend color.</param>
        /// <param name="b">The collection of base color.</param>
        /// <returns>A new color collection mixed.</returns>
        public static IEnumerable<Color> Mix(ColorMixTypes type, IEnumerable<Color> a, IEnumerable<Color> b)
        {
            var arr = b.ToList();
            var i = -1;
            foreach (var item in a)
            {
                i++;
                if (arr.Count < i)
                    yield return Mix(type, item, arr[i]);
                else
                    yield return item;
            }

            i++;
            for (; i < arr.Count; i++)
            {
                yield return arr[i];
            }
        }

        /// <summary>
        /// Mixes colors.
        /// </summary>
        /// <param name="merge">The handler to merge each channel.</param>
        /// <param name="a">The blend color.</param>
        /// <param name="b">The base color.</param>
        /// <returns>A new color mixed.</returns>
        public static IEnumerable<Color> Mix(Func<byte, byte, ColorChannels, byte> merge, IEnumerable<Color> a, IEnumerable<Color> b)
        {
            var arr = b.ToList();
            var i = -1;
            foreach (var item in a)
            {
                i++;
                if (arr.Count < i)
                    yield return Mix(merge, item, arr[i]);
                else
                    yield return item;
            }

            i++;
            for (; i < arr.Count; i++)
            {
                yield return arr[i];
            }
        }

#if NETFRAMEWORK
        /// <summary>
        /// Mixes bitmaps.
        /// </summary>
        /// <param name="type">The type to mix bitmaps.</param>
        /// <param name="a">The blend bitmap.</param>
        /// <param name="b">The base bitmap.</param>
        /// <returns>A new bitmap mixed.</returns>
        public static Bitmap Mix(ColorMixTypes type, Bitmap a, Bitmap b)
        {
            int x, y;
            if (a == null || b == null) return null;
            var w = Math.Max(a.Width, b.Width);
            var h = Math.Max(a.Height, b.Height);
            var n = new Bitmap(w, h);
            for (x = 0; x < w; x++)
            {
                if (x < a.Width && x < b.Width)
                {
                    for (y = 0; y < h; y++)
                    {
                        if (y < a.Height && x < b.Height)
                        {
                            var c = a.GetPixel(x, y);
                            var d = b.GetPixel(x, y);
                            n.SetPixel(x, y, Mix(type, c, d));
                        }
                        else if (y < a.Height)
                        {
                            for (y = 0; y < h; y++)
                            {
                                n.SetPixel(x, y, a.GetPixel(x, y));
                            }
                        }
                        else if (y < b.Height)
                        {
                            for (y = 0; y < h; y++)
                            {
                                n.SetPixel(x, y, b.GetPixel(x, y));
                            }
                        }
                    }
                }
                else if (x < a.Width)
                {
                    for (y = 0; y < h; y++)
                    {
                        if (y < a.Height)
                            n.SetPixel(x, y, a.GetPixel(x, y));
                        else
                            n.SetPixel(x, y, Color.Transparent);
                    }
                }
                else if (x < b.Width)
                {
                    for (y = 0; y < h; y++)
                    {
                        if (y < b.Height)
                            n.SetPixel(x, y, b.GetPixel(x, y));
                        else
                            n.SetPixel(x, y, Color.Transparent);
                    }
                }
            }

            return n;
        }
#endif

        private static Color MixByMean(Color a, Color b)
        {
            if (a.A == b.A)
                return Color.FromArgb(
                    Math.Max(a.A, b.A),
                    ToChannel((a.R + b.R) / 2),
                    ToChannel((a.G + b.G) / 2),
                    ToChannel((a.B + b.B) / 2));
            var topAlpha = a.A / 255f;
            var bottomAlpha = b.A / 255f;
            var total = topAlpha + bottomAlpha;
            return Color.FromArgb(
                Math.Max(a.A, b.A),
                ToChannel((a.R * topAlpha + b.R * bottomAlpha) / total),
                ToChannel((a.G * topAlpha + b.G * bottomAlpha) / total),
                ToChannel((a.B * topAlpha + b.B * bottomAlpha) / total));
        }

        private static Color MixByLighten(Color a, Color b)
        {
            var red = Math.Max(a.R, b.R);
            var green = Math.Max(a.G, b.G);
            var blue = Math.Max(a.B, b.B);
            return MixWithAlpha(red, green, blue, a, b);
        }

        private static Color MixByDarken(Color a, Color b)
        {
            var red = Math.Min(a.R, b.R);
            var green = Math.Min(a.G, b.G);
            var blue = Math.Min(a.B, b.B);
            return MixWithAlpha(red, green, blue, a, b);
        }

        private static Color MixByWetness(Color a, Color b)
        {
            var red = Math.Abs(128 - a.R) >= Math.Abs(128 - b.R) ? a.R : b.R;
            var green = Math.Abs(128 - a.G) >= Math.Abs(128 - b.G) ? a.G : b.G;
            var blue = Math.Abs(128 - a.B) >= Math.Abs(128 - b.B) ? a.B : b.B;
            return MixWithAlpha(red, green, blue, a, b);
        }

        private static Color MixByDryness(Color a, Color b)
        {
            var red = Math.Abs(128 - a.R) <= Math.Abs(128 - b.R) ? a.R : b.R;
            var green = Math.Abs(128 - a.G) <= Math.Abs(128 - b.G) ? a.G : b.G;
            var blue = Math.Abs(128 - a.B) <= Math.Abs(128 - b.B) ? a.B : b.B;
            return MixWithAlpha(red, green, blue, a, b);
        }

        private static Color MixByWeaken(Color a, Color b)
        {
            var red = 255 - (255 - a.R) * (255 - b.R) / 255f;
            var green = 255 - (255 - a.G) * (255 - b.G) / 255f;
            var blue = 255 - (255 - a.B) * (255 - b.B) / 255f;
            return MixWithAlpha(red, green, blue, a, b);
        }

        private static Color MixByDeepen(Color a, Color b)
        {
            var red = a.R * b.R / 255f;
            var green = a.G * b.G / 255f;
            var blue = a.B * b.B / 255f;
            return MixWithAlpha(red, green, blue, a, b);
        }

        private static Color MixByEmphasis(Color a, Color b)
        {
            var red = (a.R > 127 ? 255 - a.R : a.R) * (b.R > 127 ? 255 - b.R : b.R) / 255f;
            var green = (a.G > 127 ? 255 - a.G : a.G) * (b.G > 127 ? 255 - b.G : b.G) / 255f;
            var blue = (a.B > 127 ? 255 - a.B : a.B) * (b.B > 127 ? 255 - b.B : b.B) / 255f;
            if (a.R > 127) red = (b.R > 127 ? 255 : a.R) - red;
            else red = b.R > 127 ? a.R + red : red;
            if (a.G > 127) green = (b.G > 127 ? 255 : a.G) - green;
            else green = b.G > 127 ? a.G + red : green;
            if (a.B > 127) blue = (b.B > 127 ? 255 : a.B) - blue;
            else blue = b.B > 127 ? a.B + blue : blue;
            return MixWithAlpha(red, green, blue, a, b);
        }

        private static Color MixByAccent(Color a, Color b)
        {
            if (a.A == b.A)
                return Color.FromArgb(
                    Math.Max(a.A, b.A),
                    ToChannel(a.R + b.R),
                    ToChannel(a.G + b.G),
                    ToChannel(a.B + b.B));
            float alpha = a.A + b.A;
            var topAlpha = a.A / alpha;
            var bottomAlpha = b.A / alpha;
            return Color.FromArgb(
                Math.Max(a.A, b.A),
                    ToChannel(a.R * topAlpha + b.R * bottomAlpha),
                    ToChannel(a.G * topAlpha + b.G * bottomAlpha),
                    ToChannel(a.B * topAlpha + b.B * bottomAlpha));
        }

        private static Color MixByDiff(Color a, Color b)
        {
            var red = a.R - b.R;
            if (red < 0) red += 256;
            var green = a.G - b.G;
            if (green < 0) green += 256;
            var blue = a.B - b.B;
            if (blue < 0) blue += 256;
            return MixWithAlpha(red, green, blue, a, b);
        }

        private static Color MixByRemove(Color a, Color b)
        {
            var red = a.R - b.R;
            if (red < 0) red = 0;
            var green = a.G - b.G;
            if (green < 0) green = 0;
            var blue = a.B - b.B;
            if (blue < 0) blue = 0;
            return MixWithAlpha(red, green, blue, a, b);
        }

        private static Color MixBySymmetry(Color a, Color b)
        {
            var red = 2 * b.R - a.R;
            if (red < 0) red = 0;
            else if (red > 255) red = 255;
            var green = 2 * b.G - a.G;
            if (green < 0) green = 0;
            else if (green > 255) green = 255;
            var blue = 2 * b.B - a.B;
            if (blue < 0) blue = 0;
            else if (blue > 255) blue = 255;
            return MixWithAlpha(red, green, blue, a, b);
        }

        private static Color MixWithAlpha(int red, int green, int blue, Color a, Color b)
        {
            if (a.A == b.A)
                return Color.FromArgb(Math.Max(a.A, b.A), red, green, blue);
            var ratio = Math.Abs(a.A - b.A) * 1f / Math.Max(a.A, b.A);
            var negRation = 1 - ratio;
            var c = a.A > b.A ? a : b;
            return Color.FromArgb(
                Math.Max(a.A, b.A),
                ToChannel(red * negRation + c.R * ratio),
                ToChannel(green * negRation + c.G * ratio),
                ToChannel(blue * negRation + c.B * ratio));
        }

        private static Color MixWithAlpha(float red, float green, float blue, Color a, Color b)
        {
            if (a.A == b.A)
                return Color.FromArgb(Math.Max(a.A, b.A), ToChannel(red), ToChannel(green), ToChannel(blue));
            var ratio = Math.Abs(a.A - b.A) * 1f / Math.Max(a.A, b.A);
            var negRation = 1 - ratio;
            var c = a.A > b.A ? a : b;
            return Color.FromArgb(
                Math.Max(a.A, b.A),
                ToChannel(red * negRation + c.R * ratio),
                ToChannel(green * negRation + c.G * ratio),
                ToChannel(blue * negRation + c.B * ratio));
        }
    }
}
