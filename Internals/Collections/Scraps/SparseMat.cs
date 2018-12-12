using System;
using System.Text;
using SCG = System.Collections.Generic;

using Fluid.Internals.Collections;
using static Fluid.Internals.Development.Assert;

namespace Fluid.Internals.Collections
{
    public class SparseMat<T> : EquatableManagedList<SparseMatRow<T>>
    where T : struct, IEquatable<T>
    {
        protected override void AfterElementEntry(int index) {
            _E[index].SetIndex(index);
            _E[index].SetSparseMatrix(this);

            for(int i = index + 1; i < Count; ++i) {
                _E[i].SetIndex(i + 1);
            }
        }
        protected override void BeforeElementExit(int index) {
            _E[index].SetIndex(-1);                       						// Signal that SparseMatrixRow is no longer part of any SparseMatrix.
            _E[index].SetSparseMatrix(null);

            for(int i = index + 1; i < Count; ++i) {
                _E[i].SetIndex(i - 1);
            }
        }

        /// <summary>Width (length of rows) that matrix would have in its explicit form.</summary>
        int                         _width;
        /// <summary>Height (length of columns) that matrix would have in its explicit form.</summary>
        int                         _height;
        /// <summary>Index of most recently fetched (get or set) SparseMatrixRow.</summary>
        int _recentIndex;
        /// <summary>Represents a non-existent row with negative matrix index to convey it's a dummy.</summary>
        SparseMatRow<T>          _dummyMatrixRow;

        #region Accessors
        /// <summary>Width (length of rows) that matrix would have in its explicit form.</summary>
        public int                  GetWidth()              =>  _width;
        /// <summary>Height (length of columns) that matrix would have in its explicit form.</summary>
        public int                  GetHeight()             =>  _height;
        /// <summary>Rows, each with its own index.</summary>
        public SparseMatRow<T>[] GetSparseMatrixRows()   =>  _E;
        /// <summary>Represents a non-existent row with specific matrix index (negative to convey it's a dummy).</summary>
        public SparseMatRow<T>   GetDummyMatrixRow()     =>  _dummyMatrixRow;
        #endregion

        /// <summary>Create a SparseMatrix with given width, height and initial row capacity.</summary><param name="width">Width (length of rows) that matrix would have in its explicit form.</param><param name="height">Height (length of columns) that matrix would have in its explicit form.</param><param name="capacity">Initial row capacity.</param>
        public SparseMat(int width, int height, int capacity) : base(capacity) {
            _width = width;
            _height = height;
            _dummyMatrixRow = new SparseMatRow<T>(_width, -1);             // Empty row.
            _dummyMatrixRow.SetIsDummy(true);
            _dummyMatrixRow.SetSparseMatrix(this);
        }
        /// <summary>Create a SparseMatrix with given width, height and initial row capacity.</summary><param name="width">Width (length of rows) that matrix would have in its explicit form.</param><param name="height">Height (length of columns) that matrix would have in its explicit form.</param>
        public SparseMat(int width, int height) : this(width, height, 10) {
        }

        /// <summary>Create a copy of specified SparseMatrix.</summary><param name="sourceToCopy">Source SparseMatrix to copy.</param>
        public SparseMat(SparseMat<T> sourceToCopy) {
            _width = sourceToCopy._width;
            _height = sourceToCopy._height;
            _Count = sourceToCopy._Count;
            _dummyMatrixRow = new SparseMatRow<T>(sourceToCopy._dummyMatrixRow);
            _dummyMatrixRow.SetSparseMatrix(this);
            _E = new SparseMatRow<T>[_height];
            Array.Copy(sourceToCopy._E, _E, _Count);
            _recentIndex = sourceToCopy._recentIndex;
        }

        /// <summary>Creates a SparseMatrix from an array.</summary><param name="sourceArray">Source array.</param>
        public SparseMat(T[][] sourceArray) {
            int length0 = sourceArray.Length;

            for(int row = 0; row < length0; ++row) {
                int length1 = sourceArray[row].Length;

                for(int col = 0; col < length1; ++col) {
                    this[row][col] = sourceArray[row][col];
                }
            }
        }


        /// <summary>Check whether a row with specified explicit index exists. Returns (true, internal index of row) if it exists and (false, insertion index if we wish to have row with such explicitIndex).</summary><param name="explicitIndex">Explicit index to check.</param>
        public (bool,int) GetRowIndex(int explicitIndex) {
            if(_Count != 0) {
                ValidateIndex(ref _recentIndex);
                int index = _recentIndex;
                int recentExplicitIndex = _E[index].GetExplicitIndex();
                
                if(recentExplicitIndex <= explicitIndex) {              // Move forward until you reach end.
                    while(index < _Count && _E[index].GetExplicitIndex() <= explicitIndex) {           // when second condition false, row with requested explicit index does not exist.
                        
                        if(_E[index].GetExplicitIndex() == explicitIndex) {
                            _recentIndex = index;
                            return (true,index);
                        }
                        ++index;
                    }
                    return (false, --index);
                }
                else {
                    while(index > -1 && _E[index].GetExplicitIndex() >= explicitIndex) {           // when second condition false, row with requested explicit index does not exist.
                        
                        if(_E[index].GetExplicitIndex() == explicitIndex) {
                            _recentIndex = index;
                            return (true,index);
                        }
                        --index;
                    }
                    return (false, ++index);
                }
            }
            return (false, 0);
        }

        /// <summary>Retrieves row with specified explicit index.</summary>
        public override SparseMatRow<T> this[int explicitIndex] {
            get{
                int dummyIndex = 0;

                if(_Count != 0) {
                    ValidateIndex(ref _recentIndex);
                    int index = _recentIndex;
                    int recentExplicitIndex = _E[index].GetExplicitIndex();
                    
                    if(recentExplicitIndex <= explicitIndex) {              // Move forward until you reach end.
                        while(index < _Count && _E[index].GetExplicitIndex() <= explicitIndex) {           // when second condition false, row with requested explicit index does not exist.
                            
                            if(_E[index].GetExplicitIndex() == explicitIndex) {
                                _recentIndex = index;
                                return _E[index];
                            }
                            ++index;
                        }
                        dummyIndex = index;
                        _recentIndex = index;
                    }
                    else {
                        while(index > -1 && _E[index].GetExplicitIndex() >= explicitIndex) {           // when second condition false, row with requested explicit index does not exist.
                            
                            if(_E[index].GetExplicitIndex() == explicitIndex) {
                                _recentIndex = index;
                                return _E[index];
                            }
                            --index;
                        }
                        dummyIndex = index + 1;
                        _recentIndex = dummyIndex;
                    }
                }
                _dummyMatrixRow.SetIndex(dummyIndex);                                // Inform the setter of SparseMatrixRow where in a SparseMatrix a row has to be inserted.
                _dummyMatrixRow.SetExplicitIndex(explicitIndex);
                return _dummyMatrixRow;                                         // Let the setter of SparseRow indexer create a new row if needed.
            }
        }

        /// <summary>Swap rows with specified explicit indices. Correctly handles cases with non-existent rows.</summary><param name="explicitIndex1">Explicit index of first row to swap.</param><param name="explicitIndex2">Explicit index of second row to swap.</param>
        public void SwapRows(int explicitIndex1, int explicitIndex2) {
            (bool index1Exists, int index1) = GetRowIndex(explicitIndex1);
            (bool index2Exists, int index2) = GetRowIndex(explicitIndex2);

            if(index1Exists) {                                          // Row with first explicit index exists.

                if(index2Exists) {                                          // Row with second explicit index exists. Simply swap indices and explicit indices of rows.
                    SparseMatRow<T> temp = _E[index2];
                    _E[index2] = _E[index1];
                    _E[index2].SetIndex(index2);
                    _E[index2].SetExplicitIndex(explicitIndex2);
                    _E[index1] = temp;
                    _E[index1].SetIndex(index1);
                    _E[index1].SetExplicitIndex(explicitIndex1);
                }
                else {                                                      // Row with second explicit index does not exist.
                    _E[index1].SetExplicitIndex(explicitIndex2);
                    Insert(index2, _E[index1]);

                    if(index1 >= index2) {                                  // Insertion has moved our existing element.
                        RemoveAt(index1 + 1);
                    }
                    else {
                        RemoveAt(index1);
                    }
                }                                                            
            }
            else {                                                      // Row with first explicit index does not exist.
                if(index2Exists) {                                         // Row with second explicit index exists.
                    _E[index2].SetExplicitIndex(explicitIndex1);
                    Insert(index1, _E[index2]);

                    if(index2 >= index1) {                                  // Insertion has moved our existing element.
                        RemoveAt(index2 + 1);
                    }
                    else {
                        RemoveAt(index2);
                    }
                }
            }
        }

        /// <summary>Swap two columns. Correctly handles cases with non-existent elements.</summary><param name="explicitIndex1">Explicit index of first column to swap.</param><param name="explicitIndex2">Explicit index of second column to swap.</param>
        public void SwapColumns(int explicitIndex1, int explicitIndex2) {

            for(int i = 0; i < Count; ++i) {
                _E[i].SwapElementsExplicit(explicitIndex1, explicitIndex2);
            }
        }

        public void ApplyColumnSwaps(SparseMat<int> swapMatrix) {
            AreEqual<int>(swapMatrix.GetWidth(), _width, SCG.EqualityComparer<int>.Default);          // Check that swap matrix dimensions are appropriate for this SparseRow.
            AreEqual<int>(swapMatrix.GetHeight(), _width, SCG.EqualityComparer<int>.Default);
            int swapCount = swapMatrix.Count;                                                                   // Actual number of elements (SparseMatrixRows) in swapMatrix.

            for(int i = 0; i < swapCount; ++i) {
                ref var row = ref swapMatrix.E(i);                            // In-memory row i.
                int element1 = row.GetExplicitIndex();                          // Find its explicit row index. That is then index of first element to be swapped.
                int element2 = row.E(0).ImagIndex();                   // Each row is expected to have one element. That is index of second element to be swapped.
                SwapColumns(element1, element2);
            }
        }

        /// <summary>Creates a SparseMatrix that is a sum of two operand SparseMatrices.</summary><param name="left">Left operand.</param><param name="right">Right operand.</param>
        public static SparseMat<T> operator + (SparseMat<T> left, SparseMat<T> right) {
            
            AreEqual(left._width, right._width);             // Check that width and height of operands match.
            AreEqual(left._height, right._height);
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

        public static SparseRow<T> operator * (SparseMat<T> matrix, SparseRow<T> vector) {
            AreEqual(matrix._width, vector.Width);                 // Check that matrix and row can be multiplied.

            // 1) Go through each row in left matrix. Rows that do not exist, create no entries in result row.
            // 2) Move over each element in row i, check its explicit index, then search for an element with
            //      matching explicit ndex in right row.
            // 3) Add all such contributions to element with explicit index i in right row.
            // 4) Return result row.

            int matrixRowCount = matrix.Count;                                      // Number of occupied (non-zero) rows.
            var resultRow = new SparseRow<T>(matrix.GetHeight(), matrixRowCount);
            int matrixColCount;                                                  // Number of occupied columns in a matrix row. Different for each row.
            SparseMatRow<T> matrixRow = matrix._E[0];
            T resultValue;
            int explicitRowIndex;
            int explicitColIndex;

            for(int i = 0; i < matrixRowCount; ++i) {            // Loop over rows.
                matrixRow = matrix._E[i];
                matrixColCount = matrixRow.Count;
                explicitRowIndex = matrixRow.GetExplicitIndex();
                resultValue = default(T);

                for(int j = 0; j < matrixColCount; ++j) {
                    explicitColIndex = matrixRow.E(j).ImagIndex();
                    resultValue += (dynamic) matrixRow.E(j).Value * vector[explicitColIndex];                      // No worries compiler, types are ok.
                }
                resultRow[explicitRowIndex] = resultValue;
            }
            return resultRow;
        }

        public static SparseRow<T> operator * (SparseRow<T> vector, SparseMat<T> matrix) {
            AreEqual(vector.Width, matrix._height);

            int vectorColCount = vector.Count;
            int matrixRowCount = matrix._Count;
            var resultRow = new SparseRow<T>(matrix._width);        // Result row has width of input matrix.
            ref SparseMatRow<T> matrixRow = ref matrix._E[0];
            int matrixColCount;                                             // Different for each row.
            int explicitRowIndex;
            int explicitColIndex;

            for(int i = 0; i < matrixRowCount; ++i) {               // Over each row of matrix.
                matrixRow = matrix._E[i];
                explicitRowIndex = matrixRow.GetExplicitIndex();
                matrixColCount = matrixRow.Count;
                
                for(int j = 0; j < matrixColCount; ++j) {
                    explicitColIndex = matrixRow.E(j).ImagIndex();
                    resultRow[explicitColIndex] += (dynamic) matrixRow.E(j).Value * vector[explicitRowIndex];
                }
            }
            return resultRow;
        }

        /// <summary>Split matrix on left and right part. Return right part. Element at specified index will be part of right part.</summary><param name="col">Index of element at which to split. This element will be part of right matrix.</param>
        public SparseMat<T> SplitAtColumn(int explicitCol) {
            int removedWidth = _width - explicitCol;
            var removedMatrix = new SparseMat<T>(removedWidth, _height);
            _width = explicitCol;                                               // Adjust width of this Matrix.

            for(int i = 0; i < _Count; ++i) {                                   // Split each SparseRow separately.
                var removedRow = _E[i].SplitAt(explicitCol);
                removedMatrix.Add(removedRow);
            }
            return removedMatrix;
        }

        /// <summary>Split matrix on upper and lower part. Return lower part. Element at specified index will be part of lower part.</summary><param name="col">Index of element at which to split. This element will be part of lower matrix.</param>
        public SparseMat<T> SplitAtRow(int explicitRow) {
            var splitSuccessfull = SetRecentIndexToOrAheadOf(explicitRow);
            int removedHeight = _height - explicitRow;
            SparseMat<T> removedMatrix;

            if(splitSuccessfull) {                                          // Some elements can be trimmed.
                removedMatrix = RemoveRange(_recentIndex, _Count - 1);
                TrimExcessSpace();
            }
            else {                                                          // No elements to trim.
                _height = explicitRow;
                removedMatrix = new SparseMat<T>(_width, removedHeight, 6);
            }
            return removedMatrix;
        }

        /// <summary>Removes specified range of rows from SparseMatrix and returns it. Correctly adjusts width. Range is specified in terms of internal indices, not explicit ones.</summary><param name="j">Inclusive starting index.</param><param name="k">Inclusive ending index.</param>
        new public SparseMat<T> RemoveRange(int j, int k) {
            int removedCount = k - j + 1;
            var removed = new SparseMat<T>(_height, removedCount);

            for(int i = 0; i < removedCount; ++i) {                         // Construct Matrix that we will return.
                BeforeElementExit(i);
                removed[i] = _E[j + i];
            }
            for(int i = k + 1; i < _Count; ++i) {                           // Refill hole. Shift elements remaining on right side of hole (removed range) to right.
                BeforeElementExit(i);
                _E[i - removedCount] = _E[i];
                AfterElementEntry(i - removedCount);
            }
            _Count = _Count - removedCount;                                 // Changing count, no need to zero elements at end.
            _height = _E[_Count].GetExplicitIndex() + 1;
            return removed;
        }

        /// <summary>Sets recent index to element with given explicit index. If element with specified explicit index does not exist but elements ahead exist, it sets recent index to first element ahead of specified explicit index. If specified element does not exist and end is reached, it returns false.</summary><param name="explicitIndex">Explicit index of desired element.</param>
        protected bool SetRecentIndexToOrAheadOf(int explicitIndex) {
            IndexInRange(explicitIndex, 0, _width);
                
            if(_Count != 0) {
                ValidateIndex(ref _recentIndex);
                int index = _recentIndex;
                int recentExplicitIndex = E(index).GetExplicitIndex();

                if(recentExplicitIndex <= explicitIndex) {              // Move forward until you reach end.
                    while(index < _Count && E(index).GetExplicitIndex() <= explicitIndex) {

                        if(E(index).GetExplicitIndex() == explicitIndex) {                        // Try to find an existing entry to return.
                            _recentIndex = index;
                            return true;
                        }
                        ++index;
                    }
                    _recentIndex = index - 1;
                    return false;
                }
                else {                                                  // Move backward until you reach end.
                    while(index > -1 && E(index).GetExplicitIndex() >= explicitIndex) {

                        if(E(index).GetExplicitIndex() == explicitIndex) {                        // Try to find an existing entry to return.
                            _recentIndex = index;
                            return true;
                        }
                        --index;
                    }
                    _recentIndex = index + 1;
                    return false;
                }
            }
            else {
                return false;
            }
        }

        public override string ToString() {
            var sb = new StringBuilder(360);
            sb.Append("{\n");

            for(int i = 0; i < _Count - 1; ++i) {
                sb.Append($"{_E[i].ToString()},\n");
            }
            sb.Append($"{_E[_Count - 1].ToString()}\n}}");
            return sb.ToString();
        } 
    }
}