using System;
using static System.Math;

using Fluid.Internals.Numerics;
using Fluid.Internals.Meshing;
using static Fluid.Internals.Numerics.MatOps;

namespace Fluid.Internals.Meshing {
   /// <summary>A quadrilateral element.</summary>
   public struct MeshElement {
      public MeshNode[] _Nodes;

      /// <summary>Create an instance which holds Element's vertex positions.</summary><param name="lL">Lower left vertex position.</param><param name="lR">Lower right vertex position.</param><param name="uR">Upper right vertex position.</param><param name="uL">Upper left vertex position.</param>
      public MeshElement(
         ref MeshNode n0, ref MeshNode n1, ref MeshNode n2,
         ref MeshNode n3, ref MeshNode n4, ref MeshNode n5,
         ref MeshNode n6, ref MeshNode n7, ref MeshNode n8,
         ref MeshNode n9, ref MeshNode n10, ref MeshNode n11) {

               _Nodes = new MeshNode[] {
                  n0, n1, n2, n3, n4, n5, n6, n7, n8, n9, n10, n11
               };
      }
      

      /// <summary>Calculate ksi and eta coordinates inside element using inverse transformations R and T.</summary><param name="pos">Position in terms of global x and y.</param>
      public Pos ReferenceSquareCoords(ref Pos pos) {
         double a = FuncA(ref pos);
         double b = FuncB(ref pos);
         double c = FuncC(ref pos);
         double detMALessMB = Sub(MA(), MB()).Det();
         double detNALessNB = Sub(NA(), NB()).Det();
         double ksi = 0.0;
         double eta = 0.0;

         if(pos.X * pos.Y >= 0) {                                          // Quadrants I and III.

               if(Abs(detMALessMB) > 10E-7)                                    // Opposing sides are not too parallel.
                  ksi = (-b + Sqrt(b*b + c)) / detMALessMB;
               else                                                            // Opposing sides are virtually parallel. Use simplified model to preserve precision.
                  ksi = SimplifiedKsi(ref pos);
               
               if(Abs(detNALessNB) > 10E-7)                                    // Opposing sides are not too parallel.
                  eta = (-a - Sqrt(b*b + c)) / detNALessNB;
               else                                                            // Opposing sides are virtually parallel.
                  eta = SimplifiedEta(ref pos);
         }
         else {                                                              // Quadrants II and IV.
               if(Abs(detMALessMB) > 10E-7)                                    // Opposing sides are not too parallel.
                  ksi = (-b - Sqrt(b*b + c)) / detMALessMB;
               else                                                            // Opposing sides are virtually parallel. Use simplified model to preserve precision.
                  ksi = SimplifiedKsi(ref pos);

               if(Abs(detNALessNB) > 10E-7)                                    // Opposing sides are not too parallel.
                  eta = (-a + Sqrt(b*b + c)) / detNALessNB;
               else                                                            // Opposing sides are virtually parallel.
                  eta = SimplifiedEta(ref pos);
         }
         return new Pos(ksi, eta);
      }

      double FuncA(ref Pos pos) =>
         pos.X * MG().Tr() - pos.Y * MF().Tr() + NA().Det() - NB().Det();

      double FuncB(ref Pos pos) =>
         pos.X * MG().Tr() - pos.Y * MF().Tr() + MC().Det() + MD().Det();

      double FuncC(ref Pos pos) =>
         Sub(MA(), MB()).Det() * (2*pos.X*MH().Tr() - 2*pos.Y*MJ().Tr() + Sum(MA(), MB()).Det());

      // Matrices to compute inverse transformation of specified element.
      double[][] MA() => new double[2][] {    new double[2] {_Nodes[9]._Pos.X, _Nodes[3]._Pos.X},
                                             new double[2] {_Nodes[9]._Pos.Y, _Nodes[3]._Pos.Y}  };

      double[][] MB() => new double[2][] {    new double[2] {_Nodes[6]._Pos.X, _Nodes[0]._Pos.X},
                                             new double[2] {_Nodes[6]._Pos.Y, _Nodes[0]._Pos.Y}  };

      double[][] MC() => new double[2][] {    new double[2] {_Nodes[3]._Pos.X, _Nodes[0]._Pos.X},
                                             new double[2] {_Nodes[9]._Pos.Y, _Nodes[6]._Pos.Y}  };

      double[][] MD() => new double[2][] {    new double[2] {_Nodes[9]._Pos.X, _Nodes[6]._Pos.X},
                                             new double[2] {_Nodes[3]._Pos.Y, _Nodes[0]._Pos.Y}  };

