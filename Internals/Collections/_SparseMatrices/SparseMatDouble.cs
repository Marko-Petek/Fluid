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

        /// <summary>Creates a SparseMat that is a sum of two operand SparseMats.</summary><param name="left">Left operand.</param><param name="right">Right operand.</param>
        public static SparseMatDouble operator + (SparseMatDouble left, SparseMatDouble right) {
            
            Assert.AreEqual(left.Width, right.Width);             // Check that width and height of operands match.
            Assert.AreEqual(left.Height, right.Height);

            SparseMatDouble rightMat = new SparseMatDouble(right);                     // Copy right operand. Result will appear here. Upcast to dictionary so that Dictionary's indexer is used.
            SCG.Dictionary<int,double> leftRowDict;
            double newRightElm;

            foreach(var leftRow in left) {
                leftRowDict = (SCG.Dictionary<int,double>) leftRow.Value;

                if(rightMat.TryGetValue(leftRow.Key, out var rightRow)) {      // Right row counterpart exists.

                    foreach(var leftCol in leftRow.Value) {
                        newRightElm = rightRow[leftCol.Key] + leftCol.Value;

                        if(newRightElm != 0.0)
                            rightRow[leftCol.Key] = newRightElm;
                        else                                      // temp == 0 && resultRow[kvPair.Key] != 0
                            rightRow.Remove(leftCol.Key);
                    }
                }
                else {                                                          // Right row counterpart does not exist (zeros).
                    rightMat.Add(leftRow.Key, new SparseRowDouble(leftRow.Value));
                }
            }
            return rightMat;
        }

        public static SparseRowDouble operator * (SparseMatDouble mat, SparseRowDouble row) {
            Assert.AreEqual(mat.Width, row.Width);                                  // Check that matrix and row can be multiplied.

            // 1) Go through each row in left matrix. Rows that do not exist, create no entries in result row.
            // 2) Move over each element in row i, check its virtual index, then search for an element with
            //      matching virtual index in right row.
            // 3) Add all such contributions to element with virtual index i in right row.
            // 4) Return result row.

            int matrixRowCount = mat.Count;                                         // Number of occupied (non-zero) rows.
            var resultRow = new SparseRowDouble(mat.Height, matrixRowCount);
            double temp;

            foreach(var rowPair in mat) {
                temp = 0.0;

                foreach(var colPair in rowPair.Value) {
                    if(row.TryGetValue(colPair.Key, out double rowElm))
                        temp += colPair.Value * rowElm;
                }
                resultRow[rowPair.Key] = temp;
            }
            return resultRow;
        }

        public static SparseRowDouble operator * (SparseRowDouble row, SparseMatDouble mat) {
            Assert.AreEqual(mat.Width, row.Width);                                  // Check that matrix and row can be multiplied.
            var resultRow = new SparseRowDouble(mat.Height, mat.Width);

            foreach(var matRow in mat) {

                foreach(var matCol in matRow.Value) {

                    if(row.TryGetValue(matCol.Key, out var leftRowElm))
                        resultRow[matRow.Key] += leftRowElm * matCol.Value;
                }
            }
            return resultRow;
        }
        
        public new SparseRowDouble this[int i] => (SparseRowDouble) base[i];
    }
}