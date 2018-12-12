using System;
using System.Text;
using SCG = System.Collections.Generic;

using Fluid.Internals;
using Fluid.Internals.Development;

namespace Fluid.Internals.Collections
{

	// TODO: Make sure erasure or splitting correctly adjust RecentRealIndex.
	public class IntSparseMat : EquatableList<IntSparseMatRow>
	{
		/// <summary>Width (length of rows) that matrix would have in its explicit form.</summary>
		int _Width;
		/// <summary>Width (length of rows) that matrix would have in its explicit form.</summary>
		public int Width => _Width;
		/// <summary>Height (length of columns) that matrix would have in its explicit form.</summary>
		int _Height;
		/// <summary>Height (length of columns) that matrix would have in its explicit form.</summary>
		public int Height => _Height;
		/// <summary>Index of most recently fetched (get or set) SparseMatrixRow.</summary>
		int _RecentRealIndex;
		/// <summary>RealIndex of most recently fetched (get or set) SparseMatrixRow.</summary>
		public int RecentRealIndex => _RecentRealIndex;
		/// <summary>VirtIndex of most recently fetched (get or set) SparseMatrixRow.</summary>
		public int RecentVirtIndex => _E[_RecentRealIndex].VirtIndex;
		/// <summary>Represents an unattached row, not in matrix. Negative virtual index conveys it's a dummy.</summary>
		IntSparseMatRow _DummyMatRow;
		/// <summary>Represents an unattached row, not in matrix. Negative virtual index conveys it's a dummy.</summary>
		public IntSparseMatRow DummyMatRow => _DummyMatRow;


		/// <summary>Create a SparseMatrix with given width, height, default value and initial row capacity.</summary><param name="width">Width (length of rows) that matrix would have in its explicit form.</param><param name="height">Height (length of columns) that matrix would have in its explicit form.</param><param name="capacity">Initial row capacity.</param>
		public IntSparseMat(int width, int height, int capacity = 10) : base(capacity) {
			_Width = width;
			_Height = height;
			_DummyMatRow = new IntSparseMatRow(_Width, -1);             // Empty row.
			_DummyMatRow.IsDummy = true;
			_DummyMatRow.SparseMatrix = this;
		}

		/// <summary>Create a copy of specified SparseMatrix.</summary><param name="source">Source SparseMatrix to copy.</param>
		public IntSparseMat(IntSparseMat source) {
			_Width = source.Width;
			_Height = source.Height;
			_Count = source.Count;
			_DummyMatRow = new IntSparseMatRow(source.DummyMatRow);      // FIXME: Set default value.
			_DummyMatRow.SparseMatrix = this;
			_E = new IntSparseMatRow[Height];
			Array.Copy(source._E, _E, Count);
			_RecentRealIndex = source.RecentRealIndex;
		}

		/// <summary>Creates a SparseMatrix from an array.</summary><param name="source">Source array.</param>
		public IntSparseMat(int[][] source) {                // TODO: Finish method that creates SparseMatrix from array.
			int nRows = source.Length;

			for(int i = 0; i < nRows; ++i) {
				int nCols = source[i].Length;

				for(int j = 0; j < nCols; ++j) {
					this[i][j] = source[i][j];				// Copy array items to matrix using indexer.
				}
			}
		}

