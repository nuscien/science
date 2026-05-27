using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Trivial.AI;
using Trivial.Data;
using Trivial.Devices;
using Trivial.Security;
using Trivial.Tasks;
using Trivial.Text;

namespace Trivial.Users;

/// <summary>
/// The resources of users, groups, devices and other pincipals.
/// </summary>
public interface IAccountEntityResources
{
    /// <summary>
    /// Gets or sets the users, groups, devices and other pincipals.
    /// </summary>
    IList<BaseAccountEntityInfo> Accounts { get; }
}

/// <summary>
/// The resolver of account entity.
/// </summary>
public interface IAccountEntityResolver
{
    /// <summary>
    /// Gets the user item entity by identifier.
    /// </summary>
    /// <param name="id">The identifier of the user item.</param>
    /// <returns>An instance of user item entity.</returns>
    Task<UserItemInfo> GetUserAsync(string id);

    /// <summary>
    /// Gets the  entity by identifier.
    /// </summary>
    /// <param name="id">The identifier of the .</param>
    /// <returns>An instance of  entity.</returns>
    Task<UserItemInfo> GetAuthDeviceAsync(string id);

    /// <summary>
    /// Gets the service account entity by identifier.
    /// </summary>
    /// <param name="id">The identifier of the service account.</param>
    /// <returns>An instance of service account entity.</returns>
    Task<UserItemInfo> GetServiceAccountAsync(string id);

    /// <summary>
    /// Gets the bot entity by identifier.
    /// </summary>
    /// <param name="id">The identifier of the bot.</param>
    /// <returns>An instance of bot entity.</returns>
    Task<UserItemInfo> GetBotAsync(string id);

    /// <summary>
    /// Gets the organization entity by identifier.
    /// </summary>
    /// <param name="id">The identifier of the .</param>
    /// <returns>An instance of organization entity.</returns>
    Task<UserItemInfo> GetOrganizationAsync(string id);

    /// <summary>
    /// Gets the agent entity by identifier.
    /// </summary>
    /// <param name="id">The identifier of the agent.</param>
    /// <returns>An instance of agent entity.</returns>
    Task<UserItemInfo> GetAgentAsync(string id);

    /// <summary>
    /// Gets the user group entity by identifier.
    /// </summary>
    /// <param name="id">The identifier of the user group.</param>
    /// <returns>An instance of user group entity.</returns>
    Task<UserItemInfo> GetUserGroupAsync(string id);
}
