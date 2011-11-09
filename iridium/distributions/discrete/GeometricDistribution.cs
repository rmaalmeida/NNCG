//-----------------------------------------------------------------------
// <copyright file="GeometricDistribution.cs" company="Math.NET Project">
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
// <contribution>
//    Troschuetz.Random Class Library, Stefan Trosch�tz (stefan@troschuetz.de)
//    CNClass, Aachen University of Technology, Dirk Grunwald (grunwald@cs.uiuc.edu)
// </contribution>
//-----------------------------------------------------------------------

using System;

namespace MathNet.Numerics.Distributions
{
    using RandomSources;

    /// <summary>
    /// Provides generation of geometric distributed random numbers.
    /// </summary>
    /// <remarks>
    /// The geometric distribution generates only discrete numbers.<br />
    /// The implementation bases upon information presented on
    ///   <a href="http://en.wikipedia.org/wiki/Geometric_distribution">Wikipedia - Geometric distribution</a>
    ///   and the implementation in the <a href="http://www.lkn.ei.tum.de/lehre/scn/cncl/doc/html/cncl_toc.html">
    ///   Communication Networks Class Library</a>.
    /// </remarks>
    public sealed class GeometricDistribution : DiscreteDistribution
    {
        double _p;

        #region Construction
        /// <summary>
        /// Initializes a new instance of the GeometricDistribution class,
        /// using a <see cref="SystemRandomSource"/> as underlying random number generator.
        /// </summary>
        public
        GeometricDistribution()
        {
            SetDistributionParameters(0.5);
        }

        /// <summary>
        /// Initializes a new instance of the GeometricDistribution class,
        /// using the specified <see cref="RandomSource"/> as underlying random number generator.
        /// </summary>
        /// <param name="random">A <see cref="RandomSource"/> object.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="random"/> is NULL (<see langword="Nothing"/> in Visual Basic).
        /// </exception>
        public
        GeometricDistribution(RandomSource random)
            : base(random)
        {
            SetDistributionParameters(0.5);
        }

        /// <summary>
        /// Initializes a new instance of the GeometricDistribution class,
        /// using a <see cref="SystemRandomSource"/> as underlying random number generator.
        /// </summary>
        public
        GeometricDistribution(double probabilityOfSuccess)
        {
            SetDistributionParameters(probabilityOfSuccess);
        }
        #endregion

        #region Distribution Parameters
        /// <summary>
        /// Gets or sets the success probability parameter.
        /// </summary>
        public double ProbabilityOfSuccess
        {
            get { return _p; }
            set { SetDistributionParameters(value); }
        }

        /// <summary>
        /// Configure all distribution parameters.
        /// </summary>
        public
        void
        SetDistributionParameters(double probabilityOfSuccess)
        {
            if(!IsValidParameterSet(probabilityOfSuccess))
            {
                throw new ArgumentException(Properties.LocalStrings.ArgumentParameterSetInvalid, "probabilityOfSuccess");
            }

            _p = probabilityOfSuccess;
        }

        /// <summary>
        /// Determines whether the specified parameters are valid.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if value is greater than or equal to 0.0, and less than or equal to 1.0; otherwise, <see langword="false"/>.
        /// </returns>
        public static
        bool
        IsValidParameterSet(double probabilityOfSuccess)
        {
            return probabilityOfSuccess >= 0.0 && probabilityOfSuccess <= 1.0;
        }
        #endregion

        #region Distribution Properties
        /// <summary>
        /// Gets the minimum possible value of generated random numbers.
        /// </summary>
        public override int Minimum
        {
            get { return 1; }
        }

        /// <summary>
        /// Gets the maximum possible value of generated random numbers.
        /// </summary>
        public override int Maximum
        {
            get { return int.MaxValue; }
        }

        /// <summary>
        /// Gets the mean value of generated random numbers.
        /// </summary>
        public override double Mean
        {
            get { return 1.0 / _p; }
        }

        /// <summary>
        /// Gets the median of generated random numbers.
        /// </summary>
        public override int Median
        {
            get { return (int)Math.Ceiling(-Constants.Ln2 / Math.Log(1.0 - _p)); }
        }

        /// <summary>
        /// Gets the variance of generated random numbers.
        /// </summary>
        public override double Variance
        {
            get { return (1.0 - _p) / (_p * _p); }
        }

        /// <summary>
        /// Gets the skewness of generated random numbers.
        /// </summary>
        public override double Skewness
        {
            get { return (2.0 - _p) / Math.Sqrt(1.0 - _p); }
        }

        /// <summary>
        /// Discrete probability mass function (pmf) of this probability distribution.
        /// </summary>
        public override
        double
        ProbabilityMass(int x)
        {
            return _p * Math.Pow(1.0 - _p, x - 1);
        }

        /// <summary>
        /// Continuous cumulative distribution function (cdf) of this probability distribution.
        /// </summary>
        public override
        double
        CumulativeDistribution(double x)
        {
            return 1.0 - Math.Pow(1.0 - _p, x);
        }
        #endregion

        #region Generator
        /// <summary>
        /// Returns a geometric distributed floating point random number.
        /// </summary>
        /// <returns>A geometric distributed double-precision floating point number.</returns>
        public override
        int
        NextInt32()
        {
            // TODO: Implement direct transformation instead of simulation
            int samples;

            for(samples = 1; RandomSource.NextDouble() >= _p; samples++)
            {
            }

            return samples;
        }
        #endregion
    }
}
