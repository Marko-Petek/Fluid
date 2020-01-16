using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using dbl = System.Double;

using Fluid.Internals.Numerics;
using Fluid.TestRef;
namespace Fluid.Internals.Collections {

/// <summary>A vector with specified dimension which holds values of type τ. Those can use arithmetic defined inside type α.</summary>
/// <typeparam name="τ">Type of values.</typeparam>
/// <typeparam name="α">Type defining arithmetic between values.</typeparam>
public partial class Vector<τ,α> : Tensor<τ,α>, IEquatable<Vector<τ,α>>
where τ : IEquatable<τ>, IComparable<τ>, new()
where α : IArithmetic<τ>, new() {
   /// <summary>Void vector.</summary>
   public static readonly Vector<τ,α> V = Factory.TopVector<τ,α>(0,0);
   /// <summary>Constructor with redundancy, used internally.</summary>
   /// <param name="strc">Structure (absorbed).</param>
   /// <param name="sup">Direct superior.</param>
   /// <param name="cap">Capacity of internal dictionary.</param>
   internal Vector(List<int> strc, Tensor<τ,α>? sup, int cap) :
   base(strc, 1, sup, 0) {                                                     // Zero capacity for dictionary holding tensors.
      Vals = new Dictionary<int, τ>(cap);
   }
   /// <summary>Creates a non-top vector with specified superior and initial capacity. Does not add the new vector to its superior or check whether the superior is rank 2.</summary>
   /// <param name="sup">Direct superior.</param>
   /// <param name="cap">Capacity of internal dictionary.</param>
   internal Vector(Tensor<τ,α> sup, int cap) : this(sup.Structure, sup, cap) { }
   
   /// <summary>Creates a top vector (null superior) with specified dimension and initial capacity.</summary>
   /// <param name="dim">Dimension. Number of spots available for values.</param>
   /// <param name="cap">Capacity of internal dictionary.</param>
   internal Vector(int dim, int cap) : this(new List<int> {dim}, null, cap) { }

   /// <summary>Adds entry to internal dictionary without checking if it is equal to zero.</summary>
   /// <param name="key">Index.</param>
   /// <param name="val">Value.</param>
   internal void Add(int key, τ val) =>
      Vals.Add(key, val);

   internal bool TryGetValue(int key, out τ val) =>
      Vals.TryGetValue(key, out val);
}

}