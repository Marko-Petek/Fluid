using System;
using System.Collections.Generic;
using Fluid.Internals.Numerics;

namespace Fluid.Internals.Collections {

/// <summary>A type responsible for Vector creation.</summary>
/// <typeparam name="τ">Value types contained inside the vector.</typeparam>
/// <typeparam name="α">Arithmetic type.</typeparam>
public static class VectorFactory<τ,α>
where τ : IEquatable<τ>, new()
where α : IArithmetic<τ>, new() {

   /// <summary>Creates a vector with specified dimension and initial capacity.</summary>
   /// <param name="dim">Dimension.</param>
   /// <param name="cap">Initial capacity.</param>
   public static Vector<τ,α> CreateVector(int dim, int cap) =>
      new Vector<τ,α>(new List<int> {dim}, Voids<τ,α>.Vec, cap);

   /// <summary>Creates a deep copy of a vector. You have to provide the already instantiated target.</summary>
   /// <param name="src">Copy source.</param>
   /// <param name="tgt">Copy target.</param>
   public static void Copy(Vector<τ,α> src, Vector<τ,α> tgt, in CopySpecStruct cs) {
      CopyMetaFields(src, tgt, cs.NonValueFieldsSpec, cs.StructureSpec);               // Structure created here.
      switch (cs.FieldsSpec & WhichFields.OnlyValues) {
         case WhichFields.OnlyValues:
            tgt.Vals = new Dictionary<int,τ>(src.Count + cs.ExtraCapacity);
            foreach(var int_val in src.Vals) {
               tgt.Vals.Add(int_val.Key, int_val.Value); } break;
         default:
            tgt.Vals = new Dictionary<int,τ>(); break; }
   }



   // public Vector<τ,α> CreateVector(int cap)

   // /// <summary>Creates a type τ vector with arithmetic α, with specified initial capacity.</summary>
   // public Vector(int cap) : this(Voids.ListInt, Voids<τ,α>.Vec, cap) { }
}

}