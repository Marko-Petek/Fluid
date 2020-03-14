using System;
using Fluid.Internals;
using Fluid.Internals.Collections;
using static Fluid.Internals.Tools;
using dbl = System.Double;
using DA = Fluid.Internals.Collections.DblArithmetic;
namespace Fluid.Internals.Numerics {
using Tnr = Tnr<dbl,DA>;
using Vec = Vec<dbl,DA>;

/// <summary>An iterative linear system solver using the method of conjugate gradients. Solves linear systems of form A x = b.</summary>
public class ConjGradsSolver {
   /// <summary>Left-hand side matrix of A x = b.</summary>
   Tnr A { get; }
   /// <summary>Right-hand side vector of A x = b.</summary>
   Tnr B { get; }

   /// <summary>Create an iterative linear system solver that uses the method of conjugate gradients. Solves linear systems of form A x = b.</summary><param name="tnrA">Left-hand side matrix of A x = b.</param><param name="vecB">Right-hand side vector of A x = b.</param>
   public ConjGradsSolver(Tnr tnrA, Tnr vecB) {
      A = tnrA;
      B = vecB;
   }
   /// <summary>The classic implementation, expecting a rank 2 tensor as left operand and a vector as right operand.</summary>
   /// <param name="x0">Initial guess vector.</param>
   /// <param name="maxRes">Maximum residual. Determines when the solution is good enough.</param>
   /// <remarks><see cref="TestRefs.ConjGrads3By3"/></remarks>
   public Vec? Solve(Vec? x0, double maxRes) {
      int iteration = 0;
      double maxResSqr = maxRes * maxRes;
      var r = new Vec?[2];
      var Ax0 = (Vec?) A.ContractTop(x0, 2, 1); //A * x0;
      var b = (Vec) B;
      var d0 = b.SubTop(Ax0);
      var d = new Vec?[2] { null, d0 };
      var x = new Vec?[2] { x0, null };
      var rr = new double[2];

      double alfa;
      double beta;
      Vec? Ad;
      int i = 0;
      int j = 1;
      while (true) {
         var Ax = (Vec?) A.ContractTop(x[i], 2, 1);
         b = (Vec) B;
         r[i] = b.SubTop(Ax);
         d[i] = d[j];
         for (int k = 0; k < B.Strc[0]; ++k) {
            ++iteration;
            rr[i] = r[i].ContractTop(r[i]);
            if (rr[i] < maxResSqr)
               return x[i];
            Ad = (Vec?) A.ContractTop(d[i], 2, 1);
            alfa = rr[i] / d[i].ContractTop(Ad);
            x[j] = x[i] + d[i].MulTop(alfa);
            r[j] = r[i] - Ad.MulTop(alfa);
            rr[j] = r[j].ContractTop(r[j]);
            beta = rr[j] / rr[i];
            d[j] = r[j] + d[i].MulTop(beta);
            i = (i + 1) % 2;
            j = (j + 1) % 2;
         }
      }
   }
   /// <summary>Special version with a rank 4 tensor as LH operand and a rank 2 tensor as RH operand.</summary>
   /// <param name="x0">Initial guess.</param>
   /// <param name="maxRes">Maximum residual. Determines when the solution is good enough.</param>
   public Tnr? Solve(Tnr? x0, double maxRes) {
      int iteration = 0;
      double maxResSqr = maxRes * maxRes;
      var r = new Tnr?[2];                                   // rank 2
      // T.DebugTag = "BeforeNullContraction";
      // var interRes = A.Contract(x0, 3, 1);
      var Ax0 = A.ContractTop(x0, 3, 1).SelfContractTop(3, 4);       // First contract operates on rank 4 tensor, second self-contract also on rank 4 tensor.
      var d0 = B - Ax0;
      var d = new Tnr?[2] { null, d0 };                      // rank 2
      var x = new Tnr?[2] { x0, null };                      // rank 2
      var rr = new double[2];

      double alfa;
      double beta;
      Tnr? Ad;
      int i = 0;
      int j = 1;
      while (true) {
         r[i] = B - A.ContractTop(x[i], 3, 1).SelfContractTop(3, 4);
         d[i] = d[j];
         for (int k = 0; k < B.Strc[0]*B.Strc[1]; ++k) {
            ++iteration;
            rr[i] = r[i].ContractTop(r[i], 1, 1).SelfContractR2();         // Scalar.
            if (rr[i] < maxResSqr)
               return x[i];
            // T.DebugTag = "OverflowContraction";
            // var AdDebug = A.Contract(d[i], 3, 1);
            Ad = A.ContractTop(d[i], 3, 1).SelfContractTop(3, 4);         // Rank 2.
            var dAd = d[i].ContractTop(Ad, 1, 1).SelfContractR2();         // Scalar.
            alfa = alfa = rr[i] / dAd;
            x[j] = x[i] + alfa * d[i];
            r[j] = r[i] - alfa * Ad;
            rr[j] = r[j].ContractTop(r[j], 1, 1).SelfContractR2();
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