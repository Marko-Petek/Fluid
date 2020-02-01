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
   /// <summary>Indexer. Check for empty parent after operation.</summary>
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
   public Tensor<τ,α>? TnrProductTop(Vector<τ,α>? vec2) {
      if(vec2 != null) {
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
      else return null;
   }
   


   
   
   /// <summary>Sum two vectors, return new vector as a result.</summary>
   /// <param name="vec1">Left operand.</param>
   /// <param name="vec2">Right operand.</param>
   /// <remarks><see cref="TestRefs.Op_VectorAddition"/></remarks>
   public static Vector<τ,α>? operator + (Vector<τ,α>? vec1, Vector<τ,α>? vec2) =>
      vec1.SumTop(vec2);
   
   /// <summary>Subtract two vectors. Returns new top vector as result.</summary>
   /// <param name="vec1">Left operand.</param>
   /// <param name="vec2">Right operand.</param>
   /// <remarks><see cref="TestRefs.Op_VectorSubtraction"/></remarks>
   public static Vector<τ,α>? operator - (Vector<τ,α>? vec1, Vector<τ,α>? vec2) =>
      vec1.SubTop(vec2);

   
   
   /// <summary>Negate operator. Creates a new vector with its own substructure.</summary>
   /// <param name="vec">Vector to negate.</param>
   public static Vector<τ,α>? operator - (Vector<τ,α>? vec) =>
      vec.Negate();
   /// <remarks> <see cref="TestRefs.Op_ScalarVectorMultiplication"/> </remarks>
   public static Vector<τ,α>? operator * (τ scal, Vector<τ,α> vec) =>
      MulTop(scal, vec);
   

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