using System;
using System.Collections.Generic;

namespace Fluid.Internals.Collections {
   public class DumVector<τ,α> : Vector<τ,α> {
      /// <summary>Index inside parent rank 2 tensor.</summary>
      public int Index { get; set; }
      /// <summary>Dimension of dummy vector's slot is irrelevant.</summary>
      /// <param name="Vector's superior."></param>
      /// <param name="capacity"></param>
      /// <returns></returns>
      internal DumVector(Tensor<τ,α> sup) : base() {
         Sup = sup;
      }
      internal DumVector(Tensor<τ,α> src, Tensor<τ,α> sup) : base(src) {
         Sup = sup;
      }
   }
}
