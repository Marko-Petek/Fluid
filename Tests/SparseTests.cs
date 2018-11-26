using System;
using Xunit;
using Fluid.Internals.Collections;
using Fluid.Internals.Numerics;

namespace Fluid.Tests
{
    public class SparseTests
    {
        /// <summary>Test addition of two sparse rows.</summary>
        [Fact] public void AddTwoSparseRows()
        {
            var sparseRow = new SparseRow<double>(3);
            sparseRow[0] = 1.0;
            sparseRow[1] = 3.0;
            sparseRow[2] = 2.0;

            var sparseRow2 = new SparseRow<double>(3);
            sparseRow2[0] = 2.0;
            sparseRow2[1] = 3.0;
            sparseRow2[2] = 1.0;

            var resultSparseRow = sparseRow + sparseRow2;
            var expectedSparseRow = new SparseRow<double>(3,3) {    // Expected: 3.0, 6.0, 3.0
                new SparseElement<double>(0, 3.0),
                new SparseElement<double>(1, 6.0),
                new SparseElement<double>(2, 3.0)
            };
            Assert.True(resultSparseRow.Equals(expectedSparseRow));
        }

        /// <summary>Dot (inner product) two sparse rows.</summary>
        [Fact] public void DotTwoSparseRows() {
            var sparseRow = new SparseRow<double>(3);
            sparseRow[0] = 2.0;
            sparseRow[1] = 1.0;
            sparseRow[2] = 3.0;

            var sparseRow2 = new SparseRow<double>(3);
            sparseRow2[0] = 5.0;
            sparseRow2[1] = 2.0;
            sparseRow2[2] = 3.0;

            var result = sparseRow * sparseRow2;
            Assert.True(result == 21.0);                  // Expected :  21
        }

        /// <summary>Dot (inner product) two sparse rows.</summary>
        [Fact] public void DotTwoSparseRows2() {
            var sparseRow = new SparseRow<double>(3);
            sparseRow[0] = 2.0;
            sparseRow[1] = 1.0;
            sparseRow[2] = 3.0;

            var sparseRow2 = new SparseRow<double>(3);
            sparseRow2[1] = 2.0;
            sparseRow2[2] = 3.0;

            var result = sparseRow * sparseRow2;
            Assert.True(result == 11.0);                 // Expected :  11
        }

        /// <summary>Add two sparse matrices.</summary>
        [Fact] public void AddTwoSparseMatrices() {
            var sparseMatrix = new SparseMatrix<double>(3,3);
            sparseMatrix[0][0] = 1.0;
            sparseMatrix[0][1] = 2.0;
            sparseMatrix[0][2] = 3.0;
            sparseMatrix[1][0] = 2.0;
            sparseMatrix[1][1] = 1.0;
            sparseMatrix[1][2] = 4.0;
            sparseMatrix[2][0] = 3.0;
            sparseMatrix[2][1] = 4.0;
            sparseMatrix[2][2] = 1.0;

            var sparseMatrix2 = new SparseMatrix<double>(3,3);
            sparseMatrix2[1][0] = 5.0;
            sparseMatrix2[1][1] = 2.0;
            sparseMatrix2[1][2] = 3.0;
            sparseMatrix2[2][0] = 2.0;
            sparseMatrix2[2][1] = 1.0;

            var sparseMatrix3 = sparseMatrix + sparseMatrix2;
            var expectedMatrix = new SparseMatrix<double>(3,3,3);

            expectedMatrix[0][0] = 1.0; expectedMatrix[0][1] = 2.0; expectedMatrix[0][2] = 3.0;
            expectedMatrix[1][0] = 7.0; expectedMatrix[1][1] = 3.0; expectedMatrix[1][2] = 7.0;
            expectedMatrix[2][0] = 5.0; expectedMatrix[2][1] = 5.0; expectedMatrix[2][2] = 1.0;
            Assert.True(sparseMatrix3.Equals(expectedMatrix));

            // Expected:
            // 1 2 3
            // 7 3 7
            // 5 5 1
        }

        /// <summary>Multiply a sparse matrix with a sparse row.</summary>
        [Fact] public void MultiplySparseMatrixAndSparseRow() {
            var sparseMatrix = new SparseMatrix<double>(3,3);
            sparseMatrix[0][0] = 1.0;
            sparseMatrix[0][1] = 2.0;
            sparseMatrix[0][2] = 3.0;
            sparseMatrix[1][0] = 2.0;
            sparseMatrix[1][1] = 1.0;
            sparseMatrix[1][2] = 4.0;
            sparseMatrix[2][0] = 3.0;
            sparseMatrix[2][1] = 4.0;
            sparseMatrix[2][2] = 1.0;

            var sparseMatrix2 = new SparseMatrix<double>(3,3);
            sparseMatrix2[1][0] = 5.0;
            sparseMatrix2[1][1] = 2.0;
            sparseMatrix2[1][2] = 3.0;
            sparseMatrix2[2][0] = 2.0;
            sparseMatrix2[2][1] = 1.0;

            var sparseRow = new SparseRow<double>(3);
            sparseRow[0] = 2.0;
            sparseRow[1] = 1.0;
            sparseRow[2] = 3.0;

            var sparseRow2 = new SparseRow<double>(3);
            sparseRow2[0] = 5.0;
            sparseRow2[1] = 2.0;
            sparseRow2[2] = 3.0;

            var resultSparseRow = sparseMatrix * sparseRow;
            var expectedSparseRow = new SparseRow<double>(3) {
                new SparseElement<double>(0, 13.0), new SparseElement<double>(1, 17.0), new SparseElement<double>(2, 13.0)
            };
            Assert.True(resultSparseRow.Equals(expectedSparseRow));             // Expected: {0: 13}, {1: 17}, {2: 13}

            var resultSparseRow2 = sparseMatrix2 * sparseRow2;
            var expectedSparseRow2 = new SparseRow<double>(3) {
                new SparseElement<double>(1, 38.0), new SparseElement<double>(2, 12.0)
            };
            Assert.True(resultSparseRow2.Equals(expectedSparseRow2));           // Expected: {1: 38}, {2: 12}

            var resultSparseRow3 = sparseRow * sparseMatrix;
            var expectedSparseRow3 = new SparseRow<double>(3) {
                new SparseElement<double>(0, 13.0), new SparseElement<double>(1, 17.0), new SparseElement<double>(2, 13.0)
            };
            Assert.True(resultSparseRow3.Equals(expectedSparseRow3));           // Expected: {0: 13}, {1: 17}, {2: 13}
        }


    }
}
