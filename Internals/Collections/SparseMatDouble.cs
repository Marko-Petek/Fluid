using System;

namespace Fluid.Internals.Collections
{
    public class SparseMatDouble : SparseMat<double>
    {
        public SparseMatDouble(int width, int height, int capacity = 6) : base(width, height, capacity) {
        }

        public SparseMatDouble(SparseMatDouble source) : base(source) {
        }

        public SparseMatDouble(double[][] source) : base(source) {
        }
    }
}