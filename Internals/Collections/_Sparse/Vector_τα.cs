using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using dbl = System.Double;

using Fluid.Internals;
using Fluid.Internals.Numerics;
using static Fluid.Internals.Toolbox;
using Fluid.TestRef;
namespace Fluid.Internals.Collections {
using static Factory;
   
/// <summary>A vector with specified dimension which holds values of type τ. Those can use arithmetic defined inside type α.</summary>
/// <typeparam name="τ">Type of values.</typeparam>
/// <typeparam name="α">Type defining arithmetic between values.</typeparam>
public partial class Vector<τ,α> : Tensor<τ,α>, IEquatable<Vector<τ,α>>
where τ : IEquatable<τ>, IComparable<τ>
where α : IArithmetic<τ>, new() {
   public new int Count => CountInternal;
   protected override int CountInternal => Scals.Count;
   /// <summary>Scalars. An extra wrapped Dictionary which holds vector elements.</summary>
   public Dictionary<int,τ> Scals { get; internal set; }


   

   /// <remarks> <see cref="TestRefs.VectorIndexer"/> </remarks>
   new public τ this[int i] {
      get {
         Scals.TryGetValue(i, out τ val);
         return val; }
      set {
         if(!value.Equals(default(τ))) {
            Scals[i] = value; }
         else
            Scals.Remove(i); }
   }

   /// <summary>Tensor product of a vector with another tensor. Returns top tensor (null superior) as result.</summary>
   /// <param name="tnr2">Right hand operand.</param>
   /// <remarks> <see cref="TestRefs.TensorProduct"/> </remarks>
   public override Tensor<τ,α> TnrProductTop(Tensor<τ,α> tnr2) {
      if(tnr2.Rank > 1) {
         int newRank = Rank + tnr2.Rank;
         var struct1 = CopySubstructure();
         var struct2 = tnr2.CopySubstructure();
         var newStructure = struct1.Concat(struct2).ToList();
         // We must substitute this vector with a tensor whose elements are multiples of tnr2.
         var res = TopTensor<τ,α>(newStructure, Scals.Count);
         foreach(var int_subVal in Scals) {
            int subKey = int_subVal.Key;
            var subVal = int_subVal.Value;
            res.AddPlus(subKey, subVal*tnr2); }                   // TODO: Check multiply for top/sub.
         return res; }
      else {
         var vec2 = (Vector<τ,α>) tnr2;
         return TnrProductTop(vec2); }
   }
   /// <remarks> <see cref="TestRefs.VectorTnrProductVector"/> </remarks>
   public Tensor<τ,α> TnrProductTop(Vector<τ,α> vec2) {
      int dim1 = Structure.Last();
      int dim2 = vec2.Structure.Last();
      var newStructure = new List<int> {dim1, dim2};
      var res = TopTensor<τ,α>(newStructure, Scals.Count);
      foreach(var int_subVal1 in Scals) {
         int subKey = int_subVal1.Key;
         var subVal1 = int_subVal1.Value;
         var newVec = subVal1*vec2;
         res.AddPlusIfNotEmpty(subKey, newVec); }
      return res;
   }
   /// <summary>Sums vec2 to vec1. Modifies vec1, does not destroy vec2.</summary>
   /// <param name="vec1">Sumand 1.</param>
   /// <param name="vec2">Sumand 2. Is not destroyed.</param>
   /// <remarks><see cref="TestRefs.VectorSum"/></remarks>
   public static Vector<τ,α>? SumM(Vector<τ,α> vec1, Vector<τ,α> vec2) {
      foreach(var int_subVal2 in vec2.Scals) {
         int subKey = int_subVal2.Key;
         var subVal2 = int_subVal2.Value;
         var subVal1 = vec1[subKey];
         var sum = O<τ,α>.A.Sum(subVal1, subVal2);
         vec1[subKey] = sum; }
      if(vec1.Count != 0)
         return vec1;
      else
         return null;
   }
   /// <summary>Subtracts vec2 from vec1. Modifies vec1, does not destroy vec2.</summary>
   /// <param name="vec1">Minuend.</param>
   /// <param name="vec2">Subtrahend. Is not destroyed.</param>
   /// <remarks><see cref="TestRefs.VectorSub"/></remarks>
   public static Vector<τ,α>? SubM(Vector<τ,α> vec1, Vector<τ,α> vec2) {
      foreach(var int_subVal2 in vec2.Scals) {
         int subKey = int_subVal2.Key;
         var subVal2 = int_subVal2.Value;
         var subVal1 = vec1[subKey];
         var dif = O<τ,α>.A.Sub(subVal1, subVal2);
         vec1[subKey] = dif; }
      if(vec1.Count != 0)
         return vec1;
      else
         return null;
   }


   
   /// <summary>Multiplies caller with a scalar.</summary>
   /// <param name="scal">Scalar.</param>
   /// <remarks> <see cref="TestRefs.VectorMul"/> </remarks>
   public override void Mul(τ scal) {
      var keys = Scals.Keys.ToArray();                       // To avoid "collection was modified during enumeration".
      foreach(var subKey in keys)
         Scals[subKey] = O<τ,α>.A.Mul(scal, Scals[subKey]);
   }
   /// <summary>Sum two vectors. Does not check substructure match.</summary>
   /// <param name="vec1">Left operand.</param>
   /// <param name="vec2">Right operand.</param>
   /// <remarks><see cref="TestRefs.Op_VectorAddition"/></remarks>
   public static Vector<τ,α> operator + (Vector<τ,α> vec1, Vector<τ,α> vec2) {
      //var newStruc = vec2.CopySubstructure();
      var res = new Vector<τ,α>(vec2, in CopySpecs.S320_04);
      res.AssignStructFromSubStruct(vec2);
      foreach(var int_val1 in vec1.Scals) {
         int key = int_val1.Key;
         var val1 = int_val1.Value;
         if(vec2.Scals.TryGetValue(key, out var val2)) {
            res[key] = O<τ,α>.A.Sum(val1, val2); }
         else {
            res.Add(key, val1); } }
      return res;
   }
   /// <summary>Subtract two vectors. Does not check substructure match.</summary>
   /// <param name="vec1">Left operand.</param>
   /// <param name="vec2">Right operand.</param>
   /// <remarks><see cref="TestRefs.Op_VectorSubtraction"/></remarks>
   public static Vector<τ,α> operator - (Vector<τ,α> vec1, Vector<τ,α> vec2) {
      //var newStruc = vec1.CopySubstructure();
      var res = new Vector<τ,α>(vec1, in CopySpecs.S320_04);
      res.AssignStructFromSubStruct(vec1);
      foreach(var int_val2 in vec2.Scals) {
         int key = int_val2.Key;
         var val2 = int_val2.Value;
         if(vec1.Scals.TryGetValue(key, out var val1)) {
            res[key] = O<τ,α>.A.Sub(val1, val2); }
         else {
            res.Add(key, O<τ,α>.A.Neg(val2)); } }
      return res;
   }
   /// <summary>Modifies this vector by negating each element.</summary>
   public override void Negate() {
      var keys = Scals.Keys.ToArray();                    // We have to do this (access via indexer), because we can't change collection during enumeration.
      for(int i = 0; i < keys.Length; ++i)
         Scals[keys[i]] = O<τ,α>.A.Neg(Scals[keys[i]]);
   }
   /// <summary>Negate operator. Creates a new vector with its own substructure.</summary>
   /// <param name="vec">Vector to negate.</param>
   public static Vector<τ,α> operator - (Vector<τ,α> vec) {
      var newStruc = vec.CopySubstructure();
      var res = new Vector<τ,α>(newStruc, Voids<τ,α>.Vec, vec.Count);
      foreach(var int_val in vec.Scals)
         res.Scals.Add(int_val.Key, O<τ,α>.A.Neg(int_val.Value));
      return res;
   }
   /// <remarks> <see cref="TestRefs.Op_ScalarVectorMultiplication"/> </remarks>
   public static Vector<τ,α> operator * (τ scal, Vector<τ,α> vec) {
      var newStrc = vec.CopySubstructure();
      var res = new Vector<τ,α>(newStrc, Voids<τ,α>.Vec, vec.Count);
      foreach(var int_val in vec.Scals)
         res.Scals.Add(int_val.Key, O<τ,α>.A.Mul(scal, vec[int_val.Key]));
      return res;
   }

   public static Vector<τ,α> ScalVecMulTop(τ scal, Vector<τ,α> vec) {
      var res = TopVector<τ,α>(vec.Dim, vec.Count);
      foreach(var inx_scl in vec.Scals)
         res.Scals.Add(inx_scl.Key, O<τ,α>.A.Mul(scal, vec[inx_scl.Key]));
      return res;
   }

   public static Tensor<τ,α> ContractPart2(Vector<τ,α> vec1, Tensor<τ,α> tnr2, int truInx2, List<int> struc3, int conDim) {
      if(tnr2.Rank > 2) {                                                  // Result is tensor.
         Tensor<τ,α> elimTnr2, sumand, sum;
         sum = new Tensor<τ,α>(struc3);                                    // Set sum to a zero tensor.
         for(int i = 0; i < conDim; ++i) {
            elimTnr2 = tnr2.ReduceRank(truInx2, i);
            if(vec1.Scals.TryGetValue(i, out var val) && elimTnr2 != Voids<τ,α>.Vec) {
               sumand = val*elimTnr2;
               sum.Sum(sumand); } }
         if(sum.Count != 0)
            return sum;
         else
            return Voids<τ,α>.Vec; }
      else if(tnr2.Rank == 2) {
         Tensor<τ,α> elimTnr2;
         Vector<τ,α> elimVec2, sumand, sum;
         sum = new Vector<τ,α>(struc3, Voids<τ,α>.Vec, 4);
         for(int i = 0; i < conDim; ++i) {
            elimTnr2 = tnr2.ReduceRank(truInx2, i);
            if(elimTnr2 != Voids<τ,α>.Vec) {
               elimVec2 = (Vector<τ,α>) tnr2.ReduceRank(truInx2, i);
               if(vec1.Scals.TryGetValue(i, out var val)) {
                  sumand = val*elimVec2;
                  sum.Sum(sumand); } } }
         if(sum.Scals.Count != 0)
            return sum;
         else
            return Voids<τ,α>.Vec; }
      else {                                                               // Result is scalar.
         throw new ArgumentException("Explicitly cast tnr2 to vector before using contract."); }
   }

   public static Tensor<τ,α> Contract(Vector<τ,α> vec1, Tensor<τ,α> tnr2, int natInx2) {
      (List<int> struc3, _, int truInx2, int conDim) = Tensor<τ,α>.ContractPart1(vec1, tnr2, 1, natInx2);
      return ContractPart2(vec1, tnr2, truInx2, struc3, conDim);
   }
   public static τ Contract(Vector<τ,α> vec1, Vector<τ,α> vec2) {
      τ res = O<τ,α>.A.Zero();
      foreach(var int_val1 in vec1.Scals) {
         if(vec2.Scals.TryGetValue(int_val1.Key, out var val2))
            res = O<τ,α>.A.Sum(res, O<τ,α>.A.Mul(int_val1.Value, val2)); }
      return res;
   }
   /// <summary>Dot (scalar) product.</summary>
   /// <remarks> <see cref="TestRefs.Op_VectorDotVector"/> </remarks>
   public static τ operator *(Vector<τ,α> vec1, Vector<τ,α> vec2) =>
      Contract(vec1, vec2);

   /// <summary>Calculates square of Euclidean norm of SparseRow.</summary>
   public τ NormSqr() {
      τ res = O<τ,α>.A.Zero();
      foreach(var kv in Scals)
         res = O<τ,α>.A.Sum(res, O<τ,α>.A.Mul(kv.Value, kv.Value));
      return res;
   }

   public bool Equals(Vector<τ,α> vec2) {
      if(!Scals.Keys.OrderBy(key => key).SequenceEqual(vec2.Scals.Keys.OrderBy(key => key)))    // Keys have to match.
         return false;
      foreach(var int_val in Scals) {
         τ val2 = vec2[int_val.Key];
         if(!int_val.Value.Equals(val2))        // Fetch did not suceed or values are not equal.
            return false; }
      return true;
   }

   public bool Equals(Vector<τ,α> vec2, τ eps) {
      if(!Scals.Keys.OrderBy(key => key).SequenceEqual(vec2.Scals.Keys.OrderBy(key => key)))    // Keys have to match.
         return false;
      foreach(var int_val1 in Scals) {
         τ val2 = vec2[int_val1.Key];
         if(O<τ,α>.A.Abs(O<τ,α>.A.Sub(int_val1.Value, val2)).CompareTo(eps) > 0 ) // Values do not agree within tolerance.
            return false; }
      return true;                                                              // All values agree within tolerance.
   }
   /// <summary>So that foreach statements work properly.</summary>
   new public IEnumerator<KeyValuePair<int,τ>> GetEnumerator() {
      foreach(var kv in Scals)
         yield return kv;
   }

   public override string ToString() {
      var sb = new StringBuilder(2*Count);
      sb.Append("{");
      foreach(var emt in Scals) {
         sb.Append($"{emt.ToString()}, ");
      }
      sb.Remove(sb.Length - 2, 2);
      sb.Append("}");
      return sb.ToString();
   }
   /// <summary>Converts a sparse Vector to a regular array.</summary>
   public τ[] ToArray() {
      var arr = new τ[Dim];
      foreach(var int_val in Scals)
         arr[int_val.Key] = int_val.Value;
      return arr;
   }
}
}