//-----------------------------------------------------------------------
// <copyright file="DiscreteDistribution.cs" company="Math.NET Project">
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
// </contribution>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace MathNet.Numerics.Distributions
{
    using RandomSources;

    /// <summary>
    /// Declares common functionality for all discrete random number
    /// distributions based on a random source.
    /// </summary>
    public abstract class DiscreteDistribution :
        IDiscreteGenerator,
        IDiscreteProbabilityDistribution
    {
        RandomSource _random;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscreteDistribution"/> class, using a 
        /// <see cref="SystemRandomSource"/> as underlying random number generator.
        /// </summary>
        protected
        DiscreteDistribution()
            : this(new SystemRandomSource())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscreteDistribution"/> class, using the
        /// specified <see cref="RandomSource"/> as underlying random number generator.
        /// </summary>
        /// <param name="random">A <see cref="RandomSource"/> object.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="random"/> is NULL (<see langword="Nothing"/> in Visual Basic).
        /// </exception>
        protected
        DiscreteDistribution(RandomSource random)
        {
            if(random == null)
            {
                throw new ArgumentNullException("random", Properties.LocalStrings.ArgumentNull("generator"));
            }

            _random = random;
        }

        /// <summary>
        /// Returns a distributed integer random number.
        /// </summary>
        /// <returns>A distributed 32 bit signed integer number.</returns>
        public abstract
        int
        NextInt32();

        /// <summary>
        /// Enumerates over infinitely many integer random numbers.
        /// </summary>
        public
        IEnumerable<int>
        EnumerateInt32()
        {
            while(true)
            {
                yield return NextInt32();
            }
        }

        /// <summary>
        /// Enumerates over a fixed number of integer random numbers.
        /// </summary>
        public
        IEnumerable<int>
        EnumerateInt32(int count)
        {
            for(int i = 0; i < count; i++)
            {
                yield return NextInt32();
            }
        }

        /// <summary>
        /// Discrete probability mass function (pmf) of this probability distribution.
        /// </summary>
        public abstract
        double
        ProbabilityMass(int x);

        /// <summary>
        /// Continuous cumulative distribution function (cdf) of this probability distribution.
        /// </summary>
        public abstract
        double
        CumulativeDistribution(double x);

        /// <summary>
        /// Resets the random number distribution, so that it produces the same random number sequence again.
        /// </summary>
        public
        void
        Reset()
        {
            _random.Reset();
        }

        /// <summary>
        /// Lower limit of a random variable with this probability distribution.
        /// </summary>
        public abstract int Minimum
        {
            get;
        }

        /// <summary>
        /// Upper limit of a random variable with this probability distribution.
        /// </summary>
        public abstract int Maximum
        {
            get;
        }

        /// <summary>
        /// The expected value of a random variable with this probability distribution.
        /// </summary>
        public abstract double Mean
        {
            get;
        }

        /// <summary>
        /// The value separating the lower half part from the upper half part of a random variable with this probability distribution.
        /// </summary>
        public abstract int Median
        {
            get;
        }

        /// <summary>
        /// Average of the squared distances to the expected value of a random variable with this probability distribution.
        /// </summary>
        public abstract double Variance
        {
            get;
        }

        /// <summary>
        /// Measure of the asymmetry of this probability distribution.
        /// </summary>
        public abstract double Skewness
        {
            get;
        }

        /// <summary>
        /// Gets or sets a <see cref="RandomSource"/> object that can be used
        /// as underlying random number generator.
        /// </summary>
        public virtual RandomSource RandomSource
        {
            get { return _random; }
            set { _random = value; }
        }

        /// <summary>
        /// Gets a value indicating whether the random number distribution can be reset,
        /// so that it produces the same  random number sequence again.
        /// </summary>
        public bool CanReset
        {
            get { return _random.CanReset; }
        }
    }
}
