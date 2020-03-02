using System;
using System.Collections.Generic;
using System.Linq;
using Fluid.Internals.Numerics;
namespace Fluid.Internals.Collections {

/// <summary>A type responsible for construction.</summary>
public static class RefTnrFactory {
   /// <summary>Creates a top vector (null superior) with specified dimension and initial capacity.</summary>
   /// <param name="dim">Dimension.</param>
   /// <param name="cap">Initial capacity.</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static RefVec<τ,α> TopRefVec<τ,α>(int dim, int cap = 6)
   where τ : class, IEquatable<τ>, IComparable<τ>
   where α : IArithmetic<τ?>, new() =>
      new RefVec<τ,α>(dim, cap);
   /// <summary>Creates a top vector (null superior) with dimension and capacity inferred from a specified vector.</summary>
   /// <param name="vec">New vector will take on same dimension and capacity as this one.</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static RefVec<τ,α> TopRefVec<τ,α>(RefVec<τ,α> vec)
   where τ : class, IEquatable<τ>, IComparable<τ>
   where α : IArithmetic<τ?>, new() =>
      new RefVec<τ,α>(vec.Dim, vec.Count);
   
   /// <summary>Creates a non-top vector (non-null superior) with specified initial capacity. Adds it to its specified superior at the specified index. Dimension is inferred from superior's structure.</summary>
   /// <param name="sup">Direct superior.</param>
   /// <param name="inx">Index inside superior.</param>
   /// <param name="cap">Initial capacity.</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static RefVec<τ,α> SubRefVec<τ,α>(this RefTnr<τ,α> sup, int inx, int cap = 6)
   where τ : class, IEquatable<τ>, IComparable<τ>
   where α : IArithmetic<τ?>, new() {
      Assume.True(sup.Rank == 2, () => "Vector's superior rank not 2. You can only create a subvector with a rank 2 superior.");
      return SubRefVecß(sup, inx, cap);
   }

   internal static RefVec<τ,α> SubRefVecß<τ,α>(this RefTnr<τ,α> sup, int inx, int cap = 6)
   where τ : class, IEquatable<τ>, IComparable<τ>
   where α : IArithmetic<τ?>, new() {
      var vec = new RefVec<τ,α>(sup, cap);
      sup[RefVec<τ,α>.V, inx] = vec;
      return vec;
   }

   /// <summary>Creates a top vector from an array span.</summary>
   /// <param name="span">Array span of values.</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static RefVec<τ,α> TopRefVecFromSpan<τ,α>(Span<τ> span)
   where τ : class, IEquatable<τ>, IComparable<τ>
   where α : IArithmetic<τ?>, new() {
      var vec = new RefVec<τ,α>(span.Length, span.Length);
      for(int i = 0; i < span.Length; ++i) {
         if(!span[i].Equals(Nullable<τ,α>.O.Zero()))
            vec.Add(i, span[i]); }
      return vec;
   }
   /// <summary>Creates a non-top vector from an array span. Adds it to its specified superior at the specified index.</summary>
   /// <param name="span">Array span of values.</param>
   /// <param name="sup">Direct superior with an existing structure.</param>
   /// <param name="inx">Index inside superior.</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static RefVec<τ,α>? SubVecFromSpan<τ,α>(Span<τ> span, RefTnr<τ,α> sup, int inx)
   where τ : class, IEquatable<τ>, IComparable<τ>
   where α : IArithmetic<τ?>, new() {
      Assume.True(sup.Rank == 2, () => "Vector's superior rank not 2. You can only create a subvector with a rank 2 superior.");
      var vec = new RefVec<τ,α>(sup, span.Length);
      for(int i = 0; i < span.Length; ++i) {
         if(!span[i].Equals(Nullable<τ,α>.O.Zero()))
            vec.Add(i, span[i]); }
      if(vec.Count > 0) {                                                           // Created vector is not empty.
         sup[RefVec<τ,α>.V, inx] = vec;
         return vec; }
      else
         return null;
   }
   /// <summary>Creates a top tensor (null superior) with specified structure and initial capacity. Rank is assigned as the length of structure array.</summary>
   /// <param name="strc">Specifies dimension of each rank.</param>
   /// <param name="cap">Initially assigned memory.</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static RefTnr<τ,α> TopRefTnr<τ,α>(List<int> strc, int cap = 6)
   where τ : class, IEquatable<τ>, IComparable<τ>
   where α : IArithmetic<τ?>, new() {
      Assume.True(strc.Count > 1, () =>
         "For creating tensors of rank 1 use Vector's constructor.");
      return new RefTnr<τ,α>(strc, cap);
   }
   
   /// <summary>Creates a non-top tensor (non-null superior) and adds it to its specified superior at the specified index. Assumes superior's structure is initialized.</summary>
   /// <param name="sup">Direct superior.</param>
   /// <param name="inx">Index inside superior.</param>
   /// <param name="cap">Capacity of internal dictionary.</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static RefTnr<τ,α> SubTensor<τ,α>(this RefTnr<τ,α> sup, int inx, int cap = 6)
   where τ : class, IEquatable<τ>, IComparable<τ>
   where α : IArithmetic<τ?>, new() {
      Assume.True(sup.Rank > 1, () =>
         "Superior's rank too low. Cannot create a subtensor on a vector.");
      return SubRefTnrß(sup, inx, cap);
   }

