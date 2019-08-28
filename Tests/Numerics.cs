using Xunit;
using System;
using System.Linq;
using System.Threading;
using Fluid.Internals;
using Fluid.Internals.Collections;
using Fluid.Internals.Numerics;
using Fluid.TestRef;
using TB = Fluid.Internals.Toolbox;

namespace Fluid.Tests {
   using dbl = Double;
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
      /// <remarks><see cref="TestRefs.ConjGrads3By3"/></remarks>
      [Theory] public void ConjGrads3By3(params double[] input) {
        var A = Tensor.FromFlatSpec(input.AsSpan<double>(0,9), 3,3);
        //.CreateFromArray(input, 6, 0, 3, 0, 3);
        var b = Vector.FromFlatSpec(input.AsSpan<dbl>(9,3));
        //CreateFromArray(input, 9, 3);
        var expSol = Vector.FromFlatSpec(input.AsSpan<dbl>(15,3));
        //CreateFromArray(input, 15, 3);
        var initPoint = Vector.FromFlatSpec(input.AsSpan<dbl>(12,3));
        //Vector.CreateFromArray(input, 12, 3);
        var solver = new ConjGradsSolver(A, b);
        var sol = solver.Solve(initPoint, 0.001);
        Assert.True(sol.Equals(expSol, 0.01));
      }

      [InlineData(
         4.0, 2.0, 7.0, 8.0,              // Left-hand side.
         2.0, 2.0, 4.0, 9.0,
         7.0, 4.0, 3.0, 7.0,
         8.0, 9.0, 7.0, 4.0,

         9.0, 5.0, 7.0, 3.0,              // Right hand side.
         1.0, 1.0, 1.0, 1.0,              // Init vector.
         1.197, -1.365, 0.638, 0.309 )]   // Solution vector.
      /// <remarks><see cref="TestRefs.ConjGrads4By4"/></remarks>
      [Theory] public void ConjGrads4By4(params double[] input) {
        var A = Tensor.FromFlatSpec(input.AsSpan<dbl>(0,16), 4,4);
        var b = Vector.FromFlatSpec(input.AsSpan<dbl>(16,4));
        var initPoint = Vector.FromFlatSpec(input.AsSpan<dbl>(20,4));
        var expSol = Vector.FromFlatSpec(input.AsSpan<dbl>(24,4));
        var solver = new ConjGradsSolver(A, b);
        var sol = solver.Solve(initPoint, 0.001);
        Assert.True(sol.Equals(expSol, 0.01));
      }
      // 1,1  1,2
      // 1,1  2,1
      // 1,1  2,2
      // 1,2  2,1
      // 2,2  1,2
      // 2,2  2,1
      [InlineData(
         4, 2, 2,                                        // Toprank spec: K,U,F
         2,2,2,2,  2,2,  2,2,                            // Dimensions: K, U, F
         4,2, 2,2,  2,8, 4,7,  2,4, 8,7,  2,7,7,4,      // K-spec
         9,7,5,3)]                                      // F-spec
      /// <remarks><see cref="TestRefs.ConjGradsSpecial"/></remarks>
      [Theory] public void ConjGradsSpecial(params int[] data) {
         int   rankK = data[0],                                         // Read top ranks.
               rankU = data[1],
               rankF = data[2];
         int arrPos = 3;
         var strucK = data.Skip(arrPos).Take(rankK).ToArray();          // Read K structure.
         arrPos += rankK;
         var strucU = data.Skip(arrPos).Take(rankU).ToArray();          // Read U structure.
         arrPos += rankU;
         var strucF = data.Skip(arrPos).Take(rankF).ToArray();          // Read F structure.
         arrPos += rankF;
         int nElmsInK = strucK.Aggregate(1, (int total, int val) => total*val);
         int nElmsInU = strucU.Aggregate(1, (int total, int val) => total*val);
         int nElmsInF = strucF.Aggregate(1, (int total, int val) => total*val);
         var spanK = data.Skip(arrPos).Take(nElmsInK).Select(a => (double) a).ToArray().AsSpan();
         var K = Tensor.FromFlatSpec(spanK, strucK);
         arrPos += nElmsInK;
         var spanF = data.Skip(arrPos).Take(nElmsInF).Select(a => (double) a).ToArray().AsSpan();
         var F = Tensor.FromFlatSpec(spanF, strucF);
         var spanX0 = Enumerable.Repeat(0.0, nElmsInF).ToArray().AsSpan();
         var solver = new ConjGradsSolver(K, F);
         TB.DebugTag = "x0Creation";
         var x0 = Tensor.FromFlatSpec(spanX0, strucF);
         var solution = solver.Solve(x0, 0.001);
         Assert.True(true);
      }

      /// <remarks><see cref="TestRefs.GaussQuadrature"/></remarks>
      [Fact] public void GaussQuadrature1() {
         var integrator = new Quadrature(2, 1, x => x[0]*x[0]);    // 1D case.
         var result = integrator.Integrate();
         Assert.True(result.Equals(2.0/3, 0.001));
      }

      /// <remarks><see cref="TestRefs.GaussQuadrature"/></remarks>
      [Fact] public void GaussQuadrature2() {
         var integrator = new Quadrature(2, 2,  x => x[0]*x[0]*x[1]*x[1]);    // 1D case.
         var result = integrator.Integrate();
         Assert.True(result.Equals(4.0/9, 0.001));
      }

      /// <remarks><see cref="TestRefs.GaussQuadrature"/></remarks>
      [Fact] public void GaussQuadrature3() {
         var integrator = new Quadrature(7, 2,  x => Math.Pow(x[0], 12.0) * Math.Pow(x[1], 8.0));    // 1D case.
         var result = integrator.Integrate();
         Assert.True(result.Equals(4.0/117, 0.001));
      }
   }
}