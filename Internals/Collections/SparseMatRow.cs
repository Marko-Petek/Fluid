using System;
using System.Text;

using Fluid.Internals.Development;

namespace Fluid.Internals.Collections
{
    public class SparseMatRow<T> : SparseRow<T>, IEquatable<SparseMatRow<T>>
    where T : struct, IEquatable<T>
    {
        /// <summary>Array that contains SparseRow.</summary>
        SparseMat<T> _sparseMatrix;
        /// <summary>Index of SparseMatrixRow inside internal list of SparseMatrix.</summary>
        int             _index;
        /// <summary>Row index at which row would be situated at in a matrix. Is -1 if this instance doesn't belong to any matrix.</summary>
        int             _explicitIndex;
        /// <summary>Signals whether this is an existing SparseMatrixRow or just a static dummy representing a zero row.</summary>
        bool _isDummy = false;

        public void SetSparseMatrix(SparseMat<T> sparseMatrix) => _sparseMatrix = sparseMatrix;
        /// <summary>Index of SparseMatrixRow inside internal list of SparseMatrix.</summary>
        public int GetIndex()                           =>  _index;
        /// <summary>Set index of SparseMatrixRow inside internal list of SparseMatrix.</summary><param name="index">Index of SparseMatrixRow inside internal list of SparseMatrix.</param>
        public void SetIndex(int index)                           =>  _index = index;
        /// <summary>Index at which row would be situated at in a matrix.</summary>
        public int GetExplicitIndex()                   =>  _explicitIndex;
        /// <summary>Set index at which row would be situated at in a matrix.</summary>
        public void SetExplicitIndex(int explicitIndex)  =>  _explicitIndex = explicitIndex;
        /// <summary>Signals whether this is an existing SparseMatrixRow or just a static dummy representing a zero row.</summary>
        public bool GetIsDummy() => _isDummy;
        /// <summary>Set flag that signals whether this is an existing SparseMatrixRow or just a static dummy representing a zero row.</summary>
        public void SetIsDummy(bool isDummy) => _isDummy = isDummy;

        /// <summary>Create SparseMatrixRow with unspecified matrix index (negative).</summary><param name="sparseMatrix">SparseRow's owner.</param><param name="explicitIndex">Row index.</param>
        public SparseMatRow(int width) : base(width) {
            _index = -1;
            _explicitIndex = -1;
        }
        /// <summary>Create SparseMatrixRow with specified matrix index.</summary><param name="sparseMatrix">SparseRow's owner.</param><param name="explicitIndex">Row index.</param>
        public SparseMatRow(int width, int explicitIndex) : base(width) {
            _explicitIndex = explicitIndex;
        }
        /// <summary>Create SparseRow with specified index and add a specified element to it.</summary><param name="sparseMatrix">SparseRow's owner.</param><param name="explicitIndex">Row index inside matrix.</param><param name="firstElement">Initial SparseElement.</param>
        public SparseMatRow(int width, int explicitIndex, SparseElm<T> firstElement) :
        this(width, explicitIndex) {
            Add(firstElement);
        }
        /// <summary>Create a copy of SparseMatrixRow with identical owner.</summary><param name="sourceRow">SparseMatrixRow whose owner will also be assigned as owner of new SparseMatrixRow.</param>
        public SparseMatRow(SparseMatRow<T> sourceMatrixRow) :
        this(sourceMatrixRow._Width, sourceMatrixRow._explicitIndex) {
            _sparseMatrix = sourceMatrixRow._sparseMatrix;                  // Set same owner.
            AddRange(sourceMatrixRow);
        }
        /// <summary>Create a SparseMatrixRow out of a regular SparseRow. It has null owner and negative explicitIndex.</summary><param name="sourceRow">Source SparseRow to copy elements from.</param><param name="ownerMatrix">SparseMatrix which will be specified as owner of SparseMatrixRow.</param><param name="explicitIndex">Index that row would have in a writen-out matrix.</param>
        public SparseMatRow(SparseRow<T> sourceRow, int explicitIndex) : this(sourceRow.Width, explicitIndex) {
            AddRange(sourceRow);
        }

        /// <summary>Create a new SparseRow by adopting specified source array.</summary><param name="source">Source array to adopt.</param>
        new public SparseMatRow<T> CreateFromArray(SparseElm<T>[] source) {
            var row = new SparseMatRow<T>(source.Length);
            row._E = source;
            
            for(int i = 0; i < source.Length; ++i) {                            // Remember to reset indices.
                AfterElementEntry(i);
            }
            return row;
        }


        /// <summary>Retrieves an element with given matrix index (0 if not present), or sets it (creates if not present).</summary>
        public override T this[int explicitIndex] {
            get {
                if(_Count != 0) {
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
                        _RecentIndex = (index < _Count) ? index : (_Count-1);
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
                var sparseMatrixRow = this;                                 
                
                if(_isDummy) {                                                                // Indicates that this row is not part of a matrix. (Indexer of SparseMatrix returns a static SparseMatrixRow with index -1 if row does not exist in matrix.)
                    if(!SparseElm<T>._EqualityComparer.Equals(value, default(T))) {      // Create new row only if provided value is not zero.  
                        sparseMatrixRow = new SparseMatRow<T>(this);     // Create an actual row from static dummy.
                        _RecentIndex = _index;
                        _sparseMatrix.Insert(_index, sparseMatrixRow);                           // Insertion index has been passed from the getter on SparseMatrix.
                    }
                    else {          // Row does not exist anyway, job done.
                        _RecentIndex = _index;
                        return;
                    }
                }
                int insertIndex = sparseMatrixRow.Count;                   // If dummy has been inserted, it is empty. Next if loop will not execute, insertIndex has to be accurate here.
                
                if(_Count != 0) {
                    ValidateIndex(ref _RecentIndex);
                    int index = _RecentIndex;
                    int recentExplicitIndex = E(index).ImagIndex();

                    if(recentExplicitIndex <= explicitIndex) {              // Move forward until you reach end.
                        while(index < _Count && E(index).ImagIndex() <= explicitIndex) {          // Find element, delete if value provided is zero.

                            if(E(index).ImagIndex() == explicitIndex) {                        // Try to find an existing entry to modify.
                                if(SparseElm<T>._EqualityComparer.Equals(value, default(T))) {
                                    RemoveAt(index);                                                // If zero is provided, simply remove SparseElement.
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
                        _RecentIndex = index;
                    }
                    else {                                                  // Move backward until you reach end.
                        while(index > -1 && E(index).ImagIndex() >= explicitIndex) {

                            if(E(index).ImagIndex() == explicitIndex) {                        // Try to find an existing entry to modify.
                                if(SparseElm<T>._EqualityComparer.Equals(value, default(T))) {
                                    RemoveAt(index);                                                // If zero is provided, simply remove SparseElement.
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
                        sparseMatrixRow.Insert(insertIndex, new SparseElm<T>(insertIndex, explicitIndex, value));  // Count = 0, end has been reached or SparseElement at index now has explicit index above the desired one. Insert a freshly created element.
                }
            }
        }

        /// <summary>Creates a SparseMatrixRow that is a sum of two operand SparseMatrixRows.</summary><param name="left">Left operand.</param><param name="right">Right operand.</param>
        public static SparseMatRow<T> operator + (SparseMatRow<T> left, SparseMatRow<T> right) {
            
            var resultRow = (SparseRow<T>)left + (SparseRow<T>)right;
            return new SparseMatRow<T>(resultRow, left._explicitIndex);     // Owner null = unknown.
        }

        public bool Equals(SparseMatRow<T> other) {
            var thisRow = (SparseRow<T>)this;
            var otherRow = (SparseRow<T>)other;
            return (thisRow.Equals(otherRow));
        }

        /// <summary>Splits SparseRow in two SparseRows. This SparseRow is modified to be left remainder, while right remainder is returned as result.</summary><param name="explicitIndex">Index at which to split. Element at this index will be part of right remainder.</param>
        new public SparseMatRow<T> SplitAt(int explicitIndex) {
            SetRecentIndexToOrAheadOf(explicitIndex);
            TrimExcessSpace();
            return RemoveRange(_RecentIndex, _E.Length - 1);      // Remove every element from specified point onwards.
        }

        /// <summary>Removes specified range from List and returns it.</summary><param name="j">Inclusive starting index.</param><param name="k">Inclusive ending index.</param>
        new public SparseMatRow<T> RemoveRange(int j, int k) {
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

        /// <summary>Create a string of form (explicitRowIndex, sparseElement 0, sparseElement 1, ..., sparseElement N-1).</summary>
        public override string ToString() {
            StringBuilder sb = new StringBuilder(72);
            sb.Append($"{{{_explicitIndex}: ");

            for(int i = 0; i < _Count - 1; ++i) {
                sb.Append($"{_E[i].ToString()}, ");
            }
            sb.Append($"{_E[_Count - 1].ToString()}}}");
            return sb.ToString();
        }
    }
}