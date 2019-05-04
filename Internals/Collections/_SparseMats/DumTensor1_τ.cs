using System;

using Fluid.Internals.Numerics;

namespace Fluid.Internals.Collections {
   internal class DumTensor1<τ> : Tensor1<τ> where τ : new() {
      /// <summary>Index inside parent rank 2 tensor.</summary>
      public int Index { get; set; }
      /// <summary>Dimension of dummy tensor's slot is irrelevant.</summary>
      /// <param name="owner"></param>
      /// <param name="capacity"></param>
      /// <returns></returns>
      internal DumTensor1(Tensor2<τ> owner, int capacity = 1) : base(width, capacity) {
         Tensor2 = owner;
      }
      internal DumTensor1(SparseRow<τ> source, Tensor2<τ> owner) : base(source) {
         Tensor2 = owner;
      }
   }
}