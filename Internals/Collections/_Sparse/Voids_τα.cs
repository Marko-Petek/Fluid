using System;
using System.Collections.Generic;
using Fluid.Internals.Numerics;

namespace Fluid.Internals.Collections {
   public static class Voids<τ,α>
   where τ : IEquatable<τ>, new()
   where α : IArithmetic<τ>, new() {
      public static Tensor<τ,α> Tnr = new Tensor<τ,α>(0);
      public static Vector<τ,α> Vec = new Vector<τ,α>(0);
      public static τ Tau { get; } = new τ();
      public static List<int> ListInt { get; } = new List<int>(0);
   }
}