﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Text;
using System.Web;

using Trivial.Text;
using Trivial.Web;

namespace Trivial.Collection
{
    /// <summary>
    /// The key value pairs with string key and string value.
    /// </summary>
    public class StringKeyValuePairs : List<KeyValuePair<string, string>>
    {
        /// <summary>
        /// Initializes a new instance of the StringKeyValuePairs class.
        /// </summary>
        public StringKeyValuePairs() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the StringKeyValuePairs class.
        /// </summary>
        /// <param name="collection">The collection whose elements are copied to the new list.</param>
        public StringKeyValuePairs(IEnumerable<KeyValuePair<string, string>> collection) : base(collection)
        {
        }

        /// <summary>
        /// Gets the equal sign for key and value.
        /// </summary>
        public virtual string EqualSign => "=";

        /// <summary>
        /// Gets the separator which is used between each key values.
        /// </summary>
        public virtual string Separator => "&";

        /// <summary>
        /// Gets the separator which is used between each values for a key if has more than one.
        /// </summary>
        public virtual string ValueSeparator => ",";

        /// <summary>
        /// Gets or sets the value of the specific key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>A value of the specific key.</returns>
        public string this[string key]
        {
            get
            {
                return GetValue(key);
            }

            set
            {
                ListExtensions.Set(this, key, value);
            }
        }

        /// <summary>
        /// Adds a key and the value to the end of the key value pairs.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="clearOthers">true if clear the others of the property before adding; otherwise, false.</param>
        public void Add(string key, string value, bool clearOthers = false)
        {
            if (clearOthers) ListExtensions.Remove(this, key);
            Add(new KeyValuePair<string, string>(key, value));
        }

        /// <summary>
        /// Adds a key and the value to the end of the key value pairs.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="clearOthers">true if clear the others of the property before adding; otherwise, false.</param>
        public void Add(string key, SecureString value, bool clearOthers = false)
        {
            Add(key, Security.SecureStringExtensions.ToUnsecureString(value), clearOthers);
        }

        /// <summary>
        /// Adds a key and the value to the end of the key value pairs.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="clearOthers">true if clear the others of the property before adding; otherwise, false.</param>
        public void Add(string key, StringBuilder value, bool clearOthers = false)
        {
            Add(key, value.ToString(), clearOthers);
        }

        /// <summary>
        /// Adds a key and the value to the end of the key value pairs.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="clearOthers">true if clear the others of the property before adding; otherwise, false.</param>
        public void Add(string key, int value, bool clearOthers = false)
        {
            Add(key, value.ToString("g", CultureInfo.InvariantCulture), clearOthers);
        }

        /// <summary>
        /// Adds a key and the value to the end of the key value pairs.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="clearOthers">true if clear the others of the property before adding; otherwise, false.</param>
        public void Add(string key, long value, bool clearOthers = false)
        {
            Add(key, value.ToString("g", CultureInfo.InvariantCulture), clearOthers);
        }

        /// <summary>
        /// Adds a key and the value to the end of the key value pairs.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="clearOthers">true if clear the others of the property before adding; otherwise, false.</param>
        public void Add(string key, double value, bool clearOthers = false)
        {
            Add(key, value.ToString("g", CultureInfo.InvariantCulture), clearOthers);
        }

        /// <summary>
        /// Adds a key and the value to the end of the key value pairs.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="clearOthers">true if clear the others of the property before adding; otherwise, false.</param>
        public void Add(string key, bool value, bool clearOthers = false)
        {
            Add(key, value ? JsonBoolean.TrueString : JsonBoolean.FalseString, clearOthers);
        }

        /// <summary>
        /// Adds a key and a set of value to the end of the key value pairs.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="clearOthers">true if clear the others of the property before adding; otherwise, false.</param>
        public void Add(string key, IEnumerable<string> value, bool clearOthers = false)
        {
            if (clearOthers) ListExtensions.Remove(this, key);
            if (value == null) return;
            foreach (var item in value)
            {
                Add(new KeyValuePair<string, string>(key, item));
            }
        }

        /// <summary>
        /// Gets the query value by a specific key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="separator">An optional string to use as a separator is included in the returned string only if it has more than one value.</param>
        /// <returns>The query value.</returns>
        public string GetValue(string key, string separator = null)
        {
            var arr = ListExtensions.GetValues(this, key).ToList();
            if (arr.Count == 0) return null;
            return string.Join(separator ?? ValueSeparator ?? string.Empty, arr);
        }

