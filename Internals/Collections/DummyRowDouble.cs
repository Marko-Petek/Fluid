using System;

namespace Fluid.Internals.Collections {

    internal class DummyRowDouble : SparseRowDouble, IDummyRow<double>
    {
        SparseMat<double> _Owner;
        int _Index;


        public SparseMat<double> Owner { get => _Owner; set => _Owner = value; }
        public int Index { get => _Index; set => _Index = value; }


        internal DummyRowDouble(SparseMat<double> owner, int width, int capacity = 1) : base(width, capacity) {
            _Owner = owner;
        }

        internal DummyRowDouble(SparseRowDouble source, SparseMat<double> owner) : base(source) {
            _Owner = owner;

        }

        internal override SparseRow<double> CreateSparseRow(int width, int capacity = 1) => new DummyRowDouble(Owner, Width, capacity);
    
        //internal override SparseRow<int> CreateSparseRow(SparseRowInt source) => new DummyRowInt(source);
    }
}