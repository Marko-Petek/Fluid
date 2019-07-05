using System;
using static System.Math;

using Fluid.Internals.Collections;
using Fluid.Internals.Meshing;
using Fluid.Internals.Numerics;

namespace Fluid.ChannelFlow {   
   /// <summary>Block on west side of obstruction.</summary><remarks>Sealed, because we call a virtual method in constructor.</remarks>
   public sealed class WestBlock : ObstructionBlock {   
      /// <summary>Create a mesh block just west of circular obstruction.</summary><param name="channelMesh">Main mesh.</param>
      public WestBlock(ChannelMesh channelMesh, ChannelCylinderSystem channelFlow, SouthBlock southBlock)
      : base(channelMesh, channelFlow,
         channelMesh.ObstructionRect._LL.X, channelMesh.ObstructionRect._LL.Y,
         channelMesh.ObstructionRect._UL.X, channelMesh.ObstructionRect._UL.Y,
         channelMesh.LeftSquare._UL.X, channelMesh.LeftSquare._UL.Y,
         channelMesh.LeftSquare._LL.X, channelMesh.LeftSquare._LL.Y) {
         
         CreateNodes();
         NConstraints = ApplyConstraints();
         Mesh.NConstraints = Mesh.NConstraints + NConstraints;
         MoveNodesToMainMesh(southBlock);
      }


