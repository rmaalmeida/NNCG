//-----------------------------------------------------------------------
// <copyright file="Number.cs" company="Math.NET Project">
//    Copyright (c) 2002-2009, Christoph R�egg.
//    All Right Reserved.
// </copyright>
// <author>
//    Christoph R�egg, http://christoph.ruegg.name
// </author>
// <product>
//    Math.NET Iridium, part of the Math.NET Project.
//    http://mathnet.opensourcedotnet.info
// </product>
// <license type="opensource" name="LGPL" version="2 or later">
//    This program is free software; you can redistribute it and/or modify
//    it under the terms of the GNU Lesser General Public License as published 
//    by the Free Software Foundation; either version 2 of the License, or
//    any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU Lesser General Public License for more details.
//
//    You should have received a copy of the GNU Lesser General Public 
//    License along with this program; if not, write to the Free Software
//    Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
// </license>
//-----------------------------------------------------------------------

using System;
using MathNet.Numerics.Properties;

namespace MathNet.Numerics
{
    // TODO: Not used anywhere. Remove?
    ////[StructLayout(LayoutKind.Explicit)]
    ////internal struct FloatingPoint64
    ////{
    ////    [FieldOffset(0)]
    ////    internal System.Double float64;

    ////    [FieldOffset(0)]
    ////    internal System.Int64 signed64;

    ////    [FieldOffset(0)]
    ////    internal System.UInt64 unsigned64;
    ////}

    /// <summary>
    /// Helper functions for dealing with floating point numbers.
    /// </summary>
    public static class Number
    {
        /// <summary>2^(-1074)</summary>
        public const double SmallestNumberGreaterThanZero = Double.Epsilon;

        /// <summary>2^(-53)</summary>
        public static readonly double RelativeAccuracy = EpsilonOf(1.0);

        /// <summary>2^(-52)</summary>
        public static readonly double PositiveRelativeAccuracy = PositiveEpsilonOf(1.0);

        /// <summary>10 * 2^(-52)</summary>
        public static readonly double DefaultRelativeAccuracy = 10 * PositiveRelativeAccuracy;

        /// <summary>
        /// Evaluates the minimum distance to the next distinguishable number near the argument value.
        /// </summary>
        /// <returns>Relative Epsilon (positive double or NaN).</returns>
        /// <remarks>Evaluates the <b>negative</b> epsilon. The more common positive epsilon is equal to two times this negative epsilon.</remarks>
        /// <seealso cref="PositiveEpsilonOf(double)"/>
        public static
        double
        EpsilonOf(double value)
        {
            if(Double.IsInfinity(value) || Double.IsNaN(value))
            {
                return Double.NaN;
            }

            long signed64 = BitConverter.DoubleToInt64Bits(value);

            if(signed64 == 0)
            {
                signed64++;
                return BitConverter.Int64BitsToDouble(signed64) - value;
            }

            if(signed64-- < 0)
            {
                return BitConverter.Int64BitsToDouble(signed64) - value;
            }

            return value - BitConverter.Int64BitsToDouble(signed64);
        }

        /// <summary>
        /// Evaluates the minimum distance to the next distinguishable number near the argument value.
        /// </summary>
        /// <returns>Relative Epsilon (positive double or NaN)</returns>
        /// <remarks>Evaluates the <b>positive</b> epsilon. See also <see cref="EpsilonOf"/></remarks>
        /// <seealso cref="EpsilonOf(double)"/>
        public static
        double
        PositiveEpsilonOf(double value)
        {
            return 2 * EpsilonOf(value);
        }

        /// <summary>
        /// Increments a floating point number to the next bigger number representable by the data type.
        /// </summary>
        /// <remarks>
        /// The incrementation step length depends on the provided value.
        /// Increment(double.MaxValue) will return positive infinity.
        /// </remarks>
        public static
        double
        Increment(double value)
        {
            if(Double.IsInfinity(value) || Double.IsNaN(value))
            {
                return value;
            }

            long signed64 = BitConverter.DoubleToInt64Bits(value);
            if(signed64 < 0)
            {
                signed64--;
            }
            else
            {
                signed64++;
            }

            if(signed64 == -9223372036854775808)
            {
                /* = "-0", make it "+0" */

                return 0;
            }

            value = BitConverter.Int64BitsToDouble(signed64);

            return Double.IsNaN(value)
                ? Double.NaN
                : value;
        }

