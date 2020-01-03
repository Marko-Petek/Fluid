using System;
using System.Collections.Generic;
using System.Linq;
using Fluid.Internals.Numerics;
namespace Fluid.Internals.Collections {
   using static Fluid.Internals.Collections.HowToCopyStructure;
   using static Fluid.Internals.Collections.WhichNonValueFields;
   using static Fluid.Internals.Collections.WhichFields;

/// <summary>A type responsible for construction.</summary>
public static partial class Factory {
   /// <summary>Creates a top vector (null superior) with specified dimension and initial capacity.</summary>
   /// <param name="dim">Dimension.</param>
   /// <param name="cap">Initial capacity.</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static Vector<τ,α> TopVector<τ,α>(int dim, int cap = 6)
   where τ : struct, IEquatable<τ>, IComparable<τ>
   where α : IArithmetic<τ>, new() =>
      new Vector<τ,α>(new List<int> {dim}, null, cap);
   /// <summary>Creates a non-top vector (non-null superior) with specified dimension and initial capacity. Adds it to its specified superior at the specified index.</summary>
   /// <param name="sup">Direct superior.</param>
   /// <param name="inx">Index inside superior.</param>
   /// <param name="dim">Dimension.</param>
   /// <param name="cap">Initial capacity.</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static Vector<τ,α> SubVector<τ,α>(Tensor<τ,α> sup, int inx, int dim, int cap = 6)
   where τ : struct, IEquatable<τ>, IComparable<τ>
   where α : IArithmetic<τ>, new() {
      var vec = new Vector<τ,α>(new List<int> {dim}, sup, cap);
      sup[Vector<τ,α>.V, inx] = vec;
      return vec;
   }
   /// <summary>Creates a top vector from an array span.</summary>
   /// <param name="span">Array span of values.</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static Vector<τ,α> TopVecFromSpan<τ,α>(Span<τ> span)
   where τ : struct, IEquatable<τ>, IComparable<τ>
   where α : IArithmetic<τ>, new() {
      var vec = new Vector<τ,α>(new List<int>(1) {span.Length}, null, span.Length);
      for(int i = 0; i < span.Length; ++i) {
         if(!span[i].Equals(default(τ)))
            vec.Add(i, span[i]); }
      return vec;
   }
   /// <summary>Creates a non-top vector from an array span. Adds it to its specified superior at the specified index.</summary>
   /// <param name="span">Array span of values.</param>
   /// <param name="sup">Direct superior with an existing structure.</param>
   /// <param name="inx">Index inside superior.</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static Vector<τ,α> SubVecFromSpan<τ,α>(Span<τ> span, Tensor<τ,α> sup, int inx)
   where τ : struct, IEquatable<τ>, IComparable<τ>
   where α : IArithmetic<τ>, new() {
      var vec = new Vector<τ,α>(sup.Structure, sup, span.Length);
      for(int i = 0; i < span.Length; ++i) {
         if(!span[i].Equals(default(τ)))
            vec.Add(i, span[i]); }
      sup[Vector<τ,α>.V, inx] = vec;
      return vec;
   }
   /// <summary>Creates a top tensor (null superior) with specified structure and initial capacity. Rank is assigned as the length of structure array.</summary>
   /// <param name="structure">Specifies dimension of each rank.</param>
   /// <param name="cap">Initially assigned memory.</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static Tensor<τ,α> TopTensor<τ,α>(List<int> structure, int cap = 6)
   where τ : struct, IEquatable<τ>, IComparable<τ>
   where α : IArithmetic<τ>, new() =>
      new Tensor<τ,α>(structure, cap);

