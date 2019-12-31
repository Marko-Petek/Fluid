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

/// <summary>A vector with specified dimension which holds values of type τ. Those can use arithmetic defined inside type α.</summary>
/// <typeparam name="τ">Type of values.</typeparam>
/// <typeparam name="α">Type defining arithmetic between values.</typeparam>
public partial class Vector<τ,α> : Tensor<τ,α>, IEquatable<Vector<τ,α>>
where τ : struct, IEquatable<τ>, IComparable<τ>
where α : IArithmetic<τ> {
   /// <summary>Incomplete constructor. Init: internal dictionary, rank. NonInit: structure, superior.</summary>
   internal Vector() : base(0) {             // base(0) ensures that minimal memory is wasted for inherited dictionary.
      Vals = new Dictionary<int, τ>();
      Rank = 1;
   }
   /// <summary>Creates a type τ vector with arithmetic α, with specified initial capacity.</summary>
   internal Vector(int cap) : this(Voids.ListInt, Voids<τ,α>.Vec, cap) { }


   internal Vector(List<int> structure, int cap) {

   }

   internal Vector(List<int> structure, Tensor<τ,α> sup, int cap) : base(structure, 1, sup, 0) {
      Vals = new Dictionary<int, τ>(cap);
   }
   internal Vector(Tensor<τ,α> sup, int cap) : this(sup.Structure, sup, cap) { }
   
   /// <summary>Creates a top vector with specified dimension and initial capacity.</summary>
   internal Vector(int dim, int cap) : this(new List<int> {dim}, Voids<τ,α>.Vec, cap) { }
   
   /// <summary>Creates a vector as a deep copy of another. You can optionally specify which meta-fields to copy. Default is AllExceptSup.</summary>
   /// <param name="src"></param>
   internal Vector(Vector<τ,α> src, in CopySpecStruct cs) : base(0) {                 // Capacity of base tensor should be 0.
      Factory<τ,α>.Copy(src, this, cs);
   }
   internal Vector(Vector<τ,α> src) : this(src, CopySpecs.S342_00) { }

   /// <summary>Adds entry to internal dictionary without checking if it is equal to zero.</summary>
   /// <param name="key">Index.</param>
   /// <param name="val">Value.</param>
   internal void Add(int key, τ val) =>
      Vals.Add(key, val);
}

}