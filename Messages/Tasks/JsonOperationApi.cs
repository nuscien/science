using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Trivial.Data;
using Trivial.Text;

namespace Trivial.Tasks;

/// <summary>
/// The API with JSON operation set.
/// </summary>
public class JsonOperationApi : IJsonObjectHost
{
    private readonly Dictionary<string, JsonOperationInfo> ops = new();

    /// <summary>
    /// Adds or removes the event handler occurred on processing.
    /// </summary>
    public event DataEventHandler<JsonOperationProcessingArgs> Processing;

    /// <summary>
    /// Gets the additional operations.
    /// </summary>
    public IEnumerable<JsonOperationInfo> Operations => ops.Values;

    /// <summary>
    /// Gets or sets the default HTTP method.
    /// </summary>
    public HttpMethod DefaultHttpMethod { get; set; } = HttpMethod.Post;

    /// <summary>
    /// Processes.
    /// </summary>
    /// <param name="path">The relative path.</param>
    /// <param name="httpMethod">The HTTP method.</param>
    /// <param name="args">The arguments.</param>
    /// <param name="contextValue">The context value.</param>
    /// <param name="cancellationToken">The optional cancellation token.</param>
    /// <returns>The result.</returns>
    public Task<JsonObjectNode> ProcessAsync(string path, HttpMethod httpMethod, JsonObjectNode args, object contextValue, CancellationToken cancellationToken = default)
    {
        var info = GetInfo(path, httpMethod);
        Processing?.Invoke(this, new(new(info?.Operation, path, false, httpMethod, contextValue)));
        return info.ProcessAsync(args, contextValue, cancellationToken);
    }

    /// <summary>
    /// Processes.
    /// </summary>
    /// <param name="path">The relative path.</param>
    /// <param name="ignorePathCase">true if ignore case of path; otherwise, false.</param>
    /// <param name="httpMethod">The HTTP method.</param>
    /// <param name="args">The arguments.</param>
    /// <param name="contextValue">The context value.</param>
    /// <param name="cancellationToken">The optional cancellation token.</param>
    /// <returns>The result.</returns>
    public Task<JsonObjectNode> ProcessAsync(string path, bool ignorePathCase, HttpMethod httpMethod, JsonObjectNode args, object contextValue, CancellationToken cancellationToken = default)
    {
        var info = GetInfo(path, httpMethod, ignorePathCase);
        Processing?.Invoke(this, new(new(info?.Operation, path, false, httpMethod, contextValue)));
        return info.ProcessAsync(args, contextValue, cancellationToken);
    }

    /// <summary>
    /// Processes.
    /// </summary>
    /// <param name="path">The relative path.</param>
    /// <param name="httpMethod">The HTTP method.</param>
    /// <param name="args">The arguments.</param>
    /// <param name="contextValue">The context value.</param>
    /// <param name="cancellationToken">The optional cancellation token.</param>
    /// <returns>The result.</returns>
    public Task<string> ProcessAsync(string path, HttpMethod httpMethod, string args, object contextValue, CancellationToken cancellationToken = default)
    {
        var info = GetInfo(path, httpMethod);
        Processing?.Invoke(this, new(new(info?.Operation, path, false, httpMethod, contextValue)));
        return info.ProcessAsync(args, contextValue, cancellationToken);
    }

    /// <summary>
    /// Processes.
    /// </summary>
    /// <param name="path">The relative path.</param>
    /// <param name="ignorePathCase">true if ignore case of path; otherwise, false.</param>
    /// <param name="httpMethod">The HTTP method.</param>
    /// <param name="args">The arguments.</param>
    /// <param name="contextValue">The context value.</param>
    /// <param name="cancellationToken">The optional cancellation token.</param>
    /// <returns>The result.</returns>
    public Task<string> ProcessAsync(string path, bool ignorePathCase, HttpMethod httpMethod, string args, object contextValue, CancellationToken cancellationToken = default)
    {
        var info = GetInfo(path, httpMethod, ignorePathCase);
        Processing?.Invoke(this, new(new(info?.Operation, path, false, httpMethod, contextValue)));
        return info.ProcessAsync(args, contextValue, cancellationToken);
    }

    /// <summary>
    /// Registers.
    /// </summary>
    /// <param name="operation">The JSON operation to register.</param>
    /// <returns>The operation identifier.</returns>
    public string Register(BaseJsonOperation operation)
    {
        var op = new JsonOperationInfo(operation);
        if (string.IsNullOrWhiteSpace(op.Id)) return null;
        ops[op.Id] = op;
        return op.Id;
    }

    /// <summary>
    /// Removes the value with the specified key from the collection.
    /// </summary>
    /// <param name="id">The operation identifier.</param>
    /// <returns>true if the element is successfully found and removed; otherwise, false. This method returns false if key is not found in the mapping.</returns>
    public bool Remove(string id)
        => id != null && ops.Remove(id);

    /// <summary>
    /// Removes the value with the specified key from the collection.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <returns>true if the element is successfully found and removed; otherwise, false. This method returns false if key is not found in the mapping.</returns>
    public bool Remove(BaseJsonOperation operation)
    {
        if (operation == null) return false;
        string id = null;
        foreach (var item in ops.Values)
        {
            if (item?.Operation != operation) continue;
            id = item.Id;
        }

        return id != null && ops.Remove(id);
    }

    /// <summary>
    /// Gets the specific operation.
    /// </summary>
    /// <param name="id">The operation identifier.</param>
    /// <returns>The operation; or null, if non-exist.</returns>
    public BaseJsonOperation Get(string id)
        => id != null && ops.TryGetValue(id, out var item) ? item.Operation : null;