      double[][] MF() => new double[2][] {    new double[2] {_Nodes[3]._Pos.X - _Nodes[0]._Pos.X, 0.0},
                                             new double[2] {0.0 , _Nodes[9]._Pos.X - _Nodes[6]._Pos.X}   };

      double[][] MG() => new double[2][] {    new double[2] {_Nodes[3]._Pos.Y - _Nodes[0]._Pos.Y, 0.0},
                                             new double[2] {0.0 , _Nodes[9]._Pos.Y - _Nodes[6]._Pos.Y}   };

      double[][] MH() => new double[2][] {    new double[2] {_Nodes[9]._Pos.Y + _Nodes[6]._Pos.Y, 0.0},
                                             new double[2] {0.0 , -_Nodes[3]._Pos.Y - _Nodes[0]._Pos.Y}  };

      double[][] MJ() => new double[2][] {    new double[2] {_Nodes[9]._Pos.X + _Nodes[6]._Pos.X, 0.0},
                                             new double[2] {0.0 , -_Nodes[3]._Pos.X - _Nodes[0]._Pos.X}  };

      double[][] NA() => new double[2][] {    new double[2] {_Nodes[9]._Pos.X, _Nodes[6]._Pos.X},
                                             new double[2] {_Nodes[9]._Pos.Y, _Nodes[6]._Pos.Y}  };

      double[][] NB() => new double[2][] {    new double[2] {_Nodes[0]._Pos.X, _Nodes[3]._Pos.X},
                                             new double[2] {_Nodes[0]._Pos.Y, _Nodes[3]._Pos.Y}  };

      /// <summary>Distance of specified point P to a line going thorugh lower edge.</summary><param name="P">Specified point.</param>
      double DistanceToLowerEdge(ref Pos P) {
         var lowerEdgeVector = new Vec2(ref _Nodes[0]._Pos, ref _Nodes[3]._Pos);    // Vector from lower left to lower right vertex.
         lowerEdgeVector.Normalize();
         var posVector = new Vec2(ref _Nodes[0]._Pos, ref P);            // Choose a point Q on lower edge: we choose LowerLeft vertex. Then take our specified point P and create a vector.
         return Abs(lowerEdgeVector.Cross(ref posVector));       // Take cross product of the two which will give you desired distance.
      }

      /// <summary>Distance of specified point P to a line going thorugh left edge.</summary><param name="P">Specified point.</param>
      double DistanceToLeftEdge(ref Pos P) {
         var leftEdgeVector = new Vec2(ref _Nodes[0]._Pos, ref _Nodes[9]._Pos);     // Vector from lower left to lower right vertex.
         leftEdgeVector.Normalize();
         var posVector = new Vec2(ref _Nodes[0]._Pos, ref P);            // Choose a point Q on left edge: we choose LowerLeft vertex. Then take our specified point P and create a vector.
         return Abs(leftEdgeVector.Cross(ref posVector));       // Take cross product of the two which will give you desired distance.
      }

      double SimplifiedKsi(ref Pos pos) {
         double wholeStretchDist = DistanceToLeftEdge(ref _Nodes[3]._Pos);       // Distance between parallel edges.
         double posDistance = DistanceToLeftEdge(ref pos);                       // Distance of pos from left edge.
         return 2.0*(posDistance / wholeStretchDist) - 1.0;                      // Transform to [-1,+1] interval.
      }

      double SimplifiedEta(ref Pos pos) {
         double wholeStretchDist = DistanceToLowerEdge(ref _Nodes[9]._Pos);
         double posDistance = DistanceToLowerEdge(ref pos);
         return 2.0*(posDistance / wholeStretchDist) - 1.0;
      }

      /// <summary>Returns values of desired variables at specified reference position (ksi, eta) inside element.</summary><param name="pos">Position on reference square in terms of (ksi, eta).</param><param name="vars">Indices of variables whose values we wish to retrieve.</param>
      public double[] Values(ref Pos pos, params int[] vars) {
         var values = new double[vars.Length];

         for(int var = 0; var < vars.Length; ++var) {
               for(int node = 0; node < 12; ++node) {
                  values[var] += _Nodes[node].Vars[var]._value * Phi[node](pos.X, pos.Y);
               }
         }
         return values;
      }

