using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Trivial.Data;
using Trivial.Reflection;
using Trivial.Text;

namespace Trivial.Tasks;

/// <summary>
/// The internal JSON operation.
/// </summary>
internal class InternalJsonOperation<TIn, TOut> : BaseJsonOperation<TIn, TOut>
{
    /// <summary>
    /// Initializes a new instance of the InternalJsonOperation class.
    /// </summary>
    /// <param name="handler">The processing handler.</param>
    /// <param name="id">The operation identifier.</param>
    public InternalJsonOperation(Func<TIn, object, CancellationToken, Task<TOut>> handler, string id = null)
    {
        Id = id;
        Handler = handler;
    }

    /// <summary>
    /// Gets or sets the operation identifier.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Gets the processing handler.
    /// </summary>
    public Func<TIn, object, CancellationToken, Task<TOut>> Handler { get; }

    /// <inheritdoc />
    public override Task<TOut> ProcessAsync(TIn args, object contextValue, CancellationToken cancellationToken = default)
    {
        if (Handler == null) throw new InvalidOperationException("The handler was null.");
        return Handler(args, contextValue, cancellationToken);
    }

    /// <inheritdoc />
    public override JsonOperationDescription CreateDescription(IJsonNodeSchemaCreationHandler<Type> handler)
    {
        var desc = base.CreateDescription(handler);
        if (!string.IsNullOrEmpty(Id)) desc.Id = Id;
        return desc;
    }
}

/// <summary>
/// The internal JSON operation.
/// </summary>
internal class InternalSimpleJsonOperation<TIn, TOut> : BaseJsonOperation<TIn, TOut>
{
    /// <summary>
    /// Initializes a new instance of the InternalSimpleJsonOperation class.
    /// </summary>
    /// <param name="handler">The processing handler.</param>
    /// <param name="id">The operation identifier.</param>
    public InternalSimpleJsonOperation(Func<TIn, CancellationToken, Task<TOut>> handler, string id = null)
    {
        Id = id;
        Handler = handler;
    }

    /// <summary>
    /// Gets or sets the operation identifier.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Gets the processing handler.
    /// </summary>
    public Func<TIn, CancellationToken, Task<TOut>> Handler { get; }

    /// <inheritdoc />
    public override Task<TOut> ProcessAsync(TIn args, object contextValue, CancellationToken cancellationToken = default)
    {
        if (Handler == null) throw new InvalidOperationException("The handler was null.");
        return Handler(args, cancellationToken);
    }

    /// <inheritdoc />
    public override JsonOperationDescription CreateDescription(IJsonNodeSchemaCreationHandler<Type> handler)
    {
        var desc = base.CreateDescription(handler);
        if (!string.IsNullOrEmpty(Id)) desc.Id = Id;
        return desc;
    }
}

/// <summary>
/// The base JSON operation.
/// </summary>
internal class InternalSyncJsonOperation<TIn, TOut> : BaseJsonOperation<TIn, TOut>
{
    /// <summary>
    /// Initializes a new instance of the InternalSyncJsonOperation class.
    /// </summary>
    /// <param name="handler">The processing handler.</param>
    /// <param name="id">The operation identifier.</param>
    public InternalSyncJsonOperation(Func<TIn, object, TOut> handler, string id = null)
    {
        Id = id;
        Handler = handler;
    }

    /// <summary>
    /// Gets or sets the operation identifier.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Gets the processing handler.
    /// </summary>
    public Func<TIn, object, TOut> Handler { get; }

    /// <inheritdoc />
    public override Task<TOut> ProcessAsync(TIn args, object contextValue, CancellationToken cancellationToken = default)
    {
        if (Handler == null) throw new InvalidOperationException("The handler was null.");
        return Task.FromResult(Handler(args, contextValue));
    }

