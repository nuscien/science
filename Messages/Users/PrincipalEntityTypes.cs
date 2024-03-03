using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Principal;
using System.Text;
using System.Text.Json.Serialization;

using Trivial.Security;
using Trivial.Text;

namespace Trivial.Users
{
    /// <summary>
    /// Security principal entity types.
    /// </summary>
    public enum PrincipalEntityTypes : byte
    {
        /// <summary>
        /// Unknown.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// User.
        /// </summary>
        User = 1,

        /// <summary>
        /// User group or role.
        /// </summary>
        Group = 2,

        /// <summary>
        /// App or service.
        /// </summary>
        Service = 3,

        /// <summary>
        /// Bot.
        /// </summary>
        Bot = 4,

        /// <summary>
        /// Authenticated device.
        /// </summary>
        Device = 5,

        /// <summary>
        /// The special agent.
        /// </summary>
        Agent = 6,

        /// <summary>
        /// The other type.
        /// </summary>
        Other = 63,
    }
}
