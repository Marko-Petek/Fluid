using System;
using System.Linq;
using System.Threading;
using Xunit;
using Fluid.Internals.Collections;
using Fluid.Internals.Numerics;
using SCG = System.Collections.Generic;
using TB = Fluid.Internals.Toolbox;

namespace Fluid.Tests {
   using Tensor = Tensor<double,DblArithmetic>;
   using Vector = Vector<double,DblArithmetic>;
   using TensorInt = Tensor<int,IntArithmetic>;
   using VectorInt = Vector<int,IntArithmetic>;
   using IA = IntArithmetic;
   
   public partial class Thread3 {      //TODO: Log timing of methods for a large number of operations and save results.
      static Thread3() {
         TB.EntryPointSetup("Starting Thread3 tests.");
      }
      /// <summary>Test Tensor.Copy method.</summary>
      /// <param name="data"></param>
      [InlineData(1,7,                    // Rank 1 tensor.
         6,5,3,8,0,1,4)]
      [InlineData(2,3,3,                  // Rank 2 tensor.
         6,5,3,8,0,1,4,3,1)]
      [InlineData(3,3,2,3,                // Rank 3 tensor.
         6,5,3,8,0,1,4,3,1,6,0,9,1,3,7,2,7,7)]
      [InlineData(4,2,2,3,3,                // Rank 3 tensor.
         6,5,3,8,0,1,4,3,1,6,0,9,1,3,7,2,7,7,1,1,3,6,5,4,0,0,4,2,8,5,3,3,5,7,5,3)]
      [Theory] public void Copy(params int[] data) {
         int topRank = data[0];
         var struc = data.Skip(1).Take(topRank).ToArray();
         int count = 1;                                        // How many emts to take into slice.
         foreach(int emt in struc)
            count *= emt;
         var slc = new Span<int>(data, 1 + topRank, count);
         var tnr = Tensor<int,IA>.CreateFromFlatSpec(slc, struc);
         var tnrCpy = tnr.Copy(new TensorInt.CopySpecStruct(TensorInt.GeneralSpecs.Both, TensorInt.MetaSpecs.All, TensorInt.StructureSpecs.TrueCopy));
         Assert.True(tnr.Equals(tnrCpy));
      }
      
      /// <summary>Add two sparse vectors.</summary>
      [InlineData(1, 3, 2,   2, 3, 1,  3, 6, 3)]
      [Theory] public void AddTwoVecs(params int[] data) {       var vec1 = VectorInt.CreateFromArray(data, 0, 3);
         var vec2 = VectorInt.CreateFromArray(data, 3, 3);
         var res = vec1 + vec2;
         var expRes = VectorInt.CreateFromArray(data, 6, 3);
         Assert.True(res.Equals(expRes));
      }
      /// <summary>Subtract two vectors.</summary>
      [InlineData(1, 3, 2,   2, 3, 1,  -1, 0, 1)]
      [Theory] public void SubTwoVecs(params int[] data) {
         var vec1 = VectorInt.CreateFromArray(data, 0, 3);
         var vec2 = VectorInt.CreateFromArray(data, 3, 3);
         var res = vec1 - vec2;
         var expRes = VectorInt.CreateFromArray(data, 6, 3);
         Assert.True(res.Equals(expRes));
      }

      //[InlineData(
      //2, 2, 2,
      //7, 5,
      //3, 1 )]
      //[InlineData(
      //2, 3, 3,
      //7, 5, 9,
      //3, 1, 2,
      //0, 3, 4 )]
      //[InlineData(
      //2, 4, 4,
      //7, 5, 9, 0,
      //3, 1, 2, 3,
      //0, 3, 4, 8,
      //6, 5, 2, 2 )]
      //[Theory] public void ElimRank2(params int[] data) {      // Testing elimination of rank 1 from a second rank tensor.
      //   int rank = data[0];
      //   var structure = data.Skip(1).Take(rank).ToArray();
      //   var tnr = Tensor<int,IA>.CreateFromFlatSpec(new Span<int>(data, rank + 1,
      //      structure[0]*structure[1]), structure);
      //   var tnr2 = tnr.ElimRank(1, 1);
      //   var expRes = Vector<int,IA>.CreateFromSpan(new Span<int>(data, rank + 1 + structure[1], structure[1]));
      //   Assert.True(tnr2.Equals(expRes));
      //}

