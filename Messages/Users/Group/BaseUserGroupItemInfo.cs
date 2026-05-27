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
/// The base entity of communication group, role, team and interest group.
/// </summary>
public class BaseUserGroupItemInfo : BaseAccountEntityInfo
{
    /// <summary>
    /// Initializes a new instance of the BaseUserGroupItemInfo class.
    /// </summary>
    public BaseUserGroupItemInfo()
        : base(AccountEntityTypes.Group)
    {
    }

    /// <summary>
    /// Initializes a new instance of the BaseUserGroupItemInfo class.
    /// </summary>
    /// <param name="args">The initialization arguments.</param>
    /// <param name="nickname">The nickname or display name.</param>
    /// <param name="avatar">The avatar URI.</param>
    public BaseUserGroupItemInfo(ResourceEntityArgs args, string nickname = null, Uri avatar = null)
        : base(AccountEntityTypes.Group, args, nickname, avatar)
    {
    }

    /// <summary>
    /// Initializes a new instance of the BaseUserGroupItemInfo class.
    /// </summary>
    /// <param name="args">The initialization arguments.</param>
    public BaseUserGroupItemInfo(AccountEntityArgs args)
        : base(AccountEntityTypes.Group, args)
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
        : base(AccountEntityTypes.Group, id, nickname, avatar, creation)
    {
    }

    /// <summary>
    /// Gets or sets the membership policy about how a user or bot joins into this user group.
    /// </summary>
    [DataMember(Name = "memberPolicy")]
    [JsonPropertyName("memberPolicy")]
    [Description("The default policy about the group.")]
#if NETCOREAPP
    [Column("memberpolicy")]
#endif
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
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("The optional email address of the group.")]
#if NETCOREAPP
    [Column("email")]
#endif
    public string Email
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value?.Trim());
    }

    /// <inheritdoc />
    protected override void ToString(StringBuilder sb)
    {
        if (string.IsNullOrWhiteSpace(Email)) return;
        sb.AppendLine();
        sb.Append("Email = ");
        sb.Append(Email);
    }
}
