using System;
using System.Linq;
using dbl = System.Double;

using Fluid.Internals.Numerics;

namespace Fluid.Internals.Lsfem {
   /// <summary>Contains up to 5 unique nodes from the Mesh that are contained in no other PseudoElement.</summary>
   public class PseudoElement {
      /// <summary>Local indices of nodes from 0 to 4.</summary>
      public int[] PEInxs { get; set; }
      /// <summary>Global indices of nodes on Mesh.</summary>
      public int[] GInxs { get; set; }
      internal Vec2[] _Poss;
      /// <summary>Positions of nodes.</summary>
      public Vec2[] Poss {
         get => _Poss;
         protected set => _Poss = value;
      }

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
         PEInxs = lInx;
         GInxs = gInx;
         Poss = pos;
      }
      public PseudoElement(ref int startGInx, params (int lInx, dbl[] p)[] eNodes) {
         int nNodes = eNodes.Length;
         var lInx = new int[nNodes];
         var gInx = new int[nNodes];
         var pos = new Vec2[nNodes];
         for(int i = 0; i < nNodes; ++i) {            // Over each specified eNode.
            lInx[i] = eNodes[i].lInx;
            gInx[i] = startGInx++;
            pos[i] = new Vec2(eNodes[i].p); }
         PEInxs = lInx;
         GInxs = gInx;
         Poss = pos;
      }
      public static (int currGInx, PseudoElement) CreateCustom(int currGInx, params (int lInx, dbl[])[] eNodes) {
         var pe = new PseudoElement(ref currGInx, eNodes);
         return (currGInx, pe);
      }
      /// <summary>Create 5 PseudoElement (PE) nodes from the specified PE corners.</summary>
      /// <param name="startGInx">Current (unoccupied) global index.</param>
      /// <param name="p0">Lower left corner.</param>
      /// <param name="p3">Lower right corner.</param>
      /// <param name="p6">Upper right corner.</param>
      /// <param name="p9">Upper left corner.</param>
      public static (int currGInx, PseudoElement) CreateFromCorners(int startGInx, (dbl x, dbl y) p0,
      (dbl x, dbl y) p3, (dbl x, dbl y) p9) {
         dbl xp0 = p0.x + 2*(p9.x - p0.x)/3,
             yp0 = p0.y + 2*(p9.y - p0.y)/3,
             xp1 = p0.x + (p9.x - p0.x)/3,
             yp1 = p0.y + (p9.y - p0.y)/3,
             xp2 = p0.x,
             yp2 = p0.y,
             xp3 = p0.x + (p3.x - p0.x)/3,
             yp3 = p0.y + (p3.y - p0.y)/3,
             xp4 = p0.x + 2*(p3.x - p0.x)/3,
             yp4 = p0.y + 2*(p3.y - p0.y)/3;
         var pe = new PseudoElement(ref startGInx, (0,xp0,yp0), (1,xp1,yp1),
            (2,xp2,yp2), (3,xp3,yp3), (4,xp4,yp4));
         return (startGInx, pe);
      }

      public static (int currGInx, PseudoElement) CreatePatchElement(int startGInx, dbl[] p0,
      dbl[] p3, dbl[] p9) {
         dbl xp0 = p0[0] + 2*(p9[0] - p0[0])/3,
             yp0 = p0[1] + 2*(p9[1] - p0[1])/3,
             xp1 = p0[0] + (p9[0] - p0[0])/3,
             yp1 = p0[1] + (p9[1] - p0[1])/3,
             xp2 = p0[0],
             yp2 = p0[1],
             xp3 = p0[0] + (p3[0] - p0[0])/3,
             yp3 = p0[1] + (p3[1] - p0[1])/3,
             xp4 = p0[0] + 2*(p3[0] - p0[0])/3,
             yp4 = p0[1] + 2*(p3[1] - p0[1])/3;
         var pe = new PseudoElement(ref startGInx, (0,xp0,yp0), (1,xp1,yp1),
            (2,xp2,yp2), (3,xp3,yp3), (4,xp4,yp4));
         return (startGInx, pe);
      }

      public static (int currGInx, PseudoElement) CreateJointElement(int startGInx,
      dbl[] p0, dbl[] p9) {
         dbl xp0 = p0[0],
             yp0 = p0[1],
             xp1 = p0[0] + (p9[0] - p0[0])/3,
             yp1 = p0[1] + (p9[1] - p0[1])/3,
             xp2 = p0[0] + 2*(p9[0] - p0[0])/3,
             yp2 = p0[1] + 2*(p9[1] - p0[1])/3;
         var pe = new PseudoElement(ref startGInx, (0,xp0,yp0), (1,xp1,yp1), (2,xp2,yp2));
         return (startGInx, pe);
      }

      public int this[int pEInx] {
         get =>
            PEInxs.First(inx => inx == pEInx);
      }
   }
}