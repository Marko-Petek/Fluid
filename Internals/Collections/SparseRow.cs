using System;
using System.Text;
using System.Linq;
using SCG = System.Collections.Generic;

namespace Fluid.Internals.Collections
{
    public abstract class SparseRow<T> : SCG.Dictionary<int,T>
    {
        /// <summary>Reference to default equality comparer.</summary>
        public static SCG.EqualityComparer<T> _EqComparer = SCG.EqualityComparer<T>.Default;

        //public static SparseRow<T> Default { get; set; } = 
        /// <summary>Width of row if written out explicitly.</summary>
        protected int _Width;
        /// <summary>Matrix of which this SparseRow is part of.</summary>
        protected SparseMat<T> _matrix;


        /// <summary>Width of row if written out explicitly.</summary>
        public int Width => _Width;
        /// <summary>Reference to default equality comparer.</summary>
        public static SCG.EqualityComparer<T> EqComparer => _EqComparer;


        /// <summary>Does not assigns width. User of this constructor must do it manually.</summary>
        protected SparseRow() : base() {}

        /// <summary>Create a SparseRow with specified width it would have in explicit form and specified initial capacity.</summary><param name="width">Width it would have in explicit form.</param><param name="capacity">Initial capacity.</param>
        public SparseRow(int width, int capacity = 6) : base(capacity) {
            _Width = width;
        }

        /// <summary>Creates a SparseRow as a copy of specified SparseRow.</summary><param name="source">Source to copy.</param>
        public SparseRow(SparseRow<T> source) : base(source.Count) {
            foreach(var pair in source) {
                Add(pair.Key, pair.Value);
            }
        }

        /// <summary>Creates an instance of the same (most derived) type as instance on which it is invoked.</summary><param name="width">Width (length of rows) that matrix would have in its explicit form.</param><param name="capacity">Initial row capacity.</param>
        internal abstract SparseRow<T> CreateSparseRow(int width, int capacity = 6);

        internal abstract SparseRow<T> CreateSparseRow(SparseRow<T> source);

        internal SparseRow<T> CreateFromArray(T[] source) => CreateSparseRow(source.Length, source.Length);

        /// <summary>Splits SparseRow in two SparseRows. This SparseRow is modified (left remainder), while chopped-off part (right remainder) is put into specified second argument.</summary><param name="virtIndex">Index at which to split. Element at this index will be chopped off and end up as part of returned SparseRow.</param><param name="removedCols">Right remainder will be put in here.</param>
        public SparseRow<T> SplitAt(int virtIndex) {
            var removedCols = CreateSparseRow(Width - virtIndex);

            foreach(int key in Keys) {

                if(key >= virtIndex) {
                    removedCols.Add(key, this[key]);            // Add to right remainder.
                    Remove(key);                                // Remove from left remainder.
                }
            }
            return removedCols;
        }

        /// <summary>Append specified SparseRow to this one.</summary><param name="rightCols">SparseRow to append.</param>
        public void MergeWith(SparseRow<T> rightCols) {
            _Width += rightCols.Width;                         // Readjust width.
            
            foreach(int key in rightCols.Keys) {
                this[key] = rightCols[key];
            }
        }

        /// <summary>Swap two elements specified by virtual indices.</summary><param name="virtIndex1">Virtual index of first element.</param><param name="virtIndex2">Virtual index of second element.</param><remarks>Useful for swapping columns.</remarks>
        public void SwapElms(int virtIndex1, int virtIndex2) {
            bool firstExists = TryGetValue(virtIndex1, out T val1);
            bool secondExists = TryGetValue(virtIndex2, out T val2);

            if(firstExists) {

                if(secondExists) {
                    this[virtIndex1] = val2;
                    this[virtIndex2] = val1;
                }
                else {
                    Remove(virtIndex1);             // Element at virtIndex1 becomes 0 and is removed.
                    Add(virtIndex2, val1);
                }
            }
            else {

                if(secondExists) {
                    Add(virtIndex1, val2);
                    Remove(virtIndex2);
                }                                   // Else nothing happens, both are 0.
            }
        }

        /// <summary>Apply element swaps as specified by a given swap matrix.</summary><param name="swapMatrix">SparseMatrix where non-zero element at [i][j] signifies a permutation i --> j.</param>
        public void ApplySwaps(SparseMatInt swapMatrix) {

            foreach(var rowPair in swapMatrix)
                foreach(var colPair in rowPair.Value)
                    SwapElms(rowPair.Key, colPair.Key);
        }

        /// <summary>Create a string of form {{key1, val1}, {key2, val2}, ..., {keyN,valN}}..</summary>
        public override string ToString() {
            StringBuilder sb = new StringBuilder(72);
            sb.Append("{");

            foreach(var elm in this.OrderBy( kvPair => kvPair.Key )) {
                sb.Append($"{{{elm.Key.ToString()}, {elm.Value.ToString()}}}, ");
            }
            int length = sb.Length;
            sb.Remove(length - 2, 2);
            sb.Append("}");
            return sb.ToString();
        }

        /// <summary>New indexer definition (hides Dictionary's indexer). Returns 0 for non-existing elements.</summary>
        public new T this[int i] {
            get {
                TryGetValue(i, out T val);         // Outputs zero if value not found.
                return val;
            }
            set {
                if(!EqComparer.Equals(value, default(T))) {     // Setting a value different from 0.
                    var dummy = this as DummyRow<T>;            // Try downcasting to DummyRow.

                    if(dummy != null) {                         // Cast succeeded. Add new row to its owner and add value to it.
                        var newRow = CreateSparseRow(Width);
                        newRow.Add(i, value);
                        dummy.Owner.Add(i, newRow);
                    }
                    else
                        base[i] = value;
                }
                else
                    Remove(i);
            }
        }
    }
}