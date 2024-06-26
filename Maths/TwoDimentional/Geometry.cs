﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trivial.Maths;

/// <summary>
/// The utility for geometry.
/// </summary>
public static partial class Geometry
{
    /// <summary>
    /// Computes euclidean metric.
    /// </summary>
    /// <param name="start">The start point.</param>
    /// <param name="end">The end point.</param>
    /// <returns>The distance.</returns>
    public static double Distance(Point2D<double> start, Point2D<double> end)
    {
        if (start == null) start = new();
        if (end == null) end = new();
        var width = end.X - start.X;
        var height = end.Y - start.Y;
        return Math.Sqrt(width * width + height * height);
    }

    /// <summary>
    /// Computes euclidean metric.
    /// </summary>
    /// <param name="start">The start point.</param>
    /// <param name="end">The end point.</param>
    /// <returns>The distance.</returns>
    public static float Distance(PointF start, PointF end)
    {
        var width = end.X - start.X;
        var height = end.Y - start.Y;
#if NETFRAMEWORK
        return (float)Math.Sqrt(width * width + height * height);
#else
        return MathF.Sqrt(width * width + height * height);
#endif
    }

    /// <summary>
    /// Computes vector cross product.
    /// </summary>
    /// <param name="a">A point.</param>
    /// <param name="b">Another point.</param>
    /// <param name="o">The vertex/origin point.</param>
    /// <returns>The vector cross product. Greater than 0 if anticlockwise; equals to 0 if collineation; less than 0 if clockwise.</returns>
    public static double CrossProduct(Point2D<double> a, Point2D<double> b, Point2D<double> o = null)
    {
        if (a == null) a = new();
        if (b == null) b = new();
        if (o == null) o = new();
        return (a.X - o.X) * (b.Y - o.Y) - (b.X - o.X) * (a.Y - o.Y);
    }

    /// <summary>
    /// Computes vector cross product.
    /// </summary>
    /// <param name="a">A point.</param>
    /// <param name="b">Another point.</param>
    /// <param name="o">The vertex/origin point.</param>
    /// <returns>The vector cross product. Greater than 0 if anticlockwise; equals to 0 if collineation; less than 0 if clockwise.</returns>
    public static float CrossProduct(PointF a, PointF b, PointF o)
        => (a.X - o.X) * (b.Y - o.Y) - (b.X - o.X) * (a.Y - o.Y);

    /// <summary>
    /// Computes vector cross product.
    /// </summary>
    /// <param name="a">A point.</param>
    /// <param name="b">Another point.</param>
    /// <returns>The vector cross product. Greater than 0 if anticlockwise; equals to 0 if collineation; less than 0 if clockwise.</returns>
    public static float CrossProduct(PointF a, PointF b)
        => CrossProduct(a, b, new PointF(0, 0));

    /// <summary>
    /// Computes vector dot (scalar) product.
    /// </summary>
    /// <param name="a">A point.</param>
    /// <param name="b">Another point.</param>
    /// <param name="o">The vertex/origin point.</param>
    /// <returns>The vector dot (scalar) product. Greater than 0 if it is obtuse angle; equals to 0 if it is right angle; less than 0 if it is acute angle.</returns>
    public static double DotProduct(Point2D<double> a, Point2D<double> b, Point2D<double> o = null)
    {
        if (a == null) a = new();
        if (b == null) b = new();
        if (o == null) o = new();
        return (a.X - o.X) * (b.X - o.X) + (a.Y - o.Y) * (b.Y - o.Y);
    }

    /// <summary>
    /// Computes vector dot (scalar) product.
    /// </summary>
    /// <param name="a">A point.</param>
    /// <param name="b">Another point.</param>
    /// <param name="o">The vertex/origin point.</param>
    /// <returns>The vector dot (scalar) product. Greater than 0 if it is obtuse angle; equals to 0 if it is right angle; less than 0 if it is acute angle.</returns>
    public static float DotProduct(PointF a, PointF b, PointF o)
        => (a.X - o.X) * (b.X - o.X) + (a.Y - o.Y) * (b.Y - o.Y);

    /// <summary>
    /// Computes vector dot (scalar) product.
    /// </summary>
    /// <param name="a">A point.</param>
    /// <param name="b">Another point.</param>
    /// <returns>The vector dot (scalar) product. Greater than 0 if it is obtuse angle; equals to 0 if it is right angle; less than 0 if it is acute angle.</returns>
    public static float DotProduct(PointF a, PointF b)
        => DotProduct(a, b, new PointF(0, 0));

    /// <summary>
    /// Gets the point after counter clockwise rotation.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="origin">The origin point.</param>
    /// <param name="alpha">The angle to counter clockwise rotate.</param>
    /// <returns>The point after rotation.</returns>
    public static DoublePoint2D Rotate(Point2D<double> point, Point2D<double> origin, Angle alpha)
    {
        if (point == null) point = new();
        if (alpha.Degrees == 0) return new DoublePoint2D(point.X, point.Y);
        if (origin == null) origin = new();
        var radian = alpha.Radians;
        var x = origin.X - point.X;
        var y = origin.Y - point.Y;
        return new DoublePoint2D(
            x * Math.Cos(radian) - y * Math.Sin(radian) + x,
            y * Math.Cos(radian) + x * Math.Sin(radian) + y);
    }

    /// <summary>
    /// Gets the point after counter clockwise rotation.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="origin">The origin point.</param>
    /// <param name="alpha">The angle to counter clockwise rotate.</param>
    /// <returns>The point after rotation.</returns>
    public static PointF Rotate(PointF point, PointF origin, Angle alpha)
    {
        if (alpha.Degrees == 0) return point;
        var radian = (float)alpha.Radians;
        var x = origin.X - point.X;
        var y = origin.Y - point.Y;
#if NETFRAMEWORK
        return new PointF(
            x * (float)Math.Cos(radian) - y * (float)Math.Sin(radian) + x,
            y * (float)Math.Cos(radian) + x * (float)Math.Sin(radian) + y);
#else
        return new PointF(
            x * MathF.Cos(radian) - y * MathF.Sin(radian) + x,
            y * MathF.Cos(radian) + x * MathF.Sin(radian) + y);
#endif
    }

    /// <summary>
    /// Computes included angle.
    /// </summary>
    /// <param name="vertex">The vertex point.</param>
    /// <param name="start">The start point.</param>
    /// <param name="end">The end point.</param>
    /// <returns>The included angle radian of line vertex-start and line vertex-end.</returns>
    internal static double AngleRadian(Point2D<double> vertex, Point2D<double> start, Point2D<double> end)
    {
        if (vertex == null) vertex = new();
        if (start == null) start = new();
        if (end == null) end = new();
        var dsx = start.X - vertex.X;
        var dsy = start.Y - vertex.Y;
        var dex = end.X - vertex.X;
        var dey = end.Y - vertex.Y;
        if (Math.Abs(dex) < InternalHelper.DoubleAccuracy && Math.Abs(dey) < InternalHelper.DoubleAccuracy) return 0;
        var cosfi = dsx * dex + dsy * dey;
        var norm = (dsx * dsx + dsy * dsy) * (dex * dex + dey * dey);
        cosfi /= Math.Sqrt(norm);
        if (cosfi >= 1.0) return 0;
        if (cosfi <= -1.0) return -Math.PI;
        var fi = Math.Acos(cosfi);
        if (dsx * dey - dsy * dex > 0) return fi;
        return -fi;
    }

    /// <summary>
    /// Computes included angle.
    /// </summary>
    /// <param name="vertex">The vertex point.</param>
    /// <param name="start">The start point.</param>
    /// <param name="end">The end point.</param>
    /// <returns>The included angle radian of line vertex-start and line vertex-end.</returns>
    internal static float AngleRadian(PointF vertex, PointF start, PointF end)
    {
        var dsx = start.X - vertex.X;
        var dsy = start.Y - vertex.Y;
        var dex = end.X - vertex.X;
        var dey = end.Y - vertex.Y;
        if (Math.Abs(dex) < InternalHelper.DoubleAccuracy && Math.Abs(dey) < InternalHelper.DoubleAccuracy) return 0;
        var cosfi = dsx * dex + dsy * dey;
        var norm = (dsx * dsx + dsy * dsy) * (dex * dex + dey * dey);
        cosfi /= (float)Math.Sqrt(norm);
        if (cosfi >= 1.0) return 0;
        if (cosfi <= -1.0) return -(float)Math.PI;
        var fi = (float)Math.Acos(cosfi);
        if (dsx * dey - dsy * dex > 0) return fi;
        return -fi;
    }

    /// <summary>
    /// Computes included angle.
    /// </summary>
    /// <param name="vertex">The vertex point.</param>
    /// <param name="start">The start point.</param>
    /// <param name="end">The end point.</param>
    /// <returns>The included angle of line vertex-start and line vertex-end.</returns>
    public static Angle Angle(Point2D<double> vertex, Point2D<double> start, Point2D<double> end)
        => Maths.Angle.FromRadian(AngleRadian(vertex, start, end));

    /// <summary>
    /// Computes included angle.
    /// </summary>
    /// <param name="vertex">The vertex point.</param>
    /// <param name="start">The start point.</param>
    /// <param name="end">The end point.</param>
    /// <returns>The included angle of line vertex-start and line vertex-end.</returns>
    public static Angle Angle(PointF vertex, PointF start, PointF end)
        => Maths.Angle.FromRadian(AngleRadian(vertex, start, end));

