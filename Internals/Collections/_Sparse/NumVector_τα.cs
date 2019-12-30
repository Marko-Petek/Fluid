using System;
using System.Linq;
using Fluid.Internals.Numerics;

namespace Fluid.Internals.Collections {
   public class NumVector<τ,α> : Vector<τ,α>
   where τ : struct, IEquatable<τ>, IComparable<τ>
   where α : IArithmetic<τ>, new() {
      public bool Equals(Vector<τ,α> vec2, τ eps) {
         if(!Vals.Keys.OrderBy(key => key).SequenceEqual(vec2.Vals.Keys.OrderBy(key => key)))    // Keys have to match.
            return false;
         foreach(var int_val1 in Vals) {
            τ val2 = vec2[int_val1.Key];
            if(O<τ,α>.A.Abs(O<τ,α>.A.Sub(int_val1.Value, val2)).CompareTo(eps) > 0 ) // Values do not agree within tolerance.
               return false; }
         return true;                                                              // All values agree within tolerance.
      }
   }
}