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

        
    }
}
#endif