using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Trivial.Data;
using Trivial.Reflection;
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
    /// Adds or removes the event handler occurred on processing.
    /// </summary>
    public event DataEventHandler<JsonOperationProcessingArgs> Processed;

    /// <summary>
    /// Adds or removes the event handler occurred on processing.
    /// </summary>
    public event DataEventHandler<JsonOperationProcessingErrorArgs> ProcessFailed;

    /// <summary>
    /// Gets the additional operations.
    /// </summary>
    public IEnumerable<JsonOperationInfo> Operations => ops.Values;

    /// <summary>
    /// Gets or sets the default HTTP method.
    /// </summary>
    public HttpMethod DefaultHttpMethod { get; set; } = HttpMethod.Post;

    /// <summary>
    /// Gets all identifiers of the operations registered.
    /// </summary>
    public IEnumerable<string> Ids => ops.Keys;

    /// <summary>
    /// Processes.
    /// </summary>
    /// <param name="input">The input info.</param>
    /// <param name="cancellationToken">The optional cancellation token.</param>
    /// <returns>The result.</returns>
    /// <exception cref="ArgumentNullException">input should not be null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The relative path is not found.</exception>
    /// <exception cref="InvalidOperationException">Operation is not valid.</exception>
    /// <exception cref="OperationCanceledException">Operation is cancelled.</exception>
    public async Task<JsonObjectNode> ProcessAsync(JsonOperationInput<JsonObjectNode> input, CancellationToken cancellationToken = default)
    {
        if (input == null) throw new ArgumentNullException(nameof(input), "input was null.");
        var path = input.Path;
        var httpMethod = input.HttpMethod;
        var argument = input.Argument;
        var contextValue = input.ContextValue;
        var info = GetInfo(path, httpMethod, input.IgnorePathCase);
        var operation = (info?.Operation) ?? throw new ArgumentOutOfRangeException(nameof(input), "The relative path is not found.");
        input.OnRoute(operation);
        var args = new JsonOperationProcessingArgs(operation, input);
        OnRoute(input);
        Processing?.Invoke(this, new(args));
        JsonObjectNode resp;
        try
        {
            resp = await info.ProcessAsync(argument, contextValue, cancellationToken);
            input.OnProcess(resp, operation);
        }
        catch (Exception ex)
        {
            var error = new JsonOperationProcessingErrorArgs(ex, args);
            input.OnFailure(ex, operation);
            OnFailure(input, ex);
            ProcessFailed?.Invoke(this, new(error));
            throw;
        }

        OnProcess(input, resp);
        Processed?.Invoke(this, new(args));
        return resp;
    }

    /// <summary>
    /// Processes.
    /// </summary>
    /// <param name="path">The relative path.</param>
    /// <param name="httpMethod">The HTTP method.</param>
    /// <param name="argument">The argument.</param>
    /// <param name="contextValue">The context value.</param>
    /// <param name="cancellationToken">The optional cancellation token.</param>
    /// <returns>The result.</returns>
    public Task<JsonObjectNode> ProcessAsync(string path, HttpMethod httpMethod, JsonObjectNode argument, object contextValue, CancellationToken cancellationToken = default)
        => ProcessAsync(new JsonOperationInput<JsonObjectNode>(path, httpMethod, argument, contextValue), cancellationToken);

    /// <summary>
    /// Processes.
    /// </summary>
    /// <param name="path">The relative path.</param>
    /// <param name="ignorePathCase">true if ignore case of path; otherwise, false.</param>
    /// <param name="httpMethod">The HTTP method.</param>
    /// <param name="argument">The argument.</param>
    /// <param name="contextValue">The context value.</param>
    /// <param name="cancellationToken">The optional cancellation token.</param>
    /// <returns>The result.</returns>
    public Task<JsonObjectNode> ProcessAsync(string path, bool ignorePathCase, HttpMethod httpMethod, JsonObjectNode argument, object contextValue, CancellationToken cancellationToken = default)
        => ProcessAsync(new JsonOperationInput<JsonObjectNode>(path, ignorePathCase, httpMethod, argument, contextValue), cancellationToken);

    /// <summary>
    /// Processes.
    /// </summary>
    /// <param name="input">The input info.</param>
    /// <param name="cancellationToken">The optional cancellation token.</param>
    /// <returns>The result.</returns>
    /// <exception cref="ArgumentNullException">input should not be null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The relative path is not found.</exception>
    /// <exception cref="InvalidOperationException">Operation is not valid.</exception>
    /// <exception cref="OperationCanceledException">Operation is cancelled.</exception>
    public async Task<string> ProcessAsync(JsonOperationInput<string> input, CancellationToken cancellationToken = default)
    {
        if (input == null) throw new ArgumentNullException(nameof(input), "input was null.");
        var path = input.Path;
        var httpMethod = input.HttpMethod;
        var argument = input.Argument;
        var contextValue = input.ContextValue;
        var info = GetInfo(path, httpMethod, input.IgnorePathCase);
        var operation = (info?.Operation) ?? throw new ArgumentOutOfRangeException(nameof(input), "The relative path is not found.");
        input.OnRoute(operation);
        var args = new JsonOperationProcessingArgs(operation, input);
        OnRoute(input);
        Processing?.Invoke(this, new(args));
        string resp;
        try
        {
            resp = await info.ProcessAsync(argument, contextValue, cancellationToken);
            input.OnProcess(resp, operation);
        }
        catch (Exception ex)
        {
            var error = new JsonOperationProcessingErrorArgs(ex, args);
            input.OnFailure(ex, operation);
            OnFailure(input, ex);
            ProcessFailed?.Invoke(this, new(error));
            throw;
        }

        OnProcess(input, resp);
        Processed?.Invoke(this, new(args));
        return resp;
    }

    /// <summary>
    /// Processes.
    /// </summary>
    /// <param name="path">The relative path.</param>
    /// <param name="httpMethod">The HTTP method.</param>
    /// <param name="argument">The argument.</param>
    /// <param name="contextValue">The context value.</param>
    /// <param name="cancellationToken">The optional cancellation token.</param>
    /// <returns>The result.</returns>
    public Task<string> ProcessAsync(string path, HttpMethod httpMethod, string argument, object contextValue, CancellationToken cancellationToken = default)
        => ProcessAsync(new JsonOperationInput<string>(path, httpMethod, argument, contextValue), cancellationToken);

    /// <summary>
    /// Processes.
    /// </summary>
    /// <param name="path">The relative path.</param>
    /// <param name="ignorePathCase">true if ignore case of path; otherwise, false.</param>
    /// <param name="httpMethod">The HTTP method.</param>
    /// <param name="argument">The argument.</param>
    /// <param name="contextValue">The context value.</param>
    /// <param name="cancellationToken">The optional cancellation token.</param>
    /// <returns>The result.</returns>
    public Task<string> ProcessAsync(string path, bool ignorePathCase, HttpMethod httpMethod, string argument, object contextValue, CancellationToken cancellationToken = default)
        => ProcessAsync(new JsonOperationInput<string>(path, ignorePathCase, httpMethod, argument, contextValue), cancellationToken);

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
    /// Registers.
    /// </summary>
    /// <param name="handler">The processing handler.</param>
    /// <param name="id">The operation identifier.</param>
    /// <param name="schemaHandler">The optional schema handler.</param>
    /// <returns>The operation identifier.</returns>
    public BaseJsonOperation RegisterFunc<TIn, TOut>(Func<TIn, object, CancellationToken, Task<TOut>> handler, string id = null, BaseJsonOperationSchemaHandler schemaHandler = null)
    {
        var op = new JsonOperationInfo(JsonOperations.Create(handler, id, schemaHandler));
        if (string.IsNullOrWhiteSpace(op.Id)) return null;
        ops[op.Id] = op;
        return op.Operation;
    }

    /// <summary>
    /// Registers.
    /// </summary>
    /// <param name="handler">The processing handler.</param>
    /// <param name="id">The operation identifier.</param>
    /// <param name="schemaHandler">The optional schema handler.</param>
    /// <returns>The operation identifier.</returns>
    public BaseJsonOperation RegisterFunc<TIn, TOut>(Func<TIn, CancellationToken, Task<TOut>> handler, string id = null, BaseJsonOperationSchemaHandler schemaHandler = null)
    {
        var op = new JsonOperationInfo(JsonOperations.Create(handler, id, schemaHandler));
        if (string.IsNullOrWhiteSpace(op.Id)) return null;
        ops[op.Id] = op;
        return op.Operation;
    }

    /// <summary>
    /// Registers.
    /// </summary>
    /// <param name="handler">The processing handler.</param>
    /// <param name="id">The operation identifier.</param>
    /// <param name="schemaHandler">The optional schema handler.</param>
    /// <returns>The operation identifier.</returns>
    public BaseJsonOperation RegisterFunc<TIn, TOut>(Func<TIn, object, TOut> handler, string id = null, BaseJsonOperationSchemaHandler schemaHandler = null)
    {
        var op = new JsonOperationInfo(JsonOperations.Create(handler, id, schemaHandler));
        if (string.IsNullOrWhiteSpace(op.Id)) return null;
        ops[op.Id] = op;
        return op.Operation;
    }

    /// <summary>
    /// Registers.
    /// </summary>
    /// <param name="handler">The processing handler.</param>
    /// <param name="id">The operation identifier.</param>
    /// <param name="schemaHandler">The optional schema handler.</param>
    /// <returns>The operation identifier.</returns>
    public BaseJsonOperation RegisterFunc<TIn, TOut>(Func<TIn, TOut> handler, string id = null, BaseJsonOperationSchemaHandler schemaHandler = null)
    {
        var op = new JsonOperationInfo(JsonOperations.Create(handler, id, schemaHandler));
        if (string.IsNullOrWhiteSpace(op.Id)) return null;
        ops[op.Id] = op;
        return op.Operation;
    }

    /// <summary>
    /// Registers.
    /// </summary>
    /// <param name="target">The target object.</param>
    /// <param name="method">The method info.</param>
    /// <param name="schemaHandler">The optional schema handler.</param>
    /// <param name="id">The operation identifier.</param>
    /// <returns>The operation identifier.</returns>
    public BaseJsonOperation RegisterFromMethod(object target, MethodInfo method, BaseJsonOperationSchemaHandler schemaHandler = null, string id = null)
    {
        var op = new JsonOperationInfo(JsonOperations.Create(target, method, schemaHandler));
        if (string.IsNullOrWhiteSpace(op.Id)) return null;
        ops[op.Id] = op;
        return op.Operation;
    }

    /// <summary>
    /// Registers.
    /// </summary>
    /// <param name="target">The target object.</param>
    /// <param name="methodName">The method name.</param>
    /// <param name="schemaHandler">The optional schema handler.</param>
    /// <param name="id">The operation identifier.</param>
    /// <returns>The operation identifier.</returns>
    public BaseJsonOperation RegisterFromMethod(object target, string methodName, BaseJsonOperationSchemaHandler schemaHandler = null, string id = null)
    {
        MethodInfo method;
        if (target is Type type)
        {
            method = type.GetMethod(methodName);
            target = null;
        }
        else
        {
            method = target.GetType().GetMethod(methodName);
        }

        return RegisterFromMethod(target, method, schemaHandler, id);
    }

    /// <summary>
    /// Registers.
    /// </summary>
    /// <param name="target">The target object.</param>
    /// <param name="methodName">The method name.</param>
    /// <param name="parameterTypes">An array of type objects representing the number, order, and type of the parameters for the method to get; or an empty array of type objects to get a method that takes no parameters.</param>
    /// <param name="schemaHandler">The optional schema handler.</param>
    /// <param name="id">The operation identifier.</param>
    /// <returns>The operation identifier.</returns>
    public BaseJsonOperation RegisterFromMethod(object target, string methodName, Type[] parameterTypes, BaseJsonOperationSchemaHandler schemaHandler = null, string id = null)
    {
        MethodInfo method;
        if (target is Type type)
        {
            method = type.GetMethod(methodName, parameterTypes);
            target = null;
        }
        else
        {
            method = target.GetType().GetMethod(methodName, parameterTypes);
        }

        return RegisterFromMethod(target, method, schemaHandler, id);
    }

    /// <summary>
    /// Creates a JSON operation.
    /// </summary>
    /// <param name="target">The target object.</param>
    /// <param name="property">The property info.</param>
    /// <param name="id">The operation identifier.</param>
    /// <returns>The JSON operation.</returns>
    public BaseJsonOperation RegisterFromProperty(object target, PropertyInfo property, string id = null)
    {
        if (property == null || !ObjectConvert.TryGetProperty(target, property, out BaseJsonOperation operation)) return null;
        var desc = JsonOperationDescription.CreateFromProperty(target, property, id);
        if (string.IsNullOrWhiteSpace(desc?.Id)) return null;
        var op = new JsonOperationInfo(operation, desc);
        ops[op.Id] = op;
        return op.Operation;
    }

    /// <summary>
    /// Creates a JSON operation.
    /// </summary>
    /// <param name="target">The target object.</param>
    /// <param name="propertyName">The property name.</param>
    /// <param name="id">The operation identifier.</param>
    /// <returns>The JSON operation.</returns>
    public BaseJsonOperation RegisterFromProperty(object target, string propertyName, string id = null)
    {
        if (target is Type type) return RegisterFromProperty(null, type.GetProperty(propertyName), id);
        if (string.IsNullOrWhiteSpace(propertyName) || !ObjectConvert.TryGetProperty(target, propertyName, out BaseJsonOperation operation)) return null;
        var desc = JsonOperationDescription.CreateFromProperty(target, propertyName, id);
        if (string.IsNullOrWhiteSpace(desc?.Id)) return null;
        var op = new JsonOperationInfo(operation, desc);
        ops[op.Id] = op;
        return op.Operation;
    }

    /// <summary>
    /// Registers all members with path info.
    /// </summary>
    /// <param name="target">The target object.</param>
    /// <param name="schemaHandler">The optional schema handler.</param>
    /// <returns>The JSON operation list registered.</returns>
    public IList<BaseJsonOperation> RegisterRange(object target, BaseJsonOperationSchemaHandler schemaHandler = null)
    {
        var list = new List<BaseJsonOperation>();
        if (target == null) return list;
        if (target is Type type) target = null;
        else type = target.GetType();
        var methods = type.GetMethods();
        foreach (var method in methods)
        {
            var path = JsonOperations.GetJsonDescriptionPath(method);
            if (path == null) continue;
            var operation = RegisterFromMethod(target, method, schemaHandler);
            if (operation == null) continue;
            list.Add(operation);
        }

        var properties = type.GetProperties();
        foreach (var property in properties)
        {
            var path = JsonOperations.GetJsonDescriptionPath(property);
            if (path == null) continue;
            var operation = RegisterFromProperty(target, property);
            if (operation == null) continue;
            list.Add(operation);
        }

        return list;
    }

    /// <summary>
    /// Registers all operations.
    /// </summary>
    /// <param name="operations">The operation list to register.</param>
    /// <returns>The operation identifier list.</returns>
    public IList<string> RegisterRange(IEnumerable<BaseJsonOperation> operations)
    {
        var list = new List<string>();
        if (operations == null) return list;
        foreach (var operation in operations)
        {
            var id = Register(operation);
            list.Add(id);
        }

        return list;
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
    public virtual JsonOperationInfo GetInfo(string path, HttpMethod httpMethod, bool ignoreCase = false)
    {
        var hm = JsonOperations.FormatPath(httpMethod?.Method);
        if (string.IsNullOrEmpty(hm)) hm = null;
        var defaultHttpMethod = JsonOperations.FormatPath(DefaultHttpMethod?.Method);
        if (string.IsNullOrEmpty(defaultHttpMethod)) defaultHttpMethod = "post";
        if (ignoreCase) path = JsonOperations.FormatPath(path);
        foreach (var item in ops.Values)
        {
            var itemPath = ignoreCase ? JsonOperations.FormatPath(item.Path) : item.Path;
            if (itemPath != path) continue;
            var httpMethodString = JsonOperations.FormatPath(item.HttpMethodString);
            if (hm == httpMethodString) return item;
            if ((hm == null && httpMethodString == defaultHttpMethod) || (hm == defaultHttpMethod && httpMethodString == null)) return item;
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

    /// <summary>
    /// Processes on operation routed.
    /// </summary>
    /// <param name="input">The input info.</param>
    protected virtual void OnRoute(JsonOperationInput input)
    {
    }

    /// <summary>
    /// Processes on operation routed.
    /// </summary>
    /// <param name="input">The input info.</param>
    /// <param name="result">The result data.</param>
    protected virtual void OnProcess(JsonOperationInput<string> input, string result)
    {
    }

    /// <summary>
    /// Processes on operation routed.
    /// </summary>
    /// <param name="input">The input info.</param>
    /// <param name="result">The result data.</param>
    protected virtual void OnProcess(JsonOperationInput<JsonObjectNode> input, JsonObjectNode result)
    {
    }

    /// <summary>
    /// Processes on operation routed.
    /// </summary>
    /// <param name="input">The input info.</param>
    /// <param name="exception">The exception instance.</param>
    protected virtual void OnFailure(JsonOperationInput input, Exception exception)
    {
    }

    /// <summary>
    /// Creates JSON operation description collection by a given type.
    /// </summary>
    /// <returns>A collection of the JSON operation description.</returns>
    internal IEnumerable<JsonOperationDescription> CreateDescription()
    {
        foreach (var item in ops.Values)
        {
            var d = item?.CreateDescription();
            if (d != null) yield return d;
        }
    }
}

/// <summary>
/// The input for JSON operation API.
/// </summary>
public class JsonOperationInput
{
    /// <summary>
    /// Initializes a new instance of the BaseJsonOperationProcessingContext class.
    /// </summary>
    /// <param name="path">The relative path.</param>
    /// <param name="httpMethod">The HTTP method.</param>
    /// <param name="contextValue">The context value.</param>
    public JsonOperationInput(string path, HttpMethod httpMethod, object contextValue) : this(path, false, httpMethod, contextValue)
    {
    }

    /// <summary>
    /// Initializes a new instance of the BaseJsonOperationProcessingContext class.
    /// </summary>
    /// <param name="path">The relative path.</param>
    /// <param name="ignoreCase">true if ignore case for the relative path; otherwise, false.</param>
    /// <param name="httpMethod">The HTTP method.</param>
    /// <param name="contextValue">The context value.</param>
    public JsonOperationInput(string path, bool ignoreCase, HttpMethod httpMethod, object contextValue)
    {
        Path = path;
        IgnorePathCase = ignoreCase;
        HttpMethod = httpMethod;
        ContextValue = contextValue;
    }

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
/// The input for JSON operation API.
/// </summary>
public class JsonOperationInput<T> : JsonOperationInput
{
    /// <summary>
    /// Initializes a new instance of the JsonOperationInput class.
    /// </summary>
    /// <param name="path">The relative path.</param>
    /// <param name="httpMethod">The HTTP method.</param>
    /// <param name="argument">The input data.</param>
    /// <param name="contextValue">The context value.</param>
    public JsonOperationInput(string path, HttpMethod httpMethod, T argument, object contextValue)
        : base(path, httpMethod, contextValue)
    {
        Argument = argument;
    }

    /// <summary>
    /// Initializes a new instance of the JsonOperationInput class.
    /// </summary>
    /// <param name="path">The relative path.</param>
    /// <param name="ignoreCase">true if ignore case for the relative path; otherwise, false.</param>
    /// <param name="httpMethod">The HTTP method.</param>
    /// <param name="argument">The input data.</param>
    /// <param name="contextValue">The context value.</param>
    public JsonOperationInput(string path, bool ignoreCase, HttpMethod httpMethod, T argument, object contextValue)
        : base(path, ignoreCase, httpMethod, contextValue)
    {
        Argument = argument;
    }

    /// <summary>
    /// Gets the input data.
    /// </summary>
    public T Argument { get; }

    /// <summary>
    /// Occurs on operation has processed.
    /// </summary>
    /// <param name="operation">The operation.</param>
    public virtual void OnRoute(BaseJsonOperation operation)
    {
    }

    /// <summary>
    /// Occurs on operation has processed.
    /// </summary>
    /// <param name="result">The output data.</param>
    /// <param name="operation">The operation.</param>
    public virtual void OnProcess(T result, BaseJsonOperation operation)
    {
    }

    /// <summary>
    /// Occurs on operation is failed.
    /// </summary>
    /// <param name="exception">The exception instance.</param>
    /// <param name="operation">The operation.</param>
    public virtual void OnFailure(Exception exception, BaseJsonOperation operation)
    {
    }
}

/// <summary>
/// The arguments for JSON operation API.
/// </summary>
public class JsonOperationProcessingArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the JsonOperationProcessingArgs class.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="input">The input info.</param>
    public JsonOperationProcessingArgs(BaseJsonOperation operation, JsonOperationInput input)
    {
        Operation = operation;
        Input = input;
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
    /// Gets the input info.
    /// </summary>
    public JsonOperationInput Input { get; }

    /// <summary>
    /// Gets the path.
    /// </summary>
    public string Path => Input?.Path;

    /// <summary>
    /// Gets a value indicating whether ignore case for the relative path.
    /// </summary>
    public bool IgnorePathCase => Input?.IgnorePathCase ?? false;

    /// <summary>
    /// Gets the HTTP method.
    /// </summary>
    public HttpMethod HttpMethod => Input?.HttpMethod;

    /// <summary>
    /// Gets the context value.
    /// </summary>
    public object ContextValue => Input?.ContextValue;
}

/// <summary>
/// The error arguments for JSON operation API.
/// </summary>
public class JsonOperationProcessingErrorArgs : JsonOperationProcessingArgs
{
    /// <summary>
    /// Initializes a new instance of the JsonOperationProcessingArgs class.
    /// </summary>
    /// <param name="exception">The exception.</param>
    /// <param name="operation">The operation.</param>
    /// <param name="input">The input info.</param>
    public JsonOperationProcessingErrorArgs(Exception exception, BaseJsonOperation operation, JsonOperationInput input)
        : base(operation, input)
    {
        Exception = exception;
    }

    /// <summary>
    /// Initializes a new instance of the JsonOperationProcessingArgs class.
    /// </summary>
    /// <param name="exception">The exception.</param>
    /// <param name="copy">The arguments to copy.</param>
    public JsonOperationProcessingErrorArgs(Exception exception, JsonOperationProcessingArgs copy)
        : this(exception, copy?.Operation, copy?.Input)
    {
    }

    /// <summary>
    /// Gets the exception.
    /// </summary>
    public Exception Exception { get; }

    /// <summary>
    /// Gets the exception type.
    /// </summary>
    /// <returns>The type of the exception.</returns>
    public Type GetExceptionType()
        => Exception?.GetType();
}

/// <summary>
/// The JSON operation information.
/// </summary>
public class JsonOperationInfo : IJsonOperationDescriptive
{
    /// <summary>
    /// Initializes a new instance of the JsonOperationInfo class.
    /// </summary>
    /// <param name="operation">The JSON operation.</param>
    /// <param name="description">The optional JSON operation description.</param>
    internal JsonOperationInfo(BaseJsonOperation operation, JsonOperationDescription description = null)
    {
        Operation = operation;
        OperationDescription = description ?? operation?.CreateDescription() ?? new();
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

    /// <summary>
    /// Creates operation description.
    /// </summary>
    /// <returns>The operation description.</returns>
    public JsonOperationDescription CreateDescription()
        => Operation?.CreateDescription();
}
