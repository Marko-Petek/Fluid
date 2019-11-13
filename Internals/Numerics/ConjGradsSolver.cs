#nullable enable
using System;
using Fluid.Internals;
using Fluid.Internals.Collections;
using TB = Fluid.Internals.Toolbox;
using dbl = System.Double;
using DA = Fluid.Internals.Numerics.DblArithmetic;

namespace Fluid.Internals.Numerics {
   using Tnr = Tensor<dbl,DA>;
   using Vec = Vector<dbl,DA>;
   using V = Voids<dbl,DA>;

   /// <summary>An iterative linear system solver using the method of conjugate gradients. Solves linear systems of form A x = b.</summary>
   public class ConjGradsSolver {
      /// <summary>Left-hand side matrix of A x = b.</summary>
      Tnr A { get; }
      /// <summary>Right-hand side vector of A x = b.</summary>
      Tnr b { get; }

      /// <summary>Create an iterative linear system solver that uses the method of conjugate gradients. Solves linear systems of form A x = b.</summary><param name="tnrA">Left-hand side matrix of A x = b.</param><param name="vecB">Right-hand side vector of A x = b.</param>
      public ConjGradsSolver(Tnr tnrA, Tnr vecB) {
         A = tnrA;
         b = vecB;
      }
      /// <summary>The classic implementation, expecting a rank 2 tensor as left operand and a vector as right operand.</summary>
      /// <param name="x0">Initial guess vector.</param>
      /// <param name="maxRes">Maximum residual. Determines when the solution is good enough.</param>
      /// <remarks><see cref="TestRefs.ConjGrads3By3"/></remarks>
      public Vec Solve(Vec x0, double maxRes) {
         int iteration = 0;
         double maxResSqr = maxRes * maxRes;
         var r = new Vec[2];
         var d0 = (Vec) b - (Vec) Tnr.Contract(A, x0, 2, 1);           //A * x0;
         var d = new Vec[2] { V.Vec, d0 };
         var x = new Vec[2] { x0, V.Vec };
         var rr = new double[2];

         double alfa;
         double beta;
         Vec Ad;
         int i = 0;
         int j = 1;
         while (true) {
            r[i] = (Vec) b - (Vec) Tnr.Contract(A, x[i], 2, 1);
            d[i] = d[j];
            for (int k = 0; k < b.Structure[0]; ++k) {
               ++iteration;
               rr[i] = r[i] * r[i];
               if (rr[i] < maxResSqr)
                  return x[i];
               Ad = (Vec) Tnr.Contract(A, d[i], 2, 1);
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
      public Tnr Solve(Tnr x0, double maxRes) {
         //throw new NotImplementedException();
         int iteration = 0;
         double maxResSqr = maxRes * maxRes;
         var r = new Tnr[2];                                   // rank 2
         // TB.DebugTag = "BeforeNullContraction";
         // var interRes = A.Contract(x0, 3, 1);
         var Ax0 = Tnr.Contract(A, x0, 3, 1).SelfContract(3, 4);       // First contract operates on rank 4 tensor, second self-contract also on rank 4 tensor.
         var d0 = b - Ax0;
         var d = new Tnr[2] { V.Vec, d0 };                      // rank 2
         var x = new Tnr[2] { x0, V.Vec };                      // rank 2
         var rr = new double[2];

         double alfa;
         double beta;
         Tnr Ad;
         int i = 0;
         int j = 1;
         while (true) {
            r[i] = b - Tnr.Contract(A, x[i], 3, 1).SelfContract(3, 4);
            d[i] = d[j];
            for (int k = 0; k < b.Structure[0]*b.Structure[1]; ++k) {
               ++iteration;
               rr[i] = Tnr.Contract(r[i], r[i], 1, 1).SelfContractR2();         // Scalar.
               if (rr[i] < maxResSqr)
                  return x[i];
               // TB.DebugTag = "OverflowContraction";
               // var AdDebug = A.Contract(d[i], 3, 1);
               Ad = Tnr.Contract(A, d[i], 3, 1).SelfContract(3, 4);         // Rank 2.
               var dAd = Tnr.Contract(d[i], Ad, 1, 1).SelfContractR2();         // Scalar.
               alfa = alfa = rr[i] / dAd;
               x[j] = x[i] + alfa * d[i];
               r[j] = r[i] - alfa * Ad;
               rr[j] = Tnr.Contract(r[j], r[j], 1, 1).SelfContractR2();
               beta = rr[j] / rr[i];
               d[j] = r[j] + beta * d[i];
               i = (i + 1) % 2;
               j = (j + 1) % 2;
            }
         }
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
#nullable restore