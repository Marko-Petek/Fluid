using Fluid.Internals.Collections;
using Fluid.Internals.Meshing;
using Fluid.Internals.Numerics;

namespace Fluid.ChannelFlow {
   using Vector = Vector<double,DblArithmetic>;
   public static class SparseRowHelper
   {
      /// <summary>Transfer values from this SparseRow to specified nodes array.</summary><param name="vec">Source SparseRow of values.</param><param name="nodes">Receiving Node[] array of values.</param>
      public static void UpdateNodeArray(this Vector vec, MeshNode[] nodes) {
         int nNodes = nodes.Length;
         int nVars = nodes[0].Vars.Length;
         for(int i = 0; i < nNodes; ++i)
            for(int j = 0; j < nVars; ++j)
               nodes[i].Vars[j].Val += vec[nVars*i + j];
      }
   }
}