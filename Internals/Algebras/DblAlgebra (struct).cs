using System;
using dbl = System.Double;
using Fluid.Internals.Numerics;

namespace Fluid.Internals.Algebras {
public struct DblAlgebra : IAlgebra<double> {
   public double Sum(double first, double second) => first + second;
   public double Sub(double first, double second) => first - second;
   public double Mul(double first, double second) => first*second;
   public double Div(double first, double second) => first/second;
   public double Abs(double val) => Math.Abs(val);
   public double Neg(double val) => -val;
   public double Unit() => 1.0;
   public double Zero()  => 0.0;
   
}
}