      [InlineData(3, 2, 2, 2,
      1,2,4,5,9,8,5,6,
      4,5,5,6)]
      [Theory] public void ElimRank3(params int[] data) {
         int rank = data[0];
         var structure = data.Skip(1).Take(rank).ToArray();
         int nEmts = structure[0]*structure[1]*structure[2];
         var tnr = Tensor<int,IA>.CreateFromFlatSpec(new Span<int>(data, rank + 1,
            nEmts), structure);
         var tnr2 = tnr.ElimRank(1, 1);
         var seq1 = data.Skip(6).Take(2);
         var seq2 = data.Skip(10).Take(2);
         var span = seq1.Concat(seq2).ToArray().AsSpan();
         var expRes = Tensor<int,IA>.CreateFromFlatSpec(span, new int[] {2,2});
         Assert.True(tnr2.Equals(expRes));
      }

      [InlineData(4, 2, 2, 2, 2,
      1,2,4,5,9,8,5,6, 3,7,2,4,8,9,0,3,
      4,5,5,6, 2,4,0,3)]
      [Theory] public void ElimRank4(params int[] data) {
         int rank = data[0];
         var structure = data.Skip(1).Take(rank).ToArray();
         int nEmts = (int) Math.Pow(2,4);
         var tnr = Tensor<int,IA>.CreateFromFlatSpec(new Span<int>(data, rank + 1,
            nEmts), structure);
         var tnr2 = tnr.ElimRank(1, 1);
         var seq1 = data.Skip(7).Take(2);
         var seq2 = data.Skip(11).Take(2);
         var seq3 = data.Skip(15).Take(2);
         var seq4 = data.Skip(19).Take(2);
         var span = seq1.Concat(seq2).Concat(seq3).Concat(seq4).ToArray().AsSpan();
         var expRes = Tensor<int,IA>.CreateFromFlatSpec(span, new int[] {2,2,2});
         Assert.True(tnr2.Equals(expRes));
      }

      [InlineData(2, 2, 2, 2,
      1,2,4,5,9,8,5,6, 3,7,2,4,8,9,0,3,
      9,8,5,6, 8,9,0,3)]
      [Theory] public void ElimRank4_2(params int[] data) {
         var structure = data.Take(4).ToArray();
         var tnr = Tensor<int,IA>.CreateFromFlatSpec(new Span<int>(data, 4, // FIXME: Creates bad Structure array for vector (as if it was not part of other tensor.)
            16), structure);
         var tnr2 = tnr.ElimRank(2, 1);
         var span = data.Skip(4 + 16).Take(8).ToArray().AsSpan();
         var expRes = Tensor<int,IA>.CreateFromFlatSpec(span, new int[] {2,2,2});
         Assert.True(tnr2.Equals(expRes));
      }

