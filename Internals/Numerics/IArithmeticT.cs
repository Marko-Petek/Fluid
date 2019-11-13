#nullable enable
using System;
using dbl = System.Double;
using F2D = System.Func<double,double,double>;

namespace Fluid.Internals.Numerics {
   /// <summary>Performs arithmetic on operands of type T.</summary><typeparam name="τ">Operand and result type.</typeparam>
   public interface IArithmetic<τ> {
      τ Sum(τ first, τ second);
      τ Sub(τ first, τ second);
      τ Mul(τ first, τ second);
      τ Div(τ first, τ second);
      τ Abs(τ val);
      τ Neg(τ val);
      τ Unit();
      τ Zero();
   }

   public struct IntArithmetic : IArithmetic<int> {
      public int Sum(int first, int second) => first + second;
      public int Sub(int first, int second) => first - second;
      public int Mul(int first, int second) => first*second;
      public int Div(int first, int second) => first/second;
      public int Abs(int val) => Math.Abs(val);
      public int Neg(int val) => -val;
      public int Unit() => 1;
      public int Zero() => 0;
   }

   public struct DblArithmetic : IArithmetic<double> {
      public double Sum(double first, double second) => first + second;
      public double Sub(double first, double second) => first - second;
      public double Mul(double first, double second) => first*second;
      public double Div(double first, double second) => first/second;
      public double Abs(double val) => Math.Abs(val);
      public double Neg(double val) => -val;
      public double Unit() => 1.0;
      public double Zero()  => 0.0;
   }

   public struct Func2DArithmetic : IArithmetic<Func2D> {
      public Func2D Sum(Func2D f1, Func2D f2) =>
         new Func2D( (x,y) => f1.F(x,y) + f2.F(x,y) );
      public Func2D Sub(Func2D f1, Func2D f2) =>
         new Func2D( (x,y) => f1.F(x,y) - f2.F(x,y) );
      public Func2D Mul(Func2D f1, Func2D f2) =>
         new Func2D( (x,y) => f1.F(x,y) * f2.F(x,y) );
      public Func2D Div(Func2D f1, Func2D f2) =>
         new Func2D( (x,y) => f1.F(x,y) / f2.F(x,y) );
      public Func2D Abs(Func2D f) =>
         new Func2D( (x,y) => Math.Abs(f.F(x,y)) );
      public Func2D Neg(Func2D f) =>
         new Func2D( (x,y) => -f.F(x,y) );
      public Func2D Unit() =>
         new Func2D( (x,y) => 1.0 );
      public Func2D Zero() =>
         new Func2D( (x,y) => 0.0 );
   }

   public struct NoArithmetic : IArithmetic<int> {
      public int Sum(int a, int b) => throw new NotImplementedException("You are not supposed to use arithmetic.");
      public int Sub(int a, int b) => throw new NotImplementedException("You are not supposed to use arithmetic.");
      public int Mul(int a, int b) => throw new NotImplementedException("You are not supposed to use arithmetic.");
      public int Div(int a, int b) => throw new NotImplementedException("You are not supposed to use arithmetic.");
      public int Abs(int a) => throw new NotImplementedException("You are not supposed to use arithmetic.");
      public int Neg(int a) => throw new NotImplementedException("You are not supposed to use arithmetic.");
      public int Unit() => throw new NotImplementedException("You are not supposed to use arithmetic.");
      public int Zero() => throw new NotImplementedException("You are not supposed to use arithmetic.");
   }

   public static class O<τ,α> where α : IArithmetic<τ>, new() {
      public static α A { get; } = new α();
   }
}
#nullable restore