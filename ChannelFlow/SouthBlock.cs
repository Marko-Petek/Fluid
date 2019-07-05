using System;
using static System.Math;

using Fluid.Internals.Collections;
using Fluid.Internals.Meshing;
using Fluid.Internals.Numerics;

namespace Fluid.ChannelFlow {   
   /// <summary>Block on south side of obstruction.</summary><remarks>Sealed, because we call a virtual method in constructor.</remarks>
   public sealed class SouthBlock : ObstructionBlock {   
      /// <summary>Create a mesh block just south of circular obstruction.</summary><param name="channelMesh">Main mesh.</param>
      public SouthBlock(ChannelMesh channelMesh, ChannelCylinderSystem channelFlow)
      : base(channelMesh, channelFlow,
         channelMesh.ObstructionRect._LR.X, channelMesh.ObstructionRect._LR.Y,
         channelMesh.ObstructionRect._LL.X, channelMesh.ObstructionRect._LL.Y,
         channelMesh.LeftSquare._LL.X, channelMesh.LeftSquare._LL.Y,
         channelMesh.LeftSquare._LR.X, channelMesh.LeftSquare._LR.Y) {
            CreateNodes();
            NConstraints = ApplyConstraints();
            Mesh.NConstraints += NConstraints;
            MoveNodesToMainMesh();
      }


      /// <summary>Returns real-world position of upper boundary node located at a given arc length coordinate.</summary><param name="ksi">Arc length coordinate from 0 to 1.</param>
      protected override Pos CalcUpperBoundaryPos(double ksi) {
         ref var upperLeft = ref _quadrilateral._UL;
         double x = upperLeft.X - ksi * Mesh.Width;
         double y = upperLeft.Y;
         return new Pos(x,y);
      }
      /// <summary>Returns real-world position of lower boundary node located at a given arc length coordinate.</summary>param name="ksi">Arc length coordinate from 0 to 1.</param>
      protected override Pos CalcLowerBoundaryPos(double ksi) {
         ref var lowerLeft = ref _quadrilateral._LL;
         ref var lowerRight = ref _quadrilateral._LR;
         double x = lowerLeft.X + (lowerRight.X-lowerLeft.X) * (0.5 + Cos((PI/4)*(3-2*ksi)) / Sqrt(2));
         double y = lowerLeft.Y - QuarterMoonHeight * (Sin((PI/4)*(3-2*ksi)) - 1/Sqrt(2)) / (1 - 1/Sqrt(2));
         return new Pos(x,y);
      }
      /// <summary>Returns real-world position of left boundary node located at a given arc length coordinate.</summary><param name="eta">Arc length coordinate from 0 to 1.</param>
      protected override Pos CalcLeftBoundaryPos(double eta) {
         ref var lowerLeft = ref _quadrilateral._LL;
         double x = lowerLeft.X + eta * DiagonalProjection;
         double y = lowerLeft.Y - eta * DiagonalProjection;
         return new Pos(x,y);
      }
      /// <summary>Returns real-world position of left boundary node located at a given arc length coordinate.</summary><param name="eta">Arc length coordinate from 0 to 1.</param>
      protected override Pos CalcRightBoundaryPos(double eta) {
         ref var lowerRight = ref _quadrilateral._LR;
         double x = lowerRight.X - eta * DiagonalProjection;
         double y = lowerRight.Y - eta * DiagonalProjection;
         return new Pos(x,y);
      }
      protected override int ApplyConstraints() {
         int col = 0;
         int constraintsCount = 0;
         while(col < NCols) {                                            // Col 0 - 22
            for(int p = 2; p < 5; ++p) {                  // Obstruction boundary.
               NodeCmt(0, col, p).Vars[0].Constrained = true;      // u, 0 is set implicitly for both u and v due to them being value types.
               NodeCmt(0, col, p).Vars[1].Constrained = true;      // v
               constraintsCount += 2; }
            for(int p = 2; p < 5; ++p) {                  // Channel boundary.
               NodeCmt(23, col, p).Vars[0].Constrained = true;     // u
               NodeCmt(23, col, p).Vars[1].Constrained = true;     // v
               NodeCmt(23, col, p).Vars[2].Constrained = true;     // a
               NodeCmt(23, col, p).Vars[4].Constrained = true;     // c
               constraintsCount += 4; }
            ++col; }
         // GetNodeCmp(0, col, 2).Constrainedness(0) = true;                     // Obstruction boundary, u, Col 23   !Already set.
         // GetNodeCmp(0, col, 2).Constrainedness(1) = true;                     // v
         // GetNodeCmp(23, col, 2).Constrainedness(0) = true;                    // Channel boundary, u.
         // GetNodeCmp(23, col, 2).Constrainedness(1) = true;                    // v
         // GetNodeCmp(23, col, 2).Constrainedness(2) = true;                    // a
         // GetNodeCmp(23, col, 2).Constrainedness(4) = true;                    // c
         return constraintsCount;
      }
      void MoveNodesToMainMesh() {
         int nPos = Mesh.NPos;                         // mapping
         var blockToGlobal = new int[NRows + 1][][];
         int row = 0, col = 0;                             
         while(row < NRows) {                                                // Row 0 - 22.
            blockToGlobal[row] = new int[NCols + 1][];
            col = 0;
            while(col < NCols) {                                            // Col 0 - 19.
               blockToGlobal[row][col] = new int[5];
               for(int p = 0; p < 5; ++p) {
                  Mesh.G[nPos] = NodeCmt(row,col,p);
                  blockToGlobal[row][col][p] = nPos++; }
               ++col; }
            blockToGlobal[row][col] = new int[5];
            for(int node = 0; node < 3; ++node) {
               Mesh.G[nPos] = NodeCmt(row,col,node);
               blockToGlobal[row][col][node] = nPos++; }
            for(int node = 3; node < 5; ++node)                           // Col 20.
               blockToGlobal[row][col][node] = Int32.MinValue;
            ++row; }
         col = 0;                                                    // Row 23.
         blockToGlobal[row] = new int[NCols + 1][];
         while(col < NCols) {                                         // Col 0 - 19
            blockToGlobal[row][col] = new int[5];
            for(int node = 0; node < 2; ++node)
               blockToGlobal[row][col][node] = Int32.MinValue;
            for(int node = 2; node < 5; ++node) {
               Mesh.G[nPos] = NodeCmt(row,col,node);
               blockToGlobal[row][col][node] = nPos++; }
            ++col; }
         blockToGlobal[row][col] = new int[5];                           // Col 20.
         for(int node = 0; node < 2; ++node)
            blockToGlobal[row][col][node] = Int32.MinValue;
         Mesh.G[nPos] = NodeCmt(row,col,2);
         blockToGlobal[row][col][2] = nPos++;
         for(int node = 3; node < 5; ++node)
            blockToGlobal[row][col][node] = Int32.MinValue;
         _CmtInxToGblInxMap = blockToGlobal;        // Apply local to field.
         Mesh.NPos = nPos;                   // Last global index to pass to next block which will generate local to global mapping.
         _Nodes = null;                                              // Free memory on block.
         NodeCmt = NodeOnMainCmt;                              // Rewire.
         NodeStd = NodeOnMainStd;
      }
   }
}