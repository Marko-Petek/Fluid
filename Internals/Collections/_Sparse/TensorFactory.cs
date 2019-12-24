using System;
using System.Collections.Generic;
using System.Linq;
using Fluid.Internals.Numerics;
namespace Fluid.Internals.Collections {
   using static Fluid.Internals.Collections.HowToCopyStructure;
   using static Fluid.Internals.Collections.WhichNonValueFields;
   using static Fluid.Internals.Collections.WhichFields;

/// <summary>A type responsible for Tensor and Vector creation.</summary>
/// <typeparam name="τ">Value types contained inside the tensor or vector.</typeparam>
/// <typeparam name="α">Arithmetic type.</typeparam>
public static class TensorFactory<τ,α>
where τ : IEquatable<τ>, new()
where α : IArithmetic<τ>, new() {

   //public static Tensor<τ,α> CreateTopTensor()

   public static Vector<τ,α> CreateEmpty(int cap, List<int> structure, Tensor<τ,α> sup) {
      var vec = new Vector<τ,α>(structure, sup, cap);
      return vec;
   }
   public static Vector<τ,α> CreateEmpty(int cap, List<int> structure) =>
      CreateEmpty(cap, structure, Voids<τ,α>.Vec);
   
   public static Vector<τ,α> CreateEmpty(int cap) =>
      CreateEmpty(cap, Voids.ListInt, Voids<τ,α>.Vec);
   
   /// <summary>Create an empty tensor with optionally specified structure and superior.</summary>
   /// <param name="cap">Capacity.</param>
   /// <param name="structure">Structure whose reference will be absorbed into the new tensor.</param>
   public static Tensor<τ,α> CreateEmptyTensor(int cap, int rank,
   List<int> structure, Tensor<τ,α>? sup) =>
      rank switch {
         1 => Vector<τ,α>.CreateEmpty(cap, structure, sup),
         _ => new Tensor<τ,α>(structure, rank, sup, cap) };
   
   public static Tensor<τ,α> CreateEmptyTensor(int cap, int rank, List<int> structure) =>
      CreateEmptyTensor(cap, rank, structure, sup: null);

   public static Tensor<τ,α> CreateEmptyTensor(int cap, int rank) =>
      CreateEmptyTensor(cap, rank, Voids.ListInt, Voids<τ,α>.Vec);

   /// <summary>Creates a vector with specified dimension and initial capacity.</summary>
   /// <param name="dim">Dimension.</param>
   /// <param name="cap">Initial capacity.</param>
   public static Vector<τ,α> CreateTopVector(int dim, int cap) =>
      new Vector<τ,α>(new List<int> {dim}, Voids<τ,α>.Vec, cap);

   /// <summary>Creates a top tensor with specified structure and initial capacity. Rank is assigned as the length of structure array.</summary>
   /// <param name="structure">Specifies dimension of each rank.</param>
   /// <param name="cap">Initially assigned memory.</param>
   public static Tensor<τ,α> CreateTopTensor(List<int> structure, int cap = 6) =>
      new Tensor<τ,α>(structure, cap);

   /// <summary>Creates a non-top vector from an array slice.</summary>
   /// <param name="slc">Array slice.</param>
   /// <param name="strc">The existing structure that the vector should atain.</param>
   /// <param name="sup">Direct superior.</param>
   public static Vector<τ,α> VectorFromFlatSpec(Span<τ> slc , List<int> strc, Tensor<τ,α> sup) {
      var vec = new Vector<τ,α>(strc, sup, slc.Length);
      for(int i = 0; i < slc.Length; ++i) {
         if(!slc[i].Equals(O<τ,α>.A.Zero()))
            vec.Vals.Add(i, slc[i]); }
      return vec;
   }
   public static Vector<τ,α> VectorFromFlatSpec(Span<τ> slc) =>
      VectorFromFlatSpec(slc, new List<int>(1) { slc.Length }, Voids<τ,α>.Vec);

   /// <summary>Creates a tensor with specified structure from values provided within a Span.</summary>
   /// <param name="slice">Span of values.</param>
   /// <param name="structure">Structure of new tensor.</param>
   public static Tensor<τ,α> TensorFromFlatSpec(Span<τ> slice, params int[] structure) {
      int tnrRank = structure.Length;
      if(tnrRank == 1)
         return TensorFactory<τ,α>.VectorFromFlatSpec(slice);
      else {
         var res = new Tensor<τ,α>(structure.ToList(), tnrRank, Voids<τ,α>.Vec, structure[0]);
         Recursion(slice, 0, res);
         return res;
      }

      void Recursion(Span<τ> slc, int slot, Tensor<τ,α> tgt) {       // Specifiy slice and the structure dimension (natural rank index) to which it belongs.
         //int tgtRank = //ChangeRankNotation(structure.Length, dim);
         int nIter = structure[slot];                              // As many iterations as it is the size of the dimension.
         int nEmtsInSlice = slc.Length / nIter;
         if(tgt.Rank > 2) {
            //var res = new Tensor<τ,α>(structure.ToList(), trueRank, null, structure[dim]);
            for(int i = 0; i < nIter; ++i) {                      // Over each tensor. Create new slices and run recursion on them.
               var newSlc = slc.Slice(i*nEmtsInSlice, nEmtsInSlice);
               var subTnr = new Tensor<τ,α>(tgt.Structure, tgt.Rank - 1, tgt, structure[slot]);
               Recursion(newSlc, slot + 1, subTnr);
               if(subTnr.Count != 0)
                  tgt.AddOnly(i, subTnr); } }
         else {                                                 // We are at rank 2, subrank = vector rank.
            for(int i = 0; i < nIter; ++i) {
               var newSlc = slc.Slice(i*nEmtsInSlice, nEmtsInSlice);
               var subVec = TensorFactory<τ,α>.VectorFromFlatSpec(newSlc, tgt.Structure, tgt);
               if(subVec.Count != 0)
                  tgt.AddOnly(i, subVec); } }
      }
   }

   /// <summary>Creates a deep copy of a vector. You have to provide the already instantiated target.</summary>
   /// <param name="src">Copy source.</param>
   /// <param name="tgt">Copy target.</param>
   public static void Copy(Vector<τ,α> src, Vector<τ,α> tgt, in CopySpecStruct cs) {
      CopyMetaFields(src, tgt, cs.NonValueFieldsSpec, cs.StructureSpec);               // Structure created here.
      if(cs.FieldsSpec.HasFlag(OnlyValues)) {
         tgt.Vals = new Dictionary<int,τ>(src.Count + cs.ExtraCapacity);
            foreach(var int_val in src.Vals) {
               tgt.Vals.Add(int_val.Key, int_val.Value); } }
      else {
         tgt.Vals = new Dictionary<int,τ>(); }
   }
   /// <summary>Make a shallow or deep copy of a tensor. Set CopySpec field for fine tunning, ensure proper capacity of the target tensor.</summary>
   /// <param name="aSrc">Copy source.</param>
   /// <param name="aTgt">Copy target.</param>
   /// <param name="css">Exact specification of what fields to copy. Default is all.</param>
   public static void Copy(in Tensor<τ,α> aSrc, Tensor<τ,α> aTgt, in CopySpecStruct css) {
      Assume.True(aSrc.Rank > 1, () =>
         "Tensors's rank has to be at least 2 to be copied via this method.");
      CopyMetaFields(aSrc, aTgt, in css.NonValueFieldsSpec, in css.StructureSpec);
      if(css.FieldsSpec.HasFlag(OnlyValues)) {
         var newStruc = aTgt.Structure;                                             // At this point top tensor tgt has a structure created by CopyMetaFields. It will be assigned to all subsequent subtensors.
         int endRank = css.EndRank;
         Recursion(aSrc, aTgt);

         void Recursion(Tensor<τ,α> src, Tensor<τ,α> tgt) {
         if(src.Rank > 2) {                                       // Subordinates are tensors.
            foreach (var int_subSrc in src) {
               int subKey = int_subSrc.Key;
               var subSrc = int_subSrc.Value;
               var subTgt = new Tensor<τ,α>(newStruc, subSrc.Rank, tgt, subSrc.Count);
               tgt.AddOnly(subKey, subTgt);
               if(src.Rank > endRank)
                  Recursion(subSrc, subTgt); } }
         else if(src.Rank == 2) {                                 // Subordinates are vectors.
            foreach(var int_subSrc in src) {
               int subKey = int_subSrc.Key;
               var subVec = (Vector<τ,α>) int_subSrc.Value;
               var subTgt = new Vector<τ,α>(newStruc, tgt, subVec.Count);
               tgt.AddOnly(subKey, subTgt);
               subTgt.Vals = new Dictionary<int,τ>(subVec.Vals);
            } }
         else
            throw new InvalidOperationException(
               "Tensors's rank has to be at least 2 to be copied via this method."); }
      }
   }
   public static void CopyMetaFields(Tensor<τ,α> src, Tensor<τ,α> tgt, in WhichNonValueFields wnvf,
   in HowToCopyStructure htcs) {
      if( wnvf.HasFlag(Structure)) {
         if(htcs.HasFlag(ReferToOriginalStructure))
            tgt.Structure = src.Structure;
         else
            tgt.Structure = new List<int>(src.Structure); }
      else {                                                                        // Create empty Structure, don't just assign VoidStructure. This way we can change it and impact all subtensors.
         tgt.Structure = new List<int>(4); }
      if(wnvf.HasFlag(Rank))
         tgt.Rank = src.Rank;
      if(wnvf.HasFlag(Superior))
         tgt.Superior = src.Superior;
   }

}

}