using System;
using System.Collections.Generic;
using System.Linq;

using Fluid.Internals.Algebras;
namespace Fluid.Internals.Tensors {

/// <summary>A type responsible for construction.</summary>
public static class TnrFactory {
   /// <summary>Creates a top vector (null superior) with specified dimension and initial capacity.</summary>
   /// <param name="dim">Dimension.</param>
   /// <param name="cap">Initial capacity.</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static Vec<τ,α> TopVec<τ,α>(int dim, int cap = 6)  where τ : notnull, IEquatable<τ>, IComparable<τ>  where α : IAlgebra<τ>, new() =>
      new Vec<τ,α>(dim, cap);
      
   /// <summary>Infers dimension and capacity from specified vector. Creates a top vector (null superior).</summary>
   /// <param name="v">New vector will take on same dimension and capacity as this one.</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static Vec<τ,α> TopVec<τ,α>(Vec<τ,α> v)  where τ : notnull, IEquatable<τ>, IComparable<τ>  where α : IAlgebra<τ>, new() =>
      new Vec<τ,α>(v.Dim, v.Count);
   
   /// <summary>Creates a non-top vector (non-null superior) with specified initial capacity. Adds it to its specified superior at the specified index. Dimension is inferred from superior's structure.</summary>
   /// <param name="sup">Direct superior.</param>
   /// <param name="inx">Index inside superior.</param>
   /// <param name="cap">Initial capacity.</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static Vec<τ,α> SubVec<τ,α>(this Tnr<τ,α>? sup, int inx, int cap = 6)  where τ : notnull, IEquatable<τ>, IComparable<τ>  where α : IAlgebra<τ>, new()
      => sup != null ? SubVecβ(sup, inx, cap)
         : throw new ArgumentNullException(nameof(sup), "Specified superior was null.");

   /// <summary>No null checks.</summary>
   /// <param name="sup">Direct superior.</param>
   /// <param name="inx">Index inside superior.</param>
   /// <param name="cap">Initial capacity.</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   internal static Vec<τ,α> SubVecβ<τ,α>(this Tnr<τ,α> sup, int inx, int cap = 6)  where τ : notnull, IEquatable<τ>, IComparable<τ>  where α : IAlgebra<τ>, new() {
      Assume.True(sup.Rank == 2, ()
         => "Vector's superior rank not 2. You can only create a subvec with a rank 2 superior.");
      var vec = new Vec<τ,α>(sup, cap);
      sup[Vec<τ,α>.V, inx] = vec;
      return vec;
   }

