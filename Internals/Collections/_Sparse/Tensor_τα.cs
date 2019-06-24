/*  
 ██████╗  █████╗ ███╗   ██╗██╗  ██╗    ██╗███╗   ██╗██████╗ ██╗ ██████╗███████╗███████╗
 ██╔══██╗██╔══██╗████╗  ██║██║ ██╔╝    ██║████╗  ██║██╔══██╗██║██╔════╝██╔════╝██╔════╝
 ██████╔╝███████║██╔██╗ ██║█████╔╝     ██║██╔██╗ ██║██║  ██║██║██║     █████╗  ███████╗
 ██╔══██╗██╔══██║██║╚██╗██║██╔═██╗     ██║██║╚██╗██║██║  ██║██║██║     ██╔══╝  ╚════██║
 ██║  ██║██║  ██║██║ ╚████║██║  ██╗    ██║██║ ╚████║██████╔╝██║╚██████╗███████╗███████║
 ╚═╝  ╚═╝╚═╝  ╚═╝╚═╝  ╚═══╝╚═╝  ╚═╝    ╚═╝╚═╝  ╚═══╝╚═════╝ ╚═╝ ╚═════╝╚══════╝╚══════╝
   We have two notations for rank indices. Let N be the tensor's top rank:
   - Slot notation:
      [1, N] which is how mathematicians would assign ordering to tensor's slots.
   - Rank notation:
      [0, N-1] where the value corresponds to the rank of tensors held by that slot.
   
   Relation between notations:
   Slot 1 holds tensors of rank N-1.
   Slot 2 holds tensors of rank N-2.
   ...
   Slot N-1 holds tensors of rank 1.
   Slot N holds tensors of rank 0.

   Relation is therefore: R = N - S  or  S = N - R.
    
 ██████╗  █████╗ ███╗   ██╗██╗  ██╗    ██████╗ ███████╗██████╗ ██╗   ██╗ ██████╗████████╗██╗ ██████╗ ███╗   ██╗
 ██╔══██╗██╔══██╗████╗  ██║██║ ██╔╝    ██╔══██╗██╔════╝██╔══██╗██║   ██║██╔════╝╚══██╔══╝██║██╔═══██╗████╗  ██║
 ██████╔╝███████║██╔██╗ ██║█████╔╝     ██████╔╝█████╗  ██║  ██║██║   ██║██║        ██║   ██║██║   ██║██╔██╗ ██║
 ██╔══██╗██╔══██║██║╚██╗██║██╔═██╗     ██╔══██╗██╔══╝  ██║  ██║██║   ██║██║        ██║   ██║██║   ██║██║╚██╗██║
 ██║  ██║██║  ██║██║ ╚████║██║  ██╗    ██║  ██║███████╗██████╔╝╚██████╔╝╚██████╗   ██║   ██║╚██████╔╝██║ ╚████║
 ╚═╝  ╚═╝╚═╝  ╚═╝╚═╝  ╚═══╝╚═╝  ╚═╝    ╚═╝  ╚═╝╚══════╝╚═════╝  ╚═════╝  ╚═════╝   ╚═╝   ╚═╝ ╚═════╝ ╚═╝  ╚═══╝
   Rank reduction reduces a tensor's rank(R) by 1, therefore input tensor has to be at least R2. We eliminate a single rank in favor of an element(E) inside that rank at a specific index. Let's say we eliminate R2 in favor of E3. Imagine the whole tensor as a hierarchy, specifically, imagine R4 tensors laid out in a line as nodes, below them all R3 tensors and below those all R2 tensors as nodes. R3 tensors are connected to their respective R4 superiors and R2 tensors to their respective R3 superiors. We stop by each R3 node and choose its subordinate R2E3 (its whole branch). Now we substitute the R3 tensor we stopped by, with the chosen R2 tensor. That means each rank 3 node is removed and replaced by the chosen R2 subordinate. Therefore R3 becomes R2 and R4 becomes R3 - the whole tensor's rank is reduced by 1.

   Lowest rank we can eliminate: 0. Choose a R0Ei element for each R1Ej, wipe out entire R1 line this way. If R0Ej had a superior, add R0E1 to it.
   Highest rank we can eliminate: N. Choose the RNEi element and return it.

   Elimination process is dependent on how many ranks exist abouve the eliminated rank i:
      2+: Choose a RiEx element on each RjEy, let RiEx take RjEy's place. Wipe out entire Rj line this way. Add RiEx to RjEy's superior RkEz.
      1: Choose a RiEx element on RNE0 (the only element) and return it.
      0: Throw exception.
*/

using System;
using System.Text;
using System.Linq;
using SCG = System.Collections.Generic;
using TB = Fluid.Internals.Toolbox;
using static Fluid.Internals.Numerics.MatOps;
using Fluid.Internals.Numerics;

