using Xunit;
using System.Threading;
using Fluid.Internals;
using Fluid.Internals.Collections;
using Fluid.Internals.Numerics;
using TB = Fluid.Internals.Toolbox;

namespace Fluid.Tests {
   using SparseMat = SparseMat<double,DblArithmetic>;
   using SparseRow = SparseRow<double,DblArithmetic>;
   using SparseMatInt = SparseMat<int,IntArithmetic>;
   using SparseRowInt = SparseRow<int,IntArithmetic>;
   public partial class Thread2 {

      [InlineData(
          1.0,  0.0, -1.0,    // Left-hand side.
         -2.0,  3.0,  0.0,
          1.0, -3.0,  2.0,

          5.0,  1.0, -10.0,   // Right hand side.
          1.0,  1.0, 1.0,     // Init vector.
          1.0, 1.0, -4.0 )]   // Solution vector.
      [Theory] public void ConjugateGradients(params double[] input) {
         var A = SparseMat.CreateFromArray(input, 6, 0, 3, 0, 3);
         var b = SparseRow.CreateFromArray(input, 9, 3);
         var expSol = SparseRow.CreateFromArray(input, 15, 3);
         var initPoint = SparseRow.CreateFromArray(input, 12, 3);
         var solver = new ConjGradsSolver(A, b);
         var sol = solver.Solve(initPoint, 0.001);
         Assert.True(sol.Equals(expSol, 0.01));
      }
   }
}