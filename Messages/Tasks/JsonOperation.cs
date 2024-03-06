using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Trivial.Reflection;
using Trivial.Text;

namespace Trivial.Tasks;
/// <summary>
/// The interface for JSON operation self-describing host.
/// </summary>
public interface IJsonTypeOperationDescriptive : IJsonOperationDescriptive
{
    /// <summary>
    /// Creates operation description.
    /// </summary>
    /// <param name="handler">The additional handler to control the creation.</param>
    /// <returns>The operation description.</returns>
    JsonOperationDescription CreateDescription(BaseJsonOperationSchemaHandler handler);
}

/// <summary>
/// The base JSON operation.
/// </summary>
public abstract class BaseJsonOperation : IJsonOperationDescriptive
{
    /// <summary>
    /// Processes.
    /// </summary>
    /// <param name="arguments">The arguments.</param>
    /// <param name="contextValue">The context value.</param>
    /// <param name="cancellationToken">The optional cancellation token.</param>
    /// <returns>The result.</returns>
    public abstract Task<JsonObjectNode> ProcessAsync(JsonObjectNode arguments, object contextValue, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes.
    /// </summary>
    /// <param name="arguments">The arguments.</param>
    /// <param name="contextValue">The context value.</param>
    /// <param name="cancellationToken">The optional cancellation token.</param>
    /// <returns>The result.</returns>
    /// <exception cref="ArgumentException">arguments was not valid.</exception>
    /// <exception cref="JsonException">JSON serialize or deserialize failed.</exception>
    /// <exception cref="NotSupportedException">Not supported.</exception>
    public virtual async Task<string> ProcessAsync(string arguments, object contextValue, CancellationToken cancellationToken = default)
    {
        var result = await ProcessAsync(JsonObjectNode.Parse(arguments), contextValue, cancellationToken);
        return JsonOperations.SerializeResult(result);
    }

    /// <summary>
    /// Creates operation description.
    /// </summary>
    /// <returns>The operation description.</returns>
    public abstract JsonOperationDescription CreateDescription();

    /// <summary>
    /// Runs asynchronized.
    /// </summary>
    /// <param name="action">The action to run.</param>
    /// <returns>The task result.</returns>
    protected static Task RunAsync(Action action = null)
        => Task.Run(action ?? JsonOperations.Empty);
}

/// <summary>
/// The base JSON operation.
/// </summary>
public abstract class BaseJsonOperation<TIn, TOut> : BaseJsonOperation, IJsonTypeOperationDescriptive
{
    /// <summary>
    /// Gets or sets the schema description creation handler.
    /// </summary>
    public BaseJsonOperationSchemaHandler SchemaHandler { get; set; }

    /// <summary>
    /// Processes.
    /// </summary>
    /// <param name="arguments">The arguments.</param>
    /// <param name="contextValue">The context value.</param>
    /// <param name="cancellationToken">The optional cancellation token.</param>
    /// <returns>The result.</returns>
    public abstract Task<TOut> ProcessAsync(TIn arguments, object contextValue, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes.
    /// </summary>
    /// <param name="arguments">The arguments.</param>
    /// <param name="contextValue">The context value.</param>
    /// <param name="cancellationToken">The optional cancellation token.</param>
    /// <returns>The result.</returns>
    /// <exception cref="ArgumentException">arguments was not valid.</exception>
    /// <exception cref="JsonException">JSON serialize or deserialize failed.</exception>
    /// <exception cref="NotSupportedException">Not supported.</exception>
    public override async Task<JsonObjectNode> ProcessAsync(JsonObjectNode arguments, object contextValue, CancellationToken cancellationToken = default)
    {
        if (arguments == null) return null;
        var result = await ProcessAsync(JsonOperations.DeserializeArguments<TIn>(arguments?.ToString()), contextValue, cancellationToken);
        return JsonOperations.ToResultJson(result);
    }

    /// <summary>
    /// Processes.
    /// </summary>
    /// <param name="arguments">The arguments.</param>
    /// <param name="contextValue">The context value.</param>
    /// <param name="cancellationToken">The optional cancellation token.</param>
    /// <returns>The result.</returns>
    /// <exception cref="ArgumentException">arguments was not valid.</exception>
    /// <exception cref="JsonException">JSON serialize or deserialize failed.</exception>
    /// <exception cref="NotSupportedException">Not supported.</exception>
    public override async Task<string> ProcessAsync(string arguments, object contextValue, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(arguments)) return null;
        var result = await ProcessAsync(JsonOperations.DeserializeArguments<TIn>(arguments), contextValue, cancellationToken);
        return JsonOperations.SerializeResult(result);
    }

    /// <summary>
    /// Creates operation description.
    /// </summary>
    /// <returns>The operation description.</returns>
    public override JsonOperationDescription CreateDescription()
        => CreateDescription(null);

    /// <summary>
    /// Creates operation description.
    /// </summary>
    /// <returns>The operation description.</returns>
    public virtual JsonOperationDescription CreateDescription(BaseJsonOperationSchemaHandler handler)
    {
        var type = GetType();
        var method = type.GetMethod(nameof(ProcessAsync), [typeof(TIn), typeof(object), typeof(CancellationToken)]);
        handler ??= SchemaHandler ?? JsonOperations.SchemaHandler;
        var desc = JsonOperationDescription.Create(method, null, handler);
        if (desc == null) return null;
        if (string.IsNullOrEmpty(desc.Description)) desc.Description = StringExtensions.GetDescription(type);
        JsonOperations.UpdatePath(desc, GetPathInfo() ?? JsonOperations.GetJsonDescriptionPath(method), type);
        OnOperationDescriptionDataFill(desc.Data);
        handler.OnCreate(method, desc);
        return desc;
    }

    /// <summary>
    /// Occurs on fill the JSON operation description data.
    /// </summary>
    /// <param name="data"></param>
    protected virtual void OnOperationDescriptionDataFill(JsonObjectNode data)
    {
    }

    /// <summary>
    /// Gets path information.
    /// </summary>
    /// <returns>THe path info.</returns>
    protected virtual JsonOperationPathAttribute GetPathInfo()
        => null;
}

/// <summary>
/// The schema creation handler for JSON operation.
/// </summary>
public class BaseJsonOperationSchemaHandler : IJsonNodeSchemaCreationHandler<Type>
{
    /// <summary>
    /// Formats or converts the schema instance by customization.
    /// </summary>
    /// <param name="type">The source type.</param>
    /// <param name="result">The JSON schema created to convert or format.</param>
    /// <param name="breadcrumb">The path breadcrumb.</param>
    /// <returns>The JSON schema of final result.</returns>
    public virtual JsonNodeSchemaDescription Convert(Type type, JsonNodeSchemaDescription result, NodePathBreadcrumb<Type> breadcrumb)
        => result is JsonIntegerSchemaDescription i ? new JsonNumberSchemaDescription(i)
        {
            Tag = i.Tag,
            Description = i.Description,
        } : result;

    /// <summary>
    /// Occurs on the JSON operation description is created.
    /// </summary>
    /// <param name="method">The method info.</param>
    /// <param name="description">The JSON operation description.</param>
    public virtual void OnCreate(MethodInfo method, JsonOperationDescription description)
    {
    }
}
