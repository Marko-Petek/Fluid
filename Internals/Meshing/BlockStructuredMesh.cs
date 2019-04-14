using System;

using Fluid.Internals.Collections;
using Fluid.Internals.Numerics;

namespace Fluid.Internals.Meshing {
   using SparseRow = SparseRow<double,DblArithmetic>;
   /// <summary>A mesh made of quadrilateral elements.</summary>
   public abstract class BlockStructuredMesh {
      /// <summary>Number of independent variables (= number of equations).</summary>
      public int NVars { get; protected set; }
      /// <summary>Global vector of node values. Each Node[] array represents variable values at a single position.</summary>
      protected MeshNode[] _Nodes;
      /// <summary>Number of constrained variables.</summary>
      public int NConstraints { get; set; }
      /// <summary>Global vector of node values. Each Node[] array represents variable values at a single position.</summary>
      public MeshNode[] Nodes => _Nodes;
      /// <summary>Global vector of nodes (positions, variable values, variable constrainedness).</summary>
      public ref MeshNode Node(int index) => ref _Nodes[index];


      // /// <summary>Create a block-structured mesh.</summary>
      public BlockStructuredMesh(int nVars) {
         NVars = nVars;
      }


      /// <summary>Create a SparseRow out of Nodes array. Simply flatten list and copy it to SparseRow.</summary>
      public SparseRow NodesArrayToSparseRow() {
         int rowWidth = _Nodes.Length * NVars;
         var sparseRow = new SparseRow(rowWidth, rowWidth);
         int index = 0;
         for(int i = 0; i < _Nodes.Length; ++i) {
            for(int j = 0; j < NVars; ++j) {
               index = 8*i + j;
               sparseRow[index] = _Nodes[i].Var(j).Val; }}
         return sparseRow;
      }

      /// <summary>Find solution value of specified variables at specified point.</summary><param name="pos">Sought after position.</param><param name="vars">Indices of variables we wish to retrieve.</param>S
      public abstract double[] Solution(ref Pos pos, params int[] vars);
   }
}