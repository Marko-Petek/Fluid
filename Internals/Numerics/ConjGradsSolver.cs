using System;
using Fluid.Internals.Collections;

namespace Fluid.Internals.Numerics {
   using Tensor2 = Tensor2<double,DblArithmetic>;
   using SparseRow = SparseRow<double,DblArithmetic>;

   /// <summary>An iterative linear system solver using the method of conjugate gradients. Solves linear systems of form A x = b.</summary>
   public class ConjGradsSolver {
      /// <summary>Left-hand side matrix of A x = b.</summary>
      Tensor2 A { get; }
      /// <summary>Right-hand side vector of A x = b.</summary>
      SparseRow b { get; }

      /// <summary>Create an iterative linear system solver that uses the method of conjugate gradients. Solves linear systems of form A x = b.</summary><param name="matA">Left-hand side matrix of A x = b.</param><param name="rowB">Right-hand side vector of A x = b.</param>
      public ConjGradsSolver(Tensor2 matA, SparseRow rowB) {
         A = matA;
         b = rowB;
      }

      /// <summary>Finds and returns solution of system as a SparseRow.</summary><param name="x0">A SparseRow with same width as solution. Provides starting point in phase space.</param><param name="maxRes">Value of residual at which iterative solution process stops.</param>
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
      public SparseRow Solve(SparseRow x0, double maxRes) {
         int iteration = 0;
         double maxResSqr = maxRes*maxRes;
         var r = new SparseRow[2];
         var d0 = b - A*x0;
         var d = new SparseRow[2] {null, d0};
         var x = new SparseRow[2] {x0, null};
         var rr = new double[2];

         double alfa;
         double beta;
         SparseRow Ad;
         int i = 0;
         int j = 1;
         while(true) {
            r[i] = b - A*x[i];
            d[i] = d[j];
            for(int k = 0; k < b.Width; ++k) {
               ++iteration;
               rr[i] = r[i]*r[i];
               if(rr[i] < maxResSqr)
                  return x[i];
               Ad = A*d[i];
               alfa = rr[i]/(d[i]*Ad);
               x[j] = x[i] + alfa*d[i];
               r[j] = r[i] - alfa*Ad;
               rr[j] = r[j]*r[j];
               beta = rr[j]/rr[i];
               d[j] = r[j] + beta*d[i];
               i = (i + 1) % 2;
               j = (j + 1) % 2; } 
         }



         // double maxResSqr = maxResidual*maxResidual;
         // var r = b - A*x0;
         // var p = new SparseRow(r);
         // var x = new SparseRow(x0);
         // if(r.NormSqr() < maxResidual)
         //    return x;
         // double alfa, rr = r*r, rrNew;
         // SparseRow Ap;
         // while(true) {
         //    for(int j = 0; j < b.Width - 1; ++j) {
         //       ++iteration;
         //       Ap = A*p;
         //       alfa = rr / (p*Ap);
         //       x = x + alfa*p;
         //       r = r - alfa*Ap;
         //       rrNew = r*r;
         //       if(rrNew < maxResSqr)
         //          return x;
         //       p = r + (rrNew/rr)*p;
         //       rr = rrNew;
         //    }
         //    r = b - A*x;
         //    p = new SparseRow(r);
         //    rr = r*r;
         // }
         //return x0;

         
      }
   }
}