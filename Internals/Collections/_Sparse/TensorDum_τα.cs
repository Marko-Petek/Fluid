using System;

using Fluid.Internals.Numerics;

namespace Fluid.Internals.Collections {
   /// <summary>Dummy R1 tensor used inside a R2 tensor's getter.</summary>
   /// <typeparam name="τ"></typeparam>
   public class TensorDum<τ,α> : Tensor<τ,α>
      where τ : IEquatable<τ>, IComparable<τ>, new()
      where α : IArithmetic<τ>, new(){
      /// <summary>Index inside superior tensor.</summary>
      public int Inx { get; set; }
      internal TensorDum(int[] structure, int rank, Tensor<τ,α> sup, int cap, int inx) :
         base(structure, rank, sup, cap) {
            Inx = inx;
      }
      /// <summary>Dimension of dummy tensor's slot is irrelevant.</summary>
      /// <param name="sup">Superior that prompted the creation of this dummy tensor.</param>
      internal TensorDum(Tensor<τ,α> sup, int inx, int cap) : base(sup, cap) {
         Sup = sup;
         Inx = inx;
      }
      /// <summary>Creates a dummy with exactly the same fields as source, but lets you override superior.</summary>
      /// <param name="src"></param>
      /// <param name="inx"></param>
      internal TensorDum(Tensor<τ,α> src, Tensor<τ,α> sup, int inx) : base(src) {
         Sup = sup;
         Inx = inx;
      }
   }
}