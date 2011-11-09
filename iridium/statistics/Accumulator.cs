//-----------------------------------------------------------------------
// <copyright file="Accumulator.cs" company="Math.NET Project">
//    Copyright (c) 2004-2009, Christoph R�egg, Joannes Vermorel.
//    All Right Reserved.
// </copyright>
// <author>
//    Christoph R�egg, http://christoph.ruegg.name
//    Joannes Vermorel, http://www.vermorel.com
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
using System.Collections;
using System.Collections.Generic;

namespace MathNet.Numerics.Statistics
{
    /// <summary>
    /// The <c>Accumulator</c> provides online algorithms to computes the first
    /// statistical moments and their derivatives.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <c>Accumulator</c> provides memory efficient online algorithms
    /// to compute the first statistical moments (mean, variance) and their
    /// derivatives (sigma, error estimate).
    /// </para>
    /// <para>
    /// The memory required by the accumulator is <c>O(1)</c> independent
    /// from the distribution size. All methods are executed in a <c>O(1)</c>
    /// computational time.
    /// </para>
    /// <para>
    /// The <c>Accumulator</c> is not thread safe.
    /// </para>
    /// </remarks>
    public class Accumulator
    {
        /* Design note (joannes):
         * The Min/Max have not been included on purpose. It usually clearer
         * (because being trivial) to manage explicitly in the client the Min/Max 
         * than using a library to do so.
         * 
         * The skewness and kurtosis have not been included because I never heard of
         * anyone using those indicator in practice.
         * */

        /// <summary>
        /// Sum of the values added to the accumulator.
        /// </summary>
        private double _sum;

        /// <summary>
        /// Sum of the square of the values added to the accumulator.
        /// </summary>
        private double _squaredSum;

        /// <summary>
        /// Number of values added to the accumulator.
        /// </summary>
        private int _count;

        // Variables required for numerically more stable way of computing the mean and variance:
        private double _mean;
        private double _varianceSum;

        /// <summary>
        /// Initializes a new instance of the Accumulator class.
        /// </summary>
        public Accumulator()
        {
            Clear();
        }

        /// <summary>
        /// Initializes a new instance of the Accumulator class that contains the provided values.
        /// </summary>
        public Accumulator(double[] values)
        {
            Clear();
            AddRange(values);
        }

        /// <summary>
        /// Initializes a new instance of the Accumulator class that contains the provided values.
        /// </summary>
        [Obsolete("This method is obsolete, please use the generic version instead: Accumulator(IEnumerable<double>)", false)]
        public Accumulator(ICollection values)
        {
            Clear();
            AddRange(values);
        }

        /// <summary>
        /// Initializes a new instance of the Accumulator class that contains the provided values.
        /// </summary>
        public Accumulator(IEnumerable<double> values)
        {
            Clear();
            AddRange(values);
        }

        #region Add/Remove Samples
        /// <summary>
        /// Adds a real value to the accumulator.
        /// </summary>
        public void Add(double value)
        {
            _sum += value;
            _squaredSum += value * value;

            _count++;

            if(_count == 1)
            {
                _mean = value;
                _varianceSum = 0;
            }
            else
            {
                double meanPrevious = _mean;
                _mean = meanPrevious + ((value - meanPrevious) / _count);
                _varianceSum = _varianceSum + ((value - meanPrevious) * (value - _mean));
            }
        }

        /// <summary>
        /// Adds a range of values to the accumulator.
        /// </summary>
        public void AddRange(double[] values)
        {
            for(int i = 0; i < values.Length; i++)
            {
                Add(values[i]);
            }
        }

        /// <summary>
        /// Adds a range of values to the accumulator.
        /// </summary>
        [Obsolete("This method is obsolete, please use the generic version instead: AddRange(IEnumerable<double>)", false)]
        public void AddRange(ICollection values)
        {
            foreach(object obj in values)
            {
                if(!(obj is double))
                {
                    throw new ArgumentException(Properties.LocalStrings.ArgumentTypeMismatch);
                }

                Add((double)obj);
            }
        }

        /// <summary>
        /// Adds a range of values to the accumulator.
        /// </summary>
        public void AddRange(IEnumerable<double> values)
        {
            foreach(double v in values)
            {
                Add(v);
            }
        }

