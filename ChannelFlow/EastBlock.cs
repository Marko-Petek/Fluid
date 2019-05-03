using System;
using static System.Math;

using Fluid.Internals.Collections;
using Fluid.Internals.Meshing;
using Fluid.Internals.Numerics;

namespace Fluid.ChannelFlow {
   /// <summary>Block on east side of obstruction.</summary><remarks>Sealed, because we call a virtual method in constructor.</remarks>
   public class EastBlock : ObstructionBlock {   
      /// <summary>Create a mesh block just east of circular obstruction.</summary><param name="channelMesh">Main mesh.</param>
      public EastBlock(ChannelMesh channelMesh, ChannelCylinderSystem channelFlow, NorthBlock northBlock, SouthBlock southBlock)
      : base(channelMesh, channelFlow,
         channelMesh.ObstructionRect._UR.X, channelMesh.ObstructionRect._UR.Y,
         channelMesh.ObstructionRect._LR.X, channelMesh.ObstructionRect._LR.Y,
         channelMesh.LeftSquare._LR.X, channelMesh.LeftSquare._LR.Y,
         channelMesh.LeftSquare._UR.X, channelMesh.LeftSquare._UR.Y) {  
            CreateNodes();
            NConstraints = ApplyConstraints();
            Mesh.NConstraints = Mesh.NConstraints + NConstraints;
            MoveNodesToMainMesh(northBlock, southBlock);
      }


      /// <summary>Returns real-world position of upper boundary node located at a given arc length coordinate.</summary><param name="ksi">Arc length coordinate from 0 to 1.</param>
      protected override Pos CalcUpperBoundaryPos(double ksi) {
         ref var upperLeft = ref _quadrilateral._UL;
         double y = upperLeft.Y - ksi * Mesh.Width;
         double x = upperLeft.X;
         return new Pos(x,y);
      }
      /// <summary>Returns real-world position of lower boundary node located at a given arc length coordinate.</summary>param name="ksi">Arc length coordinate from 0 to 1.</param>
      protected override Pos CalcLowerBoundaryPos(double ksi) {
         ref var lowerLeft = ref _quadrilateral._LL;
         ref var lowerRight = ref _quadrilateral._LR;
         double y = lowerLeft.Y - (lowerLeft.Y-lowerRight.Y) * (0.5 + Cos((PI/4)*(3-2*ksi)) / Sqrt(2));
         double x = lowerLeft.X + QuarterMoonHeight * (Sin((PI/4)*(3-2*ksi)) - 1/Sqrt(2)) / (1 - 1/Sqrt(2));
         return new Pos(x,y);
      }
      /// <summary>Returns real-world position of left boundary node located at a given arc length coordinate.</summary><param name="eta">Arc length coordinate from 0 to 1.</param>
      protected override Pos CalcLeftBoundaryPos(double eta) {
         ref var lowerLeft = ref _quadrilateral._LL;
         double y = lowerLeft.Y + eta * DiagonalProjection;
         double x = lowerLeft.X + eta * DiagonalProjection;
         return new Pos(x,y);
      }
      /// <summary>Returns real-world position of left boundary node located at a given arc length coordinate.</summary><param name="eta">Arc length coordinate from 0 to 1.</param>
      protected override Pos CalcRightBoundaryPos(double eta) {
         ref var lowerRight = ref _quadrilateral._LR;
         double y = lowerRight.Y - eta * DiagonalProjection;
         double x = lowerRight.X + eta * DiagonalProjection;
         return new Pos(x,y);
      }
      protected override int ApplyConstraints() {        // We apply only to points on obstruction.
         int constraintCount = 0;
         for(int col = 0; col < 20; ++col)             // Row 0, Cols 0-19 (obstruction)
            for(int node = 2; node < 5; ++node) {
               NodeCmt(0, col, node).Vars[0].Constrained = true;
               NodeCmt(0, col, node).Vars[1].Constrained = true;
               constraintCount += 2; }
         return constraintCount;                                             // Must not count in points from col 20.
      }
      protected void MoveNodesToMainMesh(NorthBlock northBlock, SouthBlock southBlock) {
         int posCount = Mesh.NPos;
         var blockToGlobal = new int[NRows + 1][][];
         int row = 0;
         int col = 0;
         var northMap = northBlock.CmtInxToGblInxMap;          // We will need NorthBlock's map.
         var southMap = southBlock.CmtInxToGblInxMap;          // We will also need SouthBlock's map.
         while(row < NRows) {                                            // Rows 0 - 22
            blockToGlobal[row] = new int[NCols + 1][];
            col = 0;
            blockToGlobal[row][col] = new int[5];
            for(int node = 0; node < 3; ++node)                               // Col 0, take in nodes from Col 20 of NorthBlock.
               blockToGlobal[row][col][node] = northMap[row][20][node];
            for(int node = 3; node < 5; ++node) {
               Mesh.G[posCount] = NodeCmt(row, col, node);
               blockToGlobal[row][col][node] = posCount++; }
            col = 1;
            while(col < NCols) {                                            // Cols 1 - 19
               blockToGlobal[row][col] = new int[5];
               for(int node = 0; node < 5; ++node) {
                  Mesh.G[posCount] = NodeCmt(row, col, node);
                  blockToGlobal[row][col][node] = posCount++; }
               ++col; }
            blockToGlobal[row][col] = new int[5];                       // Col 20, Take in nodes from Col 0 of SouthBlock.
            for(int node = 0; node < 3; ++node)
               blockToGlobal[row][col][node] = southMap[row][0][node];
            for(int node = 3; node < 5; ++node)
               blockToGlobal[row][col][node] = Int32.MinValue;
            ++row; }
         col = 0;                                                    // Row 23.
         blockToGlobal[row] = new int[NCols+1][];
         blockToGlobal[row][col] = new int[5];                           // Col 0, take in node from Col 20 of NorthBlock.
         for(int node = 0; node < 2; ++node)
            blockToGlobal[row][col][node] = Int32.MinValue;
         blockToGlobal[row][col][2] = northMap[row][20][2];
         for(int node = 3; node < 5; ++node) {
            Mesh.G[posCount] = NodeCmt(row, col, node);
            blockToGlobal[row][col][node] = posCount++; }
         col = 1;
         while(col < NCols) {                                         // Cols 1 - 19
            blockToGlobal[row][col] = new int[5];
            for(int node = 0; node < 2; ++node)
               blockToGlobal[row][col][node] = Int32.MinValue;
            for(int node = 2; node < 5; ++node) {
               Mesh.G[posCount] = NodeCmt(row, col, node);
               blockToGlobal[row][col][node] = posCount++; }
            ++col; }
         blockToGlobal[row][col] = new int[5];                           // Col 20, Take in nodes from Col 0 of SouthBlock.
         for(int node = 0; node < 2; ++node)
            blockToGlobal[row][col][node] = Int32.MinValue;
         blockToGlobal[row][col][2] = southMap[row][0][2];
         for(int node = 3; node < 5; ++node)
            blockToGlobal[row][col][node] = Int32.MinValue;
         _CmtInxToGblInxMap = blockToGlobal;
         Mesh.NPos = posCount;
         _Nodes = null;                                              // Free memory on block.
         NodeCmt = NodeOnMainCmt;                              // Rewire.
         NodeStd = NodeOnMainStd;
      }
   }
}