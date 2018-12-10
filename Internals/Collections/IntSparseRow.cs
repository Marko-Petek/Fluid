#if FALSE
using System;
using System.Text;
using static System.Math;

using Fluid.Internals.Development;
using static Fluid.Internals.Operations;

namespace Fluid.Internals.Collections
{
    /// <summary>A collection of indexed elements representing entries of a (big) matrix.</summary><remarks>Integer implementation of generic SparseRow(T), written to avoid dynamically typed code. Statically typed mechanism for arithmetic inside generics did not exist at the time of writing.</remarks>
    public class IntSparseRow : EquatableList<IntSparseElm>
    {
        
        /// <summary>Width of row if written out explicitly.</summary>
        protected int _Width;
        /// <summary>Width of row if written out explicitly.</summary>
        public int Width => _Width;
        protected int RecentVirtIndex => E(RecentIndex).VirtIndex;


        /// <summary>Create a SparseRow with specified width it would have in explicit form, specified default value and specified initial capacity.</summary><param name="width">Width it would have in explicit form.</param><param name="capacity">Capacity of internal array. If <= 0 internal array is not created.</param>
        public IntSparseRow(int width, int capacity = 6) : base(capacity) {
            _Width = width;
        }

        /// <summary>Create a copy of specified IntSparseRow.</summary><param name="source">Source IntSparseRow to copy.</param>
        public IntSparseRow(IntSparseRow source) : base(source) {
            _Width = source._Width;
        }

        /// <summary>Create a new SparseRow by absorbing specified source array.</summary><param name="source">Source array to adopt.</param>
        public static IntSparseRow CreateFromArray(IntSparseElm[] source, int defaultValue = 1) {
            int count = source.Length;
            IntSparseElm lastElm = source[count - 1];
            int width = lastElm.VirtIndex + 1;                                   // Take smallest possible width.
            var intSparseRow = new IntSparseRow(width, -1);
            intSparseRow._E = source;                                                    // Absorb provided array.
            return intSparseRow;
        }

        /// <summary>Add an element beyond last non-zero value element IntSparseRow and return its virtual index.</summary><param name="val">Integer to add.</param>
        public int Add(int val) {
            if(Count == 0) {
                Add(new IntSparseElm(0, val));
                return 0;
            }
            else {
                int virtIndex =  E(Count - 1).VirtIndex + 1;
                Add(new IntSparseElm(virtIndex, val));
                return virtIndex;
            }
        }

        /// <summary>Retrieves an element with given matrix index (0 if not present), or sets it (creates if not present).</summary>
        new public virtual int this[int virtIndex] {
            get {
                Assert.IndexInRange(virtIndex, 0, Width);
                
                if(Count != 0) {
                    ValidateIndex(ref _RecentIndex);
                    int i = RecentIndex;

                    if(RecentVirtIndex <= virtIndex) {              // Move forward until you reach end.
                        while(i < Count && _E[i].VirtIndex <= virtIndex) {

                            if(_E[i].VirtIndex == virtIndex) {                        // Try to find an existing entry to return.
                                _RecentIndex = i;
                                return _E[i].Value;
                            }
                            ++i;
                        }
                        _RecentIndex = i - 1;
                    }
                    else {                                                  // Move backward until you reach end.
                        while(i > -1 && _E[i].VirtIndex >= virtIndex) {

                            if(_E[i].VirtIndex == virtIndex) {                        // Try to find an existing entry to return.
                                _RecentIndex = i;
                                return _E[i].Value;
                            }
                            --i;
                        }
                        _RecentIndex = i + 1;
                    }
                }
                return 0;
            }
            set {
                Assert.IndexInRange(virtIndex, 0, Width);
                int insertIndex = 0;

                if(Count != 0) {
                    ValidateIndex(ref _RecentIndex);
                    int i = RecentIndex;

                    if(RecentVirtIndex <= virtIndex) {
                        while(i < Count && _E[i].VirtIndex <= virtIndex) {

                            if(_E[i].VirtIndex == virtIndex) {                        // Try to find an existing entry to modify.
                                if(value == 0)
                                    RemoveAt(i);
                                else
                                    _E[i].Value = value;

                                _RecentIndex = i;
                                return;
                            }
                            ++i;
                        }
                        insertIndex = i;
                        _RecentIndex = i - 1;
                    }
                    else {
                        while(i > -1 && _E[i].VirtIndex >= virtIndex) {

                            if(_E[i].VirtIndex == virtIndex) {                        // Try to find an existing entry to modify.
                                if(value == 0)
                                    RemoveAt(i);
                                else
                                    _E[i].Value = value;

                                _RecentIndex = i;
                                return;
                            }
                            --i;
                        }
                        insertIndex = i + 1;
                        _RecentIndex = insertIndex;
                    }
                }

                if(value != 0) {
                    Insert(insertIndex, new IntSparseElm(virtIndex, value));                // Count = 0 or end has been reached, add fresh element.
                }
            }
        }

