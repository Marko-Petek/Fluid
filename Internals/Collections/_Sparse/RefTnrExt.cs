using System;
using System.Collections.Generic;
using System.Linq;
using Fluid.Internals.Numerics;
using static Fluid.Internals.Collections.RefTnrFactory;
using Lst = System.Collections.Generic.List<int>;

namespace Fluid.Internals.Collections {
public static class RefTnrExt {
   /// <summary>Compares substructures of two non-null tensors.</summary>
   /// <param name="tnr1">First tensor.</param>
   /// <param name="tnr2">Second tensor.</param>
   internal static bool CompareSubstrcß<τ,α>(this RefTnr<τ,α> t1, RefTnr<τ,α> t2)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() =>
      t1.Substrc.SequenceEqual(t2.Substrc);

   /// <summary>Sums t2 to t1, both assumed to be non-null. Modifies t1, does not destroy t2.</summary>
   /// <param name="t1">Tensor that will be modified.</param>
   /// <param name="t2">Tensor that will provide sumands.</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   /// <remarks> <see cref="TestRefs.TensorSum"/> </remarks>
   public static RefTnr<τ,α>? SumInto<τ,α>(this RefTnr<τ,α>? t1, RefTnr<τ,α>? t2)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      if(t1 == null) {
         if(t2 == null)
            return null;
         else
            return t2.CopyAsTopRefTnr(); }
      else if(t2 == null)
         return t1;

      Assume.True(t1.CompareSubstrcß(t2),
         () => "Tensor substructures do not match on Sum.");
      return SumIntoß(t1, t2);
   }

