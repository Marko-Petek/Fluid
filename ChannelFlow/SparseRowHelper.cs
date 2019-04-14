using Fluid.Internals.Collections;
using Fluid.Internals.Meshing;
using Fluid.Internals.Numerics;

namespace Fluid.ChannelFlow {
   using SparseRow = SparseRow<double,DblArithmetic>;
   public static class SparseRowHelper
   {
      /// <summary>Transfer values from this SparseRow to specified nodes array.</summary><param name="sparseRow">Source SparseRow of values.</param><param name="nodes">Receiving Node[] array of values.</param>
      public static void UpdateNodeArray(this SparseRow sparseRow, MeshNode[] nodes) {
         int nodeCount = nodes.Length;
         int nVars = nodes[0].Vars.Length;
         for(int i = 0; i < nodeCount; ++i)
            for(int j = 0; j < nVars; ++j)
               nodes[i].Var(j).Val += sparseRow[nVars*i + j];
      }
   }
}