        /// <summary>Returns an element nSteps away from recent position and keeps cursor at current position.</summary>
        /// <param name="nSteps">Number of steps to take (can be negative).</param>
        /// <returns>Return value if element exists: (true, element), else: (false, first or last element in internal array).</returns>
        public (bool exists, IntSparseElm) Peek(int nSteps = 0) {
            int peekIndex = RecentIndex + nSteps;

            if(peekIndex >= 0)
            {
                if(peekIndex < Count)
                    return (true, _E[peekIndex]);
                else
                    return (false, _E[Count - 1]);
            }  
            else
                return (false, _E[0]);
        }

        /// <summary>Returns an element nSteps away from recent position and moves cursor to its position.</summary><param name="nSteps">Number of steps to take (can be negative).</param><returns>Return value if element exists: (true, element), else: (false, first or last element in internal array).</returns>
        public (bool, IntSparseElm) Move(int nSteps = 0) {
            int moveIndex = RecentIndex + nSteps;

            if(moveIndex >= 0)
            {
                if(moveIndex < Count) {
                    _RecentIndex = moveIndex;
                    return (true, _E[moveIndex]);
                }
                else {
                    _RecentIndex = Count - 1;
                    return (false, _E[Count - 1]);
                }
            }  
            else {
                _RecentIndex = 0;
                return (false, _E[0]);
            }
        }

        // TODO: 3.XII.  Finish + operator implementation.
        /// <summary>Creates an IntSparseRow that is a sum of two IntSparseRows.</summary><param name="left">Left operand.</param><param name="right">Right operand.</param>
        public static IntSparseRow operator + (IntSparseRow left, IntSparseRow right) {
            Assert.AreEqual(left.Width, right.Width, Comparers.Int);                     // Check that rows are the same width.
            //IntSparseRow[] operands;
            IntSparseRow op1, op2;
            // int[] realColIndex = new int[] {0, 0};                                                              // Current true column indices (inside _sparseRows) of operands.
            // int[] virtColIndex;
            // int[] count;

            if(left._E[0].VirtIndex <= right._E[0].VirtIndex) {                                   // Pack operands in correct order for manipulation. One with smaller first row matrix index comes first.
                op1 = left;
                op2 = right;
                // operands = new IntSparseRow[2] {left, right};
                // virtColIndex = new int[] {left._E[0].VirtIndex, right._E[0].VirtIndex};
                // count = new int[] {left.Count, right.Count};
            }
            else {
                op1 = right;
                op2 = left;
                // operands = new IntSparseRow[2] {right, left};
                // virtColIndex = new int[] {right._E[0].VirtIndex, left._E[0].VirtIndex};
                // count = new int[] {right.Count, left.Count};
            }
            var resultRow = new IntSparseRow(left.Width, (op1.Count > op2.Count) ? op1.Count : op2.Count);    // Result row starts with capacity of larger operand.
            // int i = 0;
            // int j = 1;
            int i = 0;          // Counts cols in op1.
            int j = 0;          // Counts cols in op2.

            while(true) {
                
                while(op1._E[i].VirtIndex <= op2._E[j].VirtIndex) {   // Set index where next iteration will kick off. At start of this loop it must hold: colIndex[j] = colIndex[i].
                    
                    if(i < op1.Count) {    // Check that we haven't reached the end of any of internal arrays.

                        if(j < op2.Count) {
                            
                            if(op1._E[i].VirtIndex < op2._E[j].VirtIndex) {                              // Element with explicitColIndex[i] not present in operand j.
                                resultRow.Add(new IntSparseElm(op1._E[i]));
                            }
                            else if(op1._E[i].VirtIndex == op2._E[i].VirtIndex){                         // Matching element with explicitColIndex[i] found in operand j.
                                resultRow.Add(op1._E[i] + op2._E[j]);
                                ++j;
                                // TODO: 4.XII. Make sure i < count ... probably: if(++i < Count)
                                // if(j < op2.Count) {
                                //     virtColIndex[j] = operands[j]._E[realColIndex[j]].VirtIndex;
                                // }
                            }
                        }
                        else {                                                                          // We have reached end of j-th internal array. Write the rest of i-th elements to result.
                            while(i < op1.Count) {
                                resultRow.Add(new IntSparseElm(op1._E[i]));
                                ++i;
                            }
                            return resultRow;
                        }
                        ++i;

                        // if(realColIndex[i] < count[i]) {
                        //     virtColIndex[i] = operands[i]._E[realColIndex[i]].VirtIndex;        // Explicit index of SparseElement.
                        // }
                    }
                    else {                  // We have reached end of i-th internal array. Write the rest of j-th elements to result.
                        while(j < op2.Count) {
                            resultRow.Add(new IntSparseElm(op2._E[j]));
                            ++j;
                        }
                        return resultRow;
                    }
                }
                Swap(ref i, ref j);         // Exchange roles in each new iteration.
                Swap(ref op1, ref op2);
            }
            throw new ArithmeticException("Method which sums two SparseRows has reached an invalid state.");
        }

