using System;
using Fluid.Internals.Collections;

namespace Fluid.Internals.Numerics {
   using SparseMat = SparseMat<double,DblArithmetic>;
   using SparseRow = SparseRow<double,DblArithmetic>;

   /// <summary>An iterative linear system solver using the method of conjugate gradients. Solves linear systems of form A x = b.</summary>
   public class ConjGradsSolver {//TODO1: Test ConjGradSolver.
      /// <summary>Left-hand side matrix of A x = b.</summary>
      SparseMat A { get; }
      /// <summary>Right-hand side vector of A x = b.</summary>
      SparseRow b { get; }

      /// <summary>Create an iterative linear system solver that uses the method of conjugate gradients. Solves linear systems of form A x = b.</summary><param name="parA">Left-hand side matrix of A x = b..</param><param name="parB">Right-hand side vector of A x = b.</param>
      public ConjGradsSolver(SparseMat parA, SparseRow parB) {
         A = parA;
         b = parB;
      }

      /// <summary>Finds and returns solution of system as a SparseRow.</summary><param name="initialGuessRow">A SparseRow with same width as solution. Provides starting point in phase space.</param><param name="maximumResidual">Value of residual at which iterative solution process stops.</param>
      public SparseRow Solve(SparseRow initialGuessRow, double maximumResidual) {
         var d = new SparseRow[2] {
            b - A * initialGuessRow,                      // Difference between intial forcing vector and Ax of initial solution.
            new SparseRow(b.Width, 10) };
         var r = new SparseRow[2] {
            d[0],
            new SparseRow(b.Width, 10) };
         var x = new SparseRow[2] {
            new SparseRow(d[0]),
            new SparseRow(b.Width, 10) };
         double alfa;
         double beta;
         SparseRow ADotDi;
         double riDotRi;
         int i = 0;
         int j = 1;
         do {
            riDotRi = r[i] * r[i];
            ADotDi = A * d[i];
            alfa = riDotRi / (d[i] * ADotDi);
            x[j] = x[i] + alfa * d[i];
            r[j] = r[i] - alfa * ADotDi;
            beta = (r[j] * r[j]) / (riDotRi);
            d[j] = r[j] + beta * d[i];
            i = (i + 1) % 2;
            j = (j + 1) % 2;
         } while(r[i].NormSqr() > maximumResidual);
         return x[i];
      }
   }
}