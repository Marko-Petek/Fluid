using System;

namespace Fluid.Internals.Collections
{
    public class SparseMatInt : SparseMat<int>
    {
        public SparseMatInt(int width, int height, int capacity = 6) : base(width, height, capacity) {
            _DummyRow = new DummyRowInt(this, width, 1);
        }

        public SparseMatInt(SparseMatInt source) : base(source) {
            _DummyRow = new DummyRowInt(this, source.Width, 1);
        }

        /// <summary>Creates a SparseMatrix from an array (copies elements from array).</summary><param name="source">Source array.</param>
        public SparseMatInt(int[][] source) : base(source.Length, source[0].Length, source.Length) {
            int nRows = source.Length;

            for(int row = 0; row < nRows; ++row)
                Add(row, CreateSparseRow(source[row]));

            _DummyRow = new DummyRowInt(this, source.Length, 1);
        }

        internal override SparseRow<int> CreateSparseRow(int width, int capacity = 6) => new SparseRowInt(width, capacity);

        internal override SparseRow<int> CreateSparseRow(int[] source) => new SparseRowInt(source.Length, source.Length);

        /// <summary>Creates an instance of the same (most derived) type as instance on which it is invoked.</summary><param name="width">Width (length of rows) that matrix would have in its explicit form.</param><param name="height">Height (length of columns) that matrix would have in its explicit form.</param><param name="capacity">Initial matrix capacity.</param>
        internal override SparseMat<int> CreateSparseMat(int width, int height, int capacity = 6) => new SparseMatInt(width, height, capacity);
        
        public new SparseMatInt SplitAtRow(int virtI) => (SparseMatInt) base.SplitAtRow(virtI);
        
        public new SparseRowInt this[int i] => (SparseRowInt) base[i];
    }
}