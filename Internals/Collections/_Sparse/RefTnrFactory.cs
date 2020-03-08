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
      
   /// <summary>Infers dimension and capacity from specified vector. Creates a top vector (null superior).</summary>
   /// <param name="v">New vector will take on same dimension and capacity as this one.</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static RefVec<τ,α> TopRefVec<τ,α>(RefVec<τ,α> v)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() =>
      new RefVec<τ,α>(v.Dim, v.Count);
   
   /// <summary>Creates a non-top vector (non-null superior) with specified initial capacity. Adds it to its specified superior at the specified index. Dimension is inferred from superior's structure.</summary>
   /// <param name="sup">Direct superior.</param>
   /// <param name="inx">Index inside superior.</param>
   /// <param name="cap">Initial capacity.</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static RefVec<τ,α> SubRefVec<τ,α>(this RefTnr<τ,α>? sup, int inx, int cap = 6)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new()
      => sup != null ? SubRefVecβ(sup, inx, cap)
         : throw new ArgumentNullException(nameof(sup), "Specified superior was null.");

   /// <summary>No null checks.</summary>
   /// <param name="sup">Direct superior.</param>
   /// <param name="inx">Index inside superior.</param>
   /// <param name="cap">Initial capacity.</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   internal static RefVec<τ,α> SubRefVecβ<τ,α>(this RefTnr<τ,α> sup, int inx, int cap = 6)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      Assume.True(sup.Rank == 2, ()
         => "Vector's superior rank not 2. You can only create a subvec with a rank 2 superior.");
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
   public static RefTnr<τ,α> SubRefTnr<τ,α>(this RefTnr<τ,α>? sup, int inx, int cap = 6)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new()
      => sup != null ? SubRefTnrβ(sup, inx, cap)
         : throw new ArgumentNullException(nameof(sup), "Specified superior was null.");

   /// <summary>No null checks.</summary>
   /// <param name="sup">Direct superior.</param>
   /// <param name="inx">Index inside superior.</param>
   /// <param name="cap">Capacity of internal dictionary.</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   internal static RefTnr<τ,α> SubRefTnrβ<τ,α>(this RefTnr<τ,α> sup, int inx, int cap = 6)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      Assume.True(sup.Rank > 1, ()
         => "Superior's rank too low. Cannot create a subtensor on a vector.");
      if(sup.Rank > 2) {
         var tnr = new RefTnr<τ,α>(sup, cap);
         sup[RefTnr<τ,α>.T, inx] = tnr;
         return tnr; }
      else
         return sup.SubRefVecβ(inx, cap);
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
   /// <param name="v">Copy source.</param>
   /// <param name="extraCap">Extra capacity of copied vector (beyond existing Count).</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static RefVec<τ,α>? CopyAsTopRefVec<τ,α>(this RefVec<τ,α>? v, int extraCap = 0)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() =>
      v != null ? v.CopyAsTopRefVecβ(extraCap) : null;

   /// <summary>No null checks.</summary>
   /// <param name="v">Copy source.</param>
   /// <param name="xCap">Extra capacity of copied vector (beyond existing Count).</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   internal static RefVec<τ,α> CopyAsTopRefVecβ<τ,α>(this RefVec<τ,α> src, int extraCap = 0)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      var copy = TopRefVec<τ,α>(src.Dim, src.Count + extraCap);
      foreach(var (i, s) in src.Scals)
         copy.Add(i, s);
      return copy;
   }

   /// <summary>Creates a deep copy of a vector as a non-top vector (non-null superior).</summary>
   /// <param name="v">Copy source.</param>
   /// <param name="newSup">The copied vector's superior.</param>
   /// <param name="inx">New index inside superior.</param>
   /// <param name="xCap">Extra capacity of copied vector (beyond the number of elements).</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static RefVec<τ,α>? CopyAsSubRefVec<τ,α>(this RefVec<τ,α>? v, RefTnr<τ,α> newSup,
   int inx, int xCap = 0)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new()
      => v != null ? v.CopyAsSubRefVecβ(newSup, inx, xCap) : null;

   /// <summary>No null checks.</summary>
   /// <param name="v">Copy source.</param>
   /// <param name="newSup">The copied vector's superior.</param>
   /// <param name="inx">New index inside superior.</param>
   /// <param name="xCap">Extra capacity of copied vector (beyond the number of elements).</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   internal static RefVec<τ,α> CopyAsSubRefVecβ<τ,α>(this RefVec<τ,α> v, RefTnr<τ,α> newSup,
   int inx, int xCap = 0)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      Assume.True(v.Dim == newSup.Strc[newSup.StrcInx + 1], ()
         => "Source vector's dimension does not equal the dimension of slot where copied subvector will be placed.");
      var copy = SubRefVec<τ,α>(newSup, inx, v.Count + xCap);         // New superior's rank will be checked here.
      foreach(var (i, s) in v.Scals)
         copy.Add(i, s);
      return copy;
   }

   /// <summary>Creates a deep copy of a tensor as a top tensor (null superior).</summary>
   /// <param name="t">Source tensor (can be R1).</param>
   /// <param name="xCap">Extra capacity of all copied (sub)tensors (beyond existing Count).</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static RefTnr<τ,α>? CopyAsTopRefTnr<τ,α>(this RefTnr<τ,α>? t, int xCap = 0)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new()
      => t != null ? t.CopyAsTopRefTnrβ(xCap) : null;

   /// <summary>No null checks.</summary>
   /// <param name="t">Source tensor (can be R1).</param>
   /// <param name="xCap">Extra capacity of all copied (sub)tensors (beyond existing Count).</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   internal static RefTnr<τ,α> CopyAsTopRefTnrβ<τ,α>(this RefTnr<τ,α> t, int xCap = 0)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      if(t is RefVec<τ,α> v)
         return v.CopyAsTopRefVecβ(xCap);
      else {
         var newStrc = t.Substrc;
         var copy = TopRefTnr<τ,α>(newStrc, t.Count + xCap);
         foreach(var (i, st) in t)
            st.CopyAsSubRefTnrβ<τ,α>(copy, i, xCap);
         return copy; }
   }
   /// <summary>Creates a deep copy of a tensor as a non-top ref tensor (non-null superior).</summary>
   /// <param name="t">Copy source.</param>
   /// <param name="newSup">The copied tensor's superior.</param>
   /// <param name="inx">New index inside superior.</param>
   /// <param name="xCap">Extra capacity of all copied (sub)tensors (beyond existing Count).</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static RefTnr<τ,α>? CopyAsSubRefTnr<τ,α>(this RefTnr<τ,α>? t, RefTnr<τ,α> newSup,
   int inx, int xCap = 0)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new()
      => t != null ? t.CopyAsSubRefTnrβ(newSup, inx, xCap) : null;
   
   /// <summary>No null checks.</summary>
   /// <param name="t">Copy source that is at least R2.</param>
   /// <param name="newSup">The copied tensor's superior.</param>
   /// <param name="inx">New index inside superior.</param>
   /// <param name="xCap">Extra capacity of all copied (sub)tensors (beyond existing Count).</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   internal static RefTnr<τ,α> CopyAsSubRefTnrβ<τ,α>(this RefTnr<τ,α> t,
   RefTnr<τ,α> newSup, int inx, int xCap = 0)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      Assume.True(t.Dim == newSup.Strc[newSup.StrcInx + 1], ()
         => "Source tensor's dimension does not equal the dimension of slot where copied subtensor will be placed.");
      if(t is RefVec<τ,α> v)
         return v.CopyAsSubRefVecβ(newSup, inx, xCap);
      else {
         var nst = newSup.SubRefTnr<τ,α>(inx, t.Count + xCap);
         if(t.Rank == 2) {
            foreach(var (i, st) in t) {
            var sv = (RefVec<τ,α>) st;
            sv.CopyAsSubRefVecβ<τ,α>(nst, i, xCap); } }
         else {
            foreach (var (i, st) in t)
            st.CopyAsSubRefTnrβ(nst, i, xCap); }
         return nst; }
   }

}
}