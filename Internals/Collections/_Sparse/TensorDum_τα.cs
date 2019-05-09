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
      /// <summary>Dimension of dummy tensor's slot is irrelevant.</summary>
      /// <param name="sup">Superior that prompted the creation of this dummy tensor.</param>
      internal TensorDum(Tensor<τ,α> sup) : base(sup.Rank - 1) {
         Sup = sup;
      }
      internal TensorDum(Tensor<τ,α> src, Tensor<τ,α> sup) : base(src) {
         Sup = sup;
      }
   }
}