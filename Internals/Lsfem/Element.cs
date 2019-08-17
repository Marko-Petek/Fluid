using System;
using System.Collections.Generic;
using static System.Math;
using Fluid.Internals.Collections;
using My = Fluid.Internals.Collections.Custom;
using Fluid.Internals.Numerics;
using static Fluid.Internals.Numerics.MatOps;
using static Fluid.Internals.Numerics.SerendipityBasis;

namespace Fluid.Internals.Lsfem {
   using dbl = Double;
   using dA = DblArithmetic;
   using Tnr = Tensor<double,DblArithmetic>;
   using Vec = Vector<double,DblArithmetic>;
   // TODO: Add Jacobians storage. Add ab array of elements to each mesh block and also to main mesh.
   // TODO: Implement abstract J(stdInx,p,q) and J(cmtInx,p,q) methods that return Jacobians for each element.

   /// <summary>A quadrilateral element.</summary>
   public class Element {        
      internal static Mesh Mesh;              // TODO: Set static Mesh field on Element in Simulation class.
      /// <summary>12 element nodes. Indexing starts in lower left corner and proceeds CCW.</summary>
      public int[] T { get; }
      // Matrices to compute inverse transformation of specified element.
      readonly dbl[][] MA, MB, MC, MD, MF, MG, MH, MJ, NA, NB;
      public ref Vec2 P(int inx) => ref Mesh.P.E(T[inx]);
      /// <summary>A vector of values at specified local node index.</summary>
      /// <param name="inx">Local node index.</param>
      public Vec V(int inx) => Mesh.U(Vec.Ex, T[inx]);


      /// <summary>Create an instance which holds Element's vertex positions.</summary>
      /// <param name="nodes">12 node indices that define an element.</param>
      public Element(params int[] nodes) {
         T = nodes;
         MA = new dbl[2][] {  new dbl[2] {P(9).X, P(3).X},
                              new dbl[2] {P(9).Y, P(3).Y}  };
         MB = new dbl[2][] {  new dbl[2] {P(6).X, P(0).X},
                              new dbl[2] {P(6).Y, P(0).Y}  };
         MC = new dbl[2][] {  new dbl[2] {P(3).X, P(0).X},
                              new dbl[2] {P(9).Y, P(6).Y}  };
         MD = new dbl[2][] {  new dbl[2] {P(9).X, P(6).X},
                              new dbl[2] {P(3).Y, P(0).Y}  };
         MF = new dbl[2][] {  new dbl[2] {P(3).X - P(0).X, 0.0},
                              new dbl[2] {0.0 , P(9).X - P(6).X} };
         MG = new dbl[2][] {  new dbl[2] {P(3).Y - P(0).Y, 0.0},
                              new dbl[2] {0.0 , P(9).Y - P(6).Y} };
         MH = new dbl[2][] {  new dbl[2] {P(9).Y + P(6).Y, 0.0},
                              new dbl[2] {0.0 , -P(3).Y - P(0).Y}  };
         MJ = new dbl[2][] {  new dbl[2] {P(9).X + P(6).X, 0.0},
                              new dbl[2] {0.0 , -P(3).X - P(0).X}  };
         NA = new dbl[2][] {  new dbl[2] {P(9).X, P(6).X},
                              new dbl[2] {P(9).Y, P(6).Y}  };
         NB = new dbl[2][] {  new dbl[2] {P(0).X, P(3).X},
                              new dbl[2] {P(0).Y, P(3).Y}  };
      }
      
      /// <summary>Calculate the center of element's mass.</summary>
      public Vec2 CenterOfMass() =>
         Vec2.Sum(P(0), P(3), P(6), P(9)) / 4;

      /// <summary>Calculate ksi and eta coordinates inside element using inverse transformations R and T.</summary>
      /// <param name="posX">Position in terms of global x and y.</param>
      public Vec2 Posχ(in Vec2 posX) {
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
         return new Vec2(ξ, η);
      }
      dbl FuncA(in Vec2 pos) =>
         pos.X*MG.Tr<dbl,dA>() - pos.Y*MF.Tr<dbl,dA>() + NA.Det<dbl,dA>() - NB.Det<dbl,dA>();

