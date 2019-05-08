using System;

using Fluid.Internals.Numerics;

namespace Fluid.Internals.Collections {
   /// <summary>Dummy R1 tensor used inside a R2 tensor's getter.</summary>
   /// <typeparam name="τ"></typeparam>
   public class DumTensor1<τ> : Tensor<τ> where τ : new() {
      /// <summary>Index inside parent rank 2 tensor.</summary>
      public int Index { get; set; }
      /// <summary>Dimension of dummy tensor's slot is irrelevant.</summary>
      /// <param name="owner"></param>
      /// <param name="capacity"></param>
      /// <returns></returns>
      internal DumTensor1(Tensor2<τ> owner) : base() {
         Sup = owner;
      }
      internal DumTensor1(Tensor<τ> src, Tensor2<τ> owner) : base(src) {
         Sup = owner;
      }
   }
}