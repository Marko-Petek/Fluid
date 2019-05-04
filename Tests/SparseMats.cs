using System;
using System.Linq;
using System.Threading;
using Xunit;
using Fluid.Internals.Collections;
using Fluid.Internals.Numerics;
using SCG = System.Collections.Generic;
using TB = Fluid.Internals.Toolbox;

namespace Fluid.Tests {
   using Tensor2 = Tensor2<double,DblArithmetic>;
   using SparseRow = SparseRow<double,DblArithmetic>;
   using SparseMatInt = Tensor2<int,IntArithmetic>;
   using SparseRowInt = SparseRow<int,IntArithmetic>;
   
   public partial class Thread3 {//TODO: Log timing of methods for a large number of operations and save results.
      static Thread3() {
         TB.EntryPointSetup("Starting Thread3 tests.", () => Thread.Sleep(200));
      }
      
      /// <summary>Test addition of two sparse rows.</summary>
      [InlineData(1, 3, 2,   2, 3, 1,  3, 6, 3)]
      [Theory] public void AddTwoRows(params int[] data) {
         var row1 = SparseRowInt.CreateFromArray(data, 0, 3);
         var row2 = SparseRowInt.CreateFromArray(data, 3, 3);
         var res = row1 + row2;
         var expRes = SparseRowInt.CreateFromArray(data, 6, 3);
         Assert.True(res.Equals(expRes));
      }
      /// <summary>Test subtraction of two sparse rows.</summary>
      [InlineData(1, 3, 2,   2, 3, 1,  -1, 0, 1)]
      [Theory] public void SubTwoRows(params int[] data) {
         var row1 = SparseRowInt.CreateFromArray(data, 0, 3);
         var row2 = SparseRowInt.CreateFromArray(data, 3, 3);
         var res = row1 - row2;
         var expRes = SparseRowInt.CreateFromArray(data, 6, 3);
         Assert.True(res.Equals(expRes));
      }
      /// <summary>Dot (inner product) two sparse rows.</summary>
      [InlineData(2,1,3,  5,2,3,  21)]
      [InlineData(2,1,3, 0,2,3,  11)]
      [InlineData(9,8,1,  2,6,7,  73)]
      [Theory] public void DotTwoRows(params int[] data) {
         var row1 = SparseRowInt.CreateFromArray(data, 0, 3);
         var row2 = SparseRowInt.CreateFromArray(data, 3, 3);
         var expRes = data[6];
         var res = row1 * row2;
         Assert.True(res == expRes);
      }
      /// <summary>Add two sparse matrices.</summary>
      [InlineData(
         1, 2, 3,
         2, 1, 4,
         3, 4, 1,

         0, 0, 0,
         5, 2, 3,
         2, 1, 0,

         1, 2, 3,
         7, 3, 7,
         5, 5, 1
      )]
      [Theory] public void AddTwoMats(params int[] data) {
         var mat1 = SparseMatInt.CreateFromArray(data, 9, 0, 3, 0, 3);
         var mat2 = SparseMatInt.CreateFromArray(data, 9, 3, 3, 0, 3);
         var mat3 = mat1 + mat2;
         var expMat = SparseMatInt.CreateFromArray(data, 9, 6, 3, 0, 3);
         Assert.True(mat3.Equals(expMat));
      }
      /// <summary>Multiply a sparse matrix with a sparse row.</summary>
      [InlineData(
         1, 2, 3,
         2, 1, 4,
         3, 4, 1,

         2, 1, 3,   13, 17, 13
      )]
      [InlineData(
         0, 0, 0,
         5, 2, 3,
         2, 1, 0,

         5, 2, 3,  0, 38, 12 )]
      [Theory] public void MulMatRow(params int[] data) {
         var mat = SparseMatInt.CreateFromArray(data, 5, 0, 3, 0, 3);
         var row = SparseRowInt.CreateFromArray(data, 9, 3);
         var expRes = SparseRowInt.CreateFromArray(data, 12, 3);
         var res = mat * row;
         Assert.True(res.Equals(expRes));
      }

      [InlineData(
         3,1,9,4,
         8,5,3,2,

         9,7,3,1,  65,118 )]
      [InlineData(
      3,1,9,4,
      8,5,3,2,

      9,0,3,1,  58,83 )]
      [InlineData(
      3,1,9,0,
      8,0,3,2,

      9,7,3,1,  61,83 )]
      [InlineData(
      3,1,9,0,
      8,0,3,2,

      9,7,0,1,  34,74 )]
      [Theory] public void MulMatRowAsym(params int[] data) {
         var span = new Span<int>(data, 0, 8);
         var mat = SparseMatInt.CreateFromSpan(span, 2);
         var row = SparseRowInt.CreateFromArray(data, 8, 4);
         var expRes = SparseRowInt.CreateFromArray(data, 12, 2);
         var res = mat * row;
         Assert.True(res.Equals(expRes));
      }

