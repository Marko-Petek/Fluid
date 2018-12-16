using System;
using System.Linq;
using SCG = System.Collections.Generic;
using static System.Math;

using static Fluid.Internals.Operations;

namespace Fluid.Internals.Collections
{
    public class SparseRowInt : SparseRow<int>
    {
        /// <summary>Does not assigns width. User of this constructor must do it manually.</summary>
        protected SparseRowInt() : base() {}

        /// <summary>Create a SparseRow with specified width it would have in explicit form and specified initial capacity.</summary><param name="width">Width it would have in explicit form.</param><param name="capacity">Initial capacity.</param>
        public SparseRowInt(int width, int capacity = 6) : base(width, capacity) {
        }

        /// <summary>Creates a copy of SparseRowDouble from specified SparseRowDouble.</summary><param name="source">Source to copy.</param>
        public SparseRowInt(SparseRow<int> source) : base(source) {
        }

        /// <summary>Create a SparseRow by copying specified source array.</summary><param name="source">Source array to copy.</param><param name="target">Created SparseRow will be put in here.</param>
        protected new static SparseRowInt CreateFromArray(int[] source) {
            var sparseRow = new SparseRowInt(source.Length, source.Length);

            for(int i = 0; i < source.Length; ++i)
                sparseRow[i] = source[i];

            return sparseRow;
        }

        internal override SparseRow<int> CreateSparseRow(int width, int capacity = 6) => new SparseRowInt(width, capacity);

        internal override SparseRow<int> CreateSparseRow(SparseRow<int> source) => new SparseRowInt(source);

        /// <summary>Splits SparseRow in two SparseRows. This SparseRow is modified (left remainder), while chopped-off part (right remainder) is put into specified second argument.</summary><param name="virtIndex">Index at which to split. Element at this index will be chopped off and end up as part of returned SparseRow.</param>
        public new SparseRowInt SplitAt(int virtIndex) => (SparseRowInt) base.SplitAt(virtIndex);

        public static SparseRowInt operator +(SparseRowInt left, SparseRowInt right) {
            var resultRow = (SCG.Dictionary<int,int>) new SparseRowInt(right);                     // Copy right operand. Result will appear here. Upcast to dictionary so that Dictionary's indexer is used.
            int temp;

            foreach(var kvPair in left) {

                resultRow.TryGetValue(kvPair.Key, out int val);       // Then add to existing value.
                temp = kvPair.Value + val;

                if(temp != 0)
                    resultRow[kvPair.Key] = temp;
                else if(val != 0)                                       // temp == 0 && resultRow[kvPair.Key] != 0
                    resultRow.Remove(kvPair.Key);
            }
            return (SparseRowInt)resultRow;
        }

        public static SparseRowInt operator -(SparseRowInt left, SparseRowInt right) {
            var resultRow = (SCG.Dictionary<int,int>) new SparseRowInt(left);             // Copy right operand. Result will appear here. Upcast to dictionary so that Dictionary's indexer is used.
            int temp;

            foreach(var kvPair in right) {

                resultRow.TryGetValue(kvPair.Key, out int val);      // val is 0, if key does not exist
                temp = val - kvPair.Value;

                if(temp != 0)
                    resultRow[kvPair.Key] = temp;
                else if(val == 0)                                       // temp = 0 && right[kvPair.Key] == 0
                    resultRow.Remove(kvPair.Key);
            }
            return (SparseRowInt)resultRow;
        }

        public static int operator *(SparseRowInt left, SparseRowInt right) {
            int result = 0;

            foreach(var kvPair in left) {
                
                if(right.TryGetValue(kvPair.Key, out int val)) {
                    result += kvPair.Value * val;
                }
            }
            return result;
        }

        public static SparseRowInt operator *(int left, SparseRowInt right) {

            if(left != 0) {
                var upCast = (SCG.Dictionary<int,int>) new SparseRowInt(right);       // Upcast to dictionary so that Dictionary's indexer is used.

                foreach(var key in upCast.Keys)
                    upCast[key] *= left;

                return (SparseRowInt)upCast;
            }
            else
                return new SparseRowInt(right.Width);
        }

        /// <summary>Calculates square of Euclidean norm of SparseRow.</summary>
        public int NormSqr() {
            int result = 0;

            foreach(var elm in this) {
                result += (int)Pow(elm.Value, 2);
            }
            return result;
        }
    }
}