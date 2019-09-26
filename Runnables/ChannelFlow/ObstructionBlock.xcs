using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using static System.Math;
using static System.Char;

using Fluid.Internals.Collections;
using Fluid.Internals.Numerics;
using Fluid.Internals.Lsfem;
using static Fluid.ChannelFlow.Program;
using static Fluid.Internals.Ops;
using static Fluid.Internals.Numerics.MatOps;
using TB = Fluid.Internals.Toolbox;

namespace Fluid.ChannelFlow {
   using dbl = Double;
   using DA = DblArithmetic;
   using Vector = Vector<double,DblArithmetic>;
   using Tensor = Tensor<double,DblArithmetic>;
   /// <summary>TFI block representing a quarter of square mesh surrounding obstruction.</summary>
   public abstract class ObstructionBlock : TfiBlock {
      public ChannelCylinderSystem ChannelFlow { get; protected set; }
      /// <summary>Main mesh.</summary>
      public ChannelMesh Mesh { get; protected set; }
      /// <summary>Height measured from one of lower corners that lie on upeer part of obstruction to top of obstruction.</summary>
      protected dbl QuarterMoonHeight { get; set; }
      /// <summary>Side length (which is tilted at 45 deg) projected on vertical.</summary>
      protected dbl DiagonalProjection { get; set; }
      /// <summary>Linear coefficient appearing inside iterative map that determines vertical position of next abscisa.</summary>
      protected dbl K { get; set; }
      /// <summary>Constant appearing inside iterative map that determines vertical position of next abscisa.</summary>
      protected dbl C { get; set; }
      /// <summary>Overlap integrals that we computed in Mathematica in compact form.</summary><remarks>1st two indices are element's mesh row and col indices. 3rd and 4th specify a particular combination of basis functions. 5th and 6th specify a particular combination of coefficients inside a single term of sum.</remarks>
      protected static dbl[][][][][][] _StiffnessIntegrals;       // [23][10][12][triangular][5][5]
      /// <summary>Overlap integrals that we computed in Mathematica in compact form.</summary><remarks>1st two indices are element's mesh row and col indices. 3rd and 4th specify a particular combination of basis functions. 5th and 6th specify a particular combination of coefficients inside a single term of sum.</remarks>
      public static dbl[][][][][][] StiffnessIntegrals => _StiffnessIntegrals;
      /// <summary>An extra term needed for computation forcing vector.</summary>
      protected static dbl[][][][] _ForcingIntegrals;
      /// <summary>An extra term needed for computation forcing vector.</summary>
      public static dbl[][][][] ForcingIntegrals => _ForcingIntegrals;

      static ObstructionBlock() {
         TB.FileReader.SetDirAndFile("ChannelFlow/Input", "obstructionStiffnessIntegrals", ".txt");
         _StiffnessIntegrals = (dbl[][][][][][]) TB.FileReader.ReadArray<dbl>();
         TB.FileReader.SetFile("obstructionForcingIntegrals", ".txt");
         _ForcingIntegrals = (dbl[][][][]) TB.FileReader.ReadArray<dbl>();
      }
      public ObstructionBlock() : base() { }
      public ObstructionBlock(ChannelMesh channelMesh, ChannelCylinderSystem channelFlow,
         dbl lowerLeftX, dbl lowerLeftY,
         dbl lowerRightX, dbl lowerRightY,
         dbl upperRightX, dbl upperRightY,
         dbl upperLeftX, dbl upperLeftY) : base(channelMesh,
         lowerLeftX, lowerLeftY,
         lowerRightX, lowerRightY,
         upperRightX, upperRightY,
         upperLeftX, upperLeftY) {
            double rat = channelMesh.RelObstructionDiameter;
            double b = channelMesh.ElementDensity;
            Mesh = channelMesh;
            ChannelFlow = channelFlow;
            QuarterMoonHeight = (1 - 0.5*Sqrt(2))* channelMesh.ObstructionRadius;
            DiagonalProjection = 0.5*(channelMesh.Width - Sqrt(2)*channelMesh.ObstructionRadius);
            C = (2.0 / (Sqrt(2.0) - rat)) * ( 0.25 * PI * rat) / b;
            K = (2.0 / (Sqrt(2.0) - rat)) * (1 / b - PI * rat / (4.0 * b));
            _Nodes = new MeshNode[24][][];
      }
      