    /// <summary>
    /// Gets the position of a point that relative to a specific line.
    /// </summary>
    /// <param name="point">The point to get the relative position.</param>
    /// <param name="line">The line to compare.</param>
    /// <returns>The relative position ratio. Less than 0 if the point is on the backward extension of the line segment; greater than 1 if forward; equals to 0 if on the start point of the line segment; equals to 1 if on the end point; between 0 and 1 if on the line segment.</returns>
    /// <exception cref="ArgumentNullException">line was null.</exception>
    public static double Relation(Point2D<double> point, LineSegment line)
    {
        if (line == null) throw new ArgumentNullException(nameof(line), "line should not be null.");
        if (point == null) point = new();
        return DotProduct(point is DoublePoint2D p ? p : new DoublePoint2D(point.X, point.Y), line.End, line.Start) / (Distance(line.Start, line.End) * Distance(line.Start, line.End));
    }

    /// <summary>
    /// Gets the position of a point that relative to a specific line.
    /// </summary>
    /// <param name="point">The point to get the relative position.</param>
    /// <param name="line">The line to compare.</param>
    /// <returns>The relative position ratio. Less than 0 if the point is on the backward extension of the line segment; greater than 1 if forward; equals to 0 if on the start point of the line segment; equals to 1 if on the end point; between 0 and 1 if on the line segment.</returns>
    /// <exception cref="ArgumentNullException">line was null.</exception>
    public static float Relation(PointF point, LineSegmentF line)
    {
        if (line == null) throw new ArgumentNullException(nameof(line), "line should not be null.");
        return DotProduct(new PointF(point.X, point.Y), line.End, line.Start) / (Distance(line.Start, line.End) * Distance(line.Start, line.End));
    }

    /// <summary>
    /// Gets the foot point.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="line">The line.</param>
    /// <returns>The foot point.</returns>
    /// <exception cref="ArgumentNullException">line was null.</exception>
    public static DoublePoint2D GetFootPoint(Point2D<double> point, LineSegment line)
    {
        var r = Relation(point, line);
        return new(
            line.Start.X + r * (line.End.X - line.Start.X),
            line.Start.Y + r * (line.End.Y - line.Start.Y));
    }

    /// <summary>
    /// Gets the foot point.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="line">The line.</param>
    /// <returns>The foot point.</returns>
    /// <exception cref="ArgumentNullException">line was null.</exception>
    public static PointF GetFootPoint(PointF point, LineSegmentF line)
    {
        var r = Relation(point, line);
        return new(
            line.Start.X + r * (line.End.X - line.Start.X),
            line.Start.Y + r * (line.End.Y - line.Start.Y));
    }

    /// <summary>
    /// Computes the distance between the point and the line.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="line">The line.</param>
    /// <param name="closest">The foot point or the closest point.</param>
    /// <returns>The distance.</returns>
    /// <exception cref="ArgumentNullException">line was null.</exception>
    public static double Distance(Point2D<double> point, LineSegment line, out DoublePoint2D closest)
    {
        if (point == null) point = new();
        var r = Relation(point, line);
        if (r < 0)
        {
            closest = line.Start;
            return Distance(point, line.Start);
        }
        if (r > 1)
        {
            closest = line.End;
            return Distance(point, line.End);
        }

        closest = new(
            line.Start.X + r * (line.End.X - line.Start.X),
            line.Start.Y + r * (line.End.Y - line.Start.Y));
        return Distance(point, closest);
    }

    /// <summary>
    /// Computes the distance between the point and the line.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="line">The line.</param>
    /// <returns>The distance.</returns>
    /// <exception cref="ArgumentNullException">line was null.</exception>
    public static double Distance(Point2D<double> point, LineSegment line)
        => Distance(point, line, out _);

    /// <summary>
    /// Computes the distance between the point and the line.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="line">The line.</param>
    /// <param name="extendToLine">true if extend the line segment to a line; otherwise, false.</param>
    /// <returns>The distance.</returns>
    /// <exception cref="ArgumentNullException">line was null.</exception>
    public static double Distance(Point2D<double> point, LineSegment line, bool extendToLine)
        => extendToLine
        ? Math.Abs(CrossProduct(point, line.End, line.Start)) / Distance(line.Start, line.End)
        : Distance(point, line, out _);

    /// <summary>
    /// Computes the distance between the point and the line.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="line">The line.</param>
    /// <param name="closest">The foot point or the closest point.</param>
    /// <returns>The distance.</returns>
    /// <exception cref="ArgumentNullException">line was null.</exception>
    public static float Distance(PointF point, LineSegmentF line, out PointF closest)
    {
        var r = Relation(point, line);
        if (r < 0)
        {
            closest = line.Start;
            return Distance(point, line.Start);
        }
        if (r > 1)
        {
            closest = line.End;
            return Distance(point, line.End);
        }

        closest = new(
            line.Start.X + r * (line.End.X - line.Start.X),
            line.Start.Y + r * (line.End.Y - line.Start.Y));
        return Distance(point, closest);
    }

    /// <summary>
    /// Computes the distance between the point and the line.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="line">The line.</param>
    /// <returns>The distance.</returns>
    /// <exception cref="ArgumentNullException">line was null.</exception>
    public static double Distance(PointF point, LineSegmentF line)
        => Distance(point, line, out _);

    /// <summary>
    /// Computes the distance between the point and the line.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="line">The line.</param>
    /// <param name="extendToLine">true if extend the line segment to a line; otherwise, false.</param>
    /// <returns>The distance.</returns>
    /// <exception cref="ArgumentNullException">line was null.</exception>
    public static float Distance(PointF point, LineSegmentF line, bool extendToLine)
        => extendToLine
        ? Math.Abs(CrossProduct(point, line.End, line.Start)) / Distance(line.Start, line.End)
        : Distance(point, line, out _);

    /// <summary>
    /// Computes the distance between the point and the polyline.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="polyline">The polyline.</param>
    /// <param name="closest">The foot point or the closest point.</param>
    /// <returns>The distance.</returns>
    public static double Distance(Point2D<double> point, DoublePoint2D[] polyline, out DoublePoint2D closest)
    {
        var cd = double.PositiveInfinity;
        double td;
        LineSegment l = new();
        DoublePoint2D cq = new();
        var count = polyline.Length - 1;
        for (var i = 0; i < count; i++)
        {
            l.Start = polyline[i];
            l.End = polyline[i + 1];
            td = Distance(point, l, out var tq);
            if (td < cd)
            {
                cd = td;
                cq = tq;
            }
        }

        closest = cq;
        return cd;
    }

    /// <summary>
    /// Computes the distance between the point and the polyline.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="polyline">The polyline.</param>
    /// <returns>The distance.</returns>
    public static double Distance(Point2D<double> point, DoublePoint2D[] polyline)
        => Distance(point, polyline, out _);

    /// <summary>
    /// Computes the distance between the point and the polyline.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="polyline">The polyline.</param>
    /// <param name="closest">The foot point or the closest point.</param>
    /// <returns>The distance.</returns>
    public static double Distance(Point2D<double> point, IList<DoublePoint2D> polyline, out DoublePoint2D closest)
    {
        var cd = double.PositiveInfinity;
        double td;
        LineSegment l = new();
        DoublePoint2D cq = new();
        var count = polyline.Count - 1;
        for (var i = 0; i < count; i++)
        {
            l.Start = polyline[i];
            l.End = polyline[i + 1];
            td = Distance(point, l, out var tq);
            if (td < cd)
            {
                cd = td;
                cq = tq;
            }
        }
        closest = cq;
        return cd;
    }

    /// <summary>
    /// Computes the distance between the point and the polyline.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="polyline">The polyline.</param>
    /// <returns>The distance.</returns>
    public static double Distance(Point2D<double> point, IList<DoublePoint2D> polyline)
        => Distance(point, polyline, out _);

    /// <summary>
    /// Computes the distance between the point and the polyline.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="polyline">The polyline.</param>
    /// <param name="closest">The foot point or the closest point.</param>
    /// <returns>The distance.</returns>
    public static double Distance(PointF point, PointF[] polyline, out PointF closest)
    {
        var cd = double.PositiveInfinity;
        double td;
        LineSegmentF l = new();
        PointF cq = new();
        var count = polyline.Length - 1;
        for (var i = 0; i < count; i++)
        {
            l.Start = polyline[i];
            l.End = polyline[i + 1];
            td = Distance(point, l, out var tq);
            if (td < cd)
            {
                cd = td;
                cq = tq;
            }
        }

        closest = cq;
        return cd;
    }

    /// <summary>
    /// Computes the distance between the point and the polyline.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="polyline">The polyline.</param>
    /// <returns>The distance.</returns>
    public static double Distance(PointF point, PointF[] polyline)
        => Distance(point, polyline, out _);

    /// <summary>
    /// Computes the distance between the point and the polyline.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="polyline">The polyline.</param>
    /// <param name="closest">The foot point or the closest point.</param>
    /// <returns>The distance.</returns>
    public static double Distance(PointF point, IList<PointF> polyline, out PointF closest)
    {
        var cd = double.PositiveInfinity;
        double td;
        LineSegmentF l = new();
        PointF cq = new();
        var count = polyline.Count - 1;
        for (var i = 0; i < count; i++)
        {
            l.Start = polyline[i];
            l.End = polyline[i + 1];
            td = Distance(point, l, out var tq);
            if (td < cd)
            {
                cd = td;
                cq = tq;
            }
        }
        closest = cq;
        return cd;
    }

