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
    JsonOperationDescription CreateDescription(IJsonNodeSchemaCreationHandler<Type> handler);
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
        return result?.ToString();
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
    public IJsonNodeSchemaCreationHandler<Type> SchemaHandler { get; set; }

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
    /// <exception cref="InvalidOperationException">Serialize result to JSON failed.</exception>
    /// <exception cref="JsonException">JSON serialize or deserialize failed.</exception>
    /// <exception cref="NotSupportedException">Not supported.</exception>
    public override async Task<string> ProcessAsync(string arguments, object contextValue, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(arguments)) return null;
        var result = await ProcessAsync(JsonOperations.DeserializeArguments<TIn>(arguments), contextValue, cancellationToken);
        return Serialize(result, contextValue);
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
    /// <param name="schemaHandler">The optional schema handler.</param>
    /// <returns>The operation description.</returns>
    public virtual JsonOperationDescription CreateDescription(IJsonNodeSchemaCreationHandler<Type> schemaHandler)
    {
        var type = GetType();
        var method = type.GetMethod(nameof(ProcessAsync), [typeof(TIn), typeof(object), typeof(CancellationToken)]);
        schemaHandler ??= SchemaHandler ?? JsonOperations.SchemaHandler;
        var desc = JsonOperationDescription.Create(method, null, schemaHandler);
        if (desc == null) return null;
        if (string.IsNullOrEmpty(desc.Description)) desc.Description = StringExtensions.GetDescription(type);
        JsonOperations.UpdatePath(desc, GetPathInfo() ?? JsonOperations.GetJsonDescriptionPath(method), type);
        OnOperationDescriptionDataFill(desc.Data);
        if (schemaHandler is BaseJsonOperationSchemaHandler sh) sh.OnCreate(method, desc);
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

    /// <summary>
    /// Serializes
    /// </summary>
    /// <param name="result">The result.</param>
    /// <param name="contextValue">The context value.</param>
    /// <returns>The JSON format string.</returns>
    /// <exception cref="InvalidOperationException">Serialize result to JSON failed.</exception>
    /// <exception cref="NotSupportedException">There is no compatible JSON converter for the typeor its serializable members.</exception>
    /// <exception cref="JsonException">JSON serialize failed.</exception>
    protected virtual string Serialize(TOut result, object contextValue)
        => JsonOperations.SerializeResult(result);
}