        /// <summary>
        /// Gets the query value by a specific key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="ignoreEmpty">true if ignore empty; otherwise, false.</param>
        /// <returns>The query value.</returns>
        public string GetFirstValue(string key, bool ignoreEmpty = false)
        {
            var col = ListExtensions.GetValues(this, key);
            return ignoreEmpty ? col.FirstOrDefault(item => !string.IsNullOrWhiteSpace(item)) : col.FirstOrDefault();
        }

        /// <summary>
        /// Gets the query value by a specific key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="ignoreEmpty">true if ignore empty; otherwise, false.</param>
        /// <returns>The query value. The last one for multiple values..</returns>
        public string GetLastValue(string key, bool ignoreEmpty = false)
        {
            var col = ListExtensions.GetValues(this, key);
            return ignoreEmpty ? col.LastOrDefault(item => !string.IsNullOrWhiteSpace(item)) : col.LastOrDefault();
        }

        /// <summary>
        /// Gets the query value as an interger by a specific key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The query value as Int32; or null, if the value type is incorrect, or the value is null.</returns>
        public int? TryGetInt32Value(string key)
        {
            var v = GetFirstValue(key, true);
            if (v != null && int.TryParse(v, out var result)) return result;
            return null;
        }

        /// <summary>
        /// Gets the query value as an interger by a specific key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The query value as Int64; or null, if the value type is incorrect, or the value is null.</returns>
        public long? TryGetInt64Value(string key)
        {
            var v = GetFirstValue(key, true);
            if (v != null && long.TryParse(v, out var result)) return result;
            return null;
        }

        /// <summary>
        /// Gets the query value as a single-precision floating-point number by a specific key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The query value as Single; or null, if the value type is incorrect, or the value is null.</returns>
        public float? TryGetSingleValue(string key)
        {
            var v = GetFirstValue(key, true);
            if (v != null && float.TryParse(v, out var result)) return result;
            return null;
        }

        /// <summary>
        /// Gets the query value as a double-precision floating-point number by a specific key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The query value as Double; or null, if the value type is incorrect, or the value is null.</returns>
        public double? TryGetDoubleValue(string key)
        {
            var v = GetFirstValue(key, true);
            if (v != null && double.TryParse(v, out var result)) return result;
            return null;
        }

        /// <summary>
        /// Gets the query value as a date and time by a specific key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="useUnixTimestamp">true if the value is unix timestamp; otherwise, false.</param>
        /// <returns>The query value as DateTime.</returns>
        public DateTime? TryGetDateTimeValue(string key, bool useUnixTimestamp = false)
        {
            var v = GetFirstValue(key, true);
            var l = TryGetInt64Value(key);
            return l.HasValue ? (useUnixTimestamp ? WebFormat.ParseUnixTimestamp(l.Value) : WebFormat.ParseDate(l.Value)) : WebFormat.ParseDate(v);
        }

        /// <summary>
        /// Gets the query value as an enumeration by a specific key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="ignoreCase">true to ignore case; false to regard case; null to use default settings.</param>
        /// <returns>The query value as Enum.</returns>
        /// <exception cref="ArgumentException">TEnum is not an enumeration type.</exception>
        public TEnum? TryGetEnumValue<TEnum>(string key, bool? ignoreCase = null) where TEnum : struct
        {
            var v = GetFirstValue(key, true);
            if (v == null) return null;
            if (ignoreCase.HasValue)
            {
                if (Enum.TryParse(v, ignoreCase.Value, out TEnum result)) return result;
            }
            else
            {
                if (Enum.TryParse(v, out TEnum result)) return result;
            }

            return null;
        }

        /// <summary>
        /// Encodes the key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The key encoded.</returns>
        protected virtual string EncodeKey(string key)
        {
            return WebFormat.UrlEncode(key);
        }

