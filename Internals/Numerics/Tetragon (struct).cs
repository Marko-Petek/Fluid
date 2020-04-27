using System;
using static System.Math;
using Fluid.Internals.Lsfem;
using static Fluid.Internals.Numerics.MatOps;
using Fluid.Internals.Algebras;
namespace Fluid.Internals.Numerics {
using dbl = Double;

/// <summary>A quadrilateral element.</summary>
public struct Tetragon {
   /// <summary>Lower left vertex position in terms of x and y.</summary>
   public Vec2 _LL;
   /// <summary>Lower right vertex position in terms of x and y..</summary>
   public Vec2 _LR;
   /// <summary>Upper right vertex position in terms of x and y..</summary>
   public Vec2 _UR;
   /// <summary>Upper left vertex position in terms of x and y..</summary>
   public Vec2 _UL;
   // Matrices to compute inverse transformation of specified element.
   readonly double[][] MA, MB, MC, MD, MF, MG, MH, MJ, NA, NB;

   /// <summary>Create an instance which holds Element's vertex positions.</summary><param name="ll">Lower left vertex position.</param><param name="lr">Lower right vertex position.</param><param name="ur">Upper right vertex position.</param><param name="ul">Upper left vertex position.</param>
   public Tetragon(in Vec2 ll, in Vec2 lr, in Vec2 ur, in Vec2 ul) {
      _LL = ll;
      _LR = lr;
      _UR = ur;
      _UL = ul;
      MA = new dbl[2][] {  new dbl[2] {_UL.X, _LR.X},
                           new dbl[2] {_UL.Y, _LR.Y} };
      MB = new dbl[2][] {  new dbl[2] {_UR.X, _LL.X},
                           new dbl[2] {_UR.Y, _LL.Y} };
      MC = new dbl[2][] {  new dbl[2] {_LR.X, _LL.X},
                           new dbl[2] {_UL.Y, _UR.Y} };
      MD = new dbl[2][] {  new dbl[2] {_UL.X, _UR.X},
                           new dbl[2] {_LR.Y, _LL.Y} };
      MF = new dbl[2][] {  new dbl[2] {_LR.X - _LL.X, 0.0},
                           new dbl[2] {0.0 , _UL.X - _UR.X} };
      MG = new dbl[2][] {  new dbl[2] {_LR.Y - _LL.Y, 0.0},
                           new dbl[2] {0.0 , _UL.Y - _UR.Y} };
      MH = new dbl[2][] {  new dbl[2] {_UL.Y + _UR.Y, 0.0},
                           new dbl[2] {0.0 , -_LR.Y - _LL.Y} };
      MJ = new dbl[2][] {  new dbl[2] {_UL.X + _UR.X, 0.0},
                           new dbl[2] {0.0 , -_LR.X - _LL.X} };
      NA = new dbl[2][] {  new dbl[2] {_UL.X, _UR.X},
                           new dbl[2] {_UL.Y, _UR.Y} };
      NB = new dbl[2][] {  new dbl[2] {_LL.X, _LR.X},
                           new dbl[2] {_LL.Y, _LR.Y} };
   }
   public Tetragon(dbl llx, dbl lly, dbl lrx, dbl lry, dbl urx, dbl ury, dbl ulx, dbl uly) {
      _LL = new Vec2(llx, lly);
      _LR = new Vec2(lrx, lry);
      _UR = new Vec2(urx, ury);
      _UL = new Vec2(ulx, uly);
      MA = new dbl[2][] {  new dbl[2] {_UL.X, _LR.X},
                           new dbl[2] {_UL.Y, _LR.Y} };
      MB = new dbl[2][] {  new dbl[2] {_UR.X, _LL.X},
                           new dbl[2] {_UR.Y, _LL.Y} };
      MC = new dbl[2][] {  new dbl[2] {_LR.X, _LL.X},
                           new dbl[2] {_UL.Y, _UR.Y} };
      MD = new dbl[2][] {  new dbl[2] {_UL.X, _UR.X},
                           new dbl[2] {_LR.Y, _LL.Y} };
      MF = new dbl[2][] {  new dbl[2] {_LR.X - _LL.X, 0.0},
                           new dbl[2] {0.0 , _UL.X - _UR.X} };
      MG = new dbl[2][] {  new dbl[2] {_LR.Y - _LL.Y, 0.0},
                           new dbl[2] {0.0 , _UL.Y - _UR.Y} };
      MH = new dbl[2][] {  new dbl[2] {_UL.Y + _UR.Y, 0.0},
                           new dbl[2] {0.0 , -_LR.Y - _LL.Y} };
      MJ = new dbl[2][] {  new dbl[2] {_UL.X + _UR.X, 0.0},
                           new dbl[2] {0.0 , -_LR.X - _LL.X} };
      NA = new dbl[2][] {  new dbl[2] {_UL.X, _UR.X},
                           new dbl[2] {_UL.Y, _UR.Y} };
      NB = new dbl[2][] {  new dbl[2] {_LL.X, _LR.X},
                           new dbl[2] {_LL.Y, _LR.Y} };
   }

