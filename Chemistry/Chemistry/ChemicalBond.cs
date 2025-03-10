﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trivial.Chemistry;

/// <summary>
/// The types of chemcial bond.
/// </summary>
public enum ChemicalBondTypes : byte
{
    /// <summary>
    /// Ionic bond.
    /// </summary>
    Ionic = 0,

    /// <summary>
    /// Covalent bound.
    /// </summary>
    Covalent = 1,

    /// <summary>
    /// Metallic bound.
    /// </summary>
    Metallic = 2
}

/// <summary>
/// The chemical bond information.
/// </summary>
/// <param name="type">The chemical bond type.</param>
/// <param name="numbers">The bond numbers.</param>
public class ChemicalBond(ChemicalBondTypes type, int numbers)
{
    /// <summary>
    /// Gets the chemical bond type.
    /// </summary>
    public ChemicalBondTypes Type { get; } = type;

    /// <summary>
    /// Gets the bound numbers.
    /// </summary>
    public int Numbers { get; } = numbers;
}
