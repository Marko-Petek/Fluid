using System;

using Fluid.Internals.Collections;
using Fluid.Internals.Numerics;

namespace Fluid.Internals.Lsfem {
   using dbl = Double;
   using dA = DblArithmetic;
   using Vector = Vector<double,DblArithmetic>;
   using Tensor = Tensor<double, DblArithmetic>;
   /// <summary>A mesh made of structured blocks which consist of quadrilateral elements.</summary>
   public abstract class BlockMesh {
      /// <summary>Number of independent variables (= number of equations).</summary>
      public int M { get; internal set; }
      /// <summary>Free node values tensor, 2nd rank.</summary>
      public Tensor Uf { get; internal set; }
      /// <summary>Constrained node values tensor, 2nd rank.</summary>
      public Tensor Uc { get; internal set; }
      /// <summary>Nodes values tensor, 2nd rank. Returns value at desired index regardless of which tensor it resides in.</summary>
      /// <param name="inxs">A set of indices that extends all the way down to value rank.</param>
      public double U(params int[] inxs) {
         dbl u = Uf[inxs];
         if(u != 0.0)
            return u;
         else return Uc[inxs];
      }

      /// <summary>Dynamics tensor (stiffness matrix), 4th rank.</summary>
      public Tensor K { get; internal set; }
      /// <summary>Forcing tensor, 2nd rank.</summary>
      public Tensor F { get; internal set; }
      /// <summary>Stiffness tensor, 4th rank.  
      /// Node (1), Derivative (2), 1st index of element matrix (3), 2nd index of element matrix (4).</summary>
      public Tensor A { get; internal set; }
      /// <summary>Tripple overlap integrals.
      /// ε, φ1, φD1, J1, φ2, φD2, J2.</summary>
      public Tensor S { get; internal set; }
      /// <summary>Double overlap integrals.
      /// ε, φ1, φD1, J1, φ2.</summary>
      public Tensor T { get; internal set; }
      

      // /// <summary>Create a block-structured mesh.</summary>
      public BlockMesh(int nVars) {
         M = nVars;
      }

      /// <summary>Find solution value of specified variables at specified point.</summary><param name="pos">Sought after position.</param><param name="vars">Indices of variables we wish to retrieve.</param>S
      public abstract double[] Solution(in Pos pos, params int[] vars);
   }
}