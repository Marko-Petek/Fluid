using System;

using Fluid.Internals.Numerics;

namespace Fluid.Internals.Collections {
   internal class DumTensor1<τ,α> : Tensor1<τ,α>
      where τ : IEquatable<τ>, IComparable<τ>, new()
      where α : IArithmetic<τ>, new() {
         public int Index { get; set; }
         internal DumTensor1(Tensor2<τ,α> owner, int width, int capacity = 1) : base(width, capacity) {
            Tensor2 = owner;
         }
         internal DumTensor1(Tensor1<τ,α> source, Tensor2<τ,α> owner) : base(source) {
            Tensor2 = owner;
         }
   }
}