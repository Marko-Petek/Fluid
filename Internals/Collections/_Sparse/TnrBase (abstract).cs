using System;
using System.Collections.Generic;
namespace Fluid.Internals.Collections {

/// <summary>A tensor with specified rank and specified dimension</summary>
public abstract class TnrBase<τ> : Dictionary<int,τ> where τ : TnrBase<τ> {
   public TnrBase(int cap) : base(cap) {}
   public TnrBase() : base(6) {}
}
}
