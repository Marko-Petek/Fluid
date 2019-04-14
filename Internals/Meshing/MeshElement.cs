using System;
using static System.Math;

using Fluid.Internals.Numerics;
using Fluid.Internals.Meshing;
using static Fluid.Internals.Numerics.MatOps;

namespace Fluid.Internals.Meshing {
   using dbl = Double;
   /// <summary>A quadrilateral element.</summary>
   public readonly struct MeshElement {
      /// <summary>Basis functions.</summary>
      public static Func<dbl,dbl,dbl>[] Phi = new Func<dbl,dbl,dbl>[12] {
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
      public static Func<dbl,dbl,dbl>[] PhiDx = new Func<dbl,dbl,dbl>[12] {
         (dbl ksi, dbl eta) => 0.125*(1 - eta)*(-3 + eta + eta*eta + 3*eta*eta),
         (dbl ksi, dbl eta) => 0.125*(1 - eta)*(-1 - 2*ksi + 3*ksi*ksi),
         (dbl ksi, dbl eta) => 0.125*(1 - eta)*(1 + ksi)*(1 - 3*ksi),
         (dbl ksi, dbl eta) => 0.125*(-1 + eta)*(-3 + eta + eta*eta + 3*ksi*ksi),
         (dbl ksi, dbl eta) => 0.125*Pow(1 - eta, 2)*(1 + eta),
         (dbl ksi, dbl eta) => 0.125*(1 - eta)*Pow(1 + eta, 2),
         (dbl ksi, dbl eta) => 0.125*(-1 - eta)*(-3 - eta + eta*eta + 3*ksi*ksi),
         (dbl ksi, dbl eta) => 0.125*(-1 - eta)*(-1 + 2*ksi + 3*ksi*ksi),
         (dbl ksi, dbl eta) => 0.125*(1 + eta)*(-1 + ksi)*(1 + 3*ksi),
         (dbl ksi, dbl eta) => 0.125*(1 + eta)*(-3 - eta + eta*eta + 3*ksi*ksi),
         (dbl ksi, dbl eta) => 0.125*(-1 + eta)*Pow(1 + eta, 2),
         (dbl ksi, dbl eta) => 0.125*Pow(1 - eta, 2)*(-1 - eta)
      };
      /// <summary>Derivatives of basis functions in y direction.</summary>
      public static Func<dbl,dbl,dbl>[] PhiDy = new Func<dbl,dbl,dbl>[12] {
         (dbl ksi, dbl eta) => 0.125*(1 - ksi)*(-3 + 3*eta*eta + ksi + ksi*ksi),
         (dbl ksi, dbl eta) => 0.125*Pow(1 - ksi, 2)*(-1 - ksi),
         (dbl ksi, dbl eta) => 0.125*(-1 + ksi)*Pow(1 + ksi, 2),
         (dbl ksi, dbl eta) => 0.125*(1 + ksi)*(-3 + 3*eta*eta - ksi + ksi*ksi),
         (dbl ksi, dbl eta) => 0.125*(-1 + eta)*(1 + 3*eta)*(1 + ksi),
         (dbl ksi, dbl eta) => 0.125*(-1 + 2*eta + 3*eta*eta)*(1 + ksi),
         (dbl ksi, dbl eta) => 0.125*(-1 - ksi)*(-3 + 3*eta*eta - ksi + ksi*ksi),
         (dbl ksi, dbl eta) => 0.125*(1 - ksi)*Pow(1 + ksi, 2),
         (dbl ksi, dbl eta) => 0.125*Pow(1 - ksi, 2)*(1 + ksi),
         (dbl ksi, dbl eta) => 0.125*(-1 + ksi)*(-3 + 3*eta*eta + ksi + ksi*ksi),
         (dbl ksi, dbl eta) => 0.125*(1 + eta)*(1 - 3*eta)*(1 - ksi),
         (dbl ksi, dbl eta) => 0.125*(-1 - 2*eta + 3*eta*eta)*(1 - ksi)
      };
      public readonly MeshNode[] _Nodes;
      // Matrices to compute inverse transformation of specified element.
      readonly double[][] MA, MB, MC, MD, MF, MG, MH, MJ, NA, NB;


      /// <summary>Create an instance which holds Element's vertex positions.</summary><param name="nodes">12 mesh nodes that define an element.</param>
      public MeshElement(params MeshNode[] nodes) {
         _Nodes = nodes;
         MA = new dbl[2][] {  new dbl[2] {_Nodes[9]._Pos.X, _Nodes[3]._Pos.X},
                                 new dbl[2] {_Nodes[9]._Pos.Y, _Nodes[3]._Pos.Y}  };
         MB = new dbl[2][] {  new dbl[2] {_Nodes[6]._Pos.X, _Nodes[0]._Pos.X},
                                 new dbl[2] {_Nodes[6]._Pos.Y, _Nodes[0]._Pos.Y}  };
         MC = new dbl[2][] {  new dbl[2] {_Nodes[3]._Pos.X, _Nodes[0]._Pos.X},
                                 new dbl[2] {_Nodes[9]._Pos.Y, _Nodes[6]._Pos.Y}  };
         MD = new dbl[2][] {  new dbl[2] {_Nodes[9]._Pos.X, _Nodes[6]._Pos.X},
                                 new dbl[2] {_Nodes[3]._Pos.Y, _Nodes[0]._Pos.Y}  };
         MF = new dbl[2][] {  new dbl[2] {_Nodes[3]._Pos.X - _Nodes[0]._Pos.X, 0.0},
                                 new dbl[2] {0.0 , _Nodes[9]._Pos.X - _Nodes[6]._Pos.X} };
         MG = new dbl[2][] {  new dbl[2] {_Nodes[3]._Pos.Y - _Nodes[0]._Pos.Y, 0.0},
                                 new dbl[2] {0.0 , _Nodes[9]._Pos.Y - _Nodes[6]._Pos.Y} };
         MH = new dbl[2][] {  new dbl[2] {_Nodes[9]._Pos.Y + _Nodes[6]._Pos.Y, 0.0},
                                 new dbl[2] {0.0 , -_Nodes[3]._Pos.Y - _Nodes[0]._Pos.Y}  };
         MJ = new dbl[2][] {  new dbl[2] {_Nodes[9]._Pos.X + _Nodes[6]._Pos.X, 0.0},
                                 new dbl[2] {0.0 , -_Nodes[3]._Pos.X - _Nodes[0]._Pos.X}  };
         NA = new dbl[2][] {  new dbl[2] {_Nodes[9]._Pos.X, _Nodes[6]._Pos.X},
                                 new dbl[2] {_Nodes[9]._Pos.Y, _Nodes[6]._Pos.Y}  };
         NB = new dbl[2][] {  new dbl[2] {_Nodes[0]._Pos.X, _Nodes[3]._Pos.X},
                                 new dbl[2] {_Nodes[0]._Pos.Y, _Nodes[3]._Pos.Y}  };
      }
      

      /// <summary>Calculate ksi and eta coordinates inside element using inverse transformations R and T.</summary><param name="pos">Position in terms of global x and y.</param>
      public Pos RefSquareCoords(in Pos pos) {
         dbl a = FuncA(in pos);
         dbl b = FuncB(in pos);
         dbl c = FuncC(in pos);
         dbl detMALessMB = MA.Sub(MB).Det();
         dbl detNALessNB = NA.Sub(NB).Det();
         dbl ksi = 0.0;
         dbl eta = 0.0;
         if(pos.X*pos.Y >= 0) {                                          // Quadrants I and III.
            if(Abs(detMALessMB) > 10E-7)                                    // Opposing sides are not too parallel.
               ksi = (-b + Sqrt(b*b + c))/detMALessMB;
            else                                                            // Opposing sides are virtually parallel. Use simplified model to preserve precision.
               ksi = SimplifiedKsi(in pos);
            if(Abs(detNALessNB) > 10E-7)                                    // Opposing sides are not too parallel.
               eta = (-a - Sqrt(b*b + c))/detNALessNB;
            else                                                            // Opposing sides are virtually parallel.
               eta = SimplifiedEta(in pos); }
         else {                                                              // Quadrants II and IV.
               if(Abs(detMALessMB) > 10E-7)                                    // Opposing sides are not too parallel.
                  ksi = (-b - Sqrt(b*b + c))/detMALessMB;
               else                                                            // Opposing sides are virtually parallel. Use simplified model to preserve precision.
                  ksi = SimplifiedKsi(in pos);
               if(Abs(detNALessNB) > 10E-7)                                    // Opposing sides are not too parallel.
                  eta = (-a + Sqrt(b*b + c))/detNALessNB;
               else                                                            // Opposing sides are virtually parallel.
                  eta = SimplifiedEta(in pos); }
         return new Pos(ksi, eta);
      }
      double FuncA(in Pos pos) =>
         pos.X*MG.Tr() - pos.Y*MF.Tr() + NA.Det() - NB.Det();

      double FuncB(in Pos pos) =>
         pos.X * MG.Tr() - pos.Y*MF.Tr() + MC.Det() + MD.Det();

      double FuncC(in Pos pos) =>
         MA.Sub(MB).Det()*(2*pos.X*MH.Tr() - 2*pos.Y*MJ.Tr() + MA.Add(MB).Det());      //Sub(MA(), MB()).Det() * (2*pos.X*MH().Tr() - 2*pos.Y*MJ().Tr() + Sum(MA(), MB()).Det());
      /// <summary>Distance of specified point P to a line going thorugh lower edge.</summary><param name="P">Specified point.</param>
      double DistToLowerEdge(in Pos P) {
         var lowerEdgeVector = new Vec2(in _Nodes[0]._Pos, in _Nodes[3]._Pos);    // Vector from lower left to lower right vertex.
         lowerEdgeVector.Normalize();
         var posVec = new Vec2(in _Nodes[0]._Pos, in P);            // Choose a point Q on lower edge: we choose LowerLeft vertex. Then take our specified point P and create a vector.
         return Abs(lowerEdgeVector.Cross(in posVec));       // Take cross product of the two which will give you desired distance.
      }
      /// <summary>Distance of specified point P to a line going thorugh left edge.</summary><param name="P">Specified point.</param>
      double DistToLeftEdge(in Pos P) {
         var leftEdgeVec = new Vec2(in _Nodes[0]._Pos, in _Nodes[9]._Pos);     // Vector from lower left to lower right vertex.
         leftEdgeVec.Normalize();
         var posVec = new Vec2(in _Nodes[0]._Pos, in P);            // Choose a point Q on left edge: we choose LowerLeft vertex. Then take our specified point P and create a vector.
         return Abs(leftEdgeVec.Cross(in posVec));       // Take cross product of the two which will give you desired distance.
      }
      double SimplifiedKsi(in Pos pos) {
         double wholeStretchDist = DistToLeftEdge(in _Nodes[3]._Pos);       // Distance between parallel edges.
         double posDist = DistToLeftEdge(in pos);                       // Distance of pos from left edge.
         return 2.0*(posDist/wholeStretchDist) - 1.0;                      // Transform to [-1,+1] interval.
      }
      double SimplifiedEta(in Pos pos) {
         double wholeStretchDist = DistToLowerEdge(in _Nodes[9]._Pos);
         double posDist = DistToLowerEdge(in pos);
         return 2.0*(posDist/wholeStretchDist) - 1.0;
      }
      /// <summary>Returns values of desired variables at specified reference position (ksi, eta) inside element.</summary><param name="pos">Position on reference square in terms of (ksi, eta).</param><param name="varInxs">Indices of variables whose values we wish to retrieve.</param>
      public double[] Values(in Pos pos, params int[] varInxs) {
         var vals = new double[varInxs.Length];
         for(int varInx = 0; varInx < varInxs.Length; ++varInx)
            for(int nodInx = 0; nodInx < 12; ++nodInx)
               vals[varInx] += _Nodes[nodInx].Vars[varInx].Val*Phi[nodInx](pos.X, pos.Y);
         return vals;
      }
   }
}