        public static IntSparseRow operator - (IntSparseRow left, IntSparseRow right) {
            Assert.AreEqual(left.Width, right.Width, Comparers.Int);                     // Check that rows are the same width.
            IntSparseRow[] operands;
            int[] realColIndex = new int[] {0, 0};                                                              // Current true column indices (inside _sparseRows) of operands.
            int[] imagColIndex;
            int[] count;

            if(left._E[0].VirtIndex <= right._E[0].VirtIndex) {                                   // Pack operands in correct order for manipulation. One with smaller first row matrix index comes first.
                operands = new IntSparseRow[2] {left, right};
                imagColIndex = new int[] {left._E[0].VirtIndex, right._E[0].VirtIndex};
                count = new int[] {left.Count, right.Count};
            }
            else {
                operands = new IntSparseRow[2] {right, left};
                imagColIndex = new int[] {right._E[0].VirtIndex, left._E[0].VirtIndex};
                count = new int[] {right.Count, left.Count};
            }
            var resultRow = new IntSparseRow(left.Width, (count[0] > count[1]) ? count[0] : count[1]);    // Result row starts with capacity of larger operand.
            int i = 0;
            int j = 1;

            while(true) {
                
                while(imagColIndex[i] <= imagColIndex[j]) {   // Set index where next iteration will kick off. At start of this loop it must hold: colIndex[j] = colIndex[i].
                    
                    if(realColIndex[i] < count[i]) {    // Check that we haven't reached the end of any of internal arrays.

                        if(realColIndex[j] < count[j]) {
                            
                            if(imagColIndex[i] < imagColIndex[j]) {                                     // Element with explicitColIndex[i] not present in operand j.
                                resultRow.Add(new IntSparseElm(operands[i]._E[realColIndex[i]]));
                            }
                            else if(imagColIndex[i] == imagColIndex[j]){                                // Matching element with explicitColIndex[i] found in operand j.
                                resultRow.Add(operands[i]._E[realColIndex[i]] - operands[j]._E[realColIndex[j]]);
                                ++realColIndex[j];

                                if(realColIndex[j] < count[j]) {
                                    imagColIndex[j] = operands[j]._E[realColIndex[j]].VirtIndex;
                                }
                            }
                        }
                        else {              // We have reached end of j-th internal array. Write the rest of i-th elements to result.
                            while(realColIndex[i] < count[i]) {
                                resultRow.Add(new IntSparseElm(operands[i]._E[realColIndex[i]]));
                                ++realColIndex[i];
                            }
                            return resultRow;
                        }
                        ++realColIndex[i];

                        if(realColIndex[i] < count[i]) {
                            imagColIndex[i] = operands[i]._E[realColIndex[i]].VirtIndex;        // Explicit index of SparseElement.
                        }
                    }
                    else {                  // We have reached end of i-th internal array. Write the rest of j-th elements to result.
                        while(realColIndex[j] < count[j]) {
                            resultRow.Add(new IntSparseElm(operands[j]._E[realColIndex[j]]));
                            ++realColIndex[j];
                        }
                        return resultRow;
                    }
                }
                i = (i+1) % 2;                                  // Exchange processed operand in each new iteration.
                j = (j+1) % 2;
            }
            throw new ArithmeticException("Method which sums two SparseRows has reached an invalid state.");
        }

