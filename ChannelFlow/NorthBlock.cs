using System;
using static System.Math;

using Fluid.Internals.Collections;
using Fluid.Internals.Mesh;
using Fluid.Internals.Numerics;

namespace Fluid.ChannelFlow {   
   /// <summary>Block on north side of obstruction.</summary><remarks>Sealed, because we call a virtual method in constructor.</remarks>
   public sealed class NorthBlock : ObstructionBlock {   
      /// <summary>Create a mesh block just north of circular obstruction.</summary><param name="channelMesh">Main mesh.</param>
      public NorthBlock(ChannelMesh channelMesh, ChannelCylinderSystem channelFlow, WestBlock westBlock)
      : base(channelMesh, channelFlow,
         channelMesh.ObstructionRect._LL.X, channelMesh.ObstructionRect._LL.Y,
         channelMesh.ObstructionRect._UR.X, channelMesh.ObstructionRect._UR.Y,
         channelMesh.LeftSquare._UR.X, channelMesh.LeftSquare._UR.Y,
         channelMesh.LeftSquare._UL.X, channelMesh.LeftSquare._UL.Y) {
            CreateNodes();
            NConstraints = ApplyConstraints();
            Mesh.NConstraints = Mesh.NConstraints + NConstraints;
            MoveNodesToMainMesh(westBlock);
      }

