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

   
   // public bool TryGetValue(int inx, out τ scal) =>
   //    Scals.TryGetValue(inx, out scal);


   
   
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
      vec.MulTop(scal);
   

   
   /// <summary>Dot (scalar) product.</summary>
   /// <remarks> <see cref="TestRefs.Op_VectorDotVector"/> </remarks>
   public static τ operator *(Vector<τ,α> v1, Vector<τ,α> v2) =>
      v1.Contract(v2);

   

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