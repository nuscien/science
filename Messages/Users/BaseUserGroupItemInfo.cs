using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
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

namespace Trivial.Users;

/// <summary>
/// The user group item information.
/// </summary>
[JsonConverter(typeof(BaseUserGroupItemInfoConverter))]
public class BaseUserGroupItemInfo : BasePrincipalEntityInfo
{
    /// <summary>
    /// Initializes a new instance of the BaseUserGroupItemInfo class.
    /// </summary>
    public BaseUserGroupItemInfo()
        : base(PrincipalEntityTypes.Group)
    {
    }

    /// <summary>
    /// Initializes a new instance of the BaseUserGroupItemInfo class.
    /// </summary>
    public BaseUserGroupItemInfo(string id, string nickname, Uri avatar = null)
        : base(PrincipalEntityTypes.Group, id, nickname, avatar)
    {
    }

    /// <summary>
    /// Initializes a new instance of the BaseUserGroupItemInfo class.
    /// </summary>
    /// <param name="json">The JSON object to parse.</param>
    public BaseUserGroupItemInfo(JsonObjectNode json)
        : base(PrincipalEntityTypes.Group, json)
    {
        if (json == null) return;
        DefaultMembershipPolicy = json.TryGetEnumValue<UserGroupMembershipPolicies>("memberPolicy") ?? UserGroupMembershipPolicies.Forbidden;
    }

    /// <summary>
    /// Gets or sets the membership policy about how a user or bot joins into this user group.
    /// </summary>
    [DataMember(Name = "memberPolicy")]
    [JsonPropertyName("memberPolicy")]
    [Description("The default policy about the group.")]
    public UserGroupMembershipPolicies DefaultMembershipPolicy
    {
        get => GetCurrentProperty<UserGroupMembershipPolicies>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Converts to JSON object.
    /// </summary>
    /// <returns>A JSON object.</returns>
    public override JsonObjectNode ToJson()
    {
        var json = base.ToJson();
        if (DefaultMembershipPolicy != 0) json.SetValue("memberPolicy", DefaultMembershipPolicy.ToString());
        return json;
    }

    /// <summary>
    /// Converts the JSON raw back.
    /// </summary>
    /// <param name="value">The source value.</param>
    /// <returns>The request instance.</returns>
    public static implicit operator BaseUserGroupItemInfo(JsonObjectNode value)
        => value is null ? null : new(value);
}

/// <summary>
/// JSON value node converter.
/// </summary>
internal sealed class BaseUserGroupItemInfoConverter : JsonObjectHostConverter<BaseUserGroupItemInfo>
{
    /// <inheritdoc />
    protected override BaseUserGroupItemInfo Create(JsonObjectNode json)
        => new(json);
}
