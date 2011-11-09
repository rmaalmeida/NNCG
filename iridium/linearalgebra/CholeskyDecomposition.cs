//-----------------------------------------------------------------------
// <copyright file="CholeskyDecomposition.cs" company="Math.NET Project">
//    Copyright (c) 2004-2009, Joannes Vermorel, Christoph R�egg.
//    All Right Reserved.
// </copyright>
// <author>
//    Joannes Vermorel, http://www.vermorel.com
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
//    The MathWorks
//    NIST
// </contribution>
//-----------------------------------------------------------------------

using System;

namespace MathNet.Numerics.LinearAlgebra
{
    /// <summary>Cholesky Decomposition.</summary>
    /// <remarks>
    /// For a symmetric, positive definite matrix A, the Cholesky decomposition
    /// is an lower triangular matrix L so that A = L*L'.
    /// If the matrix is not symmetric or positive definite, the constructor
    /// returns a partial decomposition and sets an internal flag that may
    /// be queried by the <see cref="CholeskyDecomposition.IsSPD"/> property.
    /// </remarks>
    [Serializable]
    public class CholeskyDecomposition
    {
        /// <summary>Symmetric and positive definite flag.</summary>
        readonly bool _isSymmetricPositiveDefinite;

        /// <summary>Array for internal storage of decomposition.</summary>
        readonly Matrix _l;

        /// <summary>
        /// Initializes a new instance of the CholeskyDecomposition class,
        /// which provides the cholesky algorithm for symmetric and positive definite matrix.
        /// </summary>
        /// <param name="m">Square, symmetric matrix.</param>
        public
        CholeskyDecomposition(Matrix m)
        {
            if(m.RowCount != m.ColumnCount)
            {
                throw new InvalidOperationException(Properties.LocalStrings.ArgumentMatrixSquare);
            }

            double[][] a = m;
            double[][] l = Matrix.CreateMatrixData(m.RowCount, m.RowCount);

            _isSymmetricPositiveDefinite = true; // ensure square

            for(int i = 0; i < l.Length; i++)
            {
                double diagonal = 0.0;
                for(int j = 0; j < i; j++)
                {
                    double sum = a[i][j];
                    for(int k = 0; k < j; k++)
                    {
                        sum -= l[j][k] * l[i][k];
                    }

                    l[i][j] = sum /= l[j][j];
                    diagonal += sum * sum;

                    _isSymmetricPositiveDefinite &= (a[j][i] == a[i][j]); // ensure symmetry
                }

                diagonal = a[i][i] - diagonal;
                l[i][i] = Math.Sqrt(Math.Max(diagonal, 0.0));

                _isSymmetricPositiveDefinite &= (diagonal > 0.0); // ensure positive definite

                // zero out resulting upper triangle.
                for(int j = i + 1; j < l.Length; j++)
                {
                    l[i][j] = 0.0;
                }
            }

            _l = new Matrix(l);
        }

        /// <summary>Is the matrix symmetric and positive definite?</summary>
        /// <returns><c>true</c> if A is symmetric and positive definite.</returns>
        public bool IsSPD
        {
            // TODO: NUnit test
            get { return _isSymmetricPositiveDefinite; }
        }

        /// <summary>Return triangular factor.</summary>
        /// <returns>L</returns>
        [Obsolete("Use the TriangularFactor property instead")]
        public
        Matrix
        GetL()
        {
            return _l;
        }

        /// <summary>
        /// Decomposition Triangular Factor Matrix (L).
        /// </summary>
        public Matrix TriangularFactor
        {
            get { return _l; }
        }

        /// <summary>Solve A*x = b</summary>
        /// <param name="b">A Vector with a dimension as high as the number of rows of A.</param>
        /// <returns>x so that L*L'*x = b</returns>
        /// <exception cref="System.ArgumentException">Matrix row dimensions must agree.</exception>
        /// <exception cref="System.InvalidOperationException">Matrix is not symmetric positive definite.</exception>
        public
        Vector
        Solve(Vector b)
        {
            if(b.Length != _l.RowCount)
            {
                throw new ArgumentException(Properties.LocalStrings.ArgumentMatrixSameRowDimension, "b");
            }

            if(!_isSymmetricPositiveDefinite)
            {
                throw new InvalidOperationException(Properties.LocalStrings.ArgumentMatrixSymmetricPositiveDefinite);
            }

            double[][] l = _l;
            double[] bb = b;
            double[] x = new double[b.Length];

            // Solve L*y = b
            for(int i = 0; i < x.Length; i++)
            {
                double sum = bb[i];
                for(int k = i - 1; k >= 0; k--)
                {
                    sum -= l[i][k] * x[k];
                }

                x[i] = sum / l[i][i];
            }

            // Solve L'*x = y
            for(int i = x.Length - 1; i >= 0; i--)
            {
                double sum = x[i];
                for(int k = i + 1; k < x.Length; k++)
                {
                    sum -= l[k][i] * x[k];
                }

                x[i] = sum / l[i][i];
            }

            return new Vector(x);
        }

        /// <summary>Solve A*X = B</summary>
        /// <param name="b">A Matrix with as many rows as A and any number of columns.</param>
        /// <returns>X so that L*L'*X = B</returns>
        /// <exception cref="System.ArgumentException">Matrix row dimensions must agree.</exception>
        /// <exception cref="System.InvalidOperationException">Matrix is not symmetric positive definite.</exception>
        public
        Matrix
        Solve(Matrix b)
        {
            if(b.RowCount != _l.RowCount)
            {
                throw new ArgumentException(Properties.LocalStrings.ArgumentMatrixSameRowDimension, "B");
            }

            if(!_isSymmetricPositiveDefinite)
            {
                throw new InvalidOperationException(Properties.LocalStrings.ArgumentMatrixSymmetricPositiveDefinite);
            }

            int nx = b.ColumnCount;
            double[][] l = _l;
            double[][] bb = b;
            double[][] x = Matrix.CreateMatrixData(l.Length, nx);

            // Solve L*Y = B
            for(int i = 0; i < x.Length; i++)
            {
                for(int j = 0; j < nx; j++)
                {
                    double sum = bb[i][j];

                    for(int k = i - 1; k >= 0; k--)
                    {
                        sum -= l[i][k] * x[k][j];
                    }

                    x[i][j] = sum / l[i][i];
                }
            }

            // Solve L'*x = y
            for(int i = x.Length - 1; i >= 0; i--)
            {
                for(int j = 0; j < nx; j++)
                {
                    double sum = x[i][j];

                    for(int k = i + 1; k < x.Length; k++)
                    {
                        sum -= l[k][i] * x[k][j];
                    }

                    x[i][j] = sum / l[i][i];
                }
            }

            return new Matrix(x);
        }
    }
}
