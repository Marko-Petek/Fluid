using System;
using System.Linq;
using Fluid.Internals.Numerics;
using static Fluid.Internals.Collections.Factory;

namespace Fluid.Internals.Collections {
public static class TensorExt {
   /// <summary>Compares substructures of two non-null tensors.</summary>
   /// <param name="tnr1">First tensor.</param>
   /// <param name="tnr2">Second tensor.</param>
   internal static bool CompareSubstrcIntern<τ,α>(this Tensor<τ,α> t1, Tensor<τ,α> t2)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() =>
      t1.SubStructure.SequenceEqual(t2.SubStructure);

   /// <summary>Sums t2 to t1, both assumed to be non-null. Modifies t1, does not destroy t2.</summary>
   /// <param name="t1">Tensor that will be modified.</param>
   /// <param name="t2">Tensor that will provide sumands.</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   /// <remarks> <see cref="TestRefs.TensorSum"/> </remarks>
   public static Tensor<τ,α>? SumInto<τ,α>(Tensor<τ,α>? t1, Tensor<τ,α>? t2)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() {
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
   internal static Tensor<τ,α>? SumIntoIntern<τ,α>(this Tensor<τ,α> t1, Tensor<τ,α> t2)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() {
      if(t2.Rank > 2) {
         foreach(var (i, st2) in t2) {
            if(t1.TryGetValue(i, out var st1)) {                        // Equivalent subtensor exists in T1.
               var sum = SumIntoIntern(st1, st2);
               if(sum == null)
                  t1.Remove(i); }
            else                                                                 // Equivalent subtensor does not exist in T1. Copy the subtensor from T2 and add it.
               st2.CopyAsSubTnr(t1, i); } }
      else if(t2.Rank == 2) {
         foreach(var (i, st2) in t2) {
            var sv2 = (Vector<τ,α>) st2;
            if(t1.TryGetValue(i, out var st1)) {                        // Entry exists in t1, we must sum.
               var sv1 = (Vector<τ,α>) st1;
               var res = sv1.SumIntoIntern(sv2);
               if(res == null)
                  t1.Remove(i); }                                           // Crucial to remove if subvector has been anihilated.
            else {
               sv2.CopyAsSubVecIntern(t1, i); } } }                                // Entry does not exist in t1, copy as SubVec.
      else {                                                                     // We have a vector.
         var v1 = (Vector<τ,α>) t1;
         var v2 = (Vector<τ,α>) t2;
         return v1.SumIntoIntern(v2); }
      if(t1.Count != 0)
         return t1;
      else
         return null;
   }

   public static Tensor<τ,α>? SumTop<τ,α>(this Tensor<τ,α>? t1, Tensor<τ,α>? t2)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() {
      if(t1 == null) {
         if(t2 == null)
            return null;
         else
            return t2.CopyAsTopTnr(); }
      else if(t2 == null)
         return t1.CopyAsTopTnr();
      return t1.SumTopIntern(t2);
   }

   internal static Tensor<τ,α>? SumTopIntern<τ,α>(this Tensor<τ,α> t1, Tensor<τ,α> t2)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() =>
      t1.CopyAsTopTnr().SumIntoIntern(t2);

   /// <summary>Subtracts tnr2 from the caller. Tnr2 is still usable afterwards.</summary>
   /// <param name="aTnr2">Minuend which will be subtracted from the caller. Minuend is still usable after the operation.</param>
   /// <remarks><see cref="TestRefs.TensorSub"/></remarks>
   public static Tensor<τ,α>? SubInto<τ,α>(this Tensor<τ,α>? t1, Tensor<τ,α>? t2)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() {
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

   public static Tensor<τ,α>? SubIntoIntern<τ,α>(this Tensor<τ,α> t1, Tensor<τ,α> t2)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() {
      if(t2.Rank > 2) {
         foreach(var (i, st2) in t2) {
            if(t1.TryGetValue(i, out var st1)) {                        // Equivalent subtensor exists in T1.
               var sub = SubIntoIntern(st1, st2);
               if(sub == null)
                  t1.Remove(i); }
            else                                                                 // Equivalent subtensor does not exist in T1. Copy the subtensor from T2 and negate it.
               st2.CopyAsSubTnr(t1, i).NegateIntoIntern(); } }
      else if(t2.Rank == 2) {
         foreach(var (i, st2) in t2) {
            var sv2 = (Vector<τ,α>) st2;
            if(t1.TryGetValue(i, out var st1)) {                        // Entry exists in t1, we must sum.
               var sv1 = (Vector<τ,α>) st1;
               var res = sv1.SumIntoIntern(sv2);
               if(res == null)
                  t1.Remove(i); }                                           // Crucial to remove if subvector has been anihilated.
            else {
               sv2.CopyAsSubVecIntern(t1, i); } } }                                // Entry does not exist in t1, copy as subvector.
      else {                                                                     // We have a vector.
         var v1 = (Vector<τ,α>) t1;
         var v2 = (Vector<τ,α>) t2;
         return v1.SubIntoIntern(v2); }
      if(t1.Count != 0)
         return t1;
      else
         return null;
   }

   public static Tensor<τ,α>? SubTop<τ,α>(this Tensor<τ,α>? t1, Tensor<τ,α>? t2)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() {
      if(t1 == null) {
         if(t2 == null)
            return null;
         else
            return t2.NegateTopIntern(); }
      else if(t2 == null)
         return t1.CopyAsTopTnr();
      return t1.SubTopIntern(t2);
   }

   internal static Tensor<τ,α>? SubTopIntern<τ,α>(this Tensor<τ,α> t1, Tensor<τ,α> t2)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() =>
      t1.CopyAsTopTnr().SubIntoIntern(t2);

   /// <summary>Modifies this tensor by negating each element.</summary>
   public static Tensor<τ,α>? NegateInto<τ,α>(this Tensor<τ,α>? t)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() {
      if(t == null)
         return null;
      return t.NegateIntoIntern();
   }

   internal static Tensor<τ,α> NegateIntoIntern<τ,α>(this Tensor<τ,α> t)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() {
      if(t.Rank > 2) {
         foreach(var (i, st) in t)
            t.NegateIntoIntern(); }
      else if(t.Rank == 2) {
         foreach(var i_st in t) {
            var sv = (Vector<τ,α>) i_st.Value;
            sv.NegateIntoIntern(); } }
      else {
         var v = (Vector<τ,α>) t;
         v.NegateIntoIntern(); }
      return t;
   }

   public static Tensor<τ,α>? NegateTop<τ,α>(this Tensor<τ,α>? t)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() {
      if(t == null)
         return null;
      else
         return t.CopyAsTopTnr().NegateIntoIntern();
   }

   internal static Tensor<τ,α>? NegateTopIntern<τ,α>(this Tensor<τ,α> t)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() =>
      t.CopyAsTopTnr().NegateIntoIntern();

   public static Tensor<τ,α>? MulInto<τ,α>(this Tensor<τ,α>? t, τ scal)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() {
      if(t == null || scal.Equals(O<τ,α>.A.Zero()))
         return null;
      else
         return MulIntoIntern(t, scal);
   }

   /// <summary>Multiplies caller with a scalar.</summary>
   /// <param name="scal">Scalar.</param>
   /// <remarks> <see cref="TestRefs.TensorMul"/> </remarks>
   internal static Tensor<τ,α> MulIntoIntern<τ,α>(this Tensor<τ,α> t, τ scal)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() {
      if(t.Rank > 2) {                                       // Subordinates are tensors.
         foreach (var (i, st) in t) {
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
   public static Tensor<τ,α>? MulTop<τ,α>(this Tensor<τ,α>? t, τ scal)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() {
      if(t == null || scal.Equals(O<τ,α>.A.Zero()))
         return null;
      else
         return t.MulTopIntern(scal);
   }
   /// <summary>Multiplies a scalar with a vector and returns a new vector. Unsafe version: accepts a non-nullable vector and does not check whether the scalar is zero.</summary>
   /// <param name="scal"></param>
   /// <param name="vec"></param>
   internal static Tensor<τ,α> MulTopIntern<τ,α>(this Tensor<τ,α> t, τ scal)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() =>
      t.CopyAsTopTnr().MulIntoIntern(scal);

   public static Tensor<τ,α>? MulSub<τ,α>(this Tensor<τ,α> t, τ scal, Tensor<τ,α> sup, int inx)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() {
      if(t == null || scal.Equals(O<τ,α>.A.Zero()))
         return null;
      else
         return t.MulSubIntern(scal, sup, inx);
   }
   
   /// <summary>Multiplies a scalar with a vector and returns a new vector. Unsafe version: accepts a non-nullable vector and does not check whether the scalar is zero.</summary>
   /// <param name="scal"></param>
   /// <param name="vec"></param>
   internal static Tensor<τ,α> MulSubIntern<τ,α>(this Tensor<τ,α> t, τ scal, Tensor<τ,α> sup, int inx)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() =>
      t.CopyAsSubTnrIntern(sup, inx).MulIntoIntern(scal);


   
   /// <remarks> <see cref="TestRefs.TensorProduct"/> </remarks>
   public static Tensor<τ,α> TnrProdTop<τ,α>(this Tensor<τ,α> t1, Tensor<τ,α> t2)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() {
      // 1) Descend to rank 1 through a recursion and then delete that vector.
      // 2) Substitute it with a tensor of rank tnr2.Rank + 1 whose entries are tnr2s multiplied by the corresponding scalar that used to preside there in the old vector.
      var strc1 = t1.GetSubstructure();
      var strc2 = t2.GetSubstructure();
      var newStrc = strc1.Concat(strc2).ToList();
      var prod = TopTensor<τ,α>(newStrc, t1.Count);             // Create the result (tensor product) tensor.
      return Recursion(prod, t1);

      // We reconstruct t1 until we reach vector rank. In place of a vector we put a new tensor that holds corresponding multiples of t2.
      Tensor<τ,α> Recursion(Tensor<τ,α> prod, Tensor<τ,α> t1) {
         if(t1.Rank > 1) {                                       // Only reconstruction going on.
            foreach(var (i, st1) in t1) {
               var sProd = prod.SubTensorIntern(i, st1.Count);         // Reconstructed st1 on prod. Creates vector appropriately
               Recursion(sProd, st1); } }                        // Re-enter.
         else {                                                   // Reached rank 1 on t1.
            var v1 = (Vector<τ,α>) t1;
            foreach(var (i, s1) in v1)
               t2.MulSubIntern(s1, prod, i); }
         return prod; }
   }


   /// <summary>Contracts across two slot indices on a single tensor of at least rank 3.</summary>
   /// <param name="slot1">Slot index 1.</param>
   /// <param name="slot2">Slot index 2.</param>
   /// <remarks><see cref="TestRefs.TensorSelfContract"/></remarks>
   internal static Tensor<τ,α> SelfContractTop<τ,α>(this Tensor<τ,α> top, int slot1, int slot2)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() {
      // Assume.True(Rank > 2, () =>
      //    "This method is not applicable to rank 2 tensors.");
      // Assume.True(Structure[slot1 - 1] == Structure[slot2 - 1], () =>
      //    "Dimensions of contracted slots have to be equal.");
      if(top.Rank > 3) {
         var strc1 = top.Structure.Take(slot1 - 1);
         var strc2 = top.Structure.Take(slot2 - 1).Skip(slot1);
         var strc3 = top.Structure.Skip(slot2);
         var strc = strc1.Concat(strc2).Concat(strc3).ToList();
         //var res = new Tensor<τ,α>(newStruct, Rank - 2, Voids<τ,α>.Vec, Count);
         var t = TopTensor<τ,α>(strc, top.Count);
         int rank1 = ChangeRankNotation(top, slot1);
         int rank2 = ChangeRankNotation(top, slot2);
         int dimRank = Structure[slot1 - 1];                // Dimension of contracted rank.
         for(int i = 0; i < dimRank; ++i) {                    // Over each element inside contracted ranks.
            var step1Tnr = ReduceRank(rank2, i);
            var sumand = step1Tnr.ReduceRank(rank1 - 1, i);
            res.Sum(sumand); }
         return res; }
      else
         return SelfContractR3(slot1, slot2);
   }

   internal static Vector<τ,α> SelfContractR3TopIntern<τ,α>(this Tensor<τ,α> top, int slot1, int slot2)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() {
      // Assume.True(Rank == 3, () => "Tensor rank has to be 3 for this method.");
      // Assume.True(Structure[slot1 - 1] == Structure[slot2 - 1], () =>
      //    "Corresponding dimensions have to be equal.");
      var v = TopVector<τ,α>(top.Structure[2], 4);
      if(slot1 == 1) {
         if(slot2 == 2) {
            foreach(var (i, t) in top) {
               if(t.TryGetValue(i, out var st)) {
                  var sv = (Vector<τ,α>) st;
                  v.SumIntoIntern(sv); } } }
         if(slot2 == 3) {
            foreach(var (i, t) in top) {
               foreach(var (j, st) in t) {
                  var sv = (Vector<τ,α>) st;
                  if(sv.Scals.TryGetValue(i, out τ s))
                     v.Scals[j] = O<τ,α>.A.Sum(v[j], s); } } } }
      else if(slot1 == 2) {                   // natInx2 == 3
         foreach(var (i, t) in top) {
            foreach(var (j, st) in t) {
               var subVec = (Vector<τ,α>) st;
               if(subVec.Scals.TryGetValue(j, out τ val))
                  v.Scals[i] = O<τ,α>.A.Sum(v[i], val); } } }
      return v;
   }

   /// <summary>Contracts across the two slot indices on a rank 2 tensor.</summary>
   /// <remarks> <see cref="TestRefs.TensorSelfContractR2"/> </remarks>
   internal static τ SelfContractR2TopIntern<τ,α>(this Tensor<τ,α> top)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() {
      // Assume.True(Rank == 2, () => "Tensor rank has to be 2 for this method.");
      // Assume.True(Structure[0] == Structure[1], () =>
      //    "Corresponding dimensions have to be equal.");
      τ result = O<τ,α>.A.Zero();
      foreach(var (i, st) in top) {
         var sv = (Vector<τ,α>) st;
         if(sv.Scals.TryGetValue(i, out τ s))
            result = O<τ,α>.A.Sum(result, s); }
      return result;
   }



   /// <summary>A method that transorms between slot index and rank index (works both ways).</summary>
   /// <param name="topRankInx">Rank of the top-most tensor in the hierarchy.</param>
   /// <param name="slotOrRankInx">Slot (e.g. A^ijk ==> 1,2,3) or rank (e.g. A^ijk ==> 2,1,0) index.</param>
   static int ChangeRankNotation<τ,α>(this Tensor<τ,α> topTnr, int slotOrRankInx)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() =>
      topTnr.Rank - slotOrRankInx;
}
}