using System;

using Fluid.Internals.Numerics;

namespace Fluid.Internals.Collections {
   public class DumTensor1<τ> : Tensor1<τ> where τ : new() {
      /// <summary>Index inside parent rank 2 tensor.</summary>
      public int Index { get; set; }
      /// <summary>Dimension of dummy tensor's slot is irrelevant.</summary>
      /// <param name="owner"></param>
      /// <param name="capacity"></param>
      /// <returns></returns>
      internal DumTensor1(Tensor2<τ> owner) : base() {
         Tnr2 = owner;
      }
      internal DumTensor1(Tensor1<τ> src, Tensor2<τ> owner) : base(src) {
         Tnr2 = owner;
      }
   }
}