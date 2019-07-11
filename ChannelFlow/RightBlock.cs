using System;
using static System.Math;

using Fluid.Internals.Collections;
using Fluid.Internals.Numerics;
using Fluid.Internals.Mesh;
using TB = Fluid.Internals.Toolbox;
using static Fluid.Internals.Ops;
using static Fluid.Internals.Numerics.MatOps;

namespace Fluid.ChannelFlow {
   using dbl = Double;
   using DA = DblArithmetic;
   using Tensor = Tensor<double,DblArithmetic>;
   using Vector = Vector<double,DblArithmetic>;
   public sealed class RightBlock : RectBlock {
      ChannelCylinderSystem ChannelFlow { get; }
      ChannelMesh ChannelMesh { get; }
      /// <summary>Basis function overlap integrals over a single element, since all elements are the same.</summary><remarks>[j][k][n][m] = [12 basis funcs][12-j basis funcs][5 terms][5 terms]</remarks>
      static dbl[][][][] _RectStiffnessIntegrals;
      /// <summary>Basis function overlap integrals over a single element, since all elements are the same.</summary><remarks>[j][k][n][m] = [12 basis funcs][12-j basis funcs][5 terms][5 terms]</remarks>
      static dbl[][][][] RectStiffnessIntegrals => _RectStiffnessIntegrals;
      /// <summary>Basis function overlap integrals over a single element, since all elements are the same.</summary><remarks>[j][n] = [12 basis funcs][5 terms]</remarks>
      static dbl[][] _RectForcingIntegrals;
      /// <summary>Basis function overlap integrals over a single element, since all elements are the same.</summary><remarks>[j][n] = [12 basis funcs][5 terms]</remarks>
      static dbl[][] RectForcingIntegrals => _RectForcingIntegrals;

      static RightBlock() {
         TB.FileReader.SetDirAndFile(@"ChannelFlow/Input", "rectElementStiffnessIntegrals", ".txt");
         _RectStiffnessIntegrals = (dbl[][][][]) TB.FileReader.ReadArray<dbl>();
         TB.FileReader.SetFile("rectElementForcingIntegrals", ".txt");
         _RectForcingIntegrals = (dbl[][]) TB.FileReader.ReadArray<dbl>();
         //_RectStiffnessIntegrals = ReadRectStiffnessIntegrals("./Input/rectElementStiffnessIntegrals.txt");
         //_RectForcingIntegrals = ReadRectForcingIntegrals("./Input/rectElementForcingIntegrals.txt");
      }
      /// <summary>Create a Cartesian mesh block.</summary><param name="channelMesh">Mesh block's owner.</param><param name="lowerLeftX">Lower left corner x coordinate.</param><param name="lowerLeftY">Lower left corner y coordinate.</param><param name="upperRightX">Upper right corner x coordinate.</param><param name="upperRightY">Upper right corner y coordinate.</param><param name="rowCount">Number of elements in y direction.</param><param name="columnCount">Number of elements in x direction.</param>
      public RightBlock(ChannelMesh channelMesh, ChannelCylinderSystem channelFlow, EastBlock eastBlock,
         dbl lowerLeftX, dbl lowerLeftY, dbl upperRightX, dbl upperRightY,
         int rowCount, int columnCount)
         : base(channelMesh, lowerLeftX, lowerLeftY, upperRightX, upperRightY,
         rowCount, columnCount) {
            ChannelMesh = channelMesh;
            ChannelFlow = channelFlow;
            CreateNodes();
            NConstraints = ApplyConstraints();
            ChannelMesh.NConstraints += NConstraints;
            MoveNodesToMainMesh(eastBlock);
      }

