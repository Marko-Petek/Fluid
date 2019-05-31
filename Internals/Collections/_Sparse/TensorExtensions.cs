using System;
using Fluid.Internals.Numerics;

namespace Fluid.Internals.Collections {
   public static class TensorExtensions<τ,α>
   where τ : IEquatable<τ>, IComparable<τ>, new()
   where α : IArithmetic<τ>, new() {
      /// <summary>Multiplies a tensor with scalar and assigns it a specified superior.</summary>
      /// <param name="scal">Scalar.</param>
      /// <param name="tnr">Tensor.</param>
      /// <param name="resSup">Superior tensor the result will be added to.</param>
      public static Tensor<τ,α> ScalMul1(τ scal, Tensor<τ,α> tnr, in Tensor<τ,α> resSup) {
         return Recursion(in tnr, in resSup);

         Tensor<τ,α> Recursion(in Tensor<τ,α> src, in Tensor<τ,α> sup) {
            var res = new Tensor<τ,α>(sup.Structure, src.Rank, sup, src.Count);            // We copy only meta fields (whereby we copy Structure by value).
            if(src.Rank > 2) {                                       // Subordinates are tensors.
               foreach (var int_tnr in src)
                  res.Add(int_tnr.Key, Recursion(int_tnr.Value, res)); }
            else if(src.Rank == 2) {                                 // Subordinates are vectors.
               foreach (var int_tnr2 in src)
                  res.Add(int_tnr2.Key, VectorExtensions<τ,α>.ScalMul1(
                     scal, (Vector<τ,α>) int_tnr2.Value, sup)); }
            else
               return VectorExtensions<τ,α>.ScalMul1(
                     scal, (Vector<τ,α>) src, sup);
            return res;
         }
      }
   }
}
