using System;
using System.Linq;
using SCG = System.Collections.Generic;

namespace Fluid.Internals.Collections
{
    public abstract class SparseMat<T> : SCG.Dictionary<int,SparseRow<T>>
    {
        /// <summary>Width (length of rows) that matrix would have in its explicit form.</summary>
        protected int _Width;
        /// <summary>Height (length of columns) that matrix would have in its explicit form.</summary>
        protected int _Height;
        /// <summary>Used by indexer when fetching a row that does not actually exist. If a setter of that row then decides to add an element, row is copied to matrix.</summary>
        DummyRow<T> _DummyRow;


        /// <summary>Width (length of rows) that matrix would have in its explicit form.</summary>
        public int Width => _Width;
        /// <summary>Height (length of columns) that matrix would have in its explicit form.</summary>
        public int Height => _Height;
        /// <summary>Used by indexer when fetching a row that does not actually exist. If a setter of that row then decides to add an element, row is copied to matrix.</summary>
        internal DummyRow<T> DummyRow => _DummyRow;

        /// <summary>Does not assign Width or Height. User of this constructor must do it manually.</summary>
        protected SparseMat() {}

        /// <summary>Create a SparseMatrix with given width, height and initial row capacity.</summary><param name="width">Width (length of rows) that matrix would have in its explicit form.</param><param name="height">Height (length of columns) that matrix would have in its explicit form.</param><param name="capacity">Initial row capacity.</param>
        public SparseMat(int width, int height, int capacity = 6) : base(capacity) {
            _Width = width;
            _Height = height;
            _DummyRow = new DummyRow<T>(this, width);
        }

        /// <summary>Create a copy of specified SparseMatrix.</summary><param name="source">Source SparseMatrix to copy.</param>
        public SparseMat(SparseMat<T> source) : base(source) {
            _Width = source.Width;
            _Height = source.Height;
            _DummyRow = new DummyRow<T>(this, source.Width);
        }

        /// <summary>Creates an instance of the same (most derived) type as instance on which it is invoked.</summary><param name="width">Width (length of rows) that matrix would have in its explicit form.</param><param name="capacity">Initial row capacity.</param>
        internal abstract SparseRow<T> CreateSparseRow(int width, int capacity = 6);

        internal abstract SparseRow<T> CreateSparseRow(T[] source);

        /// <summary>Creates an instance of the same (most derived) type as instance on which it is invoked.</summary><param name="width">Width (length of rows) that matrix would have in its explicit form.</param><param name="height">Height (length of columns) that matrix would have in its explicit form.</param><param name="capacity">Initial matrix capacity.</param>
        internal abstract SparseMat<T> CreateSparseMat(int width, int height, int capacity = 6);

        /// <summary>Split matrix on left and right part. Return right part. Element at specified virtual index will be part of right part.</summary><param name="virtJ">Index of element at which to split. This element will be part of right matrix.</param>
        protected SparseMat<T> SplitAtCol(int virtJ) {
            int remWidth = Width - virtJ;
            var removedRightPart = CreateSparseMat(remWidth, Height);
            _Width = virtJ;                                                 // Adjust width of this Matrix.

            foreach(var rowPair in this) {                                  // Split each SparseRow separately.
                var removedCols = rowPair.Value.SplitAt(virtJ);
                removedRightPart.Add(rowPair.Key, removedCols);
            }
            return removedRightPart;
        }

        /// <summary>Split matrix on upper and lower part. Return lower part. Element at specified virtual index will be part of lower part.</summary><param name="col">Index of element at which to split. This element will be part of lower matrix.</param>
        protected SparseMat<T> SplitAtRow(int virtI) {
            int remWidth = Width;
            int removedHeight = _Height - virtI;
            var removedMatrix = CreateSparseMat(Width, removedHeight);
            var removedRows = this.Where(kvPair => kvPair.Key >= virtI);
            
            foreach(var row in removedRows) {
                removedMatrix.Add(row.Key, row.Value);
                Remove(row.Key);
            }
            return removedMatrix;
        }

        /// <summary>Swap rows with specified virtual indices. Correctly handles cases with non-existent rows.</summary><param name="virtI">Virtual index of first row to swap.</param><param name="virtJ">Virtual index of second row to swap.</param>
        public void SwapRows(int virtI, int virtJ) {

            if(TryGetValue(virtI, out var row1)) {                          // Row 1 exists.

                if(TryGetValue(virtJ, out var row2)) {                          // Row 2 exists.
                    base[virtI] = row2;
                    base[virtJ] = row1;
                }
                else {                                                          // Row 2 does not exist.
                    Remove(virtI);
                    base[virtJ] = row1;
                }
            }
            else {                                                          // Row 1 does not exist.
                if(TryGetValue(virtJ, out var row2)) {                          // Row 2 exists.
                    base[virtI] = row2;
                    Remove(virtJ);
                }
            }
        }

        /// <summary>Swap columns with specified virtual indices. Correctly handles cases with non-existent rows.</summary><param name="virtI">Virtual index of first column to swap.</param><param name="virtJ">Virtual index of second column to swap.</param>
        public void SwapCols(int virtI, int virtJ) {

            foreach(var row in this) {
                row.Value.SwapElms(virtI, virtJ);
            }
        }

        public void ApplyColSwaps(SparseMatInt swapMatrix) {
            foreach(var rowPair in swapMatrix)
                foreach(var colPair in rowPair.Value)
                    SwapCols(rowPair.Key, colPair.Key);
        }

        public new SparseRow<T> this[int i] {
            get {
                if(TryGetValue(i, out SparseRow<T> result))            // Try to fetch value at index i.
                    return result;
                else
                    return DummyRow;
            }
        }
    }
}