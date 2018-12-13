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
            var resultRow = new SparseRowDouble(right);                     // Copy right operand. Result will appear here.

            foreach(var kvPair in left) {

                if(resultRow.ContainsKey(kvPair.Key))                       // Then add to existing value.
                    resultRow[kvPair.Key] += kvPair.Value;
                else
                    resultRow.Add(kvPair.Key, kvPair.Value);                // Create new entry.
            }
            return resultRow;
        }
    }
}