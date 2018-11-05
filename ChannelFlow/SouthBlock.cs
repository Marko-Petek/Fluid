using System;
using Fluid.Dynamics.Internals;
using Fluid.Dynamics.Meshing;
using Fluid.Dynamics.Numerics;
using static System.Math;

namespace Fluid.ChannelFlow
{   
    /// <summary>Block on south side of obstruction.</summary><remarks>Sealed, because we call a virtual method in constructor.</remarks>
    public sealed class SouthBlock : ObstructionBlock
    {   
        /// <summary>Create a mesh block just south of circular obstruction.</summary><param name="channelMesh">Main mesh.</param>
        public SouthBlock(ChannelMesh channelMesh, ChannelFlow channelFlow)
        : base(channelMesh, channelFlow,
            channelMesh.GetObstructionRect()._lR._x, channelMesh.GetObstructionRect()._lR._y,
            channelMesh.GetObstructionRect()._lL._x, channelMesh.GetObstructionRect()._lL._y,
            channelMesh.GetLeftSquare()._lL._x, channelMesh.GetLeftSquare()._lL._y,
            channelMesh.GetLeftSquare()._lR._x, channelMesh.GetLeftSquare()._lR._y) {
                
            CreateNodes();
            _constraintCount = ApplyConstraints();
            _channelMesh.SetConstraintCount(_channelMesh.GetConstraintCount() + _constraintCount);
            MoveNodesToMainMesh();
        }


        /// <summary>Returns real-world position of upper boundary node located at a given arc length coordinate.</summary><param name="ksi">Arc length coordinate from 0 to 1.</param>
        protected override Pos CalcUpperBoundaryPos(double ksi) {

            ref var upperLeft = ref _quadrilateral._uL;
            double x = upperLeft._x - ksi * _channelMesh.GetWidth();
            double y = upperLeft._y;
            
            return new Pos(x,y);
        }

        /// <summary>Returns real-world position of lower boundary node located at a given arc length coordinate.</summary>param name="ksi">Arc length coordinate from 0 to 1.</param>
        protected override Pos CalcLowerBoundaryPos(double ksi) {

            ref var lowerLeft = ref _quadrilateral._lL;
            ref var lowerRight = ref _quadrilateral._lR;
            double x = lowerLeft._x + (lowerRight._x-lowerLeft._x) * (0.5 + Cos((PI/4)*(3-2*ksi)) / Sqrt(2));
            double y = lowerLeft._y - _quarterMoonHeight * (Sin((PI/4)*(3-2*ksi)) - 1/Sqrt(2)) / (1 - 1/Sqrt(2));

            return new Pos(x,y);
        }

        /// <summary>Returns real-world position of left boundary node located at a given arc length coordinate.</summary><param name="eta">Arc length coordinate from 0 to 1.</param>
        protected override Pos CalcLeftBoundaryPos(double eta) {

            ref var lowerLeft = ref _quadrilateral._lL;
            double x = lowerLeft._x + eta * _diagonalProjection;
            double y = lowerLeft._y - eta * _diagonalProjection;

            return new Pos(x,y);
        }

        /// <summary>Returns real-world position of left boundary node located at a given arc length coordinate.</summary><param name="eta">Arc length coordinate from 0 to 1.</param>
        protected override Pos CalcRightBoundaryPos(double eta) {

            ref var lowerRight = ref _quadrilateral._lR;
            double x = lowerRight._x - eta * _diagonalProjection;
            double y = lowerRight._y - eta * _diagonalProjection;

            return new Pos(x,y);
        }

        protected override int ApplyConstraints() {
            int col = 0;
            int constraintsCount = 0;

            while(col < _colCount) {                                            // Col 0 - 22
                
                for(int element = 2; element < 5; ++element) {                  // Obstruction boundary.
                    GetNodeCmp(0, col, element).Constrainedness(0) = true;      // u, 0 is set implicitly for both u and v due to them being value types.
                    GetNodeCmp(0, col, element).Constrainedness(1) = true;      // v
                    constraintsCount += 2;
                }

                for(int element = 2; element < 5; ++element) {                  // Channel boundary.
                    GetNodeCmp(23, col, element).Constrainedness(0) = true;     // u
                    GetNodeCmp(23, col, element).Constrainedness(1) = true;     // v
                    GetNodeCmp(23, col, element).Constrainedness(2) = true;     // a
                    GetNodeCmp(23, col, element).Constrainedness(4) = true;     // c
                    constraintsCount += 4;
                }
                ++col;
            }
            // GetNodeCmp(0, col, 2).Constrainedness(0) = true;                     // Obstruction boundary, u, Col 23   !Already set.
            // GetNodeCmp(0, col, 2).Constrainedness(1) = true;                     // v
            // GetNodeCmp(23, col, 2).Constrainedness(0) = true;                    // Channel boundary, u.
            // GetNodeCmp(23, col, 2).Constrainedness(1) = true;                    // v
            // GetNodeCmp(23, col, 2).Constrainedness(2) = true;                    // a
            // GetNodeCmp(23, col, 2).Constrainedness(4) = true;                    // c
            return constraintsCount;
        }

        void MoveNodesToMainMesh() {
            int posCount = _channelMesh.GetPositionCount();                         // mapping
            var blockToGlobal = new int[_rowCount + 1][][];
            int row = 0, col = 0;
                                                    
            while(row < _rowCount) {                                                // Row 0 - 22.
                blockToGlobal[row] = new int[_colCount + 1][];
                col = 0;

                while(col < _colCount) {                                            // Col 0 - 19.
                    blockToGlobal[row][col] = new int[5];
                    for(int node = 0; node < 5; ++node) {
                        _channelMesh.Node(posCount) = GetNodeCmp(row,col,node);
                        blockToGlobal[row][col][node] = posCount++;
                    }
                    ++col;
                }
                blockToGlobal[row][col] = new int[5];
                for(int node = 0; node < 3; ++node) {
                    _channelMesh.Node(posCount) = GetNodeCmp(row,col,node);
                    blockToGlobal[row][col][node] = posCount++;
                }
                for(int node = 3; node < 5; ++node) {                           // Col 20.
                    blockToGlobal[row][col][node] = Int32.MinValue;
                }
                ++row;
            }
            col = 0;                                                    // Row 23.
            blockToGlobal[row] = new int[_colCount + 1][];

            while(col < _colCount) {                                         // Col 0 - 19
                blockToGlobal[row][col] = new int[5];
                for(int node = 0; node < 2; ++node) {
                    blockToGlobal[row][col][node] = Int32.MinValue;
                }
                for(int node = 2; node < 5; ++node) {
                    _channelMesh.Node(posCount) = GetNodeCmp(row,col,node);
                    blockToGlobal[row][col][node] = posCount++;
                }
                ++col;
            }
            blockToGlobal[row][col] = new int[5];                           // Col 20.
            for(int node = 0; node < 2; ++node) {
                blockToGlobal[row][col][node] = Int32.MinValue;
            }
            _channelMesh.Node(posCount) = GetNodeCmp(row,col,2);
            blockToGlobal[row][col][2] = posCount++;
            for(int node = 3; node < 5; ++node) {
                blockToGlobal[row][col][node] = Int32.MinValue;
            }
            SetCompactPosIndexToGlobalPosIndexMap(blockToGlobal);        // Apply local to field.
            _channelMesh.SetPositionCount(posCount);                   // Last global index to pass to next block which will generate local to global mapping.
            _nodes = null;                                              // Free memory on block.
            GetNodeCmp = GetNodeCmpGlobal;                              // Rewire.
            GetNodeStd = GetNodeStdGlobal;
        }
    }
}