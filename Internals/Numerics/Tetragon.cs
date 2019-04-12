using System;
using static System.Math;

using Fluid.Internals.Meshing;
using static Fluid.Internals.Numerics.MatOps;

namespace Fluid.Internals.Numerics {
   /// <summary>A quadrilateral element.</summary>
   public struct Tetragon {
      /// <summary>Lower left vertex position in terms of x and y.</summary>
      public Pos _LL;
      /// <summary>Lower right vertex position in terms of x and y..</summary>
      public Pos _LR;
      /// <summary>Upper right vertex position in terms of x and y..</summary>
      public Pos _UR;
      /// <summary>Upper left vertex position in terms of x and y..</summary>
      public Pos _UL;

      /// <summary>Create an instance which holds Element's vertex positions.</summary><param name="lL">Lower left vertex position.</param><param name="lR">Lower right vertex position.</param><param name="uR">Upper right vertex position.</param><param name="uL">Upper left vertex position.</param>
      public Tetragon(in Pos lL, in Pos lR, in Pos uR, in Pos uL) {
         _LL = lL;
         _LR = lR;
         _UR = uR;
         _UL = uL;
      }
      public Tetragon(double lLX, double lLY, double lRX, double lRY, double uRX, double uRY, double uLX, double uLY) {
         _LL = new Pos(lLX, lLY);
         _LR = new Pos(lRX, lRY);
         _UR = new Pos(uRX, uRY);
         _UL = new Pos(uLX, uLY);
      }

      /// <summary>Calculate ksi and eta coordinates inside element using inverse transformations R and T.</summary><param name="pos">Position in terms of global x and y.</param>
      public Pos ReferenceSquareCoords(in Pos pos) {
         double a = FuncA(in pos);
         double b = FuncB(in pos);
         double c = FuncC(in pos);
         double detMALessMB = Sub(MA(), MB()).Det();
         double detNALessNB = Sub(NA(), NB()).Det();
         double ksi = 0.0;
         double eta = 0.0;
         if(pos.X*pos.Y >= 0) {                                          // Quadrants I and III.
            if(Abs(detMALessMB) > 10E-7)                                    // Opposing sides are not too parallel.
               ksi = (-b + Sqrt(b*b + c)) / detMALessMB;
            else                                                            // Opposing sides are virtually parallel. Use simplified model to preserve precision.
               ksi = SimplifiedKsi(in pos);
            if(Abs(detNALessNB) > 10E-7)                                    // Opposing sides are not too parallel.
               eta = (-a - Sqrt(b*b + c)) / detNALessNB;
            else                                                            // Opposing sides are virtually parallel.
               eta = SimplifiedEta(in pos); }
         else {                                                              // Quadrants II and IV.
            if(Abs(detMALessMB) > 10E-7)                                    // Opposing sides are not too parallel.
               ksi = (-b - Sqrt(b*b + c)) / detMALessMB;
            else                                                            // Opposing sides are virtually parallel. Use simplified model to preserve precision.
               ksi = SimplifiedKsi(in pos);
            if(Abs(detNALessNB) > 10E-7)                                    // Opposing sides are not too parallel.
               eta = (-a + Sqrt(b*b + c)) / detNALessNB;
            else                                                            // Opposing sides are virtually parallel.
               eta = SimplifiedEta(in pos); }
         return new Pos(ksi, eta);
      }
      double FuncA(in Pos pos) =>
         pos.X * MG().Tr() - pos.Y * MF().Tr() + NA().Det() - NB().Det();
      double FuncB(in Pos pos) =>
         pos.X * MG().Tr() - pos.Y * MF().Tr() + MC().Det() + MD().Det();
      double FuncC(in Pos pos) =>
         Sub(MA(), MB()).Det() * (2*pos.X*MH().Tr() - 2*pos.Y*MJ().Tr() + Sum(MA(), MB()).Det());
      // Matrices to compute inverse transformation of specified element.
      double[][] MA() => new double[2][] {new double[2] {_UL.X, _LR.X},
                                          new double[2] {_UL.Y, _LR.Y} };
      double[][] MB() => new double[2][] {new double[2] {_UR.X, _LL.X},
                                          new double[2] {_UR.Y, _LL.Y} };
      double[][] MC() => new double[2][] {new double[2] {_LR.X, _LL.X},
                                          new double[2] {_UL.Y, _UR.Y} };
      double[][] MD() => new double[2][] {new double[2] {_UL.X, _UR.X},
                                          new double[2] {_LR.Y, _LL.Y} };
      double[][] MF() => new double[2][] {new double[2] {_LR.X - _LL.X, 0.0},
                                          new double[2] {0.0 , _UL.X - _UR.X} };
      double[][] MG() => new double[2][] {new double[2] {_LR.Y - _LL.Y, 0.0},
                                          new double[2] {0.0 , _UL.Y - _UR.Y} };
      double[][] MH() => new double[2][] {new double[2] {_UL.Y + _UR.Y, 0.0},
                                          new double[2] {0.0 , -_LR.Y - _LL.Y} };
      double[][] MJ() => new double[2][] {new double[2] {_UL.X + _UR.X, 0.0},
                                          new double[2] {0.0 , -_LR.X - _LL.X} };
      double[][] NA() => new double[2][] {new double[2] {_UL.X, _UR.X},
                                          new double[2] {_UL.Y, _UR.Y} };
      double[][] NB() => new double[2][] {new double[2] {_LL.X, _LR.X},
                                          new double[2] {_LL.Y, _LR.Y} };
      /// <summary>Distance of specified point P to a line going thorugh lower edge.</summary><param name="P">Specified point.</param>
      double DistanceToLowerEdge(in Pos P) {
         var lowerEdgeVector = new Vec2(in _LL, in _LR);    // Vector from lower left to lower right vertex.
         lowerEdgeVector.Normalize();
         var posVector = new Vec2(in _LL, in P);            // Choose a point Q on lower edge: we choose LowerLeft vertex. Then take our specified point P and create a vector.
         return Abs(lowerEdgeVector.Cross(in posVector));       // Take cross product of the two which will give you desired distance.
      }
      /// <summary>Distance of specified point P to a line going thorugh left edge.</summary><param name="P">Specified point.</param>
      double DistanceToLeftEdge(in Pos P) {
         var leftEdgeVector = new Vec2(in _LL, in _UL);     // Vector from lower left to lower right vertex.
         leftEdgeVector.Normalize();
         var posVector = new Vec2(in _LL, in P);            // Choose a point Q on left edge: we choose LowerLeft vertex. Then take our specified point P and create a vector.
         return Abs(leftEdgeVector.Cross(in posVector));       // Take cross product of the two which will give you desired distance.
      }
      double SimplifiedKsi(in Pos pos) {
         double wholeStretchDist = DistanceToLeftEdge(in _LR);      // Distance between parallel edges.
         double posDistance = DistanceToLeftEdge(in pos);           // Distance of pos from left edge.
         return 2.0*(posDistance / wholeStretchDist) - 1.0;          // Transform to [-1,+1] interval.
      }
      double SimplifiedEta(in Pos pos) {
         double wholeStretchDist = DistanceToLowerEdge(in _UL);
         double posDistance = DistanceToLowerEdge(in pos);
         return 2.0*(posDistance / wholeStretchDist) - 1.0;
      }
   }
}