		/// <summary>Check whether a row with specified virtual index exists. Returns (true, internal index of row) if it exists and (false, insertion index if we wish to have row with such VirtIndex).</summary><param name="virtRowIndex">Explicit index to check.</param>
		public (bool, int) GetRealRowIndex(int virtRowIndex, int searchDir = 1) {					// Assumes elements in rows are ordered by VirtIndex.
			if(Count != 0) {
				int realIndex = RecentRealIndex;
				bool found = false;

				if(RecentVirtIndex <= virtRowIndex) {              									// Move forward until you reach end.

					while(realIndex < Count && _E[realIndex].VirtIndex < virtRowIndex)           	// when second condition false, row with requested explicit index either does not exist or is the sought after row.
						++realIndex;

					if(_E[realIndex].VirtIndex == virtRowIndex) {									// This should be our sought after element, if it exists.
						_RecentRealIndex = realIndex;
						found = true;
					}
					return (found, realIndex);
				}
				else {
					while(realIndex > -1 && _E[realIndex].VirtIndex > virtRowIndex)           		// when second condition false, row with requested explicit index either does not exist or is the sought after row..
						--realIndex;

					if(_E[realIndex].VirtIndex == virtRowIndex) {
						_RecentRealIndex = realIndex;
						found = true;
					}
					return (found, ++realIndex);
				}
			}
			return (false, 0);
		}

		/// <summary>Retrieves row with specified VirtIndex.</summary>
		public override IntSparseMatRow this[int virtRowIndex] {
			get {
				(bool exists, int realRowIndex) = GetRealRowIndex(virtRowIndex);			// If exists = false, RealRowIndex signifies where it has to be inserted.

				if(exists)
					return _E[realRowIndex];
				else {
					DummyMatRow.VirtIndex = virtRowIndex;
					return DummyMatRow;
				}


				// if(Count != 0) {
				// 	ValidateIndex(ref _RecentRealIndex);
				// 	int realIndex = RecentRealIndex;

				// 	if(RecentVirtIndex <= imagIndex) {              // Move forward until you reach end.
				// 		while(realIndex < Count && _E[realIndex].VirtIndex <= imagIndex) {           // when second condition false, row with requested explicit index does not exist.

				// 			if(_E[realIndex].VirtIndex == imagIndex) {
				// 				_RecentRealIndex = realIndex;
				// 				return _E[realIndex];
				// 			}
				// 			++realIndex;
				// 		}
				// 		dummyIndex = realIndex;
				// 		_RecentRealIndex = realIndex;
				// 	}
				// 	else {
				// 		while(realIndex > -1 && _E[realIndex].VirtIndex >= imagIndex) {           // when second condition false, row with requested explicit index does not exist.

				// 			if(_E[realIndex].VirtIndex == imagIndex) {
				// 				_RecentRealIndex = realIndex;
				// 				return _E[realIndex];
				// 			}
				// 			--realIndex;
				// 		}
				// 		dummyIndex = realIndex + 1;
				// 		_RecentRealIndex = dummyIndex;
				// 	}
				// }
				// DummyMatrixRowInt.RealIndex = dummyIndex;                                // Inform the setter of SparseMatrixRow where in a SparseMatrix a row has to be inserted.
				// DummyMatrixRowInt.VirtIndex = imagIndex;
				// return DummyMatrixRowInt;                                         // Let the setter of SparseRow indexer create a new row if needed.
			}
		}

		/// <summary>Swap rows with specified explicit indices. Correctly handles cases with non-existent rows.</summary><param name="virtIndex1">Explicit index of first row to swap.</param><param name="virtIndex2">Explicit index of second row to swap.</param>
		public void SwapRows(int virtIndex1, int virtIndex2) {
			(bool row1Exists, int realIndex1) = GetRealRowIndex(virtIndex1);
			(bool row2Exists, int realIndex2) = GetRealRowIndex(virtIndex2);

			if(row1Exists) {                                          // Row with first VirtIndex exists.

				if(row2Exists) {                                          // Row with second VirtIndex exists. Simply swap real indices and virt indices of rows.
					IntSparseMatRow temp = _E[realIndex2];
					_E[realIndex2] = _E[realIndex1];
					_E[realIndex2].VirtIndex = virtIndex2;
					
					_E[realIndex1] = temp;
					_E[realIndex1].VirtIndex = virtIndex1;
				}
				else {                                                      // Row with second VirtIndex does not exist.
					_E[realIndex1].VirtIndex = virtIndex2;
					Insert(realIndex2, _E[realIndex1]);							// Insert non-existing row 2.

					if(realIndex1 >= realIndex2)                                  // Insertion has moved our existing element.
						RemoveAt(realIndex1 + 1);
					else
						RemoveAt(realIndex1);
				}
			}
			else {                                                      // Row with first explicit index does not exist.
				if(row2Exists) {                                         // Row with second explicit index exists.
					_E[realIndex2].VirtIndex = virtIndex1;
					Insert(realIndex1, _E[realIndex2]);

					if(realIndex2 >= realIndex1)                                  // Insertion has moved our existing element.
						RemoveAt(realIndex2 + 1);
					else
						RemoveAt(realIndex2);
				}
			}
		}

