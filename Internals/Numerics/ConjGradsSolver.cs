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
      SparseRow RowB { get; }

      /// <summary>Create an iterative linear system solver that uses the method of conjugate gradients. Solves linear systems of form A x = b.</summary><param name="matA">Left-hand side matrix of A x = b.</param><param name="rowB">Right-hand side vector of A x = b.</param>
      public ConjGradsSolver(SparseMat matA, SparseRow rowB) {
         A = matA;
         RowB = rowB;
      }

      /// <summary>Finds and returns solution of system as a SparseRow.</summary><param name="initGuessRow">A SparseRow with same width as solution. Provides starting point in phase space.</param><param name="maxResidual">Value of residual at which iterative solution process stops.</param>
      // public SparseRow Solve(SparseRow initGuessRow, double maxResidual) {
      //    var r = new SparseRow[2] {
      //       RowB - (MatA * initGuessRow),
      //       new SparseRow(RowB.Width, 10) };
      //    if(r[0].NormSqr() < maxResidual)
      //       return r[0];
      //    var d = new SparseRow[2] {
      //       new SparseRow(r[0]),                      // Difference between intial forcing vector and Ax of initial solution.
      //       new SparseRow(RowB.Width, 10) };
      //    var rowX = new SparseRow[2] {
      //       new SparseRow(initGuessRow),
      //       new SparseRow(RowB.Width, 10) };
      //    double alfa;
      //    double beta;
      //    SparseRow ADotDi;
      //    double riDotRi;
      //    double rjDotRj;
      //    int i = 0;
      //    int j = 1;
      //    do {
      //       riDotRi = r[i]*r[i];
      //       ADotDi = MatA * d[i];
      //       alfa = riDotRi / (d[i]*ADotDi);
      //       rowX[j] = rowX[i] + (alfa*d[i]);
      //       r[j] = r[i] - (alfa*ADotDi);
      //       rjDotRj = r[j]*r[j];
      //       if(r[j].NormSqr() < maxResidual)
      //          break;
      //       //beta = (r[j] * r[j]) / (riDotRi);
      //       d[j] = r[j] + (rjDotRj/riDotRi)*d[i];
      //       i = (i + 1) % 2;
      //       j = (j + 1) % 2;
      //    } while(r[i].NormSqr() > maxResidual);
      //    return rowX[i];
      // }
      public SparseRow Solve(SparseRow initGuessRow, double maxResidual) {
         var r = RowB - (A * initGuessRow);
         if(r.NormSqr() < maxResidual)
            return r;
         var p = new SparseRow(r);
         var x = new SparseRow(initGuessRow);
         double alfa;
         SparseRow APi;
         double riRi = r*r;
         double rjRj;
         while(true) {
            APi = A*p;
            alfa = riRi / (p*APi);
            x = x + alfa*p;
            r = r - alfa*APi;
            rjRj = r*r;
            if(r.NormSqr() < maxResidual)
               break;
            p = r + (rjRj / riRi) * p;
            riRi = rjRj;
         }
         throw new Exception();

         // do {
         //    riRi = r[i]*r[i];
         //    APi = A * p[i];
         //    alfa = riRi / (p[i]*APi);
         //    x[j] = x[i] + (alfa*p[i]);
         //    r[j] = r[i] - (alfa*APi);
         //    rjRj = r[j]*r[j];
         //    if(r[j].NormSqr() < maxResidual)
         //       break;
         //    //beta = (r[j] * r[j]) / (riDotRi);
         //    p[j] = r[j] + (rjRj/riRi)*p[i];
         // } while(r[i].NormSqr() > maxResidual);
         // return x[i];
      }
   }
}