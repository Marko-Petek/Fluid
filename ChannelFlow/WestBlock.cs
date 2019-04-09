using System;
using static System.Math;

using Fluid.Internals.Collections;
using Msh = Fluid.Internals.Meshing;
using Fluid.Internals.Numerics;

namespace Fluid.ChannelFlow
{   
    /// <summary>Block on west side of obstruction.</summary><remarks>Sealed, because we call a virtual method in constructor.</remarks>
    public sealed class WestBlock : ObstructionBlock
    {   
        /// <summary>Create a mesh block just west of circular obstruction.</summary><param name="channelMesh">Main mesh.</param>
        public WestBlock(ChannelMesh channelMesh, ChannelFlow channelFlow, SouthBlock southBlock)
        : base(channelMesh, channelFlow,
            channelMesh.GetObstructionRect()._lL._x, channelMesh.GetObstructionRect()._lL._y,
            channelMesh.GetObstructionRect()._uL._x, channelMesh.GetObstructionRect()._uL._y,
            channelMesh.GetLeftSquare()._uL._x, channelMesh.GetLeftSquare()._uL._y,
            channelMesh.GetLeftSquare()._lL._x, channelMesh.GetLeftSquare()._lL._y) {
            
            CreateNodes();
            _constraintCount = ApplyConstraints();
            _ChannelMesh.SetConstraintCount(_ChannelMesh.GetConstraintCount() + _constraintCount);
            MoveNodesToMainMesh(southBlock);
        }


        /// <summary>Returns real-world position of upper boundary node located at a given arc length coordinate.</summary><param name="ksi">Arc length coordinate from 0 to 1.</param>
        protected override Pos CalcUpperBoundaryPos(double ksi) {

            ref var upperLeft = ref _quadrilateral._uL;
            double y = upperLeft._y + ksi * _ChannelMesh.GetWidth();
            double x = upperLeft._x;
            
            return new Pos(x,y);
        }

        /// <summary>Returns real-world position of lower boundary node located at a given arc length coordinate.</summary>param name="ksi">Arc length coordinate from 0 to 1.</param>
        protected override Pos CalcLowerBoundaryPos(double ksi) {

            ref var lowerLeft = ref _quadrilateral._lL;
            ref var lowerRight = ref _quadrilateral._lR;
            double y = lowerLeft._y + (lowerRight._y-lowerLeft._y) * (0.5 + Cos((PI/4)*(3-2*ksi)) / Sqrt(2));
            double x = lowerLeft._x - _quarterMoonHeight * (Sin((PI/4)*(3-2*ksi)) - 1/Sqrt(2)) / (1 - 1/Sqrt(2));

            return new Pos(x,y);
        }

        /// <summary>Returns real-world position of left boundary node located at a given arc length coordinate.</summary><param name="eta">Arc length coordinate from 0 to 1.</param>
        protected override Pos CalcLeftBoundaryPos(double eta) {

            ref var lowerLeft = ref _quadrilateral._lL;
            double y = lowerLeft._y - eta * _diagonalProjection;
            double x = lowerLeft._x - eta * _diagonalProjection;

            return new Pos(x,y);
        }

        /// <summary>Returns real-world position of left boundary node located at a given arc length coordinate.</summary><param name="eta">Arc length coordinate from 0 to 1.</param>
        protected override Pos CalcRightBoundaryPos(double eta) {

            ref var lowerRight = ref _quadrilateral._lR;
            double y = lowerRight._y + eta * _diagonalProjection;
            double x = lowerRight._x - eta * _diagonalProjection;

            return new Pos(x,y);
        }

        protected override int ApplyConstraints() {
            int constraintCount = 0;
            double v0 = _channelFlow.GetPeakInletVelocity();
            double w = _ChannelMesh.GetWidth();
                                                                                // Row 0 (obstruction).
            for(int col = 0; col < _colCount; ++col) {                              // Col 0 - 20
                for(int std = 0; std < 3; ++std) {                                  // Obstruction boundary.
                    SetObstructionConstraints(ref GetNodeCmp(0, col, std));
                }                                                                                                 
                SetCornerInletConstraints(ref GetNodeStd(22, col, 9));          // Row 22 (inlet), upper left corner. Set only corner nodes first for inlet. They are non-zero and if side node are to be set correctly, corner nodes must be set correctly first.
            }
            SetObstructionConstraints(ref GetNodeStd(0, 19, 3));                    // Lower right corner.
            SetCornerInletConstraints(ref GetNodeStd(22, 19, 6));                   // Upper right corner.

            for(int col = 0; col < _colCount; ++col) {                          // Row 0 (obstruction). Now set side nodes for inlet.
                SetSideInletConstraints(ref GetNodeStd(22, col, 9), ref GetNodeStd(22, col, 8), ref GetNodeStd(22, col, 7), ref GetNodeStd(22, col, 6));
            }
            return constraintCount;

            void SetObstructionConstraints(ref Msh.Node nodeRef) {
                nodeRef.Constrainedness(0) = true;                                  // u, 0 is set implicitly for both u and v due to them being value types.
                nodeRef.Constrainedness(1) = true;                                  // v
                constraintCount += 2;
            }

            void SetCornerInletConstraints(ref Msh.Node nodeRef) {                         // Applied to corners of elements.
                nodeRef.Constrainedness(0) = true;                                     // u = (4*v0/w)*y*(1-y/w)
                nodeRef.Var(0)._value = (4*v0/w) * nodeRef.GetY() * (1.0 - nodeRef.GetY()/w);
                nodeRef.Constrainedness(1) = true;                                     // v = 0
                nodeRef.Constrainedness(3) = true;                                     // b = (4*v0/w)*(1-y/w)
                nodeRef.Var(3)._value = (4*v0/w) * (1.0 - nodeRef.GetY()/w);
                nodeRef.Constrainedness(5) = true;                                     // p = 0
                constraintCount += 4;
            }

            void SetSideInletConstraints(ref Msh.Node nodeRef9, ref Msh.Node nodeRef8, ref Msh.Node nodeRef7, ref Msh.Node nodeRef6) {
                double u9 = nodeRef9.Var(0)._value;
                double h8 = (4*v0/w) * nodeRef8.GetY() * (1.0 - nodeRef8.GetY()/w);
                double h7 = (4*v0/w) * nodeRef7.GetY() * (1.0 - nodeRef7.GetY()/w);
                double u6 = nodeRef6.Var(0)._value;
                nodeRef8.Constrainedness(0) = true;                                     // u = (4*v0/w)*y*(1-y/w)
                nodeRef7.Constrainedness(0) = true;
                nodeRef8.Var(0)._value = 0.25 * (-9 * h7 + 18 * h8 + 2 * u6 - 11 * u9);
                nodeRef7.Var(0)._value = 0.25 * (18 * h7 - 9 * h8 - 11 * u6 + 2 * u9);
                nodeRef8.Constrainedness(1) = true;                                     // v = 0
                nodeRef7.Constrainedness(1) = true;
                u9 = nodeRef9.Var(3)._value;
                h8 = (4*v0/w) * (1.0 - nodeRef8.GetY()/w);
                h7 = (4*v0/w) * (1.0 - nodeRef7.GetY()/w);
                u6 = nodeRef6.Var(3)._value;
                nodeRef8.Constrainedness(3) = true;                                     // b = (4*v0/w)*(1-y/w)
                nodeRef7.Constrainedness(3) = true;
                nodeRef8.Var(3)._value = 0.25 * (-9 * h7 + 18 * h8 + 2 * u6 - 11 * u9);
                nodeRef7.Var(0)._value = 0.25 * (18 * h7 - 9 * h8 - 11 * u6 + 2 * u9);
                nodeRef8.Constrainedness(5) = true;                                     // p = 0
                nodeRef7.Constrainedness(5) = true;
                constraintCount += 8;
            }
        }

