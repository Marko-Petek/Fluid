using System;
using static System.Math;

using Fluid.Internals.Collections;
using Fluid.Internals.Numerics;
using static Fluid.Internals.Numerics.MatrixOperations;

namespace Fluid.Internals.Meshing
{
    /// <summary>Represents a method that takes three indices and returns a position by reference.</summary>
    public delegate ref Node NodeDelegate(int blockRow, int blockCol, int index);


    /// <summary>A structured submesh that provides access to global node indices via element indices.</summary>
    public abstract class MeshBlock
    {   
        /// <summary>Reference to owner mesh. Access to global nodes list.</summary>
        protected BlockStructuredMesh _mainMesh;
        /// <summary>Number of rows of elements.</summary>
        protected int _rowCount;
        /// <summary>Number of columns of elements.</summary>
        protected int _colCount;
        /// <summary>Node positions in compact form.</summary>
        protected Node[][][] _nodes;
        /// <summary>Global position indices accessible through compact block position indices.</summary><remarks>Compact block pos ordering: [element row of pos][element column of pos][compact pos index (0 - 4)], global ordering: [pos index].</remarks>
        protected int[][][] _compactPosIndexToGlobalPosIndexMap;
        /// <summary>Number of rows of elements.</summary>
        public int GetRowCount() => _rowCount;
        /// <summary>Number of columns of elements.</summary>
        public int GetColCount() => _colCount;
        /// <summary>Number of unique constraints set on this block.</summary>
        protected int _constraintCount;

        /// <summary>Number of unique constraints set on this block.</summary>
        public int GetConstraintCount() => _constraintCount;
        /// <summary>Mapping from compact block ordering of positions to main mesh'es ordering of positions.</summary><remarks>Compact local ordering: [element row][element column][1 of 5 lower left corner element nodes], global ordering: [node index].</remarks>
        public int[][][] GetCompactPosIndexToGlobalPosIndexMap() => _compactPosIndexToGlobalPosIndexMap;
        /// <summary>Set mapping from compact block ordering of positions to main mesh'es ordering of positions.</summary><remarks>Compact local ordering: [element row][element column][1 of 5 lower left corner element nodes], global ordering: [node index].</remarks>
        public int[][][] SetCompactPosIndexToGlobalPosIndexMap(int[][][] value) => _compactPosIndexToGlobalPosIndexMap = value;

        /// <summary>Intended for testing.</summary>
        public MeshBlock() {

        }

        /// <summary>Create a SubMesh and assign reference to main mesh.</summary><param name="mainMesh">Owner of this submesh who will use it fill _indexMap, mapping positions to _globalNodes list.</param>
        public MeshBlock(BlockStructuredMesh mainMesh) {
            _mainMesh = mainMesh;
            GetNodeCmp = GetNodeCmpLocal;
            GetNodeStd = GetNodeStdLocal;
        }

        /// <summary>Get reference to Position with specified compact index.</summary><param name="blockRow">Row of element (in which sought after position is located) inside block.</param><param name="blockCol">Column of element (in which sought after position is located) inside block.</param><param name="standardPosIndex">Standard element position index (0 - 11).</param><remarks>We use a delegate because first, we create all positions and add them to local positions array (delegate points to this array), then we move positions to a 1D global map on main mesh (delegate is then rewired to point there.)</remarks>
        public NodeDelegate GetNodeCmp;

        /// <summary>Get reference to Position with specified standard index.</summary><param name="blockRow">Row of element (in which sought after position is located) inside block.</param><param name="blockCol">Column of element (in which sought after position is located) inside block.</param><param name="standardPosIndex">Standard element position index (0 - 11).</param><remarks>We use a delegate because first, we create all positions and add them to local positions array (delegate points to this array), then we move positions to a 1D global map on main mesh (delegate is then rewired to point there.)</remarks>
        public NodeDelegate GetNodeStd;

        /// <summary>Get reference to Position (residing on MeshBlock) with specified compact index.</summary><param name="blockRow">Row of element (in which sought after position is located) inside block.</param><param name="blockCol">Column of element (in which sought after position is located) inside block.</param><param name="compactPosIndex">Compact element node position index (0 - 4).</param>
        ref Node GetNodeCmpLocal(int blockRow, int blockCol, int compactPosIndex) {
            return ref _nodes[blockRow][blockCol][compactPosIndex];
        }

