using System;
using System.Text;
using System.Linq;
using SCG = System.Collections.Generic;

namespace Fluid.Internals.Collections
{
    public class SparseRow<T> : SCG.Dictionary<int,T>
    {
        /// <summary>Width of row if written out explicitly.</summary>
        protected int _Width;


        /// <summary>Width of row if written out explicitly.</summary>
        public int Width => _Width;


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

        /// <summary>Create a SparseRow by copying specified source array.</summary><param name="source">Source array to adopt.</param>
        public static SparseRow<T> CreateFromArray(T[] source) {
            int length = source.Length;
            var sparseRow = new SparseRow<T>(length);

            for(int i = 0; i < length; ++i)
                sparseRow[i] = source[i];
            
            return sparseRow;
        }

        /// <summary>Splits SparseRow in two SparseRows. This SparseRow is modified (left remainder), while chopped-off part (right remainder) is returned as result.</summary><param name="virtIndex">Index at which to split. Element at this index will be chopped off and end up as part of returned SparseRow.</param>
        public SparseRow<T> SplitAt(int virtIndex) {
            //var splitSuccess = SetRecentIndexToOrAheadOf(virtIndex);
            SparseRow<T> removedCols = new SparseRow<T>(Width - virtIndex);

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

        /// <summary>Swap two elements specified by virtual indices.</summary><param name="virtIndex1">Explicit index of first element.</param><param name="virtIndex2">Explicit index of second element.</param><remarks>Useful for swapping columns.</remarks>
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
        public void ApplySwaps(List<(int,int)> swapMatrix) {        // TODO: Change parameter to SparseMatInt.

            foreach(var pair in swapMatrix) {
                SwapElms(pair.Item1, pair.Item2);
            }
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
    }
}