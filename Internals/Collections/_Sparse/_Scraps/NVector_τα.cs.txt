using System;
using System.Collections.Generic;
using System.Linq;
using Fluid.Internals.Numerics;
namespace Fluid.Internals.Collections {

/// <summary>A vector which holds values. Further implements IComparable.</summary>
/// <typeparam name="τ">Numeric type.</typeparam>
/// <typeparam name="α">Artihmetic type.</typeparam>
public class NVector<τ,α> : NTensor<τ,α>
where τ : struct, IEquatable<τ>, IComparable<τ>
where α : IArithmetic<τ>, new() {
   public new int Count => CountInternal;
   protected override int CountInternal => Vals.Count;
   public Dictionary<int,τ> Vals { get; internal set; }         // An extra wrapped Dictionary which holds values.


   /// <summary>Void value vector.</summary>
   public static readonly NVector<τ,α> NV = Factory.TopNVector<τ,α>(0,0);
   /// <summary>Constructor with redundancy, used internally.</summary>
   /// <param name="strc">Structure (absorbed).</param>
   /// <param name="sup">Direct superior.</param>
   /// <param name="cap">Capacity of internal dictionary.</param>
   internal NVector(List<int> strc, Tensor<τ,α>? sup, int cap) :
   base(strc, 1, sup, 0)  {                                                         // Zero capacity for dictionary holding tensors.
      Vals = new Dictionary<int, τ>(cap);
   }
   /// <summary>Creates a non-top NVector with specified superior and initial capacity. Does not add the new vector to its superior or check whether the superior is rank 2.</summary>
   /// <param name="sup">Direct superior.</param>
   /// <param name="cap">Capacity of internal dictionary.</param>
   internal NVector(Tensor<τ,α> sup, int cap) : this(sup.Structure, sup, cap) { }
   
   /// <summary>Creates a top NVector (null superior) with specified dimension and initial capacity.</summary>
   /// <param name="dim">Dimension. Number of spots available for values.</param>
   /// <param name="cap">Capacity of internal dictionary.</param>
   internal NVector(int dim, int cap) : this(new List<int> {dim}, null, cap) { }


   /// <remarks> <see cref="TestRefs.VectorIndexer"/> </remarks>
   new public τ this[int i] {
      get {
         Vals.TryGetValue(i, out τ val);
         return val; }
      set {
         if(!value.Equals(default(τ))) {
            Vals[i] = value; }
         else
            Vals.Remove(i); }
   }

   /// <summary>Adds entry to internal dictionary without checking if it is equal to zero.</summary>
   /// <param name="key">Index.</param>
   /// <param name="val">Value.</param>
   internal void Add(int key, τ val) =>
      Vals.Add(key, val);

   internal bool TryGetValue(int key, out τ val) =>
      Vals.TryGetValue(key, out val);

   public bool Equals(NVector<τ,α> vec2, τ eps) {
      if(!Vals.Keys.OrderBy(key => key).SequenceEqual(vec2.Vals.Keys.OrderBy(key => key)))    // Keys have to match.
         return false;
      foreach(var int_val1 in Vals) {
         τ val2 = vec2[int_val1.Key];
         if(O<τ,α>.A.Abs(O<τ,α>.A.Sub(int_val1.Value, val2)).CompareTo(eps) > 0 ) // Values do not agree within tolerance.
            return false; }
      return true;                                                              // All values agree within tolerance.
   }
}


}