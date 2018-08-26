﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Trivial.Console
{
    /// <summary>
    /// The input types for selection result.
    /// </summary>
    public enum SelectionResultTypes
    {
        /// <summary>
        /// The operation has been cancelled.
        /// </summary>
        Canceled = 0,

        /// <summary>
        /// The result is by selecting.
        /// </summary>
        Selected = 1,

        /// <summary>
        /// The result is by manual input.
        /// </summary>
        Typed = 2
    }

    /// <summary>
    /// The collection selection input information.
    /// </summary>
    public class Selection : Selection<object>
    {
        /// <summary>
        /// Enumartes for each item in the list.
        /// </summary>
        /// <typeparam name="T">The type of list item.</typeparam>
        /// <param name="col">The collection.</param>
        /// <param name="callback">The for each callback.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The count.</param>
        /// <returns>The index.</returns>
        internal static int Some<T>(IList<T> col, Func<T, int, int, bool> callback, int offset = 0, int? count = null)
        {
            if (callback == null) return -1;
            var len = col.Count;
            if (count.HasValue) len = Math.Min(count.Value + offset, len);
            var j = 0;
            for (var i = Math.Max(offset, 0); i < len; i++)
            {
                var item = col[i];
                if (callback(item, i, j)) return i;
                j++;
            }

            return -1;
        }
    }

    /// <summary>
    /// The collection selection input information.
    /// </summary>
    /// <typeparam name="T">The type of data.</typeparam>
    public class Selection<T>
    {
        /// <summary>
        /// The list.
        /// </summary>
        private List<Tuple<string, T, string, char?>> list = new List<Tuple<string, T, string, char?>>();

        /// <summary>
        /// Gets or sets the minimum length for each item.
        /// </summary>
        public int? MinLength { get; set; }

        /// <summary>
        /// Gets or sets the maximum length for each item.
        /// </summary>
        public int? MaxLength { get; set; }

        /// <summary>
        /// Gets or sets the maximum column count to display.
        /// </summary>
        public int? Column { get; set; }

        /// <summary>
        /// Gets or sets maximum row count per page.
        /// null for disable paging.
        /// </summary>
        public int? MaxRow { get; set; }

        /// <summary>
        /// Gets or sets the tips.
        /// null for disable tips.
        /// </summary>
        public string Tips { get; set; } = "Tips: [↑][↓][←][→] Select; [ENTER] OK.";

        /// <summary>
        /// Gets or sets the paging tips.
        /// null for disable tips.
        /// </summary>
        public string PagingTips { get; set; } = "← [PgUp] | {from} - {end} / {total} | [PgDn] →";

        /// <summary>
        /// Gets or sets the question message before selection.
        /// null for disable additional question line.
        /// </summary>
        public string Question { get; set; } = "Select: ";

        /// <summary>
        /// Gets or sets the question message for manual typing.
        /// null for disable manual mode.
        /// </summary>
        public string ManualQuestion { get; set; }

        /// <summary>
        /// Gets or sets the foreground color for item.
        /// </summary>
        public ConsoleColor? ForegroundColor { get; set; }

        /// <summary>
        /// Gets or sets the background color for item.
        /// </summary>
        public ConsoleColor? BackgroundColor { get; set; }

        /// <summary>
        /// Gets or sets the foreground color for item selected.
        /// </summary>
        public ConsoleColor? SelectedForegroundColor { get; set; }

        /// <summary>
        /// Gets or sets the foreground color for item selected.
        /// </summary>
        public ConsoleColor? SelectedBackgroundColor { get; set; }

        /// <summary>
        /// Gets or sets the foreground color for question.
        /// </summary>
        public ConsoleColor? QuestionForegroundColor { get; set; }

        /// <summary>
        /// Gets or sets the background color for question.
        /// </summary>
        public ConsoleColor? QuestionBackgroundColor { get; set; }

        /// <summary>
        /// Gets or sets the foreground color for tips.
        /// </summary>
        public ConsoleColor? TipsForegroundColor { get; set; }

        /// <summary>
        /// Gets or sets the background color for tips.
        /// </summary>
        public ConsoleColor? TipsBackgroundColor { get; set; }

        /// <summary>
        /// Gets or sets the foreground color for paing tips.
        /// </summary>
        public ConsoleColor? PagingForegroundColor { get; set; }

        /// <summary>
        /// Gets or sets the background color for paging tips.
        /// </summary>
        public ConsoleColor? PagingBackgroundColor { get; set; }

        /// <summary>
        /// Gets or sets the foreground color for default value.
        /// </summary>
        public ConsoleColor? DefaultValueForegroundColor { get; set; }

        /// <summary>
        /// Gets or sets the background color for default value.
        /// </summary>
        public ConsoleColor? DefaultValueBackgroundColor { get; set; }

        /// <summary>
        /// Gets or sets the prefix for the item.
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        /// Gets or sets the prefix for the item selected.
        /// </summary>
        public string SelectedPrefix { get; set; }

        /// <summary>
        /// Adds an item.
        /// </summary>
        /// <param name="value">The value to output.</param>
        /// <param name="data">The optional data.</param>
        /// <param name="title">The description displayed in item.</param>
        /// <param name="key">The hot key mapped.</param>
        public void Add(string value, T data = default(T), string title = null, char? key = null)
        {
            list.Add(new Tuple<string, T, string, char?>(value, data, title, key));
        }

        /// <summary>
        /// Copies a list.
        /// </summary>
        /// <returns>A list copied.</returns>
        internal List<Tuple<string, T, string, char?>> CopyList()
        {
            return new List<Tuple<string, T, string, char?>>(list);
        }
    }

    /// <summary>
    /// The result of the collection selection.
    /// </summary>
    /// <typeparam name="T">The type of data.</typeparam>
    public class SelectionResult<T>
    {
        /// <summary>
        /// Initializes a new instance of the SelectionResult class.
        /// </summary>
        /// <param name="value">The value input.</param>
        /// <param name="type">The input type.</param>
        public SelectionResult(string value, SelectionResultTypes type)
        {
            Value = value;
            Index = -1;
            Data = default(T);
            InputType = type;
        }

        /// <summary>
        /// Initializes a new instance of the SelectionResult class.
        /// </summary>
        /// <param name="value">The value input.</param>
        /// <param name="index">The index of item displayed.</param>
        /// <param name="data">The item data.</param>
        /// <param name="title">The title displayed in item.</param>
        public SelectionResult(string value, int index, T data, string title)
        {
            Value = value;
            Index = index;
            Data = data;
            Title = title;
            InputType = SelectionResultTypes.Selected;
        }

        /// <summary>
        /// Gets a value indicating whether it is cancelled.
        /// </summary>
        public bool IsCanceled => InputType == SelectionResultTypes.Canceled;

        /// <summary>
        /// Gets the index of item displayed.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Gets the value input.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Gets the item data.
        /// </summary>
        public T Data { get; }

        /// <summary>
        /// Gets the title displayed in item.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Gets input type.
        /// </summary>
        public SelectionResultTypes InputType { get; }
    }
}
