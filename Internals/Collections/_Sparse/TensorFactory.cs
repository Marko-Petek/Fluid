using System;
using Fluid.Internals.Numerics;

namespace Fluid.Internals.Collections {

public class TensorFactory<τ,α>
where τ : IEquatable<τ>, new()
where α : IArithmetic<τ>, new() {

   public Tensor<τ,α> CreateTopTensor() {
      
   }
}

}