    /// <summary>
    /// Computes the distance between the point and the polyline.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="polyline">The polyline.</param>
    /// <returns>The distance.</returns>
    public static double Distance(PointF point, IList<PointF> polyline)
        => Distance(point, polyline, out _);

    /// <summary>
    /// Tests if the circle is in the polygon or is intersected with the polygon.
    /// </summary>
    /// <param name="circle">The circle.</param>
    /// <param name="polygon">The polygon.</param>
    /// <returns>true if the circle is in the polygon or is intersected with the polygon; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">radius was null.</exception>
    public static bool IsInOrIntersected(CoordinateCircle circle, DoublePoint2D[] polygon)
    {
        if (circle == null || double.IsNaN(circle.Radius)) throw new ArgumentNullException(nameof(circle), "circle should not be null.");
        var d = Distance(circle.Center, polygon, out _);
        return d < circle.Radius || Math.Abs(d - circle.Radius) < InternalHelper.DoubleAccuracy;
    }

    /// <summary>
    /// Tests if the circle is in the polygon or is intersected with the polygon.
    /// </summary>
    /// <param name="circle">The circle.</param>
    /// <param name="polygon">The polygon.</param>
    /// <returns>true if the circle is in the polygon or is intersected with the polygon; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">radius was null.</exception>
    public static bool IsInOrIntersected(CoordinateCircle circle, IList<DoublePoint2D> polygon)
    {
        if (circle == null || double.IsNaN(circle.Radius)) throw new ArgumentNullException(nameof(circle), "circle should not be null.");
        var d = Distance(circle.Center, polygon, out _);
        return d < circle.Radius || Math.Abs(d - circle.Radius) < InternalHelper.DoubleAccuracy;
    }

    /// <summary>
    /// Tests if the circle is in the polygon or is intersected with the polygon.
    /// </summary>
    /// <param name="circle">The circle.</param>
    /// <param name="polygon">The polygon.</param>
    /// <returns>true if the circle is in the polygon or is intersected with the polygon; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">radius was null.</exception>
    public static bool IsInOrIntersected(CoordinateCircleF circle, PointF[] polygon)
    {
        if (circle == null || double.IsNaN(circle.Radius)) throw new ArgumentNullException(nameof(circle), "circle should not be null.");
        var d = Distance(circle.Center, polygon, out _);
        return d < circle.Radius || Math.Abs(d - circle.Radius) < InternalHelper.DoubleAccuracy;
    }

    /// <summary>
    /// Tests if the circle is in the polygon or is intersected with the polygon.
    /// </summary>
    /// <param name="circle">The circle.</param>
    /// <param name="polygon">The polygon.</param>
    /// <returns>true if the circle is in the polygon or is intersected with the polygon; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">radius was null.</exception>
    public static bool IsInOrIntersected(CoordinateCircleF circle, IList<PointF> polygon)
    {
        if (circle == null || double.IsNaN(circle.Radius)) throw new ArgumentNullException(nameof(circle), "circle should not be null.");
        var d = Distance(circle.Center, polygon, out _);
        return d < circle.Radius || Math.Abs(d - circle.Radius) < InternalHelper.DoubleAccuracy;
    }

    /// <summary>
    /// Gets the handed rotation state.
    /// </summary>
    /// <param name="a">The first line.</param>
    /// <param name="b">The second line.</param>
    /// <returns>The state. Less than 0 if left handed rotation; equals to 0 if the same; Greater than 0 if right handed rotation.</returns>
    public static double HandedRotation(LineSegment a, LineSegment b)
    {
        var dx1 = a.Start.X - a.End.X;
        var dy1 = a.Start.Y - a.End.Y;
        var dx2 = b.Start.X - b.End.X;
        var dy2 = b.Start.Y - b.End.Y;
        return dx2 * dy1 - dx1 * dy2;
    }

    /// <summary>
    /// Gets the handed rotation state.
    /// </summary>
    /// <param name="a">The first line.</param>
    /// <param name="b">The second line.</param>
    /// <returns>The state. Less than 0 if left handed rotation; equals to 0 if the same; Greater than 0 if right handed rotation.</returns>
    public static float HandedRotation(LineSegmentF a, LineSegmentF b)
    {
        var dx1 = a.Start.X - a.End.X;
        var dy1 = a.Start.Y - a.End.Y;
        var dx2 = b.Start.X - b.End.X;
        var dy2 = b.Start.Y - b.End.Y;
        return dx2 * dy1 - dx1 * dy2;
    }

    /// <summary>
    /// Computes sine of 2 vectors.
    /// </summary>
    /// <param name="a">The first vector.</param>
    /// <param name="b">The second vector.</param>
    /// <returns>The sine.</returns>
    /// <exception cref="ArgumentNullException">a or b was null.</exception>
    public static double Sin(LineSegment a, LineSegment b)
    {
        if (a == null) throw new ArgumentNullException(nameof(a), "a should not be null");
        if (b == null) throw new ArgumentNullException(nameof(b), "b should not be null");
        return (Distance(a.End, a.Start) * Distance(b.End, b.Start)) / ((a.End.X - a.Start.X) * (b.End.X - b.Start.X) + (a.End.Y - a.Start.Y) * (b.End.Y - b.Start.Y));
    }

    /// <summary>
    /// Computes cosine of 2 vectors.
    /// </summary>
    /// <param name="a">The first vector.</param>
    /// <param name="b">The second vector.</param>
    /// <returns>The cosine.</returns>
    /// <exception cref="ArgumentNullException">a or b was null.</exception>
    public static double Cos(LineSegment a, LineSegment b)
    {
        if (a == null) throw new ArgumentNullException(nameof(a), "a should not be null");
        if (b == null) throw new ArgumentNullException(nameof(b), "b should not be null");
        return ((a.End.X - a.Start.X) * (b.End.X - b.Start.X) + (a.End.Y - a.Start.Y) * (b.End.Y - b.Start.Y)) / (Distance(a.End, a.Start) * Distance(b.End, b.Start));
    }

    /// <summary>
    /// Computes sine of 2 vectors.
    /// </summary>
    /// <param name="a">The first vector.</param>
    /// <param name="b">The second vector.</param>
    /// <returns>The sine.</returns>
    /// <exception cref="ArgumentNullException">a or b was null.</exception>
    public static double Sin(LineSegmentF a, LineSegmentF b)
    {
        if (a == null) throw new ArgumentNullException(nameof(a), "a should not be null");
        if (b == null) throw new ArgumentNullException(nameof(b), "b should not be null");
        return (Distance(a.End, a.Start) * Distance(b.End, b.Start)) / ((a.End.X - a.Start.X) * (b.End.X - b.Start.X) + (a.End.Y - a.Start.Y) * (b.End.Y - b.Start.Y));
    }

    /// <summary>
    /// Computes cosine of 2 vectors.
    /// </summary>
    /// <param name="a">The first vector.</param>
    /// <param name="b">The second vector.</param>
    /// <returns>The cosine.</returns>
    /// <exception cref="ArgumentNullException">a or b was null.</exception>
    public static double Cos(LineSegmentF a, LineSegmentF b)
    {
        if (a == null) throw new ArgumentNullException(nameof(a), "a should not be null");
        if (b == null) throw new ArgumentNullException(nameof(b), "b should not be null");
        return ((a.End.X - a.Start.X) * (b.End.X - b.Start.X) + (a.End.Y - a.Start.Y) * (b.End.Y - b.Start.Y)) / (Distance(a.End, a.Start) * Distance(b.End, b.Start));
    }

    /// <summary>
    /// Tests if 2 line segments are intersected.
    /// </summary>
    /// <param name="a">The first line.</param>
    /// <param name="b">The second line.</param>
    /// <returns>true if the 2 lines are intersected, including connect with vertex; otherwise, false.</returns>
    public static bool IsIntersected(LineSegment a, LineSegment b)
    {
        if (a == null || b == null) return false;
        return (Math.Max(a.Start.X, a.End.X) >= Math.Min(b.Start.X, b.End.X)) &&
            (Math.Max(b.Start.X, b.End.X) >= Math.Min(a.Start.X, a.End.X)) &&
            (Math.Max(a.Start.Y, a.End.Y) >= Math.Min(b.Start.Y, b.End.Y)) &&
            (Math.Max(b.Start.Y, b.End.Y) >= Math.Min(a.Start.Y, a.End.Y)) &&
            (CrossProduct(b.Start, a.End, a.Start) * CrossProduct(a.End, b.End, a.Start) >= 0) &&
            (CrossProduct(a.Start, b.End, b.Start) * CrossProduct(b.End, a.End, b.Start) >= 0);
    }

    /// <summary>
    /// Tests if 2 line segments are intersected.
    /// </summary>
    /// <param name="a">The first line.</param>
    /// <param name="b">The second line.</param>
    /// <returns>true if the 2 lines are intersected, including connect with vertex; otherwise, false.</returns>
    public static bool IsIntersected(LineSegmentF a, LineSegmentF b)
    {
        if (a == null || b == null) return false;
        return (Math.Max(a.Start.X, a.End.X) >= Math.Min(b.Start.X, b.End.X)) &&
            (Math.Max(b.Start.X, b.End.X) >= Math.Min(a.Start.X, a.End.X)) &&
            (Math.Max(a.Start.Y, a.End.Y) >= Math.Min(b.Start.Y, b.End.Y)) &&
            (Math.Max(b.Start.Y, b.End.Y) >= Math.Min(a.Start.Y, a.End.Y)) &&
            (CrossProduct(b.Start, a.End, a.Start) * CrossProduct(a.End, b.End, a.Start) >= 0) &&
            (CrossProduct(a.Start, b.End, b.Start) * CrossProduct(b.End, a.End, b.Start) >= 0);
    }

