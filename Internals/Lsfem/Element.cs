using System;
using System.Collections.Generic;
using static System.Math;
using dbl = System.Double;
using DA = Fluid.Internals.Numerics.DblArithmetic;
using F2DA = Fluid.Internals.Numerics.Func2DArithmetic;

using Fluid.Internals.Collections;
using Fluid.Internals.Numerics;
using static Fluid.Internals.Lsfem.Simulation;
using static Fluid.Internals.Lsfem.Mesh;
using static Fluid.Internals.Numerics.MatOps;
using static Fluid.Internals.Numerics.SerendipityBasis;

namespace Fluid.Internals.Lsfem {
   using Tnr = Tensor<dbl,DA>;
   using SymTnr = SymTensor<dbl,DA>;
   using Vec = Vector<dbl,DA>;
   using FTnr = Tensor<Func2D,F2DA>;
   using static Fluid.Internals.Numerics.O<Func2D,F2DA>;

   /// <summary>A quadrilateral element.</summary>
   public class Element {
      /// <summary>12 element nodes. Indexing starts in lower left corner and proceeds CCW.</summary>
      public int[] P { get; }
      /// <summary>Quadruple overlap integrals ---- Rank 4 ---- ((α,p), (β,q), (γ,r), (δ,s)) ----  (36,36,36,36).</summary>
      public SymTnr Q { get; internal set; }
      /// <summary>Triple overlap integrals ---- Rank 3 ---- ((α,p), (γ,q), (η,s)) ---- (36,36,36).</summary>
      public SymTnr T { get; internal set; }
      /// <summary>A 3x3 inverse of Jacobian of transformation which takes us from reference square to element.</summary>
      public FTnr InvJ { get; private set; }
      /// <summary>Determinant of Jacobian of transformation which takes us from reference square to element.</summary>
      public Func2D DetJ { get; private set; }
      // Matrices to compute inverse transformation of specified element.
      readonly dbl[][] MA, MB, MC, MD, MF, MG, MH, MJ, NA, NB;
      public ref Vec2 Pos(int inx) => ref Msh.Pos.E(P[inx]);
      /// <summary>A vector of values at specified local node index.</summary>
      /// <param name="inx">Local node index.</param>
      public Vec Vals(int inx) => Sim.U(Vec.Ex, P[inx]);


