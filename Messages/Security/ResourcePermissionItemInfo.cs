using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Trivial.Data;
using Trivial.Reflection;
using Trivial.Tasks;
using Trivial.Text;
using Trivial.Web;
using Trivial.Users;
using System.ComponentModel.DataAnnotations.Schema;

namespace Trivial.Security;

/// <summary>
/// The permission set of a specific account associated to a resource.
/// </summary>
public class ResourcePermissionItemInfo<TOwner> : RelatedResourceEntityInfo<TOwner, BaseAccountEntityInfo>
    where TOwner : BaseResourceEntityInfo
{
    /// <summary>
    /// Initializes a new instance of the ResourcePermissionItemInfo class.
    /// </summary>
    public ResourcePermissionItemInfo()
    {
        Permissions = new();
    }

    /// <summary>
    /// Initializes a new instance of the ResourcePermissionItemInfo class.
    /// </summary>
    /// <param name="id">The entity identifer.</param>
    /// <param name="owner">The owner.</param>
    /// <param name="user">The target user, device or group to add permission.</param>
    /// <param name="permissions">The permission items.</param>
    /// <param name="creation">The creation date time.</param>
    public ResourcePermissionItemInfo(string id, TOwner owner, BaseAccountEntityInfo user, IEnumerable<string> permissions, DateTime? creation = null)
        : base(id, owner, user, creation)
    {
        Permissions = permissions?.ToList() ?? new();
    }

    /// <summary>
    /// Initializes a new instance of the ResourcePermissionItemInfo class.
    /// </summary>
    /// <param name="id">The entity identifer.</param>
    /// <param name="owner">The owner.</param>
    /// <param name="user">The target user, device or group to add permission.</param>
    /// <param name="permission">The permission item.</param>
    /// <param name="creation">The creation date time.</param>
    public ResourcePermissionItemInfo(string id, TOwner owner, BaseAccountEntityInfo user, string permission, DateTime? creation = null)
        : this(id, owner, user, new List<string>
        {
            permission
        }, creation)
    {
    }

    /// <summary>
    /// Initializes a new instance of the ResourcePermissionItemInfo class.
    /// </summary>
    /// <param name="id">The entity identifer.</param>
    /// <param name="owner">The owner.</param>
    /// <param name="user">The target user, device or group to add permission.</param>
    /// <param name="permissions">The permission items.</param>
    /// <param name="creation">The creation date time.</param>
    public ResourcePermissionItemInfo(Guid id, TOwner owner, BaseAccountEntityInfo user, IEnumerable<string> permissions, DateTime? creation = null)
        : base(id, owner, user, creation)
    {
        Permissions = permissions?.ToList() ?? new();
    }

    /// <summary>
    /// Initializes a new instance of the ResourcePermissionItemInfo class.
    /// </summary>
    /// <param name="id">The entity identifer.</param>
    /// <param name="owner">The owner.</param>
    /// <param name="user">The target user, device or group to add permission.</param>
    /// <param name="permission">The permission item.</param>
    /// <param name="creation">The creation date time.</param>
    public ResourcePermissionItemInfo(Guid id, TOwner owner, BaseAccountEntityInfo user, string permission, DateTime? creation = null)
        : this(id, owner, user, new List<string>
        {
            permission
        }, creation)
    {
    }

    /// <summary>
    /// Gets or sets the scope of permission items.
    /// </summary>
    [DataMember(Name = "permission")]
    [JsonInclude]
    [JsonPropertyName("permission")]
    [Description("The scope of permission items.")]
#if NETCOREAPP
    [Column("permission")]
#endif
    public List<string> Permissions
    {
        get => GetCurrentProperty<List<string>>();
        private set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the description of this permission settings.
    /// </summary>
    [DataMember(Name = "desc")]
    [JsonInclude]
    [JsonPropertyName("desc")]
    [Description("The description of this permission settings.")]
#if NETCOREAPP
    [Column("desc")]
#endif
    public string Description
    {
        get => GetCurrentProperty<string>();
        private set => SetCurrentProperty(value);
    }

    /// <inheritdoc />
    protected override string Supertype => "permission";

    /// <inheritdoc />
    protected override string ResourceType => "acl";

    /// <summary>
    /// Adds a permission item.
    /// </summary>
    /// <param name="item">The item to add.</param>
    public void AddPermision(string item)
    {
        Permissions ??= new();
        if (Permissions.Contains(item)) return;
        Permissions.Add(item);
    }

    /// <summary>
    /// Removes a permission item.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <returns>true if item is successfully removed; otherwise, false. This method also returns false if item was not found in the scope.</returns>
    public bool RemovePermission(string item)
    {
        if (Permissions == null) return false;
        return Permissions.Remove(item);
    }
}
