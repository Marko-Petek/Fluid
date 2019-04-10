#if false
using System;
using SCG = System.Collections.Generic;

using Fluid.Internals.Development;

namespace Fluid.Internals.Collections
{
    public class SparseMatDouble : SparseMat<double>
    {
        public SparseMatDouble(int width, int height, int capacity = 6) : base(width, height, capacity) {
            _DummyRow = new DummyRowDouble(this, width, 1);
        }

        public SparseMatDouble(SparseMatDouble source) : base(source) {
            _DummyRow = new DummyRowDouble(this,source.Width, 1);
        }

        /// <summary>Creates a SparseMatrix from an array (copies elements from array).</summary><param name="source">Source array.</param>
        public SparseMatDouble(double[][] source) : base(source.Length, source[0].Length, source.Length) {
            int nRows = source.Length;
            _DummyRow = new DummyRowDouble(this, source.Length, 1);

            for(int row = 0; row < nRows; ++row)
                Add(row, CreateSparseRow(source[row]));
        }

        internal override SparseRow<double> CreateSparseRow(int width, int capacity = 6) => new SparseRowDouble(width, capacity);

        internal override SparseRow<double> CreateSparseRow(double[] source) => new SparseRowDouble(source.Length, source.Length);

        /// <summary>Creates an instance of the same (most derived) type as instance on which it is invoked.</summary><param name="width">Width (length of rows) that matrix would have in its explicit form.</param><param name="height">Height (length of columns) that matrix would have in its explicit form.</param><param name="capacity">Initial matrix capacity.</param>
        internal override SparseMat<double> CreateSparseMat(int width, int height, int capacity = 6) => new SparseMatDouble(width, height, capacity);
        
        public new SparseMatDouble SplitAtRow(int virtI) => (SparseMatDouble) base.SplitAtRow(virtI);

        /// <summary>Split matrix on left and right part. Return right part. Element at specified virtual index will be part of right part.</summary><param name="virtJ">Index of element at which to split. This element will be part of right matrix.</param>
        public new SparseMatDouble SplitAtCol(int virtJ) => (SparseMatDouble) base.SplitAtCol(virtJ);

        
        
        public new SparseRowDouble this[int i] => (SparseRowDouble) base[i];
    }
}
#endif