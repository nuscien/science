﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
/// The base JSON operation.
/// </summary>
public abstract class BaseJsonOperation : IJsonOperationDescriptive
{
    /// <summary>
    /// Processes.
    /// </summary>
    /// <param name="args">The arguments.</param>
    /// <param name="contextValue">The context value.</param>
    /// <param name="cancellationToken">The optional cancellation token.</param>
    /// <returns>The result.</returns>
    public abstract Task<JsonObjectNode> ProcessAsync(JsonObjectNode args, object contextValue, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes.
    /// </summary>
    /// <param name="args">The arguments.</param>
    /// <param name="contextValue">The context value.</param>
    /// <param name="cancellationToken">The optional cancellation token.</param>
    /// <returns>The result.</returns>
    public virtual async Task<string> ProcessAsync(string args, object contextValue, CancellationToken cancellationToken = default)
    {
        var result = await ProcessAsync(JsonObjectNode.Parse(args), contextValue, cancellationToken);
        return JsonSerializer.Serialize(result);
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
public abstract class BaseJsonOperation<TIn, TOut> : BaseJsonOperation
{
    /// <summary>
    /// Gets or sets the schema description creation handler.
    /// </summary>
    public BaseJsonOperationSchemaHandler SchemaHandler { get; set; }

    /// <summary>
    /// Processes.
    /// </summary>
    /// <param name="args">The arguments.</param>
    /// <param name="contextValue">The context value.</param>
    /// <param name="cancellationToken">The optional cancellation token.</param>
    /// <returns>The result.</returns>
    public abstract Task<TOut> ProcessAsync(TIn args, object contextValue, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes.
    /// </summary>
    /// <param name="args">The arguments.</param>
    /// <param name="contextValue">The context value.</param>
    /// <param name="cancellationToken">The optional cancellation token.</param>
    /// <returns>The result.</returns>
    public override async Task<JsonObjectNode> ProcessAsync(JsonObjectNode args, object contextValue, CancellationToken cancellationToken = default)
    {
        if (args == null) return null;
        var result = await ProcessAsync(args.Deserialize<TIn>(), contextValue, cancellationToken);
        return JsonObjectNode.ConvertFrom(result);
    }

    /// <summary>
    /// Processes.
    /// </summary>
    /// <param name="args">The arguments.</param>
    /// <param name="contextValue">The context value.</param>
    /// <param name="cancellationToken">The optional cancellation token.</param>
    /// <returns>The result.</returns>
    public override async Task<string> ProcessAsync(string args, object contextValue, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(args)) return null;
        var result = await ProcessAsync(JsonSerializer.Deserialize<TIn>(args), contextValue, cancellationToken);
        return JsonSerializer.Serialize(result);
    }

    /// <summary>
    /// Creates operation description.
    /// </summary>
    /// <returns>The operation description.</returns>
    public override JsonOperationDescription CreateDescription()
    {
        var type = GetType();
        var method = type.GetMethod("ProcessAsync", [typeof(TIn), typeof(object), typeof(CancellationToken)]);
        var handler = SchemaHandler ?? JsonOperations.SchemaHandler;
        var d = JsonOperationDescription.Create(method, null, handler);
        if (d == null) return null;
        if (string.IsNullOrEmpty(d.Description)) d.Description = StringExtensions.GetDescription(type);
        var path = GetPathInfo() ?? JsonOperations.GetJsonDescriptionPath(method);
        if (path != null)
        {
            d.Data.SetValue(JsonOperations.PathProperty, path.Path);
            if (path.HttpMethod != null) d.Data.SetValue(JsonOperations.HttpMethodProperty, path.HttpMethod.Method);
        }
        else
        {
            d.Data.SetValue(JsonOperations.PathProperty, type.Name);
        }

        handler.OnCreate(method, d);
        return d;
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
    {
        if (result is JsonIntegerSchemaDescription i) return new JsonNumberSchemaDescription(i);
        return result;
    }

    /// <summary>
    /// Occurs on the JSON operation description is created.
    /// </summary>
    /// <param name="method">The method info.</param>
    /// <param name="description">The JSON operation description.</param>
    public virtual void OnCreate(MethodInfo method, JsonOperationDescription description)
    {
    }
}
