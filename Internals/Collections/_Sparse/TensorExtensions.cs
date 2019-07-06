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
      public static Tensor<τ,α> Mul(τ scal, Tensor<τ,α> tnr, in Tensor<τ,α> resSup) {
         return Recursion(in tnr, in resSup);

         Tensor<τ,α> Recursion(in Tensor<τ,α> src, in Tensor<τ,α> sup) {
            var res = new Tensor<τ,α>(sup.Structure, src.Rank, sup, src.Count);            // We copy only meta fields (whereby we copy Structure by value).
            if(src.Rank > 2) {                                       // Subordinates are tensors.
               foreach (var int_tnr in src) {
                  var subTnr = Recursion(int_tnr.Value, res);
                  res.Add(int_tnr.Key, subTnr); } }
            else if(src.Rank == 2) {                                 // Subordinates are vectors.
               foreach (var int_tnr2 in src) {
                  var vec2 = (Vector<τ,α>) int_tnr2.Value;
                  var newVec2 = VectorExtensions<τ,α>.Mul(scal, vec2, sup);
                  res.Add(int_tnr2.Key, newVec2); } }
            else {
               var srcVec = (Vector<τ,α>) src;
               return VectorExtensions<τ,α>.Mul(scal, srcVec, sup); }
            return res;
         }
      }
   }
}
