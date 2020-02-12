﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using Trivial.Maths;

namespace Trivial.Geography
{
    /// <summary>
    /// Latitudes.
    /// </summary>
    public enum Latitudes
    {
        /// <summary>
        /// The equator.
        /// </summary>
        Equator = 0,

        /// <summary>
        /// North latitudes.
        /// </summary>
        North = 1,

        /// <summary>
        /// South latitudes.
        /// </summary>
        South = 2
    }

    /// <summary>
    /// Longitudes.
    /// </summary>
    public enum Longitudes
    {
        /// <summary>
        /// Prime meridian.
        /// </summary>
        PrimeMeridian = 0,

        /// <summary>
        /// East longitude.
        /// </summary>
        East = 1,

        /// <summary>
        /// West longitude.
        /// </summary>
        West = 2,

        /// <summary>
        /// Calendar line.
        /// </summary>
        CalendarLine = 3
    }

    /// <summary>
    /// Latitude.
    /// </summary>
    public struct Latitude : IEquatable<Latitude>
    {
        /// <summary>
        /// Latitude model.
        /// </summary>
        public class Model : Angle.Model
        {
            /// <summary>
            /// Initializes a new instance of the Latitude.Model class.
            /// </summary>
            public Model() : base(new Angle.BoundaryOptions(90, true, Angle.RectifyModes.Bounce))
            {
            }

            /// <summary>
            /// Initializes a new instance of the Latitude.Model class.
            /// </summary>
            /// <param name="degrees">The total degrees.</param>
            public Model(double degrees) : base(degrees, new Angle.BoundaryOptions(90, true, Angle.RectifyModes.Bounce))
            {
            }

            /// <summary>
            /// Initializes a new instance of the Latitude.Model class.
            /// </summary>
            /// <param name="degree">The degree part.</param>
            /// <param name="minute">The minute part.</param>
            /// <param name="second">The second part.</param>
            public Model(int degree, int minute, float second = 0) : base(degree, minute, second, new Angle.BoundaryOptions(90, true, Angle.RectifyModes.Bounce))
            {
            }

            /// <summary>
            /// Gets or sets the latitue zone.
            /// </summary>
            public Latitudes Type
            {
                get
                {
                    if (IsZero) return Latitudes.Equator;
                    return Positive ? Latitudes.North : Latitudes.South;
                }

                set
                {
                    switch (value)
                    {
                        case Latitudes.Equator:
                            Degrees = 0;
                            break;
                        case Latitudes.North:
                            if (IsNegative) Degree = -Degree;
                            break;
                        case Latitudes.South:
                            if (Positive) Degree = -Degree;
                            break;
                    }
                }
            }

            /// <summary>
            /// Indicates whether the current object is equal to another object of the same type.
            /// </summary>
            /// <returns>
            /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
            /// </returns>
            /// <param name="other">An object to compare with this object.</param>
            public override bool Equals(object other)
            {
                if (other is null) return false;
                if (other is IAngle a) return Degrees.Equals(a.Degrees);
                if (other is Latitude l) return Degrees.Equals(l.Value.Degrees);
                if (other is double d) return Degrees.Equals(d);
                return Degrees.Equals(other);
            }

            /// <summary>
            /// Returns the hash code for this instance.
            /// </summary>
            /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            /// <summary>
            /// Returns the latitude string value of this instance.
            /// </summary>
            /// <returns>A System.String containing this latitude.</returns>
            public override string ToString()
            {
                if (IsZero) return NumberSymbols.NumberZero + Angle.Symbols.DegreeUnit;
                return this.ToAbsAngleString() + (Positive ? "N" : "S");
            }

            /// <summary>
            /// Compares two latitudes to indicate if they are same.
            /// leftValue == rightValue
            /// </summary>
            /// <param name="leftValue">The left value to compare.</param>
            /// <param name="rightValue">The right value to compare.</param>
            /// <returns>true if they are same; otherwise, false.</returns>
            public static bool operator ==(Model leftValue, Latitude rightValue)
            {
                if (ReferenceEquals(leftValue, rightValue)) return true;
                return !(leftValue is null) && leftValue.Equals(rightValue);
            }

            /// <summary>
            /// Compares two latitudes to indicate if they are same.
            /// leftValue == rightValue
            /// </summary>
            /// <param name="leftValue">The left value to compare.</param>
            /// <param name="rightValue">The right value to compare.</param>
            /// <returns>true if they are same; otherwise, false.</returns>
            public static bool operator ==(Model leftValue, Model rightValue)
            {
                if (ReferenceEquals(leftValue, rightValue)) return true;
                return !(leftValue is null) && leftValue.Equals(rightValue);
            }

            /// <summary>
            /// Compares two latitudes to indicate if they are different.
            /// leftValue != rightValue
            /// </summary>
            /// <param name="leftValue">The left value to compare.</param>
            /// <param name="rightValue">The right value to compare.</param>
            /// <returns>true if they are different; otherwise, false.</returns>
            public static bool operator !=(Model leftValue, Latitude rightValue)
            {
                if (ReferenceEquals(leftValue, rightValue)) return false;
                return leftValue is null || !leftValue.Equals(rightValue);
            }