      dbl FuncB(in Vec2 pos) =>
         pos.X * MG.Tr<dbl,dA>() - pos.Y*MF.Tr<dbl,dA>() + MC.Det<dbl,dA>() + MD.Det<dbl,dA>();

      dbl FuncC(in Vec2 pos) =>
         MA.Sub<dbl,dA>(MB).Det<dbl,dA>()*(2*pos.X*MH.Tr<dbl,dA>() -
            2*pos.Y*MJ.Tr<dbl,dA>() + MA.Sum<dbl,dA>(MB).Det<dbl,dA>());
      /// <summary>Distance of specified point P to a line going through lower edge.</summary>
      /// <param name="pos">Specified point.</param>
      dbl DistToLowerEdge(in Vec2 pos) {
         var lowerEdgeVector = new Vec2(in P(0), in P(3));    // Vector from lower left to lower right vertex.
         lowerEdgeVector.Normalize();
         var posVec = new Vec2(in P(0), in pos);            // Choose a point Q on lower edge: we choose LowerLeft vertex. Then take our specified point P and create a vector.
         return Abs(lowerEdgeVector.Cross(in posVec));       // Take cross product of the two which will give you desired distance.
      }
      /// <summary>Distance of specified point P to a line going through left edge.</summary>
      /// <param name="pos">Specified point.</param>
      dbl DistToLeftEdge(in Vec2 pos) {
         var leftEdgeVec = new Vec2(in P(0), in P(9));     // Vector from lower left to lower right vertex.
         leftEdgeVec.Normalize();
         var posVec = new Vec2(in P(0), in pos);            // Choose a point Q on left edge: we choose LowerLeft vertex. Then take our specified point P and create a vector.
         return Abs(leftEdgeVec.Cross(in posVec));       // Take cross product of the two which will give you desired distance.
      }
      /// <summary>Used when horizontal edges are virtually parallel.</summary>
      /// <param name="pos">Position in terms of x and y.</param>
      dbl Simp_ξ(in Vec2 pos) {
         dbl wholeStretchDist = DistToLeftEdge(in P(3));       // Distance between parallel edges.
         dbl posDist = DistToLeftEdge(in pos);                       // Distance of pos from left edge.
         return 2.0*(posDist/wholeStretchDist) - 1.0;                      // Transform to [-1,+1] interval.
      }
      /// <summary>Used when vertical edges are virtually parallel.</summary>
      /// <param name="pos">Position in terms of x and y.</param>
      dbl Simp_η(in Vec2 pos) {
         dbl wholeStretchDist = DistToLowerEdge(in P(9));
         dbl posDist = DistToLowerEdge(in pos);
         return 2.0*(posDist/wholeStretchDist) - 1.0;
      }
      /// <summary>Returns values of desired variables at specified reference position (ksi, eta) inside element.</summary>
      /// <param name="pos">Position on reference square in terms of (ksi, eta).</param>
      /// <param name="varInxs">Indices of variables whose values we wish to retrieve.</param>
      internal dbl[] Vals(in Vec2 pos, params int[] varInxs) {
         var vals = new dbl[varInxs.Length];
         int varInx;
         for(int i = 0; i < varInxs.Length; ++i) {
            varInx = varInxs[i];
            for(int node = 0; node < 12; ++node)
               vals[i] += V(node)[varInx] * ϕ[0][node](pos.X, pos.Y); }
         return vals;
      }

      // /// <summary>
      // /// Integrand belonging to integral in first part of final variational statement.</summary><param name="ξ">Horizontal coordinate on reference square (from -1 to 1).</param><param name="η">Vertical coordinate on reference square (from -1 to 1).</param><param name="b">Index of first basis function.</param><param name="c">Index of second basis function.</param><param name="p">First index of first Jacobian.</param><param name="a">Index of third basis function.</param><param name="q">First index of second Jacobian.</param><param name="d">Index of fourth basis function.</param>
      // public static double I1(double ξ, double η, int b, int c, int p, int a, int q, int d) {
            
      // }  // TODO: Implement I1.
   }
}