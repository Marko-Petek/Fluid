using System;

namespace Fluid.Internals.Collections
{
    public class SparseRowDouble : SparseRow<double>
    {
        public SparseRowDouble(int width, int capacity = 6) : base(width, capacity) {
        }

        public static SparseRowDouble operator +(SparseRowDouble left, SparseRowDouble right) {
            
        }
    }
}