            /// <summary>
            /// Compares two latitudes to indicate if they are different.
            /// leftValue != rightValue
            /// </summary>
            /// <param name="leftValue">The left value to compare.</param>
            /// <param name="rightValue">The right value to compare.</param>
            /// <returns>true if they are different; otherwise, false.</returns>
            public static bool operator !=(Model leftValue, Model rightValue)
            {
                if (ReferenceEquals(leftValue, rightValue)) return false;
                return leftValue is null || !leftValue.Equals(rightValue);
            }

            /// <summary>
            /// Converts a latitude to its model.
            /// </summary>
            /// <param name="value">The instance.</param>
            public static implicit operator Model(Latitude value)
            {
                return new Model { Degrees = value.Value.Degrees };
            }
        }

        /// <summary>
        /// Initializes a new instance of the Latitude struct.
        /// </summary>
        /// <param name="type">The latitude type.</param>
        /// <param name="degree">The degree part.</param>
        /// <param name="minute">The minute part.</param>
        /// <param name="second">The second part.</param>
        public Latitude(Latitudes type, int degree, int minute, float second) : this(GetDegrees(type, Angle.GetDegrees(degree, minute, second)))
        {
        }

        /// <summary>
        /// Initializes a new instance of the Latitude struct.
        /// </summary>
        /// <param name="degree">The degree part.</param>
        /// <param name="minute">The minute part.</param>
        /// <param name="second">The second part.</param>
        public Latitude(int degree, int minute, float second) : this(Angle.GetDegrees(degree, minute, second))
        {
        }

        /// <summary>
        /// Initializes a new instance of the Latitude struct.
        /// </summary>
        /// <param name="degrees">The total degrees.</param>
        public Latitude(double degrees)
        {
            var i = (int)degrees / 180;
            if (i != 0) degrees += i * 180;
            if (degrees > 90) degrees = 180 - degrees;
            else if (degrees < -90) degrees = -180 - degrees;
            Value = new Angle(degrees);
            if (degrees > 0) Type = Latitudes.North;
            else if (degrees < 0) Type = Latitudes.South;
            else Type = Latitudes.Equator;
        }

        /// <summary>
        /// Initializes a new instance of the Latitude struct.
        /// </summary>
        /// <param name="angle">The angle.</param>
        public Latitude(Angle angle) : this(angle.Degrees)
        {
        }

        /// <summary>
        /// Initializes a new instance of the Latitude struct.
        /// </summary>
        /// <param name="type">The latitude type.</param>
        /// <param name="degrees">The total degrees.</param>
        public Latitude(Latitudes type, double degrees) : this(GetDegrees(type, degrees))
        {
        }

        /// <summary>
        /// Gets the latitue zone.
        /// </summary>
        public Latitudes Type { get; }

        /// <summary>
        /// The angle value in the latitude type.
        /// </summary>
        public Angle Value { get; }

        /// <summary>
        /// The angle degrees of the latitude.
        /// </summary>
        public double Degrees => Value.Degrees;

        /// <summary>
        /// The absolute angle degrees in the latitude type.
        /// </summary>
        public double AbsDegrees => Value.AbsDegrees;

        /// <summary>
        /// The angle degrees of the latitude.
        /// </summary>
        public int Degree => Value.Degree;

        /// <summary>
        /// The absolute angle degrees in the latitude type.
        /// </summary>
        public int AbsDegree => Value.Degree;

        /// <summary>
        /// Gets the arcminute of the latitude.
        /// </summary>
        public int Arcminute => Value.Arcminute;

        /// <summary>
        /// Gets the arcsecond of the latitude.
        /// </summary>
        public float Arcsecond => Value.Arcsecond;

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return Value.Degrees.GetHashCode();
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public override bool Equals(object other)
        {
            if (other is null) return false;
            if (other is Latitude l) return Value.Degrees.Equals(l.Value.Degrees);
            if (other is Model m) return Value.Degrees.Equals(m.Degrees);
            return Value.Equals(other);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(Latitude other)
        {
            return Value == other.Value;
        }

        /// <summary>
        /// Returns the latitude string value of this instance.
        /// </summary>
        /// <returns>A System.String containing this latitude.</returns>
        public override string ToString()
        {
            if (Value.IsZero) return NumberSymbols.NumberZero + Angle.Symbols.DegreeUnit;
            return Value.ToAbsAngleString() + (Value.Positive ? "N" : "S");
        }

        /// <summary>
        /// Compares two latitudes to indicate if they are same.
        /// leftValue == rightValue
        /// </summary>
        /// <param name="leftValue">The left value to compare.</param>
        /// <param name="rightValue">The right value to compare.</param>
        /// <returns>true if they are same; otherwise, false.</returns>
        public static bool operator ==(Latitude leftValue, Latitude rightValue)
        {
            if (ReferenceEquals(leftValue, rightValue)) return true;
            return leftValue.Value.Degrees == rightValue.Value.Degrees;
        }

        /// <summary>
        /// Compares two latitudes to indicate if they are same.
        /// leftValue == rightValue
        /// </summary>
        /// <param name="leftValue">The left value to compare.</param>
        /// <param name="rightValue">The right value to compare.</param>
        /// <returns>true if they are same; otherwise, false.</returns>
        public static bool operator ==(Latitude leftValue, Model rightValue)
        {
            if (ReferenceEquals(leftValue, rightValue)) return true;
            return !(rightValue is null) && leftValue.Value.Degrees == rightValue.Degrees;
        }

        /// <summary>
        /// Compares two latitudes to indicate if they are different.
        /// leftValue != rightValue
        /// </summary>
        /// <param name="leftValue">The left value to compare.</param>
        /// <param name="rightValue">The right value to compare.</param>
        /// <returns>true if they are different; otherwise, false.</returns>
        public static bool operator !=(Latitude leftValue, Latitude rightValue)
        {
            if (ReferenceEquals(leftValue, rightValue)) return false;
            return leftValue.Value.Degrees != rightValue.Value.Degrees;
        }

        /// <summary>
        /// Compares two latitudes to indicate if they are different.
        /// leftValue != rightValue
        /// </summary>
        /// <param name="leftValue">The left value to compare.</param>
        /// <param name="rightValue">The right value to compare.</param>
        /// <returns>true if they are different; otherwise, false.</returns>
        public static bool operator !=(Latitude leftValue, Model rightValue)
        {
            if (ReferenceEquals(leftValue, rightValue)) return false;
            return rightValue is null || leftValue.Value.Degrees != rightValue.Degrees;
        }

        /// <summary>
        /// Converts a number to latitude.
        /// </summary>
        /// <param name="value">The instance.</param>
        public static implicit operator Latitude(Model value)
        {
            if (value is null) return new Latitude();
            return new Latitude(value.Degrees);
        }

        private static double GetDegrees(Latitudes type, double degrees)
        {
            return type switch
            {
                Latitudes.North => degrees,
                Latitudes.South => -degrees,
                _ => 0,
            };
        }
    }