namespace Fluid.Internals.Collections {
   using IA = IntArithmetic;
   /// <summary>A tensor with specified rank and specified dimension which holds direct subordinates of type τ.</summary>
   /// <typeparam name="τ">Type of direct subordinates.</typeparam>
   public class Tensor<τ,α> : TensorBase<Tensor<τ,α>>, IEquatable<Tensor<τ,α>>
   where τ : IEquatable<τ>, IComparable<τ>, new()
   where α : IArithmetic<τ>, new() {
      /// <summary>Hierarchy's dimensional structure. First element specifies host's (tensor highest in hierarchy) rank, while last element specifies the rank of values. E.g.: {3,2,6,5} specifies structure of a tensor of 4th rank with first rank equal to 5 and last rank to 3.</summary>
      public int[] Structure { get; protected set; }
      /// <summary>Rank specifies the height (level) in the hierarchy on which the tensor sits. It equals the number of levels that exist below it. It tells us how many indices we must specify before we reach the value level.</summary>
      public int Rank { get; protected set; }
      /// <summary>Superior: a tensor directly above in the hierarchy. Null if this is the highest rank tensor.</summary>
      public Tensor<τ,α> Sup { get; protected set; }

      public static class CopySpecs {
         public static readonly CopySpecStruct Default = new CopySpecStruct();
         public static readonly CopySpecStruct AddSubtract = new CopySpecStruct(
            GeneralSpecs.Both, MetaSpecs.All, StructureSpecs.TrueCopy, 0, 4);
         //public static readonly CopySpecStruct ScalarMultiply = new CopySpecStruct(
         //   GeneralSpecs.Meta, MetaSpecs.All, StructureSpecs.TrueCopy);
         public static readonly CopySpecStruct ScalarMultiply = new CopySpecStruct(
            GeneralSpecs.Meta, MetaSpecs.All, StructureSpecs.RefCopy);
         public static readonly CopySpecStruct ElimRank3 = new CopySpecStruct(          // Used on ElimRank method.
            GeneralSpecs.Both, MetaSpecs.Rank);
         public static readonly CopySpecStruct ElimRank2 = new CopySpecStruct(          // Used on ElimRank method.
            GeneralSpecs.Both, MetaSpecs.Rank | MetaSpecs.Sup);
         /// <summary>Copy values and rank. Does not copy or assign Structure</summary>
         public static readonly CopySpecStruct S35200 = new CopySpecStruct(
            GeneralSpecs.Both, MetaSpecs.Rank);
      }
      protected Tensor(int cap) : base(cap) { }
      internal Tensor(int[] structure, int rank, Tensor<τ,α> sup, int cap) : base(cap) {
         Structure = structure ?? null;
         Rank = rank;
         Sup = sup ?? null;
      }
      /// <summary>Creates a top tensor with specified structure and initial capacity. Rank is assigned as the length of structure array.</summary>
      /// <param name="structure">Specifies dimension of each rank.</param>
      /// <param name="cap">Initially assigned memory.</param>
      public Tensor(int[] structure, int cap = 6) : this(structure, structure.Length, null, cap) { }
      /// <summary>Creates a non-top tensor with specified superior and initial capacity. Rank is assigned as one less that superior.</summary>
      /// <param name="sup">Tensor directly above in hierarchy.</param>
      /// <param name="cap">Initially assigned memory.</param>
      public Tensor(Tensor<τ,α> sup, int cap) : this(sup.Structure, sup.Rank - 1, sup ?? null, cap) { }
      /// <summary>Creates a deep copy of specified tensor. You can optionally specify which meta-fields (Structure, Rank ...) to copy.</summary>
      /// <param name="src">Source tensor to copy.</param>
      /// <param name="extraCap">How much more top rank space should the new tensor have (newCap = oldCap + extraCap).</param>
      /// <param name="cs">Exact specification of fields to copy. Default is all.</param>
      public Tensor(in Tensor<τ,α> src, in CopySpecStruct cs) :
       base(src.Count + cs.ExtraCapacity) {
         Copy(src, this, in cs);
      }
      public Tensor(in Tensor<τ,α> src) : this(in src, in CopySpecs.Default) { }
      /// <summary>Assigns this tensor as superior to the added one.</summary>
      /// <param name="key"></param>
      /// <param name="tnr"></param>
      new public void Add(int key, Tensor<τ,α> tnr) {
         tnr.Sup = this;
         tnr.Structure = Structure;
         base.Add(key, tnr);
      }
      /// <summary>Creates a tensor as a deep copy of this one: same Rank, Structure and Superior.</summary>
      public virtual Tensor<τ,α> Copy(in CopySpecStruct cs) {
         if (Rank == 1) {
            var res = new Vector<τ,α>(Count + cs.ExtraCapacity);
            var thisVector = (Vector<τ,α>)this;
            Vector<τ,α>.Copy(thisVector, res, in cs);
            return res; }
         else {
            var res = new Tensor<τ,α>(Count + cs.ExtraCapacity);
            Copy(this, res, in cs);
            return res; }
      }
      /// <summary>Make a shallow or deep copy of a tensor. Set CopySpec field for fine tunning, ensure proper capacity of the target tensor.</summary>
      /// <param name="src">Copy source.</param>
      /// <param name="tgt">Copy target.</param>
      /// <param name="cs">Exact specification of what fields to copy. Default is all.</param>
      public static void Copy(in Tensor<τ,α> src, Tensor<τ,α> tgt, in CopySpecStruct cs) {
         TB.Assert.True(src.Rank > 1,
            "Tensors's rank has to be at least 2 to be copied via this method.");
         CopyMetaFields(src, tgt, in cs.MetaFields, in cs.Structure);
         if((cs.General & GeneralSpecs.Vals) == GeneralSpecs.Vals) {
            int endRank = cs.EndRank;
            Recursion(src, tgt);

            void Recursion(Tensor<τ,α> src1, Tensor<τ,α> tgt1) {
            if(src1.Rank > 2) {                                       // Subordinates are tensors.
               foreach (var kv in src1) {
                  var tnr = new Tensor<τ,α>(kv.Value.Structure, kv.Value.Rank, kv.Value.Sup, kv.Value.Count);
                  tnr.Rank = tgt1.Rank - 1;
                  tgt1.Add(kv.Key, tnr);
                  if(src1.Rank > endRank)
                     Recursion(kv.Value, tnr); } }
            else if(src1.Rank == 2) {                                 // Subordinates are vectors.
               foreach(var kv in src1) {
                  var valAsVec = (Vector<τ,α>) kv.Value;
                  var vecCopy = new Vector<τ,α>();
                  tgt1.Add(kv.Key, vecCopy);
                  vecCopy.Vals = new SCG.Dictionary<int,τ>(valAsVec.Vals);
               } }
            else
               throw new InvalidOperationException(
                  "Tensors's rank has to be at least 2 to be copied via this method.");
            }
         }
      }
      public static void CopyMetaFields(Tensor<τ,α> src, Tensor<τ,α> tgt, in MetaSpecs mcs,
      in StructureSpecs scs) {
         if((mcs & MetaSpecs.Structure) == MetaSpecs.Structure) {
            if((scs & StructureSpecs.RefCopy) == StructureSpecs.RefCopy)
               tgt.Structure = src.Structure;
            else {
               tgt.Structure = new int[src.Structure.Length];
               Array.Copy(src.Structure, tgt.Structure, src.Structure.Length); } }
         if((mcs & MetaSpecs.Rank) == MetaSpecs.Rank)
            tgt.Rank = src.Rank;
         if((mcs & MetaSpecs.Sup) == MetaSpecs.Sup)
            tgt.Sup = src.Sup ?? null;
      }
      public static Tensor<τ,α> CreateFromArray(τ[][] arr) {
         int nRows = arr.Length;
         int nCols = arr[0].Length;
         var tnr = new Tensor<τ,α>(new int[2] {nRows, nCols});
         for(int i = 0; i < nRows; ++i)
            for(int j = 0; j < nCols; ++j)
               tnr[i,j] = arr[i][j];
         return tnr;
      }
      public static Tensor<τ,α> CreateFromArray(τ[] arr, int allRows, int startRow,
         int nRows, int startCol, int nCols, int width, int height, int startRowInx = 0, int startColInx = 0) {
            int allCols = arr.Length/allRows;
            var tnr = new Tensor<τ,α>(new int[2] {height, width});
            for(int i = startRow, k = startRowInx; i < startRow + nRows; ++i, ++k)
               for(int j = startCol, l = startColInx; j < startCol + nCols; ++j, ++l)
                  tnr[k,l] = arr[i*allCols + j];
            return tnr;
      }
      public static Tensor<τ,α> CreateFromArray(τ[] arr, int allRows, int startRow,
         int nRows, int startCol, int nCols) =>
            CreateFromArray(arr, allRows, startRow, nRows, startCol, nCols, nCols, nRows);

      public static Tensor<τ,α> CreateFromSpan(Span<τ> slice, int nRows) {
         int nCols = slice.Length / nRows;
         var tnr = new Tensor<τ,α>(new int[2] {nRows, nCols});
         for(int i = 0; i < nRows; ++i)
            for(int j = 0; j < nCols; ++j)
               tnr[i,j] = slice[i*nCols + j];
         return tnr;
      }
      /// <summary>Creates a tensor with specified structure from values provided within a Span.</summary>
      /// <param name="slice">Span of values.</param>
      /// <param name="structure">Structure of new tensor.</param>
      public static Tensor<τ,α> CreateFromFlatSpec(Span<τ> slice, params int[] structure) {
         int tnrRank = structure.Length;
         if(tnrRank == 1)
            return Vector<τ,α>.CreateFromSpan(slice);
         else
            return Recursion(slice, 0);

         Tensor<τ,α> Recursion(Span<τ> slc, int dim) {       // Specifiy slice and the structure dimension (natural rank index) to which it belongs.
            int trueRank = SlotToRankNotation(structure.Length, dim);
            int nIter = structure[dim];                              // As many iterations as it is the size of the dimension.
            int nEmtsInSlice = slc.Length / nIter;
            if(trueRank > 1) {
               var res = new Tensor<τ,α>(structure, trueRank, null, structure[dim]);
               for(int i = 0; i < nIter; ++i) {                      // Over each tensor. Create new slices and run recursion on them.
                  var newSlc = slc.Slice(i*nEmtsInSlice, nEmtsInSlice);
                  var newTnr = Recursion(newSlc, dim + 1);
                  res.Add(i, newTnr); }
               return res; }
            else                                                  // We are at rank 1 = vector rank.
               return Vector<τ,α>.CreateFromSpan(slc);
         }
      }
      /// <summary>Transforms from slot index (in the order as written by hand, e.g. A^ijk ==> i -> 0, k -> 2) to true rank index (as situated in the hierarchy, e.g., i from previous example has index 2, k has 0).</summary>
      /// <param name="rankNotation">Rank index as situated in the hierarchy. Higher number equates to being higher in the hierarchy.</param>
      int ToSlotNotation(int rankNotation) =>
         SlotToRankNotation(Structure.Length, rankNotation);
      /// <summary>Transforms from true rank index (as situated in the hierarchy, i.e. higher number equates to being higher in the hierarchy) to true rank index (in the order as written by hand, e.g. A^ijk ==> i -> 0, k -> 2).</summary>
      /// <param name="slotNotation">Rank index as written by hand, e.g. A^ijk ==> i -> 0, k -> 2.</param>
      /// <remarks>Implementation is actually identical to the one in the ToNaturalInx method.</remarks>
      int ToRankNotation(int slotNotation) =>
         SlotToRankNotation(Structure.Length, slotNotation);
      static int SlotToRankNotation(int nRanks, int slotNotation) =>
         nRanks - slotNotation;
      public Tensor<τ,α> this[uint overloadDummy, params int[] inx] {
         get {
            Tensor<τ,α> tnr = this;
            for(int i = 0; i < inx.Length; ++i) {
               if(!tnr.TryGetValue(inx[i], out tnr))
                  return null; }
            return tnr; }                                // No problem with tnr being null. We return above.
         set {
            Tensor<τ,α> tnr = this;
            if(value != null) {
               int n = inx.Length - 1;
               for(int i = 0; i < n; ++i) {
                  if(!tnr.TryGetValue(inx[i], out tnr))
                     tnr = new Tensor<τ,α>(Structure, tnr.Rank - 1, tnr, 6);
                     tnr.Sup.Add(inx[i], tnr); }
               var dict = (TensorBase<Tensor<τ,α>>) tnr;
               dict[inx[n]] = value; }
            else {
               for(int i = 0; i < inx.Length; ++i) {
                  if(!tnr.TryGetValue(inx[i], out tnr))
                     return; }
               tnr.Sup.Remove(inx[inx.Length - 1]); } }
      }
      public Vector<τ,α> this[short overloadDummy, params int[] inx] {
         get {
            Tensor<τ,α> tnr = this;
            int n = inx.Length - 1;
            for(int i = 0; i < n; ++i) {
               if(!tnr.TryGetValue(inx[i], out tnr))
                  return null; }
            if(tnr.TryGetValue(inx[n], out tnr))         // No probelm with null.
               return (Vector<τ,α>)tnr;                  // Same.
            else
               return null; }
         set {
            Tensor<τ,α> tnr = this;
            if(value != null) {
               int n = inx.Length - 1;                      // One before last chooses tensor, last chooses vector.
               for(int i = 0; i < n; ++i) {
                  if(tnr.TryGetValue(inx[i], out Tensor<τ,α> tnr2)) {
                     tnr = tnr2; }
                  else {
                     tnr = new Tensor<τ,α>(Structure, tnr.Rank - 1, tnr, 6);
                     tnr.Sup.Add(inx[i], tnr); }}
               var dict = (TensorBase<Tensor<τ,α>>) tnr;                      // We do not check that it is a vector beforehand. It is assumed that the user used indexer correctly.
               dict[inx[n]] = value; }
            else {
               int n = inx.Length;                 // Last chooses vector.
               for(int i = 0; i < n; ++i) {
                  if(!tnr.TryGetValue(inx[i], out tnr))
                     return; }
               tnr.Sup.Remove(inx[n - 1]); } }     // Vector.Superior.Remove
      }
      public virtual τ this[params int[] inx] {
         get {
            Tensor<τ,α> tnr = this;
            int n = inx.Length - 2;
            for(int i = 0; i < n; ++i) {
               if(!tnr.TryGetValue(inx[i], out tnr))
                  return default; }
            if(tnr.TryGetValue(inx[n], out tnr)) {         // No probelm with null.
               var vec = (Vector<τ,α>)tnr;                  // Same.
               vec.Vals.TryGetValue(inx[n + 1], out τ val);
               return val; }
            else
               return default; }
         set {
            Tensor<τ,α> tnr = this;
            Tensor<τ,α> tnr2;                               // Temporary to avoid null problem below.
            Vector<τ,α> vec;
            if(!value.Equals(default)) {
               if(inx.Length > 1) {                         // At least a 2nd rank tensor.
                  int n = inx.Length - 2;
                  for(int i = 0; i < n; ++i) {                    // This loop is entered only for a 3rd rank tensor or above.
                     if(tnr.TryGetValue(inx[i], out tnr2)) {
                        tnr = tnr2; }
                     else {
                        tnr = new Tensor<τ,α>(Structure, tnr.Rank - 1, tnr, 6);
                        tnr.Sup.Add(inx[i], tnr); }}
                  if(tnr.TryGetValue(inx[n], out tnr2)) {                  // Does vector exist?
                     vec = (Vector<τ,α>) tnr2; }
                  else {
                     vec = new Vector<τ,α>(Structure, tnr, 4); 
                     tnr.Add(inx[n], vec); }
                  vec.Vals[inx[n + 1]] = value; } }
            else {
               int n = inx.Length - 1;
               for(int i = 0; i < n; ++i) {
                  if(!tnr.TryGetValue(inx[i], out tnr))
                     return; }
               tnr.Remove(inx[n]); } }
      }
      /// <summary>Plus operator for two tensors.</summary><param name="tnr1">Left operand.</param><param name="tnr2">Right operand.</param>
      public static Tensor<τ,α> operator + (Tensor<τ,α> tnr1, Tensor<τ,α> tnr2) {
         TB.Assert.True(tnr1.Rank == tnr2.Rank);                                    // First, ranks must match.
         int topRank1 = tnr1.Structure.Length;                                      // We have to check that all dimensions below current ranks match.
         int topRank2 = tnr2.Structure.Length;
         var strInx1 = tnr1.ToSlotNotation(tnr1.Rank - 1) - 1;                         // Index in structure array.
         var strInx2 = tnr2.ToSlotNotation(tnr2.Rank - 1) - 1;
         for(int i = strInx1, j = strInx2; i < topRank1; ++i, ++j) {
            if(tnr1.Structure[i] != tnr2.Structure[j])
               throw new InvalidOperationException("Tensor addition: structures do not match."); }
         return Recursion(tnr1, tnr2);

         Tensor<τ,α> Recursion(Tensor<τ,α> t1, Tensor<τ,α> t2) {
            var res = new Tensor<τ,α>(t2, CopySpecs.AddSubtract);            // Deep copy with extra capacity, copy structure by value.
            if(t1.Rank > 2) {
               foreach(var int_tnr1 in t1) {
                  if(t2.TryGetValue(int_tnr1.Key, out var tnr2Val)) {
                     var subRes = Recursion(int_tnr1.Value, tnr2Val);
                     res.Add(int_tnr1.Key, subRes); } } }
            else {
               foreach(var int_tnr1 in t1) {
                  var vec1 = (Vector<τ,α>) int_tnr1.Value;
                  if(t2.TryGetValue(int_tnr1.Key, out var tnr2Val)) {      // Entry exists in t2, we must sum.
                     var vec2 = (Vector<τ,α>) tnr2Val;
                     var resAsBase = (TensorBase<Tensor<τ,α>>)res;
                     resAsBase[int_tnr1.Key] = vec1 + vec2; }
                  else
                     res.Add(int_tnr1.Key, int_tnr1.Value); } }          // Entry does not exist in t2, simply Add.
            return res;
         }
      }
      /// <summary>Minus operator for two tensors.</summary><param name="tnr1">Left operand.</param><param name="tnr2">Right operand.</param>
      public static Tensor<τ,α> operator - (Tensor<τ,α> tnr1, Tensor<τ,α> tnr2) {
         TB.Assert.True(tnr1.Rank == tnr2.Rank);                                    // First, ranks must match.
         int topRank1 = tnr1.Structure.Length;                                      // We have to check that all dimensions below current ranks match.
         int topRank2 = tnr2.Structure.Length;
         var strInx1 = tnr1.ToSlotNotation(tnr1.Rank - 1) - 1;                         // Index in structure array.
         var strInx2 = tnr2.ToSlotNotation(tnr2.Rank - 1) - 1;
         for(int i = strInx1, j = strInx2; i < topRank1; ++i, ++j) {
            if(tnr1.Structure[i] != tnr2.Structure[j])
               throw new InvalidOperationException("Tensor addition: structures do not match."); }
         return Recursion(tnr1, tnr2);

         Tensor<τ,α> Recursion(Tensor<τ,α> t1, Tensor<τ,α> t2) {
            Tensor<τ,α> res = new Tensor<τ,α>(t1, t1.Count + 4);        // Must be a deep copy with a bit of extra capacity.
            if(t1.Rank > 2) {
               foreach(var int_tnr2 in t2) {
                  if(t1.TryGetValue(int_tnr2.Key, out var tnr2Val)) {
                     var subRes = Recursion(int_tnr2.Value, tnr2Val);
                     res.Add(int_tnr2.Key, subRes); } } }
            else {
               foreach(var int_tnr2 in t2) {
                  var vec2 = (Vector<τ,α>) int_tnr2.Value;
                  if(t1.TryGetValue(int_tnr2.Key, out var tnr1Val)) {
                     var vec1 = (Vector<τ,α>) tnr1Val;
                     var resAsBase = (TensorBase<Tensor<τ,α>>)res;
                     resAsBase[int_tnr2.Key] = vec1 - vec2; }
                  else
                     res.Add(int_tnr2.Key, -vec2); } }
            return res;
        }
      }
      /// <summary>This destroys tnr2, don't use the tnr2 reference afterwards.</summary>
      /// <param name="tnr2">Disposable operand 2 whose elements will be absorbed into the caller.</param>
      public void Add(Tensor<τ,α> tnr2) {
         Tensor<τ,α> tnr1 = this;
         TB.Assert.AreEqual(tnr1.Rank, tnr2.Rank, "Ranks must be equal when performing tensor addition.");  // FIXME: Implement proper substructure checking, like you did above for + operator.
         Recursion(tnr1, tnr2);

         // You must make sure that the collection is not modified during enumeration!!!
         // Maybe cut short if t2 is disposable.
         void Recursion(Tensor<τ,α> t1, Tensor<τ,α> t2) {
            if(t2.Rank > 2) {
               foreach(var int_subTnr2 in t2) {
                  if(t1.TryGetValue(int_subTnr2.Key, out var subTnr1))            // Equivalent subtensor exists in T1.
                     Recursion(subTnr1, int_subTnr2.Value);
                  else                                                      // Equivalent subtensor does not exist in T1. Absorb the subtensor from T2 and add it.
                     t1.Add(int_subTnr2.Key, int_subTnr2.Value); } }
            else if(t2.Rank == 2) {
               foreach(var int_subTnr2 in t2) {
                  //var vec2 = (Vector<τ,α>) int_subTnr2.Value;
                  if(t1.TryGetValue(int_subTnr2.Key, out var subTnr1)) {      // Entry exists in t1, we must sum.
                     var vec1 = (Vector<τ,α>) subTnr1;
                     var vec2 = (Vector<τ,α>) int_subTnr2.Value;
                     vec1.Add(vec2); }
                  else {
                     t1.Add(int_subTnr2.Key, int_subTnr2.Value); } } }          // Entry does not exist in t2, simply Add.
            else {                                                            // We have a vector.
               var vec1 = (Vector<τ,α>) t1;
               var vec2 = (Vector<τ,α>) t2;
               t1.Add(t2);
            }
         }
      }
      /// <summary>Multiply tensor with a scalar. Operator copies structure and superior by reference.</summary>
      /// <param name="scal">Scalar.</param>
      /// <param name="tnr">Tensor.</param>
      public static Tensor<τ,α> operator * (τ scal, Tensor<τ,α> tnr) {
         return Recursion(tnr);
         Tensor<τ,α> Recursion(in Tensor<τ,α> src) {
            var res = new Tensor<τ,α>(tnr.Structure, tnr.Rank, tnr.Sup, tnr.Count);            // We copy only meta fields (whereby we copy Structure by value).
            if(src.Rank > 2) {                                       // Subordinates are tensors.
               foreach (var kv in src)
                  res.Add(kv.Key, Recursion(kv.Value)); }
            else if(src.Rank == 2) {                                 // Subordinates are vectors.
               foreach (var kv in src)
                  res.Add(kv.Key, scal*((Vector<τ,α>) kv.Value)); }
            else
               return scal*((Vector<τ,α>) src);
            return res;
         }
      }
      /// <summary>Calculates tensor product of this tensor (left-hand operand) with another tensor (right-hand operand).</summary>
      /// <param name="tnr2">Right-hand operand.</param>
      public virtual Tensor<τ,α> TnrProduct(Tensor<τ,α> tnr2) {
         // Overriden on vector when first operand is a vector.
         // 1) Descend to rank 1 through a recursion and then delete that vector.
         // 2) Substitute it with a tensor of rank tnr2.Rank + 1 whose entries are tnr2s multiplied by the corresponding scalar that used to preside there in the old vector.
         int newRank = Rank + tnr2.Rank;
         var newStructure = Structure.Concat(tnr2.Structure).ToArray();
         return Recursion(this, newRank);

         Tensor<τ,α> Recursion(Tensor<τ,α> src, int resRank) {
            var res = new Tensor<τ,α>(newStructure, resRank, null, src.Count);
            if(src.Rank > 2) {                                                    // Only copy. Adjust ranks.
               foreach(var int_tnr in src)
                  res.Add(int_tnr.Key, Recursion(int_tnr.Value, resRank - 1)); }
            else {                              // We are now at tensor which contains vectors.
               foreach(var int_vec in src) {    // Substitute each vector wiht a new tensor.
                  var vec = (Vector<τ,α>) int_vec.Value;
                  var subTnr = new Tensor<τ,α>(newStructure, resRank, null, src.Count);
                  foreach(var int_val in vec.Vals)
                     subTnr.Add(int_val.Key, TensorExtensions<τ,α>.ScalMul1(int_val.Value, tnr2, subTnr)); // ScalMul so that the correct superior propagares downwards.
                  res.Add(int_vec.Key, subTnr); } }
            return res;
         }
      }
      /// <summary>Eliminates a single rank out of a tensor by choosing a single subtensor at that rank and making it take the place of its direct superior (thus discarding all other subtensors at that rank). The resulting tensor has therefore its rank reduced by one.</summary>
      /// <param name="elimRank"> True, zero-based rank index of rank to be eliminated.</param>
      /// <param name="emtInx">Zero-based element index in that rank in favor of which the elimination will take place.</param>
      public Tensor<τ,α> ReduceRank(int elimRank, int emtInx) {
         TB.Assert.True(elimRank < Rank && elimRank > -1, "You can only eliminate a non-negative rank greater than or equal to top rank.");
         var newStructureL = Structure.Take(elimRank);
         var newStructureR = Structure.Skip(elimRank + 1);
         var newStructure = newStructureL.Concat(newStructureR).ToArray();    // Created a new structure. Assign it to new host tensor.

         if(elimRank == Rank - 1) {                                                    // 1 rank exists above elimRank == Pick one from elimRank and return it.
            if(Rank > 1) {                                                             // Result is Tensor, top tensor at least rank 3.
               if(TryGetValue(emtInx, out var subTnr)) {                                   // Sought after subordinate exists.
                  var newTnr = subTnr.Copy(in CopySpecs.S35200);                          // Works properly even for Vector.
                  newTnr.Structure = newStructure;
                  return newTnr;}
               return null; }
            else                                                                                      // Rank <= 1: impossible.
               throw new ArgumentException("Cannot eliminate rank 1 or lower on rank 1 tensor."); }
         else if(elimRank > 1) {                                                       // At least two ranks exist above elimRank & elimRank is at least 2. Obviously applicable only to Rank 4 or higher tensors.
            var res = new Tensor<τ,α>(newStructure, Rank - 1, null, Count);
            if(Rank > 3) {                                                              // No special treatment due to Vector needed.
               RecursiveCopyAndElim(this, res, emtInx, elimRank + 2);
               return res; }
            else
               throw new ArgumentException("Cannot eliminate rank 2 or above on rank 1,2,3 tensor with this branch."); }
         else if(elimRank == 1) {                                                      // At least two ranks exist above elimRank & elimRank is 1. Obviously applicable only to rank 3 or higher tensors.
            var res = new Tensor<τ,α>(newStructure, Rank - 1, null, Count);
            if(Rank > 2) {                                                             // Result is tensor.
               RecursiveCopyAndElim(this, res, emtInx, 1);                    // FIXME: ???? ElimRank == 1 so we always stop at 3. ????? 
               return res; }
            else
               throw new ArgumentException("Cannot eliminate rank 1 on rank 1,2 tensor with this branch."); }
         else {                                          // At least two ranks exist above elimRank & elimRank is 0. Obviously applicable only to rank 2 or higher tensors.
            if(Rank > 2) {                               // Result is tensor. Choose one value from each vector in subordinate rank 2 tensors, build a new vector and add those values to it. Then add that vector to superior rank 3 tensor.
               var res = new Tensor<τ,α>(newStructure, Rank - 1, null, Count);
               ElimR0_R3Plus(this, res, emtInx);
               return res; }
            else if(Rank == 2) {
               var res = new Vector<τ,α>(newStructure, null, 4);
               ElimR0_R2(this, res, emtInx);
               return res; }
            else
               throw new ArgumentException("Cannot eliminate rank 0 on rank 1 tensor with this branch."); }
      }

      // static void TnrElimination(Tensor<τ,α> src, Tensor<τ,α> tgt, int emtInx) {        // src is 2 ranks above elimRank and at least rank 3.
      //     }

      /// <summary>Can only be used to eliminate rank 3 or higher. Provided target has to be initiated one rank lower than source.</summary>
      /// <param name="src">Source tensor whose rank we are eliminating.</param>
      /// <param name="tgt">Target tensor. Has to be one rank lower than source.</param>
      /// <param name="emtInx">Element index in favor of which we are eliminating.</param>
      /// <param name="elimRank"></param>
      static void RecursiveCopyAndElim(Tensor<τ,α> src, Tensor<τ,α> tgt, int emtInx, int elimRank) {      // Recursively copies tensors.
         if(src.Rank > elimRank + 2) {                                    // We have not yet reached rank directly above rank scheduled for elimination: copy rank.
            foreach(var int_tnr in src) {
               var subTnr = new Tensor<τ,α>(tgt, src.Count);
               RecursiveCopyAndElim(int_tnr.Value, subTnr, emtInx, elimRank);
               if(subTnr.Count != 0)
                  tgt.Add(int_tnr.Key, subTnr); } }
         else {                                                             // We have reached rank directly above rank scheduled for elimination: eliminate.
            foreach(var int_tnr in src) {
               if(int_tnr.Value.TryGetValue(emtInx, out var subTnr)) {
                  var subTnrCopy = subTnr.Copy(in CopySpecs.S35200);
                  tgt.Add(int_tnr.Key, subTnrCopy); } } }
      }

      /// <summary>Eliminates rank 0 on a rank 2 tensor, resulting in a rank 1 tensor (vector),</summary>
      /// <param name="src">Rank 2 tensor.</param>
      /// <param name="tgt">Initialized result vector.</param>
      /// <param name="emtInx">Element index in favor of which the elimination will proceed.</param>
      public static void ElimR0_R2(Tensor<τ,α> src, Vector<τ,α> tgt, int emtInx) {
         TB.Assert.True(src.Rank == 2, "This method is intended for rank 2 tensors only.");
         foreach(var int_tnrR1 in src) {
            var subVec = (Vector<τ,α>) int_tnrR1.Value;
            if(subVec.Vals.TryGetValue(emtInx, out var val))
               tgt.Add(int_tnrR1.Key, val); }
      }
      /// <summary>Eliminate rank 0 on a rank 3 or higher tensor.</summary>
      /// <param name="src">Rank 3 or higher tensor.</param>
      /// <param name="tgt">Tensor one rank lower than source.</param>
      /// <param name="emtInx">Element index in favor of which to eliminate.</param>
      public static void ElimR0_R3Plus(Tensor<τ,α> src, Tensor<τ,α> tgt, int emtInx) {
         TB.Assert.True(src.Rank > 2, "This method is applicable to rank 3 and higher tensors.");
         if(src.Rank > 3) {
            foreach(var int_tnr in src) {
               var subTnr = new Tensor<τ,α>(tgt, src.Count);
               ElimR0_R3Plus(int_tnr.Value, subTnr, emtInx);
               if(subTnr.Count != 0)
                  tgt.Add(int_tnr.Key, subTnr); } }
         else {                                                                  // src.Rank == 3.
            foreach(var int_tnr in src) {
               var newVec = new Vector<τ,α>(tgt, 4);
               ElimR0_R2(int_tnr.Value, newVec, emtInx);
               tgt.Add(int_tnr.Key, newVec); } } }

      protected (int[] struc, int truInx1, int truInx2, int conDim) ContractPart1(
      Tensor<τ,α> tnr2, int natInx1, int natInx2) {
         // 1) First eliminate, creating new tensors. Then add them together using tensor product.
         int[] struc1 = Structure, 
               struc2 = tnr2.Structure;
         int rank1 = struc1.Length,
             rank2 = struc2.Length;
         TB.Assert.True(rank1 == Rank && rank2 == tnr2.Rank,
            "One of the tensors is not top rank.");
         TB.Assert.AreEqual(struc1[natInx1 - 1], struc2[natInx2 - 1],              // Check that the dimensions of contracted ranks are equal.
            "Rank dimensions at specified indices must be equal.");
         int   conDim = Structure[natInx1 - 1],                                // Dimension of rank we're contracting.
               truInx1 = ToRankNotation(natInx1),
               truInx2 = tnr2.ToRankNotation(natInx2);
         var struc3_1 = struc1.Where((emt, i) => i != (natInx1 - 1));
         var struc3_2 = struc2.Where((emt, i) => i != (natInx2 - 1));
         var struc3 = struc3_1.Concat(struc3_2).ToArray();                 // New structure.
         return (struc3, truInx1, truInx2, conDim);
      }
      
      public Tensor<τ,α> ContractPart2(Tensor<τ,α> tnr2, int truInx1, int truInx2, int[] struc3, int conDim) {
         // 1) First eliminate, creating new tensors. Then add them together using tensor product.
         if(Rank > 1) {
            if(tnr2.Rank > 1) {                                // First tensor is rank 2 or more.
               Tensor<τ,α> elimTnr1, elimTnr2, sumand, sum;
               sum = new Tensor<τ,α>(struc3);                                    // Set sum to a zero tensor.
               for(int i = 0; i < conDim; ++i) {
                  elimTnr1 = ReduceRank(truInx1, i);
                  elimTnr2 = tnr2.ReduceRank(truInx2, i);
                  if(elimTnr1 != null && elimTnr2 != null) {
                     sumand = elimTnr1.TnrProduct(elimTnr2);
                     sum.Add(sumand); } }
               if(sum.Count != 0)
                  return sum;
               else
                  return null; }
            else {                                                // Second tensor is rank 1 (a vector).
               Vector<τ,α> vec = (Vector<τ,α>) tnr2;
               if(Rank == 2) {                                    // Result will be vector.
                  Vector<τ,α> elimVec, sumand, sum;
                  sum = new Vector<τ,α>(struc3, null, 4);
                  for(int i = 0; i < conDim; ++i) {
                     elimVec = (Vector<τ,α>) ReduceRank(truInx1, i);
                     if(elimVec != null && vec.Vals.TryGetValue(i, out var val)) {
                        sumand = val*elimVec;
                        sum.Add(sumand); } }
                  if(sum.Vals.Count != 0)
                     return sum;
                  else
                     return null; }
               else {                                             // Result will be tensor.
                  Tensor<τ,α> elimTnr1, sumand, sum;
                  sum = new Tensor<τ,α>(struc3);
                  for(int i = 0; i < conDim; ++i) {
                     elimTnr1 = ReduceRank(truInx1, i);
                     if(elimTnr1 != null && vec.Vals.TryGetValue(i, out var val)) {
                        sumand = val*elimTnr1;
                        sum.Add(sumand); } }
                  if(sum.Count != 0)
                     return sum;
                  else
                     return null; } } }
         else {                                                   // First tensor is rank 1 (a vector).
            var vec1 = (Vector<τ,α>) this;
            return vec1.ContractPart2(tnr2, truInx2, struc3, conDim);}
      }

      /// <summary>Contracts two tensors over specified natural rank indices. Example: Contraction writen as A^(ijkl)B^(mnip) is specified as a (0,2) contraction of A and B, not a (3,1) contraction.</summary>
      /// <param name="tnr2">Tensor 2.</param>
      /// <param name="natInx1">One-based natural index on this tensor over which to contract.</param>
      /// <param name="natInx2">One-based natural index on tensor 2 over which to contract (it must hold: dim(rank(inx1)) = dim(rank(inx2)).</param>
      /// <remarks>Tensor contraction is a generalization of trace, which can further be viewed as a generalization of dot product.</remarks>
      public Tensor<τ,α> Contract(Tensor<τ,α> tnr2, int natInx1, int natInx2) {
         (int[] struc3, int truInx1, int truInx2, int conDim) = ContractPart1(tnr2, natInx1, natInx2);
         return ContractPart2(tnr2, truInx1, truInx2, struc3, conDim);
      }

      // TODO; Implement and test self-contract (applicable to rank 2 and above). Then finish ConjugateGrads.
      /// <summary>Use on rank 2 tensor.</summary>
      /// <param name="natInx1">First one-based natural index on this tensor over which to contract.</param>
      /// <param name="natInx2">Second one-based natural index on this tensor over which to contract.</param>
      public τ SelfContractR2() {
         TB.Assert.True(Rank == 2, "Tensor rank has to be 2 for this method.");
         TB.Assert.True(Structure[0] == Structure[1], "Corresponding dimensions have to be equal.");
         τ result = default;
         foreach(var int_vec in this) {
            var vec = (Vector<τ,α>) int_vec.Value;
            if(vec.Vals.TryGetValue(int_vec.Key, out τ val))
               result = O<τ,α>.A.Add(result, val); }
         return result;
      }

      public Vector<τ,α> SelfContractR3(int natInx1, int natInx2) {
         TB.Assert.True(Rank == 3, "Tensor rank has to be 3 for this method.");
         TB.Assert.True(Structure[natInx1 - 1] == Structure[natInx2 - 1],
            "Corresponding dimensions have to be equal.");
         Vector<τ,α> res = new Vector<τ,α>(new int[] {Structure[2]}, null, 4);
         int truInx1 = ToRankNotation(natInx1);
         int truInx2 = ToRankNotation(natInx2);
         if(natInx1 == 1) {
            if(natInx2 == 2) {
               foreach(var int_tnr in this) {
                  if(int_tnr.Value.TryGetValue(int_tnr.Key, out var subTnr)) {
                     var vec = (Vector<τ,α>) subTnr;
                     res.Add(vec); } } }
            if(natInx2 == 3) {
               foreach(var int_tnr in this) {
                  foreach(var int_subTnr in int_tnr.Value) {
                     var subVec = (Vector<τ,α>) int_subTnr.Value;
                     if(subVec.Vals.TryGetValue(int_tnr.Key, out τ val))
                        res.Vals[int_subTnr.Key] = O<τ,α>.A.Add(res[int_subTnr.Key], val); } } } }
         else if(natInx1 == 2) {                   // natInx2 == 3
            foreach(var int_tnr in this) {
               foreach(var int_subTnr in int_tnr.Value) {
                  var subVec = (Vector<τ,α>) int_subTnr.Value;
                  if(subVec.Vals.TryGetValue(int_subTnr.Key, out τ val))
                     res.Vals[int_tnr.Key] = O<τ,α>.A.Add(res[int_tnr.Key], val); } } }
         return res;
      }

      public Tensor<τ,α> SelfContract(int slotInx1, int slotInx2) {
         //throw new NotImplementedException();
         TB.Assert.True(Rank > 2, "This method is not applicable to rank 2 tensors.");
         TB.Assert.True(Structure[slotInx1 - 1] == Structure[slotInx2 - 1],
            "Dimensions of contracted slots have to be equal.");
         if(Rank > 3) {
            var newStruct1 = Structure.Take(slotInx1 - 1);
            var newStruct2 = Structure.Take(slotInx2 - 1).Skip(slotInx1);
            var newStruct3 = Structure.Skip(slotInx2);
            var newStruct = newStruct1.Concat(newStruct2).Concat(newStruct3).ToArray();
            var res = new Tensor<τ,α>(newStruct, Rank - 2, null, Count);
            var rankInx1 = ToRankNotation(slotInx1);
            var rankInx2 = ToRankNotation(slotInx2);
            int dimRank = Structure[slotInx1 - 1];                // Dimension of contracted rank.
            for(int i = 0; i < dimRank; ++i) {                    // Over each element inside contracted ranks.
               var step1Tnr = ReduceRank(rankInx2, i);
               var sumand = step1Tnr.ReduceRank(rankInx1 - 1, i);
               res.Add(sumand); }
            return res;
         }
         else
            return SelfContractR3(slotInx1, slotInx2);
      }

      public SCG.IEnumerable<Tensor<τ,α>> RankEnumerator(int rankInx) {
         TB.Assert.True(rankInx > 1, "This method applies only to ranks that hold pure tensors.");
         if(Rank > rankInx + 1) {
            foreach(var subTnr in Recursion(this)) {
               yield return subTnr;
            }
         }
         else {
            foreach(var int_subTnr in this) {
               yield return int_subTnr.Value;
            }
         }

         SCG.IEnumerable<Tensor<τ,α>> Recursion(Tensor<τ,α> src) {
            foreach(var int_tnr in src) {
               if(int_tnr.Value.Rank > rankInx + 1) {
                  foreach(var subTnr in Recursion(int_tnr.Value))
                     yield return subTnr; }
               else {
                  foreach(var int_subTnr in int_tnr.Value)
                     yield return int_subTnr.Value; }
            }
         }
      }

      /// <summary>Check two tensors for equality.</summary><param name="tnr2">Other tensor.</param>
      public bool Equals(Tensor<τ,α> tnr2) {
         Structure.Equals<int, IntArithmetic>(tnr2.Structure);
         return TnrRecursion(this, tnr2);

         bool TnrRecursion(Tensor<τ,α> sup1, Tensor<τ,α> sup2) {
            if(sup1.Count != sup2.Count)
               return false;
            if(sup1.Rank > 2) {
               foreach(var inx_subTnr1 in sup1) {
                  if(sup2.TryGetValue(inx_subTnr1.Key, out var subTnr2))
                     return TnrRecursion(inx_subTnr1.Value, subTnr2);
                  else
                     return false; }
               return true; }
            else
               return VecRecursion(sup1, sup2);
            //throw new InvalidOperationException("We shouldn't be here.");
         }

         bool VecRecursion(Tensor<τ,α> sup1, Tensor<τ,α> sup2) {
            if(sup1.Count != sup2.Count)
               return false;
            foreach(var inx_subTnr1 in sup1) {
               var vec = (Vector<τ,α>) inx_subTnr1.Value;
               sup2.TryGetValue(inx_subTnr1.Key, out var subTnr2);
               var vec2 = (Vector<τ,α>) subTnr2;
               if(!vec.Equals(vec2))
                  return false; }
            return true;
         }
      }

      public bool Equals(Tensor<τ,α> tnr2, τ eps) {
         Structure.Equals<int, IntArithmetic>(tnr2.Structure);
         return TnrRecursion(this, tnr2);

         bool TnrRecursion(Tensor<τ,α> sup1, Tensor<τ,α> sup2) {
            if(sup1.Rank > 2) {
               foreach(var inx_subTnr1 in sup1) {
                  if(sup2.TryGetValue(inx_subTnr1.Key, out var subTnr2))
                     return TnrRecursion(inx_subTnr1.Value, subTnr2);
                  else
                     return false; } }
            else
               return VecRecursion(sup1, sup2);
            throw new InvalidOperationException("We shouldn't be here.");
         }

         bool VecRecursion(Tensor<τ,α> sup1, Tensor<τ,α> sup2) {
            foreach(var inx_subTnr1 in sup1) {
               var vec = (Vector<τ,α>) inx_subTnr1.Value;
               sup2.TryGetValue(inx_subTnr1.Key, out var subTnr2);
               var vec2 = (Vector<τ,α>) subTnr2;
               if(!vec.Equals(vec2, eps))
                  return false; }
            return true;
         }                                                         // All values agree within tolerance.
      }
      // #if false // TODO: Implement Merge and Swap on Tensor.
      // /// <summary>Append specified vector to caller.</summary>
      // /// <param name="appTnr">Vector to append.</param>
      // public void MergeWith(Tensor<τ> appTnr) {
      //    Dim += appTnr.Dim;                                      // Readjust width.
      //    foreach(var kvPair in appTnr)
      //       this[kvPair.Key] = kvPair.Value;
      // }
      // /// <summary>Swap two R1 elements specified by indices.</summary>
      // /// <param name="inx1">First element index.</param>
      // /// <param name="inx2">Second element index.</param>
      // public void Swap(int inx1, int inx2) {
      //    bool firstExists = TryGetValue(inx1, out τ val1);
      //    bool secondExists = TryGetValue(inx2, out τ val2);
      //    if(firstExists) {
      //       if(secondExists) {
      //          base[inx1] = val2;
      //          base[inx2] = val1; }
      //       else {
      //          Remove(inx1);                                   // Element at inx1 becomes 0 and is removed.
      //          Add(inx2, val1); } }
      //    else if(secondExists) {
      //       Add(inx1, val2);
      //       Remove(inx2); }                                   // Else nothing happens, both are 0.
      // }
      // /// <summary>Apply element swaps as specified by a swap vector.</summary>
      // /// <param name="swapVec">Vector where an element at index i with integer value j instructs a permutation i->j.</param>
      // public void ApplySwaps(Tensor<int> swapVec) {
      //    foreach(var kVPair in swapVec)
      //       Swap(kVPair.Key, kVPair.Value);   
      // }
      // #endif
      /// <summary>Create a string of all non-zero elements in form {{key1, val1}, {key2, val2}, ..., {keyN,valN}}.</summary>
      public override string ToString() {
         StringBuilder sb = new StringBuilder(72);
         sb.Append("{");
         foreach(var elm in this.OrderBy( kvPair => kvPair.Key ))
            sb.Append($"{{{elm.Key.ToString()}, {elm.Value.ToString()}}}, ");
         int length = sb.Length;
         sb.Remove(length - 2, 2);
         sb.Append("}");
         return sb.ToString();
      }
      /// <summary>Structure that tells Tensor's Copy method how to copy a tensor.</summary>
      public readonly struct CopySpecStruct {
         /// <summary>General CopySpec specifies whether to copy only values, only meta fields or both. Contains flags: Meta (copy meta fields, specify which in MetaFields), Vals (copy values, either shallowly or deeply, specify with EndRank).</summary>
         public readonly GeneralSpecs General;
         /// <summary>MetaFields CopySpec specifies which meta fields to copy. Contains flags: Structure, Rank, Superior.</summary>
         /// <remarks>Effective only if General.Meta flag is active.</remarks>
         public readonly MetaSpecs MetaFields;
         /// <summary>Structure CopySpec specifies whether  to genuinely copy the Structure int[] array or only copy the reference.  Contains flags: TrueCopy (creates a new Structure array on the heap), RefCopy (copies only a reference to the Structure array on source).</summary>
         /// <remarks>Effective only if MetaFields.Structure flag is active.</remarks>
         public readonly StructureSpecs Structure;
         /// <summary>Lowest rank at which copying of values stops.</summary>
         /// <remarks>Effective only if General.Vals flag is active. EndRank is an inclusive lower bound.</remarks>
         public readonly int EndRank;
         public readonly int ExtraCapacity;
         
         public CopySpecStruct(GeneralSpecs gs = GeneralSpecs.Both, MetaSpecs ms = MetaSpecs.All,
         StructureSpecs ss = StructureSpecs.TrueCopy, int endRank = 0, int extraCapacity = 0) {
            General = gs;
            MetaFields = ms;
            Structure = ss;
            EndRank = endRank;
            ExtraCapacity = extraCapacity;
         }
      }
      /// <summary>Possible CopySpec settings.</summary>
      [Flags] public enum GeneralSpecs {
         Meta  = 1,
         Vals  = 1 << 1,
         Both = Meta | Vals
      }
      /// <summary>Possible StructureCopySpec settings.</summary>
      [Flags] public enum StructureSpecs {
         TrueCopy = 1,
         RefCopy  = 1 << 1
      }
      /// <summary>Possible MetaCopySpec settings.</summary>
      [Flags] public enum MetaSpecs {
         None        = 0,
         Structure   = 1,
         Rank        = 1 << 1,
         Sup         = 1 << 2,
         All = Structure | Rank | Sup,
         AllExceptSup = All & ~Sup
      }
   }
}