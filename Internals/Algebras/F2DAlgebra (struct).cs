using System;
using dbl = System.Double;
using Fluid.Internals.Numerics;

namespace Fluid.Internals.Algebras {

public struct F2DAlgebra : IAlgebra<F2D?> {
   public F2D? Sum(F2D? f1, F2D? f2) {
      if(f1 == null) {
         if(f2 == null)
            return null;
         else
            return f2; }
      else if(f2 == null)
         return f1;
      return new F2D( (x,y) => f1.F(x,y) + f2.F(x,y) );
   }
   public F2D? Sub(F2D? f1, F2D? f2) {
      if(f1 == null) {
         if(f2 == null)
            return null;
         else
            return new F2D((x,y) => -f2.F(x,y)); }
      else if(f2 == null) {
         return f1; }
      return new F2D( (x,y) => f1.F(x,y) - f2.F(x,y) );
   }
   public F2D? Mul(F2D? f1, F2D? f2) {
      if(f1 == null || f2 == null)
         return null;
      return new F2D( (x,y) => f1.F(x,y) * f2.F(x,y) );
   }
   public F2D? Div(F2D? f1, F2D? f2) {
      if(f1 == null) {
         if(f2 == null)
            throw new DivideByZeroException("Second argument of division cannot be null.");
         else
            return null; }
      else if(f2 == null)
         throw new DivideByZeroException("Second argument of division cannot be null.");
      return new F2D( (x,y) => f1.F(x,y) / f2.F(x,y) );
   }
   public F2D? Abs(F2D? f) {
      if(f == null)
         return null;
      return new F2D( (x,y) => Math.Abs(f.F(x,y)) );
   }
   public F2D? Neg(F2D? f) {
      if(f == null)
         return null;
      return new F2D( (x,y) => -f.F(x,y) );
   }
   public F2D Unit() =>
      F2D.One;
   public F2D? Zero() =>
      null;
      
}
}