        /// <summary>
        /// Clears (re-initialize) the accumulator.
        /// </summary>
        public void Clear()
        {
            _sum = 0;
            _squaredSum = 0;
            _mean = 0;
            _varianceSum = 0;
            _count = 0;
        }

        /// <summary>
        /// Removes a value from the accumulator.
        /// </summary>
        /// <remarks>
        /// <p>Caution: the <c>Accumulator</c> does not explicitly records the
        /// added values. Therefore, no exception will be thrown if an attempt
        /// is made to remove a value that have not been previously added to
        /// the accumulator.</p>
        /// </remarks>
        public void Remove(double value)
        {
            if(_count <= 0)
            {
                throw new InvalidOperationException(Properties.LocalStrings.InvalidOperationAccumulatorEmpty);
            }

            _sum -= value;
            _squaredSum -= value * value;

            double meanPrevious = _mean;
            _mean = ((meanPrevious * _count) - value) / (_count - 1);
            _varianceSum = _varianceSum - ((value - _mean) * (value - meanPrevious));

            _count--;
        }

        /// <summary>
        /// Removes a range of values from the accumulator.
        /// </summary>
        public void RemoveRange(double[] values)
        {
            for(int i = 0; i < values.Length; i++)
            {
                Remove(values[i]);
            }
        }

        /// <summary>
        /// Removes a range of values from the accumulator.
        /// </summary>
        [Obsolete("This method is obsolete, please use the generic version instead: RemoveRange(IEnumerable<double>)", false)]
        public void RemoveRange(ICollection values)
        {
            foreach(object obj in values)
            {
                if(!(obj is double))
                {
                    throw new ArgumentException(Properties.LocalStrings.ArgumentTypeMismatch);
                }

                Remove((double)obj);
            }
        }

        /// <summary>
        /// Removes a range of values from the accumulator.
        /// </summary>
        public void RemoveRange(IEnumerable<double> values)
        {
            foreach(double v in values)
            {
                Remove(v);
            }
        }
        #endregion

        /// <summary>
        /// Gets the number of values added to the accumulator.
        /// </summary>
        public int Count
        {
            get { return _count; }
        }

        /// <summary>
        /// Gets the sum of the values added to the accumulator.
        /// </summary>
        public double Sum
        {
            get { return _sum; }
        }

        /// <summary>
        /// Gets the sum of the squared values added to the accumulator.
        /// </summary>
        public double SquaredSum
        {
            get { return _squaredSum; }
        }

        /// <summary>
        /// Gets the arithmetic mean of the values added to the accumulator.
        /// </summary>
        public double Mean
        {
            get
            {
                if(_count <= 0)
                {
                    throw new InvalidOperationException(Properties.LocalStrings.InvalidOperationAccumulatorEmpty);
                }

                return _mean;
            }
        }

        /// <summary>
        /// Gets the arithmetic mean of the squared values added to the accumulator. Note that this is not equal to the squared mean of the values.
        /// </summary>
        public double MeanSquared
        {
            get
            {
                if(_count <= 0)
                {
                    throw new InvalidOperationException(Properties.LocalStrings.InvalidOperationAccumulatorEmpty);
                }

                return _squaredSum / _count;
            }
        }

        /// <summary>
        /// Gets the variance of the values added to the accumulator.
        /// </summary>
        public double Variance
        {
            get
            {
                if(_count <= 0)
                {
                    throw new InvalidOperationException(Properties.LocalStrings.InvalidOperationAccumulatorEmpty);
                }

                return _varianceSum / (_count - 1);
            }
        }

        /// <summary>
        /// Gets the standard deviation of the values added to the accumulator.
        /// </summary>
        public double Sigma
        {
            get
            {
                return Math.Sqrt(Variance);
            }
        }

        /// <summary>
        /// Gets the mean error estimate defined as the square root of the ratio of 
        /// the standard deviation to the number of values added to the accumulator.
        /// </summary>
        public double ErrorEstimate
        {
            get
            {
                if(_count <= 0)
                {
                    throw new InvalidOperationException(Properties.LocalStrings.InvalidOperationAccumulatorEmpty);
                }

                return Sigma / Math.Sqrt(_count);
            }
        }
    }
}
