using System;
using dbl = System.Double;
using Fluid.Internals.Numerics;

namespace Fluid.Internals.Algebras {

public struct IntA : IAlgebra<int> {
   public int Sum(int first, int second) => first + second;
   public int Sub(int first, int second) => first - second;
   public int Mul(int first, int second) => first*second;
   public int Div(int first, int second) => first/second;
   public int Abs(int val) => Math.Abs(val);
   public int Neg(int val) => -val;
   public int Unit => 1;
   public int Zero => 0;

}
}