using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trivial.Users;

/// <summary>
/// Genders.
/// </summary>
public enum Genders : byte
{
    /// <summary>
    /// Unknown (or secret).
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Male.
    /// </summary>
    Male = 1,

    /// <summary>
    /// Female.
    /// </summary>
    Female = 2,

    /// <summary>
    /// Genderfluid or both.
    /// </summary>
    Fluid = 5,

    /// <summary>
    /// Asexual, neutrois, machine or other kind of things without gender.
    /// </summary>
    Asexual = 6,

    /// <summary>
    /// Other.
    /// </summary>
    Other = 7
}
