using System;

namespace Fluid.Internals.Lsfem {
   public class ClosedRectBlock : RectBlock {

      protected override void CreateNodesAndElements() {
         dbl yTwoThirdsAbove, yThirdAbove, y, x, xThirdRight, xTwoThirdsRight;
         _Nodes = new MN[NRows + 1][][];                                                  // 60 node rows +1 for top row of nodes
         int nVars = MainMesh.N_m;
         for(int row = 0; row < NRows; ++row) {                                                 // Move vertically.
            _Nodes[row] = new MN[NCols + 1][];
            y = LL.Y + row * RowHeight;
            yThirdAbove = y + RowHeight / 3.0;
            yTwoThirdsAbove = y + 2 * RowHeight / 3.0;
            for(int col = 0; col < NCols; ++col) {
               x = LL.X + col * ColWidth;
               xThirdRight = x + ColWidth / 3.0;
               xTwoThirdsRight = x + 2 * ColWidth / 3.0;
               _Nodes[row][col] = new MN[] {
                  new MN(x, yTwoThirdsAbove, nVars), new MN(x, yThirdAbove, nVars),
                  new MN(x,y,nVars), new MN(xThirdRight, y, nVars),
                  new MN(xTwoThirdsRight, y, nVars) }; }
            x = UR.X;                                                                            // Add right-most column.
            _Nodes[row][NCols] = new MN[] {
               new MN(x, yTwoThirdsAbove, nVars), new MN(x, yThirdAbove, nVars),
               new MN(x, y, nVars), new MN(dbl.NaN, dbl.NaN, 0),
               new MN(dbl.NaN, dbl.NaN, 0) }; }
         y = UR.Y;                                                                               // Add upper-most row.
         _Nodes[NRows] = new MN[NCols + 1][];
         for(int col = 0; col < NCols; ++col) {
            x = LL.Y + col * ColWidth;
            xThirdRight = x + ColWidth / 3.0;
            xTwoThirdsRight = x + 2 * ColWidth / 3.0;
            _Nodes[NRows][col] = new MN[] {
               new MN(dbl.NaN, dbl.NaN, 0), new MN(dbl.NaN, dbl.NaN, 0),
               new MN(x,y, nVars), new MN(xThirdRight, y, nVars),
               new MN(xTwoThirdsRight, y, nVars) }; }
         _Nodes[NRows][NCols] = new MN[] {                                             // Add one last point.
            new MN(dbl.NaN, dbl.NaN, 0), new MN(dbl.NaN, dbl.NaN, 0),
            new MN(UR.X, UR.Y, nVars),
            new MN(dbl.NaN, dbl.NaN, 0), new MN(dbl.NaN, dbl.NaN, 0) };
      }
   }
}