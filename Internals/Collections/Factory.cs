using System;
using System.Collections.Generic;
using System.Linq;
using Fluid.Internals.Numerics;
namespace Fluid.Internals.Collections {

/// <summary>A type responsible for construction.</summary>
public static class Factory {
   /// <summary>Creates a top vector (null superior) with specified dimension and initial capacity.</summary>
   /// <param name="dim">Dimension.</param>
   /// <param name="cap">Initial capacity.</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static Vector<τ,α> TopVector<τ,α>(int dim, int cap = 6)
   where τ : struct, IEquatable<τ>, IComparable<τ>
   where α : IArithmetic<τ>, new() =>
      new Vector<τ,α>(dim, cap);
   /// <summary>Creates a non-top vector (non-null superior) with specified initial capacity. Adds it to its specified superior at the specified index. Dimension is inferred from superior's structure.</summary>
   /// <param name="sup">Direct superior.</param>
   /// <param name="inx">Index inside superior.</param>
   /// <param name="cap">Initial capacity.</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static Vector<τ,α> SubVector<τ,α>(Tensor<τ,α> sup, int inx, int cap = 6)
   where τ : struct, IEquatable<τ>, IComparable<τ>
   where α : IArithmetic<τ>, new() {
      Assume.True(sup.Rank == 2, () => "Vector's superior rank not 2. You can only create a subvector with a rank 2 superior.");
      var vec = new Vector<τ,α>(sup, cap);
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
      var vec = new Vector<τ,α>(span.Length, span.Length);
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
   public static Vector<τ,α>? SubVecFromSpan<τ,α>(Span<τ> span, Tensor<τ,α> sup, int inx)
   where τ : struct, IEquatable<τ>, IComparable<τ>
   where α : IArithmetic<τ>, new() {
      Assume.True(sup.Rank == 2, () => "Vector's superior rank not 2. You can only create a subvector with a rank 2 superior.");
      var vec = new Vector<τ,α>(sup, span.Length);
      for(int i = 0; i < span.Length; ++i) {
         if(!span[i].Equals(default(τ)))
            vec.Add(i, span[i]); }
      if(vec.Count > 0) {                                                           // Created vector is not empty.
         sup[Vector<τ,α>.V, inx] = vec;
         return vec; }
      else
         return null;
   }
   /// <summary>Creates a top tensor (null superior) with specified structure and initial capacity. Rank is assigned as the length of structure array.</summary>
   /// <param name="strc">Specifies dimension of each rank.</param>
   /// <param name="cap">Initially assigned memory.</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static Tensor<τ,α> TopTensor<τ,α>(List<int> strc, int cap = 6)
   where τ : struct, IEquatable<τ>, IComparable<τ>
   where α : IArithmetic<τ>, new() {
      Assume.True(strc.Count > 1, () =>
         "For creating tensors of rank 1 use Vector's constructor.");
      return new Tensor<τ,α>(strc, cap);
   }
   /// <summary>Creates a non-top tensor (non-null superior) and adds it to its specified superior at the specified index. Assumes superior's structure is initialized.</summary>
   /// <param name="sup">Direct superior.</param>
   /// <param name="inx">Index inside superior.</param>
   /// <param name="cap">Capacity of internal dictionary.</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static Tensor<τ,α> SubTensor<τ,α>(Tensor<τ,α> sup, int inx, int cap = 6)
   where τ : struct, IEquatable<τ>, IComparable<τ>
   where α : IArithmetic<τ>, new() {
      Assume.True(sup.Rank > 2, () =>
         "Superior's rank too low. For creating tensors of rank 1 use Factory.SubVector.");
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
         var res = new Tensor<τ,α>(strc.ToList(), strc[0]);                         // Empty tensor that enters recursion.
         Recursion(span, 0, res);
         return res; }

      void Recursion(Span<τ> spn, int slot, Tensor<τ,α> tgt) {                      // Span and natural slot index to which it belongs.
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
               var subVec = SubVecFromSpan<τ,α>(newSlc, tgt, i); } } }
   }
   /// <summary>Creates a deep copy of a vector as a top vector (null superior).</summary>
   /// <param name="src">Copy source.</param>
   /// <param name="extraCap">Extra capacity of copied vector (beyond existing Count).</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static Vector<τ,α> CopyAsTopVec<τ,α>(Vector<τ,α> src, int extraCap = 0)
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
   /// <param name="inx">New index inside superior.</param>
   /// <param name="xCap">Extra capacity of copied vector (beyond the number of elements).</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static Vector<τ,α> CopyAsSubVec<τ,α>(Vector<τ,α> src, Tensor<τ,α> newSup,
   int inx, int xCap = 0)
   where τ : struct, IEquatable<τ>, IComparable<τ>
   where α : IArithmetic<τ>, new() {
      var vec = SubVector<τ,α>(newSup, inx, src.Count + xCap);
      Assume.True(src.Dim == vec.Dim, () => "The dimension as specified by the superior does not equal the original vector's dimension.");
      foreach(var sInx_sVal in src.Vals)
         vec.Add(sInx_sVal.Key, sInx_sVal.Value);
      return vec;
   }
   /// <summary>Creates a deep copy of a tensor as a top tensor (null superior).</summary>
   /// <param name="src">Copy source.</param>
   /// <param name="xCap">Extra capacity of all copied (sub)tensors (beyond existing Count).</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static Tensor<τ,α> CopyAsTopTnr<τ,α>(Tensor<τ,α> src, int xCap = 0)
   where τ : struct, IEquatable<τ>, IComparable<τ>
   where α : IArithmetic<τ>, new() {
      if (src is Vector<τ,α> vec) {
         return CopyAsTopVec<τ,α>(vec, xCap); }
      else {
         var newStrc = new List<int>(src.Structure);
         var tgt = TopTensor<τ,α>(newStrc, src.Count + xCap);
         foreach(var sInx_sSrc in src) {
            int sInx = sInx_sSrc.Key;
            var sSrc = sInx_sSrc.Value;
            CopyAsSubTnr<τ,α>(sSrc, tgt, sInx, xCap); }
         return tgt; }
   }
   /// <summary>Creates a deep copy of a tensor as a non-top tensor (non-null superior).</summary>
   /// <param name="src">Copy source.</param>
   /// <param name="newSup">The copied tensor's superior.</param>
   /// <param name="inx">New index inside superior.</param>
   /// <param name="xCap">Extra capacity of all copied (sub)tensors (beyond existing Count).</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static Tensor<τ,α> CopyAsSubTnr<τ,α>(Tensor<τ,α> src, Tensor<τ,α> newSup, int inx, int xCap)
   where τ : struct, IEquatable<τ>, IComparable<τ>
   where α : IArithmetic<τ>, new() {
      if (src is Vector<τ,α> vec) {
         return CopyAsSubVec<τ,α>(vec, newSup, inx, xCap); }
      else {
         return Recursion(src, newSup, inx); }

      Tensor<τ,α> Recursion(Tensor<τ,α> srcR, Tensor<τ,α> newSupR, int inxR) {
         var tgtR = SubTensor<τ,α>(newSupR, inxR, srcR.Count + xCap);               // TODO: Check that count is not 0 anywhere.
         if(srcR.Rank > 2) {                                                        // Subordinates are tensors.
            foreach (var inxR_sSrcR in srcR) {
               int sInxR = inxR_sSrcR.Key;
               var sSrcR = inxR_sSrcR.Value;
               Recursion(sSrcR, tgtR, sInxR); } }
         else {                                                                     // Subordinates are vectors.
            foreach(var inxR_sSrcR in srcR) {
               int sInxR = inxR_sSrcR.Key;
               var sSrcR = (Vector<τ,α>) inxR_sSrcR.Value;
               CopyAsSubVec<τ,α>(sSrcR, tgtR, sInxR, xCap); } }
         return tgtR; }
   }
}

}