using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Trivial.Data;
using Trivial.Reflection;
using Trivial.Text;

namespace Trivial.Data;

/// <summary>
/// The entity states.
/// </summary>
public enum ResourceEntityStates : byte
{
    /// <summary>
    /// The entity does not exist or is removed.
    /// </summary>
    Deleted = 0,

    /// <summary>
    /// The entity is in trash bin.
    /// </summary>
    Recycle = 1,

    /// <summary>
    /// This is a placeholder.
    /// </summary>
    Placehoder = 2,

    /// <summary>
    /// The entity is in a progress to create or is initializing.
    /// </summary>
    Progress = 3,

    /// <summary>
    /// The entity is applying for to approval.
    /// </summary>
    Request = 4,

    /// <summary>
    /// The entity is a draft.
    /// </summary>
    Draft = 5,

    /// <summary>
    /// The entity is pending to publish.
    /// </summary>
    Publishing = 6,

    /// <summary>
    /// The entity is in service.
    /// </summary>
    Normal = 7
}

/// <summary>
/// The entity states.
/// </summary>
public enum ResourceEntityOrders : byte
{
    /// <summary>
    /// The default order.
    /// </summary>
    Default = 0,

    /// <summary>
    /// Order by last modification time descending (new-old).
    /// </summary>
    Latest = 1,

    /// <summary>
    /// Order by last modification time ascending (old-new).
    /// </summary>
    Time = 2,

    /// <summary>
    /// Order by name ascending (a-z).
    /// </summary>
    Name = 3,

    /// <summary>
    /// Order by name descending (z-a).
    /// </summary>
    Z2A = 4
}

/// <summary>
/// The state of saving resource entity.
/// </summary>
public enum ResourceEntitySavingStates : byte
{
    /// <summary>
    /// Available to save, or unknown state.
    /// </summary>
    Ready = 0,

    /// <summary>
    /// Offline initialization before saving.
    /// </summary>
    Local = 1,

    /// <summary>
    /// Working on saving.
    /// </summary>
    Saving = 2,

    /// <summary>
    /// Save failed.
    /// </summary>
    Failure = 3,

    /// <summary>
    /// Disable to save any more.
    /// </summary>
    Disabled = 6,

    /// <summary>
    /// Unknown state.
    /// </summary>
    Unknown = 7,
}