      /// <summary>Returns real-world position of upper boundary node located at a given arc length coordinate.</summary><param name="ksi">Arc length coordinate from 0 to 1.</param>
      protected override Pos CalcUpperBoundaryPos(double ksi) {
         ref var upperLeft = ref _quadrilateral._UL;
         double y = upperLeft.Y + ksi * Mesh.Width;
         double x = upperLeft.X;
         return new Pos(x,y);
      }
      /// <summary>Returns real-world position of lower boundary node located at a given arc length coordinate.</summary>param name="ksi">Arc length coordinate from 0 to 1.</param>
      protected override Pos CalcLowerBoundaryPos(double ksi) {
         ref var lowerLeft = ref _quadrilateral._LL;
         ref var lowerRight = ref _quadrilateral._LR;
         double y = lowerLeft.Y + (lowerRight.Y-lowerLeft.Y) * (0.5 + Cos((PI/4)*(3-2*ksi)) / Sqrt(2));
         double x = lowerLeft.X - QuarterMoonHeight * (Sin((PI/4)*(3-2*ksi)) - 1/Sqrt(2)) / (1 - 1/Sqrt(2));
         return new Pos(x,y);
      }
      /// <summary>Returns real-world position of left boundary node located at a given arc length coordinate.</summary><param name="eta">Arc length coordinate from 0 to 1.</param>
      protected override Pos CalcLeftBoundaryPos(double eta) {
         ref var lowerLeft = ref _quadrilateral._LL;
         double y = lowerLeft.Y - eta * DiagonalProjection;
         double x = lowerLeft.X - eta * DiagonalProjection;
         return new Pos(x,y);
      }
      /// <summary>Returns real-world position of left boundary node located at a given arc length coordinate.</summary><param name="eta">Arc length coordinate from 0 to 1.</param>
      protected override Pos CalcRightBoundaryPos(double eta) {
         ref var lowerRight = ref _quadrilateral._LR;
         double y = lowerRight.Y + eta * DiagonalProjection;
         double x = lowerRight.X - eta * DiagonalProjection;
         return new Pos(x,y);
      }
      protected override int ApplyConstraints() {
         int constraintCount = 0;
         double v0 = ChannelFlow.PeakInletVelocity;
         double w = Mesh.Width;
                                                                              // Row 0 (obstruction).
         for(int col = 0; col < NCols; ++col) {                              // Col 0 - 20
            for(int std = 0; std < 3; ++std)                                  // Obstruction boundary.
               SetObstructionConstraints(NodeCmt(0, col, std));
            SetCornerInletConstraints(NodeStd(22, col, 9)); }         // Row 22 (inlet), upper left corner. Set only corner nodes first for inlet. They are non-zero and if side node are to be set correctly, corner nodes must be set correctly first.
         SetObstructionConstraints(NodeStd(0,19,3));                    // Lower right corner.
         SetCornerInletConstraints(NodeStd(22,19,6));                   // Upper right corner.
         for(int col = 0; col < NCols; ++col)                          // Row 0 (obstruction). Now set side nodes for inlet.
            SetSideInletConstraints(NodeStd(22,col,9), NodeStd(22,col,8), NodeStd(22,col,7),
            NodeStd(22,col,6));
         return constraintCount;

         void SetObstructionConstraints(MeshNode node) {
            node.Vars[0].Constrained = true;                                  // u, 0 is set implicitly for both u and v due to them being value types.
            node.Vars[1].Constrained = true;                                  // v
            constraintCount += 2;
         }
         void SetCornerInletConstraints(MeshNode node) {                         // Applied to corners of elements.
            node.Vars[0].Constrained = true;                                     // u = (4*v0/w)*y*(1-y/w)
            node.Vars[0].Val = (4*v0/w) * node.Y * (1.0 - node.Y/w);
            node.Vars[1].Constrained = true;                                     // v = 0
            node.Vars[3].Constrained = true;                                     // b = (4*v0/w)*(1-y/w)
            node.Vars[3].Val = (4*v0/w) * (1.0 - node.Y/w);
            node.Vars[5].Constrained = true;                                     // p = 0
            constraintCount += 4;
         }
         void SetSideInletConstraints(MeshNode node9, MeshNode node8, MeshNode node7, MeshNode node6) {
            double u9 = node9.Vars[0].Val;
            double h8 = (4*v0/w) * node8.Y * (1.0 - node8.Y/w);
            double h7 = (4*v0/w) * node7.Y * (1.0 - node7.Y/w);
            double u6 = node6.Vars[0].Val;
            node8.Vars[0].Constrained = true;                                     // u = (4*v0/w)*y*(1-y/w)
            node7.Vars[0].Constrained = true;
            node8.Vars[0].Val = 0.25 * (-9 * h7 + 18 * h8 + 2 * u6 - 11 * u9);
            node7.Vars[0].Val = 0.25 * (18 * h7 - 9 * h8 - 11 * u6 + 2 * u9);
            node8.Vars[1].Constrained = true;                                     // v = 0
            node7.Vars[1].Constrained = true;
            u9 = node9.Vars[3].Val;
            h8 = (4*v0/w) * (1.0 - node8.Y/w);
            h7 = (4*v0/w) * (1.0 - node7.Y/w);
            u6 = node6.Vars[3].Val;
            node8.Vars[3].Constrained = true;                                     // b = (4*v0/w)*(1-y/w)
            node7.Vars[3].Constrained = true;
            node8.Vars[3].Val = 0.25 * (-9 * h7 + 18 * h8 + 2 * u6 - 11 * u9);
            node7.Vars[0].Val = 0.25 * (18 * h7 - 9 * h8 - 11 * u6 + 2 * u9);
            node8.Vars[5].Constrained = true;                                     // p = 0
            node7.Vars[5].Constrained = true;
            constraintCount += 8;
         }
      }
      void MoveNodesToMainMesh(SouthBlock southBlock) {
         int nPos = Mesh.NPos;
         var blockToGlobal = new int[NRows + 1][][];
         int row = 0;
         int col = 0;
         var southMap = southBlock.CmtInxToGblInxMap;      // We will need SouthBlock's map.
         while(row < NRows) {                                     // Rows 0 - 22
            blockToGlobal[row] = new int[NCols + 1][];
            col = 0;
            blockToGlobal[row][col] = new int[5];                      // Col 0. Take in nodes from Col 20 of SouthBlock.
            for(int p = 0; p < 3; ++p)
               blockToGlobal[row][col][p] = southMap[row][20][p];
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
            blockToGlobal[row][col] = new int[5];                      // Col 20
            for(int p = 0; p < 3; ++p) {
               Mesh.G[nPos] = NodeCmt(row, col, p);
               blockToGlobal[row][col][p] = nPos++; }
            for(int p = 3; p < 5; ++p)
               blockToGlobal[row][col][p] = Int32.MinValue;
            ++row; }
         col = 0;                                                    // Row 23.
         blockToGlobal[row] = new int[NCols+1][];
         blockToGlobal[row][col] = new int[5];                           // Col 0
         for(int p = 0; p < 2; ++p)
            blockToGlobal[row][col][p] = Int32.MinValue;
         blockToGlobal[row][col][2] = southMap[row][20][2];    // Take in node from Col 20 of SouthBlock.
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
         blockToGlobal[row][col] = new int[5];                           // Col 20
         for(int p = 0; p < 2; ++p)
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