    /// <inheritdoc />
    public override JsonOperationDescription CreateDescription(IJsonNodeSchemaCreationHandler<Type> handler)
    {
        var desc = base.CreateDescription(handler);
        if (!string.IsNullOrEmpty(Id)) desc.Id = Id;
        return desc;
    }
}

/// <summary>
/// The base JSON operation.
/// </summary>
internal class InternalSimpleSyncJsonOperation<TIn, TOut> : BaseJsonOperation<TIn, TOut>
{
    /// <summary>
    /// Initializes a new instance of the InternalSimpleSyncJsonOperation class.
    /// </summary>
    /// <param name="handler">The processing handler.</param>
    /// <param name="id">The operation identifier.</param>
    public InternalSimpleSyncJsonOperation(Func<TIn, TOut> handler, string id = null)
    {
        Id = id;
        Handler = handler;
    }

    /// <summary>
    /// Gets or sets the operation identifier.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Gets the processing handler.
    /// </summary>
    public Func<TIn, TOut> Handler { get; }

    /// <inheritdoc />
    public override Task<TOut> ProcessAsync(TIn args, object contextValue, CancellationToken cancellationToken = default)
    {
        if (Handler == null) throw new InvalidOperationException("The handler was null.");
        return Task.FromResult(Handler(args));
    }

    /// <inheritdoc />
    public override JsonOperationDescription CreateDescription(IJsonNodeSchemaCreationHandler<Type> handler)
    {
        var desc = base.CreateDescription(handler);
        if (!string.IsNullOrEmpty(Id)) desc.Id = Id;
        return desc;
    }
}

internal class InternalMethodJsonOperation : BaseJsonOperation, IJsonTypeOperationDescriptive
{
    private readonly Func<object, object, CancellationToken, Task<object>> handler;
    private readonly MethodInfo methodInfo;

    /// <summary>
    /// Gets a value indicating whether the operation is valid.
    /// </summary>
    internal bool IsValid => handler != null;

    /// <summary>
    /// Gets or sets the schema description creation handler.
    /// </summary>
    public IJsonNodeSchemaCreationHandler<Type> SchemaHandler { get; set; }

    /// <summary>
    /// Gets or sets the operation identifier.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Gets the argument type.
    /// </summary>
    public Type ArgumentType { get; private set; }

    /// <summary>
    /// Initializes a new instance of the InternalMethodJsonOperation class.
    /// </summary>
    /// <param name="target">The target object.</param>
    /// <param name="method">The method info.</param>
    /// <param name="id">The operation identifier.</param>
    public InternalMethodJsonOperation(object target, MethodInfo method, string id)
    {
        Id = id;
        if (method == null) return;
        methodInfo = method;
        var parameters = method.GetParameters();
        if (parameters.Length < 1) return;
        ArgumentType = parameters[0]?.ParameterType;
        switch (parameters.Length)
        {
            case 1:
                handler = (args, contextValue, cancellationToken) =>
                {
                    var task = method.Invoke(target, new object[] { args }) as Task;
                    return JsonOperations.TryGetTaskResult(task);
                };
                break;
            case 2:
                var secParam = parameters[1];
                if (secParam == null) break;
                if (secParam.ParameterType == typeof(object))
                    handler = (args, contextValue, cancellationToken) =>
                    {
                        var task = method.Invoke(target, new object[] { args, contextValue }) as Task;
                        return JsonOperations.TryGetTaskResult(task);
                    };
                else if (secParam.ParameterType == typeof(CancellationToken))
                    handler = (args, contextValue, cancellationToken) =>
                    {
                        var task = method.Invoke(target, new object[] { args, cancellationToken }) as Task;
                        return JsonOperations.TryGetTaskResult(task);
                    };
                break;
            case 3:
                handler = (args, contextValue, cancellationToken) =>
                {
                    var task = method.Invoke(target, new object[] { args, contextValue, cancellationToken }) as Task;
                    return JsonOperations.TryGetTaskResult(task);
                };
                break;
        }
    }