    /// <summary>
    /// Longitude.
    /// </summary>
    public struct Longitude : IEquatable<Longitude>
    {
        /// <summary>
        /// Longitude model.
        /// </summary>
        public class Model : Angle.Model
        {
            /// <summary>
            /// The flag indicating whether it is celestial longitude.
            /// </summary>
            private bool celestial;

            /// <summary>
            /// Initializes a new instance of the Longitude.Model class.
            /// </summary>
            public Model() : base(BoundaryOptions)
            {
            }

            /// <summary>
            /// Initializes a new instance of the Longitude.Model class.
            /// </summary>
            /// <param name="isCelestial">true if it is celestial longitude; otherwise, false.</param>
            public Model(bool isCelestial) : base(isCelestial ? CelestialBoundaryOptions : BoundaryOptions)
            {
                IsCelestial = isCelestial;
            }

            /// <summary>
            /// Initializes a new instance of the Longitude.Model class.
            /// </summary>
            /// <param name="degrees">The total degrees.</param>
            public Model(double degrees) : base(degrees, BoundaryOptions)
            {
            }

            /// <summary>
            /// Initializes a new instance of the Longitude.Model class.
            /// </summary>
            /// <param name="degree">The degree part.</param>
            /// <param name="minute">The minute part.</param>
            /// <param name="second">The second part.</param>
            public Model(int degree, int minute, float second = 0) : base(degree, minute, second, BoundaryOptions)
            {
            }

            /// <summary>
            /// Initializes a new instance of the Longitude.Model class.
            /// </summary>
            /// <param name="isCelestial">true if it is celestial longitude; otherwise, false.</param>
            /// <param name="degrees">The total degrees.</param>
            public Model(bool isCelestial, double degrees) : base(degrees, isCelestial ? CelestialBoundaryOptions : BoundaryOptions)
            {
                IsCelestial = isCelestial;
            }

            /// <summary>
            /// Initializes a new instance of the Longitude.Model class.
            /// </summary>
            /// <param name="isCelestial">true if it is celestial longitude; otherwise, false.</param>
            /// <param name="degree">The degree part.</param>
            /// <param name="minute">The minute part.</param>
            /// <param name="second">The second part.</param>
            public Model(bool isCelestial, int degree, int minute, float second = 0) : base(degree, minute, second, isCelestial ? CelestialBoundaryOptions : BoundaryOptions)
            {
                IsCelestial = isCelestial;
            }

            /// <summary>
            /// Gets a value indicating wether it is celestial longitude.
            /// </summary>
            public bool IsCelestial
            {
                get
                {
                    return celestial;
                }

                set
                {
                    if (value == celestial) return;
                    var degrees = Degrees;
                    celestial = value;
                    if (value)
                    {
                        if (degrees < 0) Degrees = degrees + 360;
                        return;
                    }

                    if (degrees > 180) Degrees = degrees - 360;
                }
            }

            /// <summary>
            /// Gets or sets the longitude zone.
            /// </summary>
            public Longitudes Type
            {
                get
                {
                    var degrees = Degrees;
                    if (degrees == 0 || degrees == 360) return Longitudes.PrimeMeridian;
                    if (degrees == 180 || degrees == -180) return Longitudes.CalendarLine;
                    if (IsCelestial) return degrees > 180 ? Longitudes.West : Longitudes.East;
                    return degrees > 0 ? Longitudes.East : Longitudes.West;
                }

                set
                {
                    switch (value)
                    {
                        case Longitudes.PrimeMeridian:
                            Degrees = 0;
                            break;
                        case Longitudes.CalendarLine:
                            Degrees = 180;
                            break;
                        case Longitudes.East:
                            if (IsCelestial)
                            {
                                var degrees = Degrees;
                                if (degrees > 180) Degrees = degrees - 180;
                                break;
                            }
                            
                            if (IsNegative) Degree = -Degree;
                            break;
                        case Longitudes.West:
                            if (IsCelestial)
                            {
                                var degrees = Degrees;
                                if (degrees < 180 && degrees > 0) Degrees = degrees + 180;
                                break;
                            }
                            
                            if (Positive) Degree = -Degree;
                            break;
                    }
                }
            }

