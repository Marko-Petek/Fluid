using System;
using Xunit;
using Fluid.Internals.Collections;
using Fluid.Internals.Numerics;

namespace Fluid.Tests {
   using SparseMat = SparseMat<double,DblArithmetic>;
   using SparseRow = SparseRow<double,DblArithmetic>;
   using SparseMatInt = SparseMat<int,IntArithmetic>;
   
   public class SparseMatsTest {
      /// <summary>Test addition of two sparse rows.</summary>
      [Fact] public void AddTwoSparseRows() {
         var sparseRow = new SparseRow(3);
         sparseRow[0] = 1.0;
         sparseRow[1] = 3.0;
         sparseRow[2] = 2.0;
         var sparseRow2 = new SparseRow(3);
         sparseRow2[0] = 2.0;
         sparseRow2[1] = 3.0;
         sparseRow2[2] = 1.0;
         var resultSparseRow = sparseRow + sparseRow2;
         var expectedSparseRow = new SparseRow(3,3) {    // Expected: 3.0, 6.0, 3.0
               {0, 3.0},
               {1, 6.0},
               {2, 3.0}
         };
         Assert.True(resultSparseRow.Equals(expectedSparseRow));
      }
      /// <summary>Dot (inner product) two sparse rows.</summary>
      [Fact] public void DotTwoSparseRows() {
         var sparseRow = new SparseRow(3);
         sparseRow[0] = 2.0;
         sparseRow[1] = 1.0;
         sparseRow[2] = 3.0;
         var sparseRow2 = new SparseRow(3);
         sparseRow2[0] = 5.0;
         sparseRow2[1] = 2.0;
         sparseRow2[2] = 3.0;
         var result = sparseRow * sparseRow2;
         Assert.True(result == 21.0);                  // Expected :  21
      }
      /// <summary>Dot (inner product) two sparse rows.</summary>
      [Fact] public void DotTwoSparseRows2() {
         var sparseRow = new SparseRow(3);
         sparseRow[0] = 2.0;
         sparseRow[1] = 1.0;
         sparseRow[2] = 3.0;
         var sparseRow2 = new SparseRow(3);
         sparseRow2[1] = 2.0;
         sparseRow2[2] = 3.0;
         var result = sparseRow * sparseRow2;
         Assert.True(result == 11.0);                 // Expected :  11
      }
      /// <summary>Add two sparse matrices.</summary>
      [Fact] public void AddTwoSparseMatrices() {
         var sparseMatrix = new SparseMat(3,3);
         sparseMatrix[0][0] = 1.0;
         sparseMatrix[0][1] = 2.0;
         sparseMatrix[0][2] = 3.0;
         sparseMatrix[1][0] = 2.0;
         sparseMatrix[1][1] = 1.0;
         sparseMatrix[1][2] = 4.0;
         sparseMatrix[2][0] = 3.0;
         sparseMatrix[2][1] = 4.0;
         sparseMatrix[2][2] = 1.0;
         var sparseMatrix2 = new SparseMat(3,3);
         sparseMatrix2[1][0] = 5.0;
         sparseMatrix2[1][1] = 2.0;
         sparseMatrix2[1][2] = 3.0;
         sparseMatrix2[2][0] = 2.0;
         sparseMatrix2[2][1] = 1.0;
         var sparseMatrix3 = sparseMatrix + sparseMatrix2;
         var expectedMatrix = new SparseMat(3,3,3);
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
         var sparseMat = new SparseMat(3,3);
         sparseMat[0][0] = 1.0;
         sparseMat[0][1] = 2.0;
         sparseMat[0][2] = 3.0;
         sparseMat[1][0] = 2.0;
         sparseMat[1][1] = 1.0;
         sparseMat[1][2] = 4.0;
         sparseMat[2][0] = 3.0;
         sparseMat[2][1] = 4.0;
         sparseMat[2][2] = 1.0;
         var sparseMatrix2 = new SparseMat(3,3);
         sparseMatrix2[1][0] = 5.0;
         sparseMatrix2[1][1] = 2.0;
         sparseMatrix2[1][2] = 3.0;
         sparseMatrix2[2][0] = 2.0;
         sparseMatrix2[2][1] = 1.0;
         var sparseRow = new SparseRow(3);
         sparseRow[0] = 2.0;
         sparseRow[1] = 1.0;
         sparseRow[2] = 3.0;
         var sparseRow2 = new SparseRow(3);
         sparseRow2[0] = 5.0;
         sparseRow2[1] = 2.0;
         sparseRow2[2] = 3.0;
         var resultSparseRow = sparseMat * sparseRow;
         var expectedSparseRow = new SparseRow(3) {
            {0, 13.0},
            {1, 17.0},
            {2, 13.0}
         };
         Assert.True(resultSparseRow.Equals(expectedSparseRow));             // Expected: {0: 13}, {1: 17}, {2: 13}
         var resultSparseRow2 = sparseMatrix2 * sparseRow2;
         var expectedSparseRow2 = new SparseRow(3) {
               {1, 38.0},
               {2, 12.0}
         };
         Assert.True(resultSparseRow2.Equals(expectedSparseRow2));           // Expected: {1: 38}, {2: 12}
         var resultSparseRow3 = sparseRow * sparseMat;
         var expectedSparseRow3 = new SparseRow(3) {
            {0, 13.0},
            {1, 17.0},
            {2, 13.0}
         };
         Assert.True(resultSparseRow3.Equals(expectedSparseRow3));           // Expected: {0: 13}, {1: 17}, {2: 13}
      }
      /// <summary>Test wether column swapping in SparseMatrix functions correctly.</summary>
      [InlineData(4,4, 1.0, 2.0, 3.0, 4.0,
                        5.0, 6.0, 7.0, 8.0,
                        8.0, 7.0, 6.0, 5.0,
                        4.0, 3.0, 2.0, 1.0)]
      [Theory] public void SparseMatColSwaps(int width, int height, params double[] matrix) {
         var inputMatrix = new SparseMat(width, height, 4);
         for(int i = 0; i < width; ++i)
            for(int j = 0; j < height; ++j)
               inputMatrix[i][j] = matrix[width*i + j];
         var transformedMatrix = new SparseMat(inputMatrix);
         for(int i = 0; i < width/2; ++i)
            transformedMatrix.SwapCols(i, width - 1 - i);                     // Swap each left half column with its counterpart on right side.
         Assert.All(transformedMatrix, rowPair => {
            for(int i = 0; i < width/2; ++i)
               rowPair.Value[i].Equals(rowPair.Value[width - 1 - i]); });     // Therefore this has to be true after swaps.
      }
      [InlineData(6,  1.0, 2.0, 3.0, 4.0, 5.0, 6.0)]
      [Theory] public void SparseRowEmtSwaps(int width, params double[] vector) {
         var inputVector = new SparseRow(width, 6);
         for(int i = 0; i < width; ++i)
               inputVector[i] = vector[i];
         double el1 = inputVector[2];
         double el2 = inputVector[4];
         inputVector.SwapElms(2,4);
         double el3 = inputVector[4];
         double el4 = inputVector[2];
         Assert.True(el1 == el3);
         Assert.True(el2 == el4);
      }
   }
}