      /// <summary>Create an instance which holds Element's vertex positions.</summary>
      /// <param name="nodes">12 node indices that define an element.</param>
      public Element(params int[] nodes) {
         P = nodes;
         var interm = CalcDetJ();
         DetJ = interm.detJ;
         InvJ = CalcInvJ(interm);
         var tnrFactor = FTnr.Contract(InvJ, ϕ, 2, 2);
         Q = CalcQuadOverlaps(tnrFactor);
         T = CalcTripOverlaps(tnrFactor);
         MA = new dbl[2][] {  new dbl[2] {Pos(9).X, Pos(3).X},
                              new dbl[2] {Pos(9).Y, Pos(3).Y}  };
         MB = new dbl[2][] {  new dbl[2] {Pos(6).X, Pos(0).X},
                              new dbl[2] {Pos(6).Y, Pos(0).Y}  };
         MC = new dbl[2][] {  new dbl[2] {Pos(3).X, Pos(0).X},
                              new dbl[2] {Pos(9).Y, Pos(6).Y}  };
         MD = new dbl[2][] {  new dbl[2] {Pos(9).X, Pos(6).X},
                              new dbl[2] {Pos(3).Y, Pos(0).Y}  };
         MF = new dbl[2][] {  new dbl[2] {Pos(3).X - Pos(0).X, 0.0},
                              new dbl[2] {0.0 , Pos(9).X - Pos(6).X} };
         MG = new dbl[2][] {  new dbl[2] {Pos(3).Y - Pos(0).Y, 0.0},
                              new dbl[2] {0.0 , Pos(9).Y - Pos(6).Y} };
         MH = new dbl[2][] {  new dbl[2] {Pos(9).Y + Pos(6).Y, 0.0},
                              new dbl[2] {0.0 , -Pos(3).Y - Pos(0).Y}  };
         MJ = new dbl[2][] {  new dbl[2] {Pos(9).X + Pos(6).X, 0.0},
                              new dbl[2] {0.0 , -Pos(3).X - Pos(0).X}  };
         NA = new dbl[2][] {  new dbl[2] {Pos(9).X, Pos(6).X},
                              new dbl[2] {Pos(9).Y, Pos(6).Y}  };
         NB = new dbl[2][] {  new dbl[2] {Pos(0).X, Pos(3).X},
                              new dbl[2] {Pos(0).Y, Pos(3).Y}  };
      }
      /// <summary>Calculate the extended inverse Jacobian for this element.</summary>
      protected FTnr CalcInvJ( (Func2D detJ, Func2D J11,
      Func2D J12, Func2D J21, Func2D J22) tuple) {
         var invJ = new FTnr(new List<int>(2) {3,3}, 3);
         (var detJ, var J11, var J12, var J21, var J22) = tuple;
         invJ[0,0] = new Func2D();
         invJ[1,1] = new Func2D( (x,y) => J22.F(x,y) / detJ.F(x,y) );
         invJ[1,2] = new Func2D( (x,y) => -J12.F(x,y) / detJ.F(x,y) );
         invJ[2,1] = new Func2D( (x,y) => -J21.F(x,y) / detJ.F(x,y) );
         invJ[2,2] = new Func2D( (x,y) => J11.F(x,y) / detJ.F(x,y) );
         return invJ;
      }
      protected (Func2D detJ, Func2D J11, Func2D J12, Func2D J21, Func2D J22)
      CalcDetJ() {
         dbl Δxd = Pos(3).X - Pos(0).X;
         dbl Δxu = Pos(6).X - Pos(9).X;
         dbl Δxl = Pos(9).X - Pos(0).X;
         dbl Δxr = Pos(6).X - Pos(3).X;
         dbl Δyd = Pos(3).Y - Pos(0).Y;
         dbl Δyu = Pos(6).Y - Pos(9).Y;
         dbl Δyl = Pos(9).Y - Pos(0).Y;
         dbl Δyr = Pos(6).Y - Pos(3).Y;
         var J11 = new Func2D( (ξ,η) => 0.25*(Δxd*(1-η) + Δxu*(1+η)) );
         var J12 = new Func2D( (ξ,η) => 0.25*(Δxl*(1-ξ) + Δxr*(1+ξ)) );
         var J21 = new Func2D( (ξ,η) => 0.25*(Δyd*(1-η) + Δyu*(1+η)) );
         var J22 = new Func2D( (ξ,η) => 0.25*(Δyl*(1-ξ) + Δyr*(1+ξ)) );
         var detJ = A.Sub(A.Mul(J11,J22), A.Mul(J12,J21));
         return (detJ, J11, J12, J21, J22);
      }
      /// <summary>Calculate the center of element's mass.</summary>
      public Vec2 CenterOfMass() =>
         Vec2.Sum(Pos(0), Pos(3), Pos(6), Pos(9)) / 4;

      /// <summary>Calculate ksi and eta coordinates inside element using inverse transformations R and T.</summary>
      /// <param name="posX">Position in terms of global x and y.</param>
      public Vec2 Posχ(in Vec2 posX) {
         dbl a = FuncA(in posX);
         dbl b = FuncB(in posX);
         dbl c = FuncC(in posX);
         dbl detMASubMB = Det(MA.Sub(MB));
         dbl detNASubNB = Det(NA.Sub(NB));  // χ
         dbl ξ;
         dbl η;
         if(posX.X*posX.Y >= 0) {                                          // Quadrants I and III.
            if(Abs(detMASubMB) > 10E-7)                                    // Opposing sides are not too parallel.
               ξ = (-b + Sqrt(b*b + c))/detMASubMB;
            else                                                            // Opposing sides are virtually parallel. Use simplified model to preserve precision.
               ξ = Simp_ξ(in posX);
            if(Abs(detNASubNB) > 10E-7)                                    // Opposing sides are not too parallel.
               η = (-a - Sqrt(b*b + c))/detNASubNB;
            else                                                            // Opposing sides are virtually parallel.
               η = Simp_η(in posX); }
         else {                                                              // Quadrants II and IV.
               if(Abs(detMASubMB) > 10E-7)                                    // Opposing sides are not too parallel.
                  ξ = (-b - Sqrt(b*b + c))/detMASubMB;
               else                                                            // Opposing sides are virtually parallel. Use simplified model to preserve precision.
                  ξ = Simp_ξ(in posX);
               if(Abs(detNASubNB) > 10E-7)                                    // Opposing sides are not too parallel.
                  η = (-a + Sqrt(b*b + c))/detNASubNB;
               else                                                            // Opposing sides are virtually parallel.
                  η = Simp_η(in posX); }
         return new Vec2(ξ, η);
      }
      dbl FuncA(in Vec2 pos) =>
         pos.X*Tr(MG) - pos.Y*Tr(MF) + Det(NA) - Det(NB);