            /// <summary>
            /// Indicates whether the current object is equal to another object of the same type.
            /// </summary>
            /// <returns>
            /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
            /// </returns>
            /// <param name="other">An object to compare with this object.</param>
            public override bool Equals(object other)
            {
                if (other is null) return false;
                if (other is IAngle a) return Degrees.Equals(a.Degrees);
                if (other is Longitude l) return Degrees.Equals(l.Value.Degrees);
                if (other is double d) return Degrees.Equals(d);
                return Degrees.Equals(other);
            }

            /// <summary>
            /// Returns the hash code for this instance.
            /// </summary>
            /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            /// <summary>
            /// Returns the longitude string value of this instance.
            /// </summary>
            /// <returns>A System.String containing this longitude.</returns>
            public override string ToString()
            {
                if (IsCelestial) return base.ToString();
                var degrees = Degrees;
                if (degrees == 0) return NumberSymbols.NumberZero + Angle.Symbols.DegreeUnit;
                if (degrees == 180 || degrees == -180) return "180" + Angle.Symbols.DegreeUnit;
                return this.ToAbsAngleString() + (Positive ? "E" : "W");
            }

            /// <summary>
            /// Compares two longitudes to indicate if they are same.
            /// leftValue == rightValue
            /// </summary>
            /// <param name="leftValue">The left value to compare.</param>
            /// <param name="rightValue">The right value to compare.</param>
            /// <returns>true if they are same; otherwise, false.</returns>
            public static bool operator ==(Model leftValue, Longitude rightValue)
            {
                if (ReferenceEquals(leftValue, rightValue)) return true;
                return !(leftValue is null) && leftValue.Equals(rightValue);
            }

            /// <summary>
            /// Compares two longitudes to indicate if they are same.
            /// leftValue == rightValue
            /// </summary>
            /// <param name="leftValue">The left value to compare.</param>
            /// <param name="rightValue">The right value to compare.</param>
            /// <returns>true if they are same; otherwise, false.</returns>
            public static bool operator ==(Model leftValue, Model rightValue)
            {
                if (ReferenceEquals(leftValue, rightValue)) return true;
                return !(leftValue is null) && leftValue.Equals(rightValue);
            }

            /// <summary>
            /// Compares two longitudes to indicate if they are different.
            /// leftValue != rightValue
            /// </summary>
            /// <param name="leftValue">The left value to compare.</param>
            /// <param name="rightValue">The right value to compare.</param>
            /// <returns>true if they are different; otherwise, false.</returns>
            public static bool operator !=(Model leftValue, Longitude rightValue)
            {
                if (ReferenceEquals(leftValue, rightValue)) return false;
                return leftValue is null || !leftValue.Equals(rightValue);
            }

            /// <summary>
            /// Compares two longitudes to indicate if they are different.
            /// leftValue != rightValue
            /// </summary>
            /// <param name="leftValue">The left value to compare.</param>
            /// <param name="rightValue">The right value to compare.</param>
            /// <returns>true if they are different; otherwise, false.</returns>
            public static bool operator !=(Model leftValue, Model rightValue)
            {
                if (ReferenceEquals(leftValue, rightValue)) return false;
                return leftValue is null || !leftValue.Equals(rightValue);
            }

            /// <summary>
            /// Converts a longitude to its model.
            /// </summary>
            /// <param name="value">The instance.</param>
            public static implicit operator Model(Longitude value)
            {
                return new Model { Degrees = value.Value.Degrees };
            }
        }

        /// <summary>
        /// The angle boundary of the longitude.
        /// </summary>
        public static readonly Angle.BoundaryOptions BoundaryOptions = new Angle.BoundaryOptions(180, true, Angle.RectifyModes.Cycle);

        /// <summary>
        /// The angle boundary of the celestial longitude.
        /// </summary>
        public static readonly Angle.BoundaryOptions CelestialBoundaryOptions = new Angle.BoundaryOptions(360, false, Angle.RectifyModes.Cycle);

        /// <summary>
        /// Initializes a new instance of the Longitude struct.
        /// </summary>
        /// <param name="type">The latitude type.</param>
        /// <param name="degree">The degree part.</param>
        /// <param name="minute">The minute part.</param>
        /// <param name="second">The second part.</param>
        public Longitude(Longitudes type, int degree, int minute, float second = 0) : this(GetDegrees(type, Angle.GetDegrees(degree, minute, second)))
        {
        }

        /// <summary>
        /// Initializes a new instance of the Longitude struct.
        /// </summary>
        /// <param name="degree">The degree part.</param>
        /// <param name="minute">The minute part.</param>
        /// <param name="second">The second part.</param>
        public Longitude(int degree, int minute, float second = 0) : this(Angle.GetDegrees(degree, minute, second))
        {
        }