    /// <summary>
    /// Gets the operation information.
    /// </summary>
    /// <param name="path">The relative path.</param>
    /// <param name="httpMethod">The HTTP method.</param>
    /// <param name="ignoreCase">true if ignore case; otherwise, false.</param>
    /// <returns>The operation info; or null, if non-exist.</returns>
    public JsonOperationInfo GetInfo(string path, HttpMethod httpMethod, bool ignoreCase = false)
    {
        var hm = JsonOperations.FormatPath(httpMethod?.Method);
        if (string.IsNullOrEmpty(hm)) hm = null;
        var defaultHttpMethod = JsonOperations.FormatPath(DefaultHttpMethod?.Method);
        if (string.IsNullOrEmpty(defaultHttpMethod)) defaultHttpMethod = "post";
        if (ignoreCase) path = JsonOperations.FormatPath(path); ;
        foreach (var item in ops.Values)
        {
            var itemPath = ignoreCase ? JsonOperations.FormatPath(item.Path) : item.Path;
            if (itemPath != path) continue;
            var httpMethodString = JsonOperations.FormatPath(item.HttpMethodString);
            if (hm == httpMethodString) return item;
            if (hm == null && httpMethodString == defaultHttpMethod) return item;
        }

        return null;
    }

    /// <summary>
    /// Tests if has the specific operation.
    /// </summary>
    /// <param name="id">The operation identifier.</param>
    /// <returns>true if contains; otherwise, false.</returns>
    public bool Contains(string id)
        => id != null && ops.TryGetValue(id, out _);

    /// <summary>
    /// Tests if has the specific operation.
    /// </summary>
    /// <param name="item">The property to test.</param>
    /// <returns>true if contains; otherwise, false.</returns>
    public bool Contains(BaseJsonOperation item)
        => item != null && ops.Values.Any(ele => ele?.Operation == item);

    /// <summary>
    /// Converts to JSON object node about the JSON operations of this instance.
    /// </summary>
    /// <returns>The JSON object about the JSON operations registered.</returns>
    public JsonObjectNode ToJson()
        => JsonOperations.ToJson(Operations.Select(ele => ele.OperationDescription));
}

/// <summary>
/// The arugments for JSON operation API.
/// </summary>
public class JsonOperationProcessingArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the JsonOperationProcessingArgs class.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="path">The relative path.</param>
    /// <param name="ignoreCase">true if ignore case for the relative path; otherwise, false.</param>
    /// <param name="httpMethod">The HTTP method.</param>
    /// <param name="contextValue">The context value.</param>
    public JsonOperationProcessingArgs(BaseJsonOperation operation, string path, bool ignoreCase, HttpMethod httpMethod, object contextValue)
    {
        Operation = operation;
        Path = path;
        IgnorePathCase = ignoreCase;
        HttpMethod = httpMethod;
        ContextValue = contextValue;
    }

    /// <summary>
    /// Gets the processing data time.
    /// </summary>
    public DateTime ProcessTime { get; }

    /// <summary>
    /// Gets the operation.
    /// </summary>
    public BaseJsonOperation Operation { get; }

    /// <summary>
    /// Gets the path.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Gets a value indicating whether ignore case for the relative path.
    /// </summary>
    public bool IgnorePathCase { get; }

    /// <summary>
    /// Gets the HTTP method.
    /// </summary>
    public HttpMethod HttpMethod { get; }

    /// <summary>
    /// Gets the context value.
    /// </summary>
    public object ContextValue { get; }
}

/// <summary>
/// The JSON operation information.
/// </summary>
public class JsonOperationInfo
{
    /// <summary>
    /// Initializes a new instance of the JsonOperationInfo class.
    /// </summary>
    /// <param name="operation">The JSON operation.</param>
    internal JsonOperationInfo(BaseJsonOperation operation)
    {
        Operation = operation;
        OperationDescription = operation?.CreateDescription() ?? new();
    }

    internal JsonOperationDescription OperationDescription { get; }

    internal BaseJsonOperation Operation { get; }

    /// <summary>
    /// Gets the operation identifier.
    /// </summary>
    public string Id => OperationDescription?.Id;

    /// <summary>
    /// Gets the operation description.
    /// </summary>
    public string Description => OperationDescription.Description;

    /// <summary>
    /// Gets the relative path of the operation.
    /// </summary>
    public string Path => OperationDescription.Data.TryGetStringTrimmedValue(JsonOperations.PathProperty, true);

    /// <summary>
    /// Gets the HTTP method of the operation.
    /// </summary>
    public string HttpMethodString => OperationDescription.Data.TryGetStringTrimmedValue(JsonOperations.HttpMethodProperty, true);

    /// <summary>
    /// Gets the optional additional data.
    /// </summary>
    public JsonObjectNode Data => OperationDescription.Data;

    /// <summary>
    /// Processes.
    /// </summary>
    /// <param name="args">The arguments.</param>
    /// <param name="contextValue">The context value.</param>
    /// <param name="cancellationToken">The optional cancellation token.</param>
    /// <returns>The result.</returns>
    public Task<JsonObjectNode> ProcessAsync(JsonObjectNode args, object contextValue, CancellationToken cancellationToken = default)
        => Operation?.ProcessAsync(args, contextValue, cancellationToken);

    /// <summary>
    /// Processes.
    /// </summary>
    /// <param name="args">The arguments.</param>
    /// <param name="contextValue">The context value.</param>
    /// <param name="cancellationToken">The optional cancellation token.</param>
    /// <returns>The result.</returns>
    public Task<string> ProcessAsync(string args, object contextValue, CancellationToken cancellationToken = default)
        => Operation?.ProcessAsync(args, contextValue, cancellationToken);
}
