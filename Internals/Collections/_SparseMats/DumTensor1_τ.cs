using System;

using Fluid.Internals.Numerics;

namespace Fluid.Internals.Collections {
   internal class DumTensor1<τ> : SparseRow<τ> where τ : new() {
         public int Index { get; set; }
         internal DumTensor1(SparseMat<τ> owner, int width, int capacity = 1) : base(width, capacity) {
            SparseMat = owner;
         }
         internal DumTensor1(SparseRow<τ> source, SparseMat<τ> owner) : base(source) {
            SparseMat = owner;
         }
   }
}