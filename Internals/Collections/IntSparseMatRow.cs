using System;
using System.Text;

using Fluid.Internals.Collections;
using Fluid.Internals.Development;

namespace Fluid.Internals.Collections
{
    public class IntSparseMatRow : IntSparseRow, IEquatable<IntSparseMatRow>
    {
        /// <summary>SparseMatrix that contains SparseMatrixRow.</summary>
        IntSparseMat _SparseMatrix;
        /// <summary>SparseMatrix that contains SparseMatrixRow.</summary>
<<<<<<< HEAD:Internals/Collections/IntSparseMatRow.cs
        public IntSparseMat SparseMatrix {set => _SparseMatrix = value; }
        /// <summary>Default value of an element.</summary>
        public int DefaultValue => _SparseMatrix.DefaultValue;
=======
        public SparseMatrixInt SparseMatrix {set => _SparseMatrix = value; }
>>>>>>> 6c8637650b1435c58d4385216c44e1c2a3d4b4bf:Internals/Collections/SparseMatrixRowInt.cs
        /// <summary>Index of SparseMatrixRow inside internal list of SparseMatrix.</summary>
        int _RealIndex;
        /// <summary>Index of SparseMatrixRow inside internal list of SparseMatrix.</summary>
        public int RealIndex { get => _RealIndex; set => _RealIndex = value; }
        /// <summary>Row index at which row would be situated at in a matrix. Is -1 if this instance doesn't belong to any matrix.</summary>
		int _VirtIndex;
        /// <summary>Row index at which row would be situated at in a matrix. Is -1 if this instance doesn't belong to any matrix.</summary>
		public int VirtIndex { get => _VirtIndex; set => _VirtIndex = value; }
        /// <summary>Signals whether this is an existing SparseMatrixRow or just a static dummy representing a zero row.</summary>
        bool _IsDummy = false;
        /// <summary>Signals whether this is an existing SparseMatrixRow or just a static dummy representing a zero row.</summary>
        public bool IsDummy { get => _IsDummy; set => _IsDummy = value; }

        
        // TODO: Check that DefaultValue mechanism is implemented accross all Sparse classes.
        /// <summary>Create SparseMatrixRow with unspecified matrix index (negative).</summary><param name="sparseMatrix">SparseRow's owner.</param><param name="explicitIndex">Row index.</param>
<<<<<<< HEAD:Internals/Collections/IntSparseMatRow.cs
        public IntSparseMatRow(int width) : base(width) {
=======
        public SparseMatrixRowInt(int width) : base(width, 6, 0) {
>>>>>>> 6c8637650b1435c58d4385216c44e1c2a3d4b4bf:Internals/Collections/SparseMatrixRowInt.cs
            _RealIndex = -1;
            _VirtIndex = -1;
        }
        /// <summary>Create SparseMatrixRow with specified matrix index.</summary><param name="sparseMatrix">SparseRow's owner.</param><param name="imagIndex">Row index.</param>
<<<<<<< HEAD:Internals/Collections/IntSparseMatRow.cs
        public IntSparseMatRow(int width, int imagIndex) : base(width) {
            _VirtIndex = imagIndex;
        }
        /// <summary>Create SparseRow with specified index and add a specified element to it.</summary><param name="sparseMatrix">SparseRow's owner.</param><param name="imagIndex">Row index inside matrix.</param><param name="firstElement">Initial SparseElement.</param>
        public IntSparseMatRow(int width, int imagIndex, IntSparseElm firstElement) :
=======
        public SparseMatrixRowInt(int width, int imagIndex, int defaultValue = 0, int capacity = 6) : base(width, defaultValue, capacity) {
            _ImagIndex = imagIndex;
        }
        /// <summary>Create SparseRow with specified index and add a specified element to it.</summary><param name="sparseMatrix">SparseRow's owner.</param><param name="imagIndex">Row index inside matrix.</param><param name="firstElement">Initial SparseElement.</param>
        public SparseMatrixRowInt(int width, int imagIndex, SparseElementInt firstElement, int defaultValue = 0) :
>>>>>>> 6c8637650b1435c58d4385216c44e1c2a3d4b4bf:Internals/Collections/SparseMatrixRowInt.cs
        this(width, imagIndex) {
            Add(firstElement);
        }
        /// <summary>Create a copy of SparseMatrixRow with identical owner.</summary><param name="sourceRow">SparseMatrixRow whose owner will also be assigned as owner of new SparseMatrixRow.</param>
        public IntSparseMatRow(IntSparseMatRow source) :
        this(source.Width, source.VirtIndex) {
            _SparseMatrix = source._SparseMatrix;                  // Set same owner.
            AddRange(source);
        }
        /// <summary>Create a SparseMatrixRow out of a regular SparseRow. It has null owner and negative explicitIndex.</summary><param name="source">Source SparseRow to copy elements from.</param><param name="ownerMatrix">SparseMatrix which will be specified as owner of SparseMatrixRow.</param><param name="imagIndex">Index that row would have in a writen-out matrix.</param>
        public IntSparseMatRow(IntSparseRow source, int imagIndex) : this(source.Width, imagIndex) {
            AddRange(source);
        }

        /// <summary>Create a new SparseRow by adopting specified source array.</summary><param name="source">Source array to adopt.</param>
        new public IntSparseMatRow CreateFromArray(IntSparseElm[] source) {
            var row = new IntSparseMatRow(source.Length);
            row._E = source;
            
            for(int i = 0; i < source.Length; ++i) {                            // Remember to reset indices.
                AfterElementEntry(i);
            }
            return row;
        }


        /// <summary>Retrieves an element with given matrix index (0 if not present), or sets it (creates if not present).</summary>
        public override int this[int imagIndex] {
            get {
                if(Count != 0) {
                    ValidateIndex(ref _RecentRealIndex);
                    int realIndex = RecentRealIndex;
                
                    if(RecentVirtIndex <= imagIndex) {              // Move forward until you reach end.
                        
                        while(realIndex < Count && _E[realIndex].VirtIndex <= imagIndex) {

                            if(_E[realIndex].VirtIndex == imagIndex) {                        // Try to find an existing entry to return.
                                RecentRealIndex = realIndex;
                                return _E[realIndex].Value;
                            }
                            ++realIndex;
                        }
                        RecentRealIndex = (realIndex < Count) ? realIndex : (Count-1);
                    }
                    else {                                                  // Move backward until you reach end.
                        
                        while(realIndex > -1 && _E[realIndex].VirtIndex >= imagIndex) {

                            if(_E[realIndex].VirtIndex == imagIndex) {                        // Try to find an existing entry to return.
                                RecentRealIndex = realIndex;
                                return _E[realIndex].Value;
                            }
                            --realIndex;
                        }
                        RecentRealIndex = realIndex + 1;
                    }
                }
                return DefaultValue;
            }
            set {
                var sparseMatrixRowInt = this;                                 
                
                if(IsDummy) {                                                               // Indicates that this row is not part of a matrix. (Indexer of SparseMatrix returns a static SparseMatrixRow with index -1 if row does not exist in matrix.)
                    if(value != DefaultValue) {                                                        // Create new row only if provided value is not zero.  
                        sparseMatrixRowInt = new IntSparseMatRow(this);     // Create an actual row from static dummy.
                        RecentRealIndex = _RealIndex;
                        _SparseMatrix.Insert(_RealIndex, sparseMatrixRowInt);                           // Insertion index has been passed from the getter on SparseMatrix.
                    }
                    else {          // Row does not exist anyway, job done.
                        RecentRealIndex = RealIndex;
                        return;
                    }
                }
                int insertIndex = sparseMatrixRowInt.Count;                                         // If dummy has been inserted, it is empty. Next if loop will not execute, insertIndex has to be accurate here.
                
                if(Count != 0) {
                    ValidateIndex(ref _RecentRealIndex);
                    int realIndex = RecentRealIndex;

                    if(RecentVirtIndex <= imagIndex) {                                              // Move forward until you reach end.
                        
                        while(realIndex < Count && _E[realIndex].VirtIndex <= imagIndex) {        // Find element, delete if value provided is zero.

                            if(_E[realIndex].VirtIndex == imagIndex) {                             // Try to find an existing entry to modify.
                                
                                if(value == DefaultValue)
                                    RemoveAt(realIndex);                                                // If zero is provided, simply remove SparseElement.
                                else
                                    _E[realIndex].Value = value;

                                RecentRealIndex = realIndex;
                                return;
                            }
                            ++realIndex;
                        }
                        insertIndex = realIndex;
                        RecentRealIndex = realIndex;
                    }
                    else {                                                                      // Move backward until you reach end.
                        while(realIndex > -1 && _E[realIndex].VirtIndex >= imagIndex) {

                            if(_E[realIndex].VirtIndex == imagIndex) {                        // Try to find an existing entry to modify.
                                
                                if(value == DefaultValue)
                                    RemoveAt(realIndex);                                                // If zero is provided, simply remove SparseElement.
                                else
                                    _E[realIndex].Value = value;

                                RecentRealIndex = realIndex;
                                return;
                            }
                            --realIndex;
                        }
                        insertIndex = realIndex + 1;
                        RecentRealIndex = insertIndex;
                    }
                }

                if(value != DefaultValue)
                    sparseMatrixRowInt.Insert(insertIndex, new IntSparseElm(insertIndex, imagIndex, value));  // Count = 0, end has been reached or SparseElement at index now has explicit index above the desired one. Insert a freshly created element.
            }
        }

        /// <summary>Creates a SparseMatrixRow that is a sum of two operand SparseMatrixRows.</summary><param name="left">Left operand.</param><param name="right">Right operand.</param>
        public static IntSparseMatRow operator + (IntSparseMatRow left, IntSparseMatRow right) {
            
            var resultRow = (IntSparseRow)left + (IntSparseRow)right;
            return new IntSparseMatRow(resultRow, left.VirtIndex);     // Owner null = unknown.
        }

        public bool Equals(IntSparseMatRow other) {
            var thisRow = (IntSparseRow)this;
            var otherRow = (IntSparseRow)other;
            return thisRow.Equals(otherRow);
        }

        /// <summary>Splits SparseRow in two SparseRows. This SparseRow is modified to be left remainder, while right remainder is returned as result.</summary><param name="imagIndex">Index at which to split. Element at this index will be part of right remainder.</param>
        new public IntSparseMatRow SplitAt(int imagIndex) {
            SetRecentIndexToOrAheadOf(imagIndex);
            TrimExcessSpace();
            return RemoveRange(RecentRealIndex, _E.Length - 1);      // Remove every element from specified point onwards.
        }

        /// <summary>Removes specified range from List and returns it.</summary><param name="j">Inclusive starting index.</param><param name="k">Inclusive ending index.</param>
        new public IntSparseMatRow RemoveRange(int j, int k) {
            int removedCount = k - j + 1;
            IntSparseElm[] removed = new IntSparseElm[removedCount];
            
            for(int i = 0; i < removedCount; ++i) {             // Construct array that we will return.
                BeforeElementExit(i);
                removed[i] = _E[j + i];
            }
            for(int i = k + 1; i < _Count; ++i) {               // Refill hole. Shift elements remaining on right side of hole (removed range) to right.
                BeforeElementExit(i);
                _E[i - removedCount] = _E[i];
                AfterElementEntry(i - removedCount);
            }
            _Count = Count - removedCount;                     // Changing count, no need to zero elements at end.
            _Width = _E[Count].VirtIndex + 1;
            return CreateFromArray(removed);
        }

        /// <summary>Create a string of form (explicitRowIndex, sparseElement 0, sparseElement 1, ..., sparseElement N-1).</summary>
        public override string ToString() {
            StringBuilder sb = new StringBuilder(72);
            sb.Append($"{{{_VirtIndex}: ");

            for(int i = 0; i < _Count - 1; ++i) {
                sb.Append($"{_E[i].ToString()}, ");
            }
            sb.Append($"{_E[_Count - 1].ToString()}}}");
            return sb.ToString();
        }
    }
}