        void MoveNodesToMainMesh(SouthBlock southBlock) {
            int posCount = _ChannelMesh.GetPositionCount();
            var blockToGlobal = new int[_rowCount + 1][][];
            int row = 0;
            int col = 0;
            var southMap = southBlock.GetCompactPosIndexToGlobalPosIndexMap();      // We will need SouthBlock's map.

            while(row < _rowCount) {                                     // Rows 0 - 22
                blockToGlobal[row] = new int[_colCount + 1][];
                col = 0;
                blockToGlobal[row][col] = new int[5];                      // Col 0. Take in nodes from Col 20 of SouthBlock.
                for(int node = 0; node < 3; ++node) {
                    blockToGlobal[row][col][node] = southMap[row][20][node];
                }
                for(int node = 3; node < 5; ++node) {
                    _ChannelMesh.Node(posCount) = GetNodeCmp(row, col, node);
                    blockToGlobal[row][col][node] = posCount++;
                }
                col = 1;

                while(col < _colCount) {                                     // Cols 1 - 19
                    blockToGlobal[row][col] = new int[5];
                    for(int node = 0; node < 5; ++node) {
                        _ChannelMesh.Node(posCount) = GetNodeCmp(row, col, node);
                        blockToGlobal[row][col][node] = posCount++;
                    }
                    ++col;
                }
                blockToGlobal[row][col] = new int[5];                      // Col 20
                for(int node = 0; node < 3; ++node) {
                    _ChannelMesh.Node(posCount) = GetNodeCmp(row, col, node);
                    blockToGlobal[row][col][node] = posCount++;
                }
                for(int node = 3; node < 5; ++node) {
                    blockToGlobal[row][col][node] = Int32.MinValue;
                }
                ++row;
            }
            col = 0;                                                    // Row 23.
            blockToGlobal[row] = new int[_colCount+1][];
            blockToGlobal[row][col] = new int[5];                           // Col 0
            for(int node = 0; node < 2; ++node) {
                blockToGlobal[row][col][node] = Int32.MinValue;
            }
            blockToGlobal[row][col][2] = southMap[row][20][2];    // Take in node from Col 20 of SouthBlock.
            for(int node = 3; node < 5; ++node) {
                _ChannelMesh.Node(posCount) = GetNodeCmp(row, col, node);
                blockToGlobal[row][col][node] = posCount++;
            }
            col = 1;

            while(col < _colCount) {                                         // Cols 1 - 19
                blockToGlobal[row][col] = new int[5];
                for(int node = 0; node < 2; ++node) {
                    blockToGlobal[row][col][node] = Int32.MinValue;
                }
                for(int node = 2; node < 5; ++node) {
                    _ChannelMesh.Node(posCount) = GetNodeCmp(row, col, node);
                    blockToGlobal[row][col][node] = posCount++;
                }
                ++col;
            }
            blockToGlobal[row][col] = new int[5];                           // Col 20
            for(int node = 0; node < 2; ++node) {
                blockToGlobal[row][col][node] = Int32.MinValue;
            }
            _ChannelMesh.Node(posCount) = GetNodeCmp(row, col, 2);
            blockToGlobal[row][col][2] = posCount++;
            for(int node = 3; node < 5; ++node) {
                blockToGlobal[row][col][node] = Int32.MinValue;
            }
            SetCompactPosIndexToGlobalPosIndexMap(blockToGlobal);
            _ChannelMesh.SetPositionCount(posCount);
            _nodes = null;                                              // Free memory on block.
            GetNodeCmp = GetNodeCmpGlobal;                              // Rewire.
            GetNodeStd = GetNodeStdGlobal;
        }
    }
}