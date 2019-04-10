
#if false
using System;
using System.Linq;
using SCG = System.Collections.Generic;
using static System.Math;

using Fluid.Internals.Development;
using static Fluid.Internals.Operations;

namespace Fluid.Internals.Collections
{
    public class SparseRowDouble : SparseRow<double>
    {
        /// <summary>Does not assigns width. User of this constructor must do it manually.</summary>
        protected SparseRowDouble() : base() {}

        /// <summary>Create a SparseRow with specified width it would have in explicit form and specified initial capacity.</summary><param name="width">Width it would have in explicit form.</param><param name="capacity">Initial capacity.</param>
        public SparseRowDouble(int width, int capacity = 6) : base(width, capacity) {
        }

        // /// <summary>Creates a copy of SparseRowDouble from specified SparseRowDouble.</summary><param name="source">Source to copy.</param>
        // public SparseRowDouble(SparseRowDouble source) : base(source) {
        // }

        /// <summary>Creates a copy of SparseRowDouble from specified SparseRowDouble.</summary><param name="source">Source to copy.</param>
        public SparseRowDouble(SparseRow<double> source) : base(source) {
        }

        /// <summary>Create a SparseRow by copying specified source array.</summary><param name="source">Source array to copy.</param><param name="target">Created SparseRow will be put in here.</param>
        protected new static SparseRowDouble CreateFromArray(double[] source) {
            var sparseRow = new SparseRowDouble(source.Length, source.Length);

            for(int i = 0; i < source.Length; ++i)
                sparseRow[i] = source[i];

            return sparseRow;
        }

        internal override SparseRow<double> CreateSparseRow(int width, int capacity = 6) => new SparseRowDouble(width, capacity);

        internal override SparseRow<double> CreateSparseRow(SparseRow<double> source) => new SparseRowDouble(source);

        /// <summary>Splits SparseRow in two SparseRows. This SparseRow is modified (left remainder), while chopped-off part (right remainder) is put into specified second argument.</summary><param name="virtIndex">Index at which to split. Element at this index will be chopped off and end up as part of returned SparseRow.</param>
        public new SparseRowDouble SplitAt(int virtIndex) => (SparseRowDouble) base.SplitAt(virtIndex);

        public static SparseRowDouble operator +(SparseRowDouble left, SparseRowDouble right) {
            var resultRow = (SCG.Dictionary<int,double>) new SparseRowDouble(right);                     // Copy right operand. Result will appear here. Upcast to dictionary so that Dictionary's indexer is used.
            double temp;

            foreach(var kvPair in left) {

                resultRow.TryGetValue(kvPair.Key, out double val);       // Then add to existing value.
                temp = kvPair.Value + val;

                if(temp != 0.0)
                    resultRow[kvPair.Key] = temp;
                else if(val != 0)                                       // temp == 0 && resultRow[kvPair.Key] != 0
                    resultRow.Remove(kvPair.Key);
            }
            return (SparseRowDouble)resultRow;
        }

        public static SparseRowDouble operator -(SparseRowDouble left, SparseRowDouble right) {
            var resultRow = (SCG.Dictionary<int,double>) new SparseRowDouble(left);             // Copy right operand. Result will appear here. Upcast to dictionary so that Dictionary's indexer is used.
            double temp;

            foreach(var kvPair in right) {

                resultRow.TryGetValue(kvPair.Key, out double val);      // val is 0, if key does not exist
                temp = val - kvPair.Value;

                if(temp != 0.0)
                    resultRow[kvPair.Key] = temp;
                else if(val == 0)                                       // temp = 0 && right[kvPair.Key] == 0
                    resultRow.Remove(kvPair.Key);
            }
            return (SparseRowDouble)resultRow;
        }

        public static double operator *(SparseRowDouble left, SparseRowDouble right) {
            double result = 0;

            foreach(var kvPair in left) {
                
                if(right.TryGetValue(kvPair.Key, out double val)) {
                    result += kvPair.Value * val;
                }
            }
            return result;
        }

        public static SparseRowDouble operator *(double left, SparseRowDouble right) {

            if(left != 0.0) {
                var upCast = (SCG.Dictionary<int,double>) new SparseRowDouble(right);       // Upcast to dictionary so that Dictionary's indexer is used.

                foreach(var key in upCast.Keys)
                    upCast[key] *= left;

                return (SparseRowDouble)upCast;
            }
            else
                return new SparseRowDouble(right.Width);
        }

        /// <summary>Calculates square of Euclidean norm of SparseRow.</summary>
        public double NormSqr() {
            double result = 0.0;

            foreach(var elm in this) {
                result += Pow(elm.Value, 2);
            }
            return result;
        }
    }
}
#endif