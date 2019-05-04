
using System;
namespace Fluid.Internals.Numerics {
   /// <summary>Performs arithmetic on operands of type T.</summary><typeparam name="T">Operand and result type.</typeparam>
   public interface IArithmetic<T> {
      T Add(T first, T second);
      T Sub(T first, T second);
      T Mul(T first, T second);
      T Div(T first, T second);
      T Abs(T val);
      T Neg(T val);
   }

   public struct IntArithmetic : IArithmetic<int> {
      public int Add(int first, int second) => first + second;
      public int Sub(int first, int second) => first - second;
      public int Mul(int first, int second) => first*second;
      public int Div(int first, int second) => first/second;
      public int Abs(int val) => Math.Abs(val);
      public int Neg(int val) => -val;
   }

   public struct DblArithmetic : IArithmetic<double> {
      public double Add(double first, double second) => first + second;
      public double Sub(double first, double second) => first - second;
      public double Mul(double first, double second) => first*second;
      public double Div(double first, double second) => first/second;
      public double Abs(double val) => Math.Abs(val);
      public double Neg(double val) => -val;
   }

   public struct NoArithmetic : IArithmetic<int> {
      public int Add(int a, int b) => throw new NotImplementedException("You are not supposed to use arithmetic.");
      public int Sub(int a, int b) => throw new NotImplementedException("You are not supposed to use arithmetic.");
      public int Mul(int a, int b) => throw new NotImplementedException("You are not supposed to use arithmetic.");
      public int Div(int a, int b) => throw new NotImplementedException("You are not supposed to use arithmetic.");
      public int Abs(int a) => throw new NotImplementedException("You are not supposed to use arithmetic.");
      public int Neg(int a) => throw new NotImplementedException("You are not supposed to use arithmetic.");
   }
}