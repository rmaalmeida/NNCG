//-----------------------------------------------------------------------
// <copyright file="PoissonDistribution.cs" company="Math.NET Project">
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
    /// Pseudo-random generation of Poisson distributed deviates.
    /// </summary>
    /// <remarks> 
    /// <para>
    /// For details about this distribution, see 
    /// <a href="http://en.wikipedia.org/wiki/Poisson_distribution">
    /// Wikipedia - Poisson distribution</a>.
    /// </para>
    /// <para>
    /// For details of the algorithm, see
    /// <a href="http://www.lkn.ei.tum.de/lehre/scn/cncl/doc/html/cncl_toc.html">
    /// Communication Networks Class Library (TU M�nchen)</a>
    /// </para>
    /// <para>
    /// pdf: f(x) = exp(-l)*l^x/x!; l = lambda
    /// </para>
    /// </remarks>
    public sealed class PoissonDistribution : DiscreteDistribution
    {
        double _lambda;
        double _helper1;

        #region construction
        /// <summary>
        /// Initializes a new instance of the PoissonDistribution class,
        /// using a <see cref="SystemRandomSource"/> as underlying random number generator.
        /// </summary>
        public
        PoissonDistribution()
        {
            SetDistributionParameters(1.0);
        }

        /// <summary>
        /// Initializes a new instance of the PoissonDistribution class,
        /// using the specified <see cref="RandomSource"/> as underlying random number generator.
        /// </summary>
        /// <param name="random">A <see cref="RandomSource"/> object.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="random"/> is NULL (<see langword="Nothing"/> in Visual Basic).
        /// </exception>
        public
        PoissonDistribution(RandomSource random)
            : base(random)
        {
            SetDistributionParameters(1.0);
        }

        /// <summary>
        /// Initializes a new instance of the PoissonDistribution class,
        /// using a <see cref="SystemRandomSource"/> as underlying random number generator.
        /// </summary>
        public
        PoissonDistribution(double lambda)
        {
            SetDistributionParameters(lambda);
        }
        #endregion

        #region Distribution Parameters
        /// <summary>
        /// Gets or sets the lambda parameter.
        /// </summary>
        public double Lambda
        {
            get { return _lambda; }
            set { SetDistributionParameters(value); }
        }

        /// <summary>
        /// Configure all distribution parameters.
        /// </summary>
        public
        void
        SetDistributionParameters(double lambda)
        {
            if(!IsValidParameterSet(lambda))
            {
                throw new ArgumentException(Properties.LocalStrings.ArgumentParameterSetInvalid, "lambda");
            }

            _lambda = lambda;
            _helper1 = Math.Exp(-_lambda);
        }

        /// <summary>
        /// Determines whether the specified parameters are valid.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if value is greater than 0.0; otherwise, <see langword="false"/>.
        /// </returns>
        public static
        bool
        IsValidParameterSet(double lambda)
        {
            return lambda > 0.0;
        }
        #endregion

        #region Distribution Properties
        /// <summary>
        /// Gets the minimum possible value of generated random numbers.
        /// </summary>
        public override int Minimum
        {
            get { return 0; }
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
            get { return _lambda; }
        }

        /// <summary>
        /// Gets the median of generated random numbers.
        /// </summary>
        public override int Median
        { // approximation, see Wikipedia
            get { return (int)Math.Floor(_lambda + (1.0 / 3.0) - (0.2 / _lambda)); }
        }

        /// <summary>
        /// Gets the variance of generated random numbers.
        /// </summary>
        public override double Variance
        {
            get { return _lambda; }
        }

        /// <summary>
        /// Gets the skewness of generated random numbers.
        /// </summary>
        public override double Skewness
        {
            get { return Math.Sqrt(_lambda); }
        }

        /// <summary>
        /// Discrete probability mass function (pmf) of this probability distribution.
        /// </summary>
        public override
        double
        ProbabilityMass(int x)
        {
            return Math.Exp(
                -_lambda
                + (x * Math.Log(_lambda))
                - Fn.FactorialLn(x));
        }

        /// <summary>
        /// Continuous cumulative distribution function (cdf) of this probability distribution.
        /// </summary>
        public override
        double
        CumulativeDistribution(double x)
        {
            return 1.0 - Fn.GammaRegularized(x + 1, _lambda);
        }
        #endregion

        #region Generator
        /// <summary>
        /// Returns a Poisson distributed random number.
        /// </summary>
        /// <returns>A Poisson distributed 32-bit signed integer.</returns>
        public override
        int
        NextInt32()
        {
            int count = 0;
            for(double product = RandomSource.NextDouble(); product >= _helper1; product *= RandomSource.NextDouble())
            {
                count++;
            }

            return count;
        }
        #endregion
    }
}
