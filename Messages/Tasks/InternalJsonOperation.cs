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
    public InternalJsonOperation(Func<TIn, object, CancellationToken, Task<TOut>> handler)
    {
        Handler = handler;
    }

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
    public InternalSimpleJsonOperation(Func<TIn, CancellationToken, Task<TOut>> handler)
    {
        Handler = handler;
    }

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
    public InternalSyncJsonOperation(Func<TIn, object, TOut> handler)
    {
        Handler = handler;
    }

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
    public InternalSimpleSyncJsonOperation(Func<TIn, TOut> handler)
    {
        Handler = handler;
    }

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
}
