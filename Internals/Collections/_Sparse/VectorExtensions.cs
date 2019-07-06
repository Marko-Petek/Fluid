using System;
using Fluid.Internals.Numerics;

namespace Fluid.Internals.Collections {
   public static class VectorExtensions<τ,α>
   where τ : IEquatable<τ>, IComparable<τ>, new()
   where α : IArithmetic<τ>, new() {
      public static Vector<τ,α> Mul(τ scal, Vector<τ,α> vec, Tensor<τ,α> resSup) {
         var res = new Vector<τ,α>(resSup, vec.Vals.Count);
         foreach(var int_vec in vec.Vals)
            res.Add(int_vec.Key, O<τ,α>.A.Mul(scal, vec[int_vec.Key]));
         return res;
      }
   }
}
