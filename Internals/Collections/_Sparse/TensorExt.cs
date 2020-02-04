using System;
using System.Linq;
using Fluid.Internals.Numerics;
using static Fluid.Internals.Collections.Factory;

namespace Fluid.Internals.Collections {
public static class TensorExt {
   /// <summary>Compares substructures of two non-null tensors.</summary>
   /// <param name="tnr1">First tensor.</param>
   /// <param name="tnr2">Second tensor.</param>
   internal static bool CompareSubstrcIntern<τ,α>(this Tensor<τ,α> t1, Tensor<τ,α> t2)
   where τ : IEquatable<τ>, IComparable<τ>
   where α : IArithmetic<τ>, new() =>
      t1.SubStructure.SequenceEqual(t2.SubStructure);

   /// <summary>Sums t2 to t1, both assumed to be non-null. Modifies t1, does not destroy t2.</summary>
   /// <param name="t1">Tensor that will be modified.</param>
   /// <param name="t2">Tensor that will provide sumands.</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   /// <remarks> <see cref="TestRefs.TensorSum"/> </remarks>
   public static Tensor<τ,α>? SumInto<τ,α>(Tensor<τ,α>? t1, Tensor<τ,α>? t2)
   where τ : IEquatable<τ>, IComparable<τ>
   where α : IArithmetic<τ>, new() {
      if(t1 == null) {
         if(t2 == null)
            return null;
         else
            return t2.CopyAsTopTnr(); }
      else if(t2 == null)
         return t1;

      Assume.True(t1.CompareSubstrcIntern(t2),
         () => "Tensor substructures do not match on Sum.");
      return SumIntoIntern(t1, t2);
   }