      /// <summary>Returns real-world position of upper boundary node located at a given arc length coordinate.</summary><param name="ksi">Arc length coordinate from 0 to 1.</param>
      protected override Pos CalcUpperBoundaryPos(double ksi) {
         ref var upperLeft = ref _quadrilateral._UL;
         double x = upperLeft.X + ksi * Mesh.Width;
         double y = upperLeft.Y;
         return new Pos(x,y);
      }
      /// <summary>Returns real-world position of lower boundary node located at a given arc length coordinate.</summary>param name="ksi">Arc length coordinate from 0 to 1.</param>
      protected override Pos CalcLowerBoundaryPos(double ksi) {
         ref var lowerLeft = ref _quadrilateral._LL;
         ref var lowerRight = ref _quadrilateral._LR;
         double x = lowerLeft.X + (lowerRight.X-lowerLeft.X) * (0.5 + Cos((PI/4)*(3-2*ksi)) / Sqrt(2));
         double y = lowerLeft.Y + QuarterMoonHeight * (Sin((PI/4)*(3-2*ksi)) - 1/Sqrt(2)) / (1 - 1/Sqrt(2));
         return new Pos(x,y);
      }
      /// <summary>Returns real-world position of left boundary node located at a given arc length coordinate.</summary><param name="eta">Arc length coordinate from 0 to 1.</param>
      protected override Pos CalcLeftBoundaryPos(double eta) {
         ref var lowerLeft = ref _quadrilateral._LL;
         double x = lowerLeft.X - eta * DiagonalProjection;
         double y = lowerLeft.Y + eta * DiagonalProjection;
         return new Pos(x,y);
      }
      /// <summary>Returns real-world position of left boundary node located at a given arc length coordinate.</summary><param name="eta">Arc length coordinate from 0 to 1.</param>
      protected override Pos CalcRightBoundaryPos(double eta) {
         ref var lowerRight = ref _quadrilateral._LR;
         double x = lowerRight.X + eta * DiagonalProjection;
         double y = lowerRight.Y + eta * DiagonalProjection;
         return new Pos(x,y);
      }
      protected override int ApplyConstraints() {
         int constraintCount = 0;
         int col = 0;
         for(int emt = 3; emt < 5; ++emt) {                  // Col 0 is different, we exclude left most points. Obstruction boundary.
            NodeCmt(0, col, emt).Vars[0].Constrained = true;      // u, 0 is set implicitly for both u and v due to them being value types.
            NodeCmt(0, col, emt).Vars[1].Constrained = true;      // v
            constraintCount += 2; }
         for(int emt = 3; emt < 5; ++emt) {                  // Channel boundary.
            NodeCmt(23, col, emt).Vars[0].Constrained = true;     // u
            NodeCmt(23, col, emt).Vars[1].Constrained = true;     // v
            NodeCmt(23, col, emt).Vars[2].Constrained = true;     // a
            NodeCmt(23, col, emt).Vars[4].Constrained = true;     // c
            constraintCount += 4; }
         ++col;
         while(col < NCols) {                                            // Col 0 - 22
            for(int emt = 2; emt < 5; ++emt) {                  // Obstruction boundary.
               NodeCmt(0, col, emt).Vars[0].Constrained = true;      // u, 0 is set implicitly for both u and v due to them being value types.
               NodeCmt(0, col, emt).Vars[1].Constrained = true;      // v
               constraintCount += 2; }
            for(int emt = 2; emt < 5; ++emt) {                  // Channel boundary.
               NodeCmt(23, col, emt).Vars[0].Constrained = true;     // u
               NodeCmt(23, col, emt).Vars[1].Constrained = true;     // v
               NodeCmt(23, col, emt).Vars[2].Constrained = true;     // a
               NodeCmt(23, col, emt).Vars[4].Constrained = true;     // c
               constraintCount += 4; }
            ++col; }
         // We also omit right most column of points.
         // GetNodeCmp(0, col, 2).Constrainedness(0) = true;                    // Obstruction boundary, u, last column
         // GetNodeCmp(0, col, 2).Constrainedness(1) = true;                    // v
         // GetNodeCmp(23, col, 2).Constrainedness(0) = true;                   // Channel boundary, u.
         // GetNodeCmp(23, col, 2).Constrainedness(1) = true;                   // v
         // GetNodeCmp(23, col, 2).Constrainedness(2) = true;                   // a
         // GetNodeCmp(23, col, 2).Constrainedness(4) = true;                   // c
         // constraintCount += 6;
         return constraintCount;
      }
      void MoveNodesToMainMesh(WestBlock westBlock) {
         int nPos = Mesh.NPos;
         var blockToGlobal = new int[NRows + 1][][];
         int row = 0;
         int col = 0;
         var westMap = westBlock.CmtInxToGblInxMap;           // We will need WestBlock's map.
         while(row < NRows) {                                     // Rows 0 - 22
            blockToGlobal[row] = new int[NCols + 1][];
            col = 0;
            blockToGlobal[row][col] = new int[5];
            for(int p = 0; p < 3; ++p)                           // Col 0. Take in nodes from Col 20 of WestBlock.
               blockToGlobal[row][col][p] = westMap[row][20][p];
            for(int p = 3; p < 5; ++p) {
               Mesh.G[nPos] = NodeCmt(row, col, p);
               blockToGlobal[row][col][p] = nPos++; }
            col = 1;
            while(col < NCols) {                                     // Cols 1 - 19
               blockToGlobal[row][col] = new int[5];
               for(int p = 0; p < 5; ++p) {
                  Mesh.G[nPos] = NodeCmt(row, col, p);
                  blockToGlobal[row][col][p] = nPos++; }
               ++col; }
            blockToGlobal[row][col] = new int[5];                      // Col 20, Last col.
            for(int p = 0; p < 3; ++p) {
               Mesh.G[nPos] = NodeCmt(row, col, p);
               blockToGlobal[row][col][p] = nPos++; }
            for(int p = 3; p < 5; ++p)
               blockToGlobal[row][col][p] = Int32.MinValue;
            ++row; }
         col = 0;                                                    // Row 23
         blockToGlobal[row] = new int[NCols+1][];
         blockToGlobal[row][col] = new int[5];                           // Col 0
         for(int p = 0; p < 2; ++p)
               blockToGlobal[row][col][p] = Int32.MinValue;
         blockToGlobal[row][col][2] = westMap[row][20][2];                   // Take in nodes from Col 20 of WestBlock.
         for(int p = 3; p < 5; ++p) {
               Mesh.G[nPos] = NodeCmt(row, col, p);
               blockToGlobal[row][col][p] = nPos++; }
         col = 1;
         while(col < NCols) {                                         // Cols 1 - 19
            blockToGlobal[row][col] = new int[5];
            for(int p = 0; p < 2; ++p)
               blockToGlobal[row][col][p] = Int32.MinValue;
            for(int p = 2; p < 5; ++p) {
               Mesh.G[nPos] = NodeCmt(row, col, p);
               blockToGlobal[row][col][p] = nPos++; }
            ++col; }
         blockToGlobal[row][col] = new int[5];
         for(int p = 0; p < 2; ++p)                           // Col 20
            blockToGlobal[row][col][p] = Int32.MinValue;
         Mesh.G[nPos] = NodeCmt(row, col, 2);
         blockToGlobal[row][col][2] = nPos++;
         for(int p = 3; p < 5; ++p)
            blockToGlobal[row][col][p] = Int32.MinValue;
         _CmtInxToGblInxMap = blockToGlobal;
         Mesh.NPos = nPos;
         _Nodes = null;                                              // Free memory on block.
         NodeCmt = NodeOnMainCmt;                              // Rewire.
         NodeStd = NodeOnMainStd;
      }
   }
}