   /// <summary>Calculate ksi and eta coordinates inside element using inverse transformations R and T.</summary><param name="pos">Position in terms of global x and y.</param>
   public Vec2 RefSquareCoords(in Vec2 pos) {
      dbl a = FuncA(in pos);
      dbl b = FuncB(in pos);
      dbl c = FuncC(in pos);
      dbl detMALessMB = MA.Sub<dbl,DblA>(MB).Det<dbl,DblA>();     //Sub(MA(), MB()).Det();
      dbl detNALessNB = NA.Sub<dbl,DblA>(NB).Det<dbl,DblA>();     //Sub(NA(), NB()).Det();
      dbl ksi, eta;
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
      return new Vec2(ksi, eta);
   }
   double FuncA(in Vec2 pos) =>
      pos.X * MG.Tr<dbl,DblA>() - pos.Y * MF.Tr<dbl,DblA>() + NA.Det<dbl,DblA>() - NB.Det<dbl,DblA>();
   double FuncB(in Vec2 pos) =>
      pos.X * MG.Tr<dbl,DblA>() - pos.Y * MF.Tr<dbl,DblA>() + MC.Det<dbl,DblA>() + MD.Det<dbl,DblA>();
   double FuncC(in Vec2 pos) =>
      MA.Sub<dbl,DblA>(MB).Det<dbl,DblA>()*(2*pos.X*MH.Tr<dbl,DblA>() - 2*pos.Y*MJ.Tr<dbl,DblA>() + MA.Sum<dbl,DblA>(MB).Det<dbl,DblA>());
   /// <summary>Distance of specified point P to a line going thorugh lower edge.</summary><param name="P">Specified point.</param>
   double DistanceToLowerEdge(in Vec2 P) {
      var lowerEdgeVector = new Vec2(in _LL, in _LR);    // Vector from lower left to lower right vertex.
      lowerEdgeVector.Normalize();
      var posVector = new Vec2(in _LL, in P);            // Choose a point Q on lower edge: we choose LowerLeft vertex. Then take our specified point P and create a vector.
      return Abs(lowerEdgeVector.Cross(in posVector));       // Take cross product of the two which will give you desired distance.
   }
   /// <summary>Distance of specified point P to a line going thorugh left edge.</summary><param name="P">Specified point.</param>
   double DistanceToLeftEdge(in Vec2 P) {
      var leftEdgeVector = new Vec2(in _LL, in _UL);     // Vector from lower left to lower right vertex.
      leftEdgeVector.Normalize();
      var posVector = new Vec2(in _LL, in P);            // Choose a point Q on left edge: we choose LowerLeft vertex. Then take our specified point P and create a vector.
      return Abs(leftEdgeVector.Cross(in posVector));       // Take cross product of the two which will give you desired distance.
   }
   double SimplifiedKsi(in Vec2 pos) {
      double wholeStretchDist = DistanceToLeftEdge(in _LR);      // Distance between parallel edges.
      double posDistance = DistanceToLeftEdge(in pos);           // Distance of pos from left edge.
      return 2.0*(posDistance / wholeStretchDist) - 1.0;          // Transform to [-1,+1] interval.
   }
   double SimplifiedEta(in Vec2 pos) {
      double wholeStretchDist = DistanceToLowerEdge(in _UL);
      double posDistance = DistanceToLowerEdge(in pos);
      return 2.0*(posDistance / wholeStretchDist) - 1.0;
   }
}
}