   /// <summary>Creates a non-top tensor (non-null superior) and adds it to its specified superior at the specified index. Assumes superior's structure is initialized.</summary>
   /// <param name="sup">Direct superior.</param>
   /// <param name="inx">Index inside superior.</param>
   /// <param name="cap">Capacity of internal dictionary.</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static Tensor<τ,α> SubTensor<τ,α>(Tensor<τ,α> sup, int inx, int cap = 6)
   where τ : struct, IEquatable<τ>, IComparable<τ>
   where α : IArithmetic<τ>, new() {
      var tnr = new Tensor<τ,α>(sup, cap);
      sup[Tensor<τ,α>.V, inx] = tnr;
      return tnr;
   }
   
   
   /// <summary>Creates a top tensor from an array span.</summary>
   /// <param name="span">Array span of values.</param>
   /// <param name="strc">Structure.</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static Tensor<τ,α> TopTnrFromSpan<τ,α>(Span<τ> span, params int[] strc)
   where τ : struct, IEquatable<τ>, IComparable<τ>
   where α : IArithmetic<τ>, new() {
      int rank = strc.Length;
      if(rank == 1)
         return TopVecFromSpan<τ,α>(span);
      else {
         var res = new Tensor<τ,α>(strc.ToList(), rank, null, strc[0]);             // Empty tensor that enters recursion.
         Recursion(span, 0, res);
         return res;
      }

      void Recursion(Span<τ> spn, int slot, Tensor<τ,α> tgt) {                      // Span and natural rank index to which it belongs.
         int nIter = strc[slot];                                                    // As many iterations as slot dimension.
         int nEmtsInSpan = spn.Length / nIter;
         if(tgt.Rank > 2) {
            for(int i = 0; i < nIter; ++i) {                                        // Over each tensor. Create new spans and run recursion on them.
               var newSpn = spn.Slice(i*nEmtsInSpan, nEmtsInSpan);
               var subTnr = new Tensor<τ,α>(tgt, strc[slot]);
               Recursion(newSpn, slot + 1, subTnr);
               if(subTnr.Count != 0)
                  tgt.Add(i, subTnr); } }
         else {                                                                     // We are at rank 2, subrank = vector rank.
            for(int i = 0; i < nIter; ++i) {
               var newSlc = spn.Slice(i*nEmtsInSpan, nEmtsInSpan);
               var subVec = SubVecFromSpan<τ,α>(newSlc, tgt);
               if(subVec.Count != 0)
                  tgt.Add(i, subVec); } }
      }
   }
   /// <summary>Creates a deep copy of a vector as a top vector (null superior).</summary>
   /// <param name="src">Copy source.</param>
   /// <param name="extraCap">Extra capacity of copied vector (beyond the number of elements).</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static Vector<τ,α> CopyAsTopVec<τ,α>(Vector<τ,α> src, int extraCap)
   where τ : struct, IEquatable<τ>, IComparable<τ>
   where α : IArithmetic<τ>, new() {
      var vec = TopVector<τ,α>(src.Dim, src.Count + extraCap);
      foreach(var int_val in src.Vals)
         vec.Add(int_val.Key, int_val.Value);
      return vec;
   }
   /// <summary>Creates a deep copy of a vector as a non-top vector (non-null superior).</summary>
   /// <param name="src">Copy source.</param>
   /// <param name="newSup">The copied vector's superior.</param>
   /// <param name="extraCap">Extra capacity of copied vector (beyond the number of elements).</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static Vector<τ,α> CopyAsSubVec<τ,α>(Vector<τ,α> src, Tensor<τ,α> newSup, int extraCap)
   where τ : struct, IEquatable<τ>, IComparable<τ>
   where α : IArithmetic<τ>, new() {
      var vec = SubVector<τ,α>(newSup, src.Dim, src.Count + extraCap);
      foreach(var int_val in src.Vals)
         vec.Add(int_val.Key, int_val.Value);
      return vec;
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
   public static void Copy<τ,α>(in Tensor<τ,α> aSrc, Tensor<τ,α> aTgt, in CopySpecStruct css)
   where τ : struct, IEquatable<τ>, IComparable<τ>
   where α : IArithmetic<τ>, new() {
      if (aSrc.Rank == 1) {
         var res = new Vector<τ,α>(Count + cs.ExtraCapacity);
         var thisVector = (Vector<τ,α>)this;
         Factory<τ,α>.Copy(thisVector, res, in cs);
         return res; }
      else {
         var res = new Tensor<τ,α>(Count + cs.ExtraCapacity);
         Factory<τ,α>.Copy(this, res, in cs);
         return res; }

      Assume.True(aSrc.Rank > 1, () =>
         "Tensors's rank has to be at least 2 to be copied via this method.");
      CopyMetaFields(aSrc, aTgt, in css.NonValueFieldsSpec, in css.StructureSpec);
      if(css.FieldsSpec.HasFlag(OnlyValues)) {
         var newStrc = aTgt.Structure;                                             // At this point top tensor tgt has a structure created by CopyMetaFields. It will be assigned to all subsequent subtensors.
         int endRank = css.EndRank;
         Recursion(aSrc, aTgt);

         void Recursion(Tensor<τ,α> src, Tensor<τ,α> tgt) {
         if(src.Rank > 2) {                                       // Subordinates are tensors.
            foreach (var int_subSrc in src) {
               int subKey = int_subSrc.Key;
               var subSrc = int_subSrc.Value;
               var subTgt = new Tensor<τ,α>(newStrc, subSrc.Rank, tgt, subSrc.Count);
               tgt.Add(subKey, subTgt);
               if(src.Rank > endRank)
                  Recursion(subSrc, subTgt); } }
         else if(src.Rank == 2) {                                 // Subordinates are vectors.
            foreach(var int_subSrc in src) {
               int subKey = int_subSrc.Key;
               var subVec = (Vector<τ,α>) int_subSrc.Value;
               var subTgt = new Vector<τ,α>(newStrc, tgt, subVec.Count);
               tgt.Add(subKey, subTgt);
               subTgt.Vals = new Dictionary<int,τ>(subVec.Vals);
            } }
         else
            throw new InvalidOperationException(
               "Tensors's rank has to be at least 2 to be copied via this method."); }
      }
   }
   internal static void CopyTnr<τ,α>(this Tensor<τ,α> aSrc, Tensor<τ,α> aTgt, in CopySpecStruct css)
   where τ : struct, IEquatable<τ>, IComparable<τ>
   where α : IArithmetic<τ>, new() {

   }
   public static void CopyMetaFields<τ,α>(Tensor<τ,α> src, Tensor<τ,α> tgt,
   in WhichNonValueFields wnvf, in HowToCopyStructure htcs)
   where τ : struct, IEquatable<τ>, IComparable<τ>
   where α : IArithmetic<τ> {
      if(wnvf.HasFlag(Structure)) {
         if(htcs.HasFlag(ReferToOriginalStructure))            // FIXME: Problem: structure assignment.
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