   /// <summary>Sums t2 to t1, both assumed to be non-null. Modifies t1, does not destroy t2. Does not check for a match between substructures.</summary>
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
            else                                                                 // Equivalent subtensor does not exist in T1. Copy the subtensor from T2 and add it.
               st2.CopyAsSubTnr(t1, si); } }
      else if(t2.Rank == 2) {
         foreach(var i_st2 in t2) {
            int si = i_st2.Key;
            var sv2 = (Vector<τ,α>) i_st2.Value;
            if(t1.TryGetValue(si, out var st1)) {                        // Entry exists in t1, we must sum.
               var sv1 = (Vector<τ,α>) st1;
               var res = sv1.SumIntoIntern(sv2);
               if(res == null)
                  t1.Remove(si); }                                           // Crucial to remove if subvector has been anihilated.
            else {
               sv2.CopyAsSubVecIntern(t1, si); } } }                                // Entry does not exist in t1, copy as SubVec.
      else {                                                                     // We have a vector.
         var v1 = (Vector<τ,α>) t1;
         var v2 = (Vector<τ,α>) t2;
         return v1.SumIntoIntern(v2); }
      if(t1.Count != 0)
         return t1;
      else
         return null;
   }

   public static Tensor<τ,α>? SumTop<τ,α>(this Tensor<τ,α>? t1, Tensor<τ,α>? t2)
   where τ : IEquatable<τ>, IComparable<τ>
   where α : IArithmetic<τ>, new() {
      if(t1 == null) {
         if(t2 == null)
            return null;
         else
            return t2.CopyAsTopTnr(); }
      else if(t2 == null)
         return t1.CopyAsTopTnr();
      return t1.SumTopIntern(t2);
   }

   internal static Tensor<τ,α>? SumTopIntern<τ,α>(this Tensor<τ,α> t1, Tensor<τ,α> t2)
   where τ : IEquatable<τ>, IComparable<τ>
   where α : IArithmetic<τ>, new() =>
      t1.CopyAsTopTnr().SumIntoIntern(t2);

   /// <summary>Subtracts tnr2 from the caller. Tnr2 is still usable afterwards.</summary>
   /// <param name="aTnr2">Minuend which will be subtracted from the caller. Minuend is still usable after the operation.</param>
   /// <remarks><see cref="TestRefs.TensorSub"/></remarks>
   public static Tensor<τ,α>? SubInto<τ,α>(this Tensor<τ,α>? t1, Tensor<τ,α>? t2)
   where τ : IEquatable<τ>, IComparable<τ>
   where α : IArithmetic<τ>, new() {
      if(t1 == null) {
         if(t2 == null)
            return null;
         else
            return t2.CopyAsTopTnr().NegateIntoIntern(); }
      else if(t2 == null)
         return t1;

      Assume.True(t1.CompareSubstrcIntern(t2),
         () => "Tensor substructures do not match on Sub.");
      return SubIntoIntern(t1, t2);
   }

   public static Tensor<τ,α>? SubIntoIntern<τ,α>(this Tensor<τ,α> t1, Tensor<τ,α> t2)
   where τ : IEquatable<τ>, IComparable<τ>
   where α : IArithmetic<τ>, new() {
      if(t2.Rank > 2) {
         foreach(var i_st2 in t2) {
            int si = i_st2.Key;
            var st2 = i_st2.Value;
            if(t1.TryGetValue(si, out var st1)) {                        // Equivalent subtensor exists in T1.
               var sub = SubIntoIntern(st1, st2);
               if(sub == null)
                  t1.Remove(si); }
            else                                                                 // Equivalent subtensor does not exist in T1. Copy the subtensor from T2 and negate it.
               st2.CopyAsSubTnr(t1, si).NegateIntoIntern(); } }
      else if(t2.Rank == 2) {
         foreach(var i_st2 in t2) {
            int si = i_st2.Key;
            var st2 = i_st2.Value;
            var sv2 = (Vector<τ,α>) st2;
            if(t1.TryGetValue(si, out var st1)) {                        // Entry exists in t1, we must sum.
               var sv1 = (Vector<τ,α>) st1;
               var res = sv1.SumIntoIntern(sv2);
               if(res == null)
                  t1.Remove(si); }                                           // Crucial to remove if subvector has been anihilated.
            else {
               sv2.CopyAsSubVecIntern(t1, si); } } }                                // Entry does not exist in t1, copy as subvector.
      else {                                                                     // We have a vector.
         var v1 = (Vector<τ,α>) t1;
         var v2 = (Vector<τ,α>) t2;
         return v1.SubIntoIntern(v2); }
      if(t1.Count != 0)
         return t1;
      else
         return null;
   }

   public static Tensor<τ,α>? SubTop<τ,α>(this Tensor<τ,α>? t1, Tensor<τ,α>? t2)
   where τ : IEquatable<τ>, IComparable<τ>
   where α : IArithmetic<τ>, new() {
      if(t1 == null) {
         if(t2 == null)
            return null;
         else
            return t2.NegateTopIntern(); }
      else if(t2 == null)
         return t1.CopyAsTopTnr();
      return t1.SubTopIntern(t2);
   }

   internal static Tensor<τ,α>? SubTopIntern<τ,α>(this Tensor<τ,α> t1, Tensor<τ,α> t2)
   where τ : IEquatable<τ>, IComparable<τ>
   where α : IArithmetic<τ>, new() =>
      t1.CopyAsTopTnr().SubIntoIntern(t2);

   /// <summary>Modifies this tensor by negating each element.</summary>
   public static Tensor<τ,α>? NegateInto<τ,α>(this Tensor<τ,α>? t)
   where τ : IEquatable<τ>, IComparable<τ>
   where α : IArithmetic<τ>, new() {
      if(t == null)
         return null;
      return t.NegateIntoIntern();
   }

   internal static Tensor<τ,α> NegateIntoIntern<τ,α>(this Tensor<τ,α> t)
   where τ : IEquatable<τ>, IComparable<τ>
   where α : IArithmetic<τ>, new() {
      if(t.Rank > 2) {
         foreach(var i_st in t) {
            var st = i_st.Value;
            t.NegateIntoIntern(); } }
      else if(t.Rank == 2) {
         foreach(var i_st in t) {
            var sv = (Vector<τ,α>) i_st.Value;
            sv.NegateIntoIntern(); } }
      else {
         var v = (Vector<τ,α>) t;
         v.NegateIntoIntern(); }
      return t;
   }

   public static Tensor<τ,α>? NegateTop<τ,α>(this Tensor<τ,α>? t)
   where τ : IEquatable<τ>, IComparable<τ>
   where α : IArithmetic<τ>, new() {
      if(t == null)
         return null;
      else
         return t.CopyAsTopTnr().NegateIntoIntern();
   }

   internal static Tensor<τ,α>? NegateTopIntern<τ,α>(this Tensor<τ,α> t)
   where τ : IEquatable<τ>, IComparable<τ>
   where α : IArithmetic<τ>, new() =>
      t.CopyAsTopTnr().NegateIntoIntern();

   public static Tensor<τ,α>? MulInto<τ,α>(this Tensor<τ,α>? t, τ scal)
   where τ : IEquatable<τ>, IComparable<τ>
   where α : IArithmetic<τ>, new() {
      if(t == null || scal.Equals(O<τ,α>.A.Zero()))
         return null;
      else
         return MulIntoIntern(t, scal);
   }

   /// <summary>Multiplies caller with a scalar.</summary>
   /// <param name="scal">Scalar.</param>
   /// <remarks> <see cref="TestRefs.TensorMul"/> </remarks>
   internal static Tensor<τ,α> MulIntoIntern<τ,α>(this Tensor<τ,α> t, τ scal)
   where τ : IEquatable<τ>, IComparable<τ>
   where α : IArithmetic<τ>, new() {
      if(t.Rank > 2) {                                       // Subordinates are tensors.
         foreach (var i_st in t) {
            var st = i_st.Value;
            MulIntoIntern(st, scal); } }
      else if(t.Rank == 2) {                                 // Subordinates are vectors.
         foreach (var i_st in t) {
            var sv = (Vector<τ,α>) i_st.Value;
            sv.MulInto(scal); } }
      else {
         var v = (Vector<τ,α>) t;
         v.MulInto(scal); }
      return t;
   }

   /// <summary>Multiplies a scalar with a vector and returns a new vector. Safe version: accepts a nullable vector and checks it for null, also checks if the scalar is zero.</summary>
   /// <param name="scal">Scalar.</param>
   /// <param name="t">Vector.</param>
   public static Tensor<τ,α>? MulTop<τ,α>(this Tensor<τ,α>? t, τ scal)
   where τ : IEquatable<τ>, IComparable<τ>
   where α : IArithmetic<τ>, new() {
      if(t == null || scal.Equals(O<τ,α>.A.Zero()))
         return null;
      else
         return t.MulTopIntern(scal);
   }
   /// <summary>Multiplies a scalar with a vector and returns a new vector. Unsafe version: accepts a non-nullable vector and does not check whether the scalar is zero.</summary>
   /// <param name="scal"></param>
   /// <param name="vec"></param>
   internal static Tensor<τ,α> MulTopIntern<τ,α>(this Tensor<τ,α> t, τ scal)
   where τ : IEquatable<τ>, IComparable<τ>
   where α : IArithmetic<τ>, new() =>
      t.CopyAsTopTnr().MulIntoIntern(scal);
}
}