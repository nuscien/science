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
    public override JsonOperationDescription CreateDescription()
    {
        var desc = base.CreateDescription();
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
    public override JsonOperationDescription CreateDescription()
    {
        var desc = base.CreateDescription();
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
    public override JsonOperationDescription CreateDescription()
    {
        var desc = base.CreateDescription();
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
    public override JsonOperationDescription CreateDescription()
    {
        var desc = base.CreateDescription();
        if (!string.IsNullOrEmpty(Id)) desc.Id = Id;
        return desc;
    }
}

internal class InternalMethodJsonOperation : BaseJsonOperation
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
    public BaseJsonOperationSchemaHandler SchemaHandler { get; set; }

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
    public override async Task<JsonObjectNode> ProcessAsync(JsonObjectNode args, object contextValue, CancellationToken cancellationToken = default)
    {
        if (args == null) return null;
        var parameter = JsonSerializer.Deserialize(args.ToString(), ArgumentType);
        var result = await handler(parameter, contextValue, cancellationToken);
        return JsonObjectNode.ConvertFrom(result);
    }

    /// <inheritdoc />
    public override async Task<string> ProcessAsync(string args, object contextValue, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(args)) return null;
        var parameter = JsonSerializer.Deserialize(args, ArgumentType);
        var result = await handler(parameter, contextValue, cancellationToken);
        return JsonSerializer.Serialize(result);
    }

    /// <inheritdoc />
    public override JsonOperationDescription CreateDescription()
    {
        var desc = JsonOperationDescription.Create(methodInfo, Id, SchemaHandler);
        JsonOperations.UpdatePath(desc, JsonOperations.GetJsonDescriptionPath(methodInfo), methodInfo.ReflectedType);
        return desc;
    }
}

internal class JsonNodeSchemaDescriptionCollection : List<(JsonNodeSchemaDescription, string)>
{
    public string GetId(JsonNodeSchemaDescription value, string id, JsonObjectNode schemas)
    {
        foreach (var item in this)
        {
            if (ReferenceEquals(item.Item1, value)) return item.Item2; 
        }

        Add((value, id));
        schemas.SetValue(id, value.ToJson());
        return id;
    }
}