      /// <summary>Basis functions.</summary>
      public static Func<double,double,double>[] Phi = new Func<double,double,double>[12] {
         (ksi, eta) => 0.125*(-1 + eta)*(1 - ksi)*(-2 + eta + eta*eta + ksi + ksi*ksi),
         (ksi, eta) => 0.125*(1 - eta)*Pow(1 - ksi, 2)*(1 + ksi),
         (ksi, eta) => 0.125*(-1 + eta)*Pow(1 - ksi, 2)*(1 + ksi),
         (ksi, eta) => 0.125*(-1 + eta)*(1 + ksi)*(-2 + eta + eta*eta - ksi + ksi*ksi),
         (ksi, eta) => 0.125*Pow(1 - eta, 2)*(1 + eta)*(1 + ksi),
         (ksi, eta) => 0.125*(1 - eta)*Pow(1 + eta, 2)*(1 + ksi),
         (ksi, eta) => 0.125*(1 + eta)*(-1 - ksi)*(-2 - eta + eta*eta - ksi + ksi*ksi),
         (ksi, eta) => 0.125*(1 + eta)*(1 - ksi)*Pow(1 + ksi, 2),
         (ksi, eta) => 0.125*(1 + eta)*Pow(1 - ksi, 2)*(1 + ksi),
         (ksi, eta) => 0.125*(1 + eta)*(-1 + ksi)*(-2 - eta + eta*eta + ksi + ksi*ksi),
         (ksi, eta) => 0.125*(1 - eta)*Pow(1 + eta, 2)*(1 - ksi),
         (ksi, eta) => 0.125*Pow(1 - eta, 2)*(1 + eta)*(1 - ksi)
      };

      /// <summary>Derivatives of basis functions in x direction.</summary>
      public static Func<double,double,double>[] PhiDx = new Func<double, double, double>[12] {
         (double ksi, double eta) => 0.125*(1 - eta)*(-3 + eta + eta*eta + 3*eta*eta),
         (double ksi, double eta) => 0.125*(1 - eta)*(-1 - 2*ksi + 3*ksi*ksi),
         (double ksi, double eta) => 0.125*(1 - eta)*(1 + ksi)*(1 - 3*ksi),
         (double ksi, double eta) => 0.125*(-1 + eta)*(-3 + eta + eta*eta + 3*ksi*ksi),
         (double ksi, double eta) => 0.125*Pow(1 - eta, 2)*(1 + eta),
         (double ksi, double eta) => 0.125*(1 - eta)*Pow(1 + eta, 2),
         (double ksi, double eta) => 0.125*(-1 - eta)*(-3 - eta + eta*eta + 3*ksi*ksi),
         (double ksi, double eta) => 0.125*(-1 - eta)*(-1 + 2*ksi + 3*ksi*ksi),
         (double ksi, double eta) => 0.125*(1 + eta)*(-1 + ksi)*(1 + 3*ksi),
         (double ksi, double eta) => 0.125*(1 + eta)*(-3 - eta + eta*eta + 3*ksi*ksi),
         (double ksi, double eta) => 0.125*(-1 + eta)*Pow(1 + eta, 2),
         (double ksi, double eta) => 0.125*Pow(1 - eta, 2)*(-1 - eta)
      };

      /// <summary>Derivatives of basis functions in y direction.</summary>
      public static Func<double,double,double>[] PhiDy = new Func<double, double, double>[12] {
         (double ksi, double eta) => 0.125*(1 - ksi)*(-3 + 3*eta*eta + ksi + ksi*ksi),
         (double ksi, double eta) => 0.125*Pow(1 - ksi, 2)*(-1 - ksi),
         (double ksi, double eta) => 0.125*(-1 + ksi)*Pow(1 + ksi, 2),
         (double ksi, double eta) => 0.125*(1 + ksi)*(-3 + 3*eta*eta - ksi + ksi*ksi),
         (double ksi, double eta) => 0.125*(-1 + eta)*(1 + 3*eta)*(1 + ksi),
         (double ksi, double eta) => 0.125*(-1 + 2*eta + 3*eta*eta)*(1 + ksi),
         (double ksi, double eta) => 0.125*(-1 - ksi)*(-3 + 3*eta*eta - ksi + ksi*ksi),
         (double ksi, double eta) => 0.125*(1 - ksi)*Pow(1 + ksi, 2),
         (double ksi, double eta) => 0.125*Pow(1 - ksi, 2)*(1 + ksi),
         (double ksi, double eta) => 0.125*(-1 + ksi)*(-3 + 3*eta*eta + ksi + ksi*ksi),
         (double ksi, double eta) => 0.125*(1 + eta)*(1 - 3*eta)*(1 - ksi),
         (double ksi, double eta) => 0.125*(-1 - 2*eta + 3*eta*eta)*(1 - ksi)
      };

      
   }
}