        /// <summary>
        /// Initializes a new instance of the Longitude struct.
        /// </summary>
        /// <param name="isCelestial">true if it is celestial longitude; otherwise, false.</param>
        /// <param name="degree">The degree part.</param>
        /// <param name="minute">The minute part.</param>
        /// <param name="second">The second part.</param>
        public Longitude(bool isCelestial, int degree, int minute, float second = 0) : this(isCelestial, Angle.GetDegrees(degree, minute, second))
        {
        }

        /// <summary>
        /// Initializes a new instance of the Longitude struct.
        /// </summary>
        /// <param name="degrees">The total degrees.</param>
        public Longitude(double degrees) : this(false, degrees)
        {
        }

        /// <summary>
        /// Initializes a new instance of the Longitude struct.
        /// </summary>
        /// <param name="isCelestial">true if it is celestial longitude; otherwise, false.</param>
        /// <param name="degrees">The total degrees.</param>
        public Longitude(bool isCelestial, double degrees)
        {
            IsCelestial = isCelestial;
            if (isCelestial)
            {
                if (degrees >= 360 || degrees < 0) degrees %= 360;
                Value = new Angle(degrees);
                if (degrees == 180) Type = Longitudes.CalendarLine;
                if (degrees == 0) Type = Longitudes.PrimeMeridian;
                else if (degrees > 180) Type = Longitudes.West;
                else Type = Longitudes.East;
                return;
            }

            if (degrees > 180 || degrees < -180)
            {
                degrees += 180;
                degrees %= 360;
                if (degrees != 0) degrees -= 180;
                else degrees = 180;
            }

            Value = new Angle(degrees);
            if (degrees == 180 || degrees == -180) Type = Longitudes.CalendarLine;
            else if (degrees > 0) Type = Longitudes.East;
            else if (degrees < 0) Type = Longitudes.West;
            else Type = Longitudes.PrimeMeridian;
        }

        /// <summary>
        /// Initializes a new instance of the Longitude struct.
        /// </summary>
        /// <param name="angle">The angle.</param>
        public Longitude(Angle angle) : this(angle.Degrees)
        {
        }

        /// <summary>
        /// Initializes a new instance of the Longitude struct.
        /// </summary>
        /// <param name="isCelestial">true if it is celestial longitude; otherwise, false.</param>
        /// <param name="angle">The angle.</param>
        public Longitude(bool isCelestial, Angle angle) : this(isCelestial, angle.Degrees)
        {
        }

        /// <summary>
        /// Initializes a new instance of the Longitude struct.
        /// </summary>
        /// <param name="type">The longitude type.</param>
        /// <param name="degrees">The total degrees.</param>
        public Longitude(Longitudes type, double degrees) : this(GetDegrees(type, degrees))
        {
        }

        /// <summary>
        /// Gets a value indicating wether it is celestial longitude.
        /// </summary>
        public bool IsCelestial { get; }

        /// <summary>
        /// Gets the longitude zone.
        /// </summary>
        public Longitudes Type { get; }

        /// <summary>
        /// The angle value in the longitude type.
        /// </summary>
        public Angle Value { get; }

        /// <summary>
        /// The angle degrees of the longitude.
        /// </summary>
        public double Degrees => Value.Degrees;

        /// <summary>
        /// The absolute angle degrees in the longitude type.
        /// </summary>
        public double AbsDegrees => Value.AbsDegrees;

        /// <summary>
        /// The angle degrees of the longitude.
        /// </summary>
        public int Degree => Value.Degree;

        /// <summary>
        /// The absolute angle degrees in the longitude type.
        /// </summary>
        public int AbsDegree => Value.Degree;

        /// <summary>
        /// Gets the arcminute of the longitude.
        /// </summary>
        public int Arcminute => Value.Arcminute;