      protected override int ApplyConstraints() {
         int constraintCount = 0;
         int col = 0;
         while(col < NCols) {                                            // Col 0 - 59
            for(int p = 2; p < 5; ++p) {                  // Upper Channel boundary.
               NodeCmt(20, col, p).Vars[0].Constrained = true;     // u
               NodeCmt(20, col, p).Vars[1].Constrained = true;     // v
               NodeCmt(20, col, p).Vars[2].Constrained = true;     // a
               NodeCmt(20, col, p).Vars[4].Constrained = true;     // c
               constraintCount += 4; }
            for(int p = 2; p < 5; ++p) {                  // Lower Channel boundary.
               NodeCmt(0, col, p).Vars[0].Constrained = true;      // u
               NodeCmt(0, col, p).Vars[1].Constrained = true;      // v
               NodeCmt(0, col, p).Vars[2].Constrained = true;      // a
               NodeCmt(0, col, p).Vars[4].Constrained = true;      // c
               constraintCount += 4; }
            ++col; }                                                                   // Col 60
         NodeCmt(20, col, 2).Vars[0].Constrained = true;                   // Channel boundary, u.
         NodeCmt(20, col, 2).Vars[1].Constrained = true;                   // v
         NodeCmt(20, col, 2).Vars[2].Constrained = true;                   // a
         NodeCmt(20, col, 2).Vars[4].Constrained = true;                   // c
         NodeCmt(0, col, 2).Vars[0].Constrained = true;                    // Channel boundary, u.
         NodeCmt(0, col, 2).Vars[1].Constrained = true;                    // v
         NodeCmt(0, col, 2).Vars[2].Constrained = true;                    // a
         NodeCmt(0, col, 2).Vars[4].Constrained = true;                    // c
         constraintCount += 8;
         return constraintCount;
      }
      void MoveNodesToMainMesh(EastBlock eastBlock) {
         int nPos = ChannelMesh.NPos;
         var blockToGlobal = new int[NRows + 1][][];
         int row = 0;
         int col = 0;
         var eastMap = eastBlock.CmtInxToGblInxMap;           // We will need EastBlock's map.
         while(row < 20) {                                                   // Row 0 - 19
            blockToGlobal[row] = new int[NCols + 1][];
            col = 0;                           
            blockToGlobal[row][col] = new int[5];
            blockToGlobal[row][col][0] = eastMap[23][19 - row][3];          // Col 0
            blockToGlobal[row][col][1] = eastMap[23][19 - row][4];
            blockToGlobal[row][col][2] = eastMap[23][20 - row][2];
            for(int p = 3; p < 5; ++p) {
               ChannelMesh.G[nPos] = NodeCmt(row, col, p);
               blockToGlobal[row][col][p] = nPos++; }
            col = 1;
            while(col < NCols) {                                             // Col 1 - 59
               blockToGlobal[row][col] = new int[5];
               for(int p = 0; p < 5; ++p) {
                  ChannelMesh.G[nPos] = NodeCmt(row, col, p);
                  blockToGlobal[row][col][p] = nPos++; }
               ++col; }
            blockToGlobal[row][col] = new int[5];                               // Col 60, last col.
            for(int p = 0; p < 3; ++p) {
               ChannelMesh.G[nPos] = NodeCmt(row, col, p);
               blockToGlobal[row][col][p] = nPos++; }
            for(int p = 3; p < 5; ++p)
               blockToGlobal[row][col][p] = Int32.MinValue;
            ++row; }                                      
         col = 0;                                                            // Row 20
         blockToGlobal[row] = new int[NCols+1][];
         blockToGlobal[row][col] = new int[5];                                   // Col 0
         for(int p = 0; p < 2; ++p)
               blockToGlobal[row][col][p] = Int32.MinValue;
         blockToGlobal[row][col][2] = eastMap[23][0][2];
         for(int p = 3; p < 5; ++p) {
               ChannelMesh.G[nPos] = NodeCmt(row, col, p);
               blockToGlobal[row][col][p] = nPos++; }
         col = 1;
         while(col < NCols) {                                                 // Col 1 - 59
            blockToGlobal[row][col] = new int[5]; 
            for(int p = 0; p < 2; ++p)
               blockToGlobal[row][col][p] = Int32.MinValue;
            for(int p = 2; p < 5; ++p) {
               ChannelMesh.G[nPos] = NodeCmt(row, col, p);
               blockToGlobal[row][col][p] = nPos++; }
            ++col; }
         blockToGlobal[row][col] = new int[5];                                   // Col 60
         for(int p = 0; p < 2; ++p)
               blockToGlobal[row][col][p] = Int32.MinValue;
         ChannelMesh.G[nPos] = NodeCmt(row, col, 2);
         blockToGlobal[row][col][2] = nPos++;                             // Last position to be added.
         for(int p = 3; p < 5; ++p)
               blockToGlobal[row][col][p] = Int32.MinValue;
         _CmtInxToGblInxMap = blockToGlobal;
         ChannelMesh.NPos = nPos;                          // In our case, this now has to be 15 620.
         _Nodes = null;                                                  // Free memory on block.
         NodeCmt = NodeOnMainCmt;                                  // Rewire.
         NodeStd = NodeOnMainStd;
      }
      /// <summary>Get an overlap integral of basis functions j and k. Get term n, m.</summary><param name="j">First overlapping basis function.</param><param name="k">Second overlapping basis function.</param><param name="n">Factor row.</param><param name="m">Factor column.</param>
      dbl GetSfsIntegral(int j, int k, int n, int m) {
         if(k < j)
               Swap(ref j, ref k);                                                     // Account fot the fact that k is always such that [j][k] forms an upper left triangular matrix.
         k -= j; 
         return _RectStiffnessIntegrals[j][k][n][m];
         // TODO: Correct behavior of this method. j and k have special properties.
      }
      /// <summary>Get an overlap integral of basis functions j and k. Get term n, m.</summary><param name="j">First overlapping basis function.</param><param name="k">Second overlapping basis function.</param><param name="n">Factor row.</param><param name="m">Factor column.</param>
      dbl GetForcingIntegral(int j, int n) {
         return _RectForcingIntegrals[j][n];
      }
      public override void AddContribsToSfsMatrix(Tensor A, dbl dt, dbl ni) {
         for(int row = 0; row < 20; ++row)
            for(int col = 0; col < 60; ++col)
               AddEmtContribToSfsMatrix(A, row, col, dt, ni);
      }
      /// <summary>Add contribution from element at specified row and col to global stiffness matrix.</summary><param name="A">Global stiffness matrix.</param><param name="row">Mesh block row where element is situated.</param><param name="col">Mesh block col where element is situated.</param><param name="dt">Time step.</param><param name="ni">Viscosity.</param>
      void AddEmtContribToSfsMatrix(Tensor A, int row, int col, dbl dt, dbl ni) {
         dbl[][] subResult;
         int gblRowBelt;                                          // Starting index of an octuple of rows which represent variable values at a single position.
         int gblColBelt;
         for(int j = 0; j < 12; ++j)                               // Over first basis function.
            for(int k = j; k < 12; ++k) {                           // Over second basis function.
               subResult = SubMatrix(row, col, j, k, dt, ni);      // 8 x 8 matrix which has to be added to global stiffness matrix.
               gblRowBelt = GblInxFromStdInx(row, col, j);
               gblColBelt = GblInxFromStdInx(row, col, k);
               for(int subResRow = 0; subResRow < 8; ++subResRow)
                  for(int subResCol = 0; subResCol < 8; ++subResCol) {                                                       // Using symmetry of global stiffness matrix.
                     A[gblRowBelt * 8 + subResRow, gblColBelt * 8 + subResCol] +=
                        subResult[subResRow][subResCol];
                     A[gblColBelt * 8 + subResCol, gblRowBelt * 8 + subResRow] +=
                        subResult[subResRow][subResCol]; } }
      }
      dbl[][] SubMatrix(int row, int col, int j, int k, dbl dt, dbl ni) {
         dbl[][] subMat = new dbl[8][];
         dbl[][][][] A = new dbl[2][][][];                                 // For two nodes j and k. Next dimension contains 3 elements: A0, A1, A2.
         for(int i = 0; i < 8; ++i)
            subMat[i] = new dbl[8];
         var node1 = NodeStd(row, col, j);
         var node2 = NodeStd(row, col, k);
         A[0] = new dbl[3][][];                                               // Create operators for node1. For 3 different matrices A0, A1, A2.
         A[0][0] = NodeOperatorMat0(node1, dt);
         A[0][0].Transpose();
         A[0][1] = NodeOperatorMat1(ni);
         A[0][1].Transpose();
         A[0][2] = NodeOperatorMat2(ni);
         A[0][2].Transpose();
         A[1] = new double[3][][];                                                   // Create operators for node1.
         A[1][0] = NodeOperatorMat0(node2, dt);
         A[1][1] = NodeOperatorMat1(ni);
         A[1][2] = NodeOperatorMat2(ni);
         for(int n = 0; n < 5; ++n)
            for(int m = 0; m < 5; ++m)
               subMat.Sum<dbl,DA>(GetSfsIntegral(j, k, n, m).Mul<dbl,DA>(A[0][NewN(n)].Dot<dbl,DA>(A[1][NewN(m)])));
         return subMat;

         int NewN(int n) => n < 3 ? n : n - 2;                      // First 3 terms contain: A0, A1, A2; last two terms contain A1 and A2.
      }
      /// <summary>Add whole block's contribution to global forcing vector.</summary><param name="b">Gloabal forcing vector.</param><param name="dt">Time step.</param><param name="ni">Viscosity.</param>
      public override void AddContribsToFcgVector(Vector b, dbl dt, dbl ni) {
         for(int row = 0; row < 20; ++row)
            for(int col = 0; col < 60; ++col)
               AddEmtContribToFcgVector(b, row, col, dt, ni);
      }
      /// <summary>Add contribution from element at specified row and col to global forcing vector.</summary><param name="b">Global forcing vector.</param><param name="row">Mesh block row where element is situated.</param><param name="col">Mesh block col where element is situated.</param><param name="dt">Time step.</param><param name="ni">Viscosity.</param>
      void AddEmtContribToFcgVector(Vector b, int row, int col, dbl dt, dbl ni) {
         double[] subVec;
         int globalRowBelt;                                          // Starting index of an octuple of rows which represent variable values at a single position.
         for(int j = 0; j < 12; ++j) {                               // Over basis functions.
               subVec = SubVec(row, col, j, dt, ni);      // 8 x 8 matrix which has to be added to global stiffness matrix.
               globalRowBelt = GblInxFromStdInx(row, col, j);
               for(int subResultRow = 0; subResultRow < 8; ++subResultRow)
                  b[globalRowBelt * 8 + subResultRow] += subVec[subResultRow]; }
      }
      /// <summary>Creates an 8 element subvector of a 96 element forcing vector  for some choice of j = 0,...,11.</summary><param name="row">Mesh block row of element.</param><param name="col">Mesh block column row of element.</param><param name="j">First overlapping basis function.</param><param name="dt">Time step.</param><param name="ni">Viscosity.</param><param name=x>Previous values of variables at point (row,col,j).</param>
      double[] SubVec(int row, int col, int j, double dt, double ni) {
         var subVec = new dbl[8];
         var node = NodeStd(row, col, j);
         var A = new dbl[3][][];                                                      // For three different operators A.            
         A[0] = NodeOperatorMat0(node, dt);                // Create 3 different matrices A0, A1, A2.
         A[0].Transpose();
         A[1] = NodeOperatorMat1(ni);
         A[1].Transpose();
         A[2] = NodeOperatorMat2(ni);
         A[2].Transpose();
         var aTf = new dbl[8];                                                        // Elemental forcing vector.
         double[][] fCoefs = new dbl[8][];                                           // Coefficients accompanying terms in f vector.
         fCoefs[0] = new dbl[4] {-node.Vars[6].Val, ni*node.Vars[2].Val, ni*node.Vars[3].Val,
            -node.Vars[0].Val * node.Vars[2].Val - node.Vars[1].Val * node.Vars[3].Val};
         fCoefs[1] = new dbl[4] {-node.Vars[7].Val, ni*node.Vars[4].Val, -ni*node.Vars[2].Val,
            -node.Vars[0].Val * node.Vars[4].Val + node.Vars[1].Val * node.Vars[2].Val};
         fCoefs[2] = new dbl[2] {-node.Vars[2].Val, node.Vars[0].Val};
         fCoefs[3] = new dbl[2] {-node.Vars[3].Val, node.Vars[0].Val};
         fCoefs[4] = new dbl[2] {-node.Vars[4].Val, node.Vars[1].Val};
         fCoefs[5] = new dbl[2] {-node.Vars[6].Val, node.Vars[5].Val};
         fCoefs[6] = new dbl[2] {-node.Vars[7].Val, node.Vars[5].Val};
         fCoefs[7] = new dbl[2] {node.Vars[7].Val, -node.Vars[6].Val};
         for(int vecRow = 0; vecRow < 8; ++vecRow)                                     // For each entry in elemental vector.
            for(int n = 0; n < 5; ++n) {                                                // For each left term-
               for(int matCol = 0; matCol < 2; ++matCol)
                  aTf[vecRow] += A[NewN(n)][vecRow][matCol]*(fCoefs[matCol][0] *     // A[pick matrix][pick row][pick col]
                     GetSfsIntegral(j,j,n,0) + fCoefs[0][1]*(GetSfsIntegral(j,j,n,1) +
                     GetSfsIntegral(j,j,n,2)) + fCoefs[0][2]*(GetSfsIntegral(j,j,n,3) +
                     GetSfsIntegral(j,j,n,4)) + fCoefs[0][3]*GetForcingIntegral(j,n));
               aTf[vecRow] += A[NewN(n)][vecRow][2] * (fCoefs[2][0] * GetSfsIntegral(j,j,n,0) + 
                  fCoefs[2][1] * (GetSfsIntegral(j,j,n,1) + GetSfsIntegral(j,j,n,3)));
               aTf[vecRow] += A[NewN(n)][vecRow][3] * (fCoefs[3][0] * GetSfsIntegral(j,j,n,0) + 
                  fCoefs[3][1] * (GetSfsIntegral(j,j,n,2) + GetSfsIntegral(j,j,n,4)));
               for(int matCol = 4; matCol < 6; ++matCol)
                  aTf[vecRow] += A[NewN(n)][vecRow][matCol]*(fCoefs[matCol][0]
                     *GetSfsIntegral(j,j,n,0) + fCoefs[matCol][1]*(GetSfsIntegral(j,j,n,1)
                     + GetSfsIntegral(j,j,n,3)));
               aTf[vecRow] += A[NewN(n)][vecRow][6]*(fCoefs[6][0]*GetSfsIntegral(j,j,n,0) + 
                  fCoefs[6][1]*(GetSfsIntegral(j,j,n,2) + GetSfsIntegral(j,j,n,4)));
               aTf[vecRow] += A[NewN(n)][vecRow][7]*(fCoefs[7][0]*(GetSfsIntegral(j,j,n,1) +
                  GetSfsIntegral(j,j,n,3)) + fCoefs[7][1]*(GetSfsIntegral(j,j,n,2) +
                  GetSfsIntegral(j,j,n,4))); }
         return aTf;

         int NewN(int n) => n < 3 ? n : n - 2;                      // First 3 terms contain: A0, A1, A2; last two terms contain A1 and A2.
      }
   }
}