    /// <summary>
    /// Computes included angle.
    /// </summary>
    /// <param name="a">The first line segment.</param>
    /// <param name="b">The second line segment.</param>
    /// <returns>The included angle of line vertex-start and line vertex-end.</returns>
    /// <exception cref="ArgumentNullException">a or b was null.</exception>
    public static Angle Angle(LineSegment a, LineSegment b)
    {
        if (a == null) throw new ArgumentNullException(nameof(a), "a should not be null");
        if (b == null) throw new ArgumentNullException(nameof(b), "b should not be null");
        return Angle(new DoublePoint2D(0, 0), new DoublePoint2D(a.End.X - a.Start.X, a.End.Y - a.Start.Y), new DoublePoint2D(b.End.X - b.Start.X, b.End.Y - b.Start.Y));
    }

    /// <summary>
    /// Computes included angle.
    /// </summary>
    /// <param name="a">The first line segment.</param>
    /// <param name="b">The second line segment.</param>
    /// <returns>The included angle of line vertex-start and line vertex-end.</returns>
    /// <exception cref="ArgumentNullException">a or b was null.</exception>
    public static Angle Angle(LineSegmentF a, LineSegmentF b)
    {
        if (a == null) throw new ArgumentNullException(nameof(a), "a should not be null");
        if (b == null) throw new ArgumentNullException(nameof(b), "b should not be null");
        return Angle(new DoublePoint2D(0, 0), new DoublePoint2D(a.End.X - a.Start.X, a.End.Y - a.Start.Y), new DoublePoint2D(b.End.X - b.Start.X, b.End.Y - b.Start.Y));
    }

    /// <summary>
    /// Gets the angle of the line.
    /// </summary>
    /// <param name="line">The line.</param>
    /// <returns>The angle.</returns>
    /// <exception cref="ArgumentNullException">line was null.</exception>
    internal static double AngleRadian(StraightLine line)
    {
        if (line == null) throw new ArgumentNullException(nameof(line), "line should noe be null.");
        if (Math.Abs(line.A) < InternalHelper.DoubleAccuracy)
            return 0;
        if (Math.Abs(line.B) < InternalHelper.DoubleAccuracy)
            return Math.PI / 2;
        if (line.Slope > 0)
            return Math.Atan(line.Slope);
        else
            return Math.PI + Math.Atan(line.Slope);
    }

    /// <summary>
    /// Gets the angle of the line.
    /// </summary>
    /// <param name="line">The line.</param>
    /// <returns>The angle.</returns>
    /// <exception cref="ArgumentNullException">line was null.</exception>
    internal static double AngleRadian(StraightLineF line)
    {
        if (line == null) throw new ArgumentNullException(nameof(line), "line should noe be null.");
        if (Math.Abs(line.A) < InternalHelper.SingleAccuracy)
            return 0;
        if (Math.Abs(line.B) < InternalHelper.SingleAccuracy)
            return Math.PI / 2;
        if (line.Slope > 0)
            return Math.Atan(line.Slope);
        else
            return Math.PI + Math.Atan(line.Slope);
    }

    /// <summary>
    /// Gets the angle of the line.
    /// </summary>
    /// <param name="line">The line.</param>
    /// <returns>The angle.</returns>
    /// <exception cref="ArgumentNullException">line was null.</exception>
    public static Angle Angle(StraightLine line)
        => Maths.Angle.FromRadian(AngleRadian(line));

    /// <summary>
    /// Gets the angle of the line.
    /// </summary>
    /// <param name="line">The line.</param>
    /// <returns>The angle.</returns>
    /// <exception cref="ArgumentNullException">line was null.</exception>
    public static Angle Angle(StraightLineF line)
        => Maths.Angle.FromRadian(AngleRadian(line));

    /// <summary>
    /// Computes symmetry point from a line.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="line">The line.</param>
    /// <returns>The symmetry point.</returns>
    /// <exception cref="ArgumentNullException">line was null.</exception>
    public static DoublePoint2D Symmetry(DoublePoint2D point, StraightLine line)
    {
        if (line == null) throw new ArgumentNullException(nameof(line), "line should noe be null.");
        if (point == null) point = new();
        return new(
            ((line.B * line.B - line.A * line.A) * point.X - 2 * line.A * line.B * point.Y - 2 * line.A * line.C) / (line.A * line.A + line.B * line.B),
            ((line.A * line.A - line.B * line.B) * point.Y - 2 * line.A * line.B * point.X - 2 * line.B * line.C) / (line.A * line.A + line.B * line.B));
    }

    /// <summary>
    /// Computes symmetry point from a line.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="line">The line.</param>
    /// <returns>The symmetry point.</returns>
    /// <exception cref="ArgumentNullException">line was null.</exception>
    public static PointF Symmetry(PointF point, StraightLineF line)
    {
        if (line == null) throw new ArgumentNullException(nameof(line), "line should noe be null.");
        return new(
            ((line.B * line.B - line.A * line.A) * point.X - 2 * line.A * line.B * point.Y - 2 * line.A * line.C) / (line.A * line.A + line.B * line.B),
            ((line.A * line.A - line.B * line.B) * point.Y - 2 * line.A * line.B * point.X - 2 * line.B * line.C) / (line.A * line.A + line.B * line.B));
    }

    /// <summary>
    /// Tests the specific 2 point are in same side.
    /// </summary>
    /// <param name="a">The first point.</param>
    /// <param name="b">The second point.</param>
    /// <param name="line">The line.</param>
    /// <returns>true if they are in same side; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">line was null.</exception>
    public static bool AreSameSide(DoublePoint2D a, DoublePoint2D b, StraightLine line)
    {
        if (line == null) throw new ArgumentNullException(nameof(line), "line should noe be null.");
        if (a == null) a = new();
        if (b == null) b = new();
        return (line.A * a.X + line.B * a.Y + line.C) * (line.A * b.X + line.B * b.Y + line.C) > 0;
    }

    /// <summary>
    /// Tests the specific 2 point are in same side.
    /// </summary>
    /// <param name="a">The first point.</param>
    /// <param name="b">The second point.</param>
    /// <param name="line">The line.</param>
    /// <returns>true if they are in same side; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">line was null.</exception>
    public static bool AreSameSide(PointF a, PointF b, StraightLineF line)
    {
        if (line == null) throw new ArgumentNullException(nameof(line), "line should noe be null.");
        return (line.A * a.X + line.B * a.Y + line.C) * (line.A * b.X + line.B * b.Y + line.C) > 0;
    }

    /// <summary>
    /// Tests if 2 line are intersection.
    /// </summary>
    /// <param name="a">The first line.</param>
    /// <param name="b">The second line.</param>
    /// <param name="p">The point intersected.</param>
    /// <returns>true if they are intersected; otherwise, false.</returns>
    public static bool IsIntersected(StraightLine a, StraightLine b, out DoublePoint2D p)
    {
        var d = a.A * b.B - b.A * a.B;
        if (Math.Abs(d) < InternalHelper.DoubleAccuracy)
        {
            p = new();
            return false;
        }

        p = new(
            (b.C * a.B - a.C * b.B) / d,
            (b.A * a.C - a.A * b.C) / d);
        return true;
    }

    /// <summary>
    /// Tests if 2 line are intersected.
    /// </summary>
    /// <param name="a">The first line.</param>
    /// <param name="b">The second line.</param>
    /// <returns>true if they are intersected; otherwise, false.</returns>
    public static bool IsIntersected(StraightLine a, StraightLine b)
        => IsIntersected(a, b, out _);

    /// <summary>
    /// Tests if 2 line are intersection.
    /// </summary>
    /// <param name="a">The first line.</param>
    /// <param name="b">The second line.</param>
    /// <param name="p">The point intersected.</param>
    /// <returns>true if they are intersected; otherwise, false.</returns>
    public static bool IsIntersected(StraightLineF a, StraightLineF b, out PointF p)
    {
        var d = a.A * b.B - b.A * a.B;
        if (Math.Abs(d) < InternalHelper.DoubleAccuracy)
        {
            p = new();
            return false;
        }

        p = new(
            (b.C * a.B - a.C * b.B) / d,
            (b.A * a.C - a.A * b.C) / d);
        return true;
    }

    /// <summary>
    /// Tests if 2 line are intersected.
    /// </summary>
    /// <param name="a">The first line.</param>
    /// <param name="b">The second line.</param>
    /// <returns>true if they are intersected; otherwise, false.</returns>
    public static bool IsIntersected(StraightLineF a, StraightLineF b)
        => IsIntersected(a, b, out _);

    /// <summary>
    /// Gets the line relected.
    /// </summary>
    /// <param name="a">Mirror.</param>
    /// <param name="b">Incoming light.</param>
    /// <returns>The output light.</returns>
    public static StraightLine Reflect(StraightLine a, StraightLine b)
    {
        var i = a.B * b.B + a.A * b.A;
        var j = b.A * a.B - a.A * b.B;
        var m = (i * a.B + j * a.A) / (a.B * a.B + a.A * a.A);
        var n = (j * a.B - i * a.A) / (a.B * a.B + a.A * a.A);
        if (Math.Abs(a.A * b.B - b.A * a.B) < InternalHelper.DoubleAccuracy)
            return new(b.A, b.B, b.C);
        var x = (a.B * b.C - b.B * a.C) / (a.A * b.B - b.A * a.B);
        var y = (b.A * a.C - a.A * b.C) / (a.A * b.B - b.A * a.B);
        return new(n, -m, m * y - x * n);
    }