        /// <summary>
        /// Gets the arcsecond of the longitude.
        /// </summary>
        public float Arcsecond => Value.Arcsecond;

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return Value.Degrees.GetHashCode();
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public override bool Equals(object other)
        {
            if (other is null) return false;
            if (other is Longitude l) return Value.Degrees.Equals(l.Value.Degrees);
            if (other is Model m) return Value.Degrees.Equals(m.Degrees);
            return Value.Equals(other);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(Longitude other)
        {
            return Value == other.Value;
        }

        /// <summary>
        /// Returns the latitude string value of this instance.
        /// </summary>
        /// <returns>A System.String containing this latitude.</returns>
        public override string ToString()
        {
            if (IsCelestial) return base.ToString();
            var degrees = Value.Degrees;
            if (degrees == 0) return NumberSymbols.NumberZero + Angle.Symbols.DegreeUnit;
            if (degrees == 180 || degrees == -180) return "180" + Angle.Symbols.DegreeUnit;
            return Value.ToAbsAngleString() + (Value.Positive ? "E" : "W");
        }

        /// <summary>
        /// Compares two longitudes to indicate if they are same.
        /// leftValue == rightValue
        /// </summary>
        /// <param name="leftValue">The left value to compare.</param>
        /// <param name="rightValue">The right value to compare.</param>
        /// <returns>true if they are same; otherwise, false.</returns>
        public static bool operator ==(Longitude leftValue, Longitude rightValue)
        {
            if (ReferenceEquals(leftValue, rightValue)) return true;
            return leftValue.Value.Degrees == rightValue.Value.Degrees;
        }

        /// <summary>
        /// Compares two longitudes to indicate if they are same.
        /// leftValue == rightValue
        /// </summary>
        /// <param name="leftValue">The left value to compare.</param>
        /// <param name="rightValue">The right value to compare.</param>
        /// <returns>true if they are same; otherwise, false.</returns>
        public static bool operator ==(Longitude leftValue, Model rightValue)
        {
            if (ReferenceEquals(leftValue, rightValue)) return true;
            return !(rightValue is null) && leftValue.Value.Degrees == rightValue.Degrees;
        }

        /// <summary>
        /// Compares two longitudes to indicate if they are different.
        /// leftValue != rightValue
        /// </summary>
        /// <param name="leftValue">The left value to compare.</param>
        /// <param name="rightValue">The right value to compare.</param>
        /// <returns>true if they are different; otherwise, false.</returns>
        public static bool operator !=(Longitude leftValue, Longitude rightValue)
        {
            if (ReferenceEquals(leftValue, rightValue)) return false;
            return leftValue.Value.Degrees != rightValue.Value.Degrees;
        }

        /// <summary>
        /// Compares two longitudes to indicate if they are different.
        /// leftValue != rightValue
        /// </summary>
        /// <param name="leftValue">The left value to compare.</param>
        /// <param name="rightValue">The right value to compare.</param>
        /// <returns>true if they are different; otherwise, false.</returns>
        public static bool operator !=(Longitude leftValue, Model rightValue)
        {
            if (ReferenceEquals(leftValue, rightValue)) return false;
            return rightValue is null || leftValue.Value.Degrees != rightValue.Degrees;
        }

        /// <summary>
        /// Converts a number to longitude.
        /// </summary>
        /// <param name="value">The instance.</param>
        public static implicit operator Longitude(Model value)
        {
            if (value is null) return new Longitude();
            return new Longitude(value.Degrees);
        }

        private static double GetDegrees(Longitudes type, double degrees)
        {
            return type switch
            {
                Longitudes.East => degrees,
                Longitudes.West => -degrees,
                Longitudes.CalendarLine => 180,
                _ => 0,
            };
        }
    }

    /// <summary>
    /// Geolocation with latitude, longitude and optional altitude.
    /// </summary>
    public struct Geolocation : IEquatable<Geolocation>
    {
        /// <summary>
        /// The geolocation model.
        /// </summary>
        public class Model
        {
            /// <summary>
            /// Initializes a new instance of the Model class.
            /// </summary>
            public Model()
            {
                Latitude = new Latitude.Model();
                Longitude = new Longitude.Model();
                Altitude = null;
            }

            /// <summary>
            /// Initializes a new instance of the Model class.
            /// </summary>
            /// <param name="latitude">The latitude.</param>
            /// <param name="longitude">The longitude.</param>
            /// <param name="desc">The optional description.</param>
            public Model(Latitude.Model latitude, Longitude.Model longitude, string desc = null)
            {
                Latitude = latitude;
                Longitude = longitude;
                Altitude = null;
                Description = desc;
            }

            /// <summary>
            /// Initializes a new instance of the Model class.
            /// </summary>
            /// <param name="latitude">The latitude.</param>
            /// <param name="longitude">The longitude.</param>
            /// <param name="altitude">The altitude.</param>
            /// <param name="desc">The optional description.</param>
            public Model(Latitude.Model latitude, Longitude.Model longitude, double altitude, string desc = null)
            {
                Latitude = latitude;
                Longitude = longitude;
                Altitude = altitude;
                Description = desc;
            }

            /// <summary>
            /// Gets the latitude.
            /// </summary>
            public Latitude.Model Latitude { get; }

            /// <summary>
            /// Gets the longitude.
            /// </summary>
            public Longitude.Model Longitude { get; }

            /// <summary>
            /// Gets or sets the radius deviation in meters.
            /// </summary>
            public double? RadiusDeviation { get; set; }

            /// <summary>
            /// Gets or sets the altitude.
            /// </summary>
            public double? Altitude { get; set; }

            /// <summary>
            /// Gets or sets the altitude deviation in meters.
            /// </summary>
            public double? AltitudeDeviation { get; set; }

            /// <summary>
            /// Gets or sets the place description.
            /// </summary>
            public string Description { get; set; }

            /// <summary>
            /// Tests if the given altitude is in the deviation of this instance.
            /// </summary>
            /// <param name="value">The altitude to test.</param>
            /// <returns>true if they are in the same altitude; otherwise, false.</returns>
            public bool IsAltitude(double value)
            {
                if (!Altitude.HasValue) return true;
                if (!AltitudeDeviation.HasValue || AltitudeDeviation == 0) return Altitude == value;
                var min = Altitude.Value - AltitudeDeviation.Value;
                var max = Altitude.Value + AltitudeDeviation.Value;
                return Math.Min(min, max) <= value && value <= Math.Max(min, max);
            }

            /// <summary>
            /// Returns the hash code for this instance.
            /// </summary>
            /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
            public override int GetHashCode()
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}; {1}; {2}. (r {3} & a {4})", Latitude, Longitude, Altitude, RadiusDeviation, AltitudeDeviation).GetHashCode();
            }