      [InlineData(1,3, 1,3,            // Specs: Rank, dims, rank, dims
         1,5,6, 6,5,2,                 // Operands
         6,5,2, 30,25,10, 36,30,12)]   // Expected result.
      [InlineData(2,2,2, 2,2,2,
         4,3, 1,6,   8,4, 2,5,
         32,16,8,20, 24,12,6,15, 8,4,2,5, 48,24,12,30
         )]
      [InlineData(1,3, 2,3,2,
         6,2,1,
         2,7, 8,8, 1,4,
         12,42, 48,48, 6,24,   4,14, 16,16, 2,8,   2,7, 8,8, 1,4)]
      [InlineData(3,2,2,3, 1,3,
         5,5,3, 2,7,1,  6,5,0, 0,7,5,
         1,2,4,
         5,10,20, 5,10,20, 3,6,12,  2,4,8, 7,14,28, 1,2,4,  6,12,24, 5,10,20, 0,0,0,  0,0,0, 7,14,28, 5,10,20)]
      [Theory] public void TensorProduct(params int[] data) {
         int pos = 0;
         var rank1 = data[pos];
         pos += 1;
         var structure1 = data.Skip(pos).Take(rank1).ToArray();
         pos += rank1;
         var rank2 = data[pos];
         pos += 1;
         var structure2 = data.Skip(pos).Take(rank2).ToArray();
         pos += rank2;
         int count1 = 1;
         foreach(int val in structure1)
            count1 *= val;
         int count2 = 1;
         foreach(int val in structure2)
            count2 *= val;
         var span1 = data.Skip(pos).Take(count1).ToArray().AsSpan();
         pos += count1;
         var operand1 = Tensor<int,IA>.CreateFromFlatSpec(span1, structure1);
         var span2 = data.Skip(pos).Take(count2).ToArray().AsSpan();
         pos += count2;
         var operand2 = Tensor<int,IA>.CreateFromFlatSpec(span2, structure2);
         var res = operand1.TnrProduct(operand2);
         var count = count1 * count2;
         var span3 = data.Skip(pos).Take(count).ToArray().AsSpan();
         var structure3 = structure1.Concat(structure2).ToArray();
         var expRes = Tensor<int,IA>.CreateFromFlatSpec(span3, structure3);
         Assert.True(res.Equals(expRes));
      }

      // TODO: Implement dot two vecs.
      ///// <summary>Dot (inner product) two vectors.</summary>
      //[InlineData(2, 1, 3, 5, 2, 3, 21)]
      //[InlineData(2, 1, 3, 0, 2, 3, 11)]
      //[InlineData(9, 8, 1, 2, 6, 7, 73)]
      //[Theory]
      //public void DotTwoVecs(params int[] data) {  // TODO: Reimplement.
      //   var vec1 = VectorInt.CreateFromArray(data, 0, 3);
      //   var vec2 = VectorInt.CreateFromArray(data, 3, 3);
      //   var expRes = data[6];
      //   var res = vec1 * vec2;
      //   Assert.True(res == expRes);
      //} 

      /// <summary>Add two 2nd rank tensors.</summary>
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
      [Theory] public void AddTwoTnrs(params int[] data) {
         var tnr1 = TensorInt.CreateFromArray(data, 9, 0, 3, 0, 3);
         var tnr2 = TensorInt.CreateFromArray(data, 9, 3, 3, 0, 3);
         var tnr3 = tnr1 + tnr2;
         var expMat = TensorInt.CreateFromArray(data, 9, 6, 3, 0, 3);
         Assert.True(tnr3.Equals(expMat));
      }
      ///// <summary>Dot a 2nd rank tensor with a vector.</summary>
      //[InlineData(
      //   1, 2, 3,
      //   2, 1, 4,
      //   3, 4, 1,

      //   2, 1, 3,   13, 17, 13
      //)]
      //[InlineData(
      //   0, 0, 0,
      //   5, 2, 3,
      //   2, 1, 0,

      //   5, 2, 3,  0, 38, 12 )]
      //[Theory] public void TnrDotVec(params int[] data) {
      //   var tnr = TensorInt.CreateFromArray(data, 5, 0, 3, 0, 3);
      //   var vec = VectorInt.CreateFromArray(data, 9, 3);
      //   var expRes = VectorInt.CreateFromArray(data, 12, 3);
      //   var res = tnr*vec;
      //   Assert.True(res.Equals(expRes));
      //}

      //[InlineData(
      //   3,1,9,4,
      //   8,5,3,2,

      //   9,7,3,1,  65,118 )]
      //[InlineData(
      //   3,1,9,4,
      //   8,5,3,2,

      //   9,0,3,1,  58,83 )]
      //[InlineData(
      //   3,1,9,0,
      //   8,0,3,2,

      //   9,7,3,1,  61,83 )]
      //[InlineData(
      //   3,1,9,0,
      //   8,0,3,2,

      //   9,7,0,1,  34,74 )]
      //[Theory] public void TnrDotVecAsym(params int[] data) {
      //   var span = new Span<int>(data, 0, 8);
      //   var tnr = TensorInt.CreateFromSpan(span, 2);
      //   var vec = VectorInt.CreateFromArray(data, 8, 4);
      //   var expRes = VectorInt.CreateFromArray(data, 12, 2);
      //   var res = tnr*vec;
      //   Assert.True(res.Equals(expRes));
      //}

