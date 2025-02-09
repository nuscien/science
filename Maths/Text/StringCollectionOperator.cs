using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trivial.Text;

/// <summary>
/// The operators for boolean collection.
/// </summary>
public enum StringCollectionOperators : byte
{
    /// <summary>
    /// Returns an empty string.
    /// </summary>
    Empty = 0,

    /// <summary>
    /// Join each strings.
    /// </summary>
    Join = 1,

    /// <summary>
    /// Join strings in each line (in envinronment).
    /// </summary>
    Lines = 2,

    /// <summary>
    /// Join strings separated by tab.
    /// </summary>
    Tabs = 3,

    /// <summary>
    /// Join strings separated by semicolon.
    /// </summary>
    Tags = 4,

    /// <summary>
    /// Join strings separated by comma.
    /// </summary>
    Commas = 5,

    /// <summary>
    /// Join strings separated by dot.
    /// </summary>
    Dots = 6,

    /// <summary>
    /// Join strings separated by slash.
    /// </summary>
    Slashes = 7,

    /// <summary>
    /// Join strings separated by vertical line.
    /// </summary>
    VerticalLines = 8,

    /// <summary>
    /// Join strings separated by vertical line.
    /// </summary>
    VerticalLineSeparators = 9,

    /// <summary>
    /// Join strings separated by white space.
    /// </summary>
    WhiteSpaces = 10,

    /// <summary>
    /// Join strings separated by 2 white spaces.
    /// </summary>
    DoubleWhiteSpaces = 11,

    /// <summary>
    /// Join strings separated by 3 white spaces.
    /// </summary>
    TripleWhiteSpaces = 12,

    /// <summary>
    /// Join string separated by 4 white spaces.
    /// </summary>
    QuadrupleWhiteSpaces = 14,

    /// <summary>
    /// Join strings separated by ampersand with white space around.
    /// </summary>
    And = 15,

    /// <summary>
    /// Join strings separated by spit poin with white space around.
    /// </summary>
    SplitPoints = 16,

    /// <summary>
    /// JSON array string format.
    /// </summary>
    JsonArray = 17,

    /// <summary>
    /// In bullet list. Each item is in a line with prefix of a split dot and a white space.
    /// </summary>
    Bullet = 18,

    /// <summary>
    /// In numbering list. Each item is in a line with prefix of one-based index and a tab.
    /// </summary>
    Numbering = 19,

    /// <summary>
    /// Returns the first one.
    /// </summary>
    First = 23,

    /// <summary>
    /// Returns the last one.
    /// </summary>
    Last = 24,

    /// <summary>
    /// Returns the first longest one.
    /// </summary>
    Longest = 25,

    /// <summary>
    /// Returns the last longest one.
    /// </summary>
    LastLongest = 26,

    /// <summary>
    /// Returns the first shortest one.
    /// </summary>
    Shortest = 27,

    /// <summary>
    /// Returns the last shortest one.
    /// </summary>
    LastShortest = 28,

    /// <summary>
    /// Unicode order (Fn #% 0-9 ?@ A-Z ^_ a-z |~ 汉).
    /// </summary>
    AscBinaryEncode = 29,

    /// <summary>
    /// Unicode desc order (汉 ~| z-a -^ Z-A @? 9-0 %# Fn).
    /// </summary>
    DescBinaryEncode = 30,
}

/// <summary>
/// The options used for string merging.
/// </summary>
public sealed class StringCollectionMergeOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether use \n instead of new line in current environment.
    /// </summary>
    public bool NewLineUseN { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether append an additional white space after comma or semi-comma.
    /// </summary>
    public bool AppendWhiteSpaceAfterComma { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether need skip null or empty.
    /// </summary>
    public bool SkipNullOrEmpty { get; set; }
}