            /// <summary>
            /// Indicates whether the current object is equal to another object of the same type.
            /// </summary>
            /// <returns>
            /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
            /// </returns>
            /// <param name="other">An object to compare with this object.</param>
            public bool Equals(Geolocation other)
            {
                return Latitude == other.Latitude && Longitude == other.Longitude && Altitude == other.Altitude && RadiusDeviation == other.RadiusDeviation && AltitudeDeviation == other.AltitudeDeviation;
            }

            /// <summary>
            /// Indicates whether the current object is equal to another object of the same type.
            /// </summary>
            /// <returns>
            /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
            /// </returns>
            /// <param name="other">An object to compare with this object.</param>
            public bool Equals(Model other)
            {
                return Latitude == other.Latitude && Longitude == other.Longitude && Altitude == other.Altitude && RadiusDeviation == other.RadiusDeviation && AltitudeDeviation == other.AltitudeDeviation;
            }

            /// <summary>
            /// Indicates whether the current object is equal to another object of the same type.
            /// </summary>
            /// <returns>
            /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
            /// </returns>
            /// <param name="other">An object to compare with this object.</param>
            public override bool Equals(object other)
            {
                if (other == null) return false;
                if (other is Geolocation l) return Latitude == l.Latitude && Longitude == l.Longitude && Altitude == l.Altitude && RadiusDeviation == l.RadiusDeviation && AltitudeDeviation == l.AltitudeDeviation;
                if (other is Model m) return Latitude == m.Latitude && Longitude == m.Longitude && Altitude == m.Altitude && RadiusDeviation == m.RadiusDeviation && AltitudeDeviation == m.AltitudeDeviation;
                return false;
            }

            /// <summary>
            /// Returns the geolocation string value of this instance.
            /// </summary>
            /// <returns>A System.String containing this geolocation instance.</returns>
            public override string ToString()
            {
                if (!Altitude.HasValue) return string.Format("Latitude = {0}; Longtitude = {1}", Latitude, Longitude);
                return string.Format("Latitude = {0}; Longtitude = {1}; Altitude = {2}m", Latitude, Longitude, Altitude.Value);
            }

            /// <summary>
            /// Compares two longitudes to indicate if they are same.
            /// leftValue == rightValue
            /// </summary>
            /// <param name="leftValue">The left value to compare.</param>
            /// <param name="rightValue">The right value to compare.</param>
            /// <returns>true if they are same; otherwise, false.</returns>
            public static bool operator ==(Model leftValue, Geolocation rightValue)
            {
                if (ReferenceEquals(leftValue, rightValue)) return true;
                return !(leftValue is null) && leftValue.Equals(rightValue);
            }

            /// <summary>
            /// Compares two longitudes to indicate if they are same.
            /// leftValue == rightValue
            /// </summary>
            /// <param name="leftValue">The left value to compare.</param>
            /// <param name="rightValue">The right value to compare.</param>
            /// <returns>true if they are same; otherwise, false.</returns>
            public static bool operator ==(Model leftValue, Model rightValue)
            {
                if (ReferenceEquals(leftValue, rightValue)) return true;
                return !(leftValue is null) && leftValue.Equals(rightValue);
            }

            /// <summary>
            /// Compares two longitudes to indicate if they are different.
            /// leftValue != rightValue
            /// </summary>
            /// <param name="leftValue">The left value to compare.</param>
            /// <param name="rightValue">The right value to compare.</param>
            /// <returns>true if they are different; otherwise, false.</returns>
            public static bool operator !=(Model leftValue, Geolocation rightValue)
            {
                if (ReferenceEquals(leftValue, rightValue)) return false;
                return leftValue is null || !leftValue.Equals(rightValue);
            }

            /// <summary>
            /// Compares two longitudes to indicate if they are different.
            /// leftValue != rightValue
            /// </summary>
            /// <param name="leftValue">The left value to compare.</param>
            /// <param name="rightValue">The right value to compare.</param>
            /// <returns>true if they are different; otherwise, false.</returns>
            public static bool operator !=(Model leftValue, Model rightValue)
            {
                if (ReferenceEquals(leftValue, rightValue)) return false;
                return leftValue is null || !leftValue.Equals(rightValue);
            }
        }

        /// <summary>
        /// Initializes a new instance of the Geolocation class.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="desc">The place description.</param>
        /// <param name="radiusDeviation">The radius deviation in meters.</param>
        public Geolocation(Latitude latitude, Longitude longitude, string desc = null, double? radiusDeviation = null)
        {
            Latitude = latitude;
            Longitude = longitude;
            Altitude = null;
            RadiusDeviation = radiusDeviation;
            AltitudeDeviation = null;
            Description = desc;
        }

        /// <summary>
        /// Initializes a new instance of the Geolocation class.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="altitude">The altitude.</param>
        /// <param name="radiusDeviation">The radius deviation in meters.</param>
        /// <param name="altitudeDeviation">The altitude deviation in meters.</param>
        public Geolocation(Latitude latitude, Longitude longitude, double altitude, double radiusDeviation, double? altitudeDeviation = null) : this(latitude, longitude, altitude, null, altitudeDeviation, radiusDeviation)
        {
        }