        /// <summary>
        /// Decrements a floating point number to the next smaller number representable by the data type.
        /// </summary>
        /// <remarks>
        /// The decrementation step length depends on the provided value.
        /// Decrement(double.MinValue) will return negative infinity.
        /// </remarks>
        public static
        double
        Decrement(double value)
        {
            if(Double.IsInfinity(value) || Double.IsNaN(value))
            {
                return value;
            }

            long signed64 = BitConverter.DoubleToInt64Bits(value);
            if(signed64 == 0)
            {
                return -Double.Epsilon;
            }

            if(signed64 < 0)
            {
                signed64++;
            }
            else
            {
                signed64--;
            }

            value = BitConverter.Int64BitsToDouble(signed64);

            return Double.IsNaN(value)
                ? Double.NaN
                : value;
        }

        /// <summary>
        /// Evaluates the count of numbers between two double numbers
        /// </summary>
        /// <remarks>The second number is included in the number, thus two equal numbers evaluate to zero and two neighbor numbers evaluate to one. Therefore, what is returned is actually the count of numbers between plus 1.</remarks>
        [CLSCompliant(false)]
        public static
        ulong
        NumbersBetween(
            double a,
            double b)
        {
            if(Double.IsNaN(a) || Double.IsInfinity(a))
            {
                throw new ArgumentException(LocalStrings.ArgumentNotInfinityNaN, "a");
            }

            if(Double.IsNaN(b) || Double.IsInfinity(b))
            {
                throw new ArgumentException(LocalStrings.ArgumentNotInfinityNaN, "b");
            }

            ulong ua = ToLexicographicalOrderedUInt64(a);
            ulong ub = ToLexicographicalOrderedUInt64(b);

            return (a >= b)
                ? unchecked(ua - ub)
                : unchecked(ub - ua);
        }

        /// <summary>
        /// Maps a double to an unsigned long integer which provides lexicographical ordering.
        /// </summary>
        [CLSCompliant(false)]
        public static
        ulong
        ToLexicographicalOrderedUInt64(double value)
        {
            long signed64 = BitConverter.DoubleToInt64Bits(value);
            ulong unsigned64 = unchecked((ulong)signed64);

            return (signed64 >= 0)
                ? unsigned64
                : unchecked(0x8000000000000000 - unsigned64);
        }

        /// <summary>
        /// Maps a double to an signed long integer which provides lexicographical ordering.
        /// </summary>
        public static
        long
        ToLexicographicalOrderedInt64(double value)
        {
            long signed64 = BitConverter.DoubleToInt64Bits(value);

            return (signed64 >= 0)
                ? signed64
                : unchecked((long)(0x8000000000000000 - (ulong)signed64));
        }

        /// <summary>
        /// Converts a long integer in signed-magnitude format to an unsigned long integer in two-complement format.
        /// </summary>
        [CLSCompliant(false)]
        public static
        ulong
        SignedMagnitudeToTwosComplementUInt64(long value)
        {
            return (value >= 0)
                ? (ulong)value
                : unchecked(0x8000000000000000 - (ulong)value);
        }

        /// <summary>
        /// Converts an unsigned long integer in two-complement to a long integer in signed-magnitude format format.
        /// </summary>
        public static
        long
        SignedMagnitudeToTwosComplementInt64(long value)
        {
            return (value >= 0)
                ? value
                : unchecked((long)(0x8000000000000000 - (ulong)value));
        }

        /// <summary>
        /// Checks whether two real numbers are almost equal.
        /// </summary>
        /// <param name="a">The first number</param>
        /// <param name="b">The second number</param>
        /// <param name="maxNumbersBetween">The maximum count of numbers between the two numbers plus one ([a,a] -> 0, [a,a+e] -> 1, [a,a+2e] -> 2, ...).</param>
        public static
        bool
        AlmostEqual(
            double a,
            double b,
            int maxNumbersBetween)
        {
            return AlmostEqual(a, b, (ulong)maxNumbersBetween);
        }

