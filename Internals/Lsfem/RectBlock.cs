using System;

using Fluid.Internals.Collections;
using Fluid.Internals.Numerics;
using static Fluid.Internals.Numerics.SerendipityBasis;

namespace Fluid.Internals.Lsfem {
   using dbl = Double;
   /// <summary>Able to create a rectangular submesh from provided border Nodes.</summary>
   public abstract class RectBlock : Block {
      /// <summary>Lower left corner coordinates.</summary>
      Vec2 LL { get; }
      /// <summary>Upper right corner coordinates.</summary>
      Vec2 UR { get; }


      /// <summary>Create a Cartesian mesh block.</summary><param name="llx">Lower left corner x coordinate.</param><param name="lly">Lower left corner y coordinate.</param><param name="urx">Upper right corner x coordinate.</param><param name="ury">Upper right corner y coordinate.</param>
      public RectBlock(dbl llx, dbl lly, dbl urx, dbl ury)
      : base() {
         LL = new Vec2(llx, lly);
         UR = new Vec2(urx, ury);
      }

      /// <summary>Create a position inside the block that corresponds to an intersection of two (curvilinear) coordinate lines: a "vertical" one going through the bottom edge at width-wise parameter value tW and a "horizontal" one going through the left edge at height-wise parameter value tH</summary>
      /// <param name="tW">Width-wise parameter value.</param>
      /// <param name="tH">Height-wise parameter value.</param>
      public override Vec2 CreatePos(dbl tW, dbl tH) {
         dbl x = LL.X + tW * (UR.X - LL.X),
             y = LL.Y + tH * (UR.Y - LL.Y);
         return new Vec2(x,y);
      }


      // /// <summary>Find solution value of specified variables at specified point.</summary><param name="pos">X and y coordinates of desired point.</param><param name="vars">Indices of variables we wish to retrieve.</param>
      // public override dbl[] Solution(in Pos pos, params int[] vars) {
      //    dbl blockX = pos.X - _LL.X;                            // Local coordinates with lower left corner as origin.
      //    dbl blockY = pos.Y - _LL.Y;
      //    dbl normX = blockX / Width;                             // Get normalized x coordinate: interval [0,1].
      //    dbl normY = blockY / Height;
      //    int nX = (int)(blockX / ColWidth);                     // Calculate how many times element's width fits in normX.
      //    int nY = (int)(blockY / RowHeight);
      //    dbl ksi = (blockX - nX*ColWidth) / ColWidth;
      //    dbl eta = (blockY - nY*RowHeight) / RowHeight;
      //    int nVars = vars.Length;
      //    dbl[] result = new dbl[nVars];
      //    for(int varInx = 0; varInx < nVars; ++varInx)                        // For each specified variable.
      //       for(int p = 0; p < 12; ++p)                     // Over element nodes.
      //          result[varInx] += ϕ[0][p](ksi,eta)*NodeStd(nY, nX, p).Vars[vars[varInx]].Val;
      //    return result;
      // }
   }
}