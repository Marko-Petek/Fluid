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
      /// <summary>Free node values tensor.</summary>
      public Tensor Uf { get; internal set; }
      /// <summary>Constrained node values tensor.</summary>
      public Tensor Uc { get; internal set; }
      /// <summary>Returns value at desired index regardless of which tensor it resides in.</summary>
      /// <param name="inxs"></param>
      public dbl U(params int[] inxs) {
         dbl u = Uf[inxs];
         if(u != 0.0)
            return u;
         else return Uc[inxs];
      }


      // /// <summary>Create a block-structured mesh.</summary>
      public BlockMesh(int nVars) {
         M = nVars;
      }


      /// <summary>Create a SparseRow out of Nodes array. Simply flatten list and copy it to SparseRow.</summary>
      public Vector NodesArrayToSparseRow() {
         int rowWidth = G.Length * M;
         var sparseRow = new Vector(rowWidth, rowWidth);
         int index;
         for(int i = 0; i < G.Length; ++i) {
            for(int j = 0; j < M; ++j) {
               index = 8*i + j;
               sparseRow[index] = G[i].Vars[j].Val; }}
         return sparseRow;
      }
      /// <summary>Find solution value of specified variables at specified point.</summary><param name="pos">Sought after position.</param><param name="vars">Indices of variables we wish to retrieve.</param>S
      public abstract double[] Solution(in Pos pos, params int[] vars);
   }
}