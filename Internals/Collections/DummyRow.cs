using System;

namespace Fluid.Internals.Collections {

    internal class DummyRow<T> : SparseRow<T>
    {
        SparseMat<T> _Owner;


        internal SparseMat<T> Owner => _Owner;


        internal DummyRow(SparseMat<T> owner, int width, int capacity = 1) : base(width, capacity) {
        }

        internal DummyRow(SparseRow<T> source) : base(source) {

        }

        internal override SparseRow<T> CreateSparseRow(int width, int capacity = 1) => new DummyRow<T>(Owner, Width, capacity);
    
        internal override SparseRow<T> CreateSparseRow(SparseRow<T> source) => new DummyRow<T>(source);
    }
}