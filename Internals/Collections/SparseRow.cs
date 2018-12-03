using System;
using System.Text;
using SCG = System.Collections.Generic;
using static System.Math;

using Fluid.Internals.Collections;
using Fluid.Internals.Development;

namespace Fluid.Internals.Collections
{
    /// <summary>A memeber of SparseArray.</summary>
    public class SparseRow<T> : EquatableManagedList<SparseElm<T>>
    where T : struct, IEquatable<T>
    {
        /// <summary>Width of row if written out explicitly.</summary>
        protected int _Width;
        /// <summary>Width of row if written out explicitly.</summary>
        public int Width => _Width;
        /// <summary>Index of most recently manipulated (get or set) SparseElement value.</summary>
        protected int _RecentIndex = 0;



        /// <summary>Create a SparseRow with specified width it would have in explicit form and specified initial capacity.</summary><param name="width">Width it would have in explicit form.</param>
        public SparseRow(int width, int capacity) : base(capacity) {
            _Width = width;
        }
        /// <summary>Create a SparseRow with specified width it would have in explicit form and default initial capacity.</summary><param name="width">Width it would have in explicit form.</param>
        public SparseRow(int width) : this(width, 6) {
        }
        /// <summary>Create a copy of specified SparseRow.</summary><param name="sparseRow">Source SparseRow to copy.</param>
        public SparseRow(SparseRow<T> sparseRow) : base(sparseRow) {
            _Width = sparseRow._Width;
        }



        /// <summary>Marks SparseElement as part of a SparseRow by letting it know its own index inside SparseRow.</summary><param name="index">True index inside SparseRow list.</param>
        protected override void AfterElementEntry(int index) {
            _E[index].RealIndex = index;

            for(int i = index + 1; i < Count; ++i) {    // Update indices of all elements ahead.
                E(i).RealIndex = i + 1;
            }
        }
        /// <summary>Marks SparseElement as not being part of a SparseRow anymore.</summary><param name="index">True index inside SparseRow list.</param>
        protected override void BeforeElementExit(int index) {
           _E[index].RealIndex = -1;

           for(int i = index + 1; i < Count; ++i) {    // Update indices of all elements ahead.
                E(i).RealIndex = i - 1;
            }
        }

        /// <summary>Create a new SparseRow by adopting specified source array.</summary><param name="source">Source array to adopt.</param>
        new public SparseRow<T> CreateFromArray(SparseElm<T>[] source) {
            int lastExplicitIndex = source[source.Length - 1].ImagIndex();
            var row = new SparseRow<T>(lastExplicitIndex + 1);
            row._E = source;
            
            for(int i = 0; i < source.Length; ++i) {                            // Remember to reset indices.
                AfterElementEntry(i);
            }
            return row;
        }

        /// <summary>Add an element to end of SparseRow and return its explicit index.</summary><param name="element">Element to be added.</param>
        public int AddElement(T element) {
            if(_Count == 0) {
                Add(new SparseElm<T>(0, 0, element));
                _RecentIndex = 0;
                return 0;
            }
            else {
                int newExplicitIndex =  E(_Count - 1).ImagIndex() + 1;
                Add(new SparseElm<T>(_Count, newExplicitIndex, element));
                return newExplicitIndex;
            }
        }

        /// <summary>Retrieves an element with given matrix index (0 if not present), or sets it (creates if not present).</summary>
        new public virtual T this[int explicitIndex] {
            get {
                Assert.IndexInRange(explicitIndex, 0, Width);
                
                if(Count != 0) {
                    ValidateIndex(ref _RecentIndex);
                    int index = _RecentIndex;
                    int recentExplicitIndex = E(index).ImagIndex();

                    if(recentExplicitIndex <= explicitIndex) {              // Move forward until you reach end.
                        while(index < _Count && E(index).ImagIndex() <= explicitIndex) {

                            if(E(index).ImagIndex() == explicitIndex) {                        // Try to find an existing entry to return.
                                _RecentIndex = index;
                                return E(index).Value;
                            }
                            ++index;
                        }
                        _RecentIndex = index - 1;
                    }
                    else {                                                  // Move backward until you reach end.
                        while(index > -1 && E(index).ImagIndex() >= explicitIndex) {

                            if(E(index).ImagIndex() == explicitIndex) {                        // Try to find an existing entry to return.
                                _RecentIndex = index;
                                return E(index).Value;
                            }
                            --index;
                        }
                        _RecentIndex = index + 1;
                    }
                }
                return default(T);
            }
            set {
                Assert.IndexInRange(explicitIndex, 0, Width);
                int insertIndex = 0;

                if(_Count != 0) {
                    ValidateIndex(ref _RecentIndex);
                    int index = _RecentIndex;
                    int recentExplicitIndex = E(index).ImagIndex();

                    if(recentExplicitIndex <= explicitIndex) {
                        while(index < _Count && _E[index].ImagIndex() <= explicitIndex) {

                            if(E(index).ImagIndex() == explicitIndex) {                        // Try to find an existing entry to modify.
                                if(SparseElm<T>._EqualityComparer.Equals(value, default(T))) {
                                    RemoveAt(index);
                                }
                                else {
                                    E(index).Value = value;
                                }
                                _RecentIndex = index;
                                return;
                            }
                            ++index;
                        }
                        insertIndex = index;
                        _RecentIndex = index - 1;
                    }
                    else {
                        while(index > -1 && _E[index].ImagIndex() >= explicitIndex) {

                            if(E(index).ImagIndex() == explicitIndex) {                        // Try to find an existing entry to modify.
                                if(SparseElm<T>._EqualityComparer.Equals(value, default(T))) {
                                    RemoveAt(index);
                                }
                                else {
                                    E(index).Value = value;
                                }
                                _RecentIndex = index;
                                return;
                            }
                            --index;
                        }
                        insertIndex = index + 1;
                        _RecentIndex = insertIndex;
                    }
                }
                if(!SparseElm<T>._EqualityComparer.Equals(value, default(T))) {
                    Insert(insertIndex, new SparseElm<T>(insertIndex, explicitIndex, value));                // Count = 0 or end has been reached, add fresh element.
                }
            }
        }

        /// <summary>Creates a SparseRow that is a sum of two operand SparseRows.</summary><param name="left">Left operand.</param><param name="right">Right operand.</param><returns>SparseRow that is sum of two operands.</returns>
        public static SparseRow<T> operator + (SparseRow<T> left, SparseRow<T> right) {
            
            Assert.AreEqual(left.Width, right.Width);                     // Check that rows are the same width.
            SparseRow<T>[] operands;
            int[] colIndex = new int[] {0, 0};                                                              // Current true column indices (inside _sparseRows) of operands.
            int[] explicitColIndex;
            int[] count;

            if(left._E[0].ImagIndex() <= right._E[0].ImagIndex()) {                                   // Pack operands in correct order for manipulation. One with smaller first row matrix index comes first.
                operands = new SparseRow<T>[2] {left, right};
                explicitColIndex = new int[] {left._E[0].ImagIndex(), right._E[0].ImagIndex()};
                count = new int[] {left._Count, right._Count};
            }
            else {
                operands = new SparseRow<T>[2] {right, left};
                explicitColIndex = new int[] {right._E[0].ImagIndex(), left._E[0].ImagIndex()};
                count = new int[] {right._Count, left._Count};
            }
            var resultRow = new SparseRow<T>(left._Width, (count[0] > count[1]) ? count[0] : count[1]);    // Result row starts with capacity of larger operand.
            
            for(int i = 0, j = 1; true; i = (i+1) % 2, j = (j+1) % 2) {      // Exchange processed operand in each new iteration.
                
                while(explicitColIndex[i] <= explicitColIndex[j]) {   // Set index where next iteration will kick off. At start of this loop it must hold: colIndex[j] = colIndex[i].
                    
                    if(colIndex[i] < count[i]) {    // Check that we haven't reached the end of any of internal arrays.

                        if(colIndex[j] < count[j]) {
                            
                            if(explicitColIndex[i] < explicitColIndex[j]) {                                     // Element with explicitColIndex[i] not present in operand j.
                                resultRow.Add(new SparseElm<T>(operands[i]._E[colIndex[i]]));
                            }
                            else if(explicitColIndex[i] == explicitColIndex[j]){                                // Matching element with explicitColIndex[i] found in operand j.
                                resultRow.Add(operands[i]._E[colIndex[i]] + operands[j]._E[colIndex[j]]);
                                ++colIndex[j];

                                if(colIndex[j] < count[j]) {
                                    explicitColIndex[j] = operands[j]._E[colIndex[j]].ImagIndex();
                                }
                            }
                        }
                        else {              // We have reached end of j-th internal array. Write the rest of i-th elements to result.
                            while(colIndex[i] < count[i]) {
                                resultRow.Add(new SparseElm<T>(operands[i]._E[colIndex[i]]));
                                ++colIndex[i];
                            }
                            return resultRow;
                        }
                        ++colIndex[i];

                        if(colIndex[i] < count[i]) {
                            explicitColIndex[i] = operands[i]._E[colIndex[i]].ImagIndex();        // Explicit index of SparseElement.
                        }
                    }
                    else {                  // We have reached end of i-th internal array. Write the rest of j-th elements to result.
                        while(colIndex[j] < count[j]) {
                            resultRow.Add(new SparseElm<T>(operands[j]._E[colIndex[j]]));
                            ++colIndex[j];
                        }
                        return resultRow;
                    }
                }
            }
            throw new ArithmeticException("Method which sums two SparseRows has reached an invalid state.");
        }

        public static SparseRow<T> operator - (SparseRow<T> left, SparseRow<T> right) {
            Assert.AreEqual(left.Width, right.Width);                     // Check that rows are the same width.
            SparseRow<T>[] operands;
            int[] colIndex = new int[] {0, 0};                                                              // Current true column indices (inside _sparseRows) of operands.
            int[] explicitColIndex;
            int[] count;

            if(left._E[0].ImagIndex() <= right._E[0].ImagIndex()) {                                   // Pack operands in correct order for manipulation. One with smaller first row matrix index comes first.
                operands = new SparseRow<T>[2] {left, right};
                explicitColIndex = new int[] {left._E[0].ImagIndex(), right._E[0].ImagIndex()};
                count = new int[] {left._Count, right._Count};
            }
            else {
                operands = new SparseRow<T>[2] {right, left};
                explicitColIndex = new int[] {right._E[0].ImagIndex(), left._E[0].ImagIndex()};
                count = new int[] {right._Count, left._Count};
            }
            var resultRow = new SparseRow<T>(left._Width, (count[0] > count[1]) ? count[0] : count[1]);    // Result row starts with capacity of larger operand.
            
            for(int i = 0, j = 1; true; i = (i+1) % 2, j = (j+1) % 2) {      // Exchange processed operand in each new iteration.
                
                while(explicitColIndex[i] <= explicitColIndex[j]) {   // Set index where next iteration will kick off. At start of this loop it must hold: colIndex[j] = colIndex[i].
                    
                    if(colIndex[i] < count[i]) {    // Check that we haven't reached the end of any of internal arrays.

                        if(colIndex[j] < count[j]) {
                            
                            if(explicitColIndex[i] < explicitColIndex[j]) {                                     // Element with explicitColIndex[i] not present in operand j.
                                resultRow.Add(new SparseElm<T>(operands[i]._E[colIndex[i]]));
                            }
                            else if(explicitColIndex[i] == explicitColIndex[j]){                                // Matching element with explicitColIndex[i] found in operand j.
                                resultRow.Add(operands[i]._E[colIndex[i]] - operands[j]._E[colIndex[j]]);
                                ++colIndex[j];

                                if(colIndex[j] < count[j]) {
                                    explicitColIndex[j] = operands[j]._E[colIndex[j]].ImagIndex();
                                }
                            }
                        }
                        else {              // We have reached end of j-th internal array. Write the rest of i-th elements to result.
                            while(colIndex[i] < count[i]) {
                                resultRow.Add(new SparseElm<T>(operands[i]._E[colIndex[i]]));
                                ++colIndex[i];
                            }
                            return resultRow;
                        }
                        ++colIndex[i];

                        if(colIndex[i] < count[i]) {
                            explicitColIndex[i] = operands[i]._E[colIndex[i]].ImagIndex();        // Explicit index of SparseElement.
                        }
                    }
                    else {                  // We have reached end of i-th internal array. Write the rest of j-th elements to result.
                        while(colIndex[j] < count[j]) {
                            resultRow.Add(new SparseElm<T>(operands[j]._E[colIndex[j]]));
                            ++colIndex[j];
                        }
                        return resultRow;
                    }
                }
            }
            throw new ArithmeticException("Method which sums two SparseRows has reached an invalid state.");
        }

        /// <summary>Creates a SparseRow that is a dot product of two operand SparseRows.</summary><param name="left">Left operand.</param><param name="right">Right operand.</param><returns>SparseRow that is sum of two operands.</returns>
        public static double operator * (SparseRow<T> left, SparseRow<T> right) {
            
            Assert.AreEqual(left.Width, right.Width);                     // Check that rows are the same width.
            SparseRow<T>[] operands;
            int[] colIndex = new int[] {0, 0};                                                              // Current true column indices (inside _sparseRows) of operands.
            int[] explicitColIndex;
            int[] count;

            if(left._E[0].ImagIndex() <= right._E[0].ImagIndex()) {                                   // Pack operands in correct order for manipulation. One with smaller first row matrix index comes first.
                operands = new SparseRow<T>[2] {left, right};
                explicitColIndex = new int[] {left._E[0].ImagIndex(), right._E[0].ImagIndex()};
                count = new int[] {left._Count, right._Count};
            }
            else {
                operands = new SparseRow<T>[2] {right, left};
                explicitColIndex = new int[] {right._E[0].ImagIndex(), left._E[0].ImagIndex()};
                count = new int[] {right._Count, left._Count};
            }
            double result = 0;
            
            for(int i = 0, j = 1; true; i = (i+1) % 2, j = (j+1) % 2) {      // Exchange processed operand in each new iteration.
                
                while(explicitColIndex[i] <= explicitColIndex[j]) {   // Set index where next iteration will kick off. At start of this loop it must hold: colIndex[j] = colIndex[i].
                    
                    if(colIndex[i] < count[i]) {    // Check that we haven't reached the end of any of internal arrays.

                        if(colIndex[j] < count[j]) {
                            
                            if(explicitColIndex[i] == explicitColIndex[j]){                                // Matching element with explicitColIndex[i] found in operand j.
                                result += (dynamic) operands[i].E(colIndex[i]).Value * operands[j].E(colIndex[j]).Value;
                                ++colIndex[j];

                                if(colIndex[j] < count[j]) {
                                    explicitColIndex[j] = operands[j]._E[colIndex[j]].ImagIndex();
                                }
                            }
                        }
                        else {                  // When we reach end of one of columns, stop.
                            return result;
                        }
                        ++colIndex[i];

                        if(colIndex[i] < count[i]) {
                            explicitColIndex[i] = operands[i]._E[colIndex[i]].ImagIndex();        // Explicit index of SparseElement.
                        }
                    }
                    else {                      // When we reach end of one of columns, stop.
                        return result;
                    }
                }
            }
            throw new ArithmeticException("Method which sums two SparseRows has reached an invalid state.");
        }

        public static SparseRow<T> operator * (double left, SparseRow<T> right) {

            var resultRow = new SparseRow<T>(right);

            for(int i = 0; i < right._Count; ++i) {
                resultRow.E(i).Value = (dynamic) left * right.E(i).Value;
            }
            return resultRow;
        }

        public double CalcNorm() {
            double result = 0.0;

            for(int i = 0; i < _Count; ++i) {
                result += Pow((dynamic)E(i).Value, 2.0);
            }
            return result;
        }

        /// <summary>Swap values of two elements specified by internal indices.</summary><param name="index1">First internal index.</param><param name="index2">Second internal index.</param>
        public void SwapElementsInternal(int index1, int index2) {
            T firstValue = _E[index1].Value;
            _E[index1].Value = _E[index2].Value;
            _E[index2].Value = firstValue;
        }

        /// <summary>Swap two elements specified by explicit indices.</summary><param name="explIndex1">Explicit index of first element.</param><param name="explIndex2">Explicit index of second element.</param><remarks>Useful for swapping columns.</remarks>
        public void SwapElementsExplicit(int explIndex1, int explIndex2) {
            T firstValue = this[explIndex1];
            T secondValue = this[explIndex2];

            if(!SparseElm<T>._EqualityComparer.Equals(secondValue, default(T))) {         // If not zero.
                this[explIndex1] = secondValue;
            }

            if(!SparseElm<T>._EqualityComparer.Equals(firstValue, default(T))) {
                this[explIndex2] = firstValue;
            }
        }

        /// <summary>Apply element swaps as specified by a given swap matrix.</summary><param name="swapMatrix">SparseMatrix where non-zero element at [i][j] signifies a permutation i --> j.</param>
        public void ApplySwaps(SparseMat<int> swapMatrix) {
            Assert.AreEqual(swapMatrix.GetWidth(), Width, SCG.EqualityComparer<int>.Default);          // Check that swap matrix dimensions are appropriate for this SparseRow.
            Assert.AreEqual(swapMatrix.GetHeight(), Width, SCG.EqualityComparer<int>.Default);
            int swapCount = swapMatrix.Count;                                                                   // Actual number of elements (SparseMatrixRows) in swapMatrix.

            for(int i = 0; i < swapCount; ++i) {
                int element1 = swapMatrix[i].GetExplicitIndex();
                int element2 = swapMatrix[i].E(0).ImagIndex();                                         // Each row is expected to have one element.
                SwapElementsExplicit(element1, element2);
            }
        }

        /// <summary>Splits SparseRow in two SparseRows. This SparseRow is modified to be left remainder, while right remainder is returned as result.</summary><param name="explicitIndex">Index at which to split. Element at this index will be part of right remainder.</param>
        public SparseRow<T> SplitAt(int explicitIndex) {
            var splitSuccessfull = SetRecentIndexToOrAheadOf(explicitIndex);
            SparseRow<T> removedPart;
            if(splitSuccessfull) {
                removedPart = RemoveRange(_RecentIndex, _E.Length - 1);      // Remove every element from specified point onwards.
                TrimExcessSpace();
            }
            else {                                                                      // No elements to trim away.
                _Width = explicitIndex;                                                 // Readjust width.
                removedPart = new SparseRow<T>(_Width - explicitIndex, 6);          // Return an empty SparseRow with width of trimming.
            }
            return removedPart;
        }

        /// <summary>Sets recent index to element with given explicit index. If element with specified explicit index does not exist, it sets recent index to first element ahead of specified explicit index.</summary><param name="explicitIndex">Explicit index of desired element.</param>
        protected bool SetRecentIndexToOrAheadOf(int explicitIndex) {
            Assert.IndexInRange(explicitIndex, 0, Width);
                
            if(_Count != 0) {
                ValidateIndex(ref _RecentIndex);
                int index = _RecentIndex;
                int recentExplicitIndex = E(index).ImagIndex();

                if(recentExplicitIndex <= explicitIndex) {              // Move forward until you reach end.
                    while(index < _Count && E(index).ImagIndex() <= explicitIndex) {

                        if(E(index).ImagIndex() == explicitIndex) {                        // Try to find an existing entry to return.
                            _RecentIndex = index;
                            return true;
                        }
                        ++index;
                    }
                    _RecentIndex = index - 1;
                    return false;
                }
                else {                                                  // Move backward until you reach end.
                    while(index > -1 && E(index).ImagIndex() >= explicitIndex) {

                        if(E(index).ImagIndex() == explicitIndex) {                        // Try to find an existing entry to return.
                            _RecentIndex = index;
                            return true;
                        }
                        --index;
                    }
                    _RecentIndex = index + 1;
                    return false;
                }
            }
            else {
                return false;
            }
        }

        /// <summary>Removes specified range from SparseRow and returns it. Correctly adjusts width. Range is specified in terms of internal indices, not explicit ones.</summary><param name="j">Inclusive starting index.</param><param name="k">Inclusive ending index.</param>
        new public SparseRow<T> RemoveRange(int j, int k) {
            int removedCount = k - j + 1;
            SparseElm<T>[] removed = new SparseElm<T>[removedCount];
            
            for(int i = 0; i < removedCount; ++i) {             // Construct array that we will return.
                BeforeElementExit(i);
                removed[i] = _E[j + i];
            }
            for(int i = k + 1; i < _Count; ++i) {               // Refill hole. Shift elements remaining on right side of hole (removed range) to right.
                BeforeElementExit(i);
                _E[i - removedCount] = _E[i];
                AfterElementEntry(i - removedCount);
            }
            _Count = _Count - removedCount;                     // Changing count, no need to zero elements at end.
            _Width = _E[_Count].ImagIndex() + 1;
            return CreateFromArray(removed);
        }

        /// <summary>Append specified SparseRow to this one.</summary><param name="rightPart">Append this SparseRow.</param>
        public void MergeWith(SparseRow<T> rightPart) {
            _Width += rightPart._Width;                         // Readjust width.
            AddRange(rightPart);
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