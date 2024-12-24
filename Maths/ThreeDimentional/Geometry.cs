using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Trivial.Maths;

/// <summary>
/// The utility for geometry.
/// </summary>
public static partial class Geometry
{
    /// <summary>
    /// Computes the distance between a point and a plane.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="a">The a parameter of plane.</param>
    /// <param name="b">The b parameter of plane.</param>
    /// <param name="c">The c parameter of plane.</param>
    /// <param name="d">The d parameter of plane.</param>
    /// <returns>The distance.</returns>
    public static double Distance(DoublePoint3D point, double a, double b, double c, double d)
        => Math.Abs(a * point.X + b * point.Y + c * point.Z + d) / Math.Sqrt(a * a + b * b + c * c);

#if NET6_0_OR_GREATER
    /// <summary>
    /// Computes the distance between a point and a plane.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="a">The a parameter of plane.</param>
    /// <param name="b">The b parameter of plane.</param>
    /// <param name="c">The c parameter of plane.</param>
    /// <param name="d">The d parameter of plane.</param>
    /// <returns>The distance.</returns>
    public static float Distance(Point3D<float> point, float a, float b, float c, float d)
        => MathF.Abs(a * point.X + b * point.Y + c * point.Z + d) / MathF.Sqrt(a * a + b * b + c * c);

    /// <summary>
    /// Computes the distance between a point and a plane.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="a">The a parameter of plane.</param>
    /// <param name="b">The b parameter of plane.</param>
    /// <param name="c">The c parameter of plane.</param>
    /// <param name="d">The d parameter of plane.</param>
    /// <returns>The distance.</returns>
    public static float Distance(Vector3 point, float a, float b, float c, float d)
        => MathF.Abs(a * point.X + b * point.Y + c * point.Z + d) / MathF.Sqrt(a * a + b * b + c * c);
#endif
}