        /// <summary>Get reference to Position (residing on MeshBlock) with specified standard index.</summary><param name="blockRow">Row of element (in which sought after position is located) inside block.</param><param name="blockCol">Column of element (in which sought after position is located) inside block.</param><param name="standardPosIndex">Standard element position index (0 - 11).</param>
        ref Node GetNodeStdLocal(int blockRow, int blockCol, int standardPosIndex) {
            (int actualBlockRow,
             int actualBlockCol,
             int compactPosIndex) = CompactPosIndex(blockRow, blockCol, standardPosIndex);
            return ref _nodes[actualBlockRow][actualBlockCol][compactPosIndex];
        }

        /// <summary>We switch the GetNodeCmp delegate to point here after all positions are created and nodes are transfered to main mesh.</summary><param name="blockRow">Row of element (in which sought after position is located) inside block.</param><param name="blockCol">Column of element (in which sought after position is located) inside block.</param><param name="compactPosIndex">Compact element node position index (0 - 4).</param>
        protected ref Node GetNodeCmpGlobal(int blockRow, int blockCol, int compactPosIndex) {
            int globalIndex = GlobalPositionIndexCmp(blockRow, blockCol, compactPosIndex);
            return ref _mainMesh.Node(globalIndex);
        }

        /// <summary>We switch the GetNodeStd delegate to point here after all positions are created and nodes are transfered to main mesh.</summary><param name="blockRow">Row of element (in which sought after position is located) inside block.</param><param name="blockCol">Column of element (in which sought after position is located) inside block.</param><param name="standardPosIndex">Standard element position index (0 - 11).</param>
        protected ref Node GetNodeStdGlobal(int blockRow, int blockCol, int standardPosIndex) {
            (int actualBlockRow,
             int actualBlockCol,
             int compactPosIndex) = CompactPosIndex(blockRow, blockCol, standardPosIndex);
             int globalIndex = GlobalPositionIndexCmp(actualBlockRow, actualBlockCol, compactPosIndex);
            return ref _mainMesh.Node(globalIndex);
        }

        /// <summary>Returns three actual indices (0 - 4) of node positions array when we specify them in conventional notation (0 -11).</summary><param name="blockRow">Row of element inside this block.</param><param name="blockCol">Column of element inside this block.</param><param name="standardPosIndex">Index of node inside element (1 - 11).</param>
        public (int actualBlockRow,
                int actualBlockCol,
                int compactPosIndex) CompactPosIndex(int blockRow, int blockCol, int standardPosIndex) {
            
            if(blockRow < _rowCount) {              // Dummy elements at ends.

                if(blockCol < _colCount) {

                    if(standardPosIndex < 3) {
                        return (blockRow, blockCol, standardPosIndex + 2);
                    }
                    else if(standardPosIndex < 6) {
                        return (blockRow, blockCol + 1, 5 - standardPosIndex);
                    }
                    else if(standardPosIndex == 6) {
                        return (blockRow + 1, blockCol + 1, 2);
                    }
                    else if(standardPosIndex < 10) {
                        return (blockRow + 1, blockCol, 11 - standardPosIndex);
                    }
                    else if(standardPosIndex < 12){
                        return (blockRow, blockCol, standardPosIndex - 10);
                    }
                    else {
                        throw new IndexOutOfRangeException("Standard index above 11.");
                    }
                }
                else{                                  // Cannot access dummy.
                    throw new IndexOutOfRangeException("Block column index of element to large.");
                }
            }
            else {
                throw new IndexOutOfRangeException("Block row index of element to large.");
            }
        }

        /// <summary>Global position index in terms of compact position index.</summary><param name="blockRow">Element's row (in which sought after position is located) inside block.</param><param name="blockColumn">Element's column (in which sought after position is located) inside block.</param><param name="localNodeIndex">Compact index of position inside element (0 - 4).</param>
        protected int GlobalPositionIndexCmp(int blockRow, int blockCol, int compactPosIndex) {
            return _compactPosIndexToGlobalPosIndexMap[blockRow][blockCol][compactPosIndex];
        }