      [InlineData(
         1, 2, 3,
         2, 1, 4,
         3, 4, 1,

         2, 1, 3,   13, 17, 13 )]
      [InlineData(
         2, 6, 1,
         3, 9, 4,
         7, 1, 6,

         3, 1, 5,   44, 32, 37 )]
      [Theory] public void MulRowMat(params int[] data) {
         var mat = SparseMatInt.CreateFromArray(data, 5, 0, 3, 0, 3);
         var row = SparseRowInt.CreateFromArray(data, 9, 3);
         var expRes = SparseRowInt.CreateFromArray(data, 12, 3);
         var res = row * mat;
         Assert.True(res.Equals(expRes));
      }

      [InlineData(
         5,3,0,2,  6,
         30,18,0,12 )]
      [Theory] public void MulNumRow(params int[] data) {
         var num = data[4];
         var row = SparseRowInt.CreateFromArray(data, 0, 4);
         var res = num*row;
         var expRes = SparseRowInt.CreateFromArray(data,5,4);
         Assert.True(res.Equals(expRes));
      }

      // [InlineData(
      //    2,6,0,
      //    3,7,1,
      //    6,0,4,
      //    3,
      //    6,18,0,
      //    9,21,3,
      //    18,0,12
      // )]
      // [Theory] public void MulNumMat(params int[] data) {
      //    var slice = new Span<int>(data, 0, 9);
      //    var mat = SparseMatInt.CreateFromSpan(slice, 3);
      //    var num = data[9];
      //    var res = num*mat;
      //    var expRes = 
      // }

      /// <summary>Test wether column swapping in SparseMatrix functions correctly.</summary>
      [InlineData(1.0, 2.0, 3.0, 4.0,
                  5.0, 6.0, 7.0, 8.0,
                  8.0, 7.0, 6.0, 5.0,
                  4.0, 3.0, 2.0, 1.0)]
      [Theory] public void MatColSwaps(params double[] arr) {
         int nCols = 4;
         var sparseMat = Tensor2.CreateFromArray(arr, 4, 0, 4, 0, 4, 4, 4);
         var transformedMatrix = new Tensor2(sparseMat);
         for(int i = 0; i < nCols/2; ++i)
            transformedMatrix.SwapCols(i, nCols - 1 - i);                     // Swap each left half column with its counterpart on right side.
         Assert.All(transformedMatrix, rowPair => {
            for(int i = 0; i < nCols/2; ++i)
               rowPair.Value[i].Equals(rowPair.Value[nCols - 1 - i]); });     // Therefore this has to be true after swaps.
      }

      [InlineData(1.0, 2.0, 3.0, 4.0, 5.0, 6.0)]
      [Theory] public void RowEmtSwaps(params double[] vector) {
         var sparseRow = SparseRow.CreateFromArray(vector, 0, 6, 0, 6);
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
         var sparseMat = Tensor2.CreateFromSpan(slice1, 4);
         var exdResult = Tensor2.CreateFromSpan(slice2, 4);
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
         var expRes1 = SparseMatInt.CreateFromArray(data, 4, 0, 4, 0, 2, 2, 4);
         var expRes2 = SparseMatInt.CreateFromArray(data, 4, 0, 4, 2, 2, 2, 4);
         var res1 = SparseMatInt.CreateFromArray(data, 4, 0, 4, 0, 4, 4, 4);
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

      [InlineData(1,2,3,4,5,6, 4,5,6,7,8,9)]
      [Theory] public void MergeWith(params int[] bothRows) {
         var row1 = SparseRowInt.CreateFromArray(bothRows, 0, 6, 0);
         var row2 = SparseRowInt.CreateFromArray(bothRows, 6, 6, 6);
         row1.MergeWith(row2);
         var expRes = SparseRowInt.CreateFromArray(bothRows, 0, 12, 0);
         Assert.True(row1.Equals(expRes));
      }

      [InlineData(
         1.01, 2.63, 3.21,
         5.56, 7.62, 5.45,
         6.50, 2.23, 7.66,

         1.02, 2.64, 3.21,
         5.55, 7.61, 5.44,
         6.51, 2.22, 7.65 )]
      [Theory] public void MatEquals(params double[] twoMats) {
         var mat1 = Tensor2.CreateFromArray(twoMats, 6, 0, 3, 0, 3);
         var tempMat2 = MatOps.CreateFromArray(twoMats, 6, 3, 3, 0, 3);
         var mat2 = Tensor2.CreateFromArray(tempMat2);
         Assert.True(mat1.Equals(mat2, 0.02));
      }
   }
}