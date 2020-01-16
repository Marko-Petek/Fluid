// Quadrature specialized for 2D cases.
using System;
using dbl = System.Double;
namespace Fluid.Internals.Numerics {

/// <summary>Guass-Legendre quadrature (integrator). Works for hyper-cubes with sides [-1,1].</summary>
public class Quadrature2D {
   /// <summary>Weights and abscissae for various order Gauss-Legendre Quadratures. [quadrature order i-2, i(2,7)][abscissa j (0,i+2)][weight val,abscissa val]</summary>
   public static double[][][] WA { get; } = new double[6][][] {
      new double[2][] {
         new double[2] {1.0, -0.5773502691896257},
         new double[2] {1.0, 0.5773502691896257} },
      new double[3][] {
         new double[2] {0.8888888888888888, 0.0},
         new double[2] {0.5555555555555556, -0.7745966692414834},
         new double[2] {0.5555555555555556, 0.7745966692414834} },
      new double[4][] {
         new double[2] {0.6521451548625461, -0.3399810435848563},
         new double[2] {0.6521451548625461, 0.3399810435848563},
         new double[2] {0.3478548451374538, -0.8611363115940526},
         new double[2] {0.3478548451374538, 0.8611363115940526} },
      new double[5][] {
         new double[2] {0.5688888888888889, 0.0},
         new double[2] {0.4786286704993665, -0.5384693101056831},
         new double[2] {0.4786286704993665, 0.5384693101056831},
         new double[2] {0.2369268850561891, -0.9061798459386640},
         new double[2] {0.2369268850561891, 0.9061798459386640} },
      new double[6][] {
         new double[2] {0.3607615730481386, 0.6612093864662645},
         new double[2] {0.3607615730481386, -0.6612093864662645},
         new double[2] {0.4679139345726910, -0.2386191860831969},
         new double[2] {0.4679139345726910, 0.2386191860831969},
         new double[2] {0.1713244923791704, -0.9324695142031521},
         new double[2] {0.1713244923791704, 0.9324695142031521} },
      new double[7][] {
         new double[2] {0.4179591836734694, 0.0},
         new double[2] {0.3818300505051189, 0.4058451513773972},
         new double[2] {0.3818300505051189, -0.4058451513773972},
         new double[2] {0.2797053914892766, -0.7415311855993945},
         new double[2] {0.2797053914892766, 0.7415311855993945},
         new double[2] {0.1294849661688697, -0.9491079123427585},
         new double[2] {0.1294849661688697, 0.9491079123427585} }
   };
   /// <summary>Order of Gauss-Legendre quadrature equals the amount of abscissae in 1D.</summary>
   public int Order { get; set; } = 7;
   /// <summary>Dimension of hypercube that defines the integration domain. 1 = line, 2 = square, 3 = cube, etc.</summary>
   public int Dim { get; set; } = 2;
   /// <summary>Function to integrate.</summary>
   public F2D F { get; set; }


   public Quadrature2D(F2D f2d) {
      F = f2d;
   }
   public Quadrature2D(int order, F2D f2d) : this(f2d) {
      Order = order;
   }

   /// <summary>Integrates on reference coordiantes [-1,1] x [-1,1].</summary>
   /// <remarks><see cref="TestRefs.GaussQuadrature"/>
   ///          <see cref="TestRefs.GaussQuadrature2D"/></remarks>
   public double Integrate() {
      double result = 0.0;
      dbl x = 0.0;
      dbl y = 0.0;
      dbl wgh = 1.0;
      for(int i = 0; i < Order; ++i) {
         x = WA[Order - 2][i][1];                       // Must be here.
         dbl newWgh = wgh*WA[Order - 2][i][0];
         for(int j = 0; j < Order; ++j) {
            y = WA[Order - 2][i][1];
            result += newWgh*WA[Order - 2][i][0]*F[x,y]; } }
      return result;
   }
   /// <summary>Order 7 Gaussian Quadrature in 2D.</summary>
   /// <param name="f">Function to integrate over reference square.</param>
   public static double Integrate(F2D f) {
      double result = 0.0;
      dbl x = 0.0;
      dbl y = 0.0;
      dbl wgh = 1.0;
      for(int i = 0; i < 7; ++i) {
         x = WA[5][i][1];                       // Must be here.
         dbl newWgh = wgh*WA[5][i][0];
         for(int j = 0; j < 7; ++j) {
            y = WA[5][i][1];
            result += newWgh*WA[5][i][0]*f[x,y]; } }
      return result;
   }
}
}