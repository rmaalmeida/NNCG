//-----------------------------------------------------------------------
// <copyright file="HypergeometricDistribution.cs" company="Math.NET Project">
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
    /// Provides generation of hyper geometric distributed random numbers.
    /// </summary>
    /// <remarks>
    /// The hyper geometric distribution generates only discrete numbers.<br />
    /// The implementation bases upon information presented on
    ///   <a href="http://en.wikipedia.org/wiki/Hypergeometric_distribution">Wikipedia - Geometric distribution</a>.
    /// </remarks>
    public sealed class HypergeometricDistribution : DiscreteDistribution
    {
        int _N;
        int _M;
        int _n;
        double _p;

        #region Construction
        /// <summary>
        /// Initializes a new instance of the HypergeometricDistribution class,
        /// using a <see cref="SystemRandomSource"/> as underlying random number generator.
        /// </summary>
        public
        HypergeometricDistribution()
        {
            SetDistributionParameters(2, 1, 1);
        }

        /// <summary>
        /// Initializes a new instance of the HypergeometricDistribution class,
        /// using the specified <see cref="RandomSource"/> as underlying random number generator.
        /// </summary>
        /// <param name="random">A <see cref="RandomSource"/> object.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="random"/> is NULL (<see langword="Nothing"/> in Visual Basic).
        /// </exception>
        public
        HypergeometricDistribution(RandomSource random)
            : base(random)
        {
            SetDistributionParameters(2, 1, 1);
        }

        /// <summary>
        /// Initializes a new instance of the HypergeometricDistribution class,
        /// using a <see cref="SystemRandomSource"/> as underlying random number generator.
        /// </summary>
        public
        HypergeometricDistribution(
            int populationSize,
            int favoredItems,
            int numberOfSamples)
        {
            SetDistributionParameters(populationSize, favoredItems, numberOfSamples);
        }
        #endregion

        #region Distribution Parameters
        /// <summary>
        /// Gets or sets the population size parameter.
        /// </summary>
        public int PopulationSize
        {
            get { return _N; }
            set { SetDistributionParameters(value, _M, _n); }
        }

        /// <summary>
        /// Gets or sets the number of items of the population that are in favor.
        /// </summary>
        public int FavoredItems
        {
            get { return _M; }
            set { SetDistributionParameters(_N, value, _n); }
        }

        /// <summary>
        /// Gets or sets the number of samples.
        /// </summary>
        public int NumberOfSamples
        {
            get { return _n; }
            set { SetDistributionParameters(_N, _M, value); }
        }

        /// <summary>
        /// Configure all distribution parameters.
        /// </summary>
        public
        void
        SetDistributionParameters(
            int populationSize,
            int favoredItems,
            int numberOfSamples)
        {
            if(!IsValidParameterSet(populationSize, favoredItems, numberOfSamples))
            {
                throw new ArgumentException(Properties.LocalStrings.ArgumentParameterSetInvalid);
            }

            _N = populationSize;
            _M = favoredItems;
            _n = numberOfSamples;
            _p = favoredItems / (double)populationSize;
        }

        /// <summary>
        /// Determines whether the specified parameters is valid.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if values are greater than or equal to 0.0 and both favored items and number of samples are not bigger than the population size; otherwise, <see langword="false"/>.
        /// </returns>
        public static
        bool
        IsValidParameterSet(
            int populationSize,
            int favoredItems,
            int numberOfSamples)
        {
            return populationSize >= 0
                && favoredItems >= 0
                && favoredItems <= populationSize
                && numberOfSamples >= 0
                && numberOfSamples <= populationSize;
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
            get { return Math.Min(_M, _n); }
        }

        /// <summary>
        /// Gets the mean value of generated random numbers.
        /// </summary>
        public override double Mean
        {
            get { return _n * _p; }
        }

        /// <summary>
        /// Gets the median of generated random numbers.
        /// Throws <see cref="NotSupportedException"/> since
        /// the value is not defined for this distribution.
        /// </summary>
        /// <exception cref="NotSupportedException">Always.</exception>
        public override int Median
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Gets the variance of generated random numbers.
        /// </summary>
        public override double Variance
        {
            get { return _n * _p * (1.0 - _p) * (_N - _n) / (_N - 1.0); }
        }

        /// <summary>
        /// Gets the skewness of generated random numbers.
        /// </summary>
        public override double Skewness
        {
            get
            {
                double d1 = (_N - (2 * _M)) * (_N - (2 * _n)) * Math.Sqrt(_N - 1.0);
                double d2 = (_N - 2) * Math.Sqrt(_n * _M * (_N - _M) * (_N - _n));
                return d1 / d2;
            }
        }

        /// <summary>
        /// Discrete probability mass function (pmf) of this probability distribution.
        /// </summary>
        public override
        double
        ProbabilityMass(int x)
        {
            return Math.Exp(
                Fn.BinomialCoefficientLn(_M, x)
                + Fn.BinomialCoefficientLn(_N - _M, _n - x)
                - Fn.BinomialCoefficientLn(_N, _n));
        }

        /// <summary>
        /// Continuous cumulative distribution function (cdf) of this probability distribution.
        /// </summary>
        public override
        double
        CumulativeDistribution(double x)
        {
            double pdf = Math.Exp(Fn.BinomialCoefficientLn(_N - _M, _n) - Fn.BinomialCoefficientLn(_N, _n));
            double cdf = pdf;

            for(int xx = 0; xx <= x - 1; xx++)
            {
                pdf *= (_M - xx) * (_n - xx) / (double)((xx + 1) * (_N - _M - _n + xx + 1));
                cdf += pdf;
            }

            return cdf;
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
            throw new NotImplementedException();
        }
        #endregion
    }
}
