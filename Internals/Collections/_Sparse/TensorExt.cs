using System;
using System.Linq;
using Fluid.Internals.Numerics;
using static Fluid.Internals.Collections.Factory;

namespace Fluid.Internals.Collections {
public static class TensorExt {
   /// <summary>Destructively sums tnr1 and tnr2. Don't use references afterwards. Returns a top tensor (null superior).</summary>
   /// <param name="tnr">&this tensor will be absorbed into tnr.</param>
   /// <remarks> <see cref="TestRefs.TensorSum"/> </remarks>
   public Tensor<τ,α>? SumInto<τ,α>(Tensor<τ,α>? tnr1, Tensor<τ,α>? tnr2)
   where τ : IEquatable<τ>, IComparable<τ>
   where α : IArithmetic<τ>, new() {
      Assume.True(DoSubstructuresMatch(tnr, this),
         () => "Tensor substructures do not match on Sum.");
      Recursion(tnr, this);
   }

   /// <summary>Sums t2 to t1, both assumed to be non-null. Modifies t1, does not destroy t2.</summary>
   /// <param name="t1">Tensor that will be modified.</param>
   /// <param name="t2">Tensor that will provide sumands.</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   internal static Tensor<τ,α>? SumIntoIntern<τ,α>(this Tensor<τ,α> t1, Tensor<τ,α> t2)
   where τ : IEquatable<τ>, IComparable<τ>
   where α : IArithmetic<τ>, new() {
      if(t2.Rank > 2) {
         foreach(var i_st2 in t2) {
            int si = i_st2.Key;
            var st2 = i_st2.Value;
            if(t1.TryGetValue(si, out var st1)) {                        // Equivalent subtensor exists in T1.
               var sum = SumIntoIntern(st1, st2);
               if(sum == null)
                  t1.Remove(si); }
            else                                                                 // Equivalent subtensor does not exist in T1. Absorb the subtensor from T2 and add it.
               t1.AddSubTnr(si, st2); } }
      else if(t2.Rank == 2) {
         foreach(var i_st2 in t2) {
            int si = i_st2.Key;
            var st2 = i_st2.Value;
            if(t1.TryGetValue(si, out var st1)) {                        // Entry exists in t1, we must sum.
               var sv1 = (Vector<τ,α>) st1;
               var sv2 = (Vector<τ,α>) st2;
               var res = sv1.SumIntoIntern(sv2);
               if(res == null)
                  t1.Remove(si); }                                           // Crucial to remove if subvector has been anihilated.
            else {
               t1.AddSubTnr(si, st2); } } }                                // Entry does not exist in t1, simply Add.
      else {                                                                     // We have a vector.
         var v1 = (Vector<τ,α>) t1;
         var v2 = (Vector<τ,α>) t2;
         return v1.SumIntoIntern(v2); }
      if(t1.Count != 0)
         return t1;
      else
         return null;
   }
}
}