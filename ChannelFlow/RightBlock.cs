using System;
using Fluid.Dynamics.Numerics;
using Fluid.Dynamics.Meshing;
using static Fluid.Dynamics.Internals.IOHelper;
using static Fluid.Dynamics.Internals.GeneralHelper;
using static System.Math;
using static Fluid.Dynamics.Numerics.Matrix;

namespace Fluid.ChannelFlow
{
    public sealed class RightBlock : RectangularBlock
    {
        ChannelFlow _channelFlow;
        ChannelMesh _channelMesh;
        /// <summary>Basis function overlap integrals over a single element, since all elements are the same.</summary><remarks>[j][k][n][m] = [12 basis funcs][12-j basis funcs][5 terms][5 terms]</remarks>
        public static double[][][][] _rectStiffnessIntegrals;
        /// <summary>Basis function overlap integrals over a single element, since all elements are the same.</summary><remarks>[j][n] = [12 basis funcs][5 terms]</remarks>
        public static double[][] _rectForcingIntegrals;

        static RightBlock() {
            _rectStiffnessIntegrals = ReadRectStiffnessIntegrals("ChannelFlow/Input/rectElementStiffnessIntegrals.txt");
            _rectForcingIntegrals = ReadRectForcingIntegrals("ChannelFlow/Input/rectElementForcingIntegrals.txt");
        }

        /// <summary>Create a Cartesian mesh block.</summary><param name="channelMesh">Mesh block's owner.</param><param name="lowerLeftX">Lower left corner x coordinate.</param><param name="lowerLeftY">Lower left corner y coordinate.</param><param name="upperRightX">Upper right corner x coordinate.</param><param name="upperRightY">Upper right corner y coordinate.</param><param name="rowCount">Number of elements in y direction.</param><param name="columnCount">Number of elements in x direction.</param>
        public RightBlock(ChannelMesh channelMesh, ChannelFlow channelFlow, EastBlock eastBlock,
            double lowerLeftX, double lowerLeftY, double upperRightX, double upperRightY,
            int rowCount, int columnCount)
        : base(channelMesh, lowerLeftX, lowerLeftY, upperRightX, upperRightY,
            rowCount, columnCount) {
            
            _channelMesh = channelMesh;
            _channelFlow = channelFlow;
            //_nodes = new Node[21][][];
            CreateNodes();
            _constraintCount = ApplyConstraints();
            _channelMesh.SetConstraintCount(_channelMesh.GetConstraintCount() + _constraintCount);
            MoveNodesToMainMesh(eastBlock);
        }


        protected override int ApplyConstraints() {
            int constraintCount = 0;
            int col = 0;

            while(col < _colCount) {                                            // Col 0 - 59

                for(int element = 2; element < 5; ++element) {                  // Upper Channel boundary.
                    GetNodeCmp(20, col, element).Constrainedness(0) = true;     // u
                    GetNodeCmp(20, col, element).Constrainedness(1) = true;     // v
                    GetNodeCmp(20, col, element).Constrainedness(2) = true;     // a
                    GetNodeCmp(20, col, element).Constrainedness(4) = true;     // c
                    constraintCount += 4;
                }
                for(int element = 2; element < 5; ++element) {                  // Lower Channel boundary.
                    GetNodeCmp(0, col, element).Constrainedness(0) = true;      // u
                    GetNodeCmp(0, col, element).Constrainedness(1) = true;      // v
                    GetNodeCmp(0, col, element).Constrainedness(2) = true;      // a
                    GetNodeCmp(0, col, element).Constrainedness(4) = true;      // c
                    constraintCount += 4;
                }
                ++col;
            }
                                                                                // Col 60
            GetNodeCmp(20, col, 2).Constrainedness(0) = true;                   // Channel boundary, u.
            GetNodeCmp(20, col, 2).Constrainedness(1) = true;                   // v
            GetNodeCmp(20, col, 2).Constrainedness(2) = true;                   // a
            GetNodeCmp(20, col, 2).Constrainedness(4) = true;                   // c

            GetNodeCmp(0, col, 2).Constrainedness(0) = true;                    // Channel boundary, u.
            GetNodeCmp(0, col, 2).Constrainedness(1) = true;                    // v
            GetNodeCmp(0, col, 2).Constrainedness(2) = true;                    // a
            GetNodeCmp(0, col, 2).Constrainedness(4) = true;                    // c
            constraintCount += 8;
            return constraintCount;
        }

