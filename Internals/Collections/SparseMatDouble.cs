using System;

using Fluid.Internals.Development;

namespace Fluid.Internals.Collections
{
    public class SparseMatDouble : SparseMat<double>
    {
        public SparseMatDouble(int width, int height, int capacity = 6) : base(width, height, capacity) {
        }

        public SparseMatDouble(SparseMatDouble source) : base(source) {
        }

        /// <summary>Creates a SparseMatrix from an array (copies elements from array).</summary><param name="source">Source array.</param>
        public SparseMatDouble(double[][] source) : base(source.Length, source[0].Length, source.Length) {
            int nRows = source.Length;

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

        /// <summary>Creates a SparseMatrix that is a sum of two operand SparseMatrices.</summary><param name="left">Left operand.</param><param name="right">Right operand.</param>
        public static SparseMat<T> operator + (SparseMat<T> left, SparseMat<T> right) {
            
            Assert.AreEqual(left.Width, right.Width);             // Check that width and height of operands match.
            Assert.AreEqual(left.Height, right.Height);

            var resultRow = (SCG.Dictionary<int,double>) new SparseRowDouble(right);                     // Copy right operand. Result will appear here. Upcast to dictionary so that Dictionary's indexer is used.
            double temp;

            foreach(var kvPair in left) {

                resultRow.TryGetValue(kvPair.Key, out double val);       // Then add to existing value.
                temp = kvPair.Value + val;

                if(temp != 0.0)
                    resultRow[kvPair.Key] = temp;
                else if(val != 0)                                       // temp == 0 && resultRow[kvPair.Key] != 0
                    resultRow.Remove(kvPair.Key);
            }
            return (SparseRowDouble)resultRow;


            SparseMat<T>[] operands;
            int[] rowIndex = new int[] {0, 0};                                                              // Current true row indices of operands.
            int[] explicitRowIndex;
            int[] count;

            if(left._E[0].GetExplicitIndex() <= right._E[0].GetExplicitIndex()) {                                   // Pack operands in correct order for manipulation. One with smaller first row matrix index comes first.
                operands = new SparseMat<T>[2] {left, right};
                explicitRowIndex = new int[] {left._E[0].GetExplicitIndex(), right._E[0].GetExplicitIndex()};
                count = new int[] {left._Count, right._Count};
            }
            else {
                operands = new SparseMat<T>[2] {right, left};
                explicitRowIndex = new int[] {right._E[0].GetExplicitIndex(), left._E[0].GetExplicitIndex()};
                count = new int[] {right._Count, left._Count};
            }
            var resultMatrix = new SparseMat<T>(left._width, left._height, (count[0] > count[1]) ? count[0] : count[1]);    // Result matrix starts with capacity of larger operand.
			int i = 0;
			int j = 1;
		

            while(true) {      											// Exchange processed operand in each new iteration.
                
                while(explicitRowIndex[i] <= explicitRowIndex[j]) {   	// Set index where next iteration will kick off. At start of this loop it must hold: rowIndex[j] = rowIndex[i].
                    
                    if(rowIndex[i] < count[i]) {    					// Check that we haven't reached the end of any of internal arrays.
                        
                        if(rowIndex[j] < count[j]) {

                            if(explicitRowIndex[i] < explicitRowIndex[j]) {
                                resultMatrix.Add(new SparseMatRow<T>(operands[i]._E[rowIndex[i]]));
                            }
                            else if(explicitRowIndex[i] == explicitRowIndex[j]){
                                var sparseMatrixRow = operands[i]._E[rowIndex[i]] + operands[j]._E[rowIndex[j]];
                                resultMatrix.Add(sparseMatrixRow);
                                ++rowIndex[j];

                                if(rowIndex[j] < count[j]) {
                                    explicitRowIndex[j] = operands[j]._E[rowIndex[j]].GetExplicitIndex();
                                }
                            }
                        }
                        else {              // We have reached end of j-th internal array. Write the rest of i-th elements to result.
                            while(rowIndex[i] < count[i]) {
                                resultMatrix.Add(new SparseMatRow<T>(operands[i]._E[rowIndex[i]]));
                                ++rowIndex[i];
                            }
                            return resultMatrix;
                        }
                        ++rowIndex[i];

                        if(rowIndex[i] < count[i]) {
                            explicitRowIndex[i] = operands[i]._E[rowIndex[i]].GetExplicitIndex();
                        }
                    }
                    else {                  // We have reached end of i-th internal array. Write the rest of j-th elements to result.
                        while(rowIndex[j] < count[j]) {
                            resultMatrix.Add(new SparseMatRow<T>(operands[j]._E[rowIndex[j]]));
                            ++rowIndex[j];
                        }
                        return resultMatrix;
                    }
                }
				i = (i + 1) % 2;                        // Exchange processed operand in each new iteration.
				j = (j + 1) % 2;

			}
            throw new ArithmeticException("Method which sums two SparseMatrices has reached an invalid state.");
        

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
        
        public new SparseRowDouble this[int i] => (SparseRowDouble) base[i];
    }
}