    /// <summary>
    /// Gets the line relected.
    /// </summary>
    /// <param name="a">Mirror.</param>
    /// <param name="b">Incoming light.</param>
    /// <returns>The output light.</returns>
    public static StraightLineF Reflect(StraightLineF a, StraightLineF b)
    {
        var i = a.B * b.B + a.A * b.A;
        var j = b.A * a.B - a.A * b.B;
        var m = (i * a.B + j * a.A) / (a.B * a.B + a.A * a.A);
        var n = (j * a.B - i * a.A) / (a.B * a.B + a.A * a.A);
        if (Math.Abs(a.A * b.B - b.A * a.B) < InternalHelper.DoubleAccuracy)
            return new(b.A, b.B, b.C);
        var x = (a.B * b.C - b.B * a.C) / (a.A * b.B - b.A * a.B);
        var y = (b.A * a.C - a.A * b.C) / (a.A * b.B - b.A * a.B);
        return new(n, -m, m * y - x * n);
    }

    /// <summary>
    /// Tests if the polygon is simple.
    /// </summary>
    /// <param name="polygon">The polygon. Requires to input by anticlockwise.</param>
    /// <returns>true if it is a simple polygon; otherwise, false.</returns>
    /// <exception cref="ArgumentException">polygon was invalid because its points are less than 3.</exception>
    /// <exception cref="ArgumentNullException">polygon was null.</exception>
    public static bool IsSimple(DoublePoint2D[] polygon)
    {
        if (polygon == null) throw new ArgumentNullException(nameof(polygon), "polygon should not be null.");
        if (polygon.Length < 3) throw new ArgumentException("polygon is invalid because the points are less than 3.", nameof(polygon));
        int cn;
        LineSegment l1 = new();
        LineSegment l2 = new();
        var count = polygon.Length;
        for (var i = 0; i < count; i++)
        {
            l1.Start = polygon[i];
            l1.End = polygon[(i + 1) % count];
            cn = count - 3;
            while (cn != 0)
            {
                l2.Start = polygon[(i + 2) % count];
                l2.End = polygon[(i + 3) % count];
                if (IsIntersected(l1, l2))
                    break;
                cn--;
            }

            if (cn != 0) return false;
        }

        return true;
    }

    /// <summary>
    /// Tests if the polygon is simple.
    /// </summary>
    /// <param name="polygon">The polygon. Requires to input by anticlockwise.</param>
    /// <returns>true if it is a simple polygon; otherwise, false.</returns>
    public static bool IsSimple(IList<DoublePoint2D> polygon)
    {
        int cn;
        LineSegment l1 = new();
        LineSegment l2 = new();
        var count = polygon.Count;
        for (var i = 0; i < count; i++)
        {
            l1.Start = polygon[i];
            l1.End = polygon[(i + 1) % count];
            cn = count - 3;
            while (cn != 0)
            {
                l2.Start = polygon[(i + 2) % count];
                l2.End = polygon[(i + 3) % count];
                if (IsIntersected(l1, l2))
                    break;
                cn--;
            }

            if (cn != 0) return false;
        }

        return true;
    }

    /// <summary>
    /// Tests if the polygon is simple.
    /// </summary>
    /// <param name="polygon">The polygon. Requires to input by anticlockwise.</param>
    /// <returns>true if it is a simple polygon; otherwise, false.</returns>
    /// <exception cref="ArgumentException">polygon was invalid because its points are less than 3.</exception>
    /// <exception cref="ArgumentNullException">polygon was null.</exception>
    public static bool IsSimple(PointF[] polygon)
    {
        if (polygon == null) throw new ArgumentNullException(nameof(polygon), "polygon should not be null.");
        if (polygon.Length < 3) throw new ArgumentException("polygon is invalid because the points are less than 3.", nameof(polygon));
        int cn;
        LineSegmentF l1 = new();
        LineSegmentF l2 = new();
        var count = polygon.Length;
        for (var i = 0; i < count; i++)
        {
            l1.Start = polygon[i];
            l1.End = polygon[(i + 1) % count];
            cn = count - 3;
            while (cn != 0)
            {
                l2.Start = polygon[(i + 2) % count];
                l2.End = polygon[(i + 3) % count];
                if (IsIntersected(l1, l2))
                    break;
                cn--;
            }

            if (cn != 0) return false;
        }

        return true;
    }

    /// <summary>
    /// Tests if the polygon is simple.
    /// </summary>
    /// <param name="polygon">The polygon. Requires to input by anticlockwise.</param>
    /// <returns>true if it is a simple polygon; otherwise, false.</returns>
    public static bool IsSimple(IList<PointF> polygon)
    {
        int cn;
        LineSegmentF l1 = new();
        LineSegmentF l2 = new();
        var count = polygon.Count;
        for (var i = 0; i < count; i++)
        {
            l1.Start = polygon[i];
            l1.End = polygon[(i + 1) % count];
            cn = count - 3;
            while (cn != 0)
            {
                l2.Start = polygon[(i + 2) % count];
                l2.End = polygon[(i + 3) % count];
                if (IsIntersected(l1, l2))
                    break;
                cn--;
            }

            if (cn != 0) return false;
        }

        return true;
    }

    /// <summary>
    /// Gets convexity for each point in the polygon.
    /// </summary>
    /// <param name="polygon">The polygon.</param>
    /// <returns>The boolean array about if each point is convexity.</returns>
    public static IList<bool> Convexity(DoublePoint2D[] polygon)
    {
        var index = 0;
        var len = polygon.Length;
        var point = polygon[0];
        var list = new List<bool>();
        for (var i = 1; i < len; i++)
        {
            list.Add(false);
            if (polygon[i].Y < point.Y || (polygon[i].Y == point.Y && polygon[i].X < point.X))
            {
                point = polygon[i];
                index = i;
            }
        }

        var count = len - 1;
        list[index] = true;
        while (count > 0)
        {
            if (CrossProduct(polygon[(index + 1) % len], polygon[(index + 2) % len], polygon[index]) >= 0)
                list[(index + 1) % len] = true;
            else
                list[(index + 1) % len] = false;
            index++;
            count--;
        }

        return list;
    }

    /// <summary>
    /// Tests if the polygon is convex.
    /// </summary>
    /// <param name="polygon">The polygon.</param>
    /// <returns>true if it is a convex polygon.</returns>
    public static bool IsConvex(DoublePoint2D[] polygon)
        => !Convexity(polygon).Any(ele => !ele);

    /// <summary>
    /// Computes the area of the polygon.
    /// </summary>
    /// <param name="polygon">The polygon.</param>
    /// <returns>The area of the polygon. Greater than 0 if anticlockwise; less than 0 if clockwise.</returns>
    public static double Area(DoublePoint2D[] polygon)
    {
        var count = polygon.Length;
        if (count < 3) return 0;
        var s = polygon[0].Y * (polygon[count - 1].X - polygon[1].X);
        for (var i = 1; i < count; i++)
        {
            s += polygon[i].Y * (polygon[i - 1].X - polygon[(i + 1) % count].X);
        }

        return s / 2;
    }

    /// <summary>
    /// Computes the area of the polygon.
    /// </summary>
    /// <param name="polygon">The polygon.</param>
    /// <returns>The area of the polygon. Greater than 0 if anticlockwise; less than 0 if clockwise.</returns>
    public static double Area(IList<DoublePoint2D> polygon)
    {
        var count = polygon.Count;
        if (count < 3) return 0;
        var s = polygon[0].Y * (polygon[count - 1].X - polygon[1].X);
        for (var i = 1; i < count; i++)
        {
            s += polygon[i].Y * (polygon[i - 1].X - polygon[(i + 1) % count].X);
        }

        return s / 2;
    }

    /// <summary>
    /// Computes the absolute area of the polygon.
    /// </summary>
    /// <param name="polygon">The polygon.</param>
    /// <returns>The area of the polygon. Greater than 0 if anticlockwise; less than 0 if clockwise.</returns>
    public static double AbsArea(DoublePoint2D[] polygon)
        => Math.Abs(Area(polygon));

    /// <summary>
    /// Computes the absolute area of the polygon.
    /// </summary>
    /// <param name="polygon">The polygon.</param>
    /// <returns>The area of the polygon. Greater than 0 if anticlockwise; less than 0 if clockwise.</returns>
    public static double AbsArea(IList<DoublePoint2D> polygon)
        => Math.Abs(Area(polygon));


    /// <summary>
    /// Computes the area of the polygon.
    /// </summary>
    /// <param name="polygon">The polygon.</param>
    /// <returns>The area of the polygon. Greater than 0 if anticlockwise; less than 0 if clockwise.</returns>
    public static float Area(PointF[] polygon)
    {
        var count = polygon.Length;
        if (count < 3) return 0;
        var s = polygon[0].Y * (polygon[count - 1].X - polygon[1].X);
        for (var i = 1; i < count; i++)
        {
            s += polygon[i].Y * (polygon[i - 1].X - polygon[(i + 1) % count].X);
        }

        return s / 2;
    }

    /// <summary>
    /// Computes the area of the polygon.
    /// </summary>
    /// <param name="polygon">The polygon.</param>
    /// <returns>The area of the polygon. Greater than 0 if anticlockwise; less than 0 if clockwise.</returns>
    public static float Area(IList<PointF> polygon)
    {
        var count = polygon.Count;
        if (count < 3) return 0;
        var s = polygon[0].Y * (polygon[count - 1].X - polygon[1].X);
        for (var i = 1; i < count; i++)
        {
            s += polygon[i].Y * (polygon[i - 1].X - polygon[(i + 1) % count].X);
        }

        return s / 2;
    }

