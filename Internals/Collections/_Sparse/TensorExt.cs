using System;
using System.Linq;
using Fluid.Internals.Numerics;
using static Fluid.Internals.Collections.Factory;

namespace Fluid.Internals.Collections {
public static class TensorExt {
   /// <summary>Destructively sums tnr1 and tnr2. Don't use references afterwards. Returns a top tensor (null superior).</summary>
   /// <param name="tnr">&this tensor will be absorbed into tnr.</param>
   /// <remarks> <see cref="TestRefs.TensorSum"/> </remarks>
   public Tensor<τ,α>? SumInto(Tensor<τ,α> tnr) {
      Assume.True(DoSubstructuresMatch(tnr, this),
         () => "Tensor substructures do not match on Sum.");
      Recursion(tnr, this);

      void Recursion(Tensor<τ,α> t1, Tensor<τ,α> t2) {
         if(t2.Rank > 2) {
            foreach(var int_st2 in t2) {
               int sInx = int_st2.Key;
               var st2 = int_st2.Value;
               if(t1.TryGetValue(sInx, out var st1)) {                        // Equivalent subtensor exists in T1.
                  Recursion(st1, st2);
                  if(st1.Count == 0)
                     t1.Remove(sInx); }
               else                                                                 // Equivalent subtensor does not exist in T1. Absorb the subtensor from T2 and add it.
                  t1.AddPlus(sInx, st2); } }
         else if(t2.Rank == 2) {
            foreach(var int_st2 in t2) {
               int sInx = int_st2.Key;
               var st2 = int_st2.Value;
               if(t1.TryGetValue(sInx, out var subTnr1)) {                        // Entry exists in t1, we must sum.
                  var sv1 = (Vector<τ,α>) subTnr1;
                  var sv2 = (Vector<τ,α>) st2;
                  sv1.Sum(sv2);
                  if(sv1.Count == 0)
                     t1.Remove(sInx); }                                           // Crucial to remove if subvector has been anihilated.
               else {
                  t1.AddPlus(sInx, st2); } } }                                // Entry does not exist in t1, simply Add.
         else {                                                                     // We have a vector.
            var vec1 = (Vector<τ,α>) t1;
            var vec2 = (Vector<τ,α>) t2;
            Vector<τ,α>.SumInto(vec1, vec2);
         }
      }
   }

   internal static Tensor<τ,α>? RcrsRank3<τ,α>(Tensor<τ,α> t1, Tensor<τ,α> t2)
   where τ : IEquatable<τ>, IComparable<τ>
   where α : IArithmetic<τ>, new() {
      


      foreach(var inx_st2 in t2) {
         int sInx = inx_st2.Key;
         var st2 = inx_st2.Value;
         if(t1.TryGetValue(sInx, out var st1)) {                        // Equivalent subtensor exists in T1.
            RcrsRank3(st1, st2);
            if(st1.Count == 0)
               t1.Remove(sInx); }
         else                                                                 // Equivalent subtensor does not exist in T1. Absorb the subtensor from T2 and add it.
            t1.AddPlus(sInx, st2); } 
   }
   /// <summary>Sums t2 to t1, both assumed to be non-null. Modifies t1, does not destroy t2.</summary>
   /// <param name="t1">Tensor that will be modified.</param>
   /// <param name="t2">Tensor that will provide sumands.</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   internal static Tensor<τ,α>? SumIntoIntern<τ,α>(this Tensor<τ,α> t1, Tensor<τ,α> t2)
   where τ : IEquatable<τ>, IComparable<τ>
   where α : IArithmetic<τ>, new() {

   }
}
}