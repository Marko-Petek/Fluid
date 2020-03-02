using Xunit;
using Xunit.Abstractions;
using System;
using System.Linq;
using static System.Math;

using Fluid.Internals;
using Fluid.Internals.Collections;
using static Fluid.Internals.Collections.TnrFactory;
using Fluid.Internals.Lsfem;
using Fluid.Internals.Numerics;
using da = Fluid.Internals.Numerics.DblArithmetic;
using ia = Fluid.Internals.Numerics.IntArithmetic;
using Fluid.TestRef;
using static Fluid.Internals.Toolbox;
using Supercluster.KDTree;
namespace Fluid.Tests {
using dbl = Double;
using Tensor = Tnr<double,DblArithmetic>;
using Vector = Vec<double,DblArithmetic>;
using TensorInt = Tnr<int,IntArithmetic>;
using VectorInt = Vec<int,IntArithmetic>;
using Emt = Element;

public class Numerics {

   [InlineData(
      1.0, -1.0,  1.0,    // Left-hand side.
      -1.0,  3.0,  -3.0,
      1.0, -3.0,  2.0,

      5.0,  1.0, -10.0,   // Right hand side.
      0.0,  0.0, 0.0,     // Init vector.
      8.0, 12.0, 9.0 )]   // Solution vector.
   /// <remarks><see cref="TestRefs.ConjGrads3By3"/></remarks>
   [Theory] public void ConjGrads3By3(params double[] input) {
      var A = TopTnrFromSpan<dbl,da>(input.AsSpan<double>(0,9), 3,3);
      var b = TopVecFromSpan<dbl,da>(input.AsSpan<dbl>(9,3));
      var expSol = TopVecFromSpan<dbl,da>(input.AsSpan<dbl>(15,3));
      var initPoint = TopVecFromSpan<dbl,da>(input.AsSpan<dbl>(12,3));
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
      var A = TopTnrFromSpan<dbl,da>(input.AsSpan<dbl>(0,16), 4,4);
      var b = TopVecFromSpan<dbl,da>(input.AsSpan<dbl>(16,4));
      var initPoint = TopVecFromSpan<dbl,da>(input.AsSpan<dbl>(20,4));
      var expSol = TopVecFromSpan<dbl,da>(input.AsSpan<dbl>(24,4));
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
      var K = TopTnrFromSpan<dbl,da>(spanK, strucK);
      arrPos += nElmsInK;
      var spanF = data.Skip(arrPos).Take(nElmsInF).Select(a => (double) a).ToArray().AsSpan();
      var F = TopTnrFromSpan<dbl,da>(spanF, strucF);
      var spanX0 = Enumerable.Repeat(0.0, nElmsInF).ToArray().AsSpan();
      var solver = new ConjGradsSolver(K, F);
      DebugTag = "x0Creation";
      var x0 = TopTnrFromSpan<dbl,da>(spanX0, strucF);
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
   /// <remarks><see cref="TestRefs.GaussQuadrature2D"/></remarks>
   [Fact] public void GaussQuadrature2D() {
      var integrator = new Quadrature(order: 7, dim: 2, r => 100.0*Pow(r[0],12)*Pow(r[1],12)
         + 7*Pow(r[0],7)*Pow(r[1],6) + Pow(r[0],2)*r[1]);
      var result = integrator.Integrate();
      Assert.True(result.Equals(2.36686, 0.001));
   }

   [Fact] public void KDTreeTrial() {
      R("Starting KDTreeTrial.");
      T.Rng.SetRange(0,1);
      var pts = Enumerable.Range(0, 1_000).Select(
         i => new dbl[2] {T.Rng.Dbl, T.Rng.Dbl} ).ToArray();
      var vals = Enumerable.Range(0, 1_000).Select( i => T.Rng.Dbl ).ToArray();
      var kdTree = new KDTree<dbl,dbl>(2, pts, vals, (r1,r2) => Sqrt( Pow(r2[0]-r1[0], 2) + Pow(r2[1]-r1[1], 2) ));
      var nearPts = kdTree.NearestNeighbors(new dbl[] {0.32, 0.21}, 5);
      for(int i = 0; i < 5; ++i) {
         dbl[] pt = nearPts[i].Item1;
         dbl val = nearPts[i].Item2;
         R($"{i}: {pt[0]}, {pt[1]}, {val}", Internals.Development.Reporter.VerbositySettings.Obnoxious);
      }
   }
}
}