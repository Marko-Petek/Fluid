using System;
using System.Collections.Generic;

namespace Fluid.Internals.Collections {
   /// <summary>A tensor with specified rank and specified dimension</summary>
   public abstract class TensorBase<τ> : Dictionary<int,τ> where τ : TensorBase<τ> {
      public TensorBase(int cap) : base(cap) {}
      public TensorBase() : base(6) {}
   }
}
