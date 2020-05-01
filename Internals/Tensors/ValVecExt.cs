using System;
using System.Collections.Generic;
using System.Linq;
using Fluid.Internals.Algebras;
using static Fluid.Internals.Tensors.ValTnrFactory;

namespace Fluid.Internals.Tensors {
public static class ValVecExt {
   /// <summary>Sums vec2 to vec1. Modifies vec1, does not destroy vec2. If vec1 is null, it creates a copy of vec2.</summary>
   /// <param name="vec1">Sumand 1 is modified. Use return as result.</param>
   /// <param name="vec2">Sumand 2. Is not destroyed.</param>
   /// <remarks><see cref="TestRefs.VectorSum"/></remarks>
   public static ValVec<τ,α>? SumInto<τ,α>(this ValVec<τ,α>? vec1, ValVec<τ,α>? vec2)  where τ : struct, IEquatable<τ>, IComparable<τ>  where α : IAlgebra<τ>, new() {
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
   internal static ValVec<τ,α>? SumIntoß<τ,α>(this ValVec<τ,α> vec1, ValVec<τ,α> vec2)  where τ : struct, IEquatable<τ>, IComparable<τ>   where α : IAlgebra<τ>, new() {
      foreach(var (i, s2) in vec2.Scals) {
         var s1 = vec1[i];
         var sum = NonNullA<τ,α>.O.Sum(s1, s2);
         vec1[i] = sum; }
      if(vec1.Count != 0)
         return vec1;
      else
         return null;
   }
   /// <summary>Subtracts vec2 from vec1. Modifies vec1, does not destroy vec2.</summary>
   /// <param name="vec1">Minuend. Is modified, use return as result.</param>
   /// <param name="vec2">Subtrahend. Is not destroyed.</param>
   /// <remarks><see cref="TestRefs.VectorSub"/></remarks>
   public static ValVec<τ,α>? SubInto<τ,α>(this ValVec<τ,α>? vec1, ValVec<τ,α>? vec2)  where τ : struct, IEquatable<τ>, IComparable<τ>   where α : IAlgebra<τ>, new() {
      if(vec2 == null)
         return vec1;
      if(vec1 == null)
         vec1 = TopVec<τ,α>(vec2);
      Assume.True(vec1.Dim == vec2.Dim, () => "The dimensions of vectors do not match on subtraction.");
      return SubIntoß(vec1, vec2);
   }
   /// <summary>Subtracts vec2 from vec1. Modifies vec1, does not destroy vec2.</summary>
   /// <param name="v1">Minuend. Is modified, use return as result.</param>
   /// <param name="v2">Subtrahend. Is not destroyed.</param>
   /// <remarks><see cref="TestRefs.VectorSub"/></remarks>
   internal static ValVec<τ,α>? SubIntoß<τ,α>(this ValVec<τ,α> v1, ValVec<τ,α> v2)  where τ : struct, IEquatable<τ>, IComparable<τ>   where α : IAlgebra<τ>, new() {
      foreach(var (i, s2) in v2) {
         var subVal1 = v1[i];
         var dif = NonNullA<τ,α>.O.Sub(subVal1, s2);
         v1[i] = dif; }
      if(v1.Count != 0)
         return v1;
      else
         return null;
   }

   public static ValVec<τ,α>? SumTop<τ,α>(this ValVec<τ,α>? vec1, ValVec<τ,α>? vec2)  where τ : struct, IEquatable<τ>, IComparable<τ>   where α : IAlgebra<τ>, new() {
      if(vec1 == null) {
         if(vec2 == null)
            return null;
         else {
            var newVec = TopVec<τ,α>(vec2);
            return newVec.SumIntoß(vec2); } }
      else if(vec2 == null) {
         return TopVec<τ,α>(vec1).SumIntoß(vec1); }
      Assume.True(vec1.Dim == vec2.Dim, () => "The dimensions of vectors do not match on addition.");
      return SumTopß(vec1, vec2);
   }

   internal static ValVec<τ,α>? SumTopß<τ,α>(this ValVec<τ,α> vec1, ValVec<τ,α> vec2)  where τ : struct, IEquatable<τ>, IComparable<τ>   where α : IAlgebra<τ>, new() =>
      TopVec<τ,α>(vec1).SumIntoß(vec1)!.SumIntoß(vec2);

   public static ValVec<τ,α>? SubTop<τ,α>(this ValVec<τ,α>? v1, ValVec<τ,α>? v2)  where τ : struct, IEquatable<τ>, IComparable<τ>   where α : IAlgebra<τ>, new() {
      if(v1 == null) {
         if(v2 == null)
            return null;
         else
            return TopVec<τ,α>(v2).SubIntoß(v2); }
      else if(v2 == null) {
         return TopVec<τ,α>(v1).SumIntoß(v1); }
      Assume.True(v1.Dim == v2.Dim, () => "The dimensions of vectors do not match on subtraction.");
      return SubTopß(v1, v2);
   }

   internal static ValVec<τ,α>? SubTopß<τ,α>(this ValVec<τ,α> v1, ValVec<τ,α> v2)  where τ : struct, IEquatable<τ>, IComparable<τ>  where α : IAlgebra<τ>, new() =>
      TopVec<τ,α>(v1).SumIntoß(v1)!.SubIntoß(v2);
   
   /// <summary>Modifies this vector by negating each element.</summary>
   public static ValVec<τ,α>? NegateInto<τ,α>(this ValVec<τ,α>? vec)  where τ : struct, IEquatable<τ>, IComparable<τ>  where α : IAlgebra<τ>, new() {
      if(vec == null)
         return null;
      return NegateIntoß(vec);
   }
   internal static ValVec<τ,α> NegateIntoß<τ,α>(this ValVec<τ,α> vec)  where τ : struct, IEquatable<τ>, IComparable<τ>  where α : IAlgebra<τ>, new() {
      var keys = vec.Scals.Keys.ToArray();                    // We have to do this (access via indexer), because we can't change collection during enumeration.
      for(int i = 0; i < keys.Length; ++i)
         vec.Scals[keys[i]] = NonNullA<τ,α>.O.Neg(vec.Scals[keys[i]]);
      return vec;
   }
   public static ValVec<τ,α>? Negate<τ,α>(this ValVec<τ,α>? vec)  where τ : struct, IEquatable<τ>, IComparable<τ>  where α : IAlgebra<τ>, new() {
      if(vec == null)
         return null;
      else
         return vec.Negateß();
   }
   public static ValVec<τ,α> Negateß<τ,α>(this ValVec<τ,α> vec)  where τ : struct, IEquatable<τ>, IComparable<τ>  where α : IAlgebra<τ>, new() =>
      TopVec<τ,α>(vec).NegateIntoß();
      
   /// <summary>Multiplies vector with a scalar. Modifies "this". Use return as result.</summary>
   /// <param name="scal">Scalar.</param>
   /// <remarks> <see cref="TestRefs.VectorMul"/> </remarks>
   public static ValVec<τ,α>? MulInto<τ,α>(this ValVec<τ,α>? vec, τ scal)  where τ : struct, IEquatable<τ>, IComparable<τ>  where α : IAlgebra<τ>, new() {
      if(vec == null || scal.Equals(NonNullA<τ,α>.O.Zero()))
         return null;
      else
         return MulIntoß(vec, scal);
   }

   internal static ValVec<τ,α> MulIntoß<τ,α>(this ValVec<τ,α> vec, τ scal)  where τ : struct, IEquatable<τ>, IComparable<τ>  where α : IAlgebra<τ>, new() {
      var inxs = vec.Scals.Keys.ToArray();                       // To avoid "collection was modified during enumeration".
      foreach(var inx in inxs)
         vec.Scals[inx] = NonNullA<τ,α>.O.Mul(scal, vec.Scals[inx]);
      return vec;
   }
   /// <summary>Multiplies a scalar with a vector and returns a new vector. Safe version: accepts a nullable vector and checks it for null, also checks if the scalar is zero.</summary>
   /// <param name="scal">Scalar.</param>
   /// <param name="vec">Vector.</param>
   public static ValVec<τ,α>? MulTop<τ,α>(this ValVec<τ,α>? vec, τ scal)  where τ : struct, IEquatable<τ>, IComparable<τ>  where α : IAlgebra<τ>, new() {
      if(vec == null || scal.Equals(NonNullA<τ,α>.O.Zero()))
         return null;
      else
         return vec.MulTopß(scal);
   }
   /// <summary>Multiplies a scalar with a vector and returns a new vector. Unsafe version: accepts a non-nullable vector and does not check whether the scalar is zero.</summary>
   /// <param name="scal"></param>
   /// <param name="vec"></param>
   internal static ValVec<τ,α> MulTopß<τ,α>(this ValVec<τ,α> vec, τ scal)  where τ : struct, IEquatable<τ>, IComparable<τ>  where α : IAlgebra<τ>, new() {
      var newVec = TopVec<τ,α>(vec);
      foreach(var (i, s) in vec) {
         newVec.Add(i, NonNullA<τ,α>.O.Mul(scal, s)); }
      return newVec;
   }

   public static ValVec<τ,α>? MulSub<τ,α>(this ValVec<τ,α>? vec, τ scal, ValTnr<τ,α> sup, int inx)  where τ : struct, IEquatable<τ>, IComparable<τ>  where α : IAlgebra<τ>, new() {
      if(vec == null || scal.Equals(NonNullA<τ,α>.O.Zero()))
         return null;
      else
         return vec.MulSubß(scal, sup, inx);
   }

   internal static ValVec<τ,α> MulSubß<τ,α>(this ValVec<τ,α> vec, τ scal, ValTnr<τ,α> sup, int inx)  where τ : struct, IEquatable<τ>, IComparable<τ>  where α : IAlgebra<τ>, new() {
      var newVec = ValTnrFactory.SubVec<τ,α>(sup, inx);
      foreach(var (i, s) in vec)
         newVec.Add(i, NonNullA<τ,α>.O.Mul(scal, s));
      return newVec;
   }
   /// <summary>Calculates Euclidean norm squared of a vector.</summary>
   public static τ NormSqr<τ,α>(this ValVec<τ,α>? vec)  where τ : struct, IEquatable<τ>, IComparable<τ>  where α : IAlgebra<τ>, new() {
      if(vec == null)
         return NonNullA<τ,α>.O.Zero();
      else
         return vec.NormSqrß();
   }

   internal static τ NormSqrß<τ,α>(this ValVec<τ,α> vec)  where τ : struct, IEquatable<τ>, IComparable<τ>  where α : IAlgebra<τ>, new() {
      τ res = NonNullA<τ,α>.O.Zero();
      foreach(var kv in vec.Scals)
         res = NonNullA<τ,α>.O.Sum(res, NonNullA<τ,α>.O.Mul(kv.Value, kv.Value));
      return res;
   }


   /// <summary>Tensor product of a vector with another tensor. Returns top tensor (null superior) as result.</summary>
   /// <param name="t2">Right hand operand.</param>
   /// <remarks> <see cref="TestRefs.TensorProduct"/> </remarks>
   public static ValTnr<τ,α>? TnrProdTop<τ,α>(this ValVec<τ,α>? v1, ValTnr<τ,α>? t2)  where τ : struct, IEquatable<τ>, IComparable<τ>  where α : IAlgebra<τ>, new() {
      if(v1 == null || t2 == null)
         return null;
      return v1.TnrProdTopß(t2);
   }

   internal static ValTnr<τ,α> TnrProdTopß<τ,α>(this ValVec<τ,α> v1, ValTnr<τ,α> t2)  where τ : struct, IEquatable<τ>, IComparable<τ>  where α : IAlgebra<τ>, new() {
      if(t2.Rank > 1) {
         int newRank = v1.Rank + t2.Rank;
         var strc1 = v1.EnumSubstrc();
         var strc2 = t2.EnumSubstrc();
         var newStrc = strc1.Concat(strc2).ToList();
         // We must substitute this vector with a tensor whose elements are multiples of tnr2.
         var prod = TopTnr<τ,α>(newStrc, v1.Scals.Count);
         foreach(var (i, s1) in v1.Scals)
            t2.MulSubβ(s1, prod, i);
         return prod; }
      else {
         var v2 = (ValVec<τ,α>) t2;
         return v1.TnrProdTopß(v2); }
   }

   public static ValTnr<τ,α>? TnrProdTop<τ,α>(this ValVec<τ,α>? v1, ValVec<τ,α>? v2)  where τ : struct, IEquatable<τ>, IComparable<τ>  where α : IAlgebra<τ>, new() {
      if(v1 == null || v2 == null)
         return null;
      return v1.TnrProdTopß(v2);
   }

   /// <remarks> <see cref="TestRefs.VectorTnrProductVector"/> </remarks>
   internal static ValTnr<τ,α> TnrProdTopß<τ,α>(this ValVec<τ,α> v1, ValVec<τ,α> v2)  where τ : struct, IEquatable<τ>, IComparable<τ>  where α : IAlgebra<τ>, new() {
      var newStrc = new List<int> {v1.Dim, v2.Dim};
      var prod = TopTnr<τ,α>(newStrc, v1.Scals.Count);
      foreach(var (i, s1) in v1.Scals)
         v2.MulSub(s1, prod, i);
      return prod;
   }

   public static ValTnr<τ,α>? ContractTopPart2<τ,α>(this ValVec<τ,α> v1, ValTnr<τ,α> t2, int rankInx2, List<int> strc, int conDim)  where τ : struct, IEquatable<τ>, IComparable<τ>  where α : IAlgebra<τ>, new() {
      if(t2.Rank > 2) {                                                  // Result is tensor.
         ValTnr<τ,α>? elimTnr2, sumand;
         var sum = TopTnr<τ,α>(strc);                                    // Set sum to a zero tensor.
         for(int i = 0; i < conDim; ++i) {
            elimTnr2 = t2.ReduceRankTop(rankInx2, i);
            if(v1.Scals.TryGetValue(i, out var s) && elimTnr2 != null) {
               sumand = elimTnr2.MulTopβ(s);
               sum.SumIntoβ(sumand); } }
         if(sum.Count != 0)
            return sum;
         else
            return null; }
      else if(t2.Rank == 2) {
         ValTnr<τ,α>? elimTnr2;
         ValVec<τ,α>? elimVec2, sumand;
         var sum = TopVec<τ,α>(strc[0]);
         for(int i = 0; i < conDim; ++i) {
            elimTnr2 = t2.ReduceRankTop(rankInx2, i);
            if(elimTnr2 != null) {
               elimVec2 = (ValVec<τ,α>?) t2.ReduceRankTop(rankInx2, i);
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

   public static ValTnr<τ,α>? ContractTop<τ,α>(this ValVec<τ,α> v1, ValTnr<τ,α> t2, int slot2)  where τ : struct, IEquatable<τ>, IComparable<τ>  where α : IAlgebra<τ>, new() {
      (List<int> strc, _, int rankInx2, int conDim) = v1.ContractTopPart1(t2, 1, slot2);
      return ContractTopPart2(v1, t2, rankInx2, strc, conDim);
   }

   public static τ ContractTop<τ,α>(this ValVec<τ,α>? v1, ValVec<τ,α>? v2)  where τ : struct, IEquatable<τ>, IComparable<τ>  where α : IAlgebra<τ>, new() {
      if(v1 == null || v2 == null)
         return NonNullA<τ,α>.O.Zero();
      else
         return v1.ContractTopß<τ,α>(v2);
   }

   public static τ ContractTopß<τ,α>(this ValVec<τ,α> v1, ValVec<τ,α> v2)  where τ : struct, IEquatable<τ>, IComparable<τ>  where α : IAlgebra<τ>, new() {
      τ res = NonNullA<τ,α>.O.Zero();
      foreach(var int_val1 in v1.Scals) {
         if(v2.Scals.TryGetValue(int_val1.Key, out var val2))
            res = NonNullA<τ,α>.O.Sum(res, NonNullA<τ,α>.O.Mul(int_val1.Value, val2)); }
      return res;
   }

   public static bool Equals<τ,α>(this ValVec<τ,α>? v1, ValVec<τ,α>? v2) where τ : struct, IEquatable<τ>, IComparable<τ>  where α : IAlgebra<τ>, new() {
      if(v1 == null) {                                                            // If both are null, return true. If only one of them is null, return false.
         if(v2 == null)
            return true;
         else
            return false; }
      else if(v2 == null)
         return false;
      if(!v1.CompareSubstrcβ(v2))                                         // If substructures mismatch, they are not equal.
         return false;
      if(!v1.Scals.Keys.OrderBy(key => key).SequenceEqual(v2.Scals.Keys.OrderBy(key => key)))    // Keys have to match.
         return false;
      foreach(var (i,s1) in v1.Scals) {
         τ s2 = v2[i];
         if(!s1.Equals(s2))
            return false; }
      return true;
   }

   public static bool Equals<τ,α>(this ValVec<τ,α>? v1, ValVec<τ,α>? v2, τ eps) where τ : struct, IEquatable<τ>, IComparable<τ>  where α : IAlgebra<τ>, new() {
      if(v1 == null) {                                                            // If both are null, return true. If only one of them is null, return false.
         if(v2 == null)
            return true;
         else
            return false; }
      else if(v2 == null)
         return false;
      if(!v1.CompareSubstrcβ(v2))                                         // If substructures mismatch, they are not equal.
         return false;
      if(!v1.Scals.Keys.OrderBy(key => key).SequenceEqual(v2.Scals.Keys.OrderBy(key => key)))    // Keys have to match.
         return false;
      foreach(var (i,s1) in v1.Scals) {
         τ s2 = v2[i];
         if(NonNullA<τ,α>.O.Abs(NonNullA<τ,α>.O.Sub(s1, s2)).CompareTo(eps) > 0 ) // Values do not agree within tolerance.
            return false; }
      return true;                                                              // All values agree within tolerance.
   }
}
}