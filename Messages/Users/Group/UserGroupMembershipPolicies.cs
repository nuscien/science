using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trivial.Users;

/// <summary>
/// User group membership policies.
/// </summary>
public enum UserGroupMembershipPolicies : byte
{
    /// <summary>
    /// Disallow to join in. Only admin can add users in.
    /// </summary>
    Forbidden = 0,

    /// <summary>
    /// Need apply for membership with approval.
    /// </summary>
    Application = 1,

    /// <summary>
    /// Allow to join in withou any approval.
    /// </summary>
    Allow = 2,

    /// <summary>
    /// Other unknown way.
    /// </summary>
    Other = 63,
}
