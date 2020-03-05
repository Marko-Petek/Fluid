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
   public static RefVec<τ,α> TopRefVec<τ,α>(int dim, int cap = 6)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() =>
      new RefVec<τ,α>(dim, cap);
   /// <summary>Creates a top vector (null superior) with dimension and capacity inferred from a specified vector.</summary>
   /// <param name="vec">New vector will take on same dimension and capacity as this one.</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static RefVec<τ,α> TopRefVec<τ,α>(RefVec<τ,α> vec)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() =>
      new RefVec<τ,α>(vec.Dim, vec.Count);
   
   /// <summary>Creates a non-top vector (non-null superior) with specified initial capacity. Adds it to its specified superior at the specified index. Dimension is inferred from superior's structure.</summary>
   /// <param name="sup">Direct superior.</param>
   /// <param name="inx">Index inside superior.</param>
   /// <param name="cap">Initial capacity.</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static RefVec<τ,α> SubRefVec<τ,α>(this RefTnr<τ,α> sup, int inx, int cap = 6)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      Assume.True(sup.Rank == 2, () => "Vector's superior rank not 2. You can only create a subvector with a rank 2 superior.");
      return SubRefVecß(sup, inx, cap);
   }

   internal static RefVec<τ,α> SubRefVecß<τ,α>(this RefTnr<τ,α> sup, int inx, int cap = 6)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      var vec = new RefVec<τ,α>(sup, cap);
      sup[RefVec<τ,α>.V, inx] = vec;
      return vec;
   }

   /// <summary>Creates a top vector from an array span.</summary>
   /// <param name="span">Array span of values.</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static RefVec<τ,α>? TopRefVecFromSpan<τ,α>(Span<τ> span)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      var vec = new RefVec<τ,α>(span.Length, span.Length);
      for(int i = 0; i < span.Length; ++i) {
         if(!span[i].Equals(Nullable<τ,α>.O.Zero()))
            vec.Add(i, span[i]); }
      return vec.Count != 0 ? vec : null;
   }
   /// <summary>Creates a non-top vector from an array span. Adds it to its specified superior at the specified index.</summary>
   /// <param name="span">Array span of values.</param>
   /// <param name="sup">Direct superior with an existing structure.</param>
   /// <param name="inx">Index inside superior.</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static RefVec<τ,α>? SubRefVecFromSpan<τ,α>(Span<τ> span, RefTnr<τ,α> sup, int inx)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
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
   public static RefTnr<τ,α> TopRefTnr<τ,α>(List<int> strc, int cap = 6)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
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
   public static RefTnr<τ,α> SubRefTnr<τ,α>(this RefTnr<τ,α> sup, int inx, int cap = 6)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      Assume.True(sup.Rank > 1, () =>
         "Superior's rank too low. Cannot create a subtensor on a vector.");
      return SubRefTnrß(sup, inx, cap);
   }

   internal static RefTnr<τ,α> SubRefTnrß<τ,α>(this RefTnr<τ,α> sup, int inx, int cap = 6)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      if(sup.Rank > 2) {
         var tnr = new RefTnr<τ,α>(sup, cap);
         sup[RefTnr<τ,α>.T, inx] = tnr;
         return tnr; }
      else
         return sup.SubRefVecß(inx, cap);
   }

   /// <summary>Creates a top tensor from an array span.</summary>
   /// <param name="span">Array span of values.</param>
   /// <param name="strc">Structure.</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static RefTnr<τ,α>? TopRefTnrFromSpan<τ,α>(Span<τ> span, params int[] strc)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      int rank = strc.Length;
      if(rank == 1)
         return TopRefVecFromSpan<τ,α>(span);
      else {
         var res = new RefTnr<τ,α>(strc.ToList(), strc[0]);                         // Empty tensor that enters recursion.
         Recursion(span, 0, res);
         return res.Count != 0 ? res : null; }

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
               var subVec = SubRefVecFromSpan<τ,α>(newSlc, tgt, i); } } }
   }
   /// <summary>Creates a deep copy of a vector as a top vector (null superior).</summary>
   /// <param name="src">Copy source.</param>
   /// <param name="extraCap">Extra capacity of copied vector (beyond existing Count).</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static RefVec<τ,α>? CopyAsTopRefVec<τ,α>(this RefVec<τ,α>? src, int extraCap = 0)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() =>
      src != null ? src.CopyAsTopRefVecß(extraCap) : null;

   internal static RefVec<τ,α> CopyAsTopRefVecß<τ,α>(this RefVec<τ,α> src, int extraCap = 0)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
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
   public static RefVec<τ,α>? CopyAsSubRefVec<τ,α>(this RefVec<τ,α>? src, RefTnr<τ,α> newSup,
   int inx, int xCap = 0)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      if(src != null) {
         return src.CopyAsSubRefVecß(newSup, inx, xCap); }
      else
         return null;
   }

   /// <summary>Rank </summary>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   internal static RefVec<τ,α> CopyAsSubRefVecß<τ,α>(this RefVec<τ,α> src, RefTnr<τ,α> newSup,
   int inx, int xCap = 0)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      Assume.True(src.Dim == newSup.Strc[newSup.StrcInx + 1], () => "Source vector's dimension does not equal the dimension of slot where copied subvector will be placed.");
      var copy = SubRefVec<τ,α>(newSup, inx, src.Count + xCap);         // New superior's rank will be checked here.
      foreach(var (i, s) in src.Scals)
         copy.Add(i, s);
      return copy;
   }

   /// <summary>Creates a deep copy of a tensor as a top tensor (null superior).</summary>
   /// <param name="src">Copy source.</param>
   /// <param name="xCap">Extra capacity of all copied (sub)tensors (beyond existing Count).</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static RefTnr<τ,α>? CopyAsTopRefTnr<τ,α>(this RefTnr<τ,α>? src, int xCap = 0)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      if(src != null) {
         if(src.Rank > 1)
            return src.CopyAsTopRefTnrß(xCap);
         else {
            var v = (RefVec<τ,α>) src;
            return v.CopyAsTopRefVecß(xCap); } }
      else
         return null;
   }

   internal static RefTnr<τ,α> CopyAsTopRefTnrß<τ,α>(this RefTnr<τ,α> src, int xCap = 0)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      Assume.True(src.Rank > 1, () => "Passed source tensor has to be at least rank 2.");
      var newStrc = new List<int>(src.Strc);
      var copy = TopRefTnr<τ,α>(newStrc, src.Count + xCap);
      foreach(var (i, t) in src)
         t.CopyAsSubRefTnr<τ,α>(copy, i, xCap);
      return copy;
   }
   /// <summary>Creates a deep copy of a tensor as a non-top tensor (non-null superior).</summary>
   /// <param name="src">Copy source.</param>
   /// <param name="newSup">The copied tensor's superior.</param>
   /// <param name="inx">New index inside superior.</param>
   /// <param name="xCap">Extra capacity of all copied (sub)tensors (beyond existing Count).</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static RefTnr<τ,α>? CopyAsSubRefTnr<τ,α>(this RefTnr<τ,α>? src, RefTnr<τ,α> newSup,
   int inx, int xCap = 0)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      if(src != null) {
         if(src.Rank > 1)
            return src.CopyAsSubRefTnrß(newSup, inx, xCap);
         else {
            var v = (RefVec<τ,α>) src;
            return v.CopyAsSubRefVecß(newSup, inx, xCap); } }
      else
         return null;
   }
   
   /// <summary>Only use this when src is at least R2.</summary>
   /// <param name="src">Copy source that is at least R2.</param>
   /// <param name="newSup">The copied tensor's superior.</param>
   /// <param name="inx">New index inside superior.</param>
   /// <param name="xCap">Extra capacity of all copied (sub)tensors (beyond existing Count).</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   internal static RefTnr<τ,α> CopyAsSubRefTnrß<τ,α>(this RefTnr<τ,α> src,
   RefTnr<τ,α> newSup, int inx, int xCap = 0)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      Assume.True(src.Rank > 1, () => "Passed source tensor has to be at least rank 2.");
      Assume.True(src.Dim == newSup.Strc[newSup.StrcInx + 1], () => "Source tensor's dimension does not equal the dimension of slot where copied subtensor will be placed.");
      var copy = newSup.SubRefTnr<τ,α>(inx, src.Count + xCap);
      if(src.Rank > 2) {                                                        // Subordinates are tensors.
         foreach (var (i, t) in src)
            CopyAsSubRefTnrß(t, copy, i, xCap); }
      else if(src.Rank == 2) {                                                                     // Subordinates are vectors.
         foreach(var (i, t) in src) {
            var v = (RefVec<τ,α>) t;
            v.CopyAsSubRefVecß<τ,α>(copy, i, xCap); } }
      return copy; }
}

}