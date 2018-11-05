using System;
using Xunit;
using Fluid.Dynamics.Internals;
using Fluid.Dynamics.Numerics;
using Fluid.Dynamics.Meshing;
using Fluid.ChannelFlow;
using static System.Console;
using static Fluid.Dynamics.Internals.IOHelper;

namespace Fluid.Dynamics.Tests
{
    public class OtherTests
    {
        [Fact] public void InvertMatrix1() {
            double[][] matrix = new double[3][] {
                new double[3] {6, 0, 0},
                new double[3] {0, 2, 0},
                new double[3] {0, 0, 8}
            };
            matrix.Invert();
            double epsilon = 0.000001;
            var expectedResult = new double[3][] {
                new double[3] { 1.0/ 6, 0.0, 0.0   },
                new double[3] {    0.0, 0.5, 0.0   },
                new double[3] {    0.0, 0.0, 0.125 }
            };
            Assert.True(ArrayHelper.Equals(matrix, expectedResult, epsilon));
        }

        [Fact] public void InvertMatrix2() {
            double[][] matrix2 = new double[3][] {
                new double[3] {1, 2, 3},
                new double[3] {4, 2, 2},
                new double[3] {5, 1, 7}
            };
            matrix2.Invert();
            double epsilon = 0.000001;
            var expectedResult2 = new double[3][] {
                new double[3] {-2.0/7, 11.0/42, 1.0/21},
                new double[3] { 3.0/7, 4.0/21, -5.0/21},
                new double[3] { 1.0/7, -3.0/14, 1.0/7}
            };
            Assert.True(ArrayHelper.Equals(matrix2, expectedResult2, epsilon));
        }

        /// <summary>Test transformation from standard block indices to compact block indices.</summary>
        [Fact] public void CmpToStdIndexTransform() {
            var block = new ObstructionBlockTester();
            Assert.True( block.CompactPosIndex(0,0,0) == (0,0,2) );
            Assert.True( block.CompactPosIndex(0,0,1) == (0,0,3) );
            Assert.True( block.CompactPosIndex(0,0,2) == (0,0,4) );
            Assert.True( block.CompactPosIndex(0,0,3) == (0,1,2) );
            Assert.True( block.CompactPosIndex(0,0,4) == (0,1,1) );
            Assert.True( block.CompactPosIndex(0,0,5) == (0,1,0) );
            Assert.True( block.CompactPosIndex(0,0,6) == (1,1,2) );
            Assert.True( block.CompactPosIndex(0,0,7) == (1,0,4) );
            Assert.True( block.CompactPosIndex(0,0,8) == (1,0,3) );
            Assert.True( block.CompactPosIndex(0,0,9) == (1,0,2) );
            Assert.True( block.CompactPosIndex(0,0,10) == (0,0,0) );
            Assert.True( block.CompactPosIndex(0,0,11) == (0,0,1) );
        }

        /// <summary>Test reading of Mathematica integration results from file into array.</summary>
        [Fact] public static void TestReadObstructionStiffnessIntegrals() {
            var integrals = ReadObstructionStiffnessIntegrals("../../../../ChannelFlow/Input/obstructionStiffnessIntegrals.txt");
            Assert.True(integrals[22][9][11][0][4][4] < 0.2468967080739);
        }

        /// <summary>Test reading of Mathematica integration results from file into array.</summary>
        [Fact] public static void TestReadObstructionForcingIntegrals() {
            var integrals = ReadObstructionForcingIntegrals("../../../../ChannelFlow/Input/obstructionForcingIntegrals.txt");
            Assert.True(integrals[22][9][11][4] < -2.82355470480E-6);
        }

        /// <summary>Reads integrals produced by right block.</summary>
        [Fact] public void TestReadRectStiffnessIntegrals() {
            var integrals = ReadRectStiffnessIntegrals("../../../../ChannelFlow/Input/rectElementStiffnessIntegrals.txt");
            Assert.True(integrals[11][0][0][0] < 0.0000317461);
        }

        /// <summary>Reads integrals produced by right block.</summary>
        [Fact] public void TestReadRectForcingIntegrals() {
            var integrals = ReadRectForcingIntegrals("../../../../ChannelFlow/Input/rectElementForcingIntegrals.txt");
            Assert.True(integrals[11][4] < -4.4072183035E-21);
        }

