using System;
using System.Linq;
using Xunit;
using Fluid.Internals.Collections;
using Fluid.Internals.Numerics;
using SCG = System.Collections.Generic;

namespace Fluid.Tests {
   using SparseMat = SparseMat<double,DblArithmetic>;
   using SparseRow = SparseRow<double,DblArithmetic>;
   using SparseMatInt = SparseMat<int,IntArithmetic>;
   using SparseRowInt = SparseRow<int,IntArithmetic>;
   
   public class SparseMatsTest {//TODO: Log timing of methods for a large number of operations and save results.
      /// <summary>Test addition of two sparse rows.</summary>
      [Fact] public void AddTwoRows() {
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
               {2, 3.0} };
         Assert.True(resultSparseRow.Equals(expectedSparseRow));
      }
      /// <summary>Dot (inner product) two sparse rows.</summary>
      [Fact] public void DotTwoRows() {
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
         var sparseRow3 = new SparseRow(3);
         sparseRow3[0] = 2.0;
         sparseRow3[1] = 1.0;
         sparseRow3[2] = 3.0;
         var sparseRow4 = new SparseRow(3);
         sparseRow4[1] = 2.0;
         sparseRow4[2] = 3.0;
         var result2 = sparseRow3 * sparseRow4;
         Assert.True(result2 == 11.0);                 // Expected :  11
      }
      /// <summary>Add two sparse matrices.</summary>
      [Fact] public void AddTwoMats() {
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
      [Fact] public void MulMatAndRow() {
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
            {2, 13.0} };
         Assert.True(resultSparseRow.Equals(expectedSparseRow));             // Expected: {0: 13}, {1: 17}, {2: 13}
         var resultSparseRow2 = sparseMatrix2 * sparseRow2;
         var expectedSparseRow2 = new SparseRow(3) {
               {1, 38.0},
               {2, 12.0} };
         Assert.True(resultSparseRow2.Equals(expectedSparseRow2));           // Expected: {1: 38}, {2: 12}
         var resultSparseRow3 = sparseRow * sparseMat;
         var expectedSparseRow3 = new SparseRow(3) {
            {0, 13.0},
            {1, 17.0},
            {2, 13.0} };
         Assert.True(resultSparseRow3.Equals(expectedSparseRow3));           // Expected: {0: 13}, {1: 17}, {2: 13}
      }

      /// <summary>Test wether column swapping in SparseMatrix functions correctly.</summary>
      [InlineData(1.0, 2.0, 3.0, 4.0,
                  5.0, 6.0, 7.0, 8.0,
                  8.0, 7.0, 6.0, 5.0,
                  4.0, 3.0, 2.0, 1.0)]
      [Theory] public void MatColSwaps(params double[] arr) {
         int nCols = 4;
         var sparseMat = SparseMat.CreateFromArray(arr, 4, 0, 4, 0, 4);
         var transformedMatrix = new SparseMat(sparseMat);
         for(int i = 0; i < nCols/2; ++i)
            transformedMatrix.SwapCols(i, nCols - 1 - i);                     // Swap each left half column with its counterpart on right side.
         Assert.All(transformedMatrix, rowPair => {
            for(int i = 0; i < nCols/2; ++i)
               rowPair.Value[i].Equals(rowPair.Value[nCols - 1 - i]); });     // Therefore this has to be true after swaps.
      }

      [InlineData(1.0, 2.0, 3.0, 4.0, 5.0, 6.0)]
      [Theory] public void RowEmtSwaps(params double[] vector) {
         var sparseRow = SparseRow.CreateFromArray(vector, 0, 6);
         double el1 = sparseRow[2];
         double el2 = sparseRow[4];
         sparseRow.SwapElms(2,4);
         double el3 = sparseRow[4];
         double el4 = sparseRow[2];
         Assert.True(el1 == el3);
         Assert.True(el2 == el4);
      }

      [InlineData(
         5.0,2.0,8.0,4.0,
         9.0,3.0,4.0,2.0,
         0.0,4.0,3.0,7.0,
         1.0,8.0,5.0,3.0,
         
         9.0,3.0,4.0,2.0,
         5.0,2.0,8.0,4.0,
         1.0,8.0,5.0,3.0,
         0.0,4.0,3.0,7.0)]
      [Theory] public void MatRowSwaps(params double[] data) {
         var slice1 = new Span<double>(data, 0, 16);
         var slice2 = new Span<double>(data, 16, 16);
         var sparseMat = SparseMat.CreateFromSpan(slice1, 4);
         var exdResult = SparseMat.CreateFromSpan(slice2, 4);
         sparseMat.SwapRows(0, 1);
         sparseMat.SwapRows(2, 3);
         Assert.True(sparseMat.Equals(exdResult));
      }

      [InlineData(
         5,2,8,4,
         9,3,4,2,
         0,4,3,7,
         1,8,5,3 )]
      [Theory] public void MatColSplit(params int[] data) {
         var expRes1 = SparseMatInt.CreateFromArray(data, 4, 0, 4, 0, 2);
         var expRes2 = SparseMatInt.CreateFromArray(data, 4, 0, 4, 2, 2);
         var res1 = SparseMatInt.CreateFromArray(data, 4, 0, 4, 0, 4);
         var res2 = res1.SplitAtCol(2);
         Assert.True(res1.Equals(expRes1) && res2.Equals(expRes2));
      }

      [InlineData( 5,2,8,4, 7,4,8,2, 1,3,0,5, 0,4,2,4 )]
      [Theory] public void RowSplit(params int[] data) {
         var expRes1 = SparseRowInt.CreateFromArray(data, 0, 8);
         var expRes2 = SparseRowInt.CreateFromArray(data, 8, 8);
         var res1 = SparseRowInt.CreateFromArray(data, 0, 16);
         var res2 = res1.SplitAt(8);
         Assert.True(expRes1.Equals(res1) && expRes2.Equals(res2));
      }

      [InlineData(
         5,2,8,4,
         9,3,4,2,
         0,4,3,7,
         1,8,5,3 )]
      [Theory] public void MatRowSplit(params int[] data) {
         var expRes1 = SparseMatInt.CreateFromArray(data, 4, 0, 2, 0, 4);
         var expRes2 = SparseMatInt.CreateFromArray(data, 4, 2, 2, 0, 4);
         var res1 = SparseMatInt.CreateFromArray(data, 4, 0, 4, 0, 4);
         var res2 = res1.SplitAtRow(2);
         Assert.True(expRes1.Equals(res1) && expRes2.Equals(res2));
      }

      [InlineData(
         5,2,8,4,
         9,3,4,2,
         0,4,3,7,
         1,8,5,3,
         
         4,2,8,5,
         2,3,4,9,
         7,4,3,0,
         3,8,5,1)]
      [Theory] public void ApplyColSwaps(params int[] data) {
         var mat = SparseMatInt.CreateFromArray(data, 8, 0, 4, 0, 4);
         var tempRes = MatOps.CreateFromArray(data, 8, 4, 4, 0, 4);
         var expRes = SparseMatInt.CreateFromArray(tempRes);
         var swapDict = new SCG.Dictionary<int,int>();
         swapDict[0] = 3;
         mat.ApplyColSwaps(swapDict);
         Assert.True(mat.Equals(expRes));
      }
   }
}