    /// <summary>
    /// Computes the absolute area of the polygon.
    /// </summary>
    /// <param name="polygon">The polygon.</param>
    /// <returns>The area of the polygon. Greater than 0 if anticlockwise; less than 0 if clockwise.</returns>
    public static float AbsArea(PointF[] polygon)
        => Math.Abs(Area(polygon));

    /// <summary>
    /// Computes the absolute area of the polygon.
    /// </summary>
    /// <param name="polygon">The polygon.</param>
    /// <returns>The area of the polygon. Greater than 0 if anticlockwise; less than 0 if clockwise.</returns>
    public static float AbsArea(IList<PointF> polygon)
        => Math.Abs(Area(polygon));

    /// <summary>
    /// Tests if the points in polygon is anticlockwise.
    /// </summary>
    /// <param name="polygon">The polygon.</param>
    /// <returns>true if it is anticlockwise; otherwise, false.</returns>
    public static bool IsAnticlockwise(DoublePoint2D[] polygon)
        => Area(polygon) > 0;

    /// <summary>
    /// Tests if the points in polygon is anticlockwise.
    /// </summary>
    /// <param name="polygon">The polygon.</param>
    /// <returns>true if it is anticlockwise; otherwise, false.</returns>
    public static bool IsAnticlockwise(IList<DoublePoint2D> polygon)
        => Area(polygon) > 0;

    /// <summary>
    /// Tests if the points in polygon is anticlockwise.
    /// </summary>
    /// <param name="polygon">The polygon.</param>
    /// <returns>true if it is anticlockwise; otherwise, false.</returns>
    public static bool IsAnticlockwise(PointF[] polygon)
        => Area(polygon) > 0;

    /// <summary>
    /// Tests if the points in polygon is anticlockwise.
    /// </summary>
    /// <param name="polygon">The polygon.</param>
    /// <returns>true if it is anticlockwise; otherwise, false.</returns>
    public static bool IsAnticlockwise(IList<PointF> polygon)
        => Area(polygon) > 0;

    /// <summary>
    /// Gets the center of gravity of the polygon.
    /// </summary>
    /// <param name="polygon">The polygon.</param>
    /// <returns>The center of gravity of the polygon.</returns>
    /// <exception cref="InvalidOperationException">Only supports simple polygon.</exception>
    public static DoublePoint2D CenterOfGravity(DoublePoint2D[] polygon)
    {
        if (!IsSimple(polygon)) throw new InvalidOperationException("Only supports simple polygon.", new NotImplementedException("Only supports simple polygon."));
        DoublePoint2D p1;
        DoublePoint2D p2;
        double xtr, ytr, wtr, xtl, ytl, wtl;
        xtr = ytr = wtr = xtl = ytl = wtl = 0d;
        var count = polygon.Length;
        for (int i = 0; i < count; i++)
        {
            p1 = polygon[i];
            p2 = polygon[(i + 1) % count];
            AddRegion(p1.X, p1.Y, p2.X, p2.Y, ref xtr, ref ytr, ref wtr, ref xtl, ref ytl, ref wtl);
        }

        return new(
            (wtr * xtr + wtl * xtl) / (wtr + wtl),
            (wtr * ytr + wtl * ytl) / (wtr + wtl));
    }

    /// <summary>
    /// Gets the center of gravity of the polygon.
    /// </summary>
    /// <param name="polygon">The polygon.</param>
    /// <returns>The center of gravity of the polygon.</returns>
    /// <exception cref="InvalidOperationException">Only supports simple polygon.</exception>
    public static DoublePoint2D CenterOfGravity(IList<DoublePoint2D> polygon)
    {
        if (!IsSimple(polygon)) throw new InvalidOperationException("Only supports simple polygon.", new NotImplementedException("Only supports simple polygon."));
        double xtr, ytr, wtr, xtl, ytl, wtl;
        xtr = ytr = wtr = xtl = ytl = wtl = 0d;
        DoublePoint2D p1;
        DoublePoint2D p2;
        var count = polygon.Count;
        for (int i = 0; i < count; i++)
        {
            p1 = polygon[i];
            p2 = polygon[(i + 1) % count];
            AddRegion(p1.X, p1.Y, p2.X, p2.Y, ref xtr, ref ytr, ref wtr, ref xtl, ref ytl, ref wtl);
        }

        return new(
            (wtr * xtr + wtl * xtl) / (wtr + wtl),
            (wtr * ytr + wtl * ytl) / (wtr + wtl));
    }

    /// <summary>
    /// Gets the center of gravity of the polygon.
    /// </summary>
    /// <param name="polygon">The polygon.</param>
    /// <returns>The center of gravity of the polygon.</returns>
    /// <exception cref="InvalidOperationException">Only supports simple polygon.</exception>
    public static PointF CenterOfGravity(PointF[] polygon)
    {
        if (!IsSimple(polygon)) throw new InvalidOperationException("Only supports simple polygon.", new NotImplementedException("Only supports simple polygon."));
        PointF p1;
        PointF p2;
        double xtr, ytr, wtr, xtl, ytl, wtl;
        xtr = ytr = wtr = xtl = ytl = wtl = 0f;
        var count = polygon.Length;
        for (int i = 0; i < count; i++)
        {
            p1 = polygon[i];
            p2 = polygon[(i + 1) % count];
            AddRegion(p1.X, p1.Y, p2.X, p2.Y, ref xtr, ref ytr, ref wtr, ref xtl, ref ytl, ref wtl);
        }

        return new(
            (float)(wtr * xtr + wtl * xtl) / (float)(wtr + wtl),
            (float)(wtr * ytr + wtl * ytl) / (float)(wtr + wtl));
    }

    /// <summary>
    /// Gets the center of gravity of the polygon.
    /// </summary>
    /// <param name="polygon">The polygon.</param>
    /// <returns>The center of gravity of the polygon.</returns>
    /// <exception cref="InvalidOperationException">Only supports simple polygon.</exception>
    public static PointF CenterOfGravity(IList<PointF> polygon)
    {
        if (!IsSimple(polygon)) throw new InvalidOperationException("Only supports simple polygon.", new NotImplementedException("Only supports simple polygon."));
        double xtr, ytr, wtr, xtl, ytl, wtl;
        xtr = ytr = wtr = xtl = ytl = wtl = 0f;
        PointF p1;
        PointF p2;
        var count = polygon.Count;
        for (int i = 0; i < count; i++)
        {
            p1 = polygon[i];
            p2 = polygon[(i + 1) % count];
            AddRegion(p1.X, p1.Y, p2.X, p2.Y, ref xtr, ref ytr, ref wtr, ref xtl, ref ytl, ref wtl);
        }

        return new(
            (float)(wtr * xtr + wtl * xtl) / (float)(wtr + wtl),
            (float)(wtr * ytr + wtl * ytl) / (float)(wtr + wtl));
    }

    private static void AddPosPart(double x, double y, double w, ref double xtr, ref double ytr, ref double wtr)
    {
        if (Math.Abs(wtr + w) < InternalHelper.DoubleAccuracy) return;
        xtr = (wtr * xtr + w * x) / (wtr + w);
        ytr = (wtr * ytr + w * y) / (wtr + w);
        wtr = w + wtr;
    }

    private static void AddNegPart(double x, double y, double w, ref double xtl, ref double ytl, ref double wtl)
    {
        if (Math.Abs(wtl + w) < InternalHelper.DoubleAccuracy) return;
        xtl = (wtl * xtl + w * x) / (wtl + w);
        ytl = (wtl * ytl + w * y) / (wtl + w);
        wtl = w + wtl;
    }

    private static void AddRegion(double x1, double y1, double x2, double y2, ref double xtr, ref double ytr, ref double wtr, ref double xtl, ref double ytl, ref double wtl)
    {
        if (Math.Abs(x1 - x2) < InternalHelper.DoubleAccuracy) return;
        if (x2 > x1)
        {
            AddPosPart((x2 + x1) / 2, y1 / 2, (x2 - x1) * y1, ref xtr, ref ytr, ref wtr);
            AddPosPart((x1 + x2 + x2) / 3, (y1 + y1 + y2) / 3, (x2 - x1) * (y2 - y1) / 2, ref xtr, ref ytr, ref wtr);
        }
        else
        {
            AddNegPart((x2 + x1) / 2, y1 / 2, (x2 - x1) * y1, ref xtl, ref ytl, ref wtl);
            AddNegPart((x1 + x2 + x2) / 3, (y1 + y1 + y2) / 3, (x2 - x1) * (y2 - y1) / 2, ref xtl, ref ytl, ref wtl);
        }
    }

