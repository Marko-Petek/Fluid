using System;
using Fluid.Internals.Collections;

namespace Fluid.Internals.Numerics {
   using Tensor = Tensor<double,DblArithmetic>;
   using Vector = Vector<double,DblArithmetic>;

   /// <summary>An iterative linear system solver using the method of conjugate gradients. Solves linear systems of form A x = b.</summary>
   public class ConjGradsSolver {
      /// <summary>Left-hand side matrix of A x = b.</summary>
      Tensor A { get; }
      /// <summary>Right-hand side vector of A x = b.</summary>
      Tensor b { get; }

      /// <summary>Create an iterative linear system solver that uses the method of conjugate gradients. Solves linear systems of form A x = b.</summary><param name="tnrA">Left-hand side matrix of A x = b.</param><param name="vecB">Right-hand side vector of A x = b.</param>
      public ConjGradsSolver(Tensor tnrA, Vector vecB) {
         A = tnrA;
         b = vecB;
      }
      /// <summary>The classic implementation, expecting a rank 2 tensor as left operand and a vector as right operand.</summary>
      /// <param name="x0">Initial guess vector.</param>
      /// <param name="maxRes">Maximum residual. Determines when the solution is good enough.</param>
      public Vector Solve(Vector x0, double maxRes) {
         int iteration = 0;
         double maxResSqr = maxRes * maxRes;
         var r = new Vector[2];
         var d0 = (Vector) b - (Vector) A.Contract(x0, 2, 1);           //A * x0;
         var d = new Vector[2] { null, d0 };
         var x = new Vector[2] { x0, null };
         var rr = new double[2];

         double alfa;
         double beta;
         Vector Ad;
         int i = 0;
         int j = 1;
         while (true) {
            r[i] = (Vector) b - (Vector) A.Contract(x[i], 2, 1);
            d[i] = d[j];
            for (int k = 0; k < b.Structure[0]; ++k) {
               ++iteration;
               rr[i] = r[i] * r[i];
               if (rr[i] < maxResSqr)
                  return x[i];
               Ad = (Vector) A.Contract(d[i], 2, 1);
               alfa = rr[i] / (d[i] * Ad);
               x[j] = x[i] + alfa * d[i];
               r[j] = r[i] - alfa * Ad;
               rr[j] = r[j] * r[j];
               beta = rr[j] / rr[i];
               d[j] = r[j] + beta * d[i];
               i = (i + 1) % 2;
               j = (j + 1) % 2;
            }
         }
      }
      /// <summary>Special version with a rank 4 tensor as LH operand and a rank 2 tensor as RH operand.</summary>
      /// <param name="x0">Initial guess.</param>
      /// <param name="maxRes">Maximum residual. Determines when the solution is good enough.</param>
      public Tensor Solve(Tensor x0, double maxRes) {
         throw new NotImplementedException();
         // int iteration = 0;
         // double maxResSqr = maxRes * maxRes;
         // var r = new Tensor[2];                                   // rank 2
         // var d0 = b - A.Contract(x0, 3, 1).Contract(x0, 3, 2);           // First contract operates on rank 4 tensor, second contract on rank 3 tensor.
         // var d = new Tensor[2] { null, d0 };                      // rank 2
         // var x = new Tensor[2] { x0, null };                      // rank 2
         // var rr = new double[2];

         // double alfa;
         // double beta;
         // Tensor Ad;
         // int i = 0;
         // int j = 1;
         // while (true) {
         //    var intermediateR = A.Contract(x[i], 3, 1);              // This is now a rank 4 tensor.
         //    r[i] = b - intermediateR.Contract(intermediateR, 3, 4);  // Contract last two ranks on intermediate to reach a rank 2 tensor.
         //    d[i] = d[j];
         //    for (int k = 0; k < b.Structure[0]*b.Structure[1]; ++k) {
         //       ++iteration;
         //       var intermediateVec = (Vector) r[i].Contract(r[i], 1, 1);  // Rank 1.
         //       rr[i] = intermediateVec*intermediateVec;                   // Scalar.
         //       if (rr[i] < maxResSqr)
         //          return x[i];
         //       var intermediateAd = A.Contract(d[i], 3, 1);               // Rank 4.
         //       Ad = intermediateAd.Contract(intermediateAd, 3, 4);         // Rank 2.
         //       var intermediateDAd = d[i].Contract(Ad, 1, 1);              // Rank 2.
         //       var dAd = intermediateDAd.Contract(intermediateDAd, 1, 2);  // Scalar.
         //       alfa = rr[i] / dAd;
         //       x[j] = x[i] + alfa * d[i];
         //       r[j] = r[i] - alfa * Ad;
         //       rr[j] = r[j] * r[j];
         //       beta = rr[j] / rr[i];
         //       d[j] = r[j] + beta * d[i];
         //       i = (i + 1) % 2;
         //       j = (j + 1) % 2;
         //    }
         // }
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