using System;
using dbl = System.Double;

using Fluid.Internals.Numerics;

namespace Fluid.Internals.Lsfem {
   /// <summary>Contains up to 5 unique nodes from the Mesh that are contained in no other PseudoElement.</summary>
   public class PseudoElement {
      /// <summary>Local indices of nodes from 0 to 4.</summary>
      public int[] LInx { get; set; }
      /// <summary>Global indices of nodes on Mesh.</summary>
      public int[] GInx { get; set; }
      /// <summary>Positions of nodes.</summary>
      public Vec2[] Pos { get; }

      /// <summary>Create a PseudoElement that contains up to 5 unique nodes from the Mesh that are contained in no other PseudoElement. First, specify the current unoccupied global index, then specify all eNodes as a sequence of tuples: (local index, x, y).</summary>
      /// <param name="startGInx">Current (unoccupied) global index.</param>
      /// <param name="lInx">Local eNode index.</param>
      /// <param name="x">X coordinate.</param>
      /// <param name="y">Y coordinate.</param>
      public PseudoElement(ref int startGInx, params (int lInx, dbl x, dbl y)[] eNodes) {
         int nNodes = eNodes.Length;
         var lInx = new int[nNodes];
         var gInx = new int[nNodes];
         var pos = new Vec2[nNodes];
         for(int i = 0; i < nNodes; ++i) {            // Over each specified eNode.
            lInx[i] = eNodes[i].lInx;
            gInx[i] = startGInx++;
            pos[i] = new Vec2(eNodes[i].x, eNodes[i].y); }
         LInx = lInx;
         GInx = gInx;
         Pos = pos;
      }
   }
}