    /// <summary>
    /// Gets the points of contact from the specific point to the polygon.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="polygon">The polygon.</param>
    /// <param name="right">The right point of contact.</param>
    /// <param name="left">The left point of contact.</param>
    /// <exception cref="ArgumentException">polygon was invalid because its points are less than 3.</exception>
    /// <exception cref="ArgumentNullException">polygon was null.</exception>
    /// <exception cref="InvalidOperationException">polygon was not simple.</exception>
    public static void PointTangentPoly(DoublePoint2D point, DoublePoint2D[] polygon, out DoublePoint2D right, out DoublePoint2D left)
    {
        if (!IsSimple(polygon)) throw new InvalidOperationException("Only supports simple polygon.", new NotImplementedException("Only supports simple polygon."));
        if (point == null) point = new();
        bool isLeft, isRight;
        var count = polygon.Length;
        LineSegment leftSegment = new();
        LineSegment rightSegment = new();
        right = polygon[0];
        left = polygon[0];
        for (int i = 1; i < count; i++)
        {
            leftSegment.Start = polygon[(i + count - 1) % count];
            leftSegment.End = polygon[i];
            rightSegment.Start = polygon[i];
            rightSegment.End = polygon[(i + 1) % count];
            isLeft = CrossProduct(leftSegment.End, point, leftSegment.Start) >= 0;
            isRight = CrossProduct(rightSegment.End, point, rightSegment.Start) >= 0;
            if (!isLeft && isRight)
            {
                if (CrossProduct(polygon[i], right, point) > 0) right = polygon[i];
            }

            if (isLeft && !isRight)
            {
                if (CrossProduct(left, polygon[i], point) > 0) left = polygon[i];
            }
        }

        return;
    }

    /// <summary>
    /// Gets the points of contact from the specific point to the polygon.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="polygon">The polygon.</param>
    /// <param name="right">The right point of contact.</param>
    /// <param name="left">The left point of contact.</param>
    /// <exception cref="ArgumentException">polygon was invalid because its points are less than 3.</exception>
    /// <exception cref="ArgumentNullException">polygon was null.</exception>
    /// <exception cref="InvalidOperationException">polygon was not simple.</exception>
    public static void PointTangentPoly(DoublePoint2D point, IList<DoublePoint2D> polygon, out DoublePoint2D right, out DoublePoint2D left)
    {
        if (!IsSimple(polygon)) throw new InvalidOperationException("Only supports simple polygon.", new NotImplementedException("Only supports simple polygon."));
        if (point == null) point = new();
        bool isLeft, isRight;
        var count = polygon.Count;
        LineSegment leftSegment = new();
        LineSegment rightSegment = new();
        right = polygon[0];
        left = polygon[0];
        for (int i = 1; i < count; i++)
        {
            leftSegment.Start = polygon[(i + count - 1) % count];
            leftSegment.End = polygon[i];
            rightSegment.Start = polygon[i];
            rightSegment.End = polygon[(i + 1) % count];
            isLeft = CrossProduct(leftSegment.End, point, leftSegment.Start) >= 0;
            isRight = CrossProduct(rightSegment.End, point, rightSegment.Start) >= 0;
            if (!isLeft && isRight)
            {
                if (CrossProduct(polygon[i], right, point) > 0) right = polygon[i];
            }

            if (isLeft && !isRight)
            {
                if (CrossProduct(left, polygon[i], point) > 0) left = polygon[i];
            }
        }

        return;
    }

    /// <summary>
    /// Tests if the polygon has a core.
    /// </summary>
    /// <param name="polygon">The polygon.</param>
    /// <returns>A point of core; or null, if non-exists.</returns>
    /// <exception cref="ArgumentException">polygon was invalid because its points are less than 3.</exception>
    /// <exception cref="ArgumentNullException">polygon was null.</exception>
    /// <exception cref="InvalidOperationException">polygon was not simple.</exception>
    public static DoublePoint2D GetCore(DoublePoint2D[] polygon)
    {
        if (!IsSimple(polygon)) throw new InvalidOperationException("Only supports simple polygon.", new NotImplementedException("Only supports simple polygon."));
        LineSegment l = new();
        var lineset = new List<StraightLine>();
        var count = polygon.Length;
        DoublePoint2D p = null;
        int i, j, k;
        for (i = 0; i < count; i++)
        {
            lineset.Add(new StraightLine(polygon[i], polygon[(i + 1) % count]));
        }

        for (i = 0; i < count; i++)
        {
            for (j = 0; j < count; j++)
            {
                if (i == j) continue;
                if (IsIntersected(lineset[i], lineset[j], out p))
                {
                    for (k = 0; k < count; k++)
                    {
                        l.Start = polygon[k];
                        l.End = polygon[(k + 1) % count];
                        if (CrossProduct(p, l.End, l.Start) > 0) break;
                    }

                    if (k == count) break;
                }
            }

            if (j < count) break;
        }

        if (i >= count) return null;
        return p;
    }

    /// <summary>
    /// Tests if the polygon has a core.
    /// </summary>
    /// <param name="polygon">The polygon.</param>
    /// <returns>A point of core; or null, if non-exists.</returns>
    /// <exception cref="ArgumentException">polygon was invalid because its points are less than 3.</exception>
    /// <exception cref="ArgumentNullException">polygon was null.</exception>
    /// <exception cref="InvalidOperationException">polygon was not simple.</exception>
    public static DoublePoint2D GetCore(IList<DoublePoint2D> polygon)
    {
        if (!IsSimple(polygon)) throw new InvalidOperationException("Only supports simple polygon.", new NotImplementedException("Only supports simple polygon."));
        LineSegment l = new();
        var lineset = new List<StraightLine>();
        var count = polygon.Count;
        DoublePoint2D p = null;
        int i, j, k;
        for (i = 0; i < count; i++)
        {
            lineset.Add(new StraightLine(polygon[i], polygon[(i + 1) % count]));
        }

        for (i = 0; i < count; i++)
        {
            for (j = 0; j < count; j++)
            {
                if (i == j) continue;
                if (IsIntersected(lineset[i], lineset[j], out p))
                {
                    for (k = 0; k < count; k++)
                    {
                        l.Start = polygon[k];
                        l.End = polygon[(k + 1) % count];
                        if (CrossProduct(p, l.End, l.Start) > 0) break;
                    }

                    if (k == count) break;
                }
            }

            if (j < count) break;
        }

        if (i >= count) return null;
        return p;
    }

    /// <summary>
    /// Tests if the polygon has a core.
    /// </summary>
    /// <param name="polygon">The polygon.</param>
    /// <returns>true if has core; otherwise, false.</returns>
    public static bool HasCore(DoublePoint2D[] polygon)
        => GetCore(polygon) != null;

    /// <summary>
    /// Tests if the polygon has a core.
    /// </summary>
    /// <param name="polygon">The polygon.</param>
    /// <returns>true if has core; otherwise, false.</returns>
    public static bool HasCore(IList<DoublePoint2D> polygon)
        => GetCore(polygon) != null;

    /// <summary>
    /// Tests if the point is in the circle.
    /// </summary>
    /// <param name="point">The point to test.</param>
    /// <param name="center">The point of center of the circle.</param>
    /// <param name="radius">The radius of the circle.</param>
    /// <returns>true if the point is in the circle; otherwise, false.</returns>
    /// <exception cref="ArgumentException">radius was invalid.</exception>
    public static bool IsIn(DoublePoint2D point, DoublePoint2D center, double radius)
    {
        if (point == null) point = new();
        if (center == null) center = new();
        if (double.IsNaN(radius)) throw new ArgumentException("radius should be a valid number.", nameof(radius), new InvalidOperationException("radius is invalid."));
        var d = (point.X - center.X) * (point.X - center.X) + (point.Y - center.Y) * (point.Y - center.Y);
        var r = radius * radius;
        return d < r || Math.Abs(d - r) < InternalHelper.DoubleAccuracy;
    }

    /// <summary>
    /// Gets the rest point of the rectangle. The rectange is made by given 3 points.
    /// </summary>
    /// <param name="a">The first point.</param>
    /// <param name="b">The second point.</param>
    /// <param name="c">The third point.</param>
    /// <returns>The forth point; or null if failed.</returns>
    public static DoublePoint2D GetRestPoint(DoublePoint2D a, DoublePoint2D b, DoublePoint2D c)
    {
        if (a == null) a = new();
        if (b == null) b = new();
        if (c == null) c = new();
        if (Math.Abs(DotProduct(a, b, c)) < InternalHelper.DoubleAccuracy)
            return new DoublePoint2D(a.X + b.X - c.X, a.Y + b.Y - c.Y);
        if (Math.Abs(DotProduct(a, c, b)) < InternalHelper.DoubleAccuracy)
            return new DoublePoint2D(a.X + c.X - b.X, a.Y + c.Y - b.X);
        if (Math.Abs(DotProduct(c, b, a)) < InternalHelper.DoubleAccuracy)
            return new DoublePoint2D(c.X + b.X - a.X, c.Y + b.Y - a.Y);
        return null;
    }

    /// <summary>
    /// Gets the relationship between 2 circles.
    /// </summary>
    /// <param name="a">The first circle.</param>
    /// <param name="b">The second circle.</param>
    /// <returns>The relationship between 2 circles.</returns>
    /// <exception cref="ArgumentException">a or b was invalid.</exception>
    public static RelationshipBetweenCircles Relation(CoordinateCircle a, CoordinateCircle b)
    {
        if (a == null) a = new();
        if (b == null) b = new();
        var center1 = a.Center;
        var center2 = b.Center;
        var radius1 = a.Radius;
        var radius2 = b.Radius;
        if (double.IsNaN(radius1)) throw new ArgumentException("a radius should be a valid number.", nameof(a), new InvalidOperationException("radius1 is invalid."));
        if (double.IsNaN(radius2)) throw new ArgumentException("b radius should be a valid number.", nameof(b), new InvalidOperationException("radius2 is invalid."));
        var d = Math.Sqrt((center1.X - center2.X) * (center1.X - center2.X) + (center1.Y - center2.Y) * (center1.Y - center2.Y));
        if (d < InternalHelper.DoubleAccuracy && Math.Abs(radius1 - radius2) < InternalHelper.DoubleAccuracy)
            return RelationshipBetweenCircles.Congruence;
        if (Math.Abs(d - radius1 - radius2) < InternalHelper.DoubleAccuracy)
            return RelationshipBetweenCircles.ExternallyTangent;
        if (Math.Abs(d - Math.Abs(radius1 - radius2)) < InternalHelper.DoubleAccuracy)
            return RelationshipBetweenCircles.Inscribe;
        if (d > radius1 + radius2)
            return RelationshipBetweenCircles.Separation;
        if (d < Math.Abs(radius1 - radius2))
            return RelationshipBetweenCircles.Inclusion;
        if (Math.Abs(radius1 - radius2) < d && d < radius1 + radius2)
            return RelationshipBetweenCircles.Intersection;
        return (RelationshipBetweenCircles)7; // Error!
    }

