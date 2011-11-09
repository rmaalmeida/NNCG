//-----------------------------------------------------------------------
// <copyright file="ComplexVector.cs" company="Math.NET Project">
//    Copyright (c) 2004-2009, Christoph Rüegg.
//    All Right Reserved.
// </copyright>
// <author>
//    Christoph Rüegg, http://christoph.ruegg.name
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
using System.Collections.Generic;
using System.Text;

namespace MathNet.Numerics.LinearAlgebra
{
    using Distributions;

    /// <summary>
    /// Complex Vector.
    /// </summary>
    [Serializable]
    public class ComplexVector :
        IList<Complex>,
        IEquatable<ComplexVector>,
        IAlmostEquatable<ComplexVector>,
        ICloneable
    {
        private readonly Complex[] _data;
        private readonly int _length;

        /// <summary>
        /// Gets dimensionality of the vector.
        /// </summary>
        public int Length
        {
            get { return _length; }
        }

        /// <summary>
        /// Gets or sets the element indexed by <c>i</c>
        /// in the <c>Vector</c>.
        /// </summary>
        /// <param name="i">Dimension index.</param>
        public Complex this[int i]
        {
            get { return _data[i]; }
            set { _data[i] = value; }
        }

        #region Constructors and static constructive methods

        /// <summary>
        /// Initializes a new instance of the ComplexVector class,
        /// for an n-dimensional vector of zeros.
        /// </summary>
        /// <param name="n">Dimensionality of vector.</param>
        public
        ComplexVector(int n)
        {
            _length = n;
            _data = new Complex[_length];
        }

        /// <summary>
        /// Initializes a new instance of the ComplexVector class,
        /// for the n-dimensional unit vector for the i-th dimension.
        /// </summary>
        /// <param name="n">Dimensionality of vector.</param>
        /// <param name="i">Coordinate index.</param>
        public
        ComplexVector(int n, int i)
        {
            _length = n;
            _data = new Complex[_length];
            _data[i] = 1.0;
        }

        /// <summary>
        /// Initializes a new instance of the ComplexVector class,
        /// for the n-dimensional vector where all cells are filled with the provided value.
        /// </summary>
        /// <param name="n">Dimensionality of vector.</param>
        /// <param name="value">Fill the vector with this scalar value.</param>
        public
        ComplexVector(
            int n,
            Complex value)
        {
            _length = n;
            _data = new Complex[_length];
            for(int i = 0; i < _length; i++)
            {
                _data[i] = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the ComplexVector class
        /// from a 1-D array, directly using the provided array as internal data structure.
        /// </summary>
        /// <param name="components">One-dimensional array of doubles.</param>
        /// <seealso cref="Create(Complex[])"/>
        public
        ComplexVector(Complex[] components)
        {
            _length = components.Length;
            _data = components;
        }

        /// <summary>
        /// Constructs a vector from a copy of a 1-D array.
        /// </summary>
        /// <param name="components">One-dimensional array of doubles.</param>
        public static
        ComplexVector
        Create(Complex[] components)
        {
            if(null == components)
            {
                throw new ArgumentNullException("components");
            }

            Complex[] newData = new Complex[components.Length];
            components.CopyTo(newData, 0);

            return new ComplexVector(newData);
        }

        /// <summary>
        /// Constructs a complex vector from a real and an imaginary vector.
        /// </summary>
        /// <param name="realComponents">One-dimensional array of doubles representing the real part of the vector.</param>
        /// <param name="imagComponents">One-dimensional array of doubles representing the imaginary part of the vector.</param>
        public static
        ComplexVector
        Create(
            IList<double> realComponents,
            IList<double> imagComponents)
        {
            if(null == realComponents)
            {
                throw new ArgumentNullException("realComponents");
            }

            if(null == imagComponents)
            {
                throw new ArgumentNullException("imagComponents");
            }

            if(realComponents.Count != imagComponents.Count)
            {
                throw new ArgumentException(Properties.LocalStrings.ArgumentVectorsSameLengths);
            }

            Complex[] newData = new Complex[realComponents.Count];
            for(int i = 0; i < newData.Length; i++)
            {
                newData[i] = new Complex(realComponents[i], imagComponents[i]);
            }

            return new ComplexVector(newData);
        }

        /// <summary>
        /// Constructs a complex vector from a real vector.
        /// </summary>
        /// <param name="realComponents">One-dimensional array of doubles representing the real part of the vector.</param>
        public static
        ComplexVector
        Create(IList<double> realComponents)
        {
            if(null == realComponents)
            {
                throw new ArgumentNullException("realComponents");
            }

            Complex[] newData = new Complex[realComponents.Count];
            for(int i = 0; i < newData.Length; i++)
            {
                newData[i] = new Complex(realComponents[i], 0d);
            }

            return new ComplexVector(newData);
        }

        /// <summary>
        /// Generates vector with random elements
        /// </summary>
        /// <param name="n">Dimensionality of vector.</param>
        /// <param name="randomDistribution">Continuous Random Distribution or Source</param>
        /// <returns>
        /// An n-dimensional vector with random elements distributed according
        /// to the specified random distribution.
        /// </returns>
        public static
        ComplexVector
        Random(
            int n,
            IContinuousGenerator randomDistribution)
        {
            Complex[] data = new Complex[n];
            for(int i = 0; i < data.Length; i++)
            {
                data[i] = Complex.Random(
                    randomDistribution,
                    randomDistribution);
            }

            return new ComplexVector(data);
        }

        /// <summary>
        /// Generates an n-dimensional vector filled with 1.
        /// </summary>
        /// <param name="n">Dimensionality of vector.</param>
        public static
        ComplexVector
        Ones(int n)
        {
            return new ComplexVector(n, 1.0);
        }

        /// <summary>
        /// Generates an n-dimensional vector filled with 0.
        /// </summary>
        /// <param name="n">Dimensionality of vector.</param>
        public static
        ComplexVector
        Zeros(int n)
        {
            return new ComplexVector(n);
        }

        /// <summary>
        /// Generates an n-dimensional unit vector for i-th coordinate.
        /// </summary>
        /// <param name="n">Dimensionality of vector.</param>
        /// <param name="i">Coordinate index.</param>
        public static
        ComplexVector
        BasisVector(int n, int i)
        {
            return new ComplexVector(n, i);
        }

        #endregion

        #region Conversion Operators and conversion to other types

        /// <summary>
        /// Returns a reference to the internal data structure.
        /// </summary>
        public static implicit
        operator Complex[](ComplexVector v)
        {
            return v._data;
        }

        /// <summary>
        /// Returns a vector bound directly to a reference of the provided array.
        /// </summary>
        public static implicit
        operator ComplexVector(Complex[] v)
        {
            return new ComplexVector(v);
        }

        /// <summary>
        /// Copies the internal data structure to an array.
        /// </summary>
        public
        Complex[]
        CopyToArray()
        {
            Complex[] newData = new Complex[_length];
            _data.CopyTo(newData, 0);
            return newData;
        }

        /// <summary>
        /// Create a matrix based on this vector in column form (one single column).
        /// </summary>
        public
        ComplexMatrix
        ToColumnMatrix()
        {
            Complex[][] m = ComplexMatrix.CreateMatrixData(_length, 1);
            for(int i = 0; i < _data.Length; i++)
            {
                m[i][0] = _data[i];
            }

            return new ComplexMatrix(m);
        }

        /// <summary>
        /// Create a matrix based on this vector in row form (one single row).
        /// </summary>
        public
        ComplexMatrix
        ToRowMatrix()
        {
            Complex[][] m = ComplexMatrix.CreateMatrixData(1, _length);
            Complex[] mRow = m[0];
            for(int i = 0; i < _data.Length; i++)
            {
                mRow[i] = _data[i];
            }

            return new ComplexMatrix(m);
        }

        #endregion

        #region Elementary operations

        /// <summary>
        /// Add another complex vector to this vector.
        /// </summary>
        /// <param name="b">The other complex vector.</param>
        /// <returns>
        /// Vector ret[i] = this[i] + b[i]
        /// </returns>
        /// <remarks>
        /// This method has the same effect as the overloaded + operator.
        /// </remarks>
        /// <seealso cref="AddInplace(IVector{Complex})"/>
        /// <seealso cref="operator + (ComplexVector, ComplexVector)"/>
        public
        ComplexVector
        Add(ComplexVector b)
        {
            CheckMatchingVectorDimensions(this, b);

            Complex[] v = new Complex[_length];
            for(int i = 0; i < v.Length; i++)
            {
                v[i] = _data[i] + b[i];
            }

            return new ComplexVector(v);
        }

        /// <summary>
        /// Add another real vector to this vector.
        /// </summary>
        /// <param name="b">The other real vector.</param>
        /// <returns>
        /// Vector ret[i] = this[i] + b[i]
        /// </returns>
        /// <remarks>
        /// This method has the same effect as the overloaded + operator.
        /// </remarks>
        /// <seealso cref="AddInplace(IVector{double})"/>
        /// <seealso cref="operator + (ComplexVector, Vector)"/>
        public
        ComplexVector
        Add(Vector b)
        {
            CheckMatchingVectorDimensions(this, b);

            Complex[] v = new Complex[_length];
            for(int i = 0; i < v.Length; i++)
            {
                v[i] = _data[i] + b._data[i];
            }

            return new ComplexVector(v);
        }

        /// <summary>
        /// Add a complex scalar to all elements of this vector.
        /// </summary>
        /// <param name="b">The complex scalar.</param>
        /// <returns>
        /// Vector ret[i] = this[i] + b
        /// </returns>
        /// <remarks>
        /// This method has the same effect as the overloaded + operator.
        /// </remarks>
        /// <seealso cref="AddInplace(Complex)"/>
        /// <seealso cref="operator + (ComplexVector, Complex)"/>
        public
        ComplexVector
        Add(Complex b)
        {
            Complex[] v = new Complex[_length];
            for(int i = 0; i < v.Length; i++)
            {
                v[i] = _data[i] + b;
            }

            return new ComplexVector(v);
        }

        /// <summary>
        /// Inplace addition of a complex vector to this vector.
        /// </summary>
        /// <remarks>
        /// This method changes this vector.
        /// </remarks>
        /// <seealso cref="Add(IVector{Complex})"/>
        /// <seealso cref="operator + (ComplexVector, ComplexVector)"/>
        public
        void
        AddInplace(ComplexVector b)
        {
            CheckMatchingVectorDimensions(this, b);

            for(int i = 0; i < _length; i++)
            {
                _data[i] += b[i];
            }
        }

        /// <summary>
        /// Inplace addition of a real vector to this vector.
        /// </summary>
        /// <remarks>
        /// This method changes this vector.
        /// </remarks>
        /// <seealso cref="Add(IVector{double})"/>
        /// <seealso cref="operator + (ComplexVector, Vector)"/>
        public
        void
        AddInplace(Vector b)
        {
            CheckMatchingVectorDimensions(this, b);

            for(int i = 0; i < _length; i++)
            {
                _data[i] += b._data[i];
            }
        }

        /// <summary>
        /// Inplace addition of a complex scalar to all elements of this vector.
        /// </summary>
        /// <remarks>
        /// This method changes this vector.
        /// </remarks>
        /// <seealso cref="Add(Complex)"/>
        /// <seealso cref="operator + (ComplexVector, Complex)"/>
        public
        void
        AddInplace(Complex b)
        {
            for(int i = 0; i < _length; i++)
            {
                _data[i] += b;
            }
        }

        /// <summary>
        /// Subtract a complex vector from this vector.
        /// </summary>
        /// <param name="b">The other complex vector.</param>
        /// <returns>
        /// Vector ret[i] = this[i] - b[i]
        /// </returns>
        /// <remarks>
        /// This method has the same effect as the overloaded - operator.
        /// </remarks>
        /// <seealso cref="SubtractInplace(IVector{Complex})"/>
        /// <seealso cref="operator - (ComplexVector, ComplexVector)"/>
        public
        ComplexVector
        Subtract(ComplexVector b)
        {
            CheckMatchingVectorDimensions(this, b);

            Complex[] v = new Complex[_length];
            for(int i = 0; i < v.Length; i++)
            {
                v[i] = _data[i] - b[i];
            }

            return new ComplexVector(v);
        }

        /// <summary>
        /// Subtract a real vector from this vector.
        /// </summary>
        /// <param name="b">The other real vector.</param>
        /// <returns>
        /// Vector ret[i] = this[i] - b[i]
        /// </returns>
        /// <remarks>
        /// This method has the same effect as the overloaded - operator.
        /// </remarks>
        /// <seealso cref="SubtractInplace(IVector{double})"/>
        /// <seealso cref="operator - (ComplexVector, Vector)"/>
        public
        ComplexVector
        Subtract(Vector b)
        {
            CheckMatchingVectorDimensions(this, b);

            Complex[] v = new Complex[_length];
            for(int i = 0; i < v.Length; i++)
            {
                v[i] = _data[i] - b._data[i];
            }

            return new ComplexVector(v);
        }

        /// <summary>
        /// Subtract a complex scalar from all elements of this vector.
        /// </summary>
        /// <param name="b">The complex scalar.</param>
        /// <returns>
        /// Vector ret[i] = this[i] - b
        /// </returns>
        /// <remarks>
        /// This method has the same effect as the overloaded - operator.
        /// </remarks>
        /// <seealso cref="SubtractInplace(Complex)"/>
        /// <seealso cref="operator - (ComplexVector, Complex)"/>
        public
        ComplexVector
        Subtract(Complex b)
        {
            Complex[] v = new Complex[_length];
            for(int i = 0; i < v.Length; i++)
            {
                v[i] = _data[i] - b;
            }

            return new ComplexVector(v);
        }

        /// <summary>
        /// Inplace subtraction of a complex vector from this vector.
        /// </summary>
        /// <remarks>
        /// This method changes this vector.
        /// </remarks>
        /// <seealso cref="Subtract(IVector{Complex})"/>
        /// <seealso cref="operator - (ComplexVector, ComplexVector)"/>
        public
        void
        SubtractInplace(ComplexVector b)
        {
            CheckMatchingVectorDimensions(this, b);

            for(int i = 0; i < _length; i++)
            {
                _data[i] -= b[i];
            }
        }

        /// <summary>
        /// Inplace subtraction of a real vector from this vector.
        /// </summary>
        /// <remarks>
        /// This method changes this vector.
        /// </remarks>
        /// <seealso cref="Subtract(IVector{double})"/>
        /// <seealso cref="operator - (ComplexVector, Vector)"/>
        public
        void
        SubtractInplace(Vector b)
        {
            CheckMatchingVectorDimensions(this, b);

            for(int i = 0; i < _length; i++)
            {
                _data[i] -= b._data[i];
            }
        }

        /// <summary>
        /// Inplace subtraction of a complex scalar from all elements of this vector.
        /// </summary>
        /// <remarks>
        /// This method changes this vector.
        /// </remarks>
        /// <seealso cref="Subtract(Complex)"/>
        /// <seealso cref="operator - (ComplexVector, Complex)"/>
        public
        void
        SubtractInplace(Complex b)
        {
            for(int i = 0; i < _length; i++)
            {
                _data[i] -= b;
            }
        }

        /// <summary>
        /// Negate this vector.
        /// </summary>
        /// <returns>
        /// Vector ret[i] = -this[i]
        /// </returns>
        /// <remarks>
        /// This method has the same effect as the overloaded - operator.
        /// </remarks>
        /// <seealso cref="NegateInplace"/>
        /// <seealso cref="operator - (ComplexVector)"/>
        public
        ComplexVector
        Negate()
        {
            Complex[] v = new Complex[_length];
            for(int i = 0; i < v.Length; i++)
            {
                v[i] = -_data[i];
            }

            return new ComplexVector(v);
        }

        /// <summary>
        /// Inplace unary minus of the <c>Vector</c>.
        /// </summary>
        /// <remarks>
        /// This method changes this vector.
        /// </remarks>
        /// <seealso cref="Negate"/>
        /// <seealso cref="operator - (ComplexVector)"/>
        public
        void
        NegateInplace()
        {
            for(int i = 0; i < _length; i++)
            {
                _data[i] = -_data[i];
            }
        }

        /// <summary>
        /// Conjugate this vector.
        /// </summary>
        /// <returns>
        /// Vector ret[i] = real(this[i]) - imag(this[i])
        /// </returns>
        /// <seealso cref="ConjugateInplace"/>
        public
        ComplexVector
        Conjugate()
        {
            Complex[] v = new Complex[_length];
            for(int i = 0; i < v.Length; i++)
            {
                v[i] = _data[i].Conjugate;
            }

            return new ComplexVector(v);
        }

        /// <summary>
        /// Inplace conjugation of this vector.
        /// </summary>
        /// <remarks>
        /// This method changes this vector.
        /// </remarks>
        /// <seealso cref="Conjugate"/>
        public
        void
        ConjugateInplace()
        {
            for(int i = 0; i < _length; i++)
            {
                _data[i] = _data[i].Conjugate;
            }
        }

        /// <summary>
        /// Scale this complex vector with a complex scalar.
        /// </summary>
        /// <param name="scalar">The scalar to scale with</param>
        /// <returns>
        /// Vector ret[i] = this[i] * scalar
        /// </returns>
        /// <remarks>
        /// This method has the same effect as the overloaded * operator.
        /// </remarks>
        /// <seealso cref="MultiplyInplace(Complex)"/>
        /// <seealso cref="operator * (ComplexVector, Complex)"/>
        public
        ComplexVector
        Multiply(Complex scalar)
        {
            Complex[] v = new Complex[_length];
            for(int i = 0; i < v.Length; i++)
            {
                v[i] = _data[i] * scalar;
            }

            return new ComplexVector(v);
        }

        /// <summary>
        /// Multiplies in place this <c>Vector</c> by a scalar.
        /// </summary>
        /// <remarks>
        /// This method changes this vector.
        /// </remarks>
        /// <seealso cref="Multiply(Complex)"/>
        /// <seealso cref="operator * (ComplexVector, Complex)"/>
        public
        void
        MultiplyInplace(Complex scalar)
        {
            for(int i = 0; i < _length; i++)
            {
                _data[i] *= scalar;
            }
        }

        #endregion

        #region Vector Products

        /// <summary>
        /// Scalar product of two vectors.
        /// </summary>
        /// <returns>
        /// Scalar ret = sum(u[i] * conj(v[i]))
        /// </returns>
        /// <seealso cref="ScalarMultiply(IVector{Complex})"/>
        /// <seealso cref="operator * (ComplexVector, ComplexVector)"/>
        public static
        Complex
        ScalarProduct(
            ComplexVector u,
            ComplexVector v)
        {
            CheckMatchingVectorDimensions(u, v);

            Complex sum = 0;
            for(int i = 0; i < u.Length; i++)
            {
                sum += u[i] * v[i].Conjugate;
            }

            return sum;
        }

        /// <summary>
        /// Scalar product of two vectors.
        /// </summary>
        /// <returns>
        /// Scalar ret = sum(u[i] * v[i])
        /// </returns>
        /// <seealso cref="ScalarMultiply(IVector{double})"/>
        /// <seealso cref="operator * (ComplexVector, Vector)"/>
        public static
        Complex
        ScalarProduct(
            ComplexVector u,
            Vector v)
        {
            CheckMatchingVectorDimensions(u, v);

            Complex sum = 0;
            for(int i = 0; i < u.Length; i++)
            {
                sum += u[i] * v._data[i];
            }

            return sum;
        }

        /// <summary>
        /// Scalar product of this vector with another complex vector.
        /// </summary>
        /// <param name="b">The other complex vector.</param>
        /// <returns>
        /// Scalar ret = sum(this[i] * conj(b[i]))
        /// </returns>
        /// <remarks>
        /// This method has the same effect as the overloaded * operator.
        /// </remarks>
        /// <seealso cref="ScalarProduct(IVector{Complex},IVector{Complex})"/>
        /// <seealso cref="operator * (ComplexVector, ComplexVector)"/>
        public
        Complex
        ScalarMultiply(ComplexVector b)
        {
            return ScalarProduct(this, b);
        }

        /// <summary>
        /// Scalar product of this vector with another real vector.
        /// </summary>
        /// <param name="b">The other real vector.</param>
        /// <returns>
        /// Scalar ret = sum(this[i] * b[i])
        /// </returns>
        /// <remarks>
        /// This method has the same effect as the overloaded * operator.
        /// </remarks>
        /// <seealso cref="ScalarProduct(IVector{Complex},IVector{double})"/>
        /// <seealso cref="operator * (ComplexVector, Vector)"/>
        public
        Complex
        ScalarMultiply(Vector b)
        {
            return ScalarProduct(this, b);
        }

        /// <summary>
        /// Dyadic Product of two vectors.
        /// </summary>
        /// <returns>
        /// ComplexMatrix M[i,j] = u[i] * conj(v[j]).
        /// </returns>
        /// <seealso cref="TensorMultiply"/>
        public static
        ComplexMatrix
        DyadicProduct(
            ComplexVector u,
            ComplexVector v)
        {
            Complex[][] m = ComplexMatrix.CreateMatrixData(u.Length, v.Length);
            for(int i = 0; i < u.Length; i++)
            {
                for(int j = 0; j < v.Length; j++)
                {
                    m[i][j] = u[i] * v[j].Conjugate;
                }
            }

            return new ComplexMatrix(m);
        }

        /// <summary>
        /// Tensor Product (Dyadic) of this and another vector.
        /// </summary>
        /// <param name="b">The vector to operate on.</param>
        /// <returns>
        /// ComplexMatrix M[i,j] = this[i] * conj(v[j]).
        /// </returns>
        /// <seealso cref="DyadicProduct"/>
        public
        ComplexMatrix
        TensorMultiply(ComplexVector b)
        {
            return DyadicProduct(this, b);
        }

        /// <summary>
        /// Cross product of two 3-dimensional vectors.
        /// </summary>
        /// <returns>
        /// Vector ret = (u[2]v[3] - u[3]v[2], u[3]v[1] - u[1]v[3], u[1]v[2] - u[2]v[1]).
        /// </returns>
        /// <seealso cref="CrossMultiply"/>
        public static
        ComplexVector
        CrossProduct(
            ComplexVector u,
            ComplexVector v)
        {
            CheckMatchingVectorDimensions(u, v);
            if(3 != u.Length)
            {
                throw new ArgumentOutOfRangeException("u", Properties.LocalStrings.ArgumentVectorThreeDimensional);
            }

            ComplexVector product = new ComplexVector(new Complex[] {
                (u[1] * v[2]) - (u[2] * v[1]),
                (u[2] * v[0]) - (u[0] * v[2]),
                (u[0] * v[1]) - (u[1] * v[0])
                });

            return product;
        }

        /// <summary>
        /// Cross product of this vector with another vector.
        /// </summary>
        /// <param name="b">The other vector.</param>
        /// <returns>
        /// Vector ret = (this[2]b[3] - this[3]b[2], this[3]b[1] - this[1]b[3], this[1]b[2] - this[2]b[1]).
        /// </returns>
        /// <seealso cref="CrossProduct"/>
        public
        ComplexVector
        CrossMultiply(ComplexVector b)
        {
            return CrossProduct(this, b);
        }

        /// <summary>
        /// Array (element-by-element) product of two vectors.
        /// </summary>
        /// <returns>
        /// Vector ret[i] = u[i] * v[i]
        /// </returns>
        /// <seealso cref="ArrayMultiply(IVector{Complex})"/>
        /// <seealso cref="ArrayMultiplyInplace(IVector{Complex})"/>
        public static
        ComplexVector
        ArrayProduct(
            ComplexVector a,
            ComplexVector b)
        {
            CheckMatchingVectorDimensions(a, b);

            Complex[] v = new Complex[a.Length];
            for(int i = 0; i < v.Length; i++)
            {
                v[i] = a[i] * b[i];
            }

            return new ComplexVector(v);
        }

        /// <summary>
        /// Array (element-by-element) product of two vectors.
        /// </summary>
        /// <returns>
        /// Vector ret[i] = u[i] * v[i]
        /// </returns>
        /// <seealso cref="ArrayMultiply(IVector{double})"/>
        /// <seealso cref="ArrayMultiplyInplace(IVector{double})"/>
        public static
        ComplexVector
        ArrayProduct(
            ComplexVector a,
            Vector b)
        {
            CheckMatchingVectorDimensions(a, b);

            Complex[] v = new Complex[a.Length];
            for(int i = 0; i < v.Length; i++)
            {
                v[i] = a[i] * b._data[i];
            }

            return new ComplexVector(v);
        }

        /// <summary>
        /// Array (element-by-element) product of this vector and another vector.
        /// </summary>
        /// <returns>
        /// Vector ret[i] = this[i] * b[i]
        /// </returns>
        /// <seealso cref="ArrayProduct(IVector{Complex},IVector{Complex})"/>
        /// <seealso cref="ArrayMultiplyInplace(IVector{Complex})"/>
        public
        ComplexVector
        ArrayMultiply(ComplexVector b)
        {
            return ArrayProduct(this, b);
        }

        /// <summary>
        /// Array (element-by-element) product of this vector and another vector.
        /// </summary>
        /// <returns>
        /// Vector ret[i] = this[i] * b[i]
        /// </returns>
        /// <seealso cref="ArrayProduct(IVector{Complex},IVector{double})"/>
        /// <seealso cref="ArrayMultiplyInplace(IVector{double})"/>
        public
        ComplexVector
        ArrayMultiply(Vector b)
        {
            return ArrayProduct(this, b);
        }

        /// <summary>
        /// Multiply in place (element-by-element) another vector to this vector.
        /// </summary>
        /// <remarks>
        /// This method changes this vector.
        /// </remarks>
        /// <seealso cref="ArrayProduct(IVector{Complex},IVector{Complex})"/>
        /// <seealso cref="ArrayMultiply(IVector{Complex})"/>
        public
        void
        ArrayMultiplyInplace(ComplexVector b)
        {
            CheckMatchingVectorDimensions(this, b);

            for(int i = 0; i < _data.Length; i++)
            {
                _data[i] *= b[i];
            }
        }

        /// <summary>
        /// Multiply in place (element-by-element) another vector to this vector.
        /// </summary>
        /// <remarks>
        /// This method changes this vector.
        /// </remarks>
        /// <seealso cref="ArrayProduct(IVector{Complex},IVector{double})"/>
        /// <seealso cref="ArrayMultiply(IVector{double})"/>
        public
        void
        ArrayMultiplyInplace(Vector b)
        {
            CheckMatchingVectorDimensions(this, b);

            for(int i = 0; i < _data.Length; i++)
            {
                _data[i] *= b._data[i];
            }
        }

        /// <summary>
        /// Array (element-by-element) raise to power.
        /// </summary>
        /// <returns>
        /// Vector ret[i] = this[i] ^ exponent
        /// </returns>
        /// <seealso cref="ArrayPowerInplace(Complex)"/>
        public
        ComplexVector
        ArrayPower(Complex exponent)
        {
            Complex[] newData = new Complex[_length];
            for(int i = 0; i < newData.Length; i++)
            {
                newData[i] = _data[i].Power(exponent);
            }

            return new ComplexVector(newData);
        }

        /// <summary>
        /// In place array (element-by-element) raise to power.
        /// </summary>
        /// <remarks>
        /// This method changes this vector.
        /// </remarks>
        /// <seealso cref="ArrayPower(Complex)"/>
        public
        void
        ArrayPowerInplace(Complex exponent)
        {
            for(int i = 0; i < _data.Length; i++)
            {
                _data[i] = _data[i].Power(exponent);
            }
        }

        /// <summary>
        /// Array (element-by-element) quotient of two vectors.
        /// </summary>
        /// <returns>
        /// Vector ret[i] = u[i] / v[i]
        /// </returns>
        /// <seealso cref="ArrayDivide(IVector{Complex})"/>
        /// <seealso cref="ArrayDivideInplace(IVector{Complex})"/>
        public static
        ComplexVector
        ArrayQuotient(
            ComplexVector a,
            ComplexVector b)
        {
            CheckMatchingVectorDimensions(a, b);

            Complex[] v = new Complex[a.Length];
            for(int i = 0; i < v.Length; i++)
            {
                v[i] = a[i] / b[i];
            }

            return new ComplexVector(v);
        }

        /// <summary>
        /// Array (element-by-element) quotient of two vectors.
        /// </summary>
        /// <returns>
        /// Vector ret[i] = u[i] / v[i]
        /// </returns>
        /// <seealso cref="ArrayDivide(IVector{double})"/>
        /// <seealso cref="ArrayDivideInplace(IVector{double})"/>
        public static
        ComplexVector
        ArrayQuotient(
            ComplexVector a,
            Vector b)
        {
            CheckMatchingVectorDimensions(a, b);

            Complex[] v = new Complex[a.Length];
            for(int i = 0; i < v.Length; i++)
            {
                v[i] = a[i] / b._data[i];
            }

            return new ComplexVector(v);
        }

        /// <summary>
        /// Array (element-by-element) quotient of this vector and another complex vector.
        /// </summary>
        /// <returns>
        /// Vector ret[i] = this[i] / b[i]
        /// </returns>
        /// <seealso cref="ArrayQuotient(IVector{Complex},IVector{Complex})"/>
        /// <seealso cref="ArrayDivideInplace(IVector{Complex})"/>
        public
        ComplexVector
        ArrayDivide(ComplexVector b)
        {
            return ArrayQuotient(this, b);
        }

        /// <summary>
        /// Array (element-by-element) quotient of this vector and another real vector.
        /// </summary>
        /// <returns>
        /// Vector ret[i] = this[i] / b[i]
        /// </returns>
        /// <seealso cref="ArrayQuotient(IVector{Complex},IVector{double})"/>
        /// <seealso cref="ArrayDivideInplace(IVector{double})"/>
        public
        ComplexVector
        ArrayDivide(Vector b)
        {
            return ArrayQuotient(this, b);
        }

        /// <summary>
        /// Divide in place (element-by-element) this vector by another complex vector.
        /// </summary>
        /// <remarks>
        /// This method changes this vector.
        /// </remarks>
        /// <seealso cref="ArrayQuotient(IVector{Complex},IVector{Complex})"/>
        /// <seealso cref="ArrayDivide(IVector{Complex})"/>
        public
        void
        ArrayDivideInplace(ComplexVector b)
        {
            CheckMatchingVectorDimensions(this, b);

            for(int i = 0; i < _data.Length; i++)
            {
                _data[i] /= b[i];
            }
        }

        /// <summary>
        /// Divide in place (element-by-element) this vector by another real vector.
        /// </summary>
        /// <remarks>
        /// This method changes this vector.
        /// </remarks>
        /// <seealso cref="ArrayQuotient(IVector{Complex},IVector{double})"/>
        /// <seealso cref="ArrayDivide(IVector{double})"/>
        public
        void
        ArrayDivideInplace(Vector b)
        {
            CheckMatchingVectorDimensions(this, b);

            for(int i = 0; i < _data.Length; i++)
            {
                _data[i] /= b._data[i];
            }
        }

        /// <summary>
        /// Map an arbitrary function to all elements of this vector.
        /// </summary>
        /// <returns>
        /// Vector ret[i] = mapping(this[i])
        /// </returns>
        /// <seealso cref="ArrayMapInplace(Converter{Complex,Complex})"/>
        public
        ComplexVector
        ArrayMap(Converter<Complex, Complex> mapping)
        {
            Complex[] newData = new Complex[_length];
            for(int i = 0; i < newData.Length; i++)
            {
                newData[i] = mapping(_data[i]);
            }

            return new ComplexVector(newData);
        }

        /// <summary>
        /// In place map an arbitrary function to all elements of this vector.
        /// </summary>
        /// <remarks>
        /// This method changes this vector.
        /// </remarks>
        /// <seealso cref="ArrayMap(Converter{Complex,Complex})"/>
        public
        void
        ArrayMapInplace(Converter<Complex, Complex> mapping)
        {
            for(int i = 0; i < _data.Length; i++)
            {
                _data[i] = mapping(_data[i]);
            }
        }

        #endregion

        #region Vector Norms

        /// <summary>
        /// L-2 Norm also known as Euclidean Norm.
        /// </summary>
        /// <returns>
        /// Scalar ret = sqrt(sum(this[i]*conjugate(this[i]))
        /// </returns>
        public
        double
        Norm()
        {
            double sum = 0;
            for(int i = 0; i < _data.Length; i++)
            {
                sum += _data[i].ModulusSquared;
            }

            return Math.Sqrt(sum);
        }

        /// <summary>
        /// L-1 Norm also known as Manhattan Norm or Taxicab Norm.
        /// </summary>
        /// <returns>
        /// Scalar ret = sum(abs(this[i]))
        /// </returns>
        public
        double
        Norm1()
        {
            double sum = 0;
            for(int i = 0; i < _data.Length; i++)
            {
                sum += _data[i].Modulus;
            }

            return sum;
        }

        #endregion

        #region Arithmetic Operator Overloading

        /// <summary>
        /// Addition Operator.
        /// </summary>
        public static
        ComplexVector
        operator +(
            ComplexVector u,
            ComplexVector v)
        {
            CheckMatchingVectorDimensions(u, v);
            return u.Add(v);
        }

        /// <summary>
        /// Addition Operator.
        /// </summary>
        public static
        ComplexVector
        operator +(
            ComplexVector complexVector,
            Vector realVector)
        {
            CheckMatchingVectorDimensions(complexVector, realVector);
            return complexVector.Add(realVector);
        }

        /// <summary>
        /// Addition Operator.
        /// </summary>
        public static
        ComplexVector
        operator +(
            ComplexVector complexVector,
            Complex scalar)
        {
            return complexVector.Add(scalar);
        }

        /// <summary>
        /// Subtraction Operator.
        /// </summary>
        public static
        ComplexVector
        operator -(
            ComplexVector u,
            ComplexVector v)
        {
            CheckMatchingVectorDimensions(u, v);
            return u.Subtract(v);
        }

        /// <summary>
        /// Subtraction Operator.
        /// </summary>
        public static
        ComplexVector
        operator -(
            ComplexVector complexVector,
            Vector realVector)
        {
            CheckMatchingVectorDimensions(complexVector, realVector);
            return complexVector.Subtract(realVector);
        }

        /// <summary>
        /// Subtraction Operator.
        /// </summary>
        public static
        ComplexVector
        operator -(
            ComplexVector complexVector,
            Complex scalar)
        {
            return complexVector.Subtract(scalar);
        }

        /// <summary>
        /// Negate a vectors
        /// </summary>
        public static
        ComplexVector
        operator -(ComplexVector v)
        {
            return v.Negate();
        }

        /// <summary>
        /// Scaling a vector by a scalar.
        /// </summary>
        public static
        ComplexVector
        operator *(
            Complex scalar,
            ComplexVector vector)
        {
            return vector.Multiply(scalar);
        }

        /// <summary>
        /// Scaling a vector by a scalar.
        /// </summary>
        public static
        ComplexVector
        operator *(
            ComplexVector vector,
            Complex scalar)
        {
            return vector.Multiply(scalar);
        }

        /// <summary>
        /// Scaling a vector by the inverse of a scalar.
        /// </summary>
        public static
        ComplexVector
        operator /(
            ComplexVector vector,
            Complex scalar)
        {
            return vector.Multiply(1 / scalar);
        }

        /// <summary>
        /// Scalar/dot product of two vectors.
        /// </summary>
        public static
        Complex
        operator *(
            ComplexVector u,
            ComplexVector v)
        {
            return ScalarProduct(u, v);
        }

        /// <summary>
        /// Scalar/dot product of two vectors.
        /// </summary>
        public static
        Complex
        operator *(
            ComplexVector u,
            Vector v)
        {
            return ScalarProduct(u, v);
        }

        #endregion

        #region Various Helpers & Infrastructure

        /// <summary>Check if size(u) == size(v) *</summary>
        private static
        void
        CheckMatchingVectorDimensions(
            ComplexVector u,
            ComplexVector v)
        {
            if(null == u)
            {
                throw new ArgumentNullException("u");
            }

            if(null == v)
            {
                throw new ArgumentNullException("v");
            }

            if(u.Length != v.Length)
            {
                throw new ArgumentException(Properties.LocalStrings.ArgumentVectorsSameLengths);
            }
        }

        /// <summary>Check if size(u) == size(v) *</summary>
        private static
        void
        CheckMatchingVectorDimensions(
            ComplexVector u,
            Vector v)
        {
            if(null == u)
            {
                throw new ArgumentNullException("u");
            }

            if(null == v)
            {
                throw new ArgumentNullException("v");
            }

            if(u.Length != v.Length)
            {
                throw new ArgumentException(Properties.LocalStrings.ArgumentVectorsSameLengths);
            }
        }

        /// <summary>Returns a deep copy of this instance.</summary>
        public
        ComplexVector
        Clone()
        {
            return Create(_data);
        }

        /// <summary>
        /// Creates an exact copy of this matrix.
        /// </summary>
        object
        ICloneable.Clone()
        {
            return Create(_data);
        }

        /// <summary>
        /// Indicates whether <c>obj</c> is equal to this instance.
        /// </summary>
        public override
        bool
        Equals(object obj)
        {
            ComplexVector v = obj as ComplexVector;
            return ReferenceEquals(v, null) ? false : Equals(v);
        }

        /// <summary>
        /// Indicates whether <c>other</c> is equal to this matrix.
        /// </summary>
        public
        bool
        Equals(ComplexVector other)
        {
            if(ReferenceEquals(other, null)
                || _length != other.Length)
            {
                return false;
            }

            // compare all values
            Complex[] otherData = other._data;
            for(int i = 0; i < _data.Length; i++)
            {
                if(!_data[i].Equals(otherData[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Serves as a hash function for this type
        /// </summary>
        public override
        int
        GetHashCode()
        {
            unchecked
            {
                return (_data.GetHashCode() * 397) ^ _length;
            }
        }

        /// <summary>
        /// Returns true if two vectors are almost equal, up to the default maximum relative error.
        /// </summary>
        public
        bool
        AlmostEquals(ComplexVector other)
        {
            if(ReferenceEquals(other, null)
                || _length != other.Length)
            {
                return false;
            }

            return Number.AlmostEqualNorm(Norm1(), other.Norm1(), (this - other).Norm1());
        }

        /// <summary>
        /// Returns true if two vectors are almost equal, up to the provided maximum relative error.
        /// </summary>
        public
        bool
        AlmostEquals(
            ComplexVector other,
            double maximumRelativeError)
        {
            if(ReferenceEquals(other, null)
                || _length != other.Length)
            {
                return false;
            }

            return Number.AlmostEqualNorm(Norm1(), other.Norm1(), (this - other).Norm1(), maximumRelativeError);
        }

        /// <summary>
        /// Returns true if two vectors are almost equal, up to the provided maximum relative error.
        /// </summary>
        public static
        bool
        AlmostEqual(
            ComplexVector u,
            ComplexVector v,
            double maximumRelativeError)
        {
            return Number.AlmostEqual(u, v, maximumRelativeError);
        }

        /// <summary>
        /// Returns true if two vectors are almost equal, up to the default maximum relative error.
        /// </summary>
        public static
        bool
        AlmostEqual(
            ComplexVector u,
            ComplexVector v)
        {
            return Number.AlmostEqual(u, v);
        }

        /// <summary>
        /// Formats this vector to a human-readable string
        /// </summary>
        public override
        string
        ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");

            for(int i = 0; i < _data.Length; i++)
            {
                if(i != 0)
                {
                    sb.Append(',');
                }

                sb.Append(_data[i].ToString());
            }

            sb.Append("]");
            return sb.ToString();
        }

        #endregion

        #region IList<Complex> Interface Implementation

        /// <summary>
        /// Index of an element.
        /// </summary>
        int
        IList<Complex>.IndexOf(Complex item)
        {
            return Array.IndexOf(_data, item);
        }

        /// <summary>
        /// True if the vector contains some element.
        /// </summary>
        bool
        ICollection<Complex>.Contains(Complex item)
        {
            return Array.IndexOf(_data, item) >= 0;
        }

        /// <summary>
        /// Copy all elements to some array.
        /// </summary>
        void
        ICollection<Complex>.CopyTo(
            Complex[] array,
            int arrayIndex)
        {
            _data.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Length.
        /// </summary>
        int ICollection<Complex>.Count
        {
            get { return Length; }
        }

        /// <summary>
        /// Get a typed enumerator over all elements.
        /// </summary>
        IEnumerator<Complex>
        IEnumerable<Complex>.GetEnumerator()
        {
            return ((IEnumerable<Complex>)_data).GetEnumerator();
        }

        /// <summary>
        /// Get a non-typed enumerator over all elements.
        /// </summary>
        System.Collections.IEnumerator
        System.Collections.IEnumerable.GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        /// <summary>
        /// False.
        /// </summary>
        bool ICollection<Complex>.IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Not Supported.
        /// </summary>
        void
        ICollection<Complex>.Add(Complex item)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Not Supported.
        /// </summary>
        void
        IList<Complex>.Insert(
            int index,
            Complex item)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Not Supported.
        /// </summary>
        bool
        ICollection<Complex>.Remove(Complex item)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Not Supported.
        /// </summary>
        void
        IList<Complex>.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Not Supported.
        /// </summary>
        void
        ICollection<Complex>.Clear()
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