        /// <summary>Global position index in terms of standard position index.</summary><param name="blockRow">Element's row (in which sought after position is located) inside block.</param><param name="blockColumn">Element's column (in which sought after position is located) inside block.</param><param name="localNodeIndex">Index of position inside element (0 - 11).</param>
        protected int GlobalPositionIndexStd(int blockRow, int blockCol, int standardPosIndex) {
            (int actualBlockRow,
             int actualBlockCol,
             int compactPosIndex) = CompactPosIndex(blockRow, blockCol, standardPosIndex);
            return _compactPosIndexToGlobalPosIndexMap[actualBlockRow][actualBlockCol][compactPosIndex];
        }

        // public Quadrilateral CreateElement(ref Position ll, ref Position lr, ref Position ur, ref Position ul) {
        //     return new Quadrilateral(ref ll, ref lr, ref ur, ref ul);
        // }
    
        /// <summary>Fill list of node positions and let main mesh fill in local-to-global index map. Then rewire the Node delegate.</summary>
        protected abstract void CreateNodes();

        /// <summary>Set values of constrained nodes and set their Constrainedness property to true. Returns number of constrained nodes.</summary>
        protected abstract int ApplyConstraints();

        /// <summary>Add whole block's contribution to global stiffness matrix.</summary><param name="A">Gloabal stiffness matrix.</param><param name="dt">Time step.</param><param name="ni">Viscosity.</param>
        public abstract void AddContributionsToStiffnessMatrix(SparseMatrix<double> A, double dt, double ni);

        public abstract void AddContributionsToForcingVector(SparseRow<double> b, double dt, double ni);

        /// <summary>Creates an 8 x 8 matrix belonging to to a single Node vector.</summary><param name="node">Node whose values of which will be used inside operator matrix.</param><param name="dt">Time step.</param><param name="ni">Viscosity coefficient.</param>
        protected double[][] NodeOperatorMatrix0(ref Node node, double dt, double ni) {
            double[][] A = new double[8][] {
                new double[8] { 1.0/dt + node.Var(2)._value, node.Var(3)._value, node.Var(0)._value, node.Var(1)._value, 0, 0, 1, 0 },
                new double[8] { node.Var(4)._value, 1.0/dt - node.Var(2)._value, -node.Var(1)._value, 0, node.Var(0)._value, 0, 0, 1 },
                new double[8] { 0, 0, 1, 0, 0, 0, 0, 0 },
                new double[8] { 0, 0, 0, 1, 0, 0, 0, 0 },
                new double[8] { 0, 0, 0, 0, 1, 0, 0, 0 },
                new double[8] { 0, 0, 0, 0, 0, 0, 1, 0 },
                new double[8] { 0, 0, 0, 0, 0, 0, 0, 1 },
                new double[8] { 0, 0, 0, 0, 0, 0, 0, 0 }
            };
            return A;
        }

        protected double[][] NodeOperatorMatrix1(ref Node node, double dt, double ni) {
            double[][] A = new double[8][] {
                new double[8] { 0, 0, -ni, 0, 0, 0, 0, 0 },
                new double[8] { 0, 0, 0, 0, -ni, 0, 0, 0 },
                new double[8] { -1, 0, 0, 0, 0, 0, 0, 0 },
                new double[8] { 0, 0, 0, 0, 0, 0, 0, 0 },
                new double[8] { 0, -1, 0, 0, 0, 0, 0, 0 },
                new double[8] { 0, 0, 0, 0, 0, -1, 0, 0 },
                new double[8] { 0, 0, 0, 0, 0, 0, 0, 0 },
                new double[8] { 0, 0, 0, 0, 0, 0, 0, -1 }
            };
            return A;
        }

        protected double[][] NodeOperatorMatrix2(ref Node node, double dt, double ni) {
            double[][] A = new double[8][] {
                new double[8] { 0, 0, 0, -ni, 0, 0, 0, 0 },
                new double[8] { 0, 0, ni, 0, 0, 0, 0, 0 },
                new double[8] { 0, 0, 0, 0, 0, 0, 0, 0 },
                new double[8] { -1, 0, 0, 0, 0, 0, 0, 0 },
                new double[8] { 0, 0, 0, 0, 0, 0, 0, 0 },
                new double[8] { 0, 0, 0, 0, 0, 0, 0, 0 },
                new double[8] { 0, 0, 0, 0, 0, -1, 0, 0 },
                new double[8] { 0, 0, 0, 0, 0, 0, 1, 0 }
            };
            return A;
        }

