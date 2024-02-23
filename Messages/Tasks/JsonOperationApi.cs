using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trivial.Tasks;

/// <summary>
/// The API with JSON operation set.
/// </summary>
public class JsonOperationApi
{
    private readonly Dictionary<string, object> dict = new();
    private readonly object locker = new();

    /// <summary>
    /// Gets a value indicating whether the operation configuration has initialized.
    /// </summary>
    public bool HasInited { get; private set; }

    /// <summary>
    /// Gets the additional operations.
    /// </summary>
    public List<BaseJsonOperation> AdditionalOperations { get; } = new();

    /// <summary>
    /// Registers.
    /// </summary>
    public void Register(BaseRouteJsonOperation operation)
    {
        if (string.IsNullOrWhiteSpace(operation?.Id)) return;
        dict[operation.Id] = operation;
    }

    /// <summary>
    /// Initializes to register all operations.
    /// </summary>
    public void Init()
    {
        if (HasInited) return;
        lock (locker)
        {
            if (HasInited) return;
            HasInited = true;
        }
    }
}