        /// <summary>
        /// Encodes the value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The value encoded.</returns>
        protected virtual string EncodeValue(string value)
        {
            return WebFormat.UrlEncode(value);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A query string.</returns>
        public override string ToString()
        {
            var arr = new List<string>();
            foreach (var item in this)
            {
                arr.Add(string.Format("{0}{2}{1}", EncodeKey(item.Key), EncodeValue(item.Value), EqualSign));
            }

            return string.Join(Separator, arr);
        }

        /// <summary>
        /// Gets the YAML format string of the value.
        /// </summary>
        /// <returns>A YAML format string.</returns>
        public virtual string ToYamlString()
        {
            var str = new StringBuilder();
            foreach (var prop in ListExtensions.ToGroups(this))
            {
                if (prop.Key == null) continue;
                str.Append(prop.Key.IndexOfAny(StringExtensions.YamlSpecialChars) >= 0
                    ? JsonString.ToJson(prop.Key)
                    : prop.Key);
                str.Append(": ");
                var values = prop.ToList();
                if (values == null || values.Count == 0)
                {
                    str.AppendLine("!!null null");
                    continue;
                }

                if (values.Count == 1)
                {
                    var item = values[0];
                    if (item == null)
                    {
                        str.AppendLine("!!null null");
                        continue;
                    }

                    str.AppendLine(item.Length == 0 || item.Length > 100 || item.IndexOfAny(StringExtensions.YamlSpecialChars) >= 0
                        ? JsonString.ToJson(item)
                        : item);
                    continue;
                }
                
                foreach (var item in values)
                {
                    str.AppendLine();
                    str.Append("- ");
                    if (item == null)
                    {
                        str.AppendLine("!!null null");
                        continue;
                    }

                    str.AppendLine(item.IndexOfAny(StringExtensions.YamlSpecialChars) >= 0
                        ? JsonString.ToJson(item)
                        : item);
                }
            }

            return str.ToString();
        }

        /// <summary>
        /// Adds 2 elements.
        /// </summary>
        /// <param name="a">Left element.</param>
        /// <param name="b">Right element.</param>
        /// <returns>The result.</returns>
        public static StringKeyValuePairs operator +(StringKeyValuePairs a, IEnumerable<KeyValuePair<string, string>> b)
        {
            if (a == null && b == null) return null;
            var col = new StringKeyValuePairs();
            if (a != null) col.AddRange(a);
            if (b != null) col.AddRange(b);
            return col;
        }

        /// <summary>
        /// Adds 2 elements.
        /// </summary>
        /// <param name="a">Left element.</param>
        /// <param name="b">Right element.</param>
        /// <returns>The result.</returns>
        public static StringKeyValuePairs operator +(StringKeyValuePairs a, KeyValuePair<string, string> b)
        {
            if (a == null) return null;
            var col = new StringKeyValuePairs();
            if (a != null) col.AddRange(a);
            col.Add(b);
            return col;
        }

        /// <summary>
        /// Deletes.
        /// </summary>
        /// <param name="a">Left element.</param>
        /// <param name="b">Right element.</param>
        /// <returns>The result.</returns>
        public static StringKeyValuePairs operator -(StringKeyValuePairs a, IEnumerable<KeyValuePair<string, string>> b)
        {
            if (a == null) return null;
            var col = new StringKeyValuePairs();
            col.AddRange(a);
            if (b == null) return col;
            foreach (var kvp in b)
            {
                a.Remove(kvp.Key, kvp.Value);
            }

            return col;
        }

        /// <summary>
        /// Deletes.
        /// </summary>
        /// <param name="a">Left element.</param>
        /// <param name="b">Right element.</param>
        /// <returns>The result.</returns>
        public static StringKeyValuePairs operator -(StringKeyValuePairs a, KeyValuePair<string, string> b)
        {
            if (a == null) return null;
            var col = new StringKeyValuePairs();
            col.AddRange(a);
            a.Remove(b.Key, b.Value);
            return col;
        }

        /// <summary>
        /// Converts to name value collection.
        /// </summary>
        /// <param name="value">The instance.</param>
        /// <returns>The name value collection.</returns>
        public static explicit operator NameValueCollection (StringKeyValuePairs value)
        {
            if (value == null) return null;
            var obj = new NameValueCollection();
            foreach (var prop in value)
            {
                obj.Add(prop.Key, prop.Value);
            }

            return obj;
        }

        /// <summary>
        /// Converts from name value collection.
        /// </summary>
        /// <param name="value">The name value collection.</param>
        /// <returns>The instance.</returns>
        public static implicit operator StringKeyValuePairs(NameValueCollection value)
        {
            if (value == null) return null;
            var obj = new StringKeyValuePairs();
            foreach (var prop in value.AllKeys)
            {
                foreach (var v in value.GetValues(prop))
                {
                    obj.Add(prop, v);
                }
            }

            return obj;
        }
    }
}