        /// <summary>Test wether column swapping in SparseMatrix functions correctly.</summary>
        [InlineData(4,4, 1.0, 2.0, 3.0, 4.0,
                         5.0, 6.0, 7.0, 8.0,
                         8.0, 7.0, 6.0, 5.0,
                         4.0, 3.0, 2.0, 1.0)]
        [Theory] public void SparseMatrixColumnSwaps(int width, int height, params double[] matrix) {
            var inputMatrix = new SparseMatrix<double>(width, height, 4);

            for(int i = 0; i < width; ++i) {
                for(int j = 0; j < height; ++j) {
                    inputMatrix[i][j] = matrix[width*i + j];
                }
            }
            var transformedMatrix = new SparseMatrix<double>(inputMatrix);

            for(int i = 0; i < width/2; ++i) {
                transformedMatrix.SwapColumns(i, width - 1 - i);
            }
            Assert.All(transformedMatrix, row => {

                for(int i = 0; i < width/2; ++i) {
                    row[i].Equals(row[width - 1 - i]);
                }
            });
        }

        [InlineData(6,  1.0, 2.0, 3.0, 4.0, 5.0, 6.0)]
        [Theory] public void SparseRowElementSwaps(int width, params double[] vector) {
            var inputVector = new SparseRow<double>(width, 6);

            for(int i = 0; i < width; ++i) {
                inputVector[i] = vector[i];
            }
            double el1 = inputVector[2];
            double el2 = inputVector[4];
            inputVector.SwapElementsExplicit(2,4);
            double el3 = inputVector[4];
            double el4 = inputVector[2];
            Assert.True(el1 == el3);
            Assert.True(el2 == el4);
        }

        [InlineData(-0.5, 0.5)]
        [InlineData(0.5, 0.5)]
        [InlineData(0.0, 0.5)]
        [InlineData(0.0, -0.5)]
        [Theory] public void PointInsideQuadrilateral(double x, double y) {
            var point = new Pos(x,y);
            var vertices = new Pos[4] {                 // Define vertices of  quadrilateral.
                new Pos(-1.0, -1.0),
                new Pos(1.0, -1.0),
                new Pos(2.0, 1.0),
                new Pos(-2.0, 1.0)
            };                             

            Assert.True(point.IsInsidePolygon(ref vertices));
        }

        [InlineData(-2.0, 0.0)] [InlineData(2.0, 0.0)] [InlineData(0.0, 1.5)] [InlineData(0.0, -1.5)]
        [Theory] public void PointOutsideQuadrilateral(double x, double y) {
            var point = new Pos(x,y);
            var vertices = new Pos[4] {                 // Define vertices of  quadrilateral.
                new Pos(-1.0, -1.0),
                new Pos(1.0, -1.0),
                new Pos(2.0, 1.0),
                new Pos(-2.0, 1.0)
            };     

            Assert.True(!point.IsInsidePolygon(ref vertices));
        }

        [InlineData(1, 0)] [InlineData(1.1, 0.2)] [InlineData(3.9, 0.99)] [InlineData(1.01, 0.99)]
        [InlineData(2.5, 0.5)] [InlineData(3.1, 0.66)] [InlineData(1.00000001, 0.9999999)] [InlineData(2, 0.1)]
        [Theory] public void TestPointInsidePolygon1(double x, double y) {
            var point = new Pos(x,y);
            var vertices = new Pos[4] {
                new Pos(1,0),
                new Pos(4,0),
                new Pos(4,1),
                new Pos(1,1)
            };

            Assert.True(point.IsInsidePolygon(ref vertices));
        }
        // TODO: Test IsInsidePolygon method.

        [InlineData(0.35, 0.0)]
        [Theory] public void TestPointOutsidePolygon1(double x, double y) {
            var point = new Pos(x,y);
            var vertices = new Pos[4] {
                new Pos(1,0),
                new Pos(4,0),
                new Pos(4,1),
                new Pos(1,1)
            };

            Assert.False(point.IsInsidePolygon(ref vertices));
        }
    }
}