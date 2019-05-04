using System;
using static System.Math;

using Fluid.Internals.Collections;
using Fluid.Internals.Numerics;
using static Fluid.Internals.Numerics.MatOps;

namespace Fluid.Internals.Meshing {
   using Tensor2 = Tensor2<double,DblArithmetic>;
   using SparseRow = SparseRow<double,DblArithmetic>;
   /// <summary>Represents a method that takes three indices and returns a position by reference.</summary>
   //public delegate ref MeshNode NodeDelegate(int blockRow, int blockCol, int index);

   // TODO: Implement abstract J(stdInx,p,q) and J(cmtInx,p,q) methods that return Jacobians for each element.

   // cmt = compact, std = standard, lcl = local, gbl = global; descriptors connected to position indices.
   /// <summary>A structured submesh that provides access to global node indices via element indices.</summary>
   public abstract class MeshBlock {   
      /// <summary>Owner mesh. Access to global nodes list.</summary>
      public BlockStructuredMesh MainMesh { get; protected set; }
      /// <summary>Number of rows of elements.</summary>
      public int NRows { get; protected set; }
      /// <summary>Number of columns of elements.</summary>
      public int NCols { get; protected set; }
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
      /// <summary>Get MeshNode with specified compact index.</summary><param name="blockRow">Row of element (in which sought after position is located) inside block.</param><param name="blockCol">Column of element (in which sought after position is located) inside block.</param><param name="standardPosIndex">Standard element position index (0 - 11).</param><remarks>We use a delegate because first, we create all positions and add them to local positions array (delegate points to this array), then we move positions to a 1D global map on main mesh (delegate is then rewired to point there.)</remarks>
      public Func<int,int,int,MeshNode> NodeCmt;
      /// <summary>Get MeshNode with specified standard index.</summary><param name="blockRow">Row of element (in which sought after position is located) inside block.</param><param name="blockCol">Column of element (in which sought after position is located) inside block.</param><param name="standardPosIndex">Standard element position index (0 - 11).</param><remarks>We use a delegate because first, we create all positions and add them to local positions array (delegate points to this array), then we move positions to a 1D global map on main mesh (delegate is then rewired to point there.)</remarks>
      public Func<int,int,int,MeshNode> NodeStd;

      /// <summary>Intended for testing.</summary>
      public MeshBlock() { }
      /// <summary>Create a SubMesh and assign reference to main mesh.</summary><param name="mainMesh">Owner of this submesh who will use it fill _indexMap, mapping positions to _globalNodes list.</param>
      public MeshBlock(BlockStructuredMesh mainMesh) {
         MainMesh = mainMesh;
         NodeCmt = NodeOnBlockCmt;
         NodeStd = NodeOnBlockStd;
      }

