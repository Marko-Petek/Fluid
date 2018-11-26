using System;
using System.Text;
using SCG = System.Collections.Generic;
using static System.Math;

using Fluid.Internals;
using Fluid.Internals.Collections;
using Fluid.Internals.Development;
using static Fluid.Internals.Operations;

namespace Fluid.Internals.Collections
{
    /// <summary>A memeber of SparseArray.</summary><remarks>Concrete class for Int32 was created for speed because a statically typed mechanism for arithmetic inside generics did not exist at the time of writing.</remarks>
    public class SparseRowInt : EquatableManagedList<SparseElementInt>
    {
        /// <summary>Default value of an element.</summary>
        readonly int _DefaultValue;
        /// <summary>Default value of an element.</summary>
        public int DefaultValue => _DefaultValue;
        /// <summary>Width of row if written out explicitly.</summary>
        protected int _Width;
        /// <summary>Width of row if written out explicitly.</summary>
        public int Width => _Width;
        /// <summary>Real index of most recently manipulated (get or set) SparseElement value.</summary>
        protected int _RecentRealIndex = 0;
        /// <summary>Real index of most recently manipulated (get or set) SparseElement value.</summary>
        public int RecentRealIndex { get => _RecentRealIndex; protected set => _RecentRealIndex = value; }
        /// <summary>Imaginary index of most recently manipulated (get or set) SparseElement value.</summary>
        protected int RecentImagIndex => E(_RecentRealIndex).ImagIndex;


