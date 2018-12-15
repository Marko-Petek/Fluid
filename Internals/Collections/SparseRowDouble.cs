using System;
using System.Linq;
using SCG = System.Collections.Generic;

using static Fluid.Internals.Operations;

namespace Fluid.Internals.Collections
{
    public class SparseRowDouble : SparseRow<double>
    {
        public SparseRowDouble(int width, int capacity = 6) : base(width, capacity) {
        }

        /// <summary>Creates a copy of SparseRowDouble from specified SparseRowDouble.</summary><param name="source">Source to copy.</param>
        public SparseRowDouble(SparseRowDouble source) : base(source) {
        }

        public static SparseRowDouble operator +(SparseRowDouble left, SparseRowDouble right) {
            var resultRow = new SCG.Dictionary<int,double>(right);                     // Copy right operand. Result will appear here.
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
            var resultRow = new SCG.Dictionary<int,double>(left);                     // Copy right operand. Result will appear here.
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
                var upCast = new SCG.Dictionary<int,double>(right);

                foreach(var key in upCast.Keys)
                    upCast[key] *= left;

                return (SparseRowDouble)upCast;
            }
            else
                return new SparseRowDouble(right.Width);
        }

        /// <summary>New indexer definition (hides Dictionary's indexer). Returns 0 for non-existing elements.</summary>
        public new double this[int i] {
            get {
                TryGetValue(i, out double val);         // Outputs zero if value not found.
                return val;
            }
            set {
                if(value != 0)
                    base[i] = value;
                else
                    Remove(i);
            }
        }
    }
}