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
    /// <param name="id">The resource identifier.</param>
    /// <param name="nickname">The nickname or display name.</param>
    /// <param name="avatar">The avatar URI.</param>
    /// <param name="creation">The creation date time.</param>
    public BaseUserGroupItemInfo(string id, string nickname, Uri avatar = null, DateTime? creation = null)
        : base(PrincipalEntityTypes.Group, id, nickname, avatar, creation)
    {
    }

    /// <summary>
    /// Initializes a new instance of the BaseUserGroupItemInfo class.
    /// </summary>
    /// <param name="json">The JSON object to parse.</param>
    protected internal BaseUserGroupItemInfo(JsonObjectNode json)
        : base(PrincipalEntityTypes.Group, json)
    {
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
    /// Gets or sets the optional email address.
    /// </summary>
    [DataMember(Name = "email")]
    [JsonPropertyName("email")]
    [Description("The optional email address.")]
    public string Email
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value?.Trim());
    }

    /// <inheritdoc />
    protected override void Fill(JsonObjectNode json)
    {
        base.Fill(json);
        DefaultMembershipPolicy = json.TryGetEnumValue<UserGroupMembershipPolicies>("memberPolicy") ?? UserGroupMembershipPolicies.Forbidden;
        Email = json.TryGetStringTrimmedValue("email");
    }

    /// <inheritdoc />
    protected override void ToString(StringBuilder sb)
    {
        if (string.IsNullOrWhiteSpace(Email)) return;
        sb.AppendLine();
        sb.Append("Email = ");
        sb.Append(Email);
    }

    /// <summary>
    /// Converts to JSON object.
    /// </summary>
    /// <returns>A JSON object.</returns>
    public override JsonObjectNode ToJson()
    {
        var json = base.ToJson();
        if (DefaultMembershipPolicy != 0) json.SetValue("memberPolicy", DefaultMembershipPolicy.ToString());
        json.SetValue("email", Email);
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
