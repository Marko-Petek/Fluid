using System;
using System.Linq;

namespace Fluid.Internals.Numerics {
   /// <summary>Guass-Legendre quadrature (integrator). Works for hyper-cubes with sides [-1,1].</summary>
   public class GaussQuadrature {
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
      public Func<double[],double> F { get; set; }


      public GaussQuadrature() {}
      public GaussQuadrature(int order, int dim, Func<double[],double> func) : this() {
         Order = order;
         Dim = dim;
         F = func;
      }


      public double Integrate() {
         double res = 0.0;
         int depth = -1;
         double[] cors = new double[Dim];
         
         void Recursion(double[] wa) {    // wa = weight and abscissa.
            ++depth;
            double wgh = 
            if(depth < Dim - 1) {                                        // Re-enter recursion, not all dimensions explored.
               for(int i = 0; i < Order; ++i) {
                  Recursion(WA[Order - 2][i]);
               }
            }
            else {
               for(int i = 0; i < Order; ++i) {
                  res += 
               }
            }
         }
      }
   }
}