		/// <summary>Swap two columns. Correctly handles cases with non-existent elements.</summary><param name="imagIndex1">Explicit index of first column to swap.</param><param name="imagIndex2">Explicit index of second column to swap.</param>
		public void SwapColumns(int imagIndex1, int imagIndex2) {

			for(int i = 0; i < Count; ++i) {
				_E[i].SwapImagElements(imagIndex1, imagIndex2);
			}
		}

		public void ApplyColumnSwaps(IntSparseMat swapMatrix) {
			Assert.AreEqual(swapMatrix.Width, Width);          // Check that swap matrix dimensions are appropriate for this SparseRow.
			Assert.AreEqual(swapMatrix.Height, Width);
			int swapCount = swapMatrix.Count;                                                                   // Actual number of elements (SparseMatrixRows) in swapMatrix.

			for(int i = 0; i < swapCount; ++i) {
				ref var row = ref swapMatrix.E(i);                            // In-memory row i.
				int imagIndex1 = swapMatrix._E[i].VirtIndex;                          // Find its explicit row index. That is then index of first element to be swapped.
				int imagIndex2 = swapMatrix._E[i].E(0).VirtIndex;                   // Each row is expected to have one element. That is index of second element to be swapped.
				SwapColumns(imagIndex1, imagIndex2);
			}
		}

		/// <summary>Creates a SparseMatrix that is a sum of two operand SparseMatrices.</summary><param name="left">Left operand.</param><param name="right">Right operand.</param>
		public static IntSparseMat operator +(IntSparseMat left, IntSparseMat right) {

			Assert.AreEqual(left.Width, right.Width);             // Check that width and height of operands match.
			Assert.AreEqual(left.Height, right.Height);

			//SparseMatrixInt[] operands;

			IntSparseMat op1, op2;
			int op1RealRowIndex = 0;
			int op2RealRowIndex = 0;
			int op1VirtRowIndex, op2VirtRowIndex;
			int op1RowCount, op2RowCount;

			//int[] realRowIndex = new int[] { 0, 0 };                                                              // Current true row indices of operands.
			//int[] imagRowIndex;
			//int[] count;

			if(left._E[0].VirtIndex <= right._E[0].VirtIndex) {                                   // Pack operands in correct order for manipulation. One with smaller first row matrix index comes first.
				operands = new IntSparseMat[2] { left, right };
				imagRowIndex = new int[] { left._E[0].VirtIndex, right._E[0].VirtIndex };
				count = new int[] { left.Count, right.Count };
			}
			else {
				operands = new IntSparseMat[2] { right, left };
				imagRowIndex = new int[] { right._E[0].VirtIndex, left._E[0].VirtIndex };
				count = new int[] { right.Count, left.Count };
			}
			var resultMatrix = new IntSparseMat(left.Width, left.Height, (count[0] > count[1]) ? count[0] : count[1]);    // Result matrix starts with capacity of larger operand.
			int i = 0;
			int j = 1;

			while(true) {

				while(imagRowIndex[i] <= imagRowIndex[j]) {   // Set index where next iteration will kick off. At start of this loop it must hold: rowIndex[j] = rowIndex[i].

					if(realRowIndex[i] < count[i]) {    // Check that we haven't reached the end of any of internal arrays.

						if(realRowIndex[j] < count[j]) {
							if(imagRowIndex[i] < imagRowIndex[j]) {
								resultMatrix.Add(new IntSparseMatRow(operands[i]._E[realRowIndex[i]]));
							}
							else if(imagRowIndex[i] == imagRowIndex[j]) {
								var sparseMatrixRow = operands[i]._E[realRowIndex[i]] + operands[j]._E[realRowIndex[j]];
								resultMatrix.Add(sparseMatrixRow);
								++realRowIndex[j];

								if(realRowIndex[j] < count[j]) {
									imagRowIndex[j] = operands[j]._E[realRowIndex[j]].ImagIndex;
								}
							}
						}
						else {              // We have reached end of j-th internal array. Write the rest of i-th elements to result.
							while(realRowIndex[i] < count[i]) {
								resultMatrix.Add(new IntSparseMatRow(operands[i]._E[realRowIndex[i]]));
								++realRowIndex[i];
							}
							return resultMatrix;
						}
						++realRowIndex[i];

						if(realRowIndex[i] < count[i]) {
							imagRowIndex[i] = operands[i]._E[realRowIndex[i]].ImagIndex;
						}
					}
					else {                  // We have reached end of i-th internal array. Write the rest of j-th elements to result.
						while(realRowIndex[j] < count[j]) {
							resultMatrix.Add(new IntSparseMatRow(operands[j]._E[realRowIndex[j]]));
							++realRowIndex[j];
						}
						return resultMatrix;
					}
				}
				i = (i + 1) % 2;                                  // Exchange processed operand in each new iteration.
				j = (j + 1) % 2;
			}
			throw new ArithmeticException("Method which sums two SparseMatrices has reached an invalid state.");


		}

