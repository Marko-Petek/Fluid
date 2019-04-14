using System;
using static System.Math;

using Fluid.Internals.Collections;
using Fluid.Internals.Numerics;
using static Fluid.Internals.Numerics.MatOps;

namespace Fluid.Internals.Meshing {
   using SparseMat = SparseMat<double,DblArithmetic>;
   using SparseRow = SparseRow<double,DblArithmetic>;
   /// <summary>Represents a method that takes three indices and returns a position by reference.</summary>
   public delegate ref MeshNode NodeDelegate(int blockRow, int blockCol, int index);

   // cmt = compact, std = standard, lcl = local, gbl = global; descriptors connected to position indices.
   /// <summary>A structured submesh that provides access to global node indices via element indices.</summary>
   public abstract class MeshBlock {   
      /// <summary>Owner mesh. Access to global nodes list.</summary>
      public BlockStructuredMesh MainMesh { get; protected set; }
      /// <summary>Number of rows of elements.</summary>
      public int RowCount { get; protected set; }
      /// <summary>Number of columns of elements.</summary>
      public int ColCount { get; protected set; }
      /// <summary>Node positions in compact form.</summary>
      protected MeshNode[][][] _Nodes;
      /// <summary>Node positions in compact form.</summary>
      public MeshNode[][][] Nodes => _Nodes;
      /// <summary>Global position indices accessible through compact block position indices.</summary><remarks>Compact block pos ordering: [element row of pos][element column of pos][compact pos index (0 - 4)], global ordering: [pos index].</remarks>
      protected int[][][] _CmtInxToGblInxMap;
      /// <summary>Global position indices accessible through compact block position indices.</summary><remarks>Compact block pos ordering: [element row of pos][element column of pos][compact pos index (0 - 4)], global ordering: [pos index].</remarks>
      public int[][][] CmtInxToGblInxMap => _CmtInxToGblInxMap;
      /// <summary>Number of unique constraints set on this block.</summary>
      public int NConstraints { get; protected set; }
      /// <summary>Get reference to Position with specified compact index.</summary><param name="blockRow">Row of element (in which sought after position is located) inside block.</param><param name="blockCol">Column of element (in which sought after position is located) inside block.</param><param name="standardPosIndex">Standard element position index (0 - 11).</param><remarks>We use a delegate because first, we create all positions and add them to local positions array (delegate points to this array), then we move positions to a 1D global map on main mesh (delegate is then rewired to point there.)</remarks>
      public NodeDelegate NodeCmt;
      /// <summary>Get reference to Position with specified standard index.</summary><param name="blockRow">Row of element (in which sought after position is located) inside block.</param><param name="blockCol">Column of element (in which sought after position is located) inside block.</param><param name="standardPosIndex">Standard element position index (0 - 11).</param><remarks>We use a delegate because first, we create all positions and add them to local positions array (delegate points to this array), then we move positions to a 1D global map on main mesh (delegate is then rewired to point there.)</remarks>
      public NodeDelegate NodeStd;



      /// <summary>Intended for testing.</summary>
      public MeshBlock() { }
      /// <summary>Create a SubMesh and assign reference to main mesh.</summary><param name="mainMesh">Owner of this submesh who will use it fill _indexMap, mapping positions to _globalNodes list.</param>
      public MeshBlock(BlockStructuredMesh mainMesh) {
         MainMesh = mainMesh;
         NodeCmt = NodeCmtLocal;
         NodeStd = NodeStdLocal;
      }

