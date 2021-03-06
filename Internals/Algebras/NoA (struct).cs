using System;
using dbl = System.Double;
using Fluid.Internals.Numerics;

namespace Fluid.Internals.Algebras {

public struct NoA : IAlgebra<int> {
   public int Sum(int a, int b) => throw new NotImplementedException("You are not supposed to use algebra.");
   public int Sub(int a, int b) => throw new NotImplementedException("You are not supposed to use algebra.");
   public int Mul(int a, int b) => throw new NotImplementedException("You are not supposed to use algebra.");
   public int Div(int a, int b) => throw new NotImplementedException("You are not supposed to use algebra.");
   public int Abs(int a) => throw new NotImplementedException("You are not supposed to use algebra.");
   public int Neg(int a) => throw new NotImplementedException("You are not supposed to use algebra.");
   public int Unit => throw new NotImplementedException("You are not supposed to use algebra.");
   public int Zero => throw new NotImplementedException("You are not supposed to use algebra.");

   public bool Equal(int x1, int x2) => throw new NotImplementedException("You are not supposed to use algebra.");

   public bool IsZero(int x) => throw new NotImplementedException("You are not supposed to use algebra.");

   public int Compare(int x1, int x2) => throw new NotImplementedException("You are not supposed to use algebra.");

}
}