      //[InlineData(
      //   1, 2, 3,
      //   2, 1, 4,
      //   3, 4, 1,

      //   2, 1, 3,   13, 17, 13 )]
      //[InlineData(
      //   2, 6, 1,
      //   3, 9, 4,
      //   7, 1, 6,

      //   3, 1, 5,   44, 32, 37 )]
      //[Theory] public void VecDotTnr(params int[] data) {
      //   var tnr = TensorInt.CreateFromArray(data, 5, 0, 3, 0, 3);
      //   var vec = VectorInt.CreateFromArray(data, 9, 3);
      //   var expRes = VectorInt.CreateFromArray(data, 12, 3);
      //   var res = vec*tnr;
      //   Assert.True(res.Equals(expRes));
      //}

      // TODO: Implement num times vec.
      //[InlineData(
      //   5,3,0,2,  6,
      //   30,18,0,12 )]
      //[Theory] public void NumTimesVec(params int[] data) {
      //   var num = data[4];
      //   var vec = VectorInt.CreateFromArray(data, 0, 4);
      //   var res = num*vec;
      //   var expRes = VectorInt.CreateFromArray(data,5,4);
      //   Assert.True(res.Equals(expRes));
      //}

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

      ///// <summary>Test wether column swapping in SparseMatrix functions correctly.</summary>
      //[InlineData(1.0, 2.0, 3.0, 4.0,
      //            5.0, 6.0, 7.0, 8.0,
      //            8.0, 7.0, 6.0, 5.0,
      //            4.0, 3.0, 2.0, 1.0)]
      //[Theory] public void MatColSwaps(params double[] arr) {
      //   int nCols = 4;
      //   var sparseMat = Tensor2.CreateFromArray(arr, 4, 0, 4, 0, 4, 4, 4);
      //   var transformedMatrix = new Tensor2(sparseMat);
      //   for(int i = 0; i < nCols/2; ++i)
      //      transformedMatrix.SwapCols(i, nCols - 1 - i);                     // Swap each left half column with its counterpart on right side.
      //   Assert.All(transformedMatrix, rowPair => {
      //      for(int i = 0; i < nCols/2; ++i)
      //         rowPair.Value[i].Equals(rowPair.Value[nCols - 1 - i]); });     // Therefore this has to be true after swaps.
      //}

      //[InlineData(1.0, 2.0, 3.0, 4.0, 5.0, 6.0)]
      //[Theory] public void RowEmtSwaps(params double[] vector) {
      //   var sparseRow = SparseRow.CreateFromArray(vector, 0, 6, 0, 6);
      //   double el1 = sparseRow[2];
      //   double el2 = sparseRow[4];
      //   sparseRow.SwapElms(2,4);
      //   double el3 = sparseRow[4];
      //   double el4 = sparseRow[2];
      //   Assert.True(el1 == el3);
      //   Assert.True(el2 == el4);
      //}

      //[InlineData(
      //   5.0,2.0,8.0,4.0,
      //   9.0,3.0,4.0,2.0,
      //   0.0,4.0,3.0,7.0,
      //   1.0,8.0,5.0,3.0,
         
      //   9.0,3.0,4.0,2.0,
      //   5.0,2.0,8.0,4.0,
      //   1.0,8.0,5.0,3.0,
      //   0.0,4.0,3.0,7.0)]
      //[Theory] public void MatRowSwaps(params double[] data) {
      //   var slice1 = new Span<double>(data, 0, 16);
      //   var slice2 = new Span<double>(data, 16, 16);
      //   var sparseMat = Tensor2.CreateFromSpan(slice1, 4);
      //   var exdResult = Tensor2.CreateFromSpan(slice2, 4);
      //   sparseMat.SwapRows(0, 1);
      //   sparseMat.SwapRows(2, 3);
      //   Assert.True(sparseMat.Equals(exdResult));
      //}

