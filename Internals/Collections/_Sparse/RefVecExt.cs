using System;
using System.Collections.Generic;
using System.Linq;
using Fluid.Internals.Numerics;
using static Fluid.Internals.Collections.RefTnrFactory;

namespace Fluid.Internals.Collections {
public static class RefVecExt {
   /// <summary>Sums vec2 to vec1. Modifies vec1, does not destroy vec2. If vec1 is null, it creates a copy of vec2.</summary>
   /// <param name="vec1">Sumand 1 is modified. Use return as result.</param>
   /// <param name="vec2">Sumand 2. Is not destroyed.</param>
   /// <remarks><see cref="TestRefs.VectorSum"/></remarks>
   public static RefVec<τ,α>? SumInto<τ,α>(this RefVec<τ,α>? vec1, RefVec<τ,α>? vec2)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      if(vec2 == null)
         return vec1;
      if(vec1 == null)
         return vec2.CopyAsTopVec();
      Assume.True(vec1.Dim == vec2.Dim, () => "The dimensions of vectors do not match on addition.");
      return SumIntoß(vec1, vec2);
   }
   /// <summary>Sums vec2 to vec1, both assumed to be non-null. Modifies vec1, does not destroy vec2.</summary>
   /// <param name="vec1">Sumand 1 is modified. Use return as result.</param>
   /// <param name="vec2">Sumand 2. Is not destroyed.</param>
   /// <remarks><see cref="TestRefs.VectorSum"/></remarks>
   internal static RefVec<τ,α>? SumIntoß<τ,α>(this RefVec<τ,α> vec1, RefVec<τ,α> vec2)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      foreach(var (i, s2) in vec2.Scals) {
         var s1 = vec1[i];
         var sum = Nullable<τ,α>.O.Sum(s1, s2);
         vec1[i] = sum; }
      if(vec1.Count != 0)
         return vec1;
      else
         return null;
   }
   /// <summary>Subtracts vec2 from vec1. Modifies vec1, does not destroy vec2.</summary>
   /// <param name="v1">Minuend. Is modified, use return as result.</param>
   /// <param name="v2">Subtrahend. Is not destroyed.</param>
   /// <remarks><see cref="TestRefs.VectorSub"/></remarks>
   public static RefVec<τ,α>? SubInto<τ,α>(this RefVec<τ,α>? v1, RefVec<τ,α>? v2)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      if(v2 == null)
         return v1;
      if(v1 == null)
         v1 = TopRefVec<τ,α>(v2);
      Assume.True(v1.Dim == v2.Dim, () => "The dimensions of vectors do not match on subtraction.");
      return SubIntoß(v1, v2);
   }
   /// <summary>Subtracts vec2 from vec1. Modifies vec1, does not destroy vec2.</summary>
   /// <param name="vec1">Minuend. Is modified, use return as result.</param>
   /// <param name="vec2">Subtrahend. Is not destroyed.</param>
   /// <remarks><see cref="TestRefs.VectorSub"/></remarks>
   internal static RefVec<τ,α>? SubIntoß<τ,α>(this RefVec<τ,α> vec1, RefVec<τ,α> vec2)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      foreach(var (i, s2) in vec2.Scals) {
         var subVal1 = vec1[i];
         var dif = Nullable<τ,α>.O.Sub(subVal1, s2);
         vec1[i] = dif; }
      if(vec1.Count != 0)
         return vec1;
      else
         return null;
   }

   public static RefVec<τ,α>? SumTop<τ,α>(this RefVec<τ,α>? v1, RefVec<τ,α>? v2)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      if(v1 == null) {
         if(v2 == null)
            return null;
         else {
            var newVec = TopRefVec<τ,α>(v2);
            return newVec.SumIntoß(v2); } }
      else if(v2 == null) {
         return TopRefVec<τ,α>(v1).SumIntoß(v1); }
      Assume.True(v1.Dim == v2.Dim, () => "The dimensions of vectors do not match on addition.");
      return SumTopß(v1, v2);
   }

   internal static RefVec<τ,α>? SumTopß<τ,α>(this RefVec<τ,α> v1, RefVec<τ,α> v2)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() =>
      TopRefVec<τ,α>(v1).SumIntoß(v1)!.SumIntoß(v2);

   public static RefVec<τ,α>? SubTop<τ,α>(this RefVec<τ,α>? v1, RefVec<τ,α>? v2)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      if(v1 == null) {
         if(v2 == null)
            return null;
         else
            return TopRefVec<τ,α>(v2).SubIntoß(v2); }
      else if(v2 == null) {
         return TopRefVec<τ,α>(v1).SumIntoß(v1); }
      Assume.True(v1.Dim == v2.Dim, () => "The dimensions of vectors do not match on addition.");
      return SumTopß(v1, v2);
   }

   internal static RefVec<τ,α>? SubTopß<τ,α>(this RefVec<τ,α> v1, RefVec<τ,α> v2)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() =>
      TopRefVec<τ,α>(v1).SumIntoß(v1)!.SubIntoß(v2);
   
   /// <summary>Modifies this vector by negating each element.</summary>
   public static RefVec<τ,α>? NegateInto<τ,α>(this RefVec<τ,α>? v)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      if(v == null)
         return null;
      return NegateIntoß(v);
   }
   internal static RefVec<τ,α> NegateIntoß<τ,α>(this RefVec<τ,α> v)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      var keys = v.Scals.Keys.ToArray();                    // We have to do this (access via indexer), because we can't change collection during enumeration.
      for(int i = 0; i < keys.Length; ++i)
         v.Scals[keys[i]] = Nullable<τ,α>.O.Neg(v.Scals[keys[i]])!;        // If v entered as non-null it will not become null here.
      return v;
   }
   public static RefVec<τ,α>? Negate<τ,α>(this RefVec<τ,α>? vec)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      if(vec == null)
         return null;
      else
         return vec.Negateß();
   }
   public static RefVec<τ,α> Negateß<τ,α>(this RefVec<τ,α> v)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() =>
      TopRefVec<τ,α>(v).NegateIntoß();
      
   /// <summary>Multiplies vector with a scalar. Modifies "this". Use return as result.</summary>
   /// <param name="s">Scalar.</param>
   /// <remarks> <see cref="TestRefs.VectorMul"/> </remarks>
   public static RefVec<τ,α>? MulInto<τ,α>(this RefVec<τ,α>? v, τ? s)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      if(v == null || s == null)
         return null;
      else
         return MulIntoß(v, s);
   }

   internal static RefVec<τ,α> MulIntoß<τ,α>(this RefVec<τ,α> v, τ s)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      var inxs = v.Scals.Keys.ToArray();                       // To avoid "collection was modified during enumeration".
      foreach(var i in inxs)
         v.Scals[i] = Nullable<τ,α>.O.Mul(s, v.Scals[i])!;      // If vec and scal entered non-null, result here will not be null.
      return v;
   }
   /// <summary>Multiplies a scalar with a vector and returns a new vector. Safe version: accepts a nullable vector and checks it for null, also checks if the scalar is zero.</summary>
   /// <param name="s">Scalar.</param>
   /// <param name="v">Vector.</param>
   public static RefVec<τ,α>? MulTop<τ,α>(this RefVec<τ,α>? v, τ? s)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      if(v == null || s == null)
         return null;
      else
         return v.MulTopß(s);
   }
   /// <summary>Multiplies a scalar with a vector and returns a new vector. Unsafe version: accepts a non-nullable vector and does not check whether the scalar is zero.</summary>
   /// <param name="ns"></param>
   /// <param name="v"></param>
   internal static RefVec<τ,α> MulTopß<τ,α>(this RefVec<τ,α> v, τ ns)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      var nv = TopRefVec<τ,α>(v);
      foreach(var (i, s) in v) {
         nv.Add(i, Nullable<τ,α>.O.Mul(s, ns)!); }       // If vec and scal entered non-null, result here will not be null.
      return nv;
   }

   public static RefVec<τ,α>? MulSub<τ,α>(this RefVec<τ,α>? v, τ? ns, RefTnr<τ,α> sup, int inx)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      if(v == null || ns == null)
         return null;
      else
         return v.MulSubß(ns, sup, inx);
   }

   internal static RefVec<τ,α> MulSubß<τ,α>(this RefVec<τ,α> v, τ ns, RefTnr<τ,α> sup, int inx)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      var nv = sup.SubRefVec<τ,α>(inx);
      foreach(var (i, s) in v)
         nv.Add(i, Nullable<τ,α>.O.Mul(ns, s)!);   // If vec and scal entered non-null, result here will not be null.
      return nv;
   }
   /// <summary>Calculates Euclidean norm squared of a vector.</summary>
   public static τ? NormSqr<τ,α>(this RefVec<τ,α>? v)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      if(v == null)
         return null;
      else
         return v.NormSqrß();
   }

   internal static τ NormSqrß<τ,α>(this RefVec<τ,α> v)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      τ? res = null;
      foreach(var (i,s) in v.Scals)
         res = Nullable<τ,α>.O.Sum(res, Nullable<τ,α>.O.Mul(s, s));
      return res!;                                                   // If v entered as non-null, result here cannot be null.
   }


   /// <summary>Tensor product of a vector with another tensor. Returns top tensor (null superior) as result.</summary>
   /// <param name="t2">Right hand operand.</param>
   /// <remarks> <see cref="TestRefs.TensorProduct"/> </remarks>
   public static RefTnr<τ,α>? TnrProdTop<τ,α>(this RefVec<τ,α>? v1, RefTnr<τ,α>? t2)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      if(v1 == null || t2 == null)
         return null;
      return v1.TnrProdTopß(t2);
   }

   internal static RefTnr<τ,α> TnrProdTopß<τ,α>(this RefVec<τ,α> v1, RefTnr<τ,α> t2)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      if(t2.Rank > 1) {
         int newRank = v1.Rank + t2.Rank;
         var strc1 = v1.EnumSubstrc();
         var strc2 = t2.EnumSubstrc();
         var newStrc = strc1.Concat(strc2).ToList();
         // We must substitute this vector with a tensor whose elements are multiples of tnr2.
         var prod = TopRefTnr<τ,α>(newStrc, v1.Scals.Count);
         foreach(var (i, s1) in v1.Scals)
            t2.MulSubß(s1, prod, i);
         return prod; }
      else {
         var v2 = (RefVec<τ,α>) t2;
         return v1.TnrProdTopß(v2); }
   }

   public static RefTnr<τ,α>? TnrProdTop<τ,α>(this RefVec<τ,α>? v1, RefVec<τ,α>? v2)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      if(v1 == null || v2 == null)
         return null;
      return v1.TnrProdTopß(v2);
   }

   /// <remarks> <see cref="TestRefs.VectorTnrProductVector"/> </remarks>
   internal static RefTnr<τ,α> TnrProdTopß<τ,α>(this RefVec<τ,α> v1, RefVec<τ,α> v2)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      var newStrc = new List<int> {v1.Dim, v2.Dim};
      var prod = TopRefTnr<τ,α>(newStrc, v1.Scals.Count);
      foreach(var (i, s1) in v1.Scals)
         v2.MulSub(s1, prod, i);
      return prod;
   }

   public static RefTnr<τ,α>? ContractTopPart2<τ,α>(this RefVec<τ,α> v1, RefTnr<τ,α> t2, int rankInx2, List<int> strc, int conDim)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      if(t2.Rank > 2) {                                                  // Result is tensor.
         RefTnr<τ,α>? elimTnr2, sumand;
         var sum = TopRefTnr<τ,α>(strc);                                    // Set sum to a zero tensor.
         for(int i = 0; i < conDim; ++i) {
            elimTnr2 = t2.ReduceRankTop(rankInx2, i);
            if(v1.Scals.TryGetValue(i, out var s) && elimTnr2 != null) {
               sumand = elimTnr2.MulTopß(s);
               sum.SumIntoß(sumand); } }
         if(sum.Count != 0)
            return sum;
         else
            return null; }
      else if(t2.Rank == 2) {
         RefTnr<τ,α>? elimTnr2;
         RefVec<τ,α>? elimVec2, sumand;
         var sum = TopRefVec<τ,α>(strc[0]);
         for(int i = 0; i < conDim; ++i) {
            elimTnr2 = t2.ReduceRankTop(rankInx2, i);
            if(elimTnr2 != null) {
               elimVec2 = (RefVec<τ,α>?) t2.ReduceRankTop(rankInx2, i);
               if(v1.Scals.TryGetValue(i, out var s) && elimVec2 != null) {
                  sumand = elimVec2.MulTopß(s);
                  sum.SumIntoß(sumand); } } }
         if(sum.Count != 0)
            return sum;
         else
            return null; }
      else {                                                               // Result is scalar.
         throw new ArgumentException("Explicitly cast t2 to vector before using contract."); }
   }

   public static RefTnr<τ,α>? ContractTop<τ,α>(this RefVec<τ,α> v1, RefTnr<τ,α> t2, int slot2)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      (List<int> strc, _, int rankInx2, int conDim) = v1.ContractTopPart1(t2, 1, slot2);
      return ContractTopPart2(v1, t2, rankInx2, strc, conDim);
   }

   public static τ? ContractTop<τ,α>(this RefVec<τ,α>? v1, RefVec<τ,α>? v2)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      if(v1 == null || v2 == null)
         return null;
      else
         return v1.ContractTopß<τ,α>(v2);
   }

   public static τ? ContractTopß<τ,α>(this RefVec<τ,α> v1, RefVec<τ,α> v2)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      τ? res = null;
      foreach(var (i,s) in v1.Scals) {
         if(v2.Scals.TryGetValue(i, out var s2))
            res = Nullable<τ,α>.O.Sum(res, Nullable<τ,α>.O.Mul(s, s2)); }
      return res;                                                          // Result can be null if two vectors are perpendicular.
   }

   public static bool Equals<τ,α>(this RefVec<τ,α>? v1, RefVec<τ,α>? v2)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      if(v1 == null) {                                                            // If both are null, return true. If only one of them is null, return false.
         if(v2 == null)
            return true;
         else
            return false; }
      else if(v2 == null)
         return false;
      if(!v1.CompareSubstrcß(v2))                                         // If substructures mismatch, they are not equal.
         return false;
      if(!v1.Scals.Keys.OrderBy(key => key).SequenceEqual(v2.Scals.Keys.OrderBy(key => key)))    // Keys have to match.
         return false;
      foreach(var (i,s1) in v1.Scals) {
         τ? s2 = v2[i];                      // This can return null, but is correctly handled in the if below.
         if(!s1.Equals(s2))
            return false; }
      return true;
   }

   public static bool Equals<τ,α>(this RefVec<τ,α>? v1, RefVec<τ,α>? v2, τ eps)  where τ : class, IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ?>, new() {
      if(v1 == null) {                                                            // If both are null, return true. If only one of them is null, return false.
         if(v2 == null)
            return true;
         else
            return false; }
      else if(v2 == null)
         return false;
      if(!v1.CompareSubstrcß(v2))                                         // If substructures mismatch, they are not equal.
         return false;
      if(!v1.Scals.Keys.OrderBy(key => key).SequenceEqual(v2.Scals.Keys.OrderBy(key => key)))    // Keys have to match.
         return false;
      foreach(var (i,s1) in v1.Scals) {
         τ? s2 = v2[i];
         τ? delta = Nullable<τ,α>.O.Abs(Nullable<τ,α>.O.Sub(s1, s2));
         if( delta != null && delta.CompareTo(eps) > 0 )                   // Deviation is not within tolerance.
            return false; }
      return true;                                                              // All deviations within tolerance.
   }
}
}