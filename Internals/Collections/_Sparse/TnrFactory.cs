using System;
using System.Collections.Generic;
using System.Linq;
using Fluid.Internals.Numerics;
namespace Fluid.Internals.Collections {

/// <summary>A type responsible for construction.</summary>
public static class TnrFactory {
   /// <summary>Creates a top vector (null superior) with specified dimension and initial capacity.</summary>
   /// <param name="dim">Dimension.</param>
   /// <param name="cap">Initial capacity.</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static Vec<τ,α> TopVec<τ,α>(int dim, int cap = 6)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() =>
      new Vec<τ,α>(dim, cap);
   /// <summary>Creates a top vector (null superior) with dimension and capacity inferred from a specified vector.</summary>
   /// <param name="vec">New vector will take on same dimension and capacity as this one.</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static Vec<τ,α> TopVec<τ,α>(Vec<τ,α> vec)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() =>
      new Vec<τ,α>(vec.Dim, vec.Count);
   
   /// <summary>Creates a non-top vector (non-null superior) with specified initial capacity. Adds it to its specified superior at the specified index. Dimension is inferred from superior's structure.</summary>
   /// <param name="sup">Direct superior.</param>
   /// <param name="inx">Index inside superior.</param>
   /// <param name="cap">Initial capacity.</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static Vec<τ,α> SubVec<τ,α>(this Tnr<τ,α> sup, int inx, int cap = 6)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() {
      Assume.True(sup.Rank == 2, () => "Vector's superior rank not 2. You can only create a subvector with a rank 2 superior.");
      return SubVecß(sup, inx, cap);
   }

