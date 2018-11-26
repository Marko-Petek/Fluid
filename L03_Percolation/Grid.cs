using System;

using Fluid.Internals.Collections;

namespace L03_Percolation
{
    public class Grid
    {
        /// <summary>Array that specifies which elements are conductive.</summary>
        SparseMatrixInt _SparseMatrix;

        public Grid(int nRows, int nCols) {
            _SparseMatrix = new SparseMatrixInt(nCols, nRows, 10_000);
        }
    }
}