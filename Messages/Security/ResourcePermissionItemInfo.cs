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
/// The resource permission settings item information.
/// </summary>
[JsonConverter(typeof(ResourcePermissionItemInfoConverter))]
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
    /// Initializes a new instance of the ResourcePermissionItemInfo class.
    /// </summary>
    /// <param name="json">The JSON object to parse.</param>
    protected internal ResourcePermissionItemInfo(JsonObjectNode json)
        : base(json)
    {
    }

    /// <summary>
    /// Gets or sets the scope of permission items.
    /// </summary>
    [DataMember(Name = "permission")]
    [JsonPropertyName("permission")]
    [Description("The scope of permission items.")]
    public List<string> Permissions
    {
        get => GetCurrentProperty<List<string>>();
        private set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the description of this permission settings.
    /// </summary>
    [DataMember(Name = "desc")]
    [JsonPropertyName("desc")]
    [Description("The description of this permission settings.")]
    public string Description
    {
        get => GetCurrentProperty<string>();
        private set => SetCurrentProperty(value);
    }

    /// <inheritdoc />
    [JsonIgnore]
#if NETCOREAPP
    [NotMapped]
#endif
    protected override string Supertype => "permission";

    /// <inheritdoc />
    [JsonIgnore]
#if NETCOREAPP
    [NotMapped]
#endif
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

    /// <inheritdoc />
    protected override void Fill(JsonObjectNode json)
    {
        base.Fill(json);
        Permissions = json.TryGetStringListValue("permission", true) ?? new();
        Description = json.TryGetStringValue("desc");
    }

    /// <summary>
    /// Converts to JSON object.
    /// </summary>
    /// <returns>A JSON object.</returns>
    public override JsonObjectNode ToJson()
    {
        var json = base.ToJson();
        json.SetValue("permission", Permissions);
        json.SetValueIfNotEmpty("desc", Description);
        return json;
    }

    /// <summary>
    /// Converts the JSON raw back.
    /// </summary>
    /// <param name="value">The source value.</param>
    /// <returns>The request instance.</returns>
    public static implicit operator ResourcePermissionItemInfo<TOwner>(JsonObjectNode value)
        => value is null ? null : new(value);

    /// <summary>
    /// Converts to JSON object.
    /// </summary>
    /// <param name="value">The JSON value.</param>
    /// <returns>A JSON object.</returns>
    public static explicit operator JsonObjectNode(ResourcePermissionItemInfo<TOwner> value)
        => value?.ToJson();
}

/// <summary>
/// JSON value node converter.
/// </summary>
internal sealed class ResourcePermissionItemInfoConverter<TOwner> : JsonObjectHostConverter<ResourcePermissionItemInfo<TOwner>>
    where TOwner : BaseResourceEntityInfo
{
    /// <inheritdoc />
    protected override ResourcePermissionItemInfo<TOwner> Create(JsonObjectNode json)
        => new(json);
}

/// <summary>
/// JSON value node converter.
/// </summary>
internal sealed class ResourcePermissionItemInfoConverter : JsonConverterFactory
{
    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType) return false;
        var generic = typeToConvert.GetGenericTypeDefinition();
        return generic == typeof(ResourcePermissionItemInfo<>);
    }

    /// <inheritdoc />
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        if (!typeToConvert.IsGenericType) return null;
        var type = typeToConvert.GetGenericArguments().FirstOrDefault();
        if (type == null) return null;
        type = typeof(ResourcePermissionItemInfoConverter<>).MakeGenericType(new[] { type });
        return (JsonConverter)Activator.CreateInstance(type);
    }
}
