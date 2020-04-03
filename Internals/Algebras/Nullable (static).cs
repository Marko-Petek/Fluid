using System;
using dbl = System.Double;
using Fluid.Internals.Numerics;

namespace Fluid.Internals.Algebras {

public static class Nullable<τ,α>   where τ : class  where α : IAlgebra<τ?>, new() {
   public static α O { get; } = new α();

}
}