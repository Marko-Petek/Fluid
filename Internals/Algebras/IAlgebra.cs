using System;
using dbl = System.Double;
using Fluid.Internals.Numerics;

namespace Fluid.Internals.Algebras {
   /// <summary>Performs arithmetic on operands of type T.</summary><typeparam name="τ">Operand and result type.</typeparam>
   public interface IAlgebra<τ> {
      τ Sum(τ first, τ second);
      τ Sub(τ first, τ second);
      τ Mul(τ first, τ second);
      τ Div(τ first, τ second);
      τ Abs(τ val);
      τ Neg(τ val);
      τ Unit();
      τ Zero();
   }

   

   

   

   

   

   
}