        /// <summary>Creates a SparseRow that is a dot product of two operand SparseRows.</summary><param name="left">Left operand.</param><param name="right">Right operand.</param><returns>SparseRow that is sum of two operands.</returns>
        public static double operator * (IntSparseRow left, IntSparseRow right) {
            
            Assert.AreEqual(left.Width, right.Width, Comparers.Int);                     // Check that rows are the same width.
            IntSparseRow[] operands;
            int[] realColIndex = new int[] {0, 0};                                                              // Current true column indices (inside _sparseRows) of operands.
            int[] imagColIndex;
            int[] count;

            if(left._E[0].VirtIndex <= right._E[0].VirtIndex) {                                   // Pack operands in correct order for manipulation. One with smaller first row matrix index comes first.
                operands = new IntSparseRow[2] {left, right};
                imagColIndex = new int[] {left._E[0].VirtIndex, right._E[0].VirtIndex};
                count = new int[] {left.Count, right.Count};
            }
            else {
                operands = new IntSparseRow[2] {right, left};
                imagColIndex = new int[] {right._E[0].VirtIndex, left._E[0].VirtIndex};
                count = new int[] {right.Count, left.Count};
            }
            double result = 0;
            int i = 0;
            int j = 1;
            
            while(true) {
                
                while(imagColIndex[i] <= imagColIndex[j]) {   // Set index where next iteration will kick off. At start of this loop it must hold: colIndex[j] = colIndex[i].
                    
                    if(realColIndex[i] < count[i]) {    // Check that we haven't reached the end of any of internal arrays.

                        if(realColIndex[j] < count[j]) {
                            
                            if(imagColIndex[i] == imagColIndex[j]){                                // Matching element with explicitColIndex[i] found in operand j.
                                result += operands[i].E(realColIndex[i]).Value * operands[j].E(realColIndex[j]).Value;
                                ++realColIndex[j];

                                if(realColIndex[j] < count[j]) {
                                    imagColIndex[j] = operands[j]._E[realColIndex[j]].VirtIndex;
                                }
                            }
                        }
                        else {                  // When we reach end of one of columns, stop.
                            return result;
                        }
                        ++realColIndex[i];

                        if(realColIndex[i] < count[i]) {
                            imagColIndex[i] = operands[i]._E[realColIndex[i]].VirtIndex;        // Explicit index of SparseElement.
                        }
                    }
                    else {                      // When we reach end of one of columns, stop.
                        return result;
                    }
                }
                i = (i+1) % 2;                  // Exchange processed operand in each new iteration.
                j = (j+1) % 2;
            }
            throw new ArithmeticException("Method which sums two SparseRows has reached an invalid state.");
        }

        public static IntSparseRow operator * (int left, IntSparseRow right) {

            var resultRow = new IntSparseRow(right);

            for(int i = 0; i < right.Count; ++i) {
                resultRow.E(i).Value = left * right._E[i].Value;
            }
            return resultRow;
        }

        public double CalcNormSquared() {
            double result = 0.0;

            for(int i = 0; i < Count; ++i) {
                result += Pow(_E[i].Value, 2.0);
            }
            return result;
        }

        /// <summary>Swap values of two elements specified by real indices.</summary><param name="realIndex1">First internal index.</param><param name="realIndex2">Second internal index.</param>
        public void SwapRealElements(int realIndex1, int realIndex2) => Swap(ref _E[realIndex1]._Value, ref _E[realIndex2]._Value);

        /// <summary>Swap two elements specified by imaginary indices.</summary><param name="imagIndex1">Explicit index of first element.</param><param name="imagIndex2">Explicit index of second element.</param><remarks>Useful for swapping columns.</remarks>
        public void SwapImagElements(int imagIndex1, int imagIndex2) {
            int firstValue = this[imagIndex1];
            this[imagIndex1] = this[imagIndex2];
            this[imagIndex2] = firstValue;
        }

        /// <summary>Apply element swaps as specified by a given swap matrix.</summary><param name="swapMatrix">SparseMatrix where non-zero element at [i][j] signifies a permutation i --> j.</param>
        public void ApplySwaps(IntSparseMat swapMatrix) {
            Assert.AreEqual(swapMatrix.Width, Width);          // Check that swap matrix dimensions are appropriate for this SparseRow.
            Assert.AreEqual(swapMatrix.Height, Width);
            int swapCount = swapMatrix.Count;                                                                   // Actual number of elements (SparseMatrixRows) in swapMatrix.

            for(int i = 0; i < swapCount; ++i) {
                int element1 = swapMatrix[i].VirtIndex;
                int element2 = swapMatrix[i]._E[0].VirtIndex;                                         // Each row is expected to have one element.
                SwapImagElements(element1, element2);
            }
        }