   /// <summary>Does not check whether the specified superior is rank 2.</summary>
   /// <param name="sup">Direct superior.</param>
   /// <param name="inx">Index inside superior.</param>
   /// <param name="cap">Initial capacity.</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   internal static Vec<τ,α> SubVecß<τ,α>(this Tnr<τ,α> sup, int inx, int cap = 6)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() {
      var vec = new Vec<τ,α>(sup, cap);
      sup[Vec<τ,α>.V, inx] = vec;
      return vec;
   }

   /// <summary>Creates a top vector from an array span.</summary>
   /// <param name="span">Array span of values.</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static Vec<τ,α>? TopVecFromSpan<τ,α>(Span<τ> span)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() {
      var vec = new Vec<τ,α>(span.Length, span.Length);
      for(int i = 0; i < span.Length; ++i) {
         if(!span[i].Equals(NonNullable<τ,α>.O.Zero()))
            vec.Add(i, span[i]); }
      return vec.Count != 0 ? vec : null;
   }
   /// <summary>Creates a non-top vector from an array span. Adds it to its specified superior at the specified index.</summary>
   /// <param name="span">Array span of values.</param>
   /// <param name="sup">Direct superior with an existing structure.</param>
   /// <param name="inx">Index inside superior.</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static Vec<τ,α>? SubVecFromSpan<τ,α>(Span<τ> span, Tnr<τ,α> sup, int inx)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() {
      Assume.True(sup.Rank == 2, () => "Vector's superior rank not 2. You can only create a subvector with a rank 2 superior.");
      var vec = new Vec<τ,α>(sup, span.Length);
      for(int i = 0; i < span.Length; ++i) {
         if(!span[i].Equals(NonNullable<τ,α>.O.Zero()))
            vec.Add(i, span[i]); }
      if(vec.Count > 0) {                                                           // Created vector is not empty.
         sup[Vec<τ,α>.V, inx] = vec;
         return vec; }
      else
         return null;
   }
   /// <summary>Creates a top tensor (null superior) with specified structure and initial capacity. Rank is assigned as the length of structure array.</summary>
   /// <param name="strc">Specifies dimension of each rank.</param>
   /// <param name="cap">Initially assigned memory.</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static Tnr<τ,α> TopTensor<τ,α>(List<int> strc, int cap = 6)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() {
      Assume.True(strc.Count > 1, () =>
         "For creating tensors of rank 1 use Vector's constructor.");
      return new Tnr<τ,α>(strc, cap);
   }
   
   /// <summary>Creates a non-top tensor (non-null superior) and adds it to its specified superior at the specified index. Assumes superior's structure is initialized.</summary>
   /// <param name="sup">Direct superior.</param>
   /// <param name="inx">Index inside superior.</param>
   /// <param name="cap">Capacity of internal dictionary.</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static Tnr<τ,α> SubTensor<τ,α>(this Tnr<τ,α> sup, int inx, int cap = 6)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() {
      Assume.True(sup.Rank > 1, () =>
         "Superior's rank too low. Cannot create a subtensor on a vector.");
      return SubTnrß(sup, inx, cap);
   }

   /// <summary>Does not check if superior's rank is at least 2.</summary>
   /// <param name="sup">Direct superior.</param>
   /// <param name="inx">Index inside superior.</param>
   /// <param name="cap">Capacity of internal dictionary.</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   /// <returns></returns>
   internal static Tnr<τ,α> SubTnrß<τ,α>(this Tnr<τ,α> sup, int inx, int cap = 6)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() {
      if(sup.Rank > 2) {
         var tnr = new Tnr<τ,α>(sup, cap);
         sup[Tnr<τ,α>.T, inx] = tnr;
         return tnr; }
      else
         return sup.SubVecß(inx, cap);
   }

   /// <summary>Creates a top tensor from an array span.</summary>
   /// <param name="span">Array span of values.</param>
   /// <param name="strc">Structure.</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static Tnr<τ,α>? TopTnrFromSpan<τ,α>(Span<τ> span, params int[] strc)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() {
      int rank = strc.Length;
      if(rank == 1)
         return TopVecFromSpan<τ,α>(span);
      else {
         var res = new Tnr<τ,α>(strc.ToList(), strc[0]);                         // Empty tensor that enters recursion.
         Recursion(span, 0, res);
         return res.Count != 0 ? res : null; }

      void Recursion(Span<τ> spn, int slot, Tnr<τ,α> tgt) {                      // Span and natural slot index to which it belongs.
         int nIter = strc[slot];                                                    // As many iterations as slot dimension.
         int nEmtsInSpan = spn.Length / nIter;
         if(tgt.Rank > 2) {
            for(int i = 0; i < nIter; ++i) {                                        // Over each tensor. Create new spans and run recursion on them.
               var newSpn = spn.Slice(i*nEmtsInSpan, nEmtsInSpan);
               var subTnr = new Tnr<τ,α>(tgt, strc[slot]);
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
   public static Vec<τ,α>? CopyAsTopVec<τ,α>(this Vec<τ,α>? src, int extraCap = 0)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() =>
      src != null ? src.CopyAsTopVecß(extraCap) : null;
   internal static Vec<τ,α> CopyAsTopVecß<τ,α>(this Vec<τ,α> src, int extraCap = 0)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() {
      var copy = TopVec<τ,α>(src.Dim, src.Count + extraCap);
      foreach(var (i, s) in src.Scals)
         copy.Add(i, s);
      return copy;
   }
   /// <summary>Creates a deep copy of a vector as a non-top vector (non-null superior).</summary>
   /// <param name="src">Copy source.</param>
   /// <param name="newSup">The copied vector's superior.</param>
   /// <param name="inx">New index inside superior.</param>
   /// <param name="xCap">Extra capacity of copied vector (beyond the number of elements).</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static Vec<τ,α>? CopyAsSubVec<τ,α>(this Vec<τ,α>? src, Tnr<τ,α> newSup,
   int inx, int xCap = 0)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() {
      if(src != null) {
         return src.CopyAsSubVecß(newSup, inx, xCap); }
      else
         return null;
   }

   /// <summary>No null checks. Creates a deep copy of a vector as a non-top vector (non-null superior).</summary>
   /// <param name="src"></param>
   /// <param name="newSup"></param>
   /// <param name="inx"></param>
   /// <param name="xCap"></param>
   /// <typeparam name="τ"></typeparam>
   /// <typeparam name="α"></typeparam>
   /// <returns></returns>
   internal static Vec<τ,α> CopyAsSubVecß<τ,α>(this Vec<τ,α> src, Tnr<τ,α> newSup,
   int inx, int xCap = 0)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() {
      Assume.True(src.Dim == newSup.Strc[newSup.StrcInx + 1], () => "Source vector's dimension does not equal the dimension of slot where copied subvector will be placed.");
      var copy = SubVec<τ,α>(newSup, inx, src.Count + xCap);            // New superior's rank will be checked here.
      foreach(var (i, s) in src.Scals)
         copy.Add(i, s);
      return copy;
   }

   /// <summary>Creates a deep copy of a tensor as a top tensor (null superior). Source tensor can be R1.</summary>
   /// <param name="src">Copy source.</param>
   /// <param name="xCap">Extra capacity of all copied (sub)tensors (beyond existing Count).</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static Tnr<τ,α>? CopyAsTopTnr<τ,α>(this Tnr<τ,α>? src, int xCap = 0)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() {
      if(src != null) {
         if(src.Rank > 1)
            return src.CopyAsTopTnrß(xCap);
         else {
            var v = (Vec<τ,α>) src;
            return v.CopyAsTopVecß(xCap); } }
      else
         return null;
   }
   
   /// <summary>Passed source has to be at least R2. Creates a deep copy of a tensor as a top tensor (null superior).</summary>
   /// <param name="src">Copy source at least R2.</param>
   /// <param name="xCap">Extra capacity of all copied (sub)tensors (beyond existing Count).</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   internal static Tnr<τ,α> CopyAsTopTnrß<τ,α>(this Tnr<τ,α> src, int xCap = 0)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() {
      Assume.True(src.Rank > 1, () => "Passed source tensor has to be at least rank 2.");
      var newStrc = new List<int>(src.Strc);
      var copy = TopTensor<τ,α>(newStrc, src.Count + xCap);
      foreach(var (i, t) in src)
         t.CopyAsSubTnrß<τ,α>(copy, i, xCap);
      return copy;
   }
   /// <summary>Creates a deep copy of a tensor as a non-top tensor (non-null superior).</summary>
   /// <param name="src">Copy source.</param>
   /// <param name="newSup">The copied tensor's superior.</param>
   /// <param name="inx">New index inside superior.</param>
   /// <param name="xCap">Extra capacity of all copied (sub)tensors (beyond existing Count).</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static Tnr<τ,α>? CopyAsSubTnr<τ,α>(this Tnr<τ,α>? src, Tnr<τ,α> newSup,
   int inx, int xCap = 0)  where τ : IEquatable<τ>, IComparable<τ> where α : IArithmetic<τ>, new() {
      if(src != null) {
         if(src.Rank > 1)
            return src.CopyAsSubTnrß(newSup, inx, xCap);
         else {
            var v = (Vec<τ,α>) src;
            return v.CopyAsSubVecß(newSup, inx, xCap); } }
      else
         return null;
   }

   
   /// <summary>No null checks.</summary>
   /// <param name="t">Source tensor that can be R1.</param>
   /// <param name="newSup">Copy's new superior.</param>
   /// <param name="inx">Copy's index inside new superior.</param>
   /// <param name="xCap">Extra capacity of all copied (sub)tensors (beyond existing Count).</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   internal static Tnr<τ,α> CopyAsSubTnrβ<τ,α>(this Tnr<τ,α> t, Tnr<τ,α> newSup,
   int inx, int xCap = 0)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() {
      Assume.True(t.Rank > 1, () => "Passed source tensor has to be at least rank 2.");
      Assume.True(t.Dim == newSup.Strc[newSup.StrcInx + 1], () =>
         "Source tensor's dimension does not equal the dimension of slot where copied subtensor will be placed.");
      
      if(t is Vec<τ,α> v)
         return v.CopyAsSubVecß(newSup, inx, xCap);
      else {
         var nst = newSup.SubTensor<τ,α>(inx, t.Count + xCap);
         if(t.Rank == 2) {
            foreach(var (i, st) in t) {
            var sv = (Vec<τ,α>) st;
            sv.CopyAsSubVecß<τ,α>(nst, i, xCap); } }
         else {
            foreach (var (i, st) in t)
            st.CopyAsSubTnrβ(nst, i, xCap); }
         return nst; }
   }

}
}