   /// <summary>Sums t2 to t1, both assumed to be non-null. Modifies t1, does not destroy t2. Does not check for a match between substructures.</summary>
   /// <param name="t1">Tensor that will be modified.</param>
   /// <param name="t2">Tensor that will provide sumands.</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   internal static RefTnr<τ,α>? SumIntoß<τ,α>(this RefTnr<τ,α> t1, RefTnr<τ,α> t2)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      if(t2.Rank > 2) {
         foreach(var (i, st2) in t2) {
            if(t1.TryGetValue(i, out var st1)) {                        // Equivalent subtensor exists in T1.
               var sum = SumIntoß(st1, st2);
               if(sum == null)
                  t1.Remove(i); }
            else                                                                 // Equivalent subtensor does not exist in T1. Copy the subtensor from T2 and add it.
               st2.CopyAsSubRefTnr(t1, i); } }
      else if(t2.Rank == 2) {
         foreach(var (i, st2) in t2) {
            var sv2 = (RefVec<τ,α>) st2;
            if(t1.TryGetValue(i, out var st1)) {                        // Entry exists in t1, we must sum.
               var sv1 = (RefVec<τ,α>) st1;
               var res = sv1.SumIntoß(sv2);
               if(res == null)
                  t1.Remove(i); }                                           // Crucial to remove if subvector has been anihilated.
            else {
               sv2.CopyAsSubRefVecß(t1, i); } } }                                // Entry does not exist in t1, copy as SubVec.
      else {                                                                     // We have a vector.
         var v1 = (RefVec<τ,α>) t1;
         var v2 = (RefVec<τ,α>) t2;
         return v1.SumIntoß(v2); }
      if(t1.Count != 0)
         return t1;
      else
         return null;
   }

   public static RefTnr<τ,α>? SumTop<τ,α>(this RefTnr<τ,α>? t1, RefTnr<τ,α>? t2)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      if(t1 == null) {
         if(t2 == null)
            return null;
         else
            return t2.CopyAsTopRefTnr(); }
      else if(t2 == null)
         return t1.CopyAsTopRefTnr();
      return t1.SumTopß(t2);
   }

   internal static RefTnr<τ,α>? SumTopß<τ,α>(this RefTnr<τ,α> t1, RefTnr<τ,α> t2)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() =>
      t1.CopyAsTopRefTnrß().SumIntoß(t2);

   /// <summary>Subtracts tnr2 from the caller. Tnr2 is still usable afterwards.</summary>
   /// <param name="aTnr2">Minuend which will be subtracted from the caller. Minuend is still usable after the operation.</param>
   /// <remarks><see cref="TestRefs.TensorSub"/></remarks>
   public static RefTnr<τ,α>? SubInto<τ,α>(this RefTnr<τ,α>? t1, RefTnr<τ,α>? t2)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      if(t1 == null) {
         if(t2 == null)
            return null;
         else
            return t2.CopyAsTopRefTnrß().NegateIntoß(); }
      else if(t2 == null)
         return t1;

      Assume.True(t1.CompareSubstrcß(t2),
         () => "Tensor substructures do not match on Sub.");
      return SubIntoß(t1, t2);
   }

   public static RefTnr<τ,α>? SubIntoß<τ,α>(this RefTnr<τ,α> t1, RefTnr<τ,α> t2)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      if(t2.Rank > 2) {
         foreach(var (i, st2) in t2) {
            if(t1.TryGetValue(i, out var st1)) {                        // Equivalent subtensor exists in T1.
               var sub = SubIntoß(st1, st2);
               if(sub == null)
                  t1.Remove(i); }
            else                                                                 // Equivalent subtensor does not exist in T1. Copy the subtensor from T2 and negate it.
               st2.CopyAsSubRefTnrß(t1, i).NegateIntoß(); } }
      else if(t2.Rank == 2) {
         foreach(var (i, st2) in t2) {
            var sv2 = (RefVec<τ,α>) st2;
            if(t1.TryGetValue(i, out var st1)) {                        // Entry exists in t1, we must sum.
               var sv1 = (RefVec<τ,α>) st1;
               var res = sv1.SubIntoß(sv2);
               if(res == null)
                  t1.Remove(i); }                                           // Crucial to remove if subvector has been anihilated.
            else {
               sv2.CopyAsSubRefVecß(t1, i).NegateIntoß(); } } }                                // Entry does not exist in t1, copy as subvector.
      else {                                                                     // We have a vector.
         var v1 = (RefVec<τ,α>) t1;
         var v2 = (RefVec<τ,α>) t2;
         return v1.SubIntoß(v2); }
      if(t1.Count != 0)
         return t1;
      else
         return null;
   }

   public static RefTnr<τ,α>? SubTop<τ,α>(this RefTnr<τ,α>? t1, RefTnr<τ,α>? t2)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      if(t1 == null) {
         if(t2 == null)
            return null;
         else
            return t2.NegateTopß(); }
      else if(t2 == null)
         return t1.CopyAsTopRefTnr();
      return t1.SubTopß(t2);
   }

   internal static RefTnr<τ,α>? SubTopß<τ,α>(this RefTnr<τ,α> t1, RefTnr<τ,α> t2)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() =>
      t1.CopyAsTopRefTnrß().SubIntoß(t2);

   /// <summary>Modifies this tensor by negating each element.</summary>
   public static RefTnr<τ,α>? NegateInto<τ,α>(this RefTnr<τ,α>? t)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      if(t == null)
         return null;
      return t.NegateIntoß();
   }

   internal static RefTnr<τ,α> NegateIntoß<τ,α>(this RefTnr<τ,α> t)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      if(t.Rank > 2) {
         foreach(var (i, st) in t)
            t.NegateIntoß(); }
      else if(t.Rank == 2) {
         foreach(var i_st in t) {
            var sv = (RefVec<τ,α>) i_st.Value;
            sv.NegateIntoß(); } }
      else {
         var v = (RefVec<τ,α>) t;
         v.NegateIntoß(); }
      return t;
   }

   public static RefTnr<τ,α>? NegateTop<τ,α>(this RefTnr<τ,α>? t)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      if(t == null)
         return null;
      else
         return t.CopyAsTopRefTnrß().NegateIntoß();
   }

   internal static RefTnr<τ,α>? NegateTopß<τ,α>(this RefTnr<τ,α> t)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() =>
      t.CopyAsTopRefTnrß().NegateIntoß();

   public static RefTnr<τ,α>? MulInto<τ,α>(this RefTnr<τ,α>? t, τ? scal)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      if(t == null || scal == null)
         return null;
      else
         return MulIntoß(t, scal);
   }

   /// <summary>Multiplies caller with a scalar.</summary>
   /// <param name="scal">Scalar.</param>
   /// <remarks> <see cref="TestRefs.TensorMul"/> </remarks>
   internal static RefTnr<τ,α> MulIntoß<τ,α>(this RefTnr<τ,α> t, τ scal)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      if(t.Rank > 2) {                                       // Subordinates are tensors.
         foreach (var (i, st) in t) {
            MulIntoß(st, scal); } }
      else if(t.Rank == 2) {                                 // Subordinates are vectors.
         foreach (var i_st in t) {
            var sv = (RefVec<τ,α>) i_st.Value;
            sv.MulInto(scal); } }
      else {
         var v = (RefVec<τ,α>) t;
         v.MulInto(scal); }
      return t;
   }

   /// <summary>Multiplies a scalar with a vector and returns a new vector. Safe version: accepts a nullable vector and checks it for null, also checks if the scalar is zero.</summary>
   /// <param name="scal">Scalar.</param>
   /// <param name="t">Vector.</param>
   public static RefTnr<τ,α>? MulTop<τ,α>(this RefTnr<τ,α>? t, τ? scal)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      if(t == null || scal == null)
         return null;
      else
         return t.MulTopß(scal);
   }
   /// <summary>Multiplies a scalar with a vector and returns a new vector. Unsafe version: accepts a non-nullable vector and does not check whether the scalar is zero.</summary>
   /// <param name="scal"></param>
   /// <param name="vec"></param>
   internal static RefTnr<τ,α> MulTopß<τ,α>(this RefTnr<τ,α> t, τ scal)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() =>
      t.CopyAsTopRefTnrß().MulIntoß(scal);

   public static RefTnr<τ,α>? MulSub<τ,α>(this RefTnr<τ,α>? t, τ? scal, RefTnr<τ,α> sup, int inx)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      if(t == null || scal == null)
         return null;
      else
         return t.MulSubß(scal, sup, inx);
   }
   
   /// <summary>Multiplies a scalar with a vector and returns a new vector. Unsafe version: accepts a non-nullable vector and does not check whether the scalar is zero.</summary>
   /// <param name="scal"></param>
   /// <param name="vec"></param>
   internal static RefTnr<τ,α> MulSubß<τ,α>(this RefTnr<τ,α> t, τ scal, RefTnr<τ,α> sup, int inx)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() =>
      t.CopyAsSubRefTnrß(sup, inx).MulIntoß(scal);


   
   /// <remarks> <see cref="TestRefs.TensorProduct"/> </remarks>
   public static RefTnr<τ,α> TnrProdTop<τ,α>(this RefTnr<τ,α> t1, RefTnr<τ,α> t2)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      // 1) Descend to rank 1 through a recursion and then delete that vector.
      // 2) Substitute it with a tensor of rank tnr2.Rank + 1 whose entries are tnr2s multiplied by the corresponding scalar that used to preside there in the old vector.
      var strc1 = t1.EnumSubstrc();
      var strc2 = t2.EnumSubstrc();
      var newStrc = strc1.Concat(strc2).ToList();
      var prod = TopRefTnr<τ,α>(newStrc, t1.Count);             // Create the result (tensor product) tensor.
      return Recursion(prod, t1);

      // We reconstruct t1 until we reach vector rank. In place of a vector we put a new tensor that holds corresponding multiples of t2.
      RefTnr<τ,α> Recursion(RefTnr<τ,α> prod, RefTnr<τ,α> t1) {
         if(t1.Rank > 1) {                                       // Only reconstruction going on.
            foreach(var (i, st1) in t1) {
               var sProd = prod.SubRefTnrß(i, st1.Count);         // Reconstructed st1 on prod. Creates vector appropriately
               Recursion(sProd, st1); } }                        // Re-enter.
         else {                                                   // Reached rank 1 on t1.
            var v1 = (RefVec<τ,α>) t1;
            foreach(var (i, s1) in v1)
               t2.MulSubß(s1, prod, i); }
         return prod; }
   }

   internal static (List<int> strc, int rank1, int rank2, int conDim) ContractTopPart1<τ,α>(
   this RefTnr<τ,α> t1, RefTnr<τ,α> t2, int slot1, int slot2)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      // 1) First eliminate, creating new tensors. Then add them together using tensor product.
      var strc1 = t1.Substrc;
      var strc2 = t2.Substrc;
      // int rank1 = struc1.Count,
      //       rank2 = struc2.Count;
      // Assume.True(rank1 == t1.Rank && rank2 == t2.Rank,
      //    () => "One of the tensors is not top rank.");
      Assume.AreEqual(strc1[slot1 - 1], strc2[slot2 - 1],              // Check that the dimensions of contracted ranks are equal.
         "Rank dimensions at specified indices must be equal.");
      int   conDim = strc1[slot1 - 1],                                // Dimension of rank we're contracting.
            rankInx1 = ChangeRankNotation<τ,α>(t1.Rank, slot1),
            rankInx2 = ChangeRankNotation<τ,α>(t2.Rank, slot2);
      var strc_1 = strc1.Where((emt, i) => i != (slot1 - 1));
      var strc_2 = strc2.Where((emt, i) => i != (slot2 - 1));
      var strc = strc_1.Concat(strc_2).ToList();                 // New structure.
      return (strc, rankInx1, rankInx2, conDim);
   }
   
   internal static RefTnr<τ,α>? ContractTopPart2<τ,α>(this RefTnr<τ,α> t1, RefTnr<τ,α> t2,
   int rankInx1, int rankInx2, List<int> strc, int conDim)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      // 1) First eliminate, creating new tensors. Then combine them together using tensor product.
      if(t1.Rank > 1) {
         if(t2.Rank > 1) {                                // First tensor is rank 2 or more.
            RefTnr<τ,α>? elimTnr1, elimTnr2;
            var sum = TopRefTnr<τ,α>(strc);                                    // Set sum to a zero tensor.
            for(int i = 0; i < conDim; ++i) {
               elimTnr1 = t1.ReduceRankTop(rankInx1, i);
               elimTnr2 = t2.ReduceRankTop(rankInx2, i);
               if(elimTnr1 != null && elimTnr2 != null) {
                  var sumand = elimTnr1.TnrProdTop(elimTnr2);
                  sum.SumIntoß(sumand); } }
            if(sum.Count != 0)
               return sum;
            else
               return null; }
         else {                                                // Second tensor is rank 1 (a vector).
            RefVec<τ,α> v2 = (RefVec<τ,α>) t2;
            if(t1.Rank == 2) {                                    // Result will be vector.
               RefVec<τ,α>? elimVec, sumand;
               var sum = TopRefVec<τ,α>(strc[0]);
               for(int i = 0; i < conDim; ++i) {
                  elimVec = (RefVec<τ,α>?) t1.ReduceRankTop(rankInx1, i);
                  if(elimVec != null && v2.Scals.TryGetValue(i, out var s)) {
                     sumand = elimVec.MulTopß(s);
                     sum.SumIntoß(sumand); } }
               if(sum.Count != 0)
                  return sum;
               else
                  return null; }
            else {                                             // Result will be tensor.
               RefTnr<τ,α>? elimTnr1, sumand;
               var sum = TopRefTnr<τ,α>(strc);
               for(int i = 0; i < conDim; ++i) {
                  elimTnr1 = t1.ReduceRankTop(rankInx1, i);
                  if(elimTnr1 != null && v2.Scals.TryGetValue(i, out var s)) {
                     sumand = elimTnr1.MulTopß(s);
                     sum.SumIntoß(sumand); } }
               if(sum.Count != 0)
                  return sum;
               else
                  return null; } } }
      else {                                                   // First tensor is rank 1 (a vector).
         var v1 = (RefVec<τ,α>) t1;
         return v1.ContractTopPart2(t2, rankInx2, strc, conDim);}
   }

   /// <summary>Contracts two tensors over specified natural rank indices. Example: Contraction writen as A^(ijkl)B^(mnip) is specified as a (0,2) contraction of A and B, not a (3,1) contraction. Tensor contraction is a generalization of trace, which can further be viewed as a generalization of dot product.</summary>
   /// <param name="t2">Tensor 2.</param>
   /// <param name="slotInx1">One-based natural index on this tensor over which to contract.</param>
   /// <param name="slotInx2">One-based natural index on tensor 2 over which to contract (it must hold: dim(rank(inx1)) = dim(rank(inx2)).</param>
   /// <remarks><see cref="TestRefs.TensorContract"/></remarks>
   public static RefTnr<τ,α>? ContractTopß<τ,α>(this RefTnr<τ,α> t1, RefTnr<τ,α> t2, int slotInx1, int slotInx2)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      (List<int> strc, int rank1, int rank2, int conDim) = ContractTopPart1(t1, t2, slotInx1, slotInx2);
      return ContractTopPart2(t1, t2, rank1, rank2, strc, conDim);
   }

   public static RefTnr<τ,α>? ContractTop<τ,α>(this RefTnr<τ,α>? t1, RefTnr<τ,α>? t2, int slotInx1, int slotInx2)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      if(t1 == null || t2 == null)
         return null;
      (List<int> strc, int rank1, int rank2, int conDim) = ContractTopPart1(t1, t2, slotInx1, slotInx2);
      return ContractTopPart2(t1, t2, rank1, rank2, strc, conDim);
   }

   internal static RefTnr<τ,α>? SelfContractTop<τ,α>(this RefTnr<τ,α>? t, int slot1, int slot2)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      if(t == null)
         return null;
      Assume.True(t.Rank > 2, () =>
         "This method is not applicable to rank 2 tensors.");
      return t.SelfContractTopß<τ,α>(slot1, slot2);
   }


   /// <summary>Contracts across two slot indices on a single tensor of at least rank 3. Tensors that are subtensors of another are handled correctly.</summary>
   /// <param name="t">Tensor to self-contract.</param>
   /// <param name="slot1">Slot index 1. Provide as if contracted tensor was top.</param>
   /// <param name="slot2">Slot index 2, greater than slot index 1. Provide as if contracted tensor was top.</param>
   /// <remarks><see cref="TestRefs.TensorSelfContract"/></remarks>
   internal static RefTnr<τ,α>? SelfContractTopß<τ,α>(this RefTnr<τ,α> t, int slot1, int slot2)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      if(t.Rank > 3) {
         var substrc = t.Substrc;                                                 // Substructure of contracted tensor.
         Assume.True(substrc[slot1 - 1] == substrc[slot2 - 1], () =>
            "Dimensions of contracted slots have to be equal.");
         var strc = substrc.Where( (x,i) => i != slot1 - 1 && i != slot2 - 1 ).ToList();  // Structure of contracted tensor.
         var nt = TopRefTnr<τ,α>(strc, t.Count);
         int rank1 = ChangeRankNotation<τ,α>(substrc.Count, slot1);               // Ranks for rank reduction are specified with regards to original tensor.
         int rank2 = ChangeRankNotation<τ,α>(substrc.Count, slot2);
         int dim = strc[slot1 - 1];                // Dimension of contracted ranks.
         for(int i = 0; i < dim; ++i) {                    // Over each element inside contracted ranks.
            var step1Tnr = t.ReduceRankTop(rank2, i);
            if(step1Tnr != null) {
               var sumand = step1Tnr.ReduceRankTop(rank1 - 1, i);
               if(sumand != null)
                  nt.SumIntoß(sumand); } }
         if(nt.Count != 0)
            return nt;
         else
            return null; }
      else
         return t.SelfContractR3Topß(slot1, slot2);
   }

   internal static RefVec<τ,α>? SelfContractR3Topß<τ,α>(this RefTnr<τ,α> t, int slot1, int slot2)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      // Assume.True(Rank == 3, () => "Tensor rank has to be 3 for this method.");
      var substrc = t.Substrc;
      Assume.True(substrc[slot1 - 1] == substrc[slot2 - 1], () =>
         "Corresponding dimensions have to be equal.");
      int dim;
      RefVec<τ,α> nv;
      if(slot1 == 1) {
         if(slot2 == 2) {
            dim = substrc[2];
            nv = TopRefVec<τ,α>(dim);
            foreach(var (i, st) in t) {
               if(st.TryGetValue(i, out var sst)) {
                  var ssv = (RefVec<τ,α>) sst;
                  nv.SumIntoß(ssv); } } }
         else {                                            // Implies slot2 == 3
            dim = substrc[1];
            nv = TopRefVec<τ,α>(dim);
            foreach(var (i, st) in t) {
               foreach(var (j, sst) in st) {
                  var ssv = (RefVec<τ,α>) sst;
                  if(ssv.Scals.TryGetValue(i, out τ? s))
                     nv[j] = Nullable<τ,α>.O.Sum(nv[j], s); } } } }
      else {                                                         // Implies  slot1 == 2, slot2 == 3
         dim = substrc[0];
         nv = TopRefVec<τ,α>(dim);
         foreach(var (i, st) in t) {
            foreach(var (j, sst) in st) {
               var ssv = (RefVec<τ,α>) sst;
               if(ssv.Scals.TryGetValue(j, out τ? s))
                  nv[i] = Nullable<τ,α>.O.Sum(nv[i], s); } } }
      if(nv.Count != 0)
         return nv;
      else
         return null;
   }

   internal static τ? SelfContractR2<τ,α>(this RefTnr<τ,α>? t)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      if(t == null)
         return Nullable<τ,α>.O.Zero();
      Assume.True(t.Rank == 2, () => "Tensor rank has to be 2 for this method.");
      return t.SelfContractR2ß<τ,α>();
   }

   /// <summary>Contracts across the two slot indices on a rank 2 tensor.</summary>
   /// <remarks> <see cref="TestRefs.TensorSelfContractR2"/> </remarks>
   internal static τ? SelfContractR2ß<τ,α>(this RefTnr<τ,α> t)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      var substrc = t.Substrc;
      Assume.True(substrc[0] == substrc[1], () =>
         "Corresponding dimensions have to be equal.");
      τ? result = Nullable<τ,α>.O.Zero();
      foreach(var (i, st) in t) {
         var sv = (RefVec<τ,α>) st;
         if(sv.Scals.TryGetValue(i, out τ? s))
            result = Nullable<τ,α>.O.Sum(result, s); }
      return result;
   }

   /// <summary>A method that transorms between slot index and rank index (works both ways).</summary>
   /// <param name="topRankInx">Rank of the top-most tensor in the hierarchy.</param>
   /// <param name="slotOrRankInx">Slot (e.g. A^ijk ==> 1,2,3) or rank (e.g. A^ijk ==> 2,1,0) index.</param>
   static int ChangeRankNotation<τ,α>(int topRank, int slotOrRankInx)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() =>
      topRank - slotOrRankInx;

   /// <summary>Eliminates a specific rank n by choosing a single tensor at that rank and substituting it in place of its direct superior (thus discarding all other tensors at rank n). The resulting tensor is a new tensor of reduced rank (method is non-destructive).</summary>
   /// <param name="r"> Rank index (zero-based) of the rank to eliminate.</param>
   /// <param name="e">Element index (zero-based) in that rank in favor of which the elimination will take place.</param>
   /// <remarks>Test: <see cref="TestRefs.TensorReduceRank"/></remarks>
   public static RefTnr<τ,α>? ReduceRankTop<τ,α>(this RefTnr<τ,α> t, int r, int e)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {                                    // FIXME: Method must properly reassign superiors.
      Assume.True(r < t.Rank && r > -1, () =>
         "You can only eliminate a rank in range [0, TopRank)." );
      var subStrc = t.Substrc;                                          // t can be a non-top tensor.
      var strcL = subStrc.Take(r);
      var strcR = subStrc.Skip(r + 1);
      var strc = strcL.Concat(strcR).ToList();
      if(r == t.Rank - 1) {                                                           // Only one rank exists above rank r. Pick one tensor from rank r and return it.
         if(t.Rank > 1) {                                                             // Tensor to be reduced is at least rank two.
            if(t.TryGetValue(e, out var st))                                    // Element exists.
               return st.CopyAsTopRefTnr();
            else
               return null; }
         else                                                                       // Rank <= 1: impossible.
            throw new ArgumentException(
               "Cannot eliminate rank 1 or lower on rank 1 tensor."); }
      else if(r > 0) {                                                       // At least two ranks exist above r & r is at least 1. Obviously applicable only to Rank 3 or higher tensors.
         if(t.Rank > 2)                                                              // No special treatment due to Vector needed.
            return t.ElimR1hOnR3hTopß(e, r);
         else
            throw new ArgumentException("Cannot eliminate rank 1h on rank 2l tensor with this branch."); }
      else {                                          // At least two ranks exist above elimRank & elimRank is 0. Obviously applicable only to rank 2 or higher tensors.
         if(t.Rank > 2)                               // Result is tensor. Choose one value from each vector in subordinate rank 2 tensors, build a new vector and add those values to it. Then add that vector to superior rank 3 tensor.
            return t.ElimR0onR3hTopß(e);
         else if(t.Rank == 2)
            return t.ElimR0onR2Topß(e);
         else
            throw new ArgumentException("Cannot eliminate rank 0 on rank 1 tensor with this branch."); }
   }

   public static RefTnr<τ,α>? ReduceRankSub<τ,α>(this RefTnr<τ,α> t, int r, int e, RefTnr<τ,α> sup, int inx)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {                                    // FIXME: Method must properly reassign superiors.
      Assume.True(r < t.Rank && r > -1, () =>
         "You can only eliminate a rank in range [0, TopRank)." );
      var subStrc = t.Substrc;                                          // t can be a non-top tensor.
      var strcL = subStrc.Take(r);
      var strcR = subStrc.Skip(r + 1);
      var strc = strcL.Concat(strcR).ToList();
      Assume.True(sup.Substrc.SequenceEqual(strc), () =>
         "Specified superior has non-matching substructure.");
      if(r == t.Rank - 1) {                                                           // Only one rank exists above rank r. Pick one tensor from rank r and return it.
         if(t.Rank > 1) {                                                             // Tensor to be reduced is at least rank two.
            if(t.TryGetValue(e, out var st))                                    // Element exists.
               return st.CopyAsSubRefTnrß(sup, inx);
            else
               return null; }
         else                                                                       // Rank <= 1: impossible.
            throw new ArgumentException(
               "Cannot eliminate rank 1 or lower on rank 1 tensor."); }
      else if(r > 0) {                                                       // At least two ranks exist above r & r is at least 1. Obviously applicable only to Rank 3 or higher tensors.
         if(t.Rank > 2)                                                              // No special treatment due to Vector needed.
            return t.ElimR1hOnR3hSubß(e, r, sup, inx);
         else
            throw new ArgumentException("Cannot eliminate rank 1h on rank 2l tensor with this branch."); }
      else {                                          // At least two ranks exist above elimRank & elimRank is 0. Obviously applicable only to rank 2 or higher tensors.
         if(t.Rank > 2)                               // Result is tensor. Choose one value from each vector in subordinate rank 2 tensors, build a new vector and add those values to it. Then add that vector to superior rank 3 tensor.
            return t.ElimR0onR3hSubß(e, sup, inx);
         else if(t.Rank == 2)
            return t.ElimR0onR2Subß(e, sup, inx);
         else
            throw new ArgumentException("Cannot eliminate rank 0 on rank 1 tensor with this branch."); }
   }

   /// <summary>Eliminates rank 0 on a rank 2 tensor, resulting in a rank 1 tensor (vector),</summary>
   /// <param name="src">Rank 2 tensor.</param>
   /// <param name="tgt">Initialized result vector.</param>
   /// <param name="emtInx">Element index in favor of which the elimination will proceed.</param>
   public static RefVec<τ,α>? ElimR0onR2Top<τ,α>(this RefTnr<τ,α> t, int emtInx)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      Assume.True(t.Rank == 2, () =>
         "This method is intended for rank 2 tensors only.");
      return t.ElimR0onR2Topß(emtInx);
   }

   internal static RefVec<τ,α>? ElimR0onR2Topß<τ,α>(this RefTnr<τ,α> t, int emtInx)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      var nv = TopRefVec<τ,α>(t.Dim);
      foreach(var (i,st) in t) {
         var sv = (RefVec<τ,α>) st;
         if(sv.TryGetValue(emtInx, out var s)) {
            if(s != null)
               nv.Add(i,s); } }
      if(nv.Count != 0)
         return nv;
      else
         return null;
   }

   public static RefVec<τ,α>? ElimR0onR2Sub<τ,α>(this RefTnr<τ,α> t, int elimInx, RefTnr<τ,α> sup, int inx)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      Assume.True(t.Rank == 2, () =>
         "This method is intended for rank 2 tensors only.");
      var strc = t.Substrc.SkipLast(1).ToList();
      Assume.True(sup.Substrc.SequenceEqual(strc), () =>
         "Specified superior has non-matching substructure.");
      return t.ElimR0onR2Subß(elimInx, sup, inx);
   }

   internal static RefVec<τ,α>? ElimR0onR2Subß<τ,α>(this RefTnr<τ,α> t, int elimInx, RefTnr<τ,α> sup, int inx)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      var nv = sup.SubRefVec<τ,α>(inx);
      foreach(var (i,st) in t) {
         var sv = (RefVec<τ,α>) st;
         if(sv.TryGetValue(elimInx, out var s)) {
            if(s != null)
               nv.Add(i,s); } }
      if(nv.Count != 0)
         return nv;
      else {
         sup.Remove(inx);
         return null; }
   }

   /// <summary>Eliminate rank 0 on a rank 3 or higher tensor resulting in a one rank lower tensor.</summary>
   /// <param name="t">Rank 3 or higher tensor.</param>
   /// <param name="tgt">Tensor one rank lower than source.</param>
   /// <param name="emtInx">Element index in favor of which to eliminate.</param>
   public static RefTnr<τ,α>? ElimR0onR3hTop<τ,α>(this RefTnr<τ,α> t, int elimInx)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      Assume.True(t.Rank > 2, () =>
         "This method is applicable to rank 3 and higher tensors.");
      return t.ElimR0onR3hTopß(elimInx);
   }

   internal static RefTnr<τ,α>? ElimR0onR3hTopß<τ,α>(this RefTnr<τ,α> t, int elimInx)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      var strc = t.Substrc.SkipLast(1).ToList();
      var nt = TopRefTnr<τ,α>(strc);
      if(t.Rank > 3) {
         foreach(var (i,st) in t) {
            st.ElimR0onR3hSubß(elimInx, nt, i); } }
      else {                                                                  // src.Rank == 3.
         foreach(var (i,st) in t) {
            st.ElimR0onR2Subß(elimInx, nt, i); } }
      if(nt.Count != 0)
         return nt;
      else
         return null;
   }

   public static RefTnr<τ,α>? ElimR0onR3hSub<τ,α>(this RefTnr<τ,α> t, int elimInx, RefTnr<τ,α> sup, int inx)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      Assume.True(t.Rank > 2, () =>
         "This method is intended for rank 3 tensors or higher.");
      var strc = t.Substrc.SkipLast(1).ToList();
      Assume.True(sup.Substrc.SequenceEqual(strc), () =>
         "Specified superior has non-matching substructure.");
      return t.ElimR0onR3hSubß(elimInx, sup, inx);
   }

   internal static RefTnr<τ,α>? ElimR0onR3hSubß<τ,α>(this RefTnr<τ,α> t, int elimInx, RefTnr<τ,α> sup, int inx)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      var nt = sup.SubRefTnrß<τ,α>(inx, t.Count);
      if(t.Rank > 3) {
         foreach(var (i,st) in t)
            st.ElimR0onR3hSubß(elimInx, nt, i); }
      else {                                                                  // src.Rank == 3.
         foreach(var (i,st) in t)
            st.ElimR0onR2Subß(elimInx, nt, i); }
      if(nt.Count != 0)
         return nt;
      else {
         sup.Remove(inx);
         return null; }
   }

   public static RefTnr<τ,α>? ElimR1hOnR3hTop<τ,α>(this RefTnr<τ,α> t, int e, int r)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      Assume.True(t.Rank > 3, () =>
         "This method is applicable to rank 3 and higher tensors.");
      return t.ElimR1hOnR3hTopß(e,r);
   }

   internal static RefTnr<τ,α>? ElimR1hOnR3hTopß<τ,α>(this RefTnr<τ,α> t, int e, int r)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {      // Recursively copies tensors.
      var subStrc = t.Substrc;                                          // t can be a non-top tensor.
      var strcL = subStrc.Take(r);
      var strcR = subStrc.Skip(r + 1);
      var strc = strcL.Concat(strcR).ToList();
      var nt = TopRefTnr<τ,α>(strc);
      
      if(t.Rank > r + 2) {                                    // We have not yet reached rank directly above rank scheduled for elimination: copy rank.
         foreach(var (i,st) in t)
            st.ElimR1hOnR3hSubß(e, r, nt, i); }
      else {                                                             // We have reached rank directly above rank scheduled for elimination: eliminate.
         foreach(var (i,st) in t) {
            if(st.TryGetValue(e, out var sst))
               sst.CopyAsSubRefTnrß(nt, i); } }
      if(nt.Count != 0)
         return nt;
      else
         return null;
   }

   public static RefTnr<τ,α>? ElimR1hOnR3hSub<τ,α>(this RefTnr<τ,α> t, int e, int r, RefTnr<τ,α> sup, int inx)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      Assume.True(t.Rank > 2, () =>
         "This method is intended for rank 3 tensors or higher.");
      var subStrc = t.Substrc;                                          // t can be a non-top tensor.
      var strcL = subStrc.Take(r);
      var strcR = subStrc.Skip(r + 1);
      var strc = strcL.Concat(strcR).ToList();
      Assume.True(sup.Substrc.SequenceEqual(strc), () =>
         "Specified superior has non-matching substructure.");
      return t.ElimR1hOnR3hSubß(e, r, sup, inx);
   }

   /// <summary>Can only be used to eliminate rank 3 or higher. Provided target has to be initiated one rank lower than source.</summary>
   /// <param name="t">Source tensor whose rank we are eliminating.</param>
   /// <param name="tgt">Target tensor. Has to be one rank lower than source.</param>
   /// <param name="e">Element index in favor of which we are eliminating.</param>
   /// <param name="r"></param>
   internal static RefTnr<τ,α>? ElimR1hOnR3hSubß<τ,α>(this RefTnr<τ,α> t, int e, int r, RefTnr<τ,α> sup, int inx)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {      // Recursively copies tensors.
      var nt = sup.SubRefTnrß<τ,α>(inx, t.Count);
      
      if(t.Rank > r + 2) {                                    // We have not yet reached rank directly above rank scheduled for elimination: copy rank.
         foreach(var (i,st) in t)
            st.ElimR1hOnR3hSubß(e, r, nt, i); }
      else {                                                             // We have reached rank directly above rank scheduled for elimination: eliminate.
         foreach(var (i,st) in t) {
            if(st.TryGetValue(e, out var sst))
               sst.CopyAsSubRefTnrß(nt, i); } }
      if(nt.Count != 0)
         return nt;
      else {
         sup.Remove(inx);
         return null; }
   }

   /// <summary>Static implementation to allow for null comparison. If two tensors are null they are equal.</summary>
   /// <param name="t1">Tensor 1.</param>
   /// <param name="t2">Tensor 2.</param>
   public static bool EqualS<τ,α>(this RefTnr<τ,α>? t1, RefTnr<τ,α>? t2)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      if(t1 == null) {                                                            // If both are null, return true. If only one of them is null, return false.
         if(t2 == null)
            return true;
         else
            return false; }
      else if(t2 == null)
         return false;
      if(!t1.CompareSubstrcß(t2))                                         // If substructures mismatch, they are not equal.
         return false;
      return TnrRecursion(t1, t2);

      static bool TnrRecursion(RefTnr<τ,α> t1, RefTnr<τ,α> t2) {                       // Recursion must be entered with non-null tensors.
         if(!t1.Keys.OrderBy(key => key).SequenceEqual(t2.Keys.OrderBy(key => key)))
            return false;                                                                    // Keys have to match. This is crucial.
         if(t1.Rank > 2) {
            foreach(var (i,st1) in t1) {
               var st2 = t2.GetNonNullTnr(i);
               return TnrRecursion(st1, st2); }
            return true; }                                                                   // Both are empty.
         else
            return VecRecursion(t1, t2); }

      static bool VecRecursion(RefTnr<τ,α> t1r2, RefTnr<τ,α> t2r2) {
         if(!t1r2.Keys.OrderBy(key => key).SequenceEqual(t2r2.Keys.OrderBy(key => key)))
            return false;                                                                    // Keys have to match. This is crucial.
         foreach(var (i,st1) in t1r2) {
            var v1 = (RefVec<τ,α>) st1;
            var v2 = t2r2.GetNonNullVec(i);
            if(!v1.Equals(v2))
               return false; }
         return true; }
   }

   /// <remarks> <see cref="TestRefs.TensorEquals"/> </remarks>
   public static bool Equals<τ,α>(RefTnr<τ,α>? t1, RefTnr<τ,α>? t2, τ eps)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      if(t1 == null) {                                                            // If both are null, return true. If only one of them is null, return false.
         if(t2 == null)
            return true;
         else
            return false; }
      else if(t2 == null)
         return false;
      if(!t1.CompareSubstrcß(t2))                                         // If substructures mismatch, they are not equal.
         return false;
      return TnrRecursion(t1, t2, eps);

      static bool TnrRecursion(RefTnr<τ,α> t1, RefTnr<τ,α> t2, τ eps) {                       // Recursion must be entered with non-null tensors.
         if(!t1.Keys.OrderBy(key => key).SequenceEqual(t2.Keys.OrderBy(key => key)))
            return false;                                                                    // Keys have to match. This is crucial.
         if(t1.Rank > 2) {
            foreach(var (i,st1) in t1) {
               var st2 = t2.GetNonNullTnr(i);
               return TnrRecursion(st1, st2, eps); }
            return true; }                                                                   // Both are empty.
         else
            return VecRecursion(t1, t2, eps); }

      static bool VecRecursion(RefTnr<τ,α> t1r2, RefTnr<τ,α> t2r2, τ eps) {
         if(!t1r2.Keys.OrderBy(key => key).SequenceEqual(t2r2.Keys.OrderBy(key => key)))
            return false;                                                                    // Keys have to match. This is crucial.
         foreach(var (i,st1) in t1r2) {
            var v1 = (RefVec<τ,α>) st1;
            var v2 = t2r2.GetNonNullVec(i);
            if(!v1.Equals(v2, eps))
               return false; }
         return true; }                                                         // All values agree within tolerance.
   }
}
}