   /// <summary>Creates a top vector from an array span.</summary>
   /// <param name="span">Array span of values.</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static Vec<τ,α>? TopVecFromSpan<τ,α>(Span<τ> span)  where τ : notnull, IEquatable<τ>, IComparable<τ>  where α : IAlgebra<τ>, new() {
      α alg = new α();
      var vec = new Vec<τ,α>(span.Length, span.Length);
      for(int i = 0; i < span.Length; ++i) {
         if(!alg.IsZero(span[i]))
            vec.Add(i, span[i]); }
      return vec.Count != 0 ? vec : null;
   }
   /// <summary>Creates a non-top vector from an array span. Adds it to its specified superior at the specified index.</summary>
   /// <param name="span">Array span of values.</param>
   /// <param name="sup">Direct superior with an existing structure.</param>
   /// <param name="inx">Index inside superior.</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static Vec<τ,α>? SubVecFromSpan<τ,α>(Span<τ> span, Tnr<τ,α> sup, int inx)  where τ : notnull, IEquatable<τ>, IComparable<τ>  where α : IAlgebra<τ>, new() {
      Assume.True(sup.Rank == 2, () => "Vector's superior rank not 2. You can only create a subvector with a rank 2 superior.");
      α alg = new α();
      var vec = new Vec<τ,α>(sup, span.Length);
      for(int i = 0; i < span.Length; ++i) {
         if(!alg.IsZero(span[i]))
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
   public static Tnr<τ,α> TopTnr<τ,α>(List<int> strc, int cap = 6)  where τ : notnull, IEquatable<τ>, IComparable<τ>  where α : IAlgebra<τ>, new() {
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
   public static Tnr<τ,α> SubTnr<τ,α>(this Tnr<τ,α>? sup, int inx, int cap = 6)  where τ : notnull, IEquatable<τ>, IComparable<τ>  where α : IAlgebra<τ>, new()
      => sup != null ? SubTnrβ(sup, inx, cap)
         : throw new ArgumentNullException(nameof(sup), "Specified superior was null.");

   /// <summary>No null checks.</summary>
   /// <param name="sup">Direct superior.</param>
   /// <param name="inx">Index inside superior.</param>
   /// <param name="cap">Capacity of internal dictionary.</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   internal static Tnr<τ,α> SubTnrβ<τ,α>(this Tnr<τ,α> sup, int inx, int cap = 6)  where τ : notnull, IEquatable<τ>, IComparable<τ>  where α : IAlgebra<τ>, new() {
      Assume.True(sup.Rank > 1, ()
         => "Superior's rank too low. Cannot create a subtensor on a vector.");
      if(sup.Rank > 2) {
         var tnr = new Tnr<τ,α>(sup, cap);
         sup[Tnr<τ,α>.T, inx] = tnr;
         return tnr; }
      else
         return sup.SubVecβ(inx, cap);
   }

   /// <summary>Creates a top tensor from an array span.</summary>
   /// <param name="span">Array span of values.</param>
   /// <param name="strc">Structure.</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static Tnr<τ,α>? TopTnrFromSpan<τ,α>(Span<τ> span, params int[] strc)  where τ : notnull, IEquatable<τ>, IComparable<τ>  where α : IAlgebra<τ>, new() {
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
   /// <param name="v">Copy source.</param>
   /// <param name="xCap">Extra capacity of copied vector (beyond existing Count).</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static Vec<τ,α>? CopyAsTopVec<τ,α>(this Vec<τ,α>? v, int xCap = 0)  where τ : notnull, IEquatable<τ>, IComparable<τ>  where α : IAlgebra<τ>, new()
      => v != null ? v.CopyAsTopVecβ(xCap) : null;
   
   /// <summary>No null checks.</summary>
   /// <param name="v">Copy source.</param>
   /// <param name="xCap">Extra capacity of copied vector (beyond existing Count).</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   internal static Vec<τ,α> CopyAsTopVecβ<τ,α>(this Vec<τ,α> v, int xCap = 0)  where τ : notnull, IEquatable<τ>, IComparable<τ>  where α : IAlgebra<τ>, new() {
      var copy = TopVec<τ,α>(v.Dim, v.Count + xCap);
      foreach(var (i, s) in v.Scals)
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
   public static Vec<τ,α>? CopyAsSubVec<τ,α>(this Vec<τ,α>? v, Tnr<τ,α> newSup,
   int inx, int xCap = 0)  where τ : notnull, IEquatable<τ>, IComparable<τ>  where α : IAlgebra<τ>, new() 
      => v != null ? v.CopyAsSubVecβ(newSup, inx, xCap) : null;

   /// <summary>No null checks.</summary>
   /// <param name="v">Copy source.</param>
   /// <param name="newSup">The copied vector's superior.</param>
   /// <param name="inx">New index inside superior.</param>
   /// <param name="xCap">Extra capacity of copied vector (beyond the number of elements).</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   internal static Vec<τ,α> CopyAsSubVecβ<τ,α>(this Vec<τ,α> v, Tnr<τ,α> newSup,
   int inx, int xCap = 0)  where τ : notnull, IEquatable<τ>, IComparable<τ>  where α : IAlgebra<τ>, new() {
      Assume.True(v.Dim == newSup.Strc[newSup.StrcInx + 1], ()
         => "Source vector's dimension does not equal the dimension of slot where copied subvector will be placed.");
      var copy = SubVec<τ,α>(newSup, inx, v.Count + xCap);            // New superior's rank will be checked here.
      foreach(var (i, s) in v.Scals)
         copy.Add(i, s);
      return copy;
   }

   /// <summary>Creates a deep copy of a tensor as a top tensor (null superior).</summary>
   /// <param name="t">Source tensor (can be R1).</param>
   /// <param name="xCap">Extra capacity of all copied (sub)tensors (beyond existing Count).</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static Tnr<τ,α>? CopyAsTopTnr<τ,α>(this Tnr<τ,α>? t, int xCap = 0)  where τ : notnull, IEquatable<τ>, IComparable<τ>  where α : IAlgebra<τ>, new()
      => t != null ? t.CopyAsTopTnrβ(xCap) : null;
   
   /// <summary>No null checks.</summary>
   /// <param name="t">Source tensor (can be R1).</param>
   /// <param name="xCap">Extra capacity of all copied (sub)tensors (beyond existing Count).</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   internal static Tnr<τ,α> CopyAsTopTnrβ<τ,α>(this Tnr<τ,α> t, int xCap = 0)  where τ : notnull, IEquatable<τ>, IComparable<τ>  where α : IAlgebra<τ>, new() {
      if(t is Vec<τ,α> v)
         return v.CopyAsTopVecβ(xCap);
      else {
         var newStrc = t.Substrc;
         var copy = TopTnr<τ,α>(newStrc, t.Count + xCap);
         foreach(var (i, st) in t)
            st.CopyAsSubTnrβ<τ,α>(copy, i, xCap);
         return copy; }
   }

   /// <summary>Creates a deep copy of a tensor as a non-top tensor (non-null superior).</summary>
   /// <param name="t">Copy source.</param>
   /// <param name="newSup">The copied tensor's superior.</param>
   /// <param name="inx">New index inside superior.</param>
   /// <param name="xCap">Extra capacity of all copied (sub)tensors (beyond existing Count).</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   public static Tnr<τ,α>? CopyAsSubTnr<τ,α>(this Tnr<τ,α>? t, Tnr<τ,α> newSup,
   int inx, int xCap = 0)  where τ : notnull, IEquatable<τ>, IComparable<τ> where α : IAlgebra<τ>, new()
      => t != null ? t.CopyAsSubTnrβ(newSup, inx, xCap) : null;

   
   /// <summary>No null checks.</summary>
   /// <param name="t">Source tensor (can be R1).</param>
   /// <param name="newSup">Copy's new superior.</param>
   /// <param name="inx">Copy's index inside new superior.</param>
   /// <param name="xCap">Extra capacity of all copied (sub)tensors (beyond existing Count).</param>
   /// <typeparam name="τ">Numeric type.</typeparam>
   /// <typeparam name="α">Arithmetic type.</typeparam>
   internal static Tnr<τ,α> CopyAsSubTnrβ<τ,α>(this Tnr<τ,α> t, Tnr<τ,α> newSup,
   int inx, int xCap = 0)  where τ : notnull, IEquatable<τ>, IComparable<τ>  where α : IAlgebra<τ>, new() {
      Assume.True(t.Dim == newSup.Strc[newSup.StrcInx + 1], ()
         => "Source tensor's dimension does not equal the dimension of slot where copied subtensor will be placed.");
      if(t is Vec<τ,α> v)
         return v.CopyAsSubVecβ(newSup, inx, xCap);
      else {
         var nst = newSup.SubTnr<τ,α>(inx, t.Count + xCap);
         if(t.Rank == 2) {
            foreach(var (i, st) in t) {
            var sv = (Vec<τ,α>) st;
            sv.CopyAsSubVecβ<τ,α>(nst, i, xCap); } }
         else {
            foreach (var (i, st) in t)
            st.CopyAsSubTnrβ(nst, i, xCap); }
         return nst; }
   }

}
}