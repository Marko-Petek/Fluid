using System;
using static System.Math;

using Fluid.Internals.Collections;
using Fluid.Internals.Numerics;
using static Fluid.Internals.Numerics.MatOps;
using static Fluid.Internals.Numerics.SerendipityBasis;

namespace Fluid.Internals.Lsfem {
   using dbl = Double;
   using dA = DblArithmetic;
   using Tensor = Tensor<double,DblArithmetic>;
   // TODO: Add Jacobians storage. Add ab array of elements to each mesh block and also to main mesh.

   /// <summary>A quadrilateral element.</summary>
   public class Element {
      /// <summary>12 element nodes. Indexing starts in lower left corner and proceeds CCW.</summary>
      public MeshNode[] P { get; }
      //public double
      // Matrices to compute inverse transformation of specified element.
      readonly dbl[][] MA, MB, MC, MD, MF, MG, MH, MJ, NA, NB;


      /// <summary>Create an instance which holds Element's vertex positions.</summary>
      /// <param name="nodes">12 mesh nodes that define an element.</param>
      public Element(params MeshNode[] nodes) {
         P = nodes;
         MA = new dbl[2][] {  new dbl[2] {P[9].Pos.X, P[3].Pos.X},
                              new dbl[2] {P[9].Pos.Y, P[3].Pos.Y}  };
         MB = new dbl[2][] {  new dbl[2] {P[6].Pos.X, P[0].Pos.X},
                              new dbl[2] {P[6].Pos.Y, P[0].Pos.Y}  };
         MC = new dbl[2][] {  new dbl[2] {P[3].Pos.X, P[0].Pos.X},
                              new dbl[2] {P[9].Pos.Y, P[6].Pos.Y}  };
         MD = new dbl[2][] {  new dbl[2] {P[9].Pos.X, P[6].Pos.X},
                              new dbl[2] {P[3].Pos.Y, P[0].Pos.Y}  };
         MF = new dbl[2][] {  new dbl[2] {P[3].Pos.X - P[0].Pos.X, 0.0},
                              new dbl[2] {0.0 , P[9].Pos.X - P[6].Pos.X} };
         MG = new dbl[2][] {  new dbl[2] {P[3].Pos.Y - P[0].Pos.Y, 0.0},
                              new dbl[2] {0.0 , P[9].Pos.Y - P[6].Pos.Y} };
         MH = new dbl[2][] {  new dbl[2] {P[9].Pos.Y + P[6].Pos.Y, 0.0},
                              new dbl[2] {0.0 , -P[3].Pos.Y - P[0].Pos.Y}  };
         MJ = new dbl[2][] {  new dbl[2] {P[9].Pos.X + P[6].Pos.X, 0.0},
                              new dbl[2] {0.0 , -P[3].Pos.X - P[0].Pos.X}  };
         NA = new dbl[2][] {  new dbl[2] {P[9].Pos.X, P[6].Pos.X},
                              new dbl[2] {P[9].Pos.Y, P[6].Pos.Y}  };
         NB = new dbl[2][] {  new dbl[2] {P[0].Pos.X, P[3].Pos.X},
                                 new dbl[2] {P[0].Pos.Y, P[3].Pos.Y}  };
      }
      

      /// <summary>Calculate ksi and eta coordinates inside element using inverse transformations R and T.</summary>
      /// <param name="posX">Position in terms of global x and y.</param>
      public Pos Posχ(in Pos posX) {
         dbl a = FuncA(in posX);
         dbl b = FuncB(in posX);
         dbl c = FuncC(in posX);
         dbl detMALessMB = MA.Sub<dbl,dA>(MB).Det<dbl,dA>();
         dbl detNALessNB = NA.Sub<dbl,dA>(NB).Det<dbl,dA>();  // χ
         dbl ξ;
         dbl η;
         if(posX.X*posX.Y >= 0) {                                          // Quadrants I and III.
            if(Abs(detMALessMB) > 10E-7)                                    // Opposing sides are not too parallel.
               ξ = (-b + Sqrt(b*b + c))/detMALessMB;
            else                                                            // Opposing sides are virtually parallel. Use simplified model to preserve precision.
               ξ = Simp_ξ(in posX);
            if(Abs(detNALessNB) > 10E-7)                                    // Opposing sides are not too parallel.
               η = (-a - Sqrt(b*b + c))/detNALessNB;
            else                                                            // Opposing sides are virtually parallel.
               η = Simp_η(in posX); }
         else {                                                              // Quadrants II and IV.
               if(Abs(detMALessMB) > 10E-7)                                    // Opposing sides are not too parallel.
                  ξ = (-b - Sqrt(b*b + c))/detMALessMB;
               else                                                            // Opposing sides are virtually parallel. Use simplified model to preserve precision.
                  ξ = Simp_ξ(in posX);
               if(Abs(detNALessNB) > 10E-7)                                    // Opposing sides are not too parallel.
                  η = (-a + Sqrt(b*b + c))/detNALessNB;
               else                                                            // Opposing sides are virtually parallel.
                  η = Simp_η(in posX); }
         return new Pos(ξ, η);
      }
      dbl FuncA(in Pos pos) =>
         pos.X*MG.Tr<dbl,dA>() - pos.Y*MF.Tr<dbl,dA>() + NA.Det<dbl,dA>() - NB.Det<dbl,dA>();

