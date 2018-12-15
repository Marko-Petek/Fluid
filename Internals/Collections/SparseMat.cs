using System;
using System.Linq;
using SCG = System.Collections.Generic;

namespace Fluid.Internals.Collections
{
    public class SparseMat<T> : SCG.Dictionary<int,SparseRow<T>>
    {
        /// <summary>Width (length of rows) that matrix would have in its explicit form.</summary>
        int _Width;
        /// <summary>Height (length of columns) that matrix would have in its explicit form.</summary>
        int _Height;


        /// <summary>Width (length of rows) that matrix would have in its explicit form.</summary>
        public int Width => _Width;
        /// <summary>Height (length of columns) that matrix would have in its explicit form.</summary>
        public int Height => _Height;


        /// <summary>Create a SparseMatrix with given width, height and initial row capacity.</summary><param name="width">Width (length of rows) that matrix would have in its explicit form.</param><param name="height">Height (length of columns) that matrix would have in its explicit form.</param><param name="capacity">Initial row capacity.</param>
        public SparseMat(int width, int height, int capacity = 6) : base(capacity) {
            _Width = width;
            _Height = height;
        }

        /// <summary>Create a copy of specified SparseMatrix.</summary><param name="source">Source SparseMatrix to copy.</param>
        public SparseMat(SparseMat<T> source) : base(source) {
            _Width = source.Width;
            _Height = source.Height;
        }

        /// <summary>Creates a SparseMatrix from an array (copies elements from array).</summary><param name="source">Source array.</param>
        public SparseMat(T[][] source) : base(source.Length) {
            int nRows = source.Length;

            for(int row = 0; row < nRows; ++row)
                Add(row, SparseRow<T>.CreateFromArray(source[row]));
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

        // TODO: Implement with parameter of type SparseMatInt.
        public void ApplySwaps() {
            throw new NotImplementedException();
        }

        /// <summary>Split matrix on left and right part. Return right part. Element at specified virtual index will be part of right part.</summary><param name="virtJ">Index of element at which to split. This element will be part of right matrix.</param>
        public SparseMat<T> SplitAtCol(int virtJ) {
            int removedWidth = Width - virtJ;
            var removedMatrix = new SparseMat<T>(removedWidth, Height);     // Right part that will be returned.
            _Width = virtJ;                                                 // Adjust width of this Matrix.

            foreach(var rowPair in this) {                                  // Split each SparseRow separately.
                var removedRow = rowPair.Value.SplitAt(virtJ);
                removedMatrix.Add(rowPair.Key, removedRow);
            }
            return removedMatrix;
        }

        /// <summary>Split matrix on upper and lower part. Return lower part. Element at specified virtual index will be part of lower part.</summary><param name="col">Index of element at which to split. This element will be part of lower matrix.</param>
        public SparseMat<T> SplitAtRow(int virtI) {
            //var splitSuccessfull = SetRecentIndexToOrAheadOf(virtI);
            int removedHeight = _Height - virtI;
            var removedMatrix = new SparseMat<T>(Width, removedHeight);
            var removedRows = this.Where(kvPair => kvPair.Key >= virtI);
            
            foreach(var row in removedRows) {
                removedMatrix.Add(row.Key, row.Value);
                Remove(row.Key);
            }
            return removedMatrix;
        }
    }
}