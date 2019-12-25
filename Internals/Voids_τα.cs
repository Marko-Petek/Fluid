using System;
using System.Collections.Generic;
using Fluid.Internals.Collections;
using Fluid.Internals.Numerics;

namespace Fluid.Internals {

public static class Voids {
   public static readonly List<int> ListInt = new List<int>(0);
   public static readonly RankedNode RNode = new RankedNode();
}
public static class Voids<τ>
where τ : IEquatable<τ>, new() {
   public static readonly τ Tau = new τ();
   public static readonly τ[] Arr = new τ[0];
   public static readonly Hierarchy<τ> Hier = new Hierarchy<τ>(Voids.RNode);
}
// public static class Voids<τ,α>
// where τ : IEquatable<τ>, new()
// where α : IArithmetic<τ>, new() {
//    public static readonly Tensor<τ,α> Tnr = new Tensor<τ,α>(0);
//    public static readonly Vector<τ,α> Vec = new Vector<τ,α>(0);
   
// }
}