      /// <summary>Get reference to Position (residing on MeshBlock) with specified compact index.</summary><param name="rowCmtInx">Row of element (in which sought after position is located) inside block.</param><param name="colCmtInx">Column of element (in which sought after position is located) inside block.</param><param name="inrCmtInx">Compact element (inner) node position index (0 - 4).</param>
      MeshNode NodeOnBlockCmt(int rowCmtInx, int colCmtInx, int inrCmtInx) =>
         Nodes[rowCmtInx][colCmtInx][inrCmtInx];
      /// <summary>Get reference to Position (residing on MeshBlock) with specified standard index.</summary><param name="rowStdInx">Row of element (in which sought after position is located) inside block.</param><param name="colStdInx">Column of element (in which sought after position is located) inside block.</param><param name="inrStdInx">Standard element position index (0 - 11).</param>
      MeshNode NodeOnBlockStd(int rowStdInx, int colStdInx, int inrStdInx) {
         (int rowCmtInx, int colCmtInx, int inrCmrInx) =
            CmtInxFromStdInx(rowStdInx, colStdInx, inrStdInx);
         return Nodes[rowCmtInx][colCmtInx][inrCmrInx];
      }
      /// <summary>We switch the NodeCmp delegate to point here after all positions are created and nodes are transfered to main mesh.</summary><param name="rowCmtInx">Row of element (in which sought after position is located) inside block.</param><param name="colCmtInx">Column of element (in which sought after position is located) inside block.</param><param name="inrCmtInx">Compact element node position index (0 - 4).</param>
      protected MeshNode NodeOnMainCmt(int rowCmtInx, int colCmtInx, int inrCmtInx) {
         int gblInx = GblInxFromCmpInx(rowCmtInx, colCmtInx, inrCmtInx);
         return MainMesh.G[gblInx];
      }
      /// <summary>We switch the GetNodeStd delegate to point here after all positions are created and nodes are transfered to main mesh.</summary><param name="rowStdInx">Row of element (in which sought after position is located) inside block.</param><param name="colStdInx">Column of element (in which sought after position is located) inside block.</param><param name="inrStdInx">Standard element position index (0 - 11).</param>
      protected MeshNode NodeOnMainStd(int rowStdInx, int colStdInx, int inrStdInx) {
         (int rowCmtInx,int colCmtInx,int inrCmtInx) =
            CmtInxFromStdInx(rowStdInx, colStdInx, inrStdInx);
         int gblInx = GblInxFromCmpInx(rowCmtInx, colCmtInx, inrCmtInx);
         return MainMesh.G[gblInx];
      }
      /// <summary>Returns three actual indices (0 - 4) of node positions array when we specify them in conventional notation (0 -11).</summary><param name="rowStdInx">Row of element inside this block.</param><param name="colStdInx">Column of element inside this block.</param><param name="inrStdInx">Index of node inside element (1 - 11).</param>
      public (int rowCmtInx,int colCmtInx,int inrCmtInx) CmtInxFromStdInx(
         int rowStdInx, int colStdInx, int inrStdInx) {
            if(rowStdInx < NRows) {                                                     // Dummy elements at ends.
               if(colStdInx < NCols) {
                  if(inrStdInx < 3)
                     return (rowStdInx, colStdInx, inrStdInx + 2);
                  else if(inrStdInx < 6)
                     return (rowStdInx, colStdInx + 1, 5 - inrStdInx);
                  else if(inrStdInx == 6)
                     return (rowStdInx + 1, colStdInx + 1, 2);
                  else if(inrStdInx < 10)
                     return (rowStdInx + 1, colStdInx, 11 - inrStdInx);
                  else if(inrStdInx < 12)
                     return (rowStdInx, colStdInx, inrStdInx - 10);
                  else
                     throw new IndexOutOfRangeException("Standard index above 11."); }
               else                                                                          // Cannot access dummy.
                  throw new IndexOutOfRangeException(
                     "Block column index of element to large."); }
            else
               throw new IndexOutOfRangeException("Block row index of element to large.");
      }
      /// <summary>Global position index in terms of compact position index.</summary><param name="rowCmtInx">Element's row (in which sought after position is located) inside block.</param><param name="blockColumn">Element's column (in which sought after position is located) inside block.</param><param name="localNodeIndex">Compact index of position inside element (0 - 4).</param>
      protected int GblInxFromCmpInx(int rowCmtInx, int colCmtInx, int inrCmtInx) =>
         _CmtInxToGblInxMap[rowCmtInx][colCmtInx][inrCmtInx];
      /// <summary>Global position index in terms of standard position index.</summary><param name="rowStdInx">Element's row (in which sought after position is located) inside block.</param><param name="blockColumn">Element's column (in which sought after position is located) inside block.</param><param name="localNodeIndex">Index of position inside element (0 - 11).</param>
      protected int GblInxFromStdInx(int rowStdInx, int colStdInx, int inrStdInx) {
         (int rowCmtInx,int colCmtInx,int inrCmtInx) =
            CmtInxFromStdInx(rowStdInx, colStdInx, inrStdInx);
         return _CmtInxToGblInxMap[rowCmtInx][colCmtInx][inrCmtInx];
      }
      /// <summary>Fill list of node positions and let main mesh fill in local-to-global index map. Then rewire the Node delegate.</summary>
      protected abstract void CreateNodes();
      /// <summary>Set values of constrained nodes and set their Constrainedness property to true. Returns number of constrained nodes.</summary>
      protected abstract int ApplyConstraints();
      /// <summary>Add whole block's contribution to global stiffness matrix.</summary><param name="A">Gloabal stiffness matrix.</param><param name="dt">Time step.</param><param name="ni">Viscosity.</param>
      public abstract void AddContribsToSfsMatrix(Tensor2 A, double dt, double ni);
      public abstract void AddContribsToFcgVector(SparseRow b, double dt, double ni);
      /// <summary>Creates an 8 x 8 matrix belonging to to a single Node vector.</summary><param name="node">Node whose values of which will be used inside operator matrix.</param><param name="dt">Time step.</param><param name="ni">Viscosity coefficient.</param>
      protected double[][] NodeOperatorMat0(MeshNode node, double dt, double ni) {
         double[][] A = new double[8][] {
            new double[8] { 1.0/dt + node.Vars[2].Val, node.Vars[3].Val, node.Vars[0].Val, node.Vars[1].Val, 0, 0, 1, 0 },
            new double[8] { node.Vars[4].Val, 1.0/dt - node.Vars[2].Val, -node.Vars[1].Val, 0, node.Vars[0].Val, 0, 0, 1 },
            new double[8] { 0, 0, 1, 0, 0, 0, 0, 0 },
            new double[8] { 0, 0, 0, 1, 0, 0, 0, 0 },
            new double[8] { 0, 0, 0, 0, 1, 0, 0, 0 },
            new double[8] { 0, 0, 0, 0, 0, 0, 1, 0 },
            new double[8] { 0, 0, 0, 0, 0, 0, 0, 1 },
            new double[8] { 0, 0, 0, 0, 0, 0, 0, 0 } };
         return A;
      }
      protected double[][] NodeOperatorMat1(MeshNode node, double dt, double ni) {
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
      protected double[][] NodeOperatorMat2(MeshNode node, double dt, double ni) {
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
      public virtual double[] Solution(in Pos pos, params int[] vars) {
         int startRow = 0;                                                          // Where current frame begins.
         int endRow = NRows - 1;                                                 // Where current frame ends.
         int startCol = 0;
         int endCol = NCols - 1;
         int nRows = NRows;                                                      // Row count of current frame.
         int nCols = NCols;
         var verts = new Pos[4];                                  // Vertices.
         verts[0] = NodeStd(startRow, startCol, 0).Pos;
         verts[1] = NodeStd(startRow, endCol, 3).Pos;
         verts[2] = NodeStd(endRow, endCol, 6).Pos;
         verts[3] = NodeStd(endRow, startCol, 9).Pos;
         int newEndRow = 0;
         int newEndCol = 0;
         while(nRows > 1 || nCols > 1) {                                            // As long as we have not narrowed our frame down to a single element.
            if(nRows > 1) {
               newEndRow = startRow + nRows/2 - 1;                                  // Set row at half frame width as end. No problem if nRows is odd.
               verts[3] = NodeStd(newEndRow, startCol, 9).Pos;                  // New upper left.
               verts[2] = NodeStd(newEndRow, endCol, 6).Pos;                    // New upper right.
               if(pos.IsInsidePolygon(verts)) {
                  endRow = newEndRow;
                  verts[3] = NodeStd(endRow, startCol, 9).Pos;                  // UL
                  verts[2] = NodeStd(endRow, endCol, 6).Pos; }                  // UR
               else {
                  startRow = newEndRow + 1;
                  verts[0] = NodeStd(startRow, startCol, 0).Pos;                // LL
                  verts[1] = NodeStd(startRow, endCol, 3).Pos; }                // LR
               nRows = endRow - startRow + 1; }
            if(nCols > 1) {
               newEndCol = startCol + nCols/2 - 1;
               verts[1] = NodeStd(startRow, newEndCol, 3).Pos;                  // new LR.
               verts[2] = NodeStd(endRow, newEndCol, 6).Pos;
               if(pos.IsInsidePolygon(verts)) {
                  endCol = newEndCol;
                  verts[1] = NodeStd(startRow, endCol, 3).Pos;
                  verts[2] = NodeStd(endRow, endCol, 6).Pos; }
               else {
                  startCol = newEndCol + 1;
                  verts[3] = NodeStd(endRow, startCol, 9).Pos;
                  verts[0] = NodeStd(startRow, startCol, 0).Pos; }
               nCols = endCol - startCol + 1; }  }                               // At this point startCol and endCol have to be the same.
         var quadEmt = CreateQuadEmt(startRow, startCol);                        // Quadrilateral that contains sought after point.
         var squarePos = quadEmt.RefSquareCoords(in pos);
         double[] funcValues = quadEmt.Vals(in squarePos, vars);
         return funcValues;
      }
      /// <summary>Creates a data structure which holds all four corner nodes of an element.</summary><param name="stdRow">Element's row inside mesh block.</param><param name="stdCol">Element's col inside mesh block.</param>
      MeshElement CreateQuadEmt(int stdRow, int stdCol) {
         var quadElm = new MeshElement(NodeStd(stdRow, stdCol, 0), NodeStd(stdRow, stdCol, 1),
            NodeStd(stdRow, stdCol, 2), NodeStd(stdRow, stdCol, 3), NodeStd(stdRow, stdCol, 4),
            NodeStd(stdRow, stdCol, 5), NodeStd(stdRow, stdCol, 6), NodeStd(stdRow, stdCol, 7),
            NodeStd(stdRow, stdCol, 8), NodeStd(stdRow, stdCol, 9), NodeStd(stdRow, stdCol, 10),
            NodeStd(stdRow, stdCol, 11) );
         return quadElm;
      }
      /// <summary>Determines whether specified position is inside MeshBlock.</summary><param name="pos">Point's position.</param>
      public abstract bool IsPointInside(in Pos pos);
   }
}