      dbl FuncB(in Vec2 pos) =>
         pos.X*Tr(MG) - pos.Y*Tr(MF) + Det(MC) + Det(MD);

      dbl FuncC(in Vec2 pos) =>
         Det(MA.Sub(MB)) * (2*pos.X * Tr(MH) - 2*pos.Y*Tr(MJ) + Det(MA.Sum(MB)));
      /// <summary>Distance of specified point P to a line going through lower edge.</summary>
      /// <param name="pos">Specified point.</param>
      dbl DistToLowerEdge(in Vec2 pos) {
         var lowerEdgeVector = new Vec2(in Pos(0), in Pos(3));    // Vector from lower left to lower right vertex.
         lowerEdgeVector.Normalize();
         var posVec = new Vec2(in Pos(0), in pos);            // Choose a point Q on lower edge: we choose LowerLeft vertex. Then take our specified point P and create a vector.
         return Abs(lowerEdgeVector.Cross(in posVec));       // Take cross product of the two which will give you desired distance.
      }
      /// <summary>Distance of specified point P to a line going through left edge.</summary>
      /// <param name="pos">Specified point.</param>
      dbl DistToLeftEdge(in Vec2 pos) {
         var leftEdgeVec = new Vec2(in Pos(0), in Pos(9));     // Vector from lower left to lower right vertex.
         leftEdgeVec.Normalize();
         var posVec = new Vec2(in Pos(0), in pos);            // Choose a point Q on left edge: we choose LowerLeft vertex. Then take our specified point P and create a vector.
         return Abs(leftEdgeVec.Cross(in posVec));       // Take cross product of the two which will give you desired distance.
      }
      /// <summary>Used when horizontal edges are virtually parallel.</summary>
      /// <param name="pos">Position in terms of x and y.</param>
      dbl Simp_ξ(in Vec2 pos) {
         dbl wholeStretchDist = DistToLeftEdge(in Pos(3));       // Distance between parallel edges.
         dbl posDist = DistToLeftEdge(in pos);                       // Distance of pos from left edge.
         return 2.0*(posDist/wholeStretchDist) - 1.0;                      // Transform to [-1,+1] interval.
      }
      /// <summary>Used when vertical edges are virtually parallel.</summary>
      /// <param name="pos">Position in terms of x and y.</param>
      dbl Simp_η(in Vec2 pos) {
         dbl wholeStretchDist = DistToLowerEdge(in Pos(9));
         dbl posDist = DistToLowerEdge(in pos);
         return 2.0*(posDist/wholeStretchDist) - 1.0;
      }

      // /// <summary>Returns values of desired variables at specified reference position (ksi, eta) inside element.</summary>
      // /// <param name="pos">Position on reference square in terms of (ksi, eta).</param>
      // /// <param name="varInxs">Indices of variables whose values we wish to retrieve.</param>
      // internal dbl[] Vals(in Vec2 pos, params int[] varInxs) {       // TODO: Rethink this.
      //    var vals = new dbl[varInxs.Length];
      //    int varInx;
      //    for(int i = 0; i < varInxs.Length; ++i) {
      //       varInx = varInxs[i];
      //       for(int node = 0; node < 12; ++node)
      //          vals[i] += Vals(node)[varInx] * ϕ[0][node](pos.X, pos.Y); }
      //    return vals;
      // }
      
      /// <summary>Takes the single unique repeating factor (tensor) in a tensor product, multiplies it with itself four times and multiplies that with the element's determinant. Then it integrates the resulting function over the element.</summary>
      /// <param name="tnrFactor">The repeating factor.</param>
      internal SymTnr CalcQuadOverlaps(FTnr tnrFactor) =>
         SymTnr.CreateAsQuadProd(tnrFactor, DetJ, Quadrature2D.Integrate);
      internal SymTnr CalcTripOverlaps(FTnr tnrFactor) =>
         SymTnr.CreateAsTripProd(tnrFactor, DetJ, Quadrature2D.Integrate);
   }
}