        /// <summary>
        /// Initializes a new instance of the Geolocation class.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="altitude">The altitude.</param>
        /// <param name="desc">The place description.</param>
        /// <param name="radiusDeviation">The radius deviation in meters.</param>
        /// <param name="altitudeDeviation">The altitude deviation in meters.</param>
        public Geolocation(Latitude latitude, Longitude longitude, double altitude, string desc = null, double? radiusDeviation = null, double? altitudeDeviation = null)
        {
            Latitude = latitude;
            Longitude = longitude;
            Altitude = altitude;
            RadiusDeviation = radiusDeviation;
            AltitudeDeviation = altitudeDeviation;
            Description = desc;
        }

        /// <summary>
        /// Gets the latitude.
        /// </summary>
        public Latitude Latitude { get; }

        /// <summary>
        /// Gets the longitude.
        /// </summary>
        public Longitude Longitude { get; }

        /// <summary>
        /// Gets the radius deviation in meters.
        /// </summary>
        public double? RadiusDeviation { get; }

        /// <summary>
        /// Gets the altitude.
        /// </summary>
        public double? Altitude { get; }

        /// <summary>
        /// Gets the altitude deviation in meters.
        /// </summary>
        public double? AltitudeDeviation { get; }

        /// <summary>
        /// Gets the place description.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Tests if the given altitude is in the deviation of this instance.
        /// </summary>
        /// <param name="value">The altitude to test.</param>
        /// <returns>true if they are in the same altitude; otherwise, false.</returns>
        public bool IsAltitude(double value)
        {
            if (!Altitude.HasValue) return true;
            if (!AltitudeDeviation.HasValue || AltitudeDeviation == 0) return Altitude == value;
            var min = Altitude.Value - AltitudeDeviation.Value;
            var max = Altitude.Value + AltitudeDeviation.Value;
            return Math.Min(min, max) <= value && value <= Math.Max(min, max);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}; {1}; {2}. (r {3} & a {4})", Latitude, Longitude, Altitude, RadiusDeviation, AltitudeDeviation).GetHashCode();
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(Geolocation other)
        {
            return Latitude == other.Latitude && Longitude == other.Longitude && Altitude == other.Altitude && RadiusDeviation == other.RadiusDeviation && AltitudeDeviation == other.AltitudeDeviation;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(Model other)
        {
            return Latitude == other.Latitude && Longitude == other.Longitude && Altitude == other.Altitude && RadiusDeviation == other.RadiusDeviation && AltitudeDeviation == other.AltitudeDeviation;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public override bool Equals(object other)
        {
            if (other == null) return false;
            if (other is Geolocation l) return Latitude == l.Latitude && Longitude == l.Longitude && Altitude == l.Altitude && RadiusDeviation == l.RadiusDeviation && AltitudeDeviation == l.AltitudeDeviation;
            if (other is Model m) return Latitude == m.Latitude && Longitude == m.Longitude && Altitude == m.Altitude && RadiusDeviation == m.RadiusDeviation && AltitudeDeviation == m.AltitudeDeviation;
            return false;
        }

        /// <summary>
        /// Returns the geolocation string value of this instance.
        /// </summary>
        /// <returns>A System.String containing this geolocation instance.</returns>
        public override string ToString()
        {
            if (!Altitude.HasValue) return string.Format("Latitude = {0}; Longtitude = {1}", Latitude, Longitude);
            return string.Format("Latitude = {0}; Longtitude = {1}; Altitude = {2}m", Latitude, Longitude, Altitude.Value);
        }

        /// <summary>
        /// Compares two longitudes to indicate if they are same.
        /// leftValue == rightValue
        /// </summary>
        /// <param name="leftValue">The left value to compare.</param>
        /// <param name="rightValue">The right value to compare.</param>
        /// <returns>true if they are same; otherwise, false.</returns>
        public static bool operator ==(Geolocation leftValue, Geolocation rightValue)
        {
            if (ReferenceEquals(leftValue, rightValue)) return true;
            return leftValue.Equals(rightValue);
        }

        /// <summary>
        /// Compares two longitudes to indicate if they are same.
        /// leftValue == rightValue
        /// </summary>
        /// <param name="leftValue">The left value to compare.</param>
        /// <param name="rightValue">The right value to compare.</param>
        /// <returns>true if they are same; otherwise, false.</returns>
        public static bool operator ==(Geolocation leftValue, Model rightValue)
        {
            if (ReferenceEquals(leftValue, rightValue)) return true;
            return leftValue.Equals(rightValue);
        }

        /// <summary>
        /// Compares two longitudes to indicate if they are different.
        /// leftValue != rightValue
        /// </summary>
        /// <param name="leftValue">The left value to compare.</param>
        /// <param name="rightValue">The right value to compare.</param>
        /// <returns>true if they are different; otherwise, false.</returns>
        public static bool operator !=(Geolocation leftValue, Geolocation rightValue)
        {
            if (ReferenceEquals(leftValue, rightValue)) return false;
            return !leftValue.Equals(rightValue);
        }

        /// <summary>
        /// Compares two longitudes to indicate if they are different.
        /// leftValue != rightValue
        /// </summary>
        /// <param name="leftValue">The left value to compare.</param>
        /// <param name="rightValue">The right value to compare.</param>
        /// <returns>true if they are different; otherwise, false.</returns>
        public static bool operator !=(Geolocation leftValue, Model rightValue)
        {
            if (ReferenceEquals(leftValue, rightValue)) return false;
            return !leftValue.Equals(rightValue);
        }
    }
}
