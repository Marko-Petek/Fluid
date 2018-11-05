using System;
using Fluid.Dynamics.Internals;
using Fluid.Dynamics.Meshing;
using Fluid.Dynamics.Numerics;
using static System.Math;

namespace Fluid.ChannelFlow
{
    /// <summary>Block on east side of obstruction.</summary><remarks>Sealed, because we call a virtual method in constructor.</remarks>
    public class EastBlock : ObstructionBlock
    {   
        /// <summary>Create a mesh block just east of circular obstruction.</summary><param name="channelMesh">Main mesh.</param>
        public EastBlock(ChannelMesh channelMesh, ChannelFlow channelFlow, NorthBlock northBlock, SouthBlock southBlock)
        : base(channelMesh, channelFlow,
            channelMesh.GetObstructionRect()._uR._x, channelMesh.GetObstructionRect()._uR._y,
            channelMesh.GetObstructionRect()._lR._x, channelMesh.GetObstructionRect()._lR._y,
            channelMesh.GetLeftSquare()._lR._x, channelMesh.GetLeftSquare()._lR._y,
            channelMesh.GetLeftSquare()._uR._x, channelMesh.GetLeftSquare()._uR._y) {
                
            CreateNodes();
            _constraintCount = ApplyConstraints();
            _channelMesh.SetConstraintCount(_channelMesh.GetConstraintCount() + _constraintCount);
            MoveNodesToMainMesh(northBlock, southBlock);
        }


        /// <summary>Returns real-world position of upper boundary node located at a given arc length coordinate.</summary><param name="ksi">Arc length coordinate from 0 to 1.</param>
        protected override Pos CalcUpperBoundaryPos(double ksi) {

            ref var upperLeft = ref _quadrilateral._uL;
            double y = upperLeft._y - ksi * _channelMesh.GetWidth();
            double x = upperLeft._x;
            
            return new Pos(x,y);
        }

        /// <summary>Returns real-world position of lower boundary node located at a given arc length coordinate.</summary>param name="ksi">Arc length coordinate from 0 to 1.</param>
        protected override Pos CalcLowerBoundaryPos(double ksi) {

            ref var lowerLeft = ref _quadrilateral._lL;
            ref var lowerRight = ref _quadrilateral._lR;
            double y = lowerLeft._y - (lowerLeft._y-lowerRight._y) * (0.5 + Cos((PI/4)*(3-2*ksi)) / Sqrt(2));
            double x = lowerLeft._x + _quarterMoonHeight * (Sin((PI/4)*(3-2*ksi)) - 1/Sqrt(2)) / (1 - 1/Sqrt(2));

            return new Pos(x,y);
        }

        /// <summary>Returns real-world position of left boundary node located at a given arc length coordinate.</summary><param name="eta">Arc length coordinate from 0 to 1.</param>
        protected override Pos CalcLeftBoundaryPos(double eta) {

            ref var lowerLeft = ref _quadrilateral._lL;
            double y = lowerLeft._y + eta * _diagonalProjection;
            double x = lowerLeft._x + eta * _diagonalProjection;

            return new Pos(x,y);
        }

        /// <summary>Returns real-world position of left boundary node located at a given arc length coordinate.</summary><param name="eta">Arc length coordinate from 0 to 1.</param>
        protected override Pos CalcRightBoundaryPos(double eta) {

            ref var lowerRight = ref _quadrilateral._lR;
            double y = lowerRight._y - eta * _diagonalProjection;
            double x = lowerRight._x + eta * _diagonalProjection;

            return new Pos(x,y);
        }

        protected override int ApplyConstraints() {        // We apply only to points on obstruction.
            int constraintCount = 0;

            for(int col = 0; col < 20; ++col) {             // Row 0, Cols 0-19 (obstruction)
                
                for(int node = 2; node < 5; ++node) {
                    GetNodeCmp(0, col, node).Constrainedness(0) = true;
                    GetNodeCmp(0, col, node).Constrainedness(1) = true;
                    constraintCount += 2;
                }
            }  
            return constraintCount;                                             // Must not count in points from col 20.

            // ref var node = ref GetNodeCmp(0, 0, 2);
            // node.Constrainedness(0) = true;
            // node.Constrainedness(1) = true;
            // node.Constrainedness(2) = true;
            // node.Constrainedness(4) = true;
            // node = ref GetNodeCmp(23, 20, 2);
            // node.Constrainedness(0) = true;
            // node.Constrainedness(1) = true;
            // node.Constrainedness(2) = true;
            // node.Constrainedness(4) = true;

        }