      /// <summary>Get reference to Position (residing on MeshBlock) with specified compact index.</summary><param name="blockRow">Row of element (in which sought after position is located) inside block.</param><param name="blockCol">Column of element (in which sought after position is located) inside block.</param><param name="compactPosIndex">Compact element node position index (0 - 4).</param>
      ref MeshNode NodeCmtLocal(int blockRow, int blockCol, int compactPosIndex) {
         return ref _Nodes[blockRow][blockCol][compactPosIndex];
      }
      /// <summary>Get reference to Position (residing on MeshBlock) with specified standard index.</summary><param name="blockRow">Row of element (in which sought after position is located) inside block.</param><param name="blockCol">Column of element (in which sought after position is located) inside block.</param><param name="standardPosIndex">Standard element position index (0 - 11).</param>
      ref MeshNode NodeStdLocal(int blockRow, int blockCol, int standardPosIndex) {
         (int actualBlockRow, int actualBlockCol, int compactPosIndex) =
            CmtInxFromStdInx(blockRow, blockCol, standardPosIndex);
         return ref _Nodes[actualBlockRow][actualBlockCol][compactPosIndex];
      }
      /// <summary>We switch the GetNodeCmp delegate to point here after all positions are created and nodes are transfered to main mesh.</summary><param name="blockRow">Row of element (in which sought after position is located) inside block.</param><param name="blockCol">Column of element (in which sought after position is located) inside block.</param><param name="compactPosIndex">Compact element node position index (0 - 4).</param>
      protected ref MeshNode NodeCmtGlobal(int blockRow, int blockCol, int compactPosIndex) {
         int globalIndex = GblInxFromCmpInx(blockRow, blockCol, compactPosIndex);
         return ref MainMesh.Node(globalIndex);
      }
      /// <summary>We switch the GetNodeStd delegate to point here after all positions are created and nodes are transfered to main mesh.</summary><param name="blockRow">Row of element (in which sought after position is located) inside block.</param><param name="blockCol">Column of element (in which sought after position is located) inside block.</param><param name="standardPosIndex">Standard element position index (0 - 11).</param>
      protected ref MeshNode NodeStdGlobal(int blockRow, int blockCol, int standardPosIndex) {
         (int actualBlockRow,int actualBlockCol,int compactPosIndex)
         = CmtInxFromStdInx(blockRow, blockCol, standardPosIndex);
         int globalIndex = GblInxFromCmpInx(actualBlockRow, actualBlockCol, compactPosIndex);
         return ref MainMesh.Node(globalIndex);
      }
      /// <summary>Returns three actual indices (0 - 4) of node positions array when we specify them in conventional notation (0 -11).</summary><param name="blockRowArg">Row of element inside this block.</param><param name="blockColArg">Column of element inside this block.</param><param name="stdIndexArg">Index of node inside element (1 - 11).</param>
      public (int blockRow,int blockCol,int cmtIndex) CmtInxFromStdInx(
         int blockRowArg, int blockColArg, int stdIndexArg) {
            if(blockRowArg < RowCount) {                                                     // Dummy elements at ends.
               if(blockColArg < ColCount) {
                  if(stdIndexArg < 3)
                     return (blockRowArg, blockColArg, stdIndexArg + 2);
                  else if(stdIndexArg < 6)
                     return (blockRowArg, blockColArg + 1, 5 - stdIndexArg);
                  else if(stdIndexArg == 6)
                     return (blockRowArg + 1, blockColArg + 1, 2);
                  else if(stdIndexArg < 10)
                     return (blockRowArg + 1, blockColArg, 11 - stdIndexArg);
                  else if(stdIndexArg < 12)
                     return (blockRowArg, blockColArg, stdIndexArg - 10);
                  else
                     throw new IndexOutOfRangeException("Standard index above 11."); }
               else                                                                          // Cannot access dummy.
                  throw new IndexOutOfRangeException(
                     "Block column index of element to large."); }
            else
               throw new IndexOutOfRangeException("Block row index of element to large.");
      }
      /// <summary>Global position index in terms of compact position index.</summary><param name="blockRow">Element's row (in which sought after position is located) inside block.</param><param name="blockColumn">Element's column (in which sought after position is located) inside block.</param><param name="localNodeIndex">Compact index of position inside element (0 - 4).</param>
      protected int GblInxFromCmpInx(int blockRow, int blockCol, int cmtIndex) {
         return _CmtInxToGblInxMap[blockRow][blockCol][cmtIndex];
      }
      /// <summary>Global position index in terms of standard position index.</summary><param name="blockRowArg">Element's row (in which sought after position is located) inside block.</param><param name="blockColumn">Element's column (in which sought after position is located) inside block.</param><param name="localNodeIndex">Index of position inside element (0 - 11).</param>
      protected int GblInxFromStdInx(int blockRowArg, int blockColArg, int stdIndex) {
         (int blockRow,int blockCol,int cmtIndex) =
            CmtInxFromStdInx(blockRowArg, blockColArg, stdIndex);
         return _CmtInxToGblInxMap[blockRow][blockCol][cmtIndex];
      }
      /// <summary>Fill list of node positions and let main mesh fill in local-to-global index map. Then rewire the Node delegate.</summary>
      protected abstract void CreateNodes();
      /// <summary>Set values of constrained nodes and set their Constrainedness property to true. Returns number of constrained nodes.</summary>
      protected abstract int ApplyConstraints();
      /// <summary>Add whole block's contribution to global stiffness matrix.</summary><param name="A">Gloabal stiffness matrix.</param><param name="dt">Time step.</param><param name="ni">Viscosity.</param>
      public abstract void AddContribsToSfsMatrix(SparseMat A, double dt, double ni);
      public abstract void AddContribsToFcgVector(SparseRow b, double dt, double ni);
      /// <summary>Creates an 8 x 8 matrix belonging to to a single Node vector.</summary><param name="node">Node whose values of which will be used inside operator matrix.</param><param name="dt">Time step.</param><param name="ni">Viscosity coefficient.</param>
      protected double[][] NodeOperatorMatrix0(ref MeshNode node, double dt, double ni) {
         double[][] A = new double[8][] {
            new double[8] { 1.0/dt + node.Var(2).Val, node.Var(3).Val, node.Var(0).Val, node.Var(1).Val, 0, 0, 1, 0 },
            new double[8] { node.Var(4).Val, 1.0/dt - node.Var(2).Val, -node.Var(1).Val, 0, node.Var(0).Val, 0, 0, 1 },
            new double[8] { 0, 0, 1, 0, 0, 0, 0, 0 },
            new double[8] { 0, 0, 0, 1, 0, 0, 0, 0 },
            new double[8] { 0, 0, 0, 0, 1, 0, 0, 0 },
            new double[8] { 0, 0, 0, 0, 0, 0, 1, 0 },
            new double[8] { 0, 0, 0, 0, 0, 0, 0, 1 },
            new double[8] { 0, 0, 0, 0, 0, 0, 0, 0 } };
         return A;
      }
      protected double[][] NodeOperatorMatrix1(ref MeshNode node, double dt, double ni) {
         double[][] A = new double[8][] {
            new double[8] { 0, 0, -ni, 0, 0, 0, 0, 0 },
            new double[8] { 0, 0, 0, 0, -ni, 0, 0, 0 },
            new double[8] { -1, 0, 0, 0, 0, 0, 0, 0 },
            new double[8] { 0, 0, 0, 0, 0, 0, 0, 0 },
            new double[8] { 0, -1, 0, 0, 0, 0, 0, 0 },
            new double[8] { 0, 0, 0, 0, 0, -1, 0, 0 },
            new double[8] { 0, 0, 0, 0, 0, 0, 0, 0 },
            new double[8] { 0, 0, 0, 0, 0, 0, 0, -1 } };
         return A;
      }
      protected double[][] NodeOperatorMatrix2(ref MeshNode node, double dt, double ni) {
         double[][] A = new double[8][] {
            new double[8] { 0, 0, 0, -ni, 0, 0, 0, 0 },
            new double[8] { 0, 0, ni, 0, 0, 0, 0, 0 },
            new double[8] { 0, 0, 0, 0, 0, 0, 0, 0 },
            new double[8] { -1, 0, 0, 0, 0, 0, 0, 0 },
            new double[8] { 0, 0, 0, 0, 0, 0, 0, 0 },
            new double[8] { 0, 0, 0, 0, 0, 0, 0, 0 },
            new double[8] { 0, 0, 0, 0, 0, -1, 0, 0 },
            new double[8] { 0, 0, 0, 0, 0, 0, 1, 0 } };
         return A;
      }
      /// <summary>Find solution value of specified variables at specified point.</summary><param name="x">X coordinate.</param><param name="y">Y coordinate.</param><param name="vars">Indices of variables we wish to retrieve.</param>
      public virtual double[] Solution(ref Pos pos, params int[] vars) {
         int startRow = 0;                                                          // Where current frame begins.
         int endRow = RowCount - 1;                                                 // Where current frame ends.
         int startCol = 0;
         int endCol = ColCount - 1;
         int nRows = RowCount;                                                      // Row count of current frame.
         int nCols = ColCount;
         var vertices = new Pos[4];
         vertices[0] = NodeStd(startRow, startCol, 0)._Pos;
         vertices[1] = NodeStd(startRow, endCol, 3)._Pos;
         vertices[2] = NodeStd(endRow, endCol, 6)._Pos;
         vertices[3] = NodeStd(endRow, startCol, 9)._Pos;
         int newEndRow = 0;
         int newEndCol = 0;
         while(nRows > 1 || nCols > 1) {                                            // As long as we have not narrowed our frame down to a single element.
            if(nRows > 1) {
               newEndRow = startRow + nRows/2 - 1;                                  // Set row at half frame width as end. No problem if nRows is odd.
               vertices[3] = NodeStd(newEndRow, startCol, 9)._Pos;                  // New upper left.
               vertices[2] = NodeStd(newEndRow, endCol, 6)._Pos;                    // New upper right.
               if(pos.IsInsidePolygon(vertices)) {
                  endRow = newEndRow;
                  vertices[3] = NodeStd(endRow, startCol, 9)._Pos;                  // UL
                  vertices[2] = NodeStd(endRow, endCol, 6)._Pos; }                  // UR
               else {
                  startRow = newEndRow + 1;
                  vertices[0] = NodeStd(startRow, startCol, 0)._Pos;                // LL
                  vertices[1] = NodeStd(startRow, endCol, 3)._Pos; }                // LR
               nRows = endRow - startRow + 1; }
            if(nCols > 1) {
               newEndCol = startCol + nCols/2 - 1;
               vertices[1] = NodeStd(startRow, newEndCol, 3)._Pos;                  // new LR.
               vertices[2] = NodeStd(endRow, newEndCol, 6)._Pos;
               if(pos.IsInsidePolygon(vertices)) {
                  endCol = newEndCol;
                  vertices[1] = NodeStd(startRow, endCol, 3)._Pos;
                  vertices[2] = NodeStd(endRow, endCol, 6)._Pos; }
               else {
                  startCol = newEndCol + 1;
                  vertices[3] = NodeStd(endRow, startCol, 9)._Pos;
                  vertices[0] = NodeStd(startRow, startCol, 0)._Pos; }
               nCols = endCol - startCol + 1;
         }  }                                                                        // At this point startCol and endCol have to be the same.
         var quadElm = CreateQuadElement(startRow, startCol);                        // Quadrilateral that contains sought after point.
         var squarePos = quadElm.ReferenceSquareCoords(ref pos);
         double[] funcValues = quadElm.Values(in squarePos, vars);
         return funcValues;
      }
      /// <summary>Creates a data structure which holds all four corner nodes of an element.</summary><param name="stdRow">Element's row inside mesh block.</param><param name="stdCol">Element's col inside mesh block.</param>
      MeshElement CreateQuadElement(int stdRow, int stdCol) {
         var quadElm = new MeshElement(
            ref NodeStd(stdRow, stdCol, 0),
            ref NodeStd(stdRow, stdCol, 1),
            ref NodeStd(stdRow, stdCol, 2),
            ref NodeStd(stdRow, stdCol, 3),
            ref NodeStd(stdRow, stdCol, 4),
            ref NodeStd(stdRow, stdCol, 5),
            ref NodeStd(stdRow, stdCol, 6),
            ref NodeStd(stdRow, stdCol, 7),
            ref NodeStd(stdRow, stdCol, 8),
            ref NodeStd(stdRow, stdCol, 9),
            ref NodeStd(stdRow, stdCol, 10),
            ref NodeStd(stdRow, stdCol, 11)
         );
         return quadElm;
      }
      /// <summary>Determines whether specified position is inside MeshBlock.</summary><param name="pos">Point's position.</param>
      public abstract bool IsPointInside(ref Pos pos);
   }
}