    /// <inheritdoc />
    public override async Task<JsonObjectNode> ProcessAsync(JsonObjectNode arguments, object contextValue, CancellationToken cancellationToken = default)
    {
        if (arguments == null) return null;
        var parameter = JsonOperations.DeserializeArguments(arguments.ToString(), ArgumentType);
        var result = await handler(parameter, contextValue, cancellationToken);
        return JsonOperations.ToResultJson(result);
    }

    /// <inheritdoc />
    public override async Task<string> ProcessAsync(string arguments, object contextValue, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(arguments)) return null;
        var parameter = JsonOperations.DeserializeArguments(arguments, ArgumentType);
        var result = await handler(parameter, contextValue, cancellationToken);
        return JsonOperations.SerializeResult(result);
    }

    /// <inheritdoc />
    public override JsonOperationDescription CreateDescription()
        => CreateDescription(null);

    /// <summary>
    /// Creates operation description.
    /// </summary>
    /// <returns>The operation description.</returns>
    public JsonOperationDescription CreateDescription(IJsonNodeSchemaCreationHandler<Type> handler)
    {
        var desc = JsonOperationDescription.Create(methodInfo, Id, handler ?? SchemaHandler ?? JsonOperations.SchemaHandler);
        JsonOperations.UpdatePath(desc, JsonOperations.GetJsonDescriptionPath(methodInfo), methodInfo.ReflectedType);
        return desc;
    }
}

internal class InternalPropertyJsonOperation : BaseJsonOperation, IJsonTypeOperationDescriptive
{
    private readonly PropertyInfo propertyInfo;
    private readonly object target;
    private BaseJsonOperation operation;

    /// <summary>
    /// Gets a value indicating whether the operation is valid.
    /// </summary>
    internal bool IsValid => operation != null;

    /// <summary>
    /// Gets or sets the operation identifier.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Initializes a new instance of the InternalMethodJsonOperation class.
    /// </summary>
    /// <param name="target">The target object.</param>
    /// <param name="property">The property info.</param>
    /// <param name="id">The operation identifier.</param>
    public InternalPropertyJsonOperation(object target, PropertyInfo property, string id)
    {
        Id = id;
        if (property == null || !ObjectConvert.TryGetProperty(target, property, out BaseJsonOperation operation)) return;
        this.operation = operation;
        this.target = target;
        propertyInfo = property;
    }

    /// <inheritdoc />
    public override Task<JsonObjectNode> ProcessAsync(JsonObjectNode args, object contextValue, CancellationToken cancellationToken = default)
        => operation.ProcessAsync(args, contextValue, cancellationToken);

    /// <inheritdoc />
    public override Task<string> ProcessAsync(string args, object contextValue, CancellationToken cancellationToken = default)
        => operation.ProcessAsync(args, contextValue, cancellationToken);

    /// <inheritdoc />
    public override JsonOperationDescription CreateDescription()
        => CreateDescription(null);

    /// <inheritdoc />
    public JsonOperationDescription CreateDescription(IJsonNodeSchemaCreationHandler<Type> schemaHandler)
    {
        var desc = JsonOperationDescription.CreateFromProperty(target, propertyInfo, h =>
        {
            return h is IJsonTypeOperationDescriptive tod ? tod.CreateDescription(schemaHandler) : h.CreateDescription();
        }, Id);
        if (string.IsNullOrWhiteSpace(desc?.Id)) return null;
        JsonOperations.UpdatePath(desc, JsonOperations.GetJsonDescriptionPath(propertyInfo), propertyInfo.ReflectedType);
        return desc;
    }
}

internal class JsonNodeSchemaDescriptionMappingItem
{
    public JsonNodeSchemaDescriptionMappingItem(string id, JsonNodeSchemaDescription value)
    {
        Id = id;
        Value = value;
    }

    public string Id { get; set; }

    public JsonNodeSchemaDescription Value { get; set; }
}
