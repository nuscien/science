using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Trivial.Data;
using Trivial.Devices;
using Trivial.Net;
using Trivial.Reflection;
using Trivial.Security;
using Trivial.Tasks;
using Trivial.Text;

namespace Trivial.Users;

/// <summary>
/// Unknown account entity information.
/// </summary>
internal sealed class UnknownAccountEntityInfo : BaseAccountEntityInfo
{
    /// <summary>
    /// Initializes a new instance of the UnknownAccountEntityInfo class.
    /// </summary>
    public UnknownAccountEntityInfo()
        : base(AccountEntityTypes.Unknown)
    {
        IsUnknownType = true;
    }

    /// <summary>
    /// Initializes a new instance of the UnknownAccountEntityInfo class.
    /// </summary>
    /// <param name="id">The resource identifier.</param>
    /// <param name="creation">The creation date time.</param>
    public UnknownAccountEntityInfo(string id, DateTime? creation = null)
        : base(AccountEntityTypes.Unknown, creation)
    {
        Id = id;
        IsUnknownType = true;
    }

    /// <summary>
    /// Initializes a new instance of the UnknownAccountEntityInfo class.
    /// </summary>
    /// <param name="id">The resource identifier.</param>
    /// <param name="nickname">The nickname or display name.</param>
    /// <param name="creation">The creation date time.</param>
    /// <param name="modification">The last modification date time.</param>
    /// <param name="avatar">The avatar URI.</param>
    public UnknownAccountEntityInfo(string id, string nickname, DateTime creation, DateTime modification, Uri avatar = null)
        : base(AccountEntityTypes.Unknown, id, nickname, avatar, creation)
    {
    }

    /// <summary>
    /// Gets a value indicating whether the account entity type is unknown.
    /// </summary>
    [JsonIgnore]
#if NETCOREAPP
    [NotMapped]
#endif
    public bool IsUnknownType { get; }

    /// <summary>
    /// The other fields deserialized from JSON.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement> ExtensionData { get; set; }
}

/// <summary>
/// Unknown account entity information.
/// </summary>
public class ApplicationCustomizedAccountEntityInfo : BaseAccountEntityInfo
{
    /// <summary>
    /// Initializes a new instance of the ApplicationCustomizedAccountEntityInfo class.
    /// </summary>
    internal protected ApplicationCustomizedAccountEntityInfo()
        : base(AccountEntityTypes.Customized)
    {
    }

    /// <summary>
    /// Initializes a new instance of the ApplicationCustomizedAccountEntityInfo class.
    /// </summary>
    /// <param name="args">The initialization arguments.</param>
    protected ApplicationCustomizedAccountEntityInfo(AccountEntityArgs args)
        : base(AccountEntityTypes.Customized, args)
    {
    }

    /// <summary>
    /// Initializes a new instance of the ApplicationCustomizedAccountEntityInfo class.
    /// </summary>
    /// <param name="id">The resource identifier.</param>
    /// <param name="nickname">The nickname or display name.</param>
    /// <param name="avatar">The avatar URI.</param>
    /// <param name="creation">The creation date time.</param>
    protected ApplicationCustomizedAccountEntityInfo(string id, string nickname, Uri avatar = null, DateTime? creation = null)
        : base(AccountEntityTypes.Customized, id, nickname, avatar, creation)
    {
    }

    /// <summary>
    /// Initializes a new instance of the ApplicationCustomizedAccountEntityInfo class.
    /// </summary>
    /// <param name="id">The resource identifier.</param>
    /// <param name="nickname">The nickname or display name.</param>
    /// <param name="creation">The creation date time.</param>
    /// <param name="modification">The last modification date time.</param>
    /// <param name="avatar">The avatar URI.</param>
    protected ApplicationCustomizedAccountEntityInfo(string id, string nickname, DateTime creation, DateTime modification, Uri avatar = null)
        : base(AccountEntityTypes.Customized, id, nickname, avatar, creation)
    {
    }

    /// <summary>
    /// Gets the sub-type of the resource.
    /// </summary>
    [JsonPropertyName("subtype")]
#if NETCOREAPP
    [NotMapped]
#endif
    public string Subtype { get; protected set; }
}
