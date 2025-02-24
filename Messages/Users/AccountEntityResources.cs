using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
