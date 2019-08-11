using System;
using System.Collections.Generic;

using Fluid.Internals.Collections;
using My = Fluid.Internals.Collections.Custom;
using Fluid.Internals.Numerics;

namespace Fluid.Internals.Lsfem {
   using dbl = Double;
   using dA = DblArithmetic;
   using Vec = Vector<double,DblArithmetic>;
   using Tnr = Tensor<double, DblArithmetic>;
   /// <summary>A mesh made of structured blocks which consist of quadrilateral elements.</summary>
   public abstract class BlockMesh {
      /// <summary>Number of independent variables (= number of equations).</summary>
      public int N_m { get; internal set; }
      /// <summary>Dynamics tensor (stiffness matrix), 4th rank.</summary>
      public Tnr K { get; internal set; }
      /// <summary>Free node values tensor, 2nd rank.</summary>
      public Tnr Uf { get; internal set; }
      /// <summary>Constrained node values tensor, 2nd rank.</summary>
      public Tnr Uc { get; internal set; }
      public Vec U(Vec dummy, int inx) {
         Vec u = Uf[dummy, inx];
         if(u != null)
            return u;
         else return Uc[dummy, inx];
      }
      /// <summary>Nodes values tensor, 2nd rank. Returns value at desired index regardless of which tensor it resides in.</summary>
      /// <param name="inxs">A set of indices that extends all the way down to value rank.</param>
      public dbl U(params int[] inxs) {
         dbl u = Uf[inxs];
         if(u != 0.0)
            return u;
         else return Uc[inxs];
      }
      /// <summary>A list of positions.   (global index) => (x,y)</summary>
      public My.List<Vec2> X { get; internal set; }
      /// <summary>A mapping from element nodes to global nodes.
      /// (element index, local node index) => global index.</summary>
      public double [][] G { get; internal set; }
      /// <summary>Forcing tensor, 2nd rank.</summary>
      public Tnr F { get; internal set; }
      /// <summary>Stiffness tensor, 4th rank.
      /// Node (1), Derivative (2), 1st index of element matrix (3), 2nd index of element matrix (4).</summary>
      public Tnr A { get; internal set; }
      /// <summary>Quadruple overlap integrals. Rank 9.
      /// ε, φ1, φD1, J1, φ2, φD2, J2.</summary>
      public Tnr Q { get; internal set; }
      /// <summary>Triple overlap integrals. Rank 7.
      /// ε, φ1, φD1, J1, φ2.</summary>
      public Tnr T { get; internal set; }

      protected ConjGradsSolver Solver { get; }

      protected List<Block> Blocks { get; set; }
      /// <summary>Contains node indices that belong to an element. (element index, nodes)</summary>
      protected List<int[]> Elements { get; }
      

      // /// <summary>Create a block-structured mesh.</summary>
      public BlockMesh(int nVars) {
         N_m = nVars;
         // TODO: Set up the mesh and boundary conditions.
         //Solver = new ConjGradsSolver();
      }
      /// <summary>Add nodes from each block to the mesh.</summary>
      protected void AddNodesFromBlocks() {

      }

      /// <summary>Find solution value of specified variables at specified point.</summary><param name="pos">Sought after position.</param><param name="vars">Indices of variables we wish to retrieve.</param>S
      public abstract double[] Solution(in Vec2 pos, params int[] vars);
   }
}