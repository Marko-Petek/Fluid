using System;

using Fluid.Internals.Numerics;

namespace Fluid.Internals.Collections {
   internal class DummyRow<τ,α> : SparseRow<τ,α>
      where τ : IEquatable<τ>, IComparable<τ>, new()
      where α : IArithmetic<τ>, new() {
         public int Index { get; set; }
         internal DummyRow(SparseMat<τ,α> owner, int width, int capacity = 1) : base(width, capacity) {
            SparseMat = owner;
         }
         internal DummyRow(SparseRow<τ,α> source, SparseMat<τ,α> owner) : base(source) {
            SparseMat = owner;
         }
   }
}