        /// <summary>Find solution value of specified variables at specified point.</summary><param name="x">X coordinate.</param><param name="y">Y coordinate.</param><param name="vars">Indices of variables we wish to retrieve.</param>
        public virtual double[] Solution(ref Pos pos, params int[] vars) {
            int startRow = 0;                                                                   // Where current frame begins.
            int endRow = _rowCount - 1;                                                         // Where current frame ends.
            int startCol = 0;
            int endCol = _colCount - 1;
            int nRows = _rowCount;                                                              // Row count of current frame.
            int nCols = _colCount;
            var vertices = new Pos[4];
            vertices[0] = GetNodeStd(startRow, startCol, 0)._pos;
            vertices[1] = GetNodeStd(startRow, endCol, 3)._pos;
            vertices[2] = GetNodeStd(endRow, endCol, 6)._pos;
            vertices[3] = GetNodeStd(endRow, startCol, 9)._pos;
            
            int newEndRow = 0;
            int newEndCol = 0;

            while(nRows > 1 || nCols > 1) {                                                     // As long as we have not narrowed our frame down to a single element.

                if(nRows > 1) {
                    newEndRow = startRow + nRows/2 - 1;                                         // Set row at half frame width as end. No problem if nRows is odd.
                    vertices[3] = GetNodeStd(newEndRow, startCol, 9)._pos;                      // New upper left.
                    vertices[2] = GetNodeStd(newEndRow, endCol, 6)._pos;                        // New upper right.

                    if(pos.IsInsidePolygon(ref vertices)) {
                        endRow = newEndRow;
                        vertices[3] = GetNodeStd(endRow, startCol, 9)._pos;                     // UL
                        vertices[2] = GetNodeStd(endRow, endCol, 6)._pos;                       // UR
                    }
                    else {
                        startRow = newEndRow + 1;
                        vertices[0] = GetNodeStd(startRow, startCol, 0)._pos;                   // LL
                        vertices[1] = GetNodeStd(startRow, endCol, 3)._pos;                     // LR
                    }
                    nRows = endRow - startRow + 1;
                }

                if(nCols > 1) {
                    newEndCol = startCol + nCols/2 - 1;
                    vertices[1] = GetNodeStd(startRow, newEndCol, 3)._pos;                  // new LR.
                    vertices[2] = GetNodeStd(endRow, newEndCol, 6)._pos;

                    if(pos.IsInsidePolygon(ref vertices)) {
                        endCol = newEndCol;
                        vertices[1] = GetNodeStd(startRow, endCol, 3)._pos;
                        vertices[2] = GetNodeStd(endRow, endCol, 6)._pos;
                    }
                    else {
                        startCol = newEndCol + 1;
                        vertices[3] = GetNodeStd(endRow, startCol, 9)._pos;
                        vertices[0] = GetNodeStd(startRow, startCol, 0)._pos;
                    }
                    nCols = endCol - startCol + 1;
                }
            }                                                                                   // At this point startCol and endCol have to be the same.
            var quadElm = CreateQuadElement(startRow, startCol);                          // Quadrilateral that contains sought after point.
            var squarePos = quadElm.ReferenceSquareCoords(ref pos);
            double[] funcValues = quadElm.Values(ref squarePos, vars);
            return funcValues;
        }
    
        /// <summary>Creates a data structure which holds all four corner nodes of an element.</summary><param name="stdRow">Element's row inside mesh block.</param><param name="stdCol">Element's col inside mesh block.</param>
        QuadElement CreateQuadElement(int stdRow, int stdCol) {
            var quadElm = new QuadElement(
                ref GetNodeStd(stdRow, stdCol, 0),
                ref GetNodeStd(stdRow, stdCol, 1),
                ref GetNodeStd(stdRow, stdCol, 2),
                ref GetNodeStd(stdRow, stdCol, 3),
                ref GetNodeStd(stdRow, stdCol, 4),
                ref GetNodeStd(stdRow, stdCol, 5),
                ref GetNodeStd(stdRow, stdCol, 6),
                ref GetNodeStd(stdRow, stdCol, 7),
                ref GetNodeStd(stdRow, stdCol, 8),
                ref GetNodeStd(stdRow, stdCol, 9),
                ref GetNodeStd(stdRow, stdCol, 10),
                ref GetNodeStd(stdRow, stdCol, 11)
            );
            return quadElm;
        }

        /// <summary>Determines whether specified position is inside MeshBlock.</summary><param name="pos">Point's position.</param>
        public abstract bool IsPointInside(ref Pos pos);
    }
}