        /// <summary>Create a SparseRow with specified width it would have in explicit form, specified default value and specified initial capacity.</summary><param name="width">Width it would have in explicit form.</param>
        public SparseRowInt(int width, int defaultValue, int capacity) : base(capacity) {
            _DefaultValue = defaultValue;
            _Width = width;
        }
        /// <summary>Create a SparseRow with specified width it would have in explicit form and specified initial capacity.</summary><param name="width">Width it would have in explicit form.</param>
        public SparseRowInt(int width, int capacity) : this(width, 0, capacity) {
        }
        /// <summary>Create a SparseRow with specified width it would have in explicit form and default initial capacity.</summary><param name="width">Width it would have in explicit form.</param>
        public SparseRowInt(int width) : this(width, 6) {
        }
        /// <summary>Create a copy of specified SparseRow.</summary><param name="source">Source SparseRow to copy.</param>
        public SparseRowInt(SparseRowInt source) : base(source) {
            _DefaultValue = source._DefaultValue;
            _Width = source._Width;
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
        new public SparseRowInt CreateFromArray(SparseElementInt[] source) {
            int lastImagIndex = source[source.Length - 1].ImagIndex;
            var row = new SparseRowInt(lastImagIndex + 1);
            row._E = source;
            
            for(int i = 0; i < source.Length; ++i) {                            // Remember to reset indices.
                AfterElementEntry(i);
            }
            return row;
        }

        /// <summary>Add an element to end of SparseRow and return its explicit index.</summary><param name="element">Element to be added.</param>
        public int Add(int element) {
            if(Count == 0) {
                Add(new SparseElementInt(0, 0, element));
                _RecentRealIndex = 0;
                return 0;
            }
            else {
                int newImagIndex =  E(Count - 1).ImagIndex + 1;
                Add(new SparseElementInt(Count, newImagIndex, element));
                return newImagIndex;
            }
        }

        /// <summary>Retrieves an element with given matrix index (0 if not present), or sets it (creates if not present).</summary>
        new public virtual int this[int imagIndex] {
            get {
                Assert.IndexInRange(imagIndex, 0, Width);
                
                if(Count != 0) {
                    ValidateIndex(ref _RecentRealIndex);
                    int i = RecentRealIndex;

                    if(RecentImagIndex <= imagIndex) {              // Move forward until you reach end.
                        while(i < Count && _E[i].ImagIndex <= imagIndex) {

                            if(_E[i].ImagIndex == imagIndex) {                        // Try to find an existing entry to return.
                                RecentRealIndex = i;
                                return _E[i].Value;
                            }
                            ++i;
                        }
                        RecentRealIndex = i - 1;
                    }
                    else {                                                  // Move backward until you reach end.
                        while(i > -1 && _E[i].ImagIndex >= imagIndex) {

                            if(_E[i].ImagIndex == imagIndex) {                        // Try to find an existing entry to return.
                                RecentRealIndex = i;
                                return _E[i].Value;
                            }
                            --i;
                        }
                        RecentRealIndex = i + 1;
                    }
                }
                return 0;
            }
            set {
                Assert.IndexInRange(imagIndex, 0, Width);
                int insertIndex = 0;

                if(Count != 0) {
                    ValidateIndex(ref _RecentRealIndex);
                    int i = RecentRealIndex;

                    if(RecentImagIndex <= imagIndex) {
                        while(i < Count && _E[i].ImagIndex <= imagIndex) {

                            if(_E[i].ImagIndex == imagIndex) {                        // Try to find an existing entry to modify.
                                if(value == 0)
                                    RemoveAt(i);
                                else
                                    _E[i].Value = value;

                                RecentRealIndex = i;
                                return;
                            }
                            ++i;
                        }
                        insertIndex = i;
                        RecentRealIndex = i - 1;
                    }
                    else {
                        while(i > -1 && _E[i].ImagIndex >= imagIndex) {

                            if(_E[i].ImagIndex == imagIndex) {                        // Try to find an existing entry to modify.
                                if(value == 0)
                                    RemoveAt(i);
                                else
                                    _E[i].Value = value;

                                _RecentRealIndex = i;
                                return;
                            }
                            --i;
                        }
                        insertIndex = i + 1;
                        _RecentRealIndex = insertIndex;
                    }
                }

                if(value != 0) {
                    Insert(insertIndex, new SparseElementInt(insertIndex, imagIndex, value));                // Count = 0 or end has been reached, add fresh element.
                }
            }
        }

        /// <summary>Creates a SparseRow that is a sum of two operand SparseRows.</summary><param name="left">Left operand.</param><param name="right">Right operand.</param><returns>SparseRow that is sum of two operands.</returns>
        public static SparseRowInt operator + (SparseRowInt left, SparseRowInt right) {
            
            Assert.AreEqual(left.Width, right.Width, Comparers.Int);                     // Check that rows are the same width.
            SparseRowInt[] operands;
            int[] realColIndex = new int[] {0, 0};                                                              // Current true column indices (inside _sparseRows) of operands.
            int[] imagColIndex;
            int[] count;

            if(left._E[0].ImagIndex <= right._E[0].ImagIndex) {                                   // Pack operands in correct order for manipulation. One with smaller first row matrix index comes first.
                operands = new SparseRowInt[2] {left, right};
                imagColIndex = new int[] {left._E[0].ImagIndex, right._E[0].ImagIndex};
                count = new int[] {left.Count, right.Count};
            }
            else {
                operands = new SparseRowInt[2] {right, left};
                imagColIndex = new int[] {right._E[0].ImagIndex, left._E[0].ImagIndex};
                count = new int[] {right.Count, left.Count};
            }
            var resultRow = new SparseRowInt(left.Width, (count[0] > count[1]) ? count[0] : count[1]);    // Result row starts with capacity of larger operand.
            int i = 0;
            int j = 1;

            while(true) {
                
                while(imagColIndex[i] <= imagColIndex[j]) {   // Set index where next iteration will kick off. At start of this loop it must hold: colIndex[j] = colIndex[i].
                    
                    if(realColIndex[i] < count[i]) {    // Check that we haven't reached the end of any of internal arrays.

                        if(realColIndex[j] < count[j]) {
                            
                            if(imagColIndex[i] < imagColIndex[j]) {                                     // Element with explicitColIndex[i] not present in operand j.
                                resultRow.Add(new SparseElementInt(operands[i]._E[realColIndex[i]]));
                            }
                            else if(imagColIndex[i] == imagColIndex[j]){                                // Matching element with explicitColIndex[i] found in operand j.
                                resultRow.Add(operands[i]._E[realColIndex[i]] + operands[j]._E[realColIndex[j]]);
                                ++realColIndex[j];

                                if(realColIndex[j] < count[j]) {
                                    imagColIndex[j] = operands[j]._E[realColIndex[j]].ImagIndex;
                                }
                            }
                        }
                        else {              // We have reached end of j-th internal array. Write the rest of i-th elements to result.
                            while(realColIndex[i] < count[i]) {
                                resultRow.Add(new SparseElementInt(operands[i]._E[realColIndex[i]]));
                                ++realColIndex[i];
                            }
                            return resultRow;
                        }
                        ++realColIndex[i];

                        if(realColIndex[i] < count[i]) {
                            imagColIndex[i] = operands[i]._E[realColIndex[i]].ImagIndex;        // Explicit index of SparseElement.
                        }
                    }
                    else {                  // We have reached end of i-th internal array. Write the rest of j-th elements to result.
                        while(realColIndex[j] < count[j]) {
                            resultRow.Add(new SparseElementInt(operands[j]._E[realColIndex[j]]));
                            ++realColIndex[j];
                        }
                        return resultRow;
                    }
                }
                i = (i+1) % 2;                                      // Exchange processed operand in each new iteration.
                j = (j+1) % 2;
            }
            throw new ArithmeticException("Method which sums two SparseRows has reached an invalid state.");
        }

        public static SparseRowInt operator - (SparseRowInt left, SparseRowInt right) {
            Assert.AreEqual(left.Width, right.Width, Comparers.Int);                     // Check that rows are the same width.
            SparseRowInt[] operands;
            int[] realColIndex = new int[] {0, 0};                                                              // Current true column indices (inside _sparseRows) of operands.
            int[] imagColIndex;
            int[] count;

            if(left._E[0].ImagIndex <= right._E[0].ImagIndex) {                                   // Pack operands in correct order for manipulation. One with smaller first row matrix index comes first.
                operands = new SparseRowInt[2] {left, right};
                imagColIndex = new int[] {left._E[0].ImagIndex, right._E[0].ImagIndex};
                count = new int[] {left.Count, right.Count};
            }
            else {
                operands = new SparseRowInt[2] {right, left};
                imagColIndex = new int[] {right._E[0].ImagIndex, left._E[0].ImagIndex};
                count = new int[] {right.Count, left.Count};
            }
            var resultRow = new SparseRowInt(left.Width, (count[0] > count[1]) ? count[0] : count[1]);    // Result row starts with capacity of larger operand.
            int i = 0;
            int j = 1;

            while(true) {
                
                while(imagColIndex[i] <= imagColIndex[j]) {   // Set index where next iteration will kick off. At start of this loop it must hold: colIndex[j] = colIndex[i].
                    
                    if(realColIndex[i] < count[i]) {    // Check that we haven't reached the end of any of internal arrays.

                        if(realColIndex[j] < count[j]) {
                            
                            if(imagColIndex[i] < imagColIndex[j]) {                                     // Element with explicitColIndex[i] not present in operand j.
                                resultRow.Add(new SparseElementInt(operands[i]._E[realColIndex[i]]));
                            }
                            else if(imagColIndex[i] == imagColIndex[j]){                                // Matching element with explicitColIndex[i] found in operand j.
                                resultRow.Add(operands[i]._E[realColIndex[i]] - operands[j]._E[realColIndex[j]]);
                                ++realColIndex[j];

                                if(realColIndex[j] < count[j]) {
                                    imagColIndex[j] = operands[j]._E[realColIndex[j]].ImagIndex;
                                }
                            }
                        }
                        else {              // We have reached end of j-th internal array. Write the rest of i-th elements to result.
                            while(realColIndex[i] < count[i]) {
                                resultRow.Add(new SparseElementInt(operands[i]._E[realColIndex[i]]));
                                ++realColIndex[i];
                            }
                            return resultRow;
                        }
                        ++realColIndex[i];

                        if(realColIndex[i] < count[i]) {
                            imagColIndex[i] = operands[i]._E[realColIndex[i]].ImagIndex;        // Explicit index of SparseElement.
                        }
                    }
                    else {                  // We have reached end of i-th internal array. Write the rest of j-th elements to result.
                        while(realColIndex[j] < count[j]) {
                            resultRow.Add(new SparseElementInt(operands[j]._E[realColIndex[j]]));
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
        public static double operator * (SparseRowInt left, SparseRowInt right) {
            
            Assert.AreEqual(left.Width, right.Width, Comparers.Int);                     // Check that rows are the same width.
            SparseRowInt[] operands;
            int[] realColIndex = new int[] {0, 0};                                                              // Current true column indices (inside _sparseRows) of operands.
            int[] imagColIndex;
            int[] count;

            if(left._E[0].ImagIndex <= right._E[0].ImagIndex) {                                   // Pack operands in correct order for manipulation. One with smaller first row matrix index comes first.
                operands = new SparseRowInt[2] {left, right};
                imagColIndex = new int[] {left._E[0].ImagIndex, right._E[0].ImagIndex};
                count = new int[] {left.Count, right.Count};
            }
            else {
                operands = new SparseRowInt[2] {right, left};
                imagColIndex = new int[] {right._E[0].ImagIndex, left._E[0].ImagIndex};
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
                                    imagColIndex[j] = operands[j]._E[realColIndex[j]].ImagIndex;
                                }
                            }
                        }
                        else {                  // When we reach end of one of columns, stop.
                            return result;
                        }
                        ++realColIndex[i];

                        if(realColIndex[i] < count[i]) {
                            imagColIndex[i] = operands[i]._E[realColIndex[i]].ImagIndex;        // Explicit index of SparseElement.
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

        public static SparseRowInt operator * (int left, SparseRowInt right) {

            var resultRow = new SparseRowInt(right);

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
        public void ApplySwaps(SparseMatrixInt swapMatrix) {
            Assert.AreEqual(swapMatrix.Width, Width);          // Check that swap matrix dimensions are appropriate for this SparseRow.
            Assert.AreEqual(swapMatrix.Height, Width);
            int swapCount = swapMatrix.Count;                                                                   // Actual number of elements (SparseMatrixRows) in swapMatrix.

            for(int i = 0; i < swapCount; ++i) {
                int element1 = swapMatrix[i].ImagIndex;
                int element2 = swapMatrix[i]._E[0].ImagIndex;                                         // Each row is expected to have one element.
                SwapImagElements(element1, element2);
            }
        }

        /// <summary>Splits SparseRow in two SparseRows. This SparseRow is modified to be left remainder, while right remainder is returned as result.</summary><param name="imagIndex">Index at which to split. Element at this index will be part of right remainder.</param>
        public SparseRowInt SplitAt(int imagIndex) {
            var splitSuccess = SetRecentIndexToOrAheadOf(imagIndex);
            SparseRowInt removedCols;
            if(splitSuccess) {
                removedCols = RemoveRange(RecentRealIndex, _E.Length - 1);      // Remove every element from specified point onwards.
                TrimExcessSpace();
            }
            else {                                                                      // No elements to trim away.
                _Width = imagIndex;                                                 // Readjust width.
                removedCols = new SparseRowInt(Width - imagIndex, 6);          // Return an empty SparseRow with width of trimming.
            }
            return removedCols;
        }

        /// <summary>Sets recent index to element with given imaginary index. If element with specified explicit index does not exist, it sets recent index to first element ahead of specified explicit index.</summary><param name="imagIndex">Explicit index of desired element.</param>
        protected bool SetRecentIndexToOrAheadOf(int imagIndex) {
            Assert.IndexInRange(imagIndex, 0, Width);
                
            if(Count != 0) {
                ValidateIndex(ref _RecentRealIndex);
                int realIndex = _RecentRealIndex;

                if(RecentImagIndex <= imagIndex) {              // Move forward until you reach end.
                    while(realIndex < _Count && _E[realIndex].ImagIndex <= imagIndex) {

                        if(_E[realIndex].ImagIndex == imagIndex) {                        // Try to find an existing entry to return.
                            RecentRealIndex = realIndex;
                            return true;
                        }
                        ++realIndex;
                    }
                    RecentRealIndex = realIndex - 1;
                    return false;
                }
                else {                                                  // Move backward until you reach end.
                    while(realIndex > -1 && _E[realIndex].ImagIndex >= imagIndex) {

                        if(_E[realIndex].ImagIndex == imagIndex) {                        // Try to find an existing entry to return.
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
        new public SparseRowInt RemoveRange(int j, int k) {
            int removedCount = k - j + 1;
            SparseElementInt[] removed = new SparseElementInt[removedCount];
            
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
            _Width = _E[Count].ImagIndex + 1;
            return CreateFromArray(removed);
        }

        /// <summary>Append specified SparseRow to this one.</summary><param name="rightCols">Append this SparseRow.</param>
        public void MergeWith(SparseRowInt rightCols) {
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