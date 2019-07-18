using System;

using Fluid.Internals.Collections;
using Fluid.Internals.Numerics;
using static Fluid.Internals.Numerics.SerendipityBasis;

namespace Fluid.Internals.Lsfem {
   using dbl = Double;
   using MN = MeshNode;
   /// <summary>Able to create a rectangular submesh from provided border Nodes.</summary>
   public abstract class RectBlock : Block {
      /// <summary>Lower left corner coordinates.</summary>
      Pos _LL;
      /// <summary>Upper right corner coordinates.</summary>
      Pos _UR;
      /// <summary>MeshBlock corner positions.</summary>
      Pos[] _Verts;
      /// <summary>Rectangle's width.</summary>
      dbl Width { get; }
      /// <summary>Rectangle's height.</summary>
      dbl Height { get; }
      /// <summary>Element's width.</summary>
      dbl ColWidth { get; }
      /// <summary>Element's height.</summary>
      dbl RowHeight { get; }


      /// <summary>Create a Cartesian mesh block.</summary><param name="mainMesh">Mesh block's owner.</param><param name="llx">Lower left corner x coordinate.</param><param name="lly">Lower left corner y coordinate.</param><param name="urx">Upper right corner x coordinate.</param><param name="ury">Upper right corner y coordinate.</param><param name="nRows">Number of elements in y direction.</param><param name="nCols">Number of elements in x direction.</param>
      public RectBlock(BlockMesh mainMesh,
         dbl llx, dbl lly, dbl urx, dbl ury, int nRows, int nCols)
         : base(mainMesh) {
            _LL = new Pos(llx, lly);
            _UR = new Pos(urx, ury);
            _Verts = new Pos[4] {
               _LL,
               new Pos(_UR.X, _LL.Y),
               _UR,
               new Pos(_LL.X, _UR.Y) };
            Height = ury - lly;
            Width = urx - llx;
            NRows = nRows;
            NCols = nCols;
            RowHeight = Height / NRows;
            ColWidth = Width / NCols;
      }

      protected override void CreateNodes() {
         dbl yTwoThirdsAbove, yThirdAbove, y, x, xThirdRight, xTwoThirdsRight;
         _Nodes = new MN[NRows + 1][][];                                                  // 60 node rows +1 for top row of nodes
         int nVars = MainMesh.M;
         for(int row = 0; row < NRows; ++row) {                                                 // Move vertically.
            _Nodes[row] = new MN[NCols + 1][];
            y = _LL.Y + row * RowHeight;
            yThirdAbove = y + RowHeight / 3.0;
            yTwoThirdsAbove = y + 2 * RowHeight / 3.0;
            for(int col = 0; col < NCols; ++col) {
               x = _LL.X + col * ColWidth;
               xThirdRight = x + ColWidth / 3.0;
               xTwoThirdsRight = x + 2 * ColWidth / 3.0;
               _Nodes[row][col] = new MN[] {
                  new MN(x, yTwoThirdsAbove, nVars), new MN(x, yThirdAbove, nVars),
                  new MN(x,y,nVars), new MN(xThirdRight, y, nVars),
                  new MN(xTwoThirdsRight, y, nVars) }; }
            x = _UR.X;                                                                            // Add right-most column.
            _Nodes[row][NCols] = new MN[] {
               new MN(x, yTwoThirdsAbove, nVars), new MN(x, yThirdAbove, nVars),
               new MN(x, y, nVars), new MN(dbl.NaN, dbl.NaN, 0),
               new MN(dbl.NaN, dbl.NaN, 0) }; }
         y = _UR.Y;                                                                               // Add upper-most row.
         _Nodes[NRows] = new MN[NCols + 1][];
         for(int col = 0; col < NCols; ++col) {
            x = _LL.Y + col * ColWidth;
            xThirdRight = x + ColWidth / 3.0;
            xTwoThirdsRight = x + 2 * ColWidth / 3.0;
            _Nodes[NRows][col] = new MN[] {
               new MN(dbl.NaN, dbl.NaN, 0), new MN(dbl.NaN, dbl.NaN, 0),
               new MN(x,y, nVars), new MN(xThirdRight, y, nVars),
               new MN(xTwoThirdsRight, y, nVars) }; }
         _Nodes[NRows][NCols] = new MN[] {                                             // Add one last point.
            new MN(dbl.NaN, dbl.NaN, 0), new MN(dbl.NaN, dbl.NaN, 0),
            new MN(_UR.X, _UR.Y, nVars),
            new MN(dbl.NaN, dbl.NaN, 0), new MN(dbl.NaN, dbl.NaN, 0) };
      }
      /// <summary>Find solution value of specified variables at specified point.</summary><param name="pos">X and y coordinates of desired point.</param><param name="vars">Indices of variables we wish to retrieve.</param>
      public override dbl[] Solution(in Pos pos, params int[] vars) {
         dbl blockX = pos.X - _LL.X;                            // Local coordinates with lower left corner as origin.
         dbl blockY = pos.Y - _LL.Y;
         dbl normX = blockX / Width;                             // Get normalized x coordinate: interval [0,1].
         dbl normY = blockY / Height;
         int nX = (int)(blockX / ColWidth);                     // Calculate how many times element's width fits in normX.
         int nY = (int)(blockY / RowHeight);
         dbl ksi = (blockX - nX*ColWidth) / ColWidth;
         dbl eta = (blockY - nY*RowHeight) / RowHeight;
         int nVars = vars.Length;
         dbl[] result = new dbl[nVars];
         for(int varInx = 0; varInx < nVars; ++varInx)                        // For each specified variable.
            for(int p = 0; p < 12; ++p)                     // Over element nodes.
               result[varInx] += ϕ[0][p](ksi,eta)*NodeStd(nY, nX, p).Vars[vars[varInx]].Val;
         return result;
      }
      /// <summary>Determine whether a point is inside this Rectangle.</summary><param name="pos">Point's position.</param>
      public override bool IsPointInside(in Pos pos) => pos.IsInsidePolygon(_Verts);
   }
}