        /// <summary>Splits SparseRow in two SparseRows. This SparseRow is modified to be left remainder, while right remainder is returned as result.</summary><param name="imagIndex">Index at which to split. Element at this index will be part of right remainder.</param>
        public IntSparseRow SplitAt(int imagIndex) {
            var splitSuccess = SetRecentIndexToOrAheadOf(imagIndex);
            IntSparseRow removedCols;
            if(splitSuccess) {
                removedCols = RemoveRange(RecentRealIndex, _E.Length - 1);      // Remove every element from specified point onwards.
                TrimExcessSpace();
            }
            else {                                                                      // No elements to trim away.
                _Width = imagIndex;                                                 // Readjust width.
                removedCols = new IntSparseRow(Width - imagIndex, 6);          // Return an empty SparseRow with width of trimming.
            }
            return removedCols;
        }

        /// <summary>Sets recent index to element with given imaginary index. If element with specified explicit index does not exist, it sets recent index to first element ahead of specified explicit index.</summary><param name="imagIndex">Explicit index of desired element.</param>
        protected bool SetRecentIndexToOrAheadOf(int imagIndex) {
            Assert.IndexInRange(imagIndex, 0, Width);
                
            if(Count != 0) {
                ValidateIndex(ref _RecentRealIndex);
                int realIndex = _RecentRealIndex;

                if(RecentVirtIndex <= imagIndex) {              // Move forward until you reach end.
                    while(realIndex < _Count && _E[realIndex].VirtIndex <= imagIndex) {

                        if(_E[realIndex].VirtIndex == imagIndex) {                        // Try to find an existing entry to return.
                            RecentRealIndex = realIndex;
                            return true;
                        }
                        ++realIndex;
                    }
                    RecentRealIndex = realIndex - 1;
                    return false;
                }
                else {                                                  // Move backward until you reach end.
                    while(realIndex > -1 && _E[realIndex].VirtIndex >= imagIndex) {

                        if(_E[realIndex].VirtIndex == imagIndex) {                        // Try to find an existing entry to return.
                            RecentRealIndex = realIndex;
                            return true;
                        }
                        --realIndex;
                    }
                    RecentRealIndex = realIndex + 1;
                    return false;
                }
            }
            else {
                return false;
            }
        }

        /// <summary>Removes specified range from SparseRow and returns it. Correctly adjusts width. Range is specified in terms of internal indices, not explicit ones.</summary><param name="j">Inclusive starting index.</param><param name="k">Inclusive ending index.</param>
        new public IntSparseRow RemoveRange(int j, int k) {
            int removedCount = k - j + 1;
            IntSparseElm[] removed = new IntSparseElm[removedCount];
            
            for(int i = 0; i < removedCount; ++i) {             // Construct array that we will return.
                BeforeElementExit(i);                           // FIXME: Return and check whether this is needed.
                removed[i] = _E[j + i];
            }
            for(int i = k + 1; i < _Count; ++i) {               // Refill hole. Shift elements remaining on right side of hole (removed range) to right.
                BeforeElementExit(i);
                _E[i - removedCount] = _E[i];
                AfterElementEntry(i - removedCount);
            }
            _Count = _Count - removedCount;                     // Changing count, no need to zero elements at end.
            _Width = _E[Count].VirtIndex + 1;
            return CreateFromArray(removed);
        }

        /// <summary>Append specified SparseRow to this one.</summary><param name="rightCols">Append this SparseRow.</param>
        public void MergeWith(IntSparseRow rightCols) {
            _Width += rightCols.Width;                         // Readjust width.
            AddRange(rightCols);
        }

        /// <summary>Create a string of form (sparseElement 0, sparseElement 1, ..., sparseElement N-1).</summary>
        public override string ToString() {
            StringBuilder sb = new StringBuilder(72);

            for(int i = 0; i < _Count - 1; ++i) {
                sb.Append($"{_E[i].ToString()}, ");
            }
            sb.Append($"{_E[_Count - 1]}");
            return sb.ToString();
        }
    }
}
#endif