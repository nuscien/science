using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Trivial.Data;
using Trivial.Net;
using Trivial.Reflection;
using Trivial.Text;

namespace Trivial.Tasks;

/// <summary>
/// The streaming response writer of the turn-based chat message.
/// </summary>
public class RoundChatMessageWriter
{
    /// <summary>
    /// The store of the current text value.
    /// </summary>
    private StringBuilder current;

    /// <summary>
    /// Adds or removes the handler occurred on the text is changed.
    /// </summary>
    public event DataEventHandler<string> Updated;

    /// <summary>
    /// Gets the error message.
    /// </summary>
    public string ErrorMessage { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether need stop.
    /// </summary>
    public bool IsEnd { get; set; }

    /// <summary>
    /// Gets or sets the turn-based chat message model.
    /// </summary>
    internal RoundChatMessageModel Model { get; set; }

    /// <summary>
    /// Gets the current length of the text.
    /// </summary>
    public int Length => current.Length;

    /// <summary>
    /// Clears the text.
    /// </summary>
    public void Clear()
    {
        current.Clear();
        Update();
    }

    /// <summary>
    /// Sets the text.
    /// </summary>
    /// <param name="value">The new text value to set.</param>
    public void Set(string value)
    {
        current.Clear();
        current.Append(value);
    }

    /// <summary>
    /// Appends a copy of the specified string to the text value.
    /// </summary>
    /// <param name="value">The string to append.</param>
    public void Append(string value)
    {
        current.Append(value);
        Update();
    }

    /// <summary>
    /// Appends the default line terminator to the end of the text value.
    /// </summary>
    public void AppendLine()
    {
        current.AppendLine();
        Update();
    }

    /// <summary>
    /// Appends a copy of the specified string followed by the default line terminator to the end of the text value.
    /// </summary>
    /// <param name="value">The string to append.</param>
    public void AppendLine(string value)
    {
        current.AppendLine(value);
        Update();
    }

    /// <summary>
    /// Appends a copy of the specified string and the default line terminator to the end of the text value.
    /// </summary>
    /// <param name="value">The string to append.</param>
    public void AppendLineAnd(string value)
    {
        current.AppendLine();
        current.Append(value);
        Update();
    }

    /// <summary>
    /// Marks as error and stops.
    /// </summary>
    /// <param name="message">The error message.</param>
    public void SetError(string message)
    {
        IsEnd = true;
        ErrorMessage = message;
        if (Model != null) Model.UpdateState(RoundChatMessageStates.ResponseError, message);
        Updated?.Invoke(this, current.ToString());
    }

    /// <summary>
    /// Gets the text value.
    /// </summary>
    /// <returns>A string whose value is the same as this instance.</returns>
    public override string ToString()
        => current.ToString() ?? string.Empty;

    /// <summary>
    /// Raises update events.
    /// </summary>
    private void Update()
    {
        var s = current.ToString();
        if (Model != null) Model.Answer = s;
        Updated?.Invoke(this, current.ToString());
    }
}