        void MoveNodesToMainMesh(EastBlock eastBlock) {
            int posCount = _channelMesh.GetPositionCount();
            var blockToGlobal = new int[_rowCount + 1][][];
            int row = 0;
            int col = 0;
            var eastMap = eastBlock.GetCompactPosIndexToGlobalPosIndexMap();           // We will need EastBlock's map.

            while(row < 20) {                                                   // Row 0 - 19
                blockToGlobal[row] = new int[_colCount + 1][];
                col = 0;                           
                blockToGlobal[row][col] = new int[5];
                blockToGlobal[row][col][0] = eastMap[23][19 - row][3];          // Col 0
                blockToGlobal[row][col][1] = eastMap[23][19 - row][4];
                blockToGlobal[row][col][2] = eastMap[23][20 - row][2];
                for(int node = 3; node < 5; ++node) {
                    _channelMesh.Node(posCount) = GetNodeCmp(row, col, node);
                    blockToGlobal[row][col][node] = posCount++;
                }
                col = 1;

                while(col < _colCount) {                                             // Col 1 - 59
                    blockToGlobal[row][col] = new int[5];
                    for(int node = 0; node < 5; ++node) {
                        _channelMesh.Node(posCount) = GetNodeCmp(row, col, node);
                        blockToGlobal[row][col][node] = posCount++;
                    }
                    ++col;
                }
                blockToGlobal[row][col] = new int[5];                               // Col 60, last col.
                for(int node = 0; node < 3; ++node) {
                    _channelMesh.Node(posCount) = GetNodeCmp(row, col, node);
                    blockToGlobal[row][col][node] = posCount++;
                }
                for(int node = 3; node < 5; ++node) {
                    blockToGlobal[row][col][node] = Int32.MinValue;
                }
                ++row;
            }                                      
            col = 0;                                                            // Row 20
            blockToGlobal[row] = new int[_colCount+1][];
            blockToGlobal[row][col] = new int[5];                                   // Col 0
            for(int node = 0; node < 2; ++node) {
                blockToGlobal[row][col][node] = Int32.MinValue;
            }
            blockToGlobal[row][col][2] = eastMap[23][0][2];
            for(int node = 3; node < 5; ++node) {
                _channelMesh.Node(posCount) = GetNodeCmp(row, col, node);
                blockToGlobal[row][col][node] = posCount++;
            }
            col = 1;

            while(col < _colCount) {                                                 // Col 1 - 59
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
            blockToGlobal[row][col] = new int[5];                                   // Col 60
            for(int node = 0; node < 2; ++node) {
                blockToGlobal[row][col][node] = Int32.MinValue;
            }
            _channelMesh.Node(posCount) = GetNodeCmp(row, col, 2);
            blockToGlobal[row][col][2] = posCount++;                             // Last position to be added.
            for(int node = 3; node < 5; ++node) {
                blockToGlobal[row][col][node] = Int32.MinValue;
            }
            SetCompactPosIndexToGlobalPosIndexMap(blockToGlobal);
            _channelMesh.SetPositionCount(posCount);                          // In our case, this now has to be 15 620.
            _nodes = null;                                                  // Free memory on block.
            GetNodeCmp = GetNodeCmpGlobal;                                  // Rewire.
            GetNodeStd = GetNodeStdGlobal;
        }

        /// <summary>Get an overlap integral of basis functions j and k. Get term n, m.</summary><param name="j">First overlapping basis function.</param><param name="k">Second overlapping basis function.</param><param name="n">Factor row.</param><param name="m">Factor column.</param>
        double GetStiffnessIntegral(int j, int k, int n, int m) {
            
            if(k < j)
                Swap(ref j, ref k);                                                     // Account fot the fact that k is always such that [j][k] forms an upper left triangular matrix.
            k -= j; 
            return _rectStiffnessIntegrals[j][k][n][m];
            // TODO: Correct behavior of this method. j and k have special properties.
        }

        /// <summary>Get an overlap integral of basis functions j and k. Get term n, m.</summary><param name="j">First overlapping basis function.</param><param name="k">Second overlapping basis function.</param><param name="n">Factor row.</param><param name="m">Factor column.</param>
        double GetForcingIntegral(int j, int n) {
            return _rectForcingIntegrals[j][n];
        }

        public override void AddContributionsToStiffnessMatrix(SparseMatrix<double> A, double dt, double ni) {
            for(int row = 0; row < 20; ++row) {
                for(int col = 0; col < 60; ++col) {
                    AddElementContributionToStiffnessMatrix(A, row, col, dt, ni);
                }
            }
        }

        /// <summary>Add contribution from element at specified row and col to global stiffness matrix.</summary><param name="A">Global stiffness matrix.</param><param name="row">Mesh block row where element is situated.</param><param name="col">Mesh block col where element is situated.</param><param name="dt">Time step.</param><param name="ni">Viscosity.</param>
        void AddElementContributionToStiffnessMatrix(SparseMatrix<double> A, int row, int col, double dt, double ni) {
            double[][] subResult;
            int globalRowBelt;                                          // Starting index of an octuple of rows which represent variable values at a single position.
            int globalColBelt;

            for(int j = 0; j < 12; ++j) {                               // Over first basis function.

                for(int k = j; k < 12; ++k) {                           // Over second basis function.
                    subResult = SubMatrix(row, col, j, k, dt, ni);      // 8 x 8 matrix which has to be added to global stiffness matrix.
                    globalRowBelt = GlobalPositionIndexStd(row, col, j);
                    globalColBelt = GlobalPositionIndexStd(row, col, k);

                    for(int subResultRow = 0; subResultRow < 8; ++subResultRow) {
                        
                        for(int subResultCol = 0; subResultCol < 8; ++subResultCol) {                                                       // Using symmetry of global stiffness matrix.
                            A[globalRowBelt * 8 + subResultRow][globalColBelt * 8 + subResultCol] = subResult[subResultRow][subResultCol];
                            A[globalColBelt * 8 + subResultCol][globalRowBelt * 8 + subResultRow] = subResult[subResultRow][subResultCol];
                        }
                    }
                }
            }
        }

        double[][] SubMatrix(int row, int col, int j, int k, double dt, double ni) {
            double[][] subMatrix = new double[8][];
            double[][][][] A = new double[2][][][];                                 // For two nodes j and k. Next dimension contains 3 elements: A0, A1, A2.

            for(int i = 0; i < 8; ++i) {
                subMatrix[i] = new double[8];
            }
            ref var node1 = ref GetNodeStd(row, col, j);
            ref var node2 = ref GetNodeStd(row, col, k);
            A[0] = new double[3][][];                                               // Create operators for node1. For 3 different matrices A0, A1, A2.
            A[0][0] = NodeOperatorMatrix0(ref node1, dt, ni);
            Transpose(ref A[0][0]);
            A[0][1] = NodeOperatorMatrix1(ref node1, dt, ni);
            Transpose(ref A[0][1]);
            A[0][2] = NodeOperatorMatrix1(ref node1, dt, ni);
            Transpose(ref A[0][2]);
            A[1] = new double[3][][];                                                   // Create operators for node1.
            A[1][0] = NodeOperatorMatrix0(ref node2, dt, ni);
            A[1][1] = NodeOperatorMatrix1(ref node2, dt, ni);
            A[1][2] = NodeOperatorMatrix1(ref node2, dt, ni);

            for(int n = 0; n < 5; ++n) {

                for(int m = 0; m < 5; ++m) {
                    AddTo(ref subMatrix, Times(GetStiffnessIntegral(j, k, n, m), Dot(A[0][NewN(n)], A[1][NewN(m)])));
                }
            }
            return subMatrix;


            int NewN(int n) {                       // First 3 terms contain: A0, A1, A2; last two terms contain A1 and A2.
                return n < 3 ? n : n - 2;
            }
        }

        /// <summary>Add whole block's contribution to global forcing vector.</summary><param name="b">Gloabal forcing vector.</param><param name="dt">Time step.</param><param name="ni">Viscosity.</param>
        public override void AddContributionsToForcingVector(SparseRow<double> b, double dt, double ni) {

            for(int row = 0; row < 20; ++row) {
                for(int col = 0; col < 60; ++col) {
                    AddElementContributionToForcingVector(b, row, col, dt, ni);
                }
            }
        }

        /// <summary>Add contribution from element at specified row and col to global forcing vector.</summary><param name="b">Global forcing vector.</param><param name="row">Mesh block row where element is situated.</param><param name="col">Mesh block col where element is situated.</param><param name="dt">Time step.</param><param name="ni">Viscosity.</param>
        void AddElementContributionToForcingVector(SparseRow<double> b, int row, int col, double dt, double ni) {
            double[] subVector;
            int globalRowBelt;                                          // Starting index of an octuple of rows which represent variable values at a single position.

            for(int j = 0; j < 12; ++j) {                               // Over basis functions.

                subVector = SubVector(row, col, j, dt, ni);      // 8 x 8 matrix which has to be added to global stiffness matrix.
                globalRowBelt = GlobalPositionIndexStd(row, col, j);

                for(int subResultRow = 0; subResultRow < 8; ++subResultRow) {
                    b[globalRowBelt * 8 + subResultRow] = subVector[subResultRow];
                }
            }
        }

        /// <summary>Creates an 8 element subvector of a 96 element forcing vector  for some choice of j = 0,...,11.</summary><param name="row">Mesh block row of element.</param><param name="col">Mesh block column row of element.</param><param name="j">First overlapping basis function.</param><param name="dt">Time step.</param><param name="ni">Viscosity.</param><param name=x>Previous values of variables at point (row,col,j).</param>
        double[] SubVector(int row, int col, int j, double dt, double ni) {
            var subVector = new double[8];
            ref var node = ref GetNodeStd(row, col, j);
            var A = new double[3][][];                                                      // For three different operators A.            
            A[0] = NodeOperatorMatrix0(ref node, dt, ni);                // Create 3 different matrices A0, A1, A2.
            Transpose(ref A[0]);
            A[1] = NodeOperatorMatrix1(ref node, dt, ni);
            Transpose(ref A[1]);
            A[2] = NodeOperatorMatrix1(ref node, dt, ni);
            Transpose(ref A[2]);
            var aTf = new double[8];                                                        // Elemental forcing vector.
            double[][] fCoeffs = new double[8][];                                           // Coefficients accompanying terms in f vector.
            fCoeffs[0] = new double[4] {-node.Var(6)._value, ni*node.Var(2)._value, ni*node.Var(3)._value, -node.Var(0)._value * node.Var(2)._value - node.Var(1)._value * node.Var(3)._value};
            fCoeffs[1] = new double[4] {-node.Var(7)._value, ni*node.Var(4)._value, -ni*node.Var(2)._value, -node.Var(0)._value * node.Var(4)._value + node.Var(1)._value * node.Var(2)._value};
            fCoeffs[2] = new double[2] {-node.Var(2)._value, node.Var(0)._value};
            fCoeffs[3] = new double[2] {-node.Var(3)._value, node.Var(0)._value};
            fCoeffs[4] = new double[2] {-node.Var(4)._value, node.Var(1)._value};
            fCoeffs[5] = new double[2] {-node.Var(6)._value, node.Var(5)._value};
            fCoeffs[6] = new double[2] {-node.Var(7)._value, node.Var(5)._value};
            fCoeffs[7] = new double[2] {node.Var(7)._value, -node.Var(6)._value};

            for(int vecRow = 0; vecRow < 8; ++vecRow) {                                     // For each entry in elemental vector.
                
                for(int n = 0; n < 5; ++n) {                                                // For each left term-

                    for(int matCol = 0; matCol < 2; ++matCol) {

                        aTf[vecRow] += A[NewN(n)][vecRow][matCol] * (fCoeffs[matCol][0] * GetStiffnessIntegral(j,j,n,0) +   // A[pick matrix][pick row][pick col]
                            fCoeffs[0][1] * (GetStiffnessIntegral(j,j,n,1) + GetStiffnessIntegral(j,j,n,2)) +
                            fCoeffs[0][2] * (GetStiffnessIntegral(j,j,n,3) + GetStiffnessIntegral(j,j,n,4)) +
                            fCoeffs[0][3] * GetForcingIntegral(j,n));
                    }
                    aTf[vecRow] += A[NewN(n)][vecRow][2] * (fCoeffs[2][0] * GetStiffnessIntegral(j,j,n,0) + 
                    fCoeffs[2][1] * (GetStiffnessIntegral(j,j,n,1) + GetStiffnessIntegral(j,j,n,3)));
                    aTf[vecRow] += A[NewN(n)][vecRow][3] * (fCoeffs[3][0] * GetStiffnessIntegral(j,j,n,0) + 
                    fCoeffs[3][1] * (GetStiffnessIntegral(j,j,n,2) + GetStiffnessIntegral(j,j,n,4)));

                    for(int matCol = 4; matCol < 6; ++matCol) {
                        aTf[vecRow] += A[NewN(n)][vecRow][matCol] * (fCoeffs[matCol][0] * GetStiffnessIntegral(j,j,n,0) + 
                    fCoeffs[matCol][1] * (GetStiffnessIntegral(j,j,n,1) + GetStiffnessIntegral(j,j,n,3)));
                    }
                    aTf[vecRow] += A[NewN(n)][vecRow][6] * (fCoeffs[6][0] * GetStiffnessIntegral(j,j,n,0) + 
                    fCoeffs[6][1] * (GetStiffnessIntegral(j,j,n,2) + GetStiffnessIntegral(j,j,n,4)));
                    aTf[vecRow] += A[NewN(n)][vecRow][7] * (fCoeffs[7][0] * (GetStiffnessIntegral(j,j,n,1) + GetStiffnessIntegral(j,j,n,3)) +
                        fCoeffs[7][1] * (GetStiffnessIntegral(j,j,n,2) + GetStiffnessIntegral(j,j,n,4)));
                }
            }
            return aTf;


            int NewN(int n) {                       // First 3 terms contain: A0, A1, A2; last two terms contain A1 and A2.
                return n < 3 ? n : n - 2;
            }
        }
    }
}