        protected void MoveNodesToMainMesh(NorthBlock northBlock, SouthBlock southBlock) {
            int posCount = _channelMesh.GetPositionCount();
            var blockToGlobal = new int[_rowCount + 1][][];
            int row = 0;
            int col = 0;
            var northMap = northBlock.GetCompactPosIndexToGlobalPosIndexMap();          // We will need NorthBlock's map.
            var southMap = southBlock.GetCompactPosIndexToGlobalPosIndexMap();          // We will also need SouthBlock's map.

            while(row < _rowCount) {                                            // Rows 0 - 22
                blockToGlobal[row] = new int[_colCount + 1][];
                col = 0;
                blockToGlobal[row][col] = new int[5];
                for(int node = 0; node < 3; ++node) {                               // Col 0, take in nodes from Col 20 of NorthBlock.
                    blockToGlobal[row][col][node] = northMap[row][20][node];
                }
                for(int node = 3; node < 5; ++node) {
                    _channelMesh.Node(posCount) = GetNodeCmp(row, col, node);
                    blockToGlobal[row][col][node] = posCount++;
                }
                col = 1;

                while(col < _colCount) {                                            // Cols 1 - 19
                    blockToGlobal[row][col] = new int[5];
                    for(int node = 0; node < 5; ++node) {
                        _channelMesh.Node(posCount) = GetNodeCmp(row, col, node);
                        blockToGlobal[row][col][node] = posCount++;
                    }
                    ++col;
                }
                blockToGlobal[row][col] = new int[5];                       // Col 20, Take in nodes from Col 0 of SouthBlock.
                for(int node = 0; node < 3; ++node) {
                    blockToGlobal[row][col][node] = southMap[row][0][node];
                }
                for(int node = 3; node < 5; ++node) {
                    blockToGlobal[row][col][node] = Int32.MinValue;
                }
                ++row;
            }
            col = 0;                                                    // Row 23.
            blockToGlobal[row] = new int[_colCount+1][];
            blockToGlobal[row][col] = new int[5];                           // Col 0, take in node from Col 20 of NorthBlock.
            for(int node = 0; node < 2; ++node) {
                blockToGlobal[row][col][node] = Int32.MinValue;
            }
            blockToGlobal[row][col][2] = northMap[row][20][2];
            for(int node = 3; node < 5; ++node) {
                _channelMesh.Node(posCount) = GetNodeCmp(row, col, node);
                blockToGlobal[row][col][node] = posCount++;
            }
            col = 1;

            while(col < _colCount) {                                         // Cols 1 - 19
                blockToGlobal[row][col] = new int[5];
                for(int node = 0; node < 2; ++node) {
                    blockToGlobal[row][col][node] = Int32.MinValue;
                }
                for(int node = 2; node < 5; ++node) {
                _channelMesh.Node(posCount) = GetNodeCmp(row, col, node);
                blockToGlobal[row][col][node] = posCount++;
                }
                ++col;
            }
            blockToGlobal[row][col] = new int[5];                           // Col 20, Take in nodes from Col 0 of SouthBlock.
            for(int node = 0; node < 2; ++node) {
                blockToGlobal[row][col][node] = Int32.MinValue;
            }
            blockToGlobal[row][col][2] = southMap[row][0][2];
            for(int node = 3; node < 5; ++node) {
                blockToGlobal[row][col][node] = Int32.MinValue;
            }
            SetCompactPosIndexToGlobalPosIndexMap(blockToGlobal);
            _channelMesh.SetPositionCount(posCount);
            _nodes = null;                                              // Free memory on block.
            GetNodeCmp = GetNodeCmpGlobal;                              // Rewire.
            GetNodeStd = GetNodeStdGlobal;
        }
    }
}