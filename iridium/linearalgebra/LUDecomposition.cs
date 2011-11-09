//-----------------------------------------------------------------------
// <copyright file="LUDecomposition.cs" company="Math.NET Project">
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
    /// <summary>
    /// LU Decomposition.
    /// </summary>
    /// <remarks>
    /// For an m-by-n matrix A with m >= n, the LU decomposition is an m-by-n
    /// unit lower triangular matrix L, an n-by-n upper triangular matrix U,
    /// and a permutation vector pivot of length m so that A(_pivot,:) = L*U.
    /// <c> If m &lt; n, then L is m-by-m and U is m-by-n. </c>
    /// The LU decomposition with pivoting always exists, even if the matrix is
    /// singular, so the constructor will never fail.  The primary use of the
    /// LU decomposition is in the solution of square systems of simultaneous
    /// linear equations.  This will fail if IsNonSingular() returns false.
    /// </remarks>
    [Serializable]
    public class LUDecomposition
    {
        readonly int _rowCount;
        readonly int _columnCount;

        /// <summary>
        /// Array for internal storage of decomposition.
        /// </summary>
        readonly double[][] _lu;

        /// <summary>
        /// Pivot sign.
        /// </summary>
        readonly int _pivotSign;

        /// <summary>
        /// Internal storage of pivot vector.
        /// </summary>
        readonly int[] _pivot;

        OnDemandComputation<bool> _isNonSingularOnDemand;
        OnDemandComputation<Matrix> _lowerTriangularFactorOnDemand;
        OnDemandComputation<Matrix> _upperTriangularFactorOnDemand;
        OnDemandComputation<int[]> _pivotOnDemand;
        OnDemandComputation<Vector> _pivotVectorOnDemand;
        OnDemandComputation<Matrix> _permutationMatrixOnDemand;
        OnDemandComputation<double> _determinantOnDemand;

        /// <summary>
        /// Initializes a new instance of the LUDecomposition class.
        /// </summary>
        /// <param name="a">Rectangular matrix</param>
        /// <returns>Structure to access L, U and pivot.</returns>
        public
        LUDecomposition(Matrix a)
        {
            /* TODO: it is usually considered as a poor practice to execute algorithms within a constructor */

            /* Use a "left-looking", dot-product, Crout/Doolittle algorithm. */

            _lu = a.Clone();
            _rowCount = a.RowCount;
            _columnCount = a.ColumnCount;

            _pivot = new int[_rowCount];
            for(int i = 0; i < _rowCount; i++)
            {
                _pivot[i] = i;
            }

            _pivotSign = 1;

            ////double[] LUrowi;
            double[] LUcolj = new double[_rowCount];

            /* Outer loop */

            for(int j = 0; j < _columnCount; j++)
            {
                /* Make a copy of the j-th column to localize references */

                for(int i = 0; i < LUcolj.Length; i++)
                {
                    LUcolj[i] = _lu[i][j];
                }

                /* Apply previous transformations */

                for(int i = 0; i < LUcolj.Length; i++)
                {
                    ////LUrowi = LU[i];

                    /* Most of the time is spent in the following dot product */

                    int kmax = Math.Min(i, j);
                    double s = 0.0;
                    for(int k = 0; k < kmax; k++)
                    {
                        s += _lu[i][k] * LUcolj[k];
                    }

                    _lu[i][j] = LUcolj[i] -= s;
                }

                /* Find pivot and exchange if necessary */

                int p = j;

                for(int i = j + 1; i < LUcolj.Length; i++)
                {
                    if(Math.Abs(LUcolj[i]) > Math.Abs(LUcolj[p]))
                    {
                        p = i;
                    }
                }

                if(p != j)
                {
                    for(int k = 0; k < _columnCount; k++)
                    {
                        double t = _lu[p][k];
                        _lu[p][k] = _lu[j][k];
                        _lu[j][k] = t;
                    }

                    int k2 = _pivot[p];
                    _pivot[p] = _pivot[j];
                    _pivot[j] = k2;

                    _pivotSign = -_pivotSign;
                }

                /* Compute multipliers */

                if((j < _rowCount) && (_lu[j][j] != 0.0))
                {
                    for(int i = j + 1; i < _rowCount; i++)
                    {
                        _lu[i][j] /= _lu[j][j];
                    }
                }
            }

            InitOnDemandComputations();
        }

        /// <summary>
        /// Indicates whether the matrix is nonsingular.
        /// </summary>
        /// <returns><c>true</c> if U, and hence A, is nonsingular.</returns>
        public bool IsNonSingular
        {
            get { return _isNonSingularOnDemand.Compute(); }
        }

        /// <summary>
        /// Returns the lower triangular factor.
        /// </summary>
        public Matrix L
        {
            get { return _lowerTriangularFactorOnDemand.Compute(); }
        }

        /// <summary>
        /// Returns the upper triangular factor.
        /// </summary>
        public Matrix U
        {
            get { return _upperTriangularFactorOnDemand.Compute(); }
        }

        /// <summary>
        /// Returns the integer pivot permutation vector.
        /// </summary>
        public int[] Pivot
        {
            get { return _pivotOnDemand.Compute(); }
        }

        /// <summary>
        /// Returns pivot permutation vector.
        /// </summary>
        public Vector PivotVector
        {
            get { return _pivotVectorOnDemand.Compute(); }
        }

        /// <summary>
        /// Returns the permutation matrix P, such that L*U = P*X.
        /// </summary>
        public Matrix PermutationMatrix
        {
            get { return _permutationMatrixOnDemand.Compute(); }
        }

        /// <summary>
        /// Returns pivot permutation vector as a one-dimensional double array.
        /// </summary>
        [Obsolete("Use the PivotVector property instead")]
        public double[] DoublePivot
        {
            get { return _pivotVectorOnDemand.Compute(); }
        }

        /// <summary>
        /// Determinant
        /// </summary>
        /// <returns>det(A)</returns>
        /// <exception cref="System.ArgumentException">Matrix must be square</exception>
        public
        double
        Determinant()
        {
            // TODO (ruegg, 2008-03-11): Change to property
            return _determinantOnDemand.Compute();
        }

        /// <summary>
        /// Solve A*X = B
        /// </summary>
        /// <param name="b">A Matrix with as many rows as A and any number of columns.</param>
        /// <returns>X so that L*U*X = B(pivot,:)</returns>
        /// <exception cref="System.ArgumentException">Matrix row dimensions must agree.</exception>
        /// <exception cref="System.SystemException">Matrix is singular.</exception>
        public
        Matrix
        Solve(Matrix b)
        {
            if(b.RowCount != _rowCount)
            {
                throw new ArgumentException(Properties.LocalStrings.ArgumentMatrixSameRowDimension, "B");
            }

            if(!IsNonSingular)
            {
                throw new InvalidOperationException(Properties.LocalStrings.ArgumentMatrixNotSingular);
            }

            // Copy right hand side with pivoting
            int nx = b.ColumnCount;
            Matrix xmat = b.GetMatrix(_pivot, 0, nx - 1);
            double[][] x = xmat;

            // Solve L*Y = B(_pivot,:)
            for(int k = 0; k < _columnCount; k++)
            {
                for(int i = k + 1; i < _columnCount; i++)
                {
                    for(int j = 0; j < nx; j++)
                    {
                        x[i][j] -= x[k][j] * _lu[i][k];
                    }
                }
            }

            // Solve U*x = Y;
            for(int k = _columnCount - 1; k >= 0; k--)
            {
                for(int j = 0; j < nx; j++)
                {
                    x[k][j] /= _lu[k][k];
                }

                for(int i = 0; i < k; i++)
                {
                    for(int j = 0; j < nx; j++)
                    {
                        x[i][j] -= x[k][j] * _lu[i][k];
                    }
                }
            }

            return xmat;
        }

        void
        InitOnDemandComputations()
        {
            _isNonSingularOnDemand = new OnDemandComputation<bool>(ComputeIsNonSingular);
            _lowerTriangularFactorOnDemand = new OnDemandComputation<Matrix>(ComputeLowerTriangularFactor);
            _upperTriangularFactorOnDemand = new OnDemandComputation<Matrix>(ComputeUpperTriangularFactor);
            _pivotOnDemand = new OnDemandComputation<int[]>(ComputePivot);
            _pivotVectorOnDemand = new OnDemandComputation<Vector>(ComputePivotVector);
            _permutationMatrixOnDemand = new OnDemandComputation<Matrix>(ComputePermutationMatrix);
            _determinantOnDemand = new OnDemandComputation<double>(ComputeDeterminant);
        }

        bool
        ComputeIsNonSingular()
        {
            for(int j = 0; j < _columnCount; j++)
            {
                if(_lu[j][j] == 0.0)
                {
                    return false;
                }
            }

            return true;
        }

        Matrix
        ComputeLowerTriangularFactor()
        {
            double[][] l = Matrix.CreateMatrixData(_rowCount, _columnCount);
            for(int i = 0; i < l.Length; i++)
            {
                for(int j = 0; j < _columnCount; j++)
                {
                    if(i > j)
                    {
                        l[i][j] = _lu[i][j];
                    }
                    else if(i == j)
                    {
                        l[i][j] = 1.0;
                    }
                    else
                    {
                        l[i][j] = 0.0;
                    }
                }
            }

            return new Matrix(l);
        }

        Matrix
        ComputeUpperTriangularFactor()
        {
            double[][] u = Matrix.CreateMatrixData(_columnCount, _columnCount);
            for(int i = 0; i < _columnCount; i++)
            {
                for(int j = 0; j < _columnCount; j++)
                {
                    u[i][j] = (i <= j) ? _lu[i][j] : 0.0;
                }
            }

            return new Matrix(u);
        }

        int[]
        ComputePivot()
        {
            int[] p = new int[_rowCount];
            for(int i = 0; i < _rowCount; i++)
            {
                p[i] = _pivot[i];
            }

            return p;
        }

        Vector
        ComputePivotVector()
        {
            double[] vals = new double[_rowCount];
            for(int i = 0; i < _rowCount; i++)
            {
                vals[i] = _pivot[i];
            }

            return new Vector(vals);
        }

        Matrix
        ComputePermutationMatrix()
        {
            int[] pivot = Pivot;
            double[][] perm = Matrix.CreateMatrixData(pivot.Length, pivot.Length);
            for(int i = 0; i < pivot.Length; i++)
            {
                perm[pivot[i]][i] = 1.0;
            }

            return new Matrix(perm);
        }

        double
        ComputeDeterminant()
        {
            if(_rowCount != _columnCount)
            {
                throw new ArgumentException(Properties.LocalStrings.ArgumentMatrixSquare);
            }

            double d = _pivotSign;
            for(int j = 0; j < _columnCount; j++)
            {
                d *= _lu[j][j];
            }

            return d;
        }
    }
}
