using System;

using Fluid.Internals.Numerics;

namespace Fluid.Internals.Collections {
   internal class DummyRow<T,TArith> : SparseRow<T,TArith>
      where T : IEquatable<T>, new()
      where TArith : IArithmetic<T>, new() {
         public int Index { get; set; }
         internal DummyRow(SparseMat<T,TArith> owner, int width, int capacity = 1) : base(width, capacity) {
            SparseMat = owner;
         }
         internal DummyRow(SparseRow<T,TArith> source, SparseMat<T,TArith> owner) : base(source) {
            SparseMat = owner;
         }

         public override SparseRow<T,TArith> CreateSparseRow(int width, int capacity = 1) => new DummyRow<T,TArith>(SparseMat, Width, capacity);
   }
}