        /// <summary>
        /// Checks whether two real numbers are almost equal.
        /// </summary>
        /// <param name="a">The first number</param>
        /// <param name="b">The second number</param>
        /// <param name="maxNumbersBetween">The maximum count of numbers between the two numbers plus one ([a,a] -> 0, [a,a+e] -> 1, [a,a+2e] -> 2, ...).</param>
        [CLSCompliant(false)]
        public static
        bool
        AlmostEqual(
            double a,
            double b,
            ulong maxNumbersBetween)
        {
            if(maxNumbersBetween < 0)
            {
                throw new ArgumentException(LocalStrings.ArgumentNotNegative, "maxNumbersBetween");
            }

            // NaN's should never equal to anything
            if(Double.IsNaN(a) || Double.IsNaN(b))
            {
                /* (a != a || b != b) */

                return false;
            }

            if(a == b)
            {
                return true;
            }

            // false, if only one of them is infinity or they differ on the infinity sign
            if(Double.IsInfinity(a) || Double.IsInfinity(b))
            {
                return false;
            }

            ulong between = NumbersBetween(a, b);
            return between <= maxNumbersBetween;
        }

        /// <summary>
        /// Checks whether two real numbers are almost equal.
        /// </summary>
        /// <param name="a">The first number</param>
        /// <param name="b">The second number</param>
        /// <param name="diff">The difference of the two numbers according to the Norm</param>
        /// <param name="maximumRelativeError">The relative accuracy required for being almost equal.</param>
        /// <returns>
        /// True if <paramref name="a"/> and <paramref name="b"/> are almost equal up to a maximum
        /// relative error of <paramref name="maximumRelativeError"/>, False otherwise.
        /// </returns>
        public static
        bool
        AlmostEqualNorm(
            double a,
            double b,
            double diff,
            double maximumRelativeError)
        {
            if((a == 0 && Math.Abs(b) < maximumRelativeError)
                || (b == 0 && Math.Abs(a) < maximumRelativeError))
            {
                return true;
            }

            return Math.Abs(diff) < maximumRelativeError * Math.Max(Math.Abs(a), Math.Abs(b));
        }

        /// <summary>
        /// Checks whether two real numbers are almost equal.
        /// </summary>
        /// <param name="a">The first number</param>
        /// <param name="b">The second number</param>
        /// <param name="diff">The difference of the two numbers according to the Norm</param>
        /// <returns>
        /// True if <paramref name="a"/> and <paramref name="b"/> are almost equal up to a maximum
        /// relative error of 10*2^(-52) = 0.22e-14, False otherwise.
        /// </returns>
        public static
        bool
        AlmostEqualNorm(
            double a,
            double b,
            double diff)
        {
            return AlmostEqualNorm(a, b, diff, DefaultRelativeAccuracy);
        }

        /// <summary>
        /// Checks whether two real numbers are almost equal.
        /// </summary>
        /// <param name="a">The first number</param>
        /// <param name="b">The second number</param>
        /// <param name="maximumRelativeError">The relative accuracy required for being almost equal.</param>
        /// <returns>
        /// True if <paramref name="a"/> and <paramref name="b"/> are almost equal up to a maximum
        /// relative error of <paramref name="maximumRelativeError"/>, False otherwise.
        /// </returns>
        public static
        bool
        AlmostEqual(
            double a,
            double b,
            double maximumRelativeError)
        {
            return AlmostEqualNorm(a, b, a - b, maximumRelativeError);
        }

        /// <summary>
        /// Checks whether two real numbers are almost equal.
        /// </summary>
        /// <param name="a">The first number</param>
        /// <param name="b">The second number</param>
        /// <returns>
        /// True if <paramref name="a"/> and <paramref name="b"/> are almost equal up to a maximum
        /// relative error of 10*2^(-52) = 0.22e-14, False otherwise.
        /// </returns>
        public static
        bool
        AlmostEqual(
            double a,
            double b)
        {
            return AlmostEqualNorm(a, b, a - b, DefaultRelativeAccuracy);
        }