      /// <summary>Fill node positions list.</summary>
      protected override void CreateNodes() {
         dbl ksi;
         dbl nextKsi;                                                            // 20
         dbl eta = 0.0;
         dbl c = 0.9821550974;                                                   // Coefficient makes sure that last eta falls on square border. Hardcoded for element density = 20.
         dbl NextEta(dbl previousEta) => (C + previousEta) / (1.0 - c*K);
         dbl nextEta = NextEta(eta);
         MeshNode twoThirdsAbove, thirdAbove, corner, thirdRight, twoThirdsRight;
         var nodes = new List<MeshNode[][]>(24);                                    // 23 rows
         MeshNode[][] constantEtaArray;
         //int nVars = MainMesh.NVars;
         int col;
         while(eta <= 1.0) {                                                        // Move vertically upwards.
            constantEtaArray = new MeshNode[21][];                                  // We need 21 (20 elements cols) nodes at constant eta. We will be writing positions here in next loop.
            nodes.Add(constantEtaArray);
            ksi = 0.0;
            nextKsi = 1.0 / Mesh.ElementDensity;
            col = 0;                                                                // Reset col counter.
            while(ksi <= 1.0) {                                                     // Move horizontally right.
               twoThirdsAbove = CreateNode(ksi, eta + 2.0 * (nextEta - eta) / 3.0);
               thirdAbove = CreateNode(ksi, eta + (nextEta - eta) / 3.0);
               corner = CreateNode(ksi, eta);
               thirdRight = CreateNode(ksi + (nextKsi - ksi) / 3.0, eta);
               twoThirdsRight = CreateNode(ksi + 2.0 * (nextKsi - ksi) / 3.0, eta);
               constantEtaArray[col] = new MeshNode[] {twoThirdsAbove, thirdAbove, corner, thirdRight, twoThirdsRight};
               ksi += 1.0 / Mesh.ElementDensity;                             // Increase ksi.
               nextKsi += 1.0 / Mesh.ElementDensity;
               ++col; }
            twoThirdsAbove = CreateNode(1.0, eta + 2.0 * (nextEta - eta) / 3.0);    // Create final three positions in row.
            thirdAbove = CreateNode(1.0, eta + (nextEta - eta) / 3.0);
            corner = CreateNode(1.0, eta);
            constantEtaArray[col] = new MeshNode[] {
               twoThirdsAbove, thirdAbove, corner,
               new MeshNode(Double.NaN, Double.NaN, 0), new MeshNode(Double.NaN, Double.NaN, 0) };
            eta = nextEta;                                                          // Increase eta.
            nextEta = NextEta(nextEta); }
         ksi = 0.0;                                                                 // Finalize the top-most row of nodes with current eta and nextEta = 1.0.
         nextKsi = 1.0 / Mesh.ElementDensity;
         constantEtaArray = new MeshNode[21][];
         nodes.Add(constantEtaArray);
         col = 0;                                            // Reset col counter.
         while(ksi <= 1.0) {
            corner = CreateNode(ksi, 1.0);
            thirdRight = CreateNode(ksi + (nextKsi - ksi) / 3.0, 1.0);
            twoThirdsRight = CreateNode(ksi + 2.0 * (nextKsi - ksi) / 3.0, 1.0);
            constantEtaArray[col] = new MeshNode[] {
               new MeshNode(Double.NaN, Double.NaN, 0), new MeshNode(Double.NaN, Double.NaN, 0),
               corner, thirdRight, twoThirdsRight };
            ksi += 1.0 / Mesh.ElementDensity;                                  // Increase ksi.
            nextKsi += 1.0 / Mesh.ElementDensity;
            ++col; }
         corner = CreateNode(1.0, 1.0);                                             // Now add one final point.
         constantEtaArray[col] = new MeshNode[] {
            new MeshNode(Double.NaN, Double.NaN, 0), new MeshNode(Double.NaN, Double.NaN, 0),
            corner,
            new MeshNode(Double.NaN, Double.NaN, 0), new MeshNode(Double.NaN, Double.NaN, 0) };
         NRows = nodes.Count - 1;                                           // Update row and column counts.
         NCols = 20;
         _Nodes = nodes.ToArray();
      }
      /// <summary>Export positions of left half of obstruction block. We shall integrate over each element in Mathematica. Requires even number of elements on side.</summary><remarks>Suggestion: introduce 2D integrator into this program and automatize process.</remarks>
      public void WriteLeftHalfOnly(string fileName) {
         FileInfo file = new FileInfo(fileName);
         using(StreamWriter sw = new StreamWriter(file.FullName, false)) {
            sw.WriteLine("{");
            for(int i = 0; i < NRows - 1; ++i) {                                 // Over all rows.
            sw.Write("{");
               for(int j = NCols/2; j < NCols - 1; ++j) {
                  sw.Write("{");
                  for(int k = 0; k < 9; k+=3)                                       // We only need corners for integration.
                     sw.Write($"{{{NodeStd(i,j,k).ToString()}}}, ");
                  sw.Write($"{{{NodeStd(i,j,9).ToString()}}}}}, "); }
               sw.Write("{");
               for(int k = 0; k < 9; k+=3)
                  sw.Write($"{{{NodeStd(i,NCols - 1,k).ToString()}}}, ");
               sw.Write($"{{{NodeStd(i,NCols - 1,9).ToString()}}}}}");
               sw.WriteLine("}, "); }
            sw.Write("{");
            for(int j = NCols/2; j < NCols - 1; ++j) {
               sw.Write("{");
               for(int k = 0; k < 9; k+=3)
                  sw.Write($"{{{NodeStd(NRows - 1,j,k).ToString()}}}, ");
               sw.Write($"{{{NodeStd(NRows - 1,j,9).ToString()}}}}}, "); }
            sw.Write("{");
            for(int k = 0; k < 9; k+=3)
               sw.Write($"{{{NodeStd(NRows - 1,NCols - 1,k).ToString()}}}, ");
            sw.WriteLine($"{{{NodeStd(NRows - 1,NCols - 1,9).ToString()}}}}}}}");
            sw.Write("}"); }
      }
      /// <summary>A getter that fetches an integral from compact integrals array based on intuitive indices we specify.</summary><param name="row">Obstruction block row (0 to 22).</param><param name="col">Obstruction block column (0 to 39).</param><param name="j">First overlapping basis function (0 to 11).</param><param name="k">Second overlapping basis function (0 to 11).</param><param name="n">First term index (0 to 4).</param><param name="m">Second term index (0 to 4).</param>
      protected double GetSfsIntegral(int row, int col, int j, int k, int n, int m) {
         if(row < 0)
            throw new ArgumentOutOfRangeException("Row index too small, below 0.");
         else if(row < 23) {
            int transCol;
            int transJ;
            int transK;
            if(col < 0)
               throw new ArgumentOutOfRangeException("Column index too small, below 0.");
            else if(col < 10) {             // Left side. We have to mirror functions across y axis.
               transCol = 9 - col;
               transJ = (15 - j) % 12;
               transK = (15 - k) % 12;                                    // Account fot the fact that k is always such that [j][k] forms an upper left triangular matrix.
               if(transJ > transK) {                           // j has to be smaller than k at entry.
                  Swap<int>(ref j, ref k);
                  transJ = (15 - j) % 12;
                  transK = (15 - k) % 12; }
               transK -= transJ;                               // Shift to account for upper fact.
               return _StiffnessIntegrals[row][transCol][transJ][transK][n][m]; }
            else if(col < 20) {
               transCol = col - 10;
               if(k < j)
                  Swap(ref j, ref k);
               transK = k - j;                                                     // Account fot the fact that k is always such that [j][k] forms an upper left triangular matrix.
               return _StiffnessIntegrals[row][transCol][j][transK][n][m]; }
            else
               throw new ArgumentOutOfRangeException("Column index too big, above 19."); }
         else
            throw new ArgumentOutOfRangeException("Row index too big, above 22.");
      }
      /// <summary>A getter that fetches an integral from compact integrals array based on intuitive indices we specify.</summary><param name="row">Obstruction block row (0 to 22).</param><param name="col">Obstruction block column (0 to 39).</param><param name="j">First overlapping basis function (0 to 11).</param><param name="n">First term index (0 to 4).</param>
      protected double GetForcingIntegral(int row, int col, int j, int n) {
         if(row < 0)
            throw new ArgumentOutOfRangeException("Row index too small, below 0.");
         else if(row < 23) {
            int transCol;
            int transJ;
            if(col < 0)
               throw new ArgumentOutOfRangeException("Column index too small, below 0.");
            else if(col < 10) {             // Left side. We have to mirror functions across y axis.
               transCol = 9 - col;
               transJ = (15 - j) % 12;
               return _ForcingIntegrals[row][transCol][transJ][n]; }
            else if(col < 20) {
               transCol = col - 10;
               return _ForcingIntegrals[row][transCol][j][n]; }
            else
               throw new ArgumentOutOfRangeException("Column index too big, above 19."); }
         else
            throw new ArgumentOutOfRangeException("Row index too big, above 22.");
      }
      /// <summary>Add whole block's contribution to global stiffness matrix.</summary><param name="A">Gloabal stiffness matrix.</param><param name="dt">Time step.</param><param name="ni">Viscosity.</param>
      public override void AddContribsToSfsMatrix(Tensor A, double dt, double ni) {
         for(int row = 0; row < 23; ++row)
            for(int col = 0; col < 20; ++col) {
               TB.Reporter.Write($"Element ({row},{col}).");
               AddEmtContribToSfsMatrix(A, row, col, dt, ni); }
      }
      /// <summary>Add contribution from element at specified row and col to global stiffness matrix.</summary><param name="A">Global stiffness matrix.</param><param name="row">Mesh block row where element is situated.</param><param name="col">Mesh block col where element is situated.</param><param name="dt">Time step.</param><param name="ni">Viscosity.</param>
      void AddEmtContribToSfsMatrix(Tensor A, int row, int col, double dt, double ni) {
         double[][] subResult;
         int globalRowBelt;                                                                  // Starting index of an octuple of rows which represent variable values at a single position.
         int globalColBelt;
         for(int j = 0; j < 12; ++j)                                                         // Over first basis function.
            for(int k = j; k < 12; ++k) {                                                    // Over second basis function.
               subResult = SubMatrix(row, col, j, k, dt, ni);                                // 8 x 8 matrix which has to be added to global stiffness matrix.
               globalRowBelt = GblInxFromStdInx(row, col, j);
               globalColBelt = GblInxFromStdInx(row, col, k);
               if(this is WestBlock && ((row == 0 && col == 0) || (row == 0 && col == 1)))
                  TB.Reporter.Write("Writing from element matrix to global matrix.");
               for(int subResultRow = 0; subResultRow < 8; ++subResultRow)
                  for(int subResultCol = 0; subResultCol < 8; ++subResultCol) {                                                       // Using symmetry of global stiffness matrix.
                     A[globalRowBelt * 8 + subResultRow, globalColBelt * 8 + subResultCol] +=
                        subResult[subResultRow][subResultCol];
                     A[globalColBelt * 8 + subResultCol, globalRowBelt * 8 + subResultRow] +=
                        subResult[subResultRow][subResultCol]; } }
      }
      /// <summary>Creates an 8 x 8 submatrix of a 96 x 96 element matrix for some choice of j,k = 0,...,11.</summary><param name="row">Mesh block row of element.</param><param name="col">Mesh block column row of element.</param><param name="j">First overlapping basis function.</param><param name="k">Second overlapping basis function. k >= j</param><param name="dt">Time step.</param><param name="ni">Viscosity.</param><remarks>Is only valid for k >= j. Make use of fact that for a fixed element (fixed row and col) submatrix at (j,k) is equal to submatrix at (k,j).</remarks>
      double[][] SubMatrix(int row, int col, int j, int k, double dt, double ni) {
         double[][] subMatrix = new double[8][];
         double[][][][] A = new double[2][][][];                         // For two nodes j and k.
         for(int i = 0; i < 8; ++i)
            subMatrix[i] = new double[8];
         A[0] = new double[3][][];                                                   // Create operators for node1. For 3 different matrices A0, A1, A2.
         A[0][0] = NodeOperatorMat0(NodeStd(row, col, j), dt);
         A[0][0].Transpose();
         A[0][1] = NodeOperatorMat1(ni);
         A[0][1].Transpose();
         A[0][2] = NodeOperatorMat2(ni);
         A[0][2].Transpose();
         A[1] = new double[3][][];                                                   // Create operators for node1.
         A[1][0] = NodeOperatorMat0(NodeStd(row, col, k), dt);
         A[1][1] = NodeOperatorMat1(ni);
         A[1][2] = NodeOperatorMat2(ni);
         for(int n = 0; n < 5; ++n)
            for(int m = 0; m < 5; ++m)
               subMatrix.SumInto<dbl,DA>(GetSfsIntegral(row, col, j, k, n, m).Mul<dbl,DA>(
                  A[0][NewN(n)].Dot<dbl,DA>(A[1][NewN(m)])));
         return subMatrix;

         int NewN(int n) => n < 3 ? n : n - 2;                      // First 3 terms contain: A0, A1, A2; last two terms contain A1 and A2.
      }
      /// <summary>Add whole block's contribution to global forcing vector.</summary><param name="b">Gloabal forcing vector.</param><param name="dt">Time step.</param><param name="ni">Viscosity.</param>
      public override void AddContribsToFcgVector(Vector b, double dt, double ni) {
         for(int row = 0; row < 23; ++row)
            for(int col = 0; col < 20; ++col)
               AddEmtContribToFcgVec(b, row, col, dt, ni);
      }
      /// <summary>Add contribution from element at specified row and col to global forcing vector.</summary><param name="b">Global forcing vector.</param><param name="row">Mesh block row where element is situated.</param><param name="col">Mesh block col where element is situated.</param><param name="dt">Time step.</param><param name="ni">Viscosity.</param>
      void AddEmtContribToFcgVec(Vector b, int row, int col, double dt, double ni) {
         double[] subVec;
         int gblRowBelt;                                          // Starting index of an octuple of rows which represent variable values at a single position.
         for(int j = 0; j < 12; ++j) {                               // Over basis functions.
            subVec = SubVec(row, col, j, dt, ni);             // 8 x 8 matrix which has to be added to global stiffness matrix.
            gblRowBelt = GblInxFromStdInx(row, col, j);
            for(int subResRowInx = 0; subResRowInx < 8; ++subResRowInx)
               b[gblRowBelt * 8 + subResRowInx] += subVec[subResRowInx]; }
      }
      /// <summary>Creates an 8 element subvector of a 96 element forcing vector  for some choice of j = 0,...,11.</summary><param name="row">Mesh block row of element.</param><param name="col">Mesh block column row of element.</param><param name="j">First overlapping basis function.</param><param name="dt">Time step.</param><param name="ni">Viscosity.</param><param name=x>Previous values of variables at point (row,col,j).</param>
      double[] SubVec(int row, int col, int j, double dt, double ni) {
         var subVector = new double[8];
         var p = NodeStd(row, col, j);
         var A = new double[3][][];                                                      // For three different operators A.            
         A[0] = NodeOperatorMat0(p, dt);                // Create 3 different matrices A0, A1, A2.
         A[0].Transpose();
         A[1] = NodeOperatorMat1(ni);
         A[1].Transpose();
         A[2] = NodeOperatorMat2(ni);
         A[2].Transpose();
         var aTf = new double[8];                                                        // Elemental forcing vector.
         double[][] fCoefs = new double[8][];                                           // Coefficients accompanying terms in f vector.
         fCoefs[0] = new double[4] {-p.Vars[6].Val, ni*p.Vars[2].Val, ni*p.Vars[3].Val,
            -p.Vars[0].Val * p.Vars[2].Val - p.Vars[1].Val * p.Vars[3].Val};
         fCoefs[1] = new double[4] {-p.Vars[7].Val, ni*p.Vars[4].Val, -ni*p.Vars[2].Val,
            -p.Vars[0].Val * p.Vars[4].Val + p.Vars[1].Val * p.Vars[2].Val};
         fCoefs[2] = new double[2] {-p.Vars[2].Val, p.Vars[0].Val};
         fCoefs[3] = new double[2] {-p.Vars[3].Val, p.Vars[0].Val};
         fCoefs[4] = new double[2] {-p.Vars[4].Val, p.Vars[1].Val};
         fCoefs[5] = new double[2] {-p.Vars[6].Val, p.Vars[5].Val};
         fCoefs[6] = new double[2] {-p.Vars[7].Val, p.Vars[5].Val};
         fCoefs[7] = new double[2] {p.Vars[7].Val, -p.Vars[6].Val};
         for(int vecRow = 0; vecRow < 8; ++vecRow)                                     // For each entry in elemental vector.
            for(int n = 0; n < 5; ++n) {                                                // For each left term-
               for(int matCol = 0; matCol < 2; ++matCol)
                  aTf[vecRow] += A[NewN(n)][vecRow][matCol] * (fCoefs[matCol][0] *    // A[pick matrix][pick row][pick col]
                     GetSfsIntegral(row,col,j,j,n,0) + fCoefs[0][1] *
                     (GetSfsIntegral(row,col,j,j,n,1) + GetSfsIntegral(row,col,j,j,n,2)) +
                     fCoefs[0][2] * (GetSfsIntegral(row,col,j,j,n,3) +
                     GetSfsIntegral(row,col,j,j,n,4)) + fCoefs[0][3] *
                     GetForcingIntegral(row,col,j,n));
               aTf[vecRow] += A[NewN(n)][vecRow][2] * (fCoefs[2][0] *
                  GetSfsIntegral(row,col,j,j,n,0) + fCoefs[2][1] *
                  (GetSfsIntegral(row,col,j,j,n,1) + GetSfsIntegral(row,col,j,j,n,3)));
               aTf[vecRow] += A[NewN(n)][vecRow][3] * (fCoefs[3][0] *
                  GetSfsIntegral(row,col,j,j,n,0) + fCoefs[3][1] *
                  (GetSfsIntegral(row,col,j,j,n,2) + GetSfsIntegral(row,col,j,j,n,4)));
               for(int matCol = 4; matCol < 6; ++matCol)
                  aTf[vecRow] += A[NewN(n)][vecRow][matCol] * (fCoefs[matCol][0] *
                  GetSfsIntegral(row,col,j,j,n,0) + fCoefs[matCol][1] *
                  (GetSfsIntegral(row,col,j,j,n,1) + GetSfsIntegral(row,col,j,j,n,3)));
               aTf[vecRow] += A[NewN(n)][vecRow][6] * (fCoefs[6][0] *
                  GetSfsIntegral(row,col,j,j,n,0) + fCoefs[6][1] *
                  (GetSfsIntegral(row,col,j,j,n,2) + GetSfsIntegral(row,col,j,j,n,4)));
               aTf[vecRow] += A[NewN(n)][vecRow][7] * (fCoefs[7][0] *
                  (GetSfsIntegral(row,col,j,j,n,1) + GetSfsIntegral(row,col,j,j,n,3)) +
                  fCoefs[7][1] * (GetSfsIntegral(row,col,j,j,n,2) +
                  GetSfsIntegral(row,col,j,j,n,4))); }
         return aTf;

         int NewN(int n) => n < 3 ? n : n - 2;                     // First 3 terms contain: A0, A1, A2; last two terms contain A1 and A2.
      }
      /// <summary>Determines whether point is inside Obstruction block which is itself a polygon.</summary><param name="pos">Point's position.</param>
      public override bool IsPointInside(in Pos pos) {
         int nVertices = 2*(NRows+NCols);                                // Construct vertices array.
         var vertices = new Pos[nVertices];
         int counter = 0;
         for(int col = 0; col < NCols; ++col) {                              // Add vertices on lower edge.
               vertices[counter] = NodeStd(0, col, 0).Pos;
               ++counter; }
         for(int row = 0; row < NRows; ++row) {                              // Add vertices on right edge.
               vertices[counter] = NodeStd(row, NCols - 1, 3).Pos;
               ++counter; }
         for(int col = NCols - 1; col > 0; --col) {                          // Add vertices on upper edge.
               vertices[counter] = NodeStd(NRows - 1, col, 6).Pos;
               ++counter; }
         for(int row = NRows - 1; row > 0; --row) {                          // Add vertices on left edge.
               vertices[counter] = NodeStd(row, 0, 9).Pos;
               ++counter; }
         return pos.IsInsidePolygon(vertices);
      }
   }
}