      //[InlineData(
      //   5,2,8,4,
      //   9,3,4,2,
      //   0,4,3,7,
      //   1,8,5,3 )]
      //[Theory] public void MatColSplit(params int[] data) {
      //   var expRes1 = SparseMatInt.CreateFromArray(data, 4, 0, 4, 0, 2, 2, 4);
      //   var expRes2 = SparseMatInt.CreateFromArray(data, 4, 0, 4, 2, 2, 2, 4);
      //   var res1 = SparseMatInt.CreateFromArray(data, 4, 0, 4, 0, 4, 4, 4);
      //   var res2 = res1.SplitAtCol(2);
      //   Assert.True(res1.Equals(expRes1) && res2.Equals(expRes2));
      //}

      //[InlineData( 5,2,8,4, 7,4,8,2, 1,3,0,5, 0,4,2,4 )]
      //[Theory] public void RowSplit(params int[] data) {
      //   var expRes1 = SparseRowInt.CreateFromArray(data, 0, 8);
      //   var expRes2 = SparseRowInt.CreateFromArray(data, 8, 8);
      //   var res1 = SparseRowInt.CreateFromArray(data, 0, 16);
      //   var res2 = res1.SplitAt(8);
      //   Assert.True(expRes1.Equals(res1) && expRes2.Equals(res2));
      //}

      //[InlineData(
      //   5,2,8,4,
      //   9,3,4,2,
      //   0,4,3,7,
      //   1,8,5,3 )]
      //[Theory] public void MatRowSplit(params int[] data) {
      //   var expRes1 = SparseMatInt.CreateFromArray(data, 4, 0, 2, 0, 4);
      //   var expRes2 = SparseMatInt.CreateFromArray(data, 4, 2, 2, 0, 4);
      //   var res1 = SparseMatInt.CreateFromArray(data, 4, 0, 4, 0, 4);
      //   var res2 = res1.SplitAtRow(2);
      //   Assert.True(expRes1.Equals(res1) && expRes2.Equals(res2));
      //}

      //[InlineData(
      //   5,2,8,4,
      //   9,3,4,2,
      //   0,4,3,7,
      //   1,8,5,3,
         
      //   4,2,8,5,
      //   2,3,4,9,
      //   7,4,3,0,
      //   3,8,5,1)]
      //[Theory] public void ApplyColSwaps(params int[] data) {
      //   var mat = SparseMatInt.CreateFromArray(data, 8, 0, 4, 0, 4);
      //   var tempRes = MatOps.CreateFromArray(data, 8, 4, 4, 0, 4);
      //   var expRes = SparseMatInt.CreateFromArray(tempRes);
      //   var swapDict = new SCG.Dictionary<int,int>();
      //   swapDict[0] = 3;
      //   mat.ApplyColSwaps(swapDict);
      //   Assert.True(mat.Equals(expRes));
      //}

      //[InlineData(1,2,3,4,5,6, 4,5,6,7,8,9)]
      //[Theory] public void MergeWith(params int[] bothRows) {
      //   var row1 = SparseRowInt.CreateFromArray(bothRows, 0, 6, 0);
      //   var row2 = SparseRowInt.CreateFromArray(bothRows, 6, 6, 6);
      //   row1.MergeWith(row2);
      //   var expRes = SparseRowInt.CreateFromArray(bothRows, 0, 12, 0);
      //   Assert.True(row1.Equals(expRes));
      //}

      [InlineData(
         1.01, 2.63, 3.21,
         5.56, 7.62, 5.45,
         6.50, 2.23, 7.66,

         1.02, 2.64, 3.21,
         5.55, 7.61, 5.44,
         6.51, 2.22, 7.65 )]
      [Theory] public void TnrEquals(params double[] twoMats) {
         var tnr1 = Tensor.CreateFromArray(twoMats, 6, 0, 3, 0, 3);
         var tempMat2 = MatOps.CreateFromArray(twoMats, 6, 3, 3, 0, 3);
         var mat2 = Tensor.CreateFromArray(tempMat2);
         Assert.True(tnr1.Equals(mat2, 0.02));
      }
   }
}