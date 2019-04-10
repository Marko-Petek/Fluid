#if false
using System;

namespace Fluid.Internals.Collections {

    internal class DummyRowInt : SparseRowInt, IDummyRow<int>
    {
        SparseMat<int> _Owner;
        int _Index;

        public SparseMat<int> Owner { get => _Owner; set => _Owner = value; }
        public int Index { get => _Index; set => _Index = value; }


        internal DummyRowInt(SparseMat<int> owner, int width, int capacity = 1) : base(width, capacity) {
            _Owner = owner;
        }
        internal DummyRowInt(SparseRowInt source, SparseMat<int> owner) : base(source) {
            _Owner = owner;
        }

        internal override SparseRow<int> CreateSparseRow(int width, int capacity = 1) => new DummyRowInt(Owner, Width, capacity);
    
        //internal override SparseRow<int> CreateSparseRow(SparseRowInt source) => new DummyRowInt(source);
    }
}
#endif