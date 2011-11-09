//-----------------------------------------------------------------------
// <copyright file="ComplexMatrix.cs" company="Math.NET Project">
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
    /// Complex Matrix.
    /// </summary>
    [Serializable]
    public class ComplexMatrix :
        IEquatable<ComplexMatrix>,
        IAlmostEquatable<ComplexMatrix>,
        ICloneable
    {
        int _rowCount;
        int _columnCount;

        /// <summary>
        /// Array for internal storage of elements.
        /// </summary>
        Complex[][] _data;

        /// <summary>
        /// Gets the number of rows.
        /// </summary>
        public int RowCount
        {
            get { return _rowCount; }
        }

        /// <summary>
        /// Gets the number of columns.
        /// </summary>
        public int ColumnCount
        {
            get { return _columnCount; }
        }

        /// <summary>
        /// Gets or set the element indexed by <c>(i, j)</c>
        /// in the <c>Matrix</c>.
        /// </summary>
        /// <param name="i">Row index.</param>
        /// <param name="j">Column index.</param>
        public Complex this[int i, int j]
        {
            get
            {
                return _data[i][j];
            }

            set
            {
                _data[i][j] = value;

                // NOTE (ruegg, 2008-03-11): The folloing line is cheap,
                // but still expensive if this setter is called
                // a lot of times.
                // - We should recommend out users to build the internal
                //   jagged array first and then create the matrix for it;
                //   or to get the internal double[][] handle.
                // - Consider to omit it here and make the users call
                //   ResetComputations() after they finished chaniging the matrix.
                ResetOnDemandComputations();
            }
        }

        #region Data -> Matrix: Constructors and static constructive methods

        /// <summary>
        /// Initializes a new instance of the ComplexMatrix class,
        /// for an m-by-n matrix of zeros.
        /// </summary>
        /// <param name="m">Number of rows.</param>
        /// <param name="n">Number of columns.</param>
        public
        ComplexMatrix(int m, int n)
        {
            _data = CreateMatrixData(m, n);
            _rowCount = m;
            _columnCount = n;

            InitOnDemandComputations();
        }

        /// <summary>
        /// Initializes a new instance of the ComplexMatrix class,
        /// for an m-by-m square matrix.
        /// </summary>
        /// <param name="m">Size of the square matrix.</param>
        /// <param name="s">Diagonal value.</param>
        public
        ComplexMatrix(
            int m,
            Complex s)
        {
            _data = new Complex[m][];
            _rowCount = m;
            _columnCount = m;

            for(int i = 0; i < m; i++)
            {
                Complex[] col = new Complex[m];
                col[i] = s;
                _data[i] = col;
            }

            InitOnDemandComputations();
        }

        /// <summary>
        /// Initializes a new instance of the ComplexMatrix class,
        /// for an m-by-n constant matrix.
        /// </summary>
        /// <param name="m">Number of rows.</param>
        /// <param name="n">Number of columns.</param>
        /// <param name="s">Fill the matrix with this scalar value.</param>
        public
        ComplexMatrix(
            int m,
            int n,
            Complex s)
        {
            _data = new Complex[m][];
            _rowCount = m;
            _columnCount = n;

            for(int i = 0; i < m; i++)
            {
                Complex[] col = new Complex[n];
                for(int j = 0; j < n; j++)
                {
                    col[j] = s;
                }

                _data[i] = col;
            }

            InitOnDemandComputations();
        }

        /// <summary>
        /// Initializes a new instance of the ComplexMatrix class
        /// from a jagged 2-D array, directly using the provided array as internal data structure.
        /// </summary>
        /// <param name="a">Two-dimensional jagged array of complex numbers.</param>
        /// <exception cref="System.ArgumentException">All rows must have the same length.</exception>
        /// <seealso cref="ComplexMatrix.Create(Complex[][])"/>
        /// <seealso cref="ComplexMatrix.Create(Complex[,])"/>
        public
        ComplexMatrix(Complex[][] a)
        {
            _data = a;
            GetRowColumnCount(_data, out _rowCount, out _columnCount);

            InitOnDemandComputations();
        }

        /// <summary>
        /// Initializes a new instance of the ComplexMatrix class
        /// from a one-dimensional packed array.
        /// </summary>
        /// <param name="values">One-dimensional array of complex numbers, packed by columns (ala Fortran).</param>
        /// <param name="m">Number of rows.</param>
        /// <exception cref="System.ArgumentException">Array length must be a multiple of m.</exception>
        public
        ComplexMatrix(
            Complex[] values,
            int m)
        {
            _rowCount = m;
            if(m == 0)
            {
                _columnCount = 0;
                if(values.Length != 0)
                {
                    throw new ArgumentException(Properties.LocalStrings.ArgumentVectorLengthsMultipleOf("m"));
                }
            }
            else
            {
                int rem;
                _columnCount = Math.DivRem(values.Length, m, out rem);
                if(rem != 0)
                {
                    throw new ArgumentException(Properties.LocalStrings.ArgumentVectorLengthsMultipleOf("m"));
                }
            }

            _data = new Complex[_rowCount][];
            for(int i = 0; i < _rowCount; i++)
            {
                Complex[] col = new Complex[_columnCount];
                for(int j = 0; j < _columnCount; j++)
                {
                    col[j] = values[i + (j * _rowCount)];
                }

                _data[i] = col;
            }

            InitOnDemandComputations();
        }

        /// <summary>
        /// Constructs a matrix from a copy of a 2-D array by deep-copy.
        /// </summary>
        /// <param name="a">Two-dimensional array of complex numbers.</param>
        public static
        ComplexMatrix
        Create(Complex[][] a)
        {
            return new ComplexMatrix(CloneMatrixData(a));
        }

        /// <summary>
        /// Constructs a matrix from a copy of a 2-D array by deep-copy.
        /// </summary>
        /// <param name="a">Two-dimensional array of complex numbers.</param>
        [CLSCompliant(false)]
        public static
        ComplexMatrix
        Create(Complex[,] a)
        {
            int rows = a.GetLength(0);
            int columns = a.GetLength(1);
            Complex[][] newData = new Complex[rows][];

            for(int i = 0; i < rows; i++)
            {
                Complex[] col = new Complex[columns];
                for(int j = 0; j < columns; j++)
                {
                    col[j] = a[i, j];
                }

                newData[i] = col;
            }

            return new ComplexMatrix(newData);
        }

        /// <summary>
        /// Construct a matrix from a real matrix by deep-copy.
        /// </summary>
        /// <param name="realMatrix">The real matrix to copy from.</param>
        public static
        ComplexMatrix
        Create(Matrix realMatrix)
        {
            int rows = realMatrix.RowCount;
            int columns = realMatrix.ColumnCount;
            Complex[][] newData = new Complex[rows][];

            for(int i = 0; i < rows; i++)
            {
                Complex[] col = new Complex[columns];
                for(int j = 0; j < columns; j++)
                {
                    col[j] = realMatrix._data[i][j];
                }

                newData[i] = col;
            }

            return new ComplexMatrix(newData);
        }

        /// <summary>
        /// Construct a complex matrix from a set of complex column vectors.
        /// </summary>
        public static
        ComplexMatrix
        CreateFromColumns(IList<ComplexVector> columnVectors)
        {
            if(null == columnVectors)
            {
                throw new ArgumentNullException("columnVectors");
            }

            if(0 == columnVectors.Count)
            {
                throw new ArgumentOutOfRangeException("columnVectors");
            }

            int rows = columnVectors[0].Length;
            int columns = columnVectors.Count;
            Complex[][] newData = new Complex[rows][];

            for(int i = 0; i < rows; i++)
            {
                Complex[] newRow = new Complex[columns];
                for(int j = 0; j < columns; j++)
                {
                    newRow[j] = columnVectors[j][i];
                }

                newData[i] = newRow;
            }

            return new ComplexMatrix(newData);
        }

        /// <summary>
        /// Construct a complex matrix from a set of complex row vectors.
        /// </summary>
        public static
        ComplexMatrix
        CreateFromRows(IList<ComplexVector> rowVectors)
        {
            if(null == rowVectors)
            {
                throw new ArgumentNullException("rowVectors");
            }

            if(0 == rowVectors.Count)
            {
                throw new ArgumentOutOfRangeException("rowVectors");
            }

            int rows = rowVectors.Count;
            Complex[][] newData = new Complex[rows][];

            for(int i = 0; i < rows; i++)
            {
                newData[i] = rowVectors[i].CopyToArray();
            }

            return new ComplexMatrix(newData);
        }

        /// <summary>
        /// Generates the identity matrix.
        /// </summary>
        /// <param name="m">Number of rows.</param>
        /// <param name="n">Number of columns.</param>
        /// <returns>An m-by-n matrix with ones on the diagonal and zeros elsewhere.</returns>
        public static
        ComplexMatrix
        Identity(int m, int n)
        {
            Complex[][] data = new Complex[m][];
            for(int i = 0; i < m; i++)
            {
                Complex[] col = new Complex[n];
                if(i < n)
                {
                    col[i] = Complex.One;
                }

                data[i] = col;
            }

            return new ComplexMatrix(data);
        }

        /// <summary>
        /// Creates a new diagonal m-by-n matrix based on the diagonal vector.
        /// </summary>
        /// <param name="diagonalVector">The values of the matrix diagonal.</param>
        /// <param name="m">Number of rows.</param>
        /// <param name="n">Number of columns.</param>
        /// <returns>
        /// An m-by-n matrix with the values from the diagonal vector on the diagonal and zeros elsewhere.
        /// </returns>
        public static
        ComplexMatrix
        Diagonal(
            ComplexVector diagonalVector,
            int m,
            int n)
        {
            Complex[][] data = new Complex[m][];
            for(int i = 0; i < m; i++)
            {
                Complex[] col = new Complex[n];
                if((i < n) && (i < diagonalVector.Length))
                {
                    col[i] = diagonalVector[i];
                }

                data[i] = col;
            }

            return new ComplexMatrix(data);
        }

        /// <summary>
        /// Creates a new square diagonal matrix based on the diagonal vector.
        /// </summary>
        /// <param name="diagonalVector">The values of the matrix diagonal.</param>
        /// <returns>
        /// An m-by-n matrix with the values from the diagonal vector on the diagonal and zeros elsewhere.
        /// </returns>
        public static
        ComplexMatrix
        Diagonal(ComplexVector diagonalVector)
        {
            return Diagonal(diagonalVector, diagonalVector.Length, diagonalVector.Length);
        }

        /// <summary>
        /// Generates an m-by-m matrix filled with 1.
        /// </summary>
        /// <param name="m">Number of rows = Number of columns</param>
        public static
        ComplexMatrix
        Ones(int m)
        {
            return new ComplexMatrix(m, m, Complex.One);
        }

        /// <summary>
        /// Generates an m-by-m matrix filled with 0.
        /// </summary>
        /// <param name="m">Number of rows = Number of columns</param>
        public static
        ComplexMatrix
        Zeros(int m)
        {
            return new ComplexMatrix(m, m);
        }

        /// <summary>
        /// Generates matrix with random real and imaginary elements.
        /// </summary>
        /// <param name="m">Number of rows.</param>
        /// <param name="n">Number of columns.</param>
        /// <param name="randomDistribution">Continuous Random Distribution or Source</param>
        /// <returns>
        /// An m-by-n matrix with real and imaginary elements distributed according to the provided distribution.
        /// </returns>
        public static
        ComplexMatrix
        Random(
            int m,
            int n,
            IContinuousGenerator randomDistribution)
        {
            Complex[][] data = new Complex[m][];
            for(int i = 0; i < m; i++)
            {
                Complex[] col = new Complex[n];
                for(int j = 0; j < n; j++)
                {
                    col[j] = Complex.Random(
                        randomDistribution,
                        randomDistribution);
                }

                data[i] = col;
            }

            return new ComplexMatrix(data);
        }

        /// <summary>
        /// Generates matrix with random real and zero imaginary elements.
        /// </summary>
        /// <param name="m">Number of rows.</param>
        /// <param name="n">Number of columns.</param>
        /// <param name="realRandomDistribution">Continuous Random Distribution or Source for the real part.</param>
        /// <returns>
        /// An m-by-n matrix with real parts distributed according to the provided distribution.
        /// </returns>
        public static
        ComplexMatrix
        RandomReal(
            int m,
            int n,
            IContinuousGenerator realRandomDistribution)
        {
            Complex[][] data = new Complex[m][];
            for(int i = 0; i < m; i++)
            {
                Complex[] col = new Complex[n];
                for(int j = 0; j < n; j++)
                {
                    col[j] = new Complex(
                        realRandomDistribution.NextDouble(),
                        0d);
                }

                data[i] = col;
            }

            return new ComplexMatrix(data);
        }

        /// <summary>
        /// Generates matrix with random modulus and argument elements.
        /// </summary>
        /// <param name="m">Number of rows.</param>
        /// <param name="n">Number of columns.</param>
        /// <param name="modulusRandomDistribution">Continuous Random Distribution or Source for the modulus part (must be non-negative!).</param>
        /// <param name="argumentRandomDistribution">Continuous Random Distribution or Source for the argument part.</param>
        /// <returns>
        /// An m-by-n matrix with imaginary parts distributed according to the provided distribution.
        /// </returns>
        public static
        ComplexMatrix
        RandomPolar(
            int m,
            int n,
            IContinuousGenerator modulusRandomDistribution,
            IContinuousGenerator argumentRandomDistribution)
        {
            Complex[][] data = new Complex[m][];
            for(int i = 0; i < m; i++)
            {
                Complex[] col = new Complex[n];
                for(int j = 0; j < n; j++)
                {
                    col[j] = Complex.RandomPolar(
                        modulusRandomDistribution,
                        argumentRandomDistribution);
                }

                data[i] = col;
            }

            return new ComplexMatrix(data);
        }

        /// <summary>
        /// Generates a matrix of complex numbers on the unit circle with random argument.
        /// </summary>
        /// <param name="m">Number of rows.</param>
        /// <param name="n">Number of columns.</param>
        /// <param name="argumentRandomDistribution">Continuous random distribution or source for the complex number arguments.</param>
        /// <returns>
        /// An m-by-n matrix with complex arguments distributed according to the provided distribution.
        /// </returns>
        public static
        ComplexMatrix
        RandomUnitCircle(
            int m,
            int n,
            IContinuousGenerator argumentRandomDistribution)
        {
            Complex[][] data = new Complex[m][];
            for(int i = 0; i < m; i++)
            {
                Complex[] col = new Complex[n];
                for(int j = 0; j < n; j++)
                {
                    col[j] = Complex.RandomUnitCircle(
                        argumentRandomDistribution);
                }

                data[i] = col;
            }

            return new ComplexMatrix(data);
        }

        #endregion //  Constructors

        #region Matrix -> Data: Back Conversions

        /// <summary>
        /// Copies the internal data structure to a 2-dimensional array.
        /// </summary>
        public
        Complex[,]
        CopyToArray()
        {
            Complex[,] newData = new Complex[_rowCount, _columnCount];
            for(int i = 0; i < _rowCount; i++)
            {
                for(int j = 0; j < _columnCount; j++)
                {
                    newData[i, j] = _data[i][j];
                }
            }

            return newData;
        }

        /// <summary>
        /// Copies the internal data structure to a jagged rectangular array.
        /// </summary>
        /// <returns></returns>
        public
        Complex[][]
        CopyToJaggedArray()
        {
            return CloneMatrixData(_data);
        }

        /// <summary>
        /// Returns the internal data structure array.
        /// </summary>
        public
        Complex[][]
        GetArray()
        {
            return _data;
        }

        /// <summary>Implicit conversion to a <c>Complex[][]</c> array.</summary>
        public static implicit
        operator Complex[][](ComplexMatrix m)
        {
            return m._data;
        }

        /// <summary>
        /// Explicit conversion to a <c>Complex[]</c> array of a single column matrix.
        /// </summary>
        /// <param name="m">Exactly one column expected.</param>
        public static explicit
        operator Complex[](ComplexMatrix m)
        {
            if(m.ColumnCount != 1)
            {
                throw new InvalidOperationException(Properties.LocalStrings.ArgumentMatrixSingleColumn);
            }

            Complex[] array = new Complex[m.RowCount];
            for(int i = 0; i < m.RowCount; i++)
            {
                array[i] = m[i, 0];
            }

            return array;
        }

        /// <summary>
        /// Explicit conversion to a <c>Complex</c> scalar of a single column and row (1-by-1) matrix.
        /// </summary>
        /// <param name="m">1-by-1 Matrix</param>
        public static explicit
        operator Complex(ComplexMatrix m)
        {
            if(m.ColumnCount != 1 || m.RowCount != 1)
            {
                throw new InvalidOperationException(Properties.LocalStrings.ArgumentMatrixSingleColumnRow);
            }

            return m[0, 0];
        }

        #endregion

        #region Internal Data Stucture

        /// <summary>
        /// Create the internal matrix data structure for a matrix of the given size.
        /// Initializing matrices directly on the internal structure may be faster
        /// than accessing the cells through the matrix class.
        /// </summary>
        /// <param name="m">Number of rows.</param>
        /// <param name="n">Number of columns.</param>
        public static
        Complex[][]
        CreateMatrixData(int m, int n)
        {
            Complex[][] data = new Complex[m][];
            for(int i = 0; i < m; i++)
            {
                data[i] = new Complex[n];
            }

            return data;
        }

        /// <summary>
        /// Creates a copy of a given internal matrix data structure.
        /// </summary>
        public static
        Complex[][]
        CloneMatrixData(Complex[][] data)
        {
            int rows, columns;
            GetRowColumnCount(data, out rows, out columns);
            Complex[][] newData = new Complex[rows][];
            for(int i = 0; i < rows; i++)
            {
                Complex[] col = new Complex[columns];
                for(int j = 0; j < columns; j++)
                {
                    col[j] = data[i][j];
                }

                newData[i] = col;
            }

            return newData;
        }

        /// <summary>
        /// Tries to find out the row column count of a given internal matrix data structure.
        /// </summary>
        public static
        void
        GetRowColumnCount(
            Complex[][] data,
            out int rows,
            out int columns)
        {
            rows = data.Length;
            columns = (rows == 0) ? 0 : data[0].Length;
        }

        #endregion

        #region Sub-matrices operation

        /// <summary>
        /// Copies a specified column of this matrix to a new vector.
        /// </summary>
        public
        ComplexVector
        GetColumnVector(int columnIndex)
        {
            if(columnIndex < 0 || columnIndex >= _columnCount)
            {
                throw new ArgumentOutOfRangeException("columnIndex");
            }

            Complex[] newData = new Complex[_rowCount];

            for(int i = 0; i < _rowCount; i++)
            {
                newData[i] = _data[i][columnIndex];
            }

            return new ComplexVector(newData);
        }

        /// <summary>
        /// Copies a specified row of this matrix to a new vector.
        /// </summary>
        public
        ComplexVector
        GetRowVector(int rowIndex)
        {
            if(rowIndex < 0 || rowIndex >= _rowCount)
            {
                throw new ArgumentOutOfRangeException("rowIndex");
            }

            Complex[] newData = new Complex[_columnCount];
            _data[rowIndex].CopyTo(newData, 0);

            return new ComplexVector(newData);
        }

        /// <summary>
        /// Copies a column vector to a specified column of this matrix.
        /// </summary>
        public
        void
        SetColumnVector(
            ComplexVector columnVector,
            int columnIndex)
        {
            if(null == columnVector)
            {
                throw new ArgumentNullException("columnVector");
            }

            if(columnIndex < 0 || columnIndex >= _columnCount)
            {
                throw new ArgumentOutOfRangeException("columnIndex");
            }

            if(columnVector.Length != _rowCount)
            {
                throw new ArgumentOutOfRangeException("columnVector");
            }

            for(int i = 0; i < _rowCount; i++)
            {
                _data[i][columnIndex] = columnVector[i];
            }

            ResetOnDemandComputations();
        }

        /// <summary>
        /// Copies a row vector to a specified row of this matrix.
        /// </summary>
        public
        void
        SetRowVector(
            ComplexVector rowVector,
            int rowIndex)
        {
            if(null == rowVector)
            {
                throw new ArgumentNullException("rowVector");
            }

            if(rowIndex < 0 || rowIndex >= _rowCount)
            {
                throw new ArgumentOutOfRangeException("rowIndex");
            }

            if(rowVector.Length != _columnCount)
            {
                throw new ArgumentOutOfRangeException("rowVector");
            }

            _data[rowIndex] = rowVector.CopyToArray();

            ResetOnDemandComputations();
        }

        /// <summary>
        /// Gets a sub matrix.
        /// </summary>
        /// <param name="i0">First row index.</param>
        /// <param name="i1">Last row index (inclusive).</param>
        /// <param name="j0">First column index.</param>
        /// <param name="j1">Last column index (inclusive).</param>
        /// <returns>A(i0:i1,j0:j1)</returns>
        /// <exception cref="System.IndexOutOfRangeException">Sub matrix indices</exception>
        public
        ComplexMatrix
        GetMatrix(
            int i0,
            int i1,
            int j0,
            int j1)
        {
            Complex[][] newData = CreateMatrixData(i1 - i0 + 1, j1 - j0 + 1);
            try
            {
                for(int i = i0; i <= i1; i++)
                {
                    for(int j = j0; j <= j1; j++)
                    {
                        newData[i - i0][j - j0] = _data[i][j];
                    }
                }
            }
            catch(IndexOutOfRangeException e)
            {
                throw new IndexOutOfRangeException(Properties.LocalStrings.ArgumentMatrixIndexOutOfRange, e);
            }

            return new ComplexMatrix(newData);
        }

        /// <summary>
        /// Gets a sub matrix.
        /// </summary>
        /// <param name="r">Array of row indices.</param>
        /// <param name="c">Array of column indices.</param>
        /// <returns>A(r(:),c(:))</returns>
        /// <exception cref="System.IndexOutOfRangeException">Sub matrix indices.</exception>
        public
        ComplexMatrix
        GetMatrix(
            int[] r,
            int[] c)
        {
            Complex[][] newData = CreateMatrixData(r.Length, c.Length);
            try
            {
                for(int i = 0; i < r.Length; i++)
                {
                    for(int j = 0; j < c.Length; j++)
                    {
                        newData[i][j] = _data[r[i]][c[j]];
                    }
                }
            }
            catch(IndexOutOfRangeException e)
            {
                throw new IndexOutOfRangeException(Properties.LocalStrings.ArgumentMatrixIndexOutOfRange, e);
            }

            return new ComplexMatrix(newData);
        }

        /// <summary>
        /// Get a sub matrix.
        /// </summary>
        /// <param name="i0">First row index.</param>
        /// <param name="i1">Last row index (inclusive).</param>
        /// <param name="c">Array of column indices.</param>
        /// <returns>A(i0:i1,c(:))</returns>
        /// <exception cref="System.IndexOutOfRangeException">Sub matrix indices.</exception>
        public
        ComplexMatrix
        GetMatrix(
            int i0,
            int i1,
            int[] c)
        {
            Complex[][] newData = CreateMatrixData(i1 - i0 + 1, c.Length);
            try
            {
                for(int i = i0; i <= i1; i++)
                {
                    for(int j = 0; j < c.Length; j++)
                    {
                        newData[i - i0][j] = _data[i][c[j]];
                    }
                }
            }
            catch(IndexOutOfRangeException e)
            {
                throw new IndexOutOfRangeException(Properties.LocalStrings.ArgumentMatrixIndexOutOfRange, e);
            }

            return new ComplexMatrix(newData);
        }

        /// <summary>
        /// Get a sub matrix.
        /// </summary>
        /// <param name="r">Array of row indices.</param>
        /// <param name="j0">First column index.</param>
        /// <param name="j1">Last column index (inclusive).</param>
        /// <returns>A(r(:),j0:j1)</returns>
        /// <exception cref="System.IndexOutOfRangeException">Sub matrix indices.</exception>
        public
        ComplexMatrix
        GetMatrix(
            int[] r,
            int j0,
            int j1)
        {
            Complex[][] newData = CreateMatrixData(r.Length, j1 - j0 + 1);
            try
            {
                for(int i = 0; i < r.Length; i++)
                {
                    for(int j = j0; j <= j1; j++)
                    {
                        newData[i][j - j0] = _data[r[i]][j];
                    }
                }
            }
            catch(IndexOutOfRangeException e)
            {
                throw new IndexOutOfRangeException(Properties.LocalStrings.ArgumentMatrixIndexOutOfRange, e);
            }

            return new ComplexMatrix(newData);
        }

        /// <summary>
        /// Set a sub matrix.
        /// </summary>
        /// <param name="i0">First row index.</param>
        /// <param name="i1">Last row index (inclusive).</param>
        /// <param name="j0">First column index.</param>
        /// <param name="j1">Last column index (inclusive).</param>
        /// <param name="x">A(i0:i1,j0:j1)</param>
        /// <exception cref="System.IndexOutOfRangeException">Sub matrix indices.</exception>
        public
        void
        SetMatrix(
            int i0,
            int i1,
            int j0,
            int j1,
            ComplexMatrix x)
        {
            try
            {
                for(int i = i0; i <= i1; i++)
                {
                    for(int j = j0; j <= j1; j++)
                    {
                        _data[i][j] = x[i - i0, j - j0];
                    }
                }
            }
            catch(IndexOutOfRangeException e)
            {
                throw new IndexOutOfRangeException(Properties.LocalStrings.ArgumentMatrixIndexOutOfRange, e);
            }

            ResetOnDemandComputations();
        }

        /// <summary>
        /// Sets a sub matrix.
        /// </summary>
        /// <param name="r">Array of row indices.</param>
        /// <param name="c">Array of column indices.</param>
        /// <param name="x">A(r(:),c(:))</param>
        /// <exception cref="System.IndexOutOfRangeException">Sub matrix indices</exception>
        public
        void
        SetMatrix(
            int[] r,
            int[] c,
            ComplexMatrix x)
        {
            try
            {
                for(int i = 0; i < r.Length; i++)
                {
                    for(int j = 0; j < c.Length; j++)
                    {
                        _data[r[i]][c[j]] = x[i, j];
                    }
                }
            }
            catch(IndexOutOfRangeException e)
            {
                throw new IndexOutOfRangeException(Properties.LocalStrings.ArgumentMatrixIndexOutOfRange, e);
            }

            ResetOnDemandComputations();
        }

        /// <summary>
        /// Sets a sub matrix.
        /// </summary>
        /// <param name="r">Array of row indices.</param>
        /// <param name="j0">First column index.</param>
        /// <param name="j1">Last column index (inclusive).</param>
        /// <param name="x">A(r(:),j0:j1)</param>
        /// <exception cref="System.IndexOutOfRangeException">Sub matrix indices</exception>
        public
        void
        SetMatrix(
            int[] r,
            int j0,
            int j1,
            ComplexMatrix x)
        {
            try
            {
                for(int i = 0; i < r.Length; i++)
                {
                    for(int j = j0; j <= j1; j++)
                    {
                        _data[r[i]][j] = x[i, j - j0];
                    }
                }
            }
            catch(IndexOutOfRangeException e)
            {
                throw new IndexOutOfRangeException(Properties.LocalStrings.ArgumentMatrixIndexOutOfRange, e);
            }

            ResetOnDemandComputations();
        }

        /// <summary>
        /// Set a sub matrix.
        /// </summary>
        /// <param name="i0">First row index.</param>
        /// <param name="i1">Last row index (inclusive).</param>
        /// <param name="c">Array of column indices.</param>
        /// <param name="x">A(i0:i1,c(:))</param>
        /// <exception cref="System.IndexOutOfRangeException">Sub matrix indices.</exception>
        public
        void
        SetMatrix(
            int i0,
            int i1,
            int[] c,
            ComplexMatrix x)
        {
            try
            {
                for(int i = i0; i <= i1; i++)
                {
                    for(int j = 0; j < c.Length; j++)
                    {
                        _data[i][c[j]] = x[i - i0, j];
                    }
                }
            }
            catch(IndexOutOfRangeException e)
            {
                throw new IndexOutOfRangeException(Properties.LocalStrings.ArgumentMatrixIndexOutOfRange, e);
            }

            ResetOnDemandComputations();
        }

        #endregion

        #region Norms computations

        /// <summary>One norm</summary>
        /// <returns>Maximum column sum.</returns>
        public
        double
        Norm1()
        {
            // TODO (ruegg, 2008-03-11): Change to property, consider cached on-demand.
            double f = 0;
            for(int j = 0; j < _columnCount; j++)
            {
                double s = 0;
                for(int i = 0; i < _rowCount; i++)
                {
                    s += _data[i][j].Modulus;
                }

                f = Math.Max(f, s);
            }

            return f;
        }

        #endregion

        #region Elementary linear operations

        /// <summary>
        /// Add another complex matrix to this complex matrix.
        /// </summary>
        /// <param name="b">The other complex matrix.</param>
        /// <returns>
        /// Matrix ret[i,j] = this[i,j] + b[i,j]
        /// </returns>
        /// <remarks>
        /// This method has the same effect as the overloaded + operator.
        /// </remarks>
        /// <seealso cref="AddInplace(IMatrix{Complex})"/>
        /// <seealso cref="operator + (ComplexMatrix, ComplexMatrix)"/>
        public
        ComplexMatrix
        Add(ComplexMatrix b)
        {
            CheckMatchingMatrixDimensions(this, b);

            Complex[][] newData = new Complex[_rowCount][];
            for(int i = 0; i < newData.Length; i++)
            {
                Complex[] thisRow = _data[i];
                Complex[] newRow = new Complex[_columnCount];
                for(int j = 0; j < newRow.Length; j++)
                {
                    newRow[j] = thisRow[j] + b[i, j];
                }

                newData[i] = newRow;
            }

            return new ComplexMatrix(newData);
        }

        /// <summary>
        /// Add another real matrix to this complex matrix.
        /// </summary>
        /// <param name="b">The other real matrix.</param>
        /// <returns>
        /// Matrix ret[i,j] = this[i,j] + b[i,j]
        /// </returns>
        /// <remarks>
        /// This method has the same effect as the overloaded + operator.
        /// </remarks>
        /// <seealso cref="AddInplace(IMatrix{double})"/>
        /// <seealso cref="operator + (ComplexMatrix, Matrix)"/>
        public
        ComplexMatrix
        Add(Matrix b)
        {
            CheckMatchingMatrixDimensions(this, b);

            Complex[][] newData = new Complex[_rowCount][];
            for(int i = 0; i < newData.Length; i++)
            {
                Complex[] thisRow = _data[i];
                Complex[] newRow = new Complex[_columnCount];
                for(int j = 0; j < newRow.Length; j++)
                {
                    newRow[j] = thisRow[j] + b._data[i][ j];
                }

                newData[i] = newRow;
            }

            return new ComplexMatrix(newData);
        }

        /// <summary>
        /// Add a complex scalar to all elements of this complex matrix.
        /// </summary>
        /// <param name="b">The complex scalar.</param>
        /// <returns>
        /// Matrix ret[i,j] = this[i,j] + b
        /// </returns>
        /// <remarks>
        /// This method has the same effect as the overloaded + operator.
        /// </remarks>
        /// <seealso cref="AddInplace(Complex)"/>
        /// <seealso cref="operator + (ComplexMatrix, Complex)"/>
        public
        ComplexMatrix
        Add(Complex b)
        {
            Complex[][] newData = new Complex[_rowCount][];
            for(int i = 0; i < newData.Length; i++)
            {
                Complex[] thisRow = _data[i];
                Complex[] newRow = new Complex[_columnCount];
                for(int j = 0; j < newRow.Length; j++)
                {
                    newRow[j] = thisRow[j] + b;
                }

                newData[i] = newRow;
            }

            return new ComplexMatrix(newData);
        }

        /// <summary>
        /// In place addition of another complex matrix to this complex matrix.
        /// </summary>
        /// <param name="b">The other complex matrix.</param>
        /// <remarks>
        /// This method changes this matrix.
        /// </remarks>
        /// <seealso cref="Add(IMatrix{Complex})"/>
        /// <seealso cref="operator + (ComplexMatrix, ComplexMatrix)"/>
        public
        void
        AddInplace(ComplexMatrix b)
        {
            CheckMatchingMatrixDimensions(this, b);

            for(int i = 0; i < _rowCount; i++)
            {
                for(int j = 0; j < _columnCount; j++)
                {
                    _data[i][j] += b[i, j];
                }
            }

            ResetOnDemandComputations();
        }

        /// <summary>
        /// In place addition of another real matrix to this complex matrix.
        /// </summary>
        /// <param name="b">The other real matrix.</param>
        /// <remarks>
        /// This method changes this matrix.
        /// </remarks>
        /// <seealso cref="Add(IMatrix{double})"/>
        /// <seealso cref="operator + (ComplexMatrix, Matrix)"/>
        public
        void
        AddInplace(Matrix b)
        {
            CheckMatchingMatrixDimensions(this, b);

            for(int i = 0; i < _rowCount; i++)
            {
                for(int j = 0; j < _columnCount; j++)
                {
                    _data[i][j] += b._data[i][ j];
                }
            }

            ResetOnDemandComputations();
        }

        /// <summary>
        /// In place addition of a complex scalar to all elements of this complex matrix.
        /// </summary>
        /// <param name="b">The complex scalar.</param>
        /// <remarks>
        /// This method changes this matrix.
        /// </remarks>
        /// <seealso cref="Add(Complex)"/>
        /// <seealso cref="operator + (ComplexMatrix, Complex)"/>
        public
        void
        AddInplace(Complex b)
        {
            for(int i = 0; i < _rowCount; i++)
            {
                for(int j = 0; j < _columnCount; j++)
                {
                    _data[i][j] += b;
                }
            }

            ResetOnDemandComputations();
        }

        /// <summary>
        /// Subtract another complex matrix from this complex matrix.
        /// </summary>
        /// <param name="b">The other complex matrix.</param>
        /// <returns>
        /// Matrix ret[i,j] = this[i,j] - b[i,j]
        /// </returns>
        /// <remarks>
        /// This method has the same effect as the overloaded - operator.
        /// </remarks>
        /// <seealso cref="SubtractInplace(IMatrix{Complex})"/>
        /// <seealso cref="operator - (ComplexMatrix, ComplexMatrix)"/>
        public
        ComplexMatrix
        Subtract(ComplexMatrix b)
        {
            CheckMatchingMatrixDimensions(this, b);

            Complex[][] newData = new Complex[_rowCount][];
            for(int i = 0; i < newData.Length; i++)
            {
                Complex[] thisRow = _data[i];
                Complex[] newRow = new Complex[_columnCount];
                for(int j = 0; j < newRow.Length; j++)
                {
                    newRow[j] = thisRow[j] - b[i, j];
                }

                newData[i] = newRow;
            }

            return new ComplexMatrix(newData);
        }

        /// <summary>
        /// Subtract another real matrix from this complex matrix.
        /// </summary>
        /// <param name="b">The other real matrix.</param>
        /// <returns>
        /// Matrix ret[i,j] = this[i,j] - b[i,j]
        /// </returns>
        /// <remarks>
        /// This method has the same effect as the overloaded - operator.
        /// </remarks>
        /// <seealso cref="SubtractInplace(IMatrix{double})"/>
        /// <seealso cref="operator - (ComplexMatrix, Matrix)"/>
        public
        ComplexMatrix
        Subtract(Matrix b)
        {
            CheckMatchingMatrixDimensions(this, b);

            Complex[][] newData = new Complex[_rowCount][];
            for(int i = 0; i < newData.Length; i++)
            {
                Complex[] thisRow = _data[i];
                Complex[] newRow = new Complex[_columnCount];
                for(int j = 0; j < newRow.Length; j++)
                {
                    newRow[j] = thisRow[j] - b._data[i][j];
                }

                newData[i] = newRow;
            }

            return new ComplexMatrix(newData);
        }

        /// <summary>
        /// Subtract a complex scalar from all elements of this complex matrix.
        /// </summary>
        /// <param name="b">The complex scalar.</param>
        /// <returns>
        /// Matrix ret[i,j] = this[i,j] - b
        /// </returns>
        /// <remarks>
        /// This method has the same effect as the overloaded - operator.
        /// </remarks>
        /// <seealso cref="SubtractInplace(Complex)"/>
        /// <seealso cref="operator - (ComplexMatrix, Complex)"/>
        public
        ComplexMatrix
        Subtract(Complex b)
        {
            Complex[][] newData = new Complex[_rowCount][];
            for(int i = 0; i < newData.Length; i++)
            {
                Complex[] thisRow = _data[i];
                Complex[] newRow = new Complex[_columnCount];
                for(int j = 0; j < newRow.Length; j++)
                {
                    newRow[j] = thisRow[j] - b;
                }

                newData[i] = newRow;
            }

            return new ComplexMatrix(newData);
        }

        /// <summary>
        /// In place subtraction of another complex matrix from this complex matrix.
        /// </summary>
        /// <param name="b">The other complex matrix.</param>
        /// <remarks>
        /// This method changes this matrix.
        /// </remarks>
        /// <seealso cref="Subtract(IMatrix{Complex})"/>
        /// <seealso cref="operator - (ComplexMatrix, ComplexMatrix)"/>
        public
        void
        SubtractInplace(ComplexMatrix b)
        {
            CheckMatchingMatrixDimensions(this, b);

            for(int i = 0; i < _rowCount; i++)
            {
                for(int j = 0; j < _columnCount; j++)
                {
                    _data[i][j] -= b[i, j];
                }
            }

            ResetOnDemandComputations();
        }

        /// <summary>
        /// In place subtraction of another real matrix from this complex matrix.
        /// </summary>
        /// <param name="b">The other real matrix.</param>
        /// <remarks>
        /// This method changes this matrix.
        /// </remarks>
        /// <seealso cref="Subtract(IMatrix{double})"/>
        /// <seealso cref="operator - (ComplexMatrix, Matrix)"/>
        public
        void
        SubtractInplace(Matrix b)
        {
            CheckMatchingMatrixDimensions(this, b);

            for(int i = 0; i < _rowCount; i++)
            {
                for(int j = 0; j < _columnCount; j++)
                {
                    _data[i][j] -= b._data[i][j];
                }
            }

            ResetOnDemandComputations();
        }

        /// <summary>
        /// In place subtraction of a complex scalar from all elements of this complex matrix.
        /// </summary>
        /// <param name="b">The complex scalar.</param>
        /// <remarks>
        /// This method changes this matrix.
        /// </remarks>
        /// <seealso cref="Subtract(Complex)"/>
        /// <seealso cref="operator - (ComplexMatrix, Complex)"/>
        public
        void
        SubtractInplace(Complex b)
        {
            for(int i = 0; i < _rowCount; i++)
            {
                for(int j = 0; j < _columnCount; j++)
                {
                    _data[i][j] -= b;
                }
            }

            ResetOnDemandComputations();
        }

        /// <summary>
        /// Negate this complex matrix.
        /// </summary>
        public
        ComplexMatrix
        Negate()
        {
            Complex[][] newData = new Complex[_rowCount][];
            for(int i = 0; i < newData.Length; i++)
            {
                Complex[] thisRow = _data[i];
                Complex[] newRow = new Complex[_columnCount];
                for(int j = 0; j < newRow.Length; j++)
                {
                    newRow[j] = -thisRow[j];
                }

                newData[i] = newRow;
            }

            return new ComplexMatrix(newData);
        }

        /// <summary>
        /// In place negation of this complex matrix.
        /// </summary>
        /// <remarks>
        /// This method changes this matrix.
        /// </remarks>
        public
        void
        NegateInplace()
        {
            for(int i = 0; i < _rowCount; i++)
            {
                for(int j = 0; j < _columnCount; j++)
                {
                    _data[i][j] = -_data[i][j];
                }
            }

            ResetOnDemandComputations();
        }

        /// <summary>
        /// Conjugate this complex matrix.
        /// </summary>
        public
        ComplexMatrix
        Conjugate()
        {
            Complex[][] newData = new Complex[_rowCount][];
            for(int i = 0; i < newData.Length; i++)
            {
                Complex[] thisRow = _data[i];
                Complex[] newRow = new Complex[_columnCount];
                for(int j = 0; j < newRow.Length; j++)
                {
                    newRow[j] = thisRow[j].Conjugate;
                }

                newData[i] = newRow;
            }

            return new ComplexMatrix(newData);
        }

        /// <summary>
        /// In place conjugation of this complex matrix.
        /// </summary>
        /// <remarks>
        /// This method changes this matrix.
        /// </remarks>
        public
        void
        ConjugateInplace()
        {
            for(int i = 0; i < _rowCount; i++)
            {
                for(int j = 0; j < _columnCount; j++)
                {
                    _data[i][j] = _data[i][j].Conjugate;
                }
            }

            ResetOnDemandComputations();
        }

        /// <summary>
        /// Complex matrix multiplication.
        /// </summary>
        /// <param name="b">The other complex matrix.</param>
        /// <returns>
        /// Matrix ret[i,j] = sum(this[i,k] * b[k,j])
        /// </returns>
        /// <remarks>
        /// This method has the same effect as the overloaded * operator.
        /// </remarks>
        /// <seealso cref="MultiplyInplace(IMatrix{Complex})"/>
        /// <seealso cref="operator * (ComplexMatrix, ComplexMatrix)"/>
        /// <exception cref="ArgumentNullException">B must not be null.</exception>
        /// <exception cref="ArgumentException">Matrix inner dimensions must agree.</exception>
        public
        ComplexMatrix
        Multiply(ComplexMatrix b)
        {
            if(null == b)
            {
                throw new ArgumentNullException("B");
            }

            if(b.RowCount != _columnCount)
            {
                throw new ArgumentException(Properties.LocalStrings.ArgumentMatrixSameDimensions);
            }

            Complex[][] newData = new Complex[_rowCount][];
            for(int i = 0; i < newData.Length; i++)
            {
                Complex[] thisRow = _data[i];
                Complex[] newRow = new Complex[b.ColumnCount];
                for(int j = 0; j < newRow.Length; j++)
                {
                    Complex s = Complex.Zero;
                    for(int k = 0; k < _columnCount; k++)
                    {
                        s += thisRow[k] * b[k, j];
                    }

                    newRow[j] = s;
                }

                newData[i] = newRow;
            }

            return new ComplexMatrix(newData);

            // TODO: The following code might be more performant for large
            // matrices. Measure and adapt if it indeed is faster.

            ////Complex[][] newData = CreateMatrixData(_rowCount, b.ColumnCount);
            ////for(int j = 0; j < b.ColumnCount; j++)
            ////{
            ////    // caching the column for performance
            ////    Complex[] columnB = new Complex[_columnCount];
            ////    for(int k = 0; k < _columnCount; k++)
            ////    {
            ////        columnB[k] = b._data[k][j];
            ////    }

            ////    // making the line-to-column product
            ////    for(int i = 0; i < _rowCount; i++)
            ////    {
            ////        Complex s = 0;

            ////        for(int k = 0; k < _columnCount; k++)
            ////        {
            ////            s += _data[i][k] * columnB[k];
            ////        }
            ////        newData[i][j] = s;
            ////    }
            ////}

            ////return new ComplexMatrix(newData);
        }

        /// <summary>
        /// Complex matrix multiplication.
        /// </summary>
        /// <param name="b">The other real matrix.</param>
        /// <returns>
        /// Matrix ret[i,j] = sum(this[i,k] * b[k,j])
        /// </returns>
        /// <remarks>
        /// This method has the same effect as the overloaded * operator.
        /// </remarks>
        /// <seealso cref="MultiplyInplace(IMatrix{double})"/>
        /// <seealso cref="operator * (ComplexMatrix, Matrix)"/>
        /// <exception cref="ArgumentNullException">B must not be null.</exception>
        /// <exception cref="ArgumentException">Matrix inner dimensions must agree.</exception>
        public
        ComplexMatrix
        Multiply(Matrix b)
        {
            if(null == b)
            {
                throw new ArgumentNullException("B");
            }

            if(b.RowCount != _columnCount)
            {
                throw new ArgumentException(Properties.LocalStrings.ArgumentMatrixSameDimensions);
            }

            Complex[][] newData = new Complex[_rowCount][];
            for(int i = 0; i < newData.Length; i++)
            {
                Complex[] thisRow = _data[i];
                Complex[] newRow = new Complex[b.ColumnCount];
                for(int j = 0; j < newRow.Length; j++)
                {
                    Complex s = Complex.Zero;
                    for(int k = 0; k < _columnCount; k++)
                    {
                        s += thisRow[k] * b._data[k][j];
                    }

                    newRow[j] = s;
                }

                newData[i] = newRow;
            }

            return new ComplexMatrix(newData);
        }

        /// <summary>
        /// Scale this complex matrix with a complex scalar.
        /// </summary>
        /// <param name="b">The other complex scalar.</param>
        /// <returns>
        /// Matrix ret[i,j] = this[i,j] * b
        /// </returns>
        /// <remarks>
        /// This method has the same effect as the overloaded * operator.
        /// </remarks>
        /// <seealso cref="MultiplyInplace(Complex)"/>
        /// <seealso cref="operator * (ComplexMatrix, Complex)"/>
        /// <exception cref="ArgumentNullException">B must not be null.</exception>
        /// <exception cref="ArgumentException">Matrix inner dimensions must agree.</exception>
        public
        ComplexMatrix
        Multiply(Complex b)
        {
            Complex[][] newData = new Complex[_rowCount][];
            for(int i = 0; i < newData.Length; i++)
            {
                Complex[] thisRow = _data[i];
                Complex[] newRow = new Complex[_columnCount];
                for(int j = 0; j < newRow.Length; j++)
                {
                    newRow[j] = thisRow[j] * b;
                }

                newData[i] = newRow;
            }

            return new ComplexMatrix(newData);
        }

        /// <summary>
        /// Inplace scale this matrix by a complex scalar.
        /// </summary>
        /// <param name="b">The other complex scalar.</param>
        /// <remarks>
        /// This method changes this matrix.
        /// </remarks>
        /// <seealso cref="Multiply(Complex)"/>
        /// <seealso cref="operator * (ComplexMatrix, Complex)"/>
        public
        void
        MultiplyInplace(Complex b)
        {
            for(int i = 0; i < _rowCount; i++)
            {
                for(int j = 0; j < _columnCount; j++)
                {
                    _data[i][j] *= b;
                }
            }

            ResetOnDemandComputations();
        }

        /// <summary>
        /// Inplace complex square matrix multiplication.
        /// </summary>
        /// <param name="b">The other square complex matrix.</param>
        /// <remarks>
        /// This method changes this matrix. Only square matrices are supported.
        /// </remarks>
        /// <seealso cref="Multiply(IMatrix{Complex})"/>
        /// <seealso cref="operator * (ComplexMatrix, ComplexMatrix)"/>
        /// <exception cref="ArgumentNullException">B must not be null.</exception>
        /// <exception cref="ArgumentException">Matrix inner dimensions must agree.</exception>
        public
        void
        MultiplyInplace(ComplexMatrix b)
        {
            if(null == b)
            {
                throw new ArgumentNullException("B");
            }

            if(_rowCount != _columnCount || b.RowCount != b.ColumnCount)
            {
                throw new ArgumentException(Properties.LocalStrings.ArgumentMatrixSquare);
            }

            if(_rowCount != b.RowCount)
            {
                throw new ArgumentException(Properties.LocalStrings.ArgumentMatrixSameDimensions);
            }

            Complex[] tempRow = new Complex[_columnCount];
            for(int i = 0; i < _data.Length; i++)
            {
                Complex[] thisRow = _data[i];
                for(int j = 0; j < tempRow.Length; j++)
                {
                    Complex s = Complex.Zero;
                    for(int k = 0; k < thisRow.Length; k++)
                    {
                        s += thisRow[k] * b[k, j];
                    }

                    tempRow[j] = s;
                }

                _data[i] = tempRow;
                tempRow = thisRow;
            }

            ResetOnDemandComputations();
        }

        /// <summary>
        /// Inplace complex square matrix multiplication.
        /// </summary>
        /// <param name="b">The other square real matrix.</param>
        /// <remarks>
        /// This method changes this matrix. Only square matrices are supported.
        /// </remarks>
        /// <seealso cref="Multiply(IMatrix{double})"/>
        /// <seealso cref="operator * (ComplexMatrix, Matrix)"/>
        /// <exception cref="ArgumentNullException">B must not be null.</exception>
        /// <exception cref="ArgumentException">Matrix inner dimensions must agree.</exception>
        public
        void
        MultiplyInplace(Matrix b)
        {
            if(null == b)
            {
                throw new ArgumentNullException("B");
            }

            if(_rowCount != _columnCount || b.RowCount != b.ColumnCount)
            {
                throw new ArgumentException(Properties.LocalStrings.ArgumentMatrixSquare);
            }

            if(_rowCount != b.RowCount)
            {
                throw new ArgumentException(Properties.LocalStrings.ArgumentMatrixSameDimensions);
            }

            Complex[] tempRow = new Complex[_columnCount];
            for(int i = 0; i < _data.Length; i++)
            {
                Complex[] thisRow = _data[i];
                for(int j = 0; j < tempRow.Length; j++)
                {
                    Complex s = Complex.Zero;
                    for(int k = 0; k < thisRow.Length; k++)
                    {
                        s += thisRow[k] * b._data[k][j];
                    }

                    tempRow[j] = s;
                }

                _data[i] = tempRow;
                tempRow = thisRow;
            }

            ResetOnDemandComputations();
        }

        /// <summary>
        /// Multiply this matrix with a right complex column vector.
        /// </summary>
        /// <param name="b">The right complex column vector.</param>
        /// <returns>
        /// Vector ret[i] = sum(this[i,k] * b[k])
        /// </returns>
        /// <remarks>
        /// This method has the same effect as the overloaded * operator.
        /// </remarks>
        /// <seealso cref="operator * (ComplexMatrix, ComplexVector)"/>
        /// <exception cref="ArgumentNullException">B must not be null.</exception>
        /// <exception cref="ArgumentException">Matrix inner dimensions must agree.</exception>
        public
        ComplexVector
        MultiplyRightColumn(ComplexVector b)
        {
            if(null == b)
            {
                throw new ArgumentNullException("B");
            }

            if(b.Length != _columnCount)
            {
                throw new ArgumentException(Properties.LocalStrings.ArgumentMatrixSameDimensions);
            }

            Complex[] newData = new Complex[_rowCount];
            for(int i = 0; i < newData.Length; i++)
            {
                Complex[] thisRow = _data[i];
                Complex s = Complex.Zero;
                for(int j = 0; j < thisRow.Length; j++)
                {
                    s += thisRow[j] * b[j];
                }

                newData[i] = s;
            }

            return new ComplexVector(newData);
        }

        /// <summary>
        /// Multiply this matrix with a right real column vector.
        /// </summary>
        /// <param name="b">The right real column vector.</param>
        /// <returns>
        /// Vector ret[i] = sum(this[i,k] * b[k])
        /// </returns>
        /// <remarks>
        /// This method has the same effect as the overloaded * operator.
        /// </remarks>
        /// <seealso cref="operator * (ComplexMatrix, Vector)"/>
        /// <exception cref="ArgumentNullException">B must not be null.</exception>
        /// <exception cref="ArgumentException">Matrix inner dimensions must agree.</exception>
        public
        ComplexVector
        MultiplyRightColumn(Vector b)
        {
            if(null == b)
            {
                throw new ArgumentNullException("B");
            }

            if(b.Length != _columnCount)
            {
                throw new ArgumentException(Properties.LocalStrings.ArgumentMatrixSameDimensions);
            }

            Complex[] newData = new Complex[_rowCount];
            for(int i = 0; i < newData.Length; i++)
            {
                Complex[] thisRow = _data[i];
                Complex s = Complex.Zero;
                for(int j = 0; j < thisRow.Length; j++)
                {
                    s += thisRow[j] * b._data[j];
                }

                newData[i] = s;
            }

            return new ComplexVector(newData);
        }

        /// <summary>
        /// Multiply a diagonal complex matrix with this matrix. This has the same effect
        /// as scaling the rows of this matrix by the scalar elements of the diagonal.
        /// </summary>
        /// <param name="diagonal">The left diagonal complex matrix.</param>
        /// <returns>
        /// ComplexMatrix ret[i,j] = this[i,j] * diagonal[i]
        /// </returns>
        /// <remarks>
        /// This method has the same effect as the overloaded * operator.
        /// </remarks>
        public
        ComplexMatrix
        MultiplyLeftDiagonal(ComplexVector diagonal)
        {
            if(null == diagonal)
            {
                throw new ArgumentNullException("diagonal");
            }

            if(_rowCount != diagonal.Length)
            {
                throw new ArgumentException(Properties.LocalStrings.ArgumentMatrixSameDimensions);
            }

            Complex[][] newData = new Complex[_rowCount][];
            for(int i = 0; i < newData.Length; i++)
            {
                Complex s = diagonal[i];
                Complex[] thisRow = _data[i];
                Complex[] newRow = new Complex[_columnCount];
                for(int j = 0; j < newRow.Length; j++)
                {
                    newRow[j] = thisRow[j] * s;
                }

                newData[i] = newRow;
            }

            return new ComplexMatrix(newData);
        }

        /// <summary>
        /// Multiply a diagonal real matrix with this matrix. This has the same effect
        /// as scaling the rows of this matrix by the scalar elements of the diagonal.
        /// </summary>
        /// <param name="diagonal">The left diagonal real matrix.</param>
        /// <returns>
        /// ComplexMatrix ret[i,j] = this[i,j] * diagonal[i]
        /// </returns>
        /// <remarks>
        /// This method has the same effect as the overloaded * operator.
        /// </remarks>
        public
        ComplexMatrix
        MultiplyLeftDiagonal(Vector diagonal)
        {
            if(null == diagonal)
            {
                throw new ArgumentNullException("diagonal");
            }

            if(_rowCount != diagonal.Length)
            {
                throw new ArgumentException(Properties.LocalStrings.ArgumentMatrixSameDimensions);
            }

            Complex[][] newData = new Complex[_rowCount][];
            for(int i = 0; i < newData.Length; i++)
            {
                double s = diagonal._data[i];
                Complex[] thisRow = _data[i];
                Complex[] newRow = new Complex[_columnCount];
                for(int j = 0; j < newRow.Length; j++)
                {
                    newRow[j] = thisRow[j] * s;
                }

                newData[i] = newRow;
            }

            return new ComplexMatrix(newData);
        }

        /// <summary>
        /// Inplace multiply a complex diagonal matrix with this matrix. This has the same effect
        /// as scaling the rows of this matrix by the scalar elements of the diagonal.
        /// </summary>
        /// <param name="diagonal">The left diagonal complex matrix.</param>
        /// <remarks>
        /// This method changes this matrix.
        /// </remarks>
        public
        void
        MultiplyLeftDiagonalInplace(ComplexVector diagonal)
        {
            if(null == diagonal)
            {
                throw new ArgumentNullException("diagonal");
            }

            if(_rowCount != diagonal.Length)
            {
                throw new ArgumentException(Properties.LocalStrings.ArgumentMatrixSameDimensions);
            }

            for(int i = 0; i < _data.Length; i++)
            {
                Complex s = diagonal[i];
                Complex[] thisRow = _data[i];
                for(int j = 0; j < thisRow.Length; j++)
                {
                    thisRow[j] *= s;
                }
            }

            ResetOnDemandComputations();
        }

        /// <summary>
        /// Inplace multiply a real diagonal matrix with this matrix. This has the same effect
        /// as scaling the rows of this matrix by the scalar elements of the diagonal.
        /// </summary>
        /// <param name="diagonal">The left diagonal real matrix.</param>
        /// <remarks>
        /// This method changes this matrix.
        /// </remarks>
        public
        void
        MultiplyLeftDiagonalInplace(Vector diagonal)
        {
            if(null == diagonal)
            {
                throw new ArgumentNullException("diagonal");
            }

            if(_rowCount != diagonal.Length)
            {
                throw new ArgumentException(Properties.LocalStrings.ArgumentMatrixSameDimensions);
            }

            for(int i = 0; i < _rowCount; i++)
            {
                double s = diagonal._data[i];
                Complex[] thisRow = _data[i];
                for(int j = 0; j < thisRow.Length; j++)
                {
                    thisRow[j] *= s;
                }
            }

            ResetOnDemandComputations();
        }

        /// <summary>
        /// Multiply this matrix with a complex diagonal matrix. This has the same effect
        /// as scaling the columns of this matrix by the scalar elements of the diagonal.
        /// </summary>
        /// <param name="diagonal">The right diagonal complex matrix.</param>
        /// <returns>
        /// ComplexMatrix ret[i,j] = this[i,j] * diagonal[j]
        /// </returns>
        /// <remarks>
        /// This method has the same effect as the overloaded * operator.
        /// </remarks>
        public
        ComplexMatrix
        MultiplyRightDiagonal(ComplexVector diagonal)
        {
            if(null == diagonal)
            {
                throw new ArgumentNullException("diagonal");
            }

            if(_columnCount != diagonal.Length)
            {
                throw new ArgumentException(Properties.LocalStrings.ArgumentMatrixSameDimensions);
            }

            Complex[][] newData = new Complex[_rowCount][];
            for(int i = 0; i < newData.Length; i++)
            {
                Complex[] thisRow = _data[i];
                Complex[] newRow = new Complex[_columnCount];
                for(int j = 0; j < newRow.Length; j++)
                {
                    newRow[j] = thisRow[j] * diagonal[j];
                }

                newData[i] = newRow;
            }

            return new ComplexMatrix(newData);
        }

        /// <summary>
        /// Multiply this matrix with a real diagonal matrix. This has the same effect
        /// as scaling the columns of this matrix by the scalar elements of the diagonal.
        /// </summary>
        /// <param name="diagonal">The right diagonal real matrix.</param>
        /// <returns>
        /// ComplexMatrix ret[i,j] = this[i,j] * diagonal[j]
        /// </returns>
        /// <remarks>
        /// This method has the same effect as the overloaded * operator.
        /// </remarks>
        public
        ComplexMatrix
        MultiplyRightDiagonal(Vector diagonal)
        {
            if(null == diagonal)
            {
                throw new ArgumentNullException("diagonal");
            }

            if(_columnCount != diagonal.Length)
            {
                throw new ArgumentException(Properties.LocalStrings.ArgumentMatrixSameDimensions);
            }

            Complex[][] newData = new Complex[_rowCount][];
            for(int i = 0; i < newData.Length; i++)
            {
                Complex[] thisRow = _data[i];
                Complex[] newRow = new Complex[_columnCount];
                for(int j = 0; j < newRow.Length; j++)
                {
                    newRow[j] = thisRow[j] * diagonal._data[j];
                }

                newData[i] = newRow;
            }

            return new ComplexMatrix(newData);
        }

        /// <summary>
        /// Inplace multiply this matrix with a complex diagonal matrix. This has the same effect
        /// as scaling the columns of this matrix by the scalar elements of the diagonal.
        /// </summary>
        /// <param name="diagonal">The right diagonal complex matrix.</param>
        /// <remarks>
        /// This method changes this matrix.
        /// </remarks>
        public
        void
        MultiplyRightDiagonalInplace(ComplexVector diagonal)
        {
            if(null == diagonal)
            {
                throw new ArgumentNullException("diagonal");
            }

            if(_columnCount != diagonal.Length)
            {
                throw new ArgumentException(Properties.LocalStrings.ArgumentMatrixSameDimensions);
            }

            for(int i = 0; i < _data.Length; i++)
            {
                Complex[] thisRow = _data[i];
                for(int j = 0; j < thisRow.Length; j++)
                {
                    thisRow[j] *= diagonal[j];
                }
            }

            ResetOnDemandComputations();
        }

        /// <summary>
        /// Inplace multiply this matrix with a real diagonal matrix. This has the same effect
        /// as scaling the columns of this matrix by the scalar elements of the diagonal.
        /// </summary>
        /// <param name="diagonal">The right diagonal real matrix.</param>
        /// <remarks>
        /// This method changes this matrix.
        /// </remarks>
        public
        void
        MultiplyRightDiagonalInplace(Vector diagonal)
        {
            if(null == diagonal)
            {
                throw new ArgumentNullException("diagonal");
            }

            if(_columnCount != diagonal.Length)
            {
                throw new ArgumentException(Properties.LocalStrings.ArgumentMatrixSameDimensions);
            }

            for(int i = 0; i < _data.Length; i++)
            {
                Complex[] thisRow = _data[i];
                for(int j = 0; j < thisRow.Length; j++)
                {
                    thisRow[j] *= diagonal._data[j];
                }
            }

            ResetOnDemandComputations();
        }

        /// <summary>
        /// Adds in place the scaled Matrix <c>s*m</c> to this Matrix.
        /// </summary>
        /// <remarks>
        /// This method changes this matrix.
        /// </remarks>
        public
        void
        MultiplyAccumulateInplace(ComplexMatrix m, Complex s)
        {
            CheckMatchingMatrixDimensions(this, m);

            for(int i = 0; i < _rowCount; i++)
            {
                Complex[] thisRow = _data[i];
                for(int j = 0; j < thisRow.Length; j++)
                {
                    thisRow[j] += s * m[i, j];
                }
            }

            ResetOnDemandComputations();
        }

        #endregion

        #region Additional elementary operations

        /// <summary>
        /// Transpose this complex matrix. The elements are not conjugated by this method,
        /// see <see cref="HermitianTranspose"/> for conjugated transposing.
        /// </summary>
        public
        ComplexMatrix
        Transpose()
        {
            Complex[][] newData = new Complex[_columnCount][];
            for(int i = 0; i < newData.Length; i++)
            {
                Complex[] newRow = new Complex[_rowCount];
                for(int j = 0; j < newRow.Length; j++)
                {
                    newRow[j] = _data[j][i];
                }

                newData[i] = newRow;
            }

            return new ComplexMatrix(newData);
        }

        /// <summary>
        /// Inplace transpose this square complex matrix. The elements are not conjugated by this method,
        /// see <see cref="HermitianTransposeInplace"/> for conjugated transposing.
        /// </summary>
        /// <remarks>
        /// This method changes this matrix. Only square matrices are supported.
        /// </remarks>
        public
        void
        TransposeInplace()
        {
            if(_rowCount != _columnCount)
            {
                throw new InvalidOperationException(Properties.LocalStrings.ArgumentMatrixSquare);
            }

            for(int i = 0; i < _data.Length; i++)
            {
                Complex[] thisRow = _data[i];
                for(int j = i + 1; j < thisRow.Length; j++)
                {
                    Complex swap = _data[j][i];
                    _data[j][i] = thisRow[j];
                    thisRow[j] = swap;
                }
            }

            ResetOnDemandComputations();
        }

        /// <summary>
        /// Transpose this complex matrix. The elements conjugated by this method,
        /// see <see cref="Transpose"/> for non-conjugated transposing.
        /// </summary>
        public
        ComplexMatrix
        HermitianTranspose()
        {
            Complex[][] newData = new Complex[_columnCount][];
            for(int i = 0; i < newData.Length; i++)
            {
                Complex[] newRow = new Complex[_rowCount];
                for(int j = 0; j < newRow.Length; j++)
                {
                    newRow[j] = _data[j][i].Conjugate;
                }

                newData[i] = newRow;
            }

            return new ComplexMatrix(newData);
        }

        /// <summary>
        /// Inplace transpose this square complex matrix. The elements are conjugated by this method,
        /// see <see cref="Transpose"/> for non-conjugated transposing.
        /// </summary>
        /// <remarks>
        /// This method changes this matrix. Only square matrices are supported.
        /// </remarks>
        public
        void
        HermitianTransposeInplace()
        {
            if(_rowCount != _columnCount)
            {
                throw new InvalidOperationException(Properties.LocalStrings.ArgumentMatrixSquare);
            }

            for(int i = 0; i < _data.Length; i++)
            {
                Complex[] thisRow = _data[i];
                thisRow[i] = thisRow[i].Conjugate;
                for(int j = i + 1; j < thisRow.Length; j++)
                {
                    Complex swap = _data[j][i].Conjugate;
                    _data[j][i] = thisRow[j].Conjugate;
                    thisRow[j] = swap;
                }
            }

            ResetOnDemandComputations();
        }

        /// <summary>
        /// Tensor Product (Kronecker) of this and another matrix.
        /// </summary>
        /// <param name="b">The matrix to operate on.</param>
        /// <returns>
        /// Kronecker Product of this and the given matrix.
        /// </returns>
        public
        ComplexMatrix
        TensorMultiply(ComplexMatrix b)
        {
            return KroneckerProduct(this, b);
        }

        /// <summary>
        /// Kronecker Product of two matrices.
        /// </summary>
        public static
        ComplexMatrix
        KroneckerProduct(
            ComplexMatrix a,
            ComplexMatrix b)
        {
            // Matrix to be created
            ComplexMatrix outMat = new ComplexMatrix(a.RowCount * b.RowCount, a.ColumnCount * b.ColumnCount);
            Complex[][] aData = a._data;

            for(int i = 0; i < a.RowCount; i++)
            {
                int rowOffset = i * b.RowCount;
                for(int j = 0; j < a.ColumnCount; j++)
                {
                    int colOffset = j * b.ColumnCount;
                    ComplexMatrix partMat = aData[i][j] * b;

                    outMat.SetMatrix(
                        rowOffset,
                        rowOffset + b.RowCount - 1,
                        colOffset,
                        colOffset + b.RowCount - 1,
                        partMat);
                }
            }

            return outMat;
        }

        #endregion

        #region Array operation on matrices

        /// <summary>
        /// Element-by-element multiplication of this matrix with another complex matrix, "ret = this .* b".
        /// </summary>
        /// <param name="b">The other complex matrix.</param>
        /// <returns>
        /// Matrix ret[i,j] = this[i,j] * b[i,j]
        /// </returns>
        /// <seealso cref="ArrayMultiplyInplace(IMatrix{Complex})"/>
        public
        ComplexMatrix
        ArrayMultiply(ComplexMatrix b)
        {
            CheckMatchingMatrixDimensions(this, b);

            Complex[][] newData = new Complex[_rowCount][];
            for(int i = 0; i < newData.Length; i++)
            {
                Complex[] thisRow = _data[i];
                Complex[] newRow = new Complex[_columnCount];
                for(int j = 0; j < newRow.Length; j++)
                {
                    newRow[j] = thisRow[j] * b[i, j];
                }

                newData[i] = newRow;
            }

            return new ComplexMatrix(newData);
        }

        /// <summary>
        /// Element-by-element multiplication of this matrix with another real matrix, "ret = this .* b".
        /// </summary>
        /// <param name="b">The other real matrix.</param>
        /// <returns>
        /// Matrix ret[i,j] = this[i,j] * b[i,j]
        /// </returns>
        /// <seealso cref="ArrayMultiplyInplace(IMatrix{double})"/>
        public
        ComplexMatrix
        ArrayMultiply(Matrix b)
        {
            CheckMatchingMatrixDimensions(this, b);

            Complex[][] newData = new Complex[_rowCount][];
            for(int i = 0; i < newData.Length; i++)
            {
                Complex[] thisRow = _data[i];
                Complex[] newRow = new Complex[_columnCount];
                for(int j = 0; j < newRow.Length; j++)
                {
                    newRow[j] = thisRow[j] * b._data[i][j];
                }

                newData[i] = newRow;
            }

            return new ComplexMatrix(newData);
        }

        /// <summary>
        /// Inplace element-by-element multiplication of this matrix with another complex matrix, "this .*= b".
        /// </summary>
        /// <param name="b">The other complex matrix.</param>
        /// <remarks>
        /// This method changes this matrix.
        /// </remarks>
        /// <seealso cref="ArrayMultiply(IMatrix{Complex})"/>
        public
        void
        ArrayMultiplyInplace(ComplexMatrix b)
        {
            CheckMatchingMatrixDimensions(this, b);

            for(int i = 0; i < _data.Length; i++)
            {
                Complex[] thisRow = _data[i];
                for(int j = 0; j < thisRow.Length; j++)
                {
                    thisRow[j] *= b[i, j];
                }
            }

            ResetOnDemandComputations();
        }

        /// <summary>
        /// Inplace element-by-element multiplication of this matrix with another real matrix, "this .*= b".
        /// </summary>
        /// <param name="b">The other real matrix.</param>
        /// <remarks>
        /// This method changes this matrix.
        /// </remarks>
        /// <seealso cref="ArrayMultiply(IMatrix{Complex})"/>
        public
        void
        ArrayMultiplyInplace(Matrix b)
        {
            CheckMatchingMatrixDimensions(this, b);

            for(int i = 0; i < _data.Length; i++)
            {
                Complex[] thisRow = _data[i];
                for(int j = 0; j < thisRow.Length; j++)
                {
                    thisRow[j] *= b._data[i][j];
                }
            }

            ResetOnDemandComputations();
        }

        /// <summary>
        /// Element-by-element division of this matrix with another complex matrix, "ret = this ./ b".
        /// </summary>
        /// <param name="b">The other complex matrix.</param>
        /// <returns>
        /// Matrix ret[i,j] = this[i,j] / b[i,j]
        /// </returns>
        /// <seealso cref="ArrayDivideInplace(IMatrix{Complex})"/>
        public
        ComplexMatrix
        ArrayDivide(ComplexMatrix b)
        {
            CheckMatchingMatrixDimensions(this, b);

            Complex[][] newData = new Complex[_rowCount][];
            for(int i = 0; i < newData.Length; i++)
            {
                Complex[] thisRow = _data[i];
                Complex[] newRow = new Complex[_columnCount];
                for(int j = 0; j < newRow.Length; j++)
                {
                    newRow[j] = thisRow[j] / b[i, j];
                }

                newData[i] = newRow;
            }

            return new ComplexMatrix(newData);
        }

        /// <summary>
        /// Element-by-element division of this matrix with another real matrix, "ret = this ./ b".
        /// </summary>
        /// <param name="b">The other real matrix.</param>
        /// <returns>
        /// Matrix ret[i,j] = this[i,j] / b[i,j]
        /// </returns>
        /// <seealso cref="ArrayDivideInplace(IMatrix{double})"/>
        public
        ComplexMatrix
        ArrayDivide(Matrix b)
        {
            CheckMatchingMatrixDimensions(this, b);

            Complex[][] newData = new Complex[_rowCount][];
            for(int i = 0; i < newData.Length; i++)
            {
                Complex[] thisRow = _data[i];
                Complex[] newRow = new Complex[_columnCount];
                for(int j = 0; j < newRow.Length; j++)
                {
                    newRow[j] = thisRow[j] / b._data[i][j];
                }

                newData[i] = newRow;
            }

            return new ComplexMatrix(newData);
        }

        /// <summary>
        /// Inplace element-by-element division of this matrix with another complex matrix, "this ./= b".
        /// </summary>
        /// <param name="b">The other complex matrix.</param>
        /// <remarks>
        /// This method changes this matrix.
        /// </remarks>
        /// <seealso cref="ArrayDivide(IMatrix{Complex})"/>
        public
        void
        ArrayDivideInplace(ComplexMatrix b)
        {
            CheckMatchingMatrixDimensions(this, b);

            for(int i = 0; i < _data.Length; i++)
            {
                Complex[] thisRow = _data[i];
                for(int j = 0; j < thisRow.Length; j++)
                {
                    thisRow[j] /= b[i, j];
                }
            }

            ResetOnDemandComputations();
        }

        /// <summary>
        /// Inplace element-by-element division of this matrix with another real matrix, "this ./= b". 
        /// </summary>
        /// <param name="b">The other real matrix.</param>
        /// <remarks>
        /// This method changes this matrix.
        /// </remarks>
        /// <seealso cref="ArrayMultiply(IMatrix{Complex})"/>
        public
        void
        ArrayDivideInplace(Matrix b)
        {
            CheckMatchingMatrixDimensions(this, b);

            for(int i = 0; i < _data.Length; i++)
            {
                Complex[] thisRow = _data[i];
                for(int j = 0; j < thisRow.Length; j++)
                {
                    thisRow[j] /= b._data[i][j];
                }
            }

            ResetOnDemandComputations();
        }

        /// <summary>
        /// Element-by-element raise to power, "ret = this .^ exponent".
        /// </summary>
        /// <param name="exponent">The exponent to raise to power to.</param>
        /// <returns>
        /// Matrix ret[i,j] = this[i,j] ^ exponent
        /// </returns>
        /// <seealso cref="ArrayPowerInplace(Complex)"/>
        public
        ComplexMatrix
        ArrayPower(Complex exponent)
        {
            Complex[][] newData = new Complex[_rowCount][];
            for(int i = 0; i < newData.Length; i++)
            {
                Complex[] thisRow = _data[i];
                Complex[] newRow = new Complex[_columnCount];
                for(int j = 0; j < newRow.Length; j++)
                {
                    newRow[j] = thisRow[j].Power(exponent);
                }

                newData[i] = newRow;
            }

            return new ComplexMatrix(newData);
        }

        /// <summary>
        /// Inplace element-by-element raise to power, "this .^= exponent".
        /// </summary>
        /// <param name="exponent">The exponent to raise to power to.</param>
        /// <remarks>
        /// This method changes this matrix.
        /// </remarks>
        /// <seealso cref="ArrayPower(Complex)"/>
        public
        void
        ArrayPowerInplace(Complex exponent)
        {
            for(int i = 0; i < _data.Length; i++)
            {
                Complex[] thisRow = _data[i];
                for(int j = 0; j < thisRow.Length; j++)
                {
                    thisRow[j] = thisRow[j].Power(exponent);
                }
            }

            ResetOnDemandComputations();
        }

        /// <summary>
        /// Map an arbitrary function to all elements of this matrix.
        /// </summary>
        /// <param name="mapping">The element-by-element mapping.</param>
        /// <returns>
        /// Matrix ret[i,j] = mapping(this[i,j])
        /// </returns>
        /// <seealso cref="ArrayMapInplace(Converter{Complex,Complex})"/>
        public
        ComplexMatrix
        ArrayMap(Converter<Complex, Complex> mapping)
        {
            Complex[][] newData = new Complex[_rowCount][];
            for(int i = 0; i < newData.Length; i++)
            {
                Complex[] thisRow = _data[i];
                Complex[] newRow = new Complex[_columnCount];
                for(int j = 0; j < newRow.Length; j++)
                {
                    newRow[j] = mapping(thisRow[j]);
                }

                newData[i] = newRow;
            }

            return new ComplexMatrix(newData);
        }

        /// <summary>
        /// Inplace map an arbitrary function to all elements of this matrix.
        /// </summary>
        /// <param name="mapping">The element-by-element mapping.</param>
        /// <remarks>
        /// This method changes this matrix.
        /// </remarks>
        /// <seealso cref="ArrayMap(Converter{Complex,Complex})"/>
        public
        void
        ArrayMapInplace(Converter<Complex, Complex> mapping)
        {
            for(int i = 0; i < _data.Length; i++)
            {
                Complex[] thisRow = _data[i];
                for(int j = 0; j < thisRow.Length; j++)
                {
                    thisRow[j] = mapping(thisRow[j]);
                }
            }

            ResetOnDemandComputations();
        }

        #endregion

        #region Arithmetic Operator Overloading

        /// <summary>
        /// Addition Operator
        /// </summary>
        public static
        ComplexMatrix
        operator +(
            ComplexMatrix m1,
            ComplexMatrix m2)
        {
            CheckMatchingMatrixDimensions(m1, m2);
            return m1.Add(m2);
        }

        /// <summary>
        /// Addition Operator
        /// </summary>
        public static
        ComplexMatrix
        operator +(
            ComplexMatrix m1,
            Matrix m2)
        {
            CheckMatchingMatrixDimensions(m1, m2);
            return m1.Add(m2);
        }

        /// <summary>
        /// Addition Operator
        /// </summary>
        public static
        ComplexMatrix
        operator +(
            ComplexMatrix m1,
            Complex scalar)
        {
            return m1.Add(scalar);
        }

        /// <summary>
        /// Addition Operator
        /// </summary>
        public static
        ComplexMatrix
        operator +(
            Complex scalar,
            ComplexMatrix m2)
        {
            return m2.Add(scalar);
        }

        /// <summary>
        /// Subtraction Operator
        /// </summary>
        public static
        ComplexMatrix
        operator -(
            ComplexMatrix m1,
            ComplexMatrix m2)
        {
            CheckMatchingMatrixDimensions(m1, m2);
            return m1.Subtract(m2);
        }

        /// <summary>
        /// Subtraction Operator
        /// </summary>
        public static
        ComplexMatrix
        operator -(
            ComplexMatrix m1,
            Matrix m2)
        {
            CheckMatchingMatrixDimensions(m1, m2);
            return m1.Subtract(m2);
        }

        /// <summary>
        /// Subtraction Operator
        /// </summary>
        public static
        ComplexMatrix
        operator -(
            ComplexMatrix m1,
            Complex scalar)
        {
            return m1.Subtract(scalar);
        }

        /// <summary>
        /// Negation Operator
        /// </summary>
        public static
        ComplexMatrix
        operator -(ComplexMatrix m1)
        {
            return m1.Negate();
        }

        /// <summary>
        /// Multiplication Operator.
        /// </summary>
        /// <exception cref="System.ArgumentException">Matrix inner dimensions must agree.</exception>
        public static
        ComplexMatrix
        operator *(
            ComplexMatrix m1,
            ComplexMatrix m2)
        {
            return m1.Multiply(m2);
        }

        /// <summary>
        /// Multiplication Operator.
        /// </summary>
        /// <exception cref="System.ArgumentException">Matrix inner dimensions must agree.</exception>
        public static
        ComplexMatrix
        operator *(
            ComplexMatrix m1,
            Matrix m2)
        {
            return m1.Multiply(m2);
        }

        /// <summary>
        /// Multiplication of a matrix by a scalar, C = s*A
        /// </summary>
        public static
        ComplexMatrix
        operator *(
            Complex s,
            ComplexMatrix m)
        {
            return m.Multiply(s);
        }

        /// <summary>
        /// Multiplication of a matrix by a scalar, C = s*A
        /// </summary>
        public static
        ComplexMatrix
        operator *(
            ComplexMatrix m,
            Complex s)
        {
            return m.Multiply(s);
        }

        /// <summary>
        /// Multiply a complex matrix with a complex column vector.
        /// </summary>
        /// <exception cref="System.ArgumentException">Matrix inner dimensions must agree.</exception>
        public static
        ComplexVector
        operator *(
            ComplexMatrix m1,
            ComplexVector v2)
        {
            return m1.MultiplyRightColumn(v2);
        }

        /// <summary>
        /// Multiply a complex matrix with a real column vector.
        /// </summary>
        /// <exception cref="System.ArgumentException">Matrix inner dimensions must agree.</exception>
        public static
        ComplexVector
        operator *(
            ComplexMatrix m1,
            Vector v2)
        {
            return m1.MultiplyRightColumn(v2);
        }

        #endregion   //Operator Overloading

        #region Various Helpers & Infrastructure

        /// <summary>
        /// Check if size(u) == size(v)
        /// </summary>
        private static
        void
        CheckMatchingMatrixDimensions(
            ComplexMatrix u,
            ComplexMatrix v)
        {
            if(u.RowCount != v.RowCount || u.ColumnCount != v.ColumnCount)
            {
                throw new ArgumentException(Properties.LocalStrings.ArgumentMatrixSameDimensions);
            }
        }

        /// <summary>
        /// Check if size(u) == size(v)
        /// </summary>
        private static
        void
        CheckMatchingMatrixDimensions(
            ComplexMatrix u,
            Matrix v)
        {
            if(u.RowCount != v.RowCount || u.ColumnCount != v.ColumnCount)
            {
                throw new ArgumentException(Properties.LocalStrings.ArgumentMatrixSameDimensions);
            }
        }

        /// <summary>
        /// Returns a deep copy of this instance.
        /// </summary>
        public
        ComplexMatrix
        Clone()
        {
            return new ComplexMatrix(CloneMatrixData(_data));
        }

        /// <summary>
        /// Creates an exact copy of this matrix.
        /// </summary>
        object
        ICloneable.Clone()
        {
            return Clone();
        }

        /// <summary>
        /// Indicates whether <c>obj</c> is equal to this instance.
        /// </summary>
        public override
        bool
        Equals(object obj)
        {
            ComplexMatrix m = obj as ComplexMatrix;
            return ReferenceEquals(m, null) ? false : Equals(m);
        }

        /// <summary>
        /// Indicates whether <c>other</c> is equal to this matrix.
        /// </summary>
        public
        bool
        Equals(ComplexMatrix other)
        {
            if(ReferenceEquals(other, null)
                || _rowCount != other.RowCount
                || _columnCount != other.ColumnCount)
            {
                return false;
            }

            // compare all values
            Complex[][] otherData = other._data;
            for(int i = 0; i < _data.Length; i++)
            {
                Complex[] x = _data[i];
                Complex[] y = otherData[i];
                for(int j = 0; j < x.Length; j++)
                {
                    if(!x[j].Equals(y[j]))
                    {
                        return false;
                    }
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
                int result = _rowCount;
                result = (result * 397) ^ _columnCount;
                result = (result * 397) ^ _data.GetHashCode();
                return result;
            }
        }
        
        /// <summary>
        /// Returns true if two matrices are almost equal, up to the default maximum relative error.
        /// </summary>
        public
        bool
        AlmostEquals(ComplexMatrix other)
        {
            if(ReferenceEquals(other, null)
                || _rowCount != other.RowCount
                || _columnCount != other.ColumnCount)
            {
                return false;
            }

            return Number.AlmostEqualNorm(Norm1(), other.Norm1(), (this - other).Norm1());
        }

        /// <summary>
        /// Returns true if two matrices are almost equal, up to the provided maximum relative error.
        /// </summary>
        public
        bool
        AlmostEquals(
            ComplexMatrix other,
            double maximumRelativeError)
        {
            if(ReferenceEquals(other, null)
                || _rowCount != other.RowCount
                || _columnCount != other.ColumnCount)
            {
                return false;
            }

            return Number.AlmostEqualNorm(Norm1(), other.Norm1(), (this - other).Norm1(), maximumRelativeError);
        }

        /// <summary>
        /// Returns true if two matrices are almost equal, up to the provided maximum relative error.
        /// </summary>
        public static
        bool
        AlmostEqual(
            ComplexMatrix u,
            ComplexMatrix v,
            double maximumRelativeError)
        {
            return Number.AlmostEqual(u, v, maximumRelativeError);
        }

        /// <summary>
        /// Returns true if two matrices are almost equal, up to the default maximum relative error.
        /// </summary>
        public static
        bool
        AlmostEqual(
            ComplexMatrix u,
            ComplexMatrix v)
        {
            return Number.AlmostEqual(u, v);
        }

        /// <summary>
        /// Formats this matrix to a human-readable string
        /// </summary>
        public override
        string
        ToString()
        {
            StringBuilder sb = new StringBuilder();

            for(int i = 0; i < _rowCount; i++)
            {
                if(i == 0)
                {
                    sb.Append("[[");
                }
                else
                {
                    sb.Append(" [");
                }

                for(int j = 0; j < _columnCount; j++)
                {
                    if(j != 0)
                    {
                        sb.Append(',');
                    }

                    sb.Append(_data[i][j].ToString());
                }

                if(i == _rowCount - 1)
                {
                    sb.Append("]]");
                }
                else
                {
                    sb.AppendLine("]");
                }
            }

            return sb.ToString();
        }

        #endregion

        #region OnDemandComputations
        void
        InitOnDemandComputations()
        {
            ////_luDecompositionOnDemand = new OnDemandComputation<LUDecomposition>(ComputeLUDecomposition);
        }

        void
        ResetOnDemandComputations()
        {
            ////_luDecompositionOnDemand.Reset();
        }

        /// <summary>
        /// Reset various internal computations.
        /// Call this method after you made changes directly
        /// on the the internal double[][] data structure.
        /// </summary>
        public
        void
        ResetComputations()
        {
            ResetOnDemandComputations();
        }

        ////LUDecomposition
        ////ComputeLUDecomposition()
        ////{
        ////    return new LUDecomposition(this);
        ////}

        #endregion
    }
}