		public static IntSparseRow operator *(IntSparseMat matrix, IntSparseRow vector) {
			Assert.AreEqual(matrix.Width, vector.Width);                 // Check that matrix and row can be multiplied.

			// 1) Go through each row in left matrix. Rows that do not exist, create no entries in result row.
			// 2) Move over each element in row i, check its explicit index, then search for an element with
			//      matching explicit ndex in right row.
			// 3) Add all such contributions to element with explicit index i in right row.
			// 4) Return result row.

			var resultRow = new IntSparseRow(matrix.Height, matrix.Count);
			int matrixColCount;                                                  // Number of occupied columns in a matrix row. Different for each row.
			IntSparseMatRow matrixRow = matrix._E[0];
			int resultValue;
			int imagRowIndex;
			int imagColIndex;

			for(int i = 0; i < matrix.Count; ++i) {            // Loop over rows.
				matrixRow = matrix._E[i];
				matrixColCount = matrixRow.Count;
				imagRowIndex = matrixRow.VirtIndex;
				resultValue = 0;

				for(int j = 0; j < matrixColCount; ++j) {
					imagColIndex = matrixRow.E(j).VirtIndex;
					resultValue += matrixRow.E(j).Value * vector[imagColIndex];                      // No worries compiler, types are ok.
				}
				resultRow[imagRowIndex] = resultValue;
			}
			return resultRow;
		}

		public static IntSparseRow operator *(IntSparseRow vector, IntSparseMat matrix) {
			Assert.AreEqual(vector.Width, matrix.Height);

			var resultRow = new IntSparseRow(matrix.Width);        // Result row has width of input matrix.
			ref IntSparseMatRow matrixRow = ref matrix._E[0];
			int matrixColCount;                                             // Different for each row.
			int imagRowIndex;
			int imagColIndex;

			for(int i = 0; i < matrix.Count; ++i) {               // Over each row of matrix.
				matrixRow = matrix._E[i];
				imagRowIndex = matrixRow.VirtIndex;
				matrixColCount = matrixRow.Count;

				for(int j = 0; j < matrixColCount; ++j) {
					imagColIndex = matrixRow.E(j).VirtIndex;
					resultRow[imagColIndex] += matrixRow.E(j).Value * vector[imagRowIndex];
				}
			}
			return resultRow;
		}

