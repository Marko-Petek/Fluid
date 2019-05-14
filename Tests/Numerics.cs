using Xunit;
using System;
using System.Threading;
using Fluid.Internals;
using Fluid.Internals.Collections;
using Fluid.Internals.Numerics;
using TB = Fluid.Internals.Toolbox;

namespace Fluid.Tests {
   using Tensor = Tensor<double,DblArithmetic>;
   using Vector = Vector<double,DblArithmetic>;
   using TensorInt = Tensor<int,IntArithmetic>;
   using VectorInt = Vector<int,IntArithmetic>;
   public partial class Thread2 {

      [InlineData(
          1.0, -1.0,  1.0,    // Left-hand side.
         -1.0,  3.0,  -3.0,
          1.0, -3.0,  2.0,

          5.0,  1.0, -10.0,   // Right hand side.
          0.0,  0.0, 0.0,     // Init vector.
          8.0, 12.0, 9.0 )]   // Solution vector.
      [Theory] public void ConjGrads3by3(params double[] input) {
         var A = Tensor.CreateFromArray(input, 6, 0, 3, 0, 3);
         var b = Vector.CreateFromArray(input, 9, 3);
         var expSol = Vector.CreateFromArray(input, 15, 3);
         var initPoint = Vector.CreateFromArray(input, 12, 3);
         var solver = new ConjGradsSolver(A, b);
         var sol = solver.Solve(initPoint, 0.001);
         Assert.True(sol.Equals(expSol, 0.01));
      }

      [InlineData(
          4.0, 2.0, 7.0, 8.0,    // Left-hand side.
          2.0, 2.0, 4.0, 9.0,
          7.0, 4.0, 3.0, 7.0,
          8.0, 9.0, 7.0, 4.0,

          9.0, 5.0, 7.0, 3.0,   // Right hand side.
          1.0, 1.0, 1.0, 1.0,     // Init vector.
          1.197, -1.365, 0.638, 0.309 )]   // Solution vector.
      [Theory] public void ConjGrads4by4(params double[] input) {
         var A = Tensor.CreateFromArray(input, 7, 0, 4, 0, 4);
         var b = Vector.CreateFromArray(input, 16, 4);
         var initPoint = Vector.CreateFromArray(input, 20, 4);
         var expSol = Vector.CreateFromArray(input, 24, 4);
         var solver = new ConjGradsSolver(A, b);
         var sol = solver.Solve(initPoint, 0.001);
         Assert.True(sol.Equals(expSol, 0.01));
      }

      [Fact] public void GaussQuadrature1() {
         var integrator = new GaussQuadrature(2, 1, x => x[0]*x[0]);    // 1D case.
         var result = integrator.Integrate();
         Assert.True(result.Equals(2.0/3, 0.001));
      }

      [Fact] public void GaussQuadrature2() {
         var integrator = new GaussQuadrature(2, 2,  x => x[0]*x[0]*x[1]*x[1]);    // 1D case.
         var result = integrator.Integrate();
         Assert.True(result.Equals(4.0/9, 0.001));
      }

      [Fact] public void GaussQuadrature3() {
         var integrator = new GaussQuadrature(7, 2,  x => Math.Pow(x[0], 12.0) * Math.Pow(x[1], 8.0));    // 1D case.
         var result = integrator.Integrate();
         Assert.True(result.Equals(4.0/117, 0.001));
      }
   }
}