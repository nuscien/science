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

internal class InternalMethodJsonOperation<TIn, TOut> : BaseJsonOperation<TIn, TOut>
{
    private Func<TIn, object, CancellationToken, Task<TOut>> handler;
    private MethodInfo methodInfo;

    /// <summary>
    /// Gets a value indicating whether the operation is valid.
    /// </summary>
    internal bool IsValid => handler != null;

    /// <summary>
    /// Gets or sets the operation identifier.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Initializes a new instance of the InternalMethodJsonOperation class.
    /// </summary>
    /// <param name="target">The target object.</param>
    /// <param name="method">The method info.</param>
    /// <param name="id">The operation identifier.</param>
    public InternalMethodJsonOperation(object target, MethodInfo method, string id)
    {
        Id = id;
        if (method == null || method.ReturnType != typeof(Task<TOut>)) return;
        methodInfo = method;
        var parameters = method.GetParameters();
        switch (parameters.Length)
        {
            case 0:
                handler = (args, contextValue, cancellationToken) =>
                {
                    var result = method.Invoke(target, null) as Task<TOut>;
                    return result;
                };
                break;
            case 1:
                handler = (args, contextValue, cancellationToken) =>
                {
                    var result = method.Invoke(target, new object[] { args }) as Task<TOut>;
                    return result;
                };
                break;
            case 2:
                var secParam = parameters[1];
                if (secParam == null) break;
                if (secParam.ParameterType == typeof(object))
                    handler = (args, contextValue, cancellationToken) =>
                    {
                        var result = method.Invoke(target, new object[] { args, contextValue }) as Task<TOut>;
                        return result;
                    };
                else if (secParam.ParameterType == typeof(CancellationToken))
                    handler = (args, contextValue, cancellationToken) =>
                    {
                        var result = method.Invoke(target, new object[] { args, cancellationToken }) as Task<TOut>;
                        return result;
                    };
                break;
            case 3:
                handler = (args, contextValue, cancellationToken) =>
                {
                    var result = method.Invoke(target, new object[] { args, contextValue, cancellationToken }) as Task<TOut>;
                    return result;
                };
                break;
        }
    }

    /// <inheritdoc />
    public override Task<TOut> ProcessAsync(TIn args, object contextValue, CancellationToken cancellationToken = default)
    {
        if (handler == null) throw new InvalidOperationException("The handler was null.");
        return handler(args, contextValue, cancellationToken);
    }

    /// <inheritdoc />
    public override JsonOperationDescription CreateDescription()
    {
        var desc = JsonOperationDescription.Create(methodInfo, Id);
        JsonOperations.UpdatePath(desc, JsonOperations.GetJsonDescriptionPath(methodInfo), methodInfo.ReflectedType);
        return desc;
    }
}