		/// <summary>Split matrix on left and right part. Return right part. Element at specified index will be part of right part.</summary><param name="col">Index of element at which to split. This element will be part of right matrix.</param>
		public IntSparseMat SplitAtColumn(int imagColIndex) {
			int removedWidth = Width - imagColIndex;
			var removedCols = new IntSparseMat(removedWidth, Height);
			_Width = imagColIndex;                                               // Adjust width of this Matrix.

			for(int i = 0; i < Count; ++i) {                                   // Split each SparseRow separately.
				var removedRow = _E[i].SplitAt(imagColIndex);
				removedCols.Add(removedRow);
			}
			return removedCols;
		}

		/// <summary>Split matrix on upper and lower part. Return lower part. Element at specified index will be part of lower part.</summary><param name="col">Index of element at which to split. This element will be part of lower matrix.</param>
		public IntSparseMat SplitAtRow(int imagRowIndex) {
			var splitSuccess = SetRecentIndexToOrAheadOf(imagRowIndex);
			int removedHeight = Height - imagRowIndex;
			IntSparseMat removedRows;

			if(splitSuccess) {                                          // Some elements can be trimmed.
				removedRows = RemoveRange(RecentRealIndex, Count - 1);
				TrimExcessSpace();
			}
			else {                                                          // No elements to trim.
				_Height = imagRowIndex;
				removedRows = new IntSparseMat(Width, removedHeight, 6);
			}
			return removedRows;
		}

		/// <summary>Removes specified range of rows from SparseMatrix and returns it. Correctly adjusts width. Range is specified in terms of internal indices, not explicit ones.</summary><param name="j">Inclusive starting index.</param><param name="k">Inclusive ending index.</param>
		new public IntSparseMat RemoveRange(int j, int k) {
			int removedCount = k - j + 1;
			var removed = new IntSparseMat(Height, removedCount);

			for(int i = 0; i < removedCount; ++i) {                         // Construct Matrix that we will return.
				BeforeElementExit(i);
				removed[i] = _E[j + i];
			}
			for(int i = k + 1; i < Count; ++i) {                           // Refill hole. Shift elements remaining on right side of hole (removed range) to right.
				BeforeElementExit(i);
				_E[i - removedCount] = _E[i];
				AfterElementEntry(i - removedCount);
			}
			_Count = Count - removedCount;                                 // Changing count, no need to zero elements at end.
			_Height = _E[Count].VirtIndex + 1;
			return removed;
		}

		/// <summary>Sets recent index to element with given explicit index. If element with specified explicit index does not exist but elements ahead exist, it sets recent index to first element ahead of specified explicit index. If specified element does not exist and end is reached, it returns false.</summary><param name="imagIndex">Explicit index of desired element.</param>
		protected bool SetRecentIndexToOrAheadOf(int imagIndex) {
			Assert.IndexInRange(imagIndex, 0, _Width);

			if(Count != 0) {
				ValidateIndex(ref _RecentRealIndex);
				int realIndex = _RecentRealIndex;

				if(RecentVirtIndex <= imagIndex) {                                                      // Move forward until you reach end.
					while(realIndex < Count && _E[realIndex].VirtIndex <= imagIndex) {

						if(_E[realIndex].VirtIndex == imagIndex) {                        // Try to find an existing entry to return.
							_RecentRealIndex = realIndex;
							return true;
						}
						++realIndex;
					}
					_RecentRealIndex = realIndex - 1;
					return false;
				}
				else {                                                  // Move backward until you reach end.
					while(realIndex > -1 && _E[realIndex].VirtIndex >= imagIndex) {

						if(_E[realIndex].VirtIndex == imagIndex) {                        // Try to find an existing entry to return.
							_RecentRealIndex = realIndex;
							return true;
						}
						--realIndex;
					}
					_RecentRealIndex = realIndex + 1;
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

// TODO: Maybe try to implement SparseMatrix as Dictionary.