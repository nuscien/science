using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Trivial.Users;

/// <summary>
/// The base principal of accoutn entity.
/// </summary>
/// <param name="entity">The account entity.</param>
/// <param name="roles">The role collection.</param>
/// <param name="options">The options.</param>
public abstract class BaseAccountEntityPrincipal<TUser>(TUser entity, IEnumerable<string> roles, BaseAccountEntityPrincipalOptions options) : IPrincipal
    where TUser : BaseUserItemInfo
{
    /// <summary>
    /// Gets the account information.
    /// </summary>
    public TUser Account { get; } = entity;

    /// <summary>
    /// Gets the collection of the role.
    /// </summary>
    public IReadOnlyList<string> Roles { get; } = (roles ?? new List<string>()).ToList().AsReadOnly();

    /// <summary>
    /// Gets the identity of the current principal.
    /// </summary>
    public IIdentity Identity { get; } = entity == null ? null : new ClaimsIdentity(entity.ToClaims(options?.Issuer), options?.AuthenticationType);

    /// <summary>
    /// Determines whether the current principal belongs to the specified role.
    /// </summary>
    /// <param name="role">The name of the role for which to check membership.</param>
    /// <returns>true if the current principal is a member of the specified role; otherwise, false.</returns>
    public bool IsInRole(string role)
        => !string.IsNullOrWhiteSpace(role) && Roles.Contains(role);
}

/// <summary>
/// The base principal of accoutn entity.
/// </summary>
/// <param name="entity">The account entity.</param>
/// <param name="roles">The role collection.</param>
/// <param name="options">The options.</param>
public abstract class BaseAccountEntityPrincipal<TUser, TGroup>(TUser entity, IEnumerable<TGroup> roles, BaseAccountEntityPrincipalOptions options) : IPrincipal
    where TUser : BaseUserItemInfo
    where TGroup : BaseUserGroupItemInfo
{
    /// <summary>
    /// Gets the account information.
    /// </summary>
    public TUser Account { get; } = entity;

    /// <summary>
    /// Gets the collection of the role.
    /// </summary>
    public IReadOnlyList<TGroup> Roles { get; } = (roles ?? new List<TGroup>()).ToList().AsReadOnly();

    /// <summary>
    /// Gets the identity of the current principal.
    /// </summary>
    public IIdentity Identity { get; } = entity == null ? null : new ClaimsIdentity(entity.ToClaims(options?.Issuer), options?.AuthenticationType);

    /// <summary>
    /// Determines whether the current principal belongs to the specified role.
    /// </summary>
    /// <param name="role">The name of the role for which to check membership.</param>
    /// <returns>true if the current principal is a member of the specified role; otherwise, false.</returns>
    public bool IsInRole(string role)
        => !string.IsNullOrWhiteSpace(role) && (Roles.Select(ele => ele.Nickname).Contains(role) || Roles.Select(ele => ele.Id).Contains(role));
}

/// <summary>
/// The options of account entity principal.
/// </summary>
public class BaseAccountEntityPrincipalOptions
{
    /// <summary>
    /// Gets or sets the type of authentication used.
    /// </summary>
    public string AuthenticationType { get; set; }

    /// <summary>
    /// Gets or sets the claim issuer.
    /// </summary>
    public string Issuer { get; set; }
}