    /// <summary>
    /// Gets the relationship between 2 circles.
    /// </summary>
    /// <param name="a">The first circle.</param>
    /// <param name="b">The second circle.</param>
    /// <returns>The relationship between 2 circles.</returns>
    /// <exception cref="ArgumentException">a or b was invalid.</exception>
    public static RelationshipBetweenCircles Relation(CoordinateCircleF a, CoordinateCircleF b)
    {
        if (a == null) a = new();
        if (b == null) b = new();
        var center1 = a.Center;
        var center2 = b.Center;
        var radius1 = a.Radius;
        var radius2 = b.Radius;
        if (float.IsNaN(radius1)) throw new ArgumentException("a radius should be a valid number.", nameof(a), new InvalidOperationException("radius1 is invalid."));
        if (float.IsNaN(radius2)) throw new ArgumentException("b radius should be a valid number.", nameof(b), new InvalidOperationException("radius2 is invalid."));
        var d = Math.Sqrt((center1.X - center2.X) * (center1.X - center2.X) + (center1.Y - center2.Y) * (center1.Y - center2.Y));
        if (d < InternalHelper.SingleAccuracy && Math.Abs(radius1 - radius2) < InternalHelper.SingleAccuracy)
            return RelationshipBetweenCircles.Congruence;
        if (Math.Abs(d - radius1 - radius2) < InternalHelper.SingleAccuracy)
            return RelationshipBetweenCircles.ExternallyTangent;
        if (Math.Abs(d - Math.Abs(radius1 - radius2)) < InternalHelper.SingleAccuracy)
            return RelationshipBetweenCircles.Inscribe;
        if (d > radius1 + radius2)
            return RelationshipBetweenCircles.Separation;
        if (d < Math.Abs(radius1 - radius2))
            return RelationshipBetweenCircles.Inclusion;
        if (Math.Abs(radius1 - radius2) < d && d < radius1 + radius2)
            return RelationshipBetweenCircles.Intersection;
        return (RelationshipBetweenCircles)7; // Error!
    }

    private static (DoublePoint2D, DoublePoint2D) Pointcuts(CoordinateCircle circle, DoublePoint2D point, double radius)
    {
        var a = point.X - circle.CenterX;
        var b = point.Y - circle.CenterY;
        var r = (a * a + b * b + circle.Radius * circle.Radius - radius * radius) / 2;
        var left = new DoublePoint2D();
        var right = new DoublePoint2D();
        if (a == 0 && b != 0)
        {
            left.Y = right.Y = r / b;
            left.X = Math.Sqrt(circle.Radius * circle.Radius - left.Y * left.Y);
            right.X = -left.X;
        }
        else if (a != 0 && b == 0)
        {
            left.X = right.X = r / a;
            left.Y = Math.Sqrt(circle.Radius * circle.Radius - left.X * right.X);
            right.Y = -left.Y;
        }
        else if (a != 0 && b != 0)
        {
            double delta;
            delta = b * b * r * r - (a * a + b * b) * (r * r - circle.Radius * circle.Radius * a * a);
            left.Y = (b * r + Math.Sqrt(delta)) / (a * a + b * b);
            right.Y = (b * r - Math.Sqrt(delta)) / (a * a + b * b);
            left.X = (r - b * left.Y) / a;
            right.X = (r - b * right.Y) / a;
        }

        left.X += left.X;
        left.Y += left.Y;
        right.X += left.X;
        right.Y += left.Y;
        return (left, right);
    }

    /// <summary>
    /// Computes the areas.
    /// </summary>
    /// <param name="a">The first circle.</param>
    /// <param name="b">The second circle.</param>
    /// <returns>The area.</returns>
    /// <exception cref="ArgumentException">a or b was invalid.</exception>
    public static double Area(CoordinateCircle a, CoordinateCircle b)
    {
        var rela = Relation(a, b);
        switch (rela)
        {
            case RelationshipBetweenCircles.Inclusion:
                return Math.Max(a.Area(), b.Area());
            case RelationshipBetweenCircles.Separation:
            case RelationshipBetweenCircles.ExternallyTangent:
            case RelationshipBetweenCircles.Inscribe:
                return a.Area() + b.Area();
            case RelationshipBetweenCircles.Congruence:
                return a.Area();
        }

        var (left, right) = Pointcuts(a, b.Center, b.Radius);
        var p1 = a.Center;
        var r1 = a.Radius;
        var p2 = b.Center;
        var r2 = b.Radius;
        if (r1 > r2)
        {
            var p3 = p1;
            p1 = p2;
            p2 = p3;
            var r3 = r1;
            r1 = r2;
            r2 = r3;
        }

        var width = p1.X - p2.X;
        var height = p1.Y - p2.Y;
        var rr = Math.Sqrt(width * width + height * height);
        var dx1 = left.X - p1.X;
        var dy1 = left.Y - p1.Y;
        var dx2 = right.X - p1.X;
        var dy2 = right.Y - p1.Y;
        var sita1 = Math.Acos((dx1 * dx2 + dy1 * dy2) / r1 / r1);
        dx1 = left.X - p2.X;
        dy1 = left.Y - p2.Y;
        dx2 = right.X - p2.X;
        dy2 = right.Y - p2.Y;
        var sita2 = Math.Acos((dx1 * dx2 + dy1 * dy2) / r2 / r2);
        return rr < r2
            ? r1 * r1 * (Math.PI - sita1 / 2 + Math.Sin(sita1) / 2) + r2 * r2 * (sita2 - Math.Sin(sita2)) / 2
            : (r1 * r1 * (sita1 - Math.Sin(sita1)) + r2 * r2 * (sita2 - Math.Sin(sita2))) / 2;
    }

    /// <summary>
    /// Gets the pointcuts.
    /// </summary>
    /// <param name="circle">The circle.</param>
    /// <param name="point">The point.</param>
    /// <returns>The 2 pointcuts.</returns>
    public static (DoublePoint2D, DoublePoint2D) Pointcuts(CoordinateCircle circle, DoublePoint2D point)
    {
        var p = new DoublePoint2D((circle.CenterX + point.X) / 2, (circle.CenterY + point.Y) / 2);
        var dx = p.X - circle.CenterX;
        var dy = p.Y - circle.CenterY;
        var r = Math.Sqrt(dx * dx + dy * dy);
        return Pointcuts(circle, p, r);
    }

    /// <summary>
    /// Gets the pointcuts.
    /// </summary>
    /// <param name="circle">The circle.</param>
    /// <param name="line">The straight line.</param>
    /// <returns>The pointcuts. It may only contains 1 or even none.</returns>
    public static (DoublePoint2D, DoublePoint2D) Pointcuts(CoordinateCircle circle, StraightLine line)
    {
        int res;
        var a = line.A;
        var b = line.B;
        var c = line.C + a * circle.CenterX + b * circle.CenterY;
        var left = new DoublePoint2D();
        var right = new DoublePoint2D();
        var r2 = circle.Radius * circle.Radius;
        if (a == 0 && b != 0)
        {
            var tmp = -c / b;
            if (r2 < tmp * tmp) return (null, null);
            if (r2 == tmp * tmp)
            {
                res = 1;
                left.Y = tmp;
                left.X = 0;
            }
            else
            {
                res = 2;
                left.Y = right.Y = tmp;
                left.X = Math.Sqrt(r2 - tmp * tmp);
                right.X = -left.X;
            }
        }
        else if (a != 0 && b == 0)
        {
            var tmp = -c / a;
            if (r2 < tmp * tmp) return (null, null);
            if (r2 == tmp * tmp)
            {
                res = 1;
                left.X = tmp;
                left.Y = 0;
            }
            else
            {
                res = 2;
                left.X = right.X = tmp;
                left.Y = Math.Sqrt(r2 - tmp * tmp);
                right.Y = -left.Y;
            }
        }
        else if (a != 0 && b != 0)
        {
            double delta;
            delta = b * b * c * c - (a * a + b * b) * (c * c - a * a * r2);
            if (delta < 0) return (null, null);
            if (delta == 0)
            {
                res = 1;
                left.Y = -b * c / (a * a + b * b);
                left.X = (-c - b * left.Y) / a;
            }
            else
            {
                res = 2;
                left.Y = (-b * c + Math.Sqrt(delta)) / (a * a + b * b);
                right.Y = (-b * c - Math.Sqrt(delta)) / (a * a + b * b);
                left.X = (-c - b * left.Y) / a;
                right.X = (-c - b * right.Y) / a;
            }
        }
        else
        {
            return (null, null);
        }

        left.X += circle.CenterX;
        left.Y += circle.CenterY;
        if (res == 1) return (left, null);
        right.X += circle.CenterX;
        right.Y += circle.CenterY;
        return (left, right);
    }
}
