using System;
using System.Collections.Generic;
using System.Linq;
using Fluid.Internals.Numerics;
using static Fluid.Internals.Collections.Factory;

namespace Fluid.Internals.Collections {
public static class VectorExt {
   /// <summary>Sums vec2 to vec1. Modifies vec1, does not destroy vec2. If vec1 is null, it creates a copy of vec2.</summary>
   /// <param name="vec1">Sumand 1 is modified. Use return as result.</param>
   /// <param name="vec2">Sumand 2. Is not destroyed.</param>
   /// <remarks><see cref="TestRefs.VectorSum"/></remarks>
   public static Vector<τ,α>? SumInto<τ,α>(this Vector<τ,α>? vec1, Vector<τ,α>? vec2)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() {
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
   internal static Vector<τ,α>? SumIntoß<τ,α>(this Vector<τ,α> vec1, Vector<τ,α> vec2)  where τ : IEquatable<τ>, IComparable<τ>   where α : IArithmetic<τ>, new() {
      foreach(var (i, s2) in vec2.Scals) {
         var s1 = vec1[i];
         var sum = O<τ,α>.A.Sum(s1, s2);
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
   public static Vector<τ,α>? SubInto<τ,α>(this Vector<τ,α>? vec1, Vector<τ,α>? vec2)  where τ : IEquatable<τ>, IComparable<τ>   where α : IArithmetic<τ>, new() {
      if(vec2 == null)
         return vec1;
      if(vec1 == null)
         vec1 = TopVector<τ,α>(vec2);
      Assume.True(vec1.Dim == vec2.Dim, () => "The dimensions of vectors do not match on subtraction.");
      return SubIntoß(vec1, vec2);
   }
   /// <summary>Subtracts vec2 from vec1. Modifies vec1, does not destroy vec2.</summary>
   /// <param name="vec1">Minuend. Is modified, use return as result.</param>
   /// <param name="vec2">Subtrahend. Is not destroyed.</param>
   /// <remarks><see cref="TestRefs.VectorSub"/></remarks>
   internal static Vector<τ,α>? SubIntoß<τ,α>(this Vector<τ,α> vec1, Vector<τ,α> vec2)  where τ : IEquatable<τ>, IComparable<τ>   where α : IArithmetic<τ>, new() {
      foreach(var (i, s2) in vec2.Scals) {
         var subVal1 = vec1[i];
         var dif = O<τ,α>.A.Sub(subVal1, s2);
         vec1[i] = dif; }
      if(vec1.Count != 0)
         return vec1;
      else
         return null;
   }

   public static Vector<τ,α>? SumTop<τ,α>(this Vector<τ,α>? vec1, Vector<τ,α>? vec2)  where τ : IEquatable<τ>, IComparable<τ>   where α : IArithmetic<τ>, new() {
      if(vec1 == null) {
         if(vec2 == null)
            return null;
         else {
            var newVec = TopVector<τ,α>(vec2);
            return newVec.SumIntoß(vec2); } }
      else if(vec2 == null) {
         return TopVector<τ,α>(vec1).SumIntoß(vec1); }
      Assume.True(vec1.Dim == vec2.Dim, () => "The dimensions of vectors do not match on addition.");
      return SumTopß(vec1, vec2);
   }

   internal static Vector<τ,α>? SumTopß<τ,α>(this Vector<τ,α> vec1, Vector<τ,α> vec2)  where τ : IEquatable<τ>, IComparable<τ>   where α : IArithmetic<τ>, new() =>
      TopVector<τ,α>(vec1).SumIntoß(vec1)!.SumIntoß(vec2);

   public static Vector<τ,α>? SubTop<τ,α>(this Vector<τ,α>? vec1, Vector<τ,α>? vec2)  where τ : IEquatable<τ>, IComparable<τ>   where α : IArithmetic<τ>, new() {
      if(vec1 == null) {
         if(vec2 == null)
            return null;
         else
            return TopVector<τ,α>(vec2).SubIntoß(vec2); }
      else if(vec2 == null) {
         return TopVector<τ,α>(vec1).SumIntoß(vec1); }
      Assume.True(vec1.Dim == vec2.Dim, () => "The dimensions of vectors do not match on addition.");
      return SumTopß(vec1, vec2);
   }

   internal static Vector<τ,α>? SubTopß<τ,α>(this Vector<τ,α> vec1, Vector<τ,α> vec2)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() =>
      TopVector<τ,α>(vec1).SumIntoß(vec1)!.SubIntoß(vec2);
   
   /// <summary>Modifies this vector by negating each element.</summary>
   public static Vector<τ,α>? NegateInto<τ,α>(this Vector<τ,α>? vec)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() {
      if(vec == null)
         return null;
      return NegateIntoß(vec);
   }
   internal static Vector<τ,α> NegateIntoß<τ,α>(this Vector<τ,α> vec)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() {
      var keys = vec.Scals.Keys.ToArray();                    // We have to do this (access via indexer), because we can't change collection during enumeration.
      for(int i = 0; i < keys.Length; ++i)
         vec.Scals[keys[i]] = O<τ,α>.A.Neg(vec.Scals[keys[i]]);
      return vec;
   }
   public static Vector<τ,α>? Negate<τ,α>(this Vector<τ,α>? vec)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() {
      if(vec == null)
         return null;
      else
         return vec.Negateß();
   }
   public static Vector<τ,α> Negateß<τ,α>(this Vector<τ,α> vec)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() =>
      TopVector<τ,α>(vec).NegateIntoß();
      
   /// <summary>Multiplies vector with a scalar. Modifies "this". Use return as result.</summary>
   /// <param name="scal">Scalar.</param>
   /// <remarks> <see cref="TestRefs.VectorMul"/> </remarks>
   public static Vector<τ,α>? MulInto<τ,α>(this Vector<τ,α>? vec, τ scal)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() {
      if(vec == null || scal.Equals(O<τ,α>.A.Zero()))
         return null;
      else
         return MulIntoß(vec, scal);
   }

   internal static Vector<τ,α> MulIntoß<τ,α>(this Vector<τ,α> vec, τ scal)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() {
      var inxs = vec.Scals.Keys.ToArray();                       // To avoid "collection was modified during enumeration".
      foreach(var inx in inxs)
         vec.Scals[inx] = O<τ,α>.A.Mul(scal, vec.Scals[inx]);
      return vec;
   }
   /// <summary>Multiplies a scalar with a vector and returns a new vector. Safe version: accepts a nullable vector and checks it for null, also checks if the scalar is zero.</summary>
   /// <param name="scal">Scalar.</param>
   /// <param name="vec">Vector.</param>
   public static Vector<τ,α>? MulTop<τ,α>(this Vector<τ,α>? vec, τ scal)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() {
      if(vec == null || scal.Equals(O<τ,α>.A.Zero()))
         return null;
      else
         return vec.MulTopß(scal);
   }
   /// <summary>Multiplies a scalar with a vector and returns a new vector. Unsafe version: accepts a non-nullable vector and does not check whether the scalar is zero.</summary>
   /// <param name="scal"></param>
   /// <param name="vec"></param>
   internal static Vector<τ,α> MulTopß<τ,α>(this Vector<τ,α> vec, τ scal)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() {
      var newVec = TopVector<τ,α>(vec);
      foreach(var (i, s) in vec) {
         newVec.Add(i, O<τ,α>.A.Mul(scal, s)); }
      return newVec;
   }

   public static Vector<τ,α>? MulSub<τ,α>(this Vector<τ,α>? vec, τ scal, Tensor<τ,α> sup, int inx)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() {
      if(vec == null || scal.Equals(O<τ,α>.A.Zero()))
         return null;
      else
         return vec.MulSubß(scal, sup, inx);
   }

   internal static Vector<τ,α> MulSubß<τ,α>(this Vector<τ,α> vec, τ scal, Tensor<τ,α> sup, int inx)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() {
      var newVec = Factory.SubVector<τ,α>(sup, inx);
      foreach(var (i, s) in vec)
         newVec.Add(i, O<τ,α>.A.Mul(scal, s));
      return newVec;
   }
   /// <summary>Calculates Euclidean norm squared of a vector.</summary>
   public static τ NormSqr<τ,α>(this Vector<τ,α>? vec)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() {
      if(vec == null)
         return O<τ,α>.A.Zero();
      else
         return vec.NormSqrß();
   }

   internal static τ NormSqrß<τ,α>(this Vector<τ,α> vec)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() {
      τ res = O<τ,α>.A.Zero();
      foreach(var kv in vec.Scals)
         res = O<τ,α>.A.Sum(res, O<τ,α>.A.Mul(kv.Value, kv.Value));
      return res;
   }


   /// <summary>Tensor product of a vector with another tensor. Returns top tensor (null superior) as result.</summary>
   /// <param name="t2">Right hand operand.</param>
   /// <remarks> <see cref="TestRefs.TensorProduct"/> </remarks>
   public static Tensor<τ,α>? TnrProdTop<τ,α>(this Vector<τ,α>? v1, Tensor<τ,α>? t2)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() {
      if(v1 == null || t2 == null)
         return null;
      return v1.TnrProdTopß(t2);
   }

   internal static Tensor<τ,α> TnrProdTopß<τ,α>(this Vector<τ,α> v1, Tensor<τ,α> t2)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() {
      if(t2.Rank > 1) {
         int newRank = v1.Rank + t2.Rank;
         var strc1 = v1.EnumSubstrc();
         var strc2 = t2.EnumSubstrc();
         var newStrc = strc1.Concat(strc2).ToList();
         // We must substitute this vector with a tensor whose elements are multiples of tnr2.
         var prod = TopTensor<τ,α>(newStrc, v1.Scals.Count);
         foreach(var (i, s1) in v1.Scals)
            t2.MulSubß(s1, prod, i);
         return prod; }
      else {
         var v2 = (Vector<τ,α>) t2;
         return v1.TnrProdTopß(v2); }
   }

   public static Tensor<τ,α>? TnrProdTop<τ,α>(this Vector<τ,α>? v1, Vector<τ,α>? v2)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() {
      if(v1 == null || v2 == null)
         return null;
      return v1.TnrProdTopß(v2);
   }

   /// <remarks> <see cref="TestRefs.VectorTnrProductVector"/> </remarks>
   internal static Tensor<τ,α> TnrProdTopß<τ,α>(this Vector<τ,α> v1, Vector<τ,α> v2)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() {
      var newStrc = new List<int> {v1.Dim, v2.Dim};
      var prod = TopTensor<τ,α>(newStrc, v1.Scals.Count);
      foreach(var (i, s1) in v1.Scals)
         v2.MulSub(s1, prod, i);
      return prod;
   }

   public static Tensor<τ,α>? ContractTopPart2<τ,α>(this Vector<τ,α> v1, Tensor<τ,α> t2, int rankInx2, List<int> strc, int conDim)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() {
      if(t2.Rank > 2) {                                                  // Result is tensor.
         Tensor<τ,α>? elimTnr2, sumand;
         var sum = TopTensor<τ,α>(strc);                                    // Set sum to a zero tensor.
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
         Tensor<τ,α>? elimTnr2;
         Vector<τ,α>? elimVec2, sumand;
         var sum = TopVector<τ,α>(strc[0]);
         for(int i = 0; i < conDim; ++i) {
            elimTnr2 = t2.ReduceRankTop(rankInx2, i);
            if(elimTnr2 != null) {
               elimVec2 = (Vector<τ,α>?) t2.ReduceRankTop(rankInx2, i);
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

   public static Tensor<τ,α>? Contract<τ,α>(this Vector<τ,α> v1, Tensor<τ,α> t2, int slot2)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() {
      (List<int> strc, _, int rankInx2, int conDim) = v1.ContractTopPart1(t2, 1, slot2);
      return ContractTopPart2(v1, t2, rankInx2, strc, conDim);
   }
   public static τ Contract<τ,α>(this Vector<τ,α> vec1, Vector<τ,α> vec2)  where τ : IEquatable<τ>, IComparable<τ>  where α : IArithmetic<τ>, new() {
      τ res = O<τ,α>.A.Zero();
      foreach(var int_val1 in vec1.Scals) {
         if(vec2.Scals.TryGetValue(int_val1.Key, out var val2))
            res = O<τ,α>.A.Sum(res, O<τ,α>.A.Mul(int_val1.Value, val2)); }
      return res;
   }
}
}