      dbl FuncB(in Pos pos) =>
         pos.X * MG.Tr<dbl,dA>() - pos.Y*MF.Tr<dbl,dA>() + MC.Det<dbl,dA>() + MD.Det<dbl,dA>();

      dbl FuncC(in Pos pos) =>
         MA.Sub<dbl,dA>(MB).Det<dbl,dA>()*(2*pos.X*MH.Tr<dbl,dA>() -
            2*pos.Y*MJ.Tr<dbl,dA>() + MA.Sum<dbl,dA>(MB).Det<dbl,dA>());
      /// <summary>Distance of specified point P to a line going through lower edge.</summary>
      /// <param name="P">Specified point.</param>
      dbl DistToLowerEdge(in Pos P) {
         var lowerEdgeVector = new Vec2(in this.P[0].Pos, in this.P[3].Pos);    // Vector from lower left to lower right vertex.
         lowerEdgeVector.Normalize();
         var posVec = new Vec2(in this.P[0].Pos, in P);            // Choose a point Q on lower edge: we choose LowerLeft vertex. Then take our specified point P and create a vector.
         return Abs(lowerEdgeVector.Cross(in posVec));       // Take cross product of the two which will give you desired distance.
      }
      /// <summary>Distance of specified point P to a line going through left edge.</summary>
      /// <param name="P">Specified point.</param>
      dbl DistToLeftEdge(in Pos P) {
         var leftEdgeVec = new Vec2(in this.P[0].Pos, in this.P[9].Pos);     // Vector from lower left to lower right vertex.
         leftEdgeVec.Normalize();
         var posVec = new Vec2(in this.P[0].Pos, in P);            // Choose a point Q on left edge: we choose LowerLeft vertex. Then take our specified point P and create a vector.
         return Abs(leftEdgeVec.Cross(in posVec));       // Take cross product of the two which will give you desired distance.
      }
      /// <summary>Used when horizontal edges are virtually parallel.</summary>
      /// <param name="pos">Position in terms of x and y.</param>
      dbl Simp_ξ(in Pos pos) {
         dbl wholeStretchDist = DistToLeftEdge(in P[3].Pos);       // Distance between parallel edges.
         dbl posDist = DistToLeftEdge(in pos);                       // Distance of pos from left edge.
         return 2.0*(posDist/wholeStretchDist) - 1.0;                      // Transform to [-1,+1] interval.
      }
      /// <summary>Used when vertical edges are virtually parallel.</summary>
      /// <param name="pos">Position in terms of x and y.</param>
      dbl Simp_η(in Pos pos) {
         dbl wholeStretchDist = DistToLowerEdge(in P[9].Pos);
         dbl posDist = DistToLowerEdge(in pos);
         return 2.0*(posDist/wholeStretchDist) - 1.0;
      }
      /// <summary>Returns values of desired variables at specified reference position (ksi, eta) inside element.</summary>
      /// <param name="pos">Position on reference square in terms of (ksi, eta).</param>
      /// <param name="varInxs">Indices of variables whose values we wish to retrieve.</param>
      public dbl[] Vals(in Pos pos, params int[] varInxs) {
         var vals = new dbl[varInxs.Length];
         for(int varInx = 0; varInx < varInxs.Length; ++varInx)
            for(int nodInx = 0; nodInx < 12; ++nodInx)
               vals[varInx] += P[nodInx].Vars[varInx].Val*ϕ[0][nodInx](pos.X, pos.Y);
         return vals;
      }

      //public static 

      /// <summary>
      /// Integrand belonging to integral in first part of final variational statement.</summary><param name="ξ">Horizontal coordinate on reference square (from -1 to 1).</param><param name="η">Vertical coordinate on reference square (from -1 to 1).</param><param name="b">Index of first basis function.</param><param name="c">Index of second basis function.</param><param name="p">First index of first Jacobian.</param><param name="a">Index of third basis function.</param><param name="q">First index of second Jacobian.</param><param name="d">Index of fourth basis function.</param>
      // public static double I1(double ξ, double η, int b, int c, int p, int a, int q, int d) {
            
      // }  // TODO: Implement I1.
   }
}