        /// <summary>
        /// Checks whether two real arrays are almost equal.
        /// </summary>
        /// <param name="x">The first vector</param>
        /// <param name="y">The second vector</param>
        /// <returns>
        /// True if all components with the same index of <paramref name="x"/> and <paramref name="y"/>
        /// are almost equal up to a maximum relative error of 10*2^(-52) = 0.22e-14, False otherwise.
        /// </returns>
        public static
        bool
        AlmostEqual(
            double[] x,
            double[] y)
        {
            if(x.Length != y.Length)
            {
                return false;
            }

            for(int i = 0; i < x.Length; i++)
            {
                if(!AlmostEqual(x[i], y[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// True if the given number is almost equal to zero, according to the specified absolute accuracy.
        /// </summary>
        /// <param name="a">The real number to check for being almost zero.</param>
        /// <param name="maximumAbsoluteError">The absolute threshold for <paramref name="a"/> to consider it as zero.</param>
        /// <returns>
        /// True if |<paramref name="a"/>| is smaller than <paramref name="maximumAbsoluteError"/>, False otherwise.
        /// </returns>
        public static
        bool
        AlmostZero(
            double a,
            double maximumAbsoluteError)
        {
            return Math.Abs(a) < maximumAbsoluteError;
        }

        /// <summary>
        /// True if the given number is almost equal to zero.
        /// </summary>
        /// <param name="a">The real number to check for being almost zero.</param>
        /// <returns>
        /// True if |<paramref name="a"/>| is smaller than 10*2^(-52) = 0.22e-14, False otherwise.
        /// </returns>
        public static
        bool
        AlmostZero(double a)
        {
            return Math.Abs(a) < DefaultRelativeAccuracy;
        }

        /// <summary>
        /// Forces small numbers near zero to zero, according to the specified absolute accuracy.
        /// </summary>
        /// <param name="a">The real number to coerce to zero, if it is almost zero.</param>
        /// <param name="maximumAbsoluteError">The absolute threshold for <paramref name="a"/> to consider it as zero.</param>
        /// <returns>Zero if |<paramref name="a"/>| is smaller than <paramref name="maximumAbsoluteError"/>, <paramref name="a"/> otherwise.</returns>
        public static
        double
        CoerceZero(
            double a,
            double maximumAbsoluteError)
        {
            if(Math.Abs(a) < maximumAbsoluteError)
            {
                return 0d;
            }

            return a;
        }

        /// <summary>
        /// Forces small numbers near zero to zero.
        /// </summary>
        /// <param name="a">The real number to coerce to zero, if it is almost zero.</param>
        /// <returns>Zero if |<paramref name="a"/>| is smaller than 10*2^(-52) = 0.22e-14, <paramref name="a"/> otherwise.</returns>
        public static
        double
        CoerceZero(double a)
        {
            if(Math.Abs(a) < DefaultRelativeAccuracy)
            {
                return 0d;
            }

            return a;
        }

        /// <summary>
        /// True if two instances of T are almost equal, up to the default maximum relative error.
        /// </summary>
        public static
            bool
            AlmostEqual<T>(
            T x,
            T y)
            where T : IAlmostEquatable<T>
        {
            if(ReferenceEquals(x, y))
            {
                return true;
            }

            if(ReferenceEquals(x, null) || ReferenceEquals(y, null))
            {
                return false;
            }

            return x.AlmostEquals(y);
        }

        /// <summary>
        /// True if two instances of T are almost equal, up to the provided maximum relative error.
        /// </summary>
        public static
            bool
            AlmostEqual<T>(
            T x,
            T y,
            double maximumRelativeError)
            where T : IAlmostEquatable<T>
        {
            if(ReferenceEquals(x, y))
            {
                return true;
            }

            if(ReferenceEquals(x, null) || ReferenceEquals(y, null))
            {
                return false;
            }

            return x.AlmostEquals(y, maximumRelativeError);
        }
    }
}