   internal static RefTnr<τ,α> SubRefTnrß<τ,α>(this RefTnr<τ,α> sup, int inx, int cap = 6)
   where τ : class, IEquatable<τ>, IComparable<τ>
   where α : IArithmetic<τ?>, new() {
      if(sup.Rank > 2) {
         var tnr = new RefTnr<τ,α>(sup, cap);
         sup[RefTnr<τ,α>.T, inx] = tnr;
         return tnr; }
      else
         return SubRefVecß(sup, inx, cap);
   }

   /// <summary>Creates a top tensor from an array span.</summary>
   /// <param name="span">Array span of values.</param>
   /// <param name="strc">Structure.</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static RefTnr<τ,α> TopTnrFromSpan<τ,α>(Span<τ> span, params int[] strc)
   where τ : class, IEquatable<τ>, IComparable<τ>
   where α : IArithmetic<τ?>, new() {
      int rank = strc.Length;
      if(rank == 1)
         return TopRefVecFromSpan<τ,α>(span);
      else {
         var res = new RefTnr<τ,α>(strc.ToList(), strc[0]);                         // Empty tensor that enters recursion.
         Recursion(span, 0, res);
         return res; }

      void Recursion(Span<τ> spn, int slot, RefTnr<τ,α> tgt) {                      // Span and natural slot index to which it belongs.
         int nIter = strc[slot];                                                    // As many iterations as slot dimension.
         int nEmtsInSpan = spn.Length / nIter;
         if(tgt.Rank > 2) {
            for(int i = 0; i < nIter; ++i) {                                        // Over each tensor. Create new spans and run recursion on them.
               var newSpn = spn.Slice(i*nEmtsInSpan, nEmtsInSpan);
               var subTnr = new RefTnr<τ,α>(tgt, strc[slot]);
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
   public static RefVec<τ,α> CopyAsTopVec<τ,α>(this RefVec<τ,α> src, int extraCap = 0)
   where τ : class, IEquatable<τ>, IComparable<τ>
   where α : IArithmetic<τ?>, new() {
      var copy = TopRefVec<τ,α>(src.Dim, src.Count + extraCap);
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
   public static RefVec<τ,α> CopyAsSubVec<τ,α>(this RefVec<τ,α> src, RefTnr<τ,α> newSup,
   int inx, int xCap = 0)
   where τ : class, IEquatable<τ>, IComparable<τ>
   where α : IArithmetic<τ?>, new() {
      Assume.True(src.Dim == newSup.Dim - 1, () => "The dimension as specified by the superior does not equal the original vector's dimension.");
      return src.CopyAsSubVecß(newSup, inx, xCap);
   }

   internal static RefVec<τ,α> CopyAsSubVecß<τ,α>(this RefVec<τ,α> src, RefTnr<τ,α> newSup,
   int inx, int xCap = 0)
   where τ : class, IEquatable<τ>, IComparable<τ>
   where α : IArithmetic<τ?>, new() {
      var copy = SubRefVec<τ,α>(newSup, inx, src.Count + xCap);
      foreach(var (i, s) in src.Scals)
         copy.Add(i, s);
      return copy;
   }

   /// <summary>Creates a deep copy of a tensor as a top tensor (null superior).</summary>
   /// <param name="src">Copy source.</param>
   /// <param name="xCap">Extra capacity of all copied (sub)tensors (beyond existing Count).</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static RefTnr<τ,α> CopyAsTopTnr<τ,α>(this RefTnr<τ,α> src, int xCap = 0)
   where τ : class, IEquatable<τ>, IComparable<τ>
   where α : IArithmetic<τ?>, new() {
      if (src is RefVec<τ,α> vec) {
         return CopyAsTopVec<τ,α>(vec, xCap); }
      else {
         var newStrc = new List<int>(src.Strc);
         var copy = TopRefTnr<τ,α>(newStrc, src.Count + xCap);
         foreach(var (i, t) in src)
            CopyAsSubTnr<τ,α>(t, copy, i, xCap);
         return copy; }
   }
   /// <summary>Creates a deep copy of a tensor as a non-top tensor (non-null superior).</summary>
   /// <param name="src">Copy source.</param>
   /// <param name="newSup">The copied tensor's superior.</param>
   /// <param name="inx">New index inside superior.</param>
   /// <param name="xCap">Extra capacity of all copied (sub)tensors (beyond existing Count).</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static RefTnr<τ,α> CopyAsSubTnr<τ,α>(this RefTnr<τ,α> src, RefTnr<τ,α> newSup,
   int inx, int xCap = 0)
   where τ : class, IEquatable<τ>, IComparable<τ>
   where α : IArithmetic<τ?>, new() {
      if (src is RefVec<τ,α> vec) {
         return CopyAsSubVec<τ,α>(vec, newSup, inx, xCap); }
      else {
         return CopyAsSubTnrß(src, newSup, inx, xCap); }
   }

   internal static RefTnr<τ,α> CopyAsSubTnrß<τ,α>(this RefTnr<τ,α> src,
   RefTnr<τ,α> newSup, int inx, int xCap = 0)
   where τ : class, IEquatable<τ>, IComparable<τ>
   where α : IArithmetic<τ?>, new() {
      var copy = SubTensor<τ,α>(newSup, inx, src.Count + xCap);               // TODO: Check that count is not 0 anywhere.
      if(src.Rank > 2) {                                                        // Subordinates are tensors.
         foreach (var (i, t) in src)
            CopyAsSubTnrß(t, copy, i, xCap); }
      else {                                                                     // Subordinates are vectors.
         foreach(var (i, t) in src) {
            var v = (RefVec<τ,α>) t;
            CopyAsSubVec<τ,α>(v, copy, i, xCap); } }
      return copy; }
}

}