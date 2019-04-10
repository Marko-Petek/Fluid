namespace Fluid.Internals.Numerics {
   /// <summary>Performs arithmetic on operands of type T.</summary><typeparam name="T">Operand and result type.</typeparam>
   public interface IArithmetic<T> {
      T Add(T first, T second);
      T Sub(T first, T second);
      T Mul(T first, T second);
      T Div(T first, T second);
   }

   public struct IntArithmetic : IArithmetic<int> {
      public int Add(int first, int second) => first + second;
      public int Sub(int first, int second) => first - second;
      public int Mul(int first, int second) => first*second;
      public int Div(int first, int second) => first/second;
   }

   public struct DblArithmetic : IArithmetic<double> {
      public double Add(double first, double second) => first + second;
      public double Sub(double first, double second) => first - second;
      public double Mul(double first, double second) => first*second;
      public double Div(double first, double second) => first/second;
   }
}