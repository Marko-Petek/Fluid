﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using dbl = System.Double;

using Fluid.Internals;
using Fluid.Internals.Algebras;
using static Fluid.Internals.Tools;
namespace Fluid.Internals.Tensors {
using static ValTnrFactory;
   
/// <summary>A vector with specified dimension which holds values of type τ. Those can use arithmetic defined inside type α.</summary>
/// <typeparam name="τ">Type of values.</typeparam>
/// <typeparam name="α">Type defining arithmetic between values.</typeparam>
public class ValVec<τ,α> : ValTnr<τ,α>, IEquatable<ValVec<τ,α>?>
where τ : struct, IEquatable<τ>, IComparable<τ>
where α : IAlgebra<τ>, new() {

   /// <summary>Void vector.</summary>
   public static readonly ValVec<τ,α> V = ValTnrFactory.TopVec<τ,α>(0,0);
   /// <summary>Constructor with redundancy, used internally.</summary>
   /// <param name="strc">Structure (absorbed).</param>
   /// <param name="sup">Direct superior.</param>
   /// <param name="cap">Capacity of internal dictionary.</param>
   internal ValVec(List<int> strc, ValTnr<τ,α>? sup, int cap) :
   base(strc, 1, sup, 0) {                                                     // Zero capacity for dictionary holding tensors.
      Scals = new Dictionary<int, τ>(cap);
   }
   /// <summary>Creates a non-top vector with specified superior and initial capacity. Does not add the new vector to its superior or check whether the superior is rank 2.</summary>
   /// <param name="sup">Direct superior.</param>
   /// <param name="cap">Capacity of internal dictionary.</param>
   internal ValVec(ValTnr<τ,α> sup, int cap) : this(sup.Strc, sup, cap) { }
   
   /// <summary>Creates a top vector (null superior) with specified dimension and initial capacity.</summary>
   /// <param name="dim">Dimension. Number of spots available for values.</param>
   /// <param name="cap">Capacity of internal dictionary.</param>
   internal ValVec(int dim, int cap) : this(new List<int> {dim}, null, cap) { }

   /// <summary>Adds entry to internal dictionary without checking if it is equal to zero.</summary>
   /// <param name="key">Index.</param>
   /// <param name="val">Value.</param>
   internal void Add(int key, τ val) =>
      Scals.Add(key, val);

   internal bool TryGetValue(int inx, out τ scal) =>
      Scals.TryGetValue(inx, out scal);

   public override int Count => Scals.Count;
   
   /// <summary>Scalars. An extra wrapped Dictionary which holds vector elements.</summary>
   public Dictionary<int,τ> Scals { get; internal set; }


   

   /// <remarks> <see cref="TestRefs.VectorIndexer"/> </remarks>
   /// <summary>Indexer. Check for empty parent after operation.</summary>
   new public τ this[int i] {
      get {
         Scals.TryGetValue(i, out τ val);
         return val; }
      set {
         if(!value.Equals(NonNullA<τ,α>.O.Zero())) {
            Scals[i] = value; }
         else
            Scals.Remove(i); }
   }

   
   // public bool TryGetValue(int inx, out τ scal) =>
   //    Scals.TryGetValue(inx, out scal);


   
   
   /// <summary>Sum two vectors, return new top vector as a result.</summary>
   /// <param name="vec1">Left operand.</param>
   /// <param name="vec2">Right operand.</param>
   /// <remarks><see cref="TestRefs.Op_VectorAddition"/></remarks>
   public static ValVec<τ,α>? operator + (ValVec<τ,α>? vec1, ValVec<τ,α>? vec2) =>
      vec1.SumTop(vec2);
   
   /// <summary>Subtract two vectors. Returns new top vector as result.</summary>
   /// <param name="vec1">Left operand.</param>
   /// <param name="vec2">Right operand.</param>
   /// <remarks><see cref="TestRefs.Op_VectorSubtraction"/></remarks>
   public static ValVec<τ,α>? operator - (ValVec<τ,α>? vec1, ValVec<τ,α>? vec2) =>
      vec1.SubTop(vec2);

   
   
   /// <summary>Negate operator. Creates a new vector with its own substructure.</summary>
   /// <param name="vec">Vector to negate.</param>
   public static ValVec<τ,α>? operator - (ValVec<τ,α>? vec) =>
      vec.Negate();
   /// <remarks> <see cref="TestRefs.Op_ScalarVectorMultiplication"/> </remarks>
   public static ValVec<τ,α>? operator * (τ scal, ValVec<τ,α> vec) =>
      vec.MulTop(scal);
   

   
   /// <summary>Dot (scalar) product.</summary>
   /// <remarks> <see cref="TestRefs.Op_VectorDotVector"/> </remarks>
   public static τ operator *(ValVec<τ,α> v1, ValVec<τ,α> v2) =>
      v1.ContractTopß(v2);

   
   [Obsolete]
   public bool Equals(ValVec<τ,α>? v2) {
      if(v2 != null) {
         if(!Scals.Keys.OrderBy(key => key).SequenceEqual(v2.Scals.Keys.OrderBy(key => key)))    // Keys have to match.
            return false;
         foreach(var (i,s1) in Scals) {
            τ s2 = v2[i];
            if(!s1.Equals(s2))        // Fetch did not suceed or values are not equal.
               return false; }
         return true; }
      else
         return false;
   }

   
   /// <summary>So that foreach statements work properly.</summary>
   new public IEnumerator<KeyValuePair<int,τ>> GetEnumerator() {
      foreach(var kv in Scals)
         yield return kv;
   }

   public override string ToString() {
      var sb = new StringBuilder(2*Count);
      sb.Append("{");
      foreach(var (i,s) in Scals.OrderBy( kvPair => kvPair.Key )) {
         sb.Append($"{{{i},{s}}}, ");
      }
      sb.Remove(sb.Length - 2, 2);
      sb.Append("}");
      return sb.ToString();
   }
   /// <summary>Converts a sparse Vector to a regular array.</summary>
   public τ[] ToArray() {
      var arr = new τ[Dim];
      foreach(var (i,s) in Scals)
         arr[i] = s;
      return arr;
   }
}
}