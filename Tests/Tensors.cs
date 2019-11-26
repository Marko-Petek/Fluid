using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using Fluid.Internals.Collections;
using Fluid.Internals.Numerics;
using Fluid.Internals;
using System.Collections.Generic;
using static Fluid.Internals.Toolbox;
using Fluid.TestRef;
using static Fluid.Tests.Utils;
using dbl = System.Double;
using IA = Fluid.Internals.Numerics.IntArithmetic;
using DA = Fluid.Internals.Numerics.DblArithmetic;
namespace Fluid.Tests {
using Tnr = Tensor<double,DblArithmetic>;
using Vec = Vector<double,DblArithmetic>;
using TnrInt = Tensor<int,IntArithmetic>;
using VecInt = Vector<int,IntArithmetic>;
using VoidsInt = Voids<int,IA>;
using VoidsDbl = Voids<dbl,DA>;

public class Tensors {

   [InlineData(1,7,                    // Rank 1 tensor.
      6,5,3,8,0,1,4)]
   [InlineData(2,3,3,                  // Rank 2 tensor.
      6,5,3,8,0,1,4,3,1)]
   [InlineData(3,3,2,3,                // Rank 3 tensor.
      6,5,3,8,0,1,4,3,1,6,0,9,1,3,7,2,7,7)]
   [InlineData(4,2,2,3,3,                // Rank 4 tensor.
      6,5,3,  8,0,1,  4,3,1,   6,0,9,  1,3,7,  2,7,7,      1,1,3,  6,5,4,  0,0,4,   2,8,5,  3,3,5,  7,5,3)]
   /// <remarks><see cref="TestRefs.TensorCopy"/></remarks>
   [Theory] public void TensorCopy(params int[] data) {
      int topRank = data[0];
      var struc = data.Skip(1).Take(topRank).ToArray();
      int count = 1;                                        // How many emts to take into slice.
      foreach(int emt in struc)
         count *= emt;
      var slc = new Span<int>(data, 1 + topRank, count);
      var tnr = TnrInt.FromFlatSpec(slc, struc);
      var tnrCpy = tnr.Copy(TnrInt.CopySpecs.S342_00);
      Assert.True(tnr.Equals(tnrCpy));
   }

   [InlineData(1, 3, 2,   2, 3, 1,  3, 6, 3)]
   [InlineData(1, 0, 2,   2, 3, 1,  3, 3, 3)]
   [InlineData(1, 3, 2,   2, 0, 1,  3, 3, 3)]
   [InlineData(1, 3, 2,  -1,-3,-2,  0, 0, 0)]
   /// <remarks><see cref="TestRefs.Op_VectorAddition"/></remarks>
   [Theory] public void Op_VectorAddition(params int[] data) {
      var vec1 = VecInt.FromFlatSpec(data.AsSpan<int>(0,3));
      var vec2 = VecInt.FromFlatSpec(data.AsSpan<int>(3,3));
      var res = vec1 + vec2;
      var expRes = VecInt.FromFlatSpec(data.AsSpan<int>(6,3));
      Assert.True(res.Equals(expRes));
   }
   
   [InlineData(1, 3, 2,   2, 3, 1,  -1, 0, 1)]
   [InlineData(1, 0, 2,   2, 3, 1,  -1,-3, 1)]
   [InlineData(1, 3, 2,   1, 3, 2,   0, 0, 0)]
   /// <remarks><see cref="TestRefs.Op_VectorSubtraction"/></remarks>
   [Theory] public void Op_VectorSubtraction(params int[] data) {
      var vec1 = VecInt.FromFlatSpec(data.AsSpan<int>(0,3));
      var vec2 = VecInt.FromFlatSpec(data.AsSpan<int>(3,3));
      var res = vec1 - vec2;
      var expRes = VecInt.FromFlatSpec(data.AsSpan<int>(6,3));
      Assert.True(res.Equals(expRes));
   }
   
   [InlineData(
      5,3,2,
      7,6,9,
      0,4,2,

      3,1,0,
      4,2,8,
      7,2,3,

         8, 4, 2,
      11, 8, 17,
         7, 6, 5)]
   [InlineData(
      0,0,0,
      7,6,9,
      0,4,2,

      3,1,0,
      4,2,8,
      7,2,3,

         3, 1, 0,
      11, 8, 17,
         7, 6, 5)]
   [InlineData(
      0,0,0,
      7,6,9,
      0,4,2,

      3,1,0,
      4,2,8,
      0,0,0,

         3, 1, 0,
      11, 8, 17,
         0, 4, 2)]
   [InlineData(
      0,0,0,
      7,6,9,
      0,4,2,

      3, 1, 0,
      4, 2, 8,
      0,-4,-2,

         3, 1, 0,
      11, 8, 17,
         0, 0, 0)]
   /// <remarks><see cref="TestRefs.TensorSum"/></remarks>
   [Theory] public void TensorSum(params int[] data) {
      var span1 = new Span<int>(data, 0, 9);
      var span2 = new Span<int>(data, 9, 9);
      var span3 = new Span<int>(data, 18, 9);
      var tnr1 = TnrInt.FromFlatSpec(span1, new int[] {3,3});
      var tnr2 = TnrInt.FromFlatSpec(span2, new int[] {3,3});
      var tnr3 = TnrInt.FromFlatSpec(span3, new int[] {3,3});
      tnr1.Sum(tnr2);
      Assert.True(tnr1.Equals(tnr3));
   }
   // Test cases described besides the inline data.
   [InlineData(            // Subtract a non-zero tensor from non-zero and get a non-zero result.
      5,3,2,
      7,6,9,
      0,4,2,

      3,1,0,
      4,2,8,
      7,2,3,

         2, 2, 2,
         3, 4, 1,
      -7, 2,-1)]
   [InlineData(            // Subtract a non-zero tensor from a zero (line) tensor and get a non-zero result.
      0,0,0,
      7,6,9,
      0,4,2,

      3,1,0,
      4,2,8,
      7,2,3,

      -3,-1, 0,
         3, 4, 1,
      -7, 2,-1)]
   [InlineData(            // Subtract a zero (line) tensor from a zero tensor and get a non-zero result.
      0,0,0,
      7,6,9,
      0,4,2,

      3,1,0,
      4,2,8,
      0,0,0,

      -3,-1, 0,
         3, 4, 1,
         0, 4, 2)]
   [InlineData(            // Subtract a non-zero tensor from a zero (line) tensor and get a zero (line) result.
      0,0,0,
      4,2,8,
      0,4,2,

      3,1,0,
      4,2,8,
      0,0,0,

      -3,-1, 0,
         0, 0, 0,
         0, 4, 2)]
   [InlineData(
      1,2,3,
      0,0,0,
      7,8,9,

      1,2,3,
      0,0,0,
      7,8,9,

      0,0,0,
      0,0,0,
      0,0,0
   )]
   /// <remarks><see cref="TestRefs.TensorSub"/></remarks>
   [Theory] public void TensorSub(params int[] data) {      // TODO: Test zero result when non-zero operands. Also for operators.
      var span1 = new Span<int>(data, 0, 9);
      var span2 = new Span<int>(data, 9, 9);
      var span3 = new Span<int>(data, 18, 9);
      var tnr1 = TnrInt.FromFlatSpec(span1, new int[] {3,3});
      var tnr2 = TnrInt.FromFlatSpec(span2, new int[] {3,3});
      var tnr3 = TnrInt.FromFlatSpec(span3, new int[] {3,3});
      tnr1.Sub(tnr2);
      Assert.True(tnr1.Equals(tnr3));
   }

   [InlineData(5, 3, 2,   7, 3, 9,  12,6,11)]
   [InlineData(1, 3, 2,   2, 3, 1,  3, 6, 3)]
   [InlineData(1, 0, 2,   2, 3, 1,  3, 3, 3)]
   [InlineData(1, 3, 2,   2, 0, 1,  3, 3, 3)]
   [InlineData(1, 3, 2,   -1,-3,-2,  0, 0, 0)]
   /// <remarks><see cref="TestRefs.VectorSum"/></remarks>
   [Theory] public void VectorSum(params int[] data) {
      var vec1 = VecInt.FromFlatSpec(data.AsSpan<int>(0,3));
      var vec2 = VecInt.FromFlatSpec(data.AsSpan<int>(3,3));
      vec1.Sum(vec2);
      var expRes = VecInt.FromFlatSpec(data.AsSpan<int>(6,3));
      Assert.True(vec1.Equals(expRes));
   }
   
   [InlineData(1, 3, 2,   2, 3, 1,  -1, 0, 1)]
   [InlineData(1, 0, 2,   2, 3, 1,  -1,-3, 1)]
   [InlineData(1, 3, 2,   2, 0, 1,  -1, 3, 1)]
   [InlineData(1, 3, 2,   1, 3, 2,  0, 0, 0)]
   /// <remarks><see cref="TestRefs.VectorSub"/></remarks>
   [Theory] public void VectorSub(params int[] data) {
      var vec1 = VecInt.FromFlatSpec(data.AsSpan<int>(0,3));
      var vec2 = VecInt.FromFlatSpec(data.AsSpan<int>(3,3));
      vec1.Sub(vec2);
      var expRes = VecInt.FromFlatSpec(data.AsSpan<int>(6,3));
      Assert.True(vec1.Equals(expRes));
   }

   [InlineData(
         3, 0, 7,
         2, 1, 0,
         0, 4, 8,

      -3, 0,-7,
      -2,-1, 0,
         0,-4,-8
   )]
   /// <remarks><see cref="TestRefs.Op_TensorNegation"/></remarks>
   [Theory] public void Op_TensorNegation(params int[] data) {
      var tnr = TnrInt.FromFlatSpec(data.AsSpan<int>(0,9), 3,3);
      var expRes = TnrInt.FromFlatSpec(data.AsSpan<int>(9,9), 3,3);
      var res = -tnr;
      Assert.True(res.Equals(expRes));
   }

   [InlineData(
      2,    2, 1,
      2,    3, 3,
      9,    1, 2, 3,   2, 1, 4,   3, 4, 1,
      1,    3,
      3,    2, 1, 3,
      1,    3,
      3,    13, 17, 13
   )]
   [InlineData(
      2,    2, 1,
      2,    3, 3,
      9,    0, 0, 0,   5, 2, 3,   2, 1, 0,
      1,    3,
      3,    5, 2, 3,
      1,    3,
      3,    0, 38, 12)]
   [InlineData(
      2,    2, 2,                                                         // ctrInxs
      3,    2, 2, 2,                                                      // struc1
      8,    3,6, 2,5,  6,0, 4,7,                                          // tnr1
      3,    2, 2, 2,                                                      // struc2
      8,    4,2, 7,5,  6,2, 6,4,                                          // tnr2,
      4,    2, 2, 2, 2,                                                   // strucExpRes
      16,   26,16, 30,14,  59,37, 66,32,   52,32, 60,28,  49,35, 42,28    // expRes
   )]
   [InlineData(
      2,    3, 3,
      3,    2, 2, 2,
      8,    3,6, 2,5,  6,0, 4,7,
      3,    2, 2, 2,
      8,    4,2, 7,5,  6,2, 6,4,
      4,    2, 2, 2, 2,
      16,   24,51, 30,42,  18,39, 22,32,   24,42, 36,36,  30,63, 38,52 )]
   [InlineData(
      2,    1, 1,
      3,    2, 2, 2,
      8,    3,6, 2,5,  6,0, 4,7,
      3,    2, 2, 2,
      8,    4,2, 7,5,  6,2, 6,4,
      4,    2, 2, 2, 2,
      16,   48,18, 57,39,  24,12, 42,30,   32,12, 38,26,  62,24, 77,53 )]
   [InlineData(
      2,    2, 1,
      2,    2, 4,
      8,    3, 1, 9, 4,   8, 5, 3, 2,
      1,    4,
      4,    9, 7, 3, 1,
      1,    2,
      2,    65, 118)]
   [InlineData(
      2,    2, 1,
      2,    2, 4,
      8,    3, 1, 9, 4,   8, 5, 3, 2,
      1,    4,
      4,    9, 0, 3, 1,
      1,    2,
      2,    58, 83)]
   [InlineData(
      2,    2, 1,
      2,    2, 4,
      8,    3, 1, 9, 0,   8, 0, 3, 2,
      1,    4,
      4,    9, 7, 3, 1,
      1,    2,
      2,    61, 83)]
   [InlineData(
      2,    2, 1,
      2,    2, 4,
      8,    3, 1, 9, 0,   8, 0, 3, 2,
      1,    4,
      4,    9, 7, 0, 1,
      1,    2,
      2,    34, 74)]
   [InlineData(
      2,    1, 1,
      1,    3,
      3,    2, 1, 3,
      2,    3, 3,
      9,    1, 2, 3,  2, 1, 4,  3, 4, 1,
      1,    3,
      3,    13, 17, 13)]
   [InlineData(
      2,    1, 1,
      1,    3,
      3,    3, 1, 5,
      2,    3, 3,
      9,    2, 6, 1,  3, 9, 4,  7, 1, 6,
      1,    3,
      3,    44, 32, 37)]
   /// <remarks><see cref="TestRefs.TensorContract"/></remarks>
   [Theory] public void TensorContract(params int[] data) {
      int read = 0, pos = 0;                                      Read(data, ref pos, ref read);
      var ctrInxs = new Span<int>(data, pos, read);               Read(data, ref pos, ref read);
      var struc1 = new Span<int>(data, pos, read).ToArray();      Read(data, ref pos, ref read);
      var tnr1 = TnrInt.FromFlatSpec(
         new Span<int>(data, pos, read), struc1);                 Read(data, ref pos, ref read);
      var struc2 = new Span<int>(data, pos, read).ToArray();      Read(data, ref pos, ref read);
      var tnr2 = TnrInt.FromFlatSpec(
         new Span<int>(data, pos, read), struc2);                 Read(data, ref pos, ref read);
      var strucExpRes = new Span<int>(data, pos, read)
         .ToArray();                                              Read(data, ref pos, ref read);
      var expRes = TnrInt.FromFlatSpec( 
         new Span<int>(data, pos, read), strucExpRes);
      var res = TnrInt.Contract(tnr1, tnr2, ctrInxs[0], ctrInxs[1]);
      Assert.True(res.Equals(expRes));
   }

   [InlineData(
      1,    1,                         // redRank
      1,    1,                         // emtInx
      2,    2, 2,                      // tnrStruc
      4,    7, 5,  3, 1,               // tnr
      1,    2,                         // expResStruc
      2,    3, 1 )]                    // expRes
   [InlineData(
      1,    1,                            // redRank
      1,    1,                            // emtInx
      2,    3, 3,                         // tnrStruc
      9,    7, 5, 9,  3, 1, 2,  0, 3, 4,  // tnr
      1,    3,                            // expResStruc
      3,    3, 1, 2 )]                    // expRes
   [InlineData(
      1,    1,                                                    // redRank
      1,    1,                                                    // emtInx
      2,    4, 4,                                                 // tnrStruc
      16,   7, 5, 9, 0,  3, 1, 2, 3,  0, 3, 4, 8,  6, 5, 2, 2,    // tnr
      1,    4,                                                    // expResStruc
      4,    3, 1, 2, 3 )]                                         // expRes
   [InlineData(
      1,    1,                                                    // redRank
      1,    1,                                                    // emtInx
      3,    2, 2, 2,
      8,    1,2, 4,5,  9,8, 5,6,
      2,    2, 2,
      4,    4,5, 5,6)]
   [InlineData(
      1,    1,                                                    // redRank
      1,    1,                                                    // emtInx
      4,    2, 2, 2, 2,
      16,   1,2,4,5,9,8,5,6, 3,7,2,4,8,9,0,3,
      3,    2,2,2,
      8,    4,5,5,6, 2,4,0,3)]
   [InlineData(
      1,    2,
      1,    1,
      4,    2, 2, 2, 2,
      16,   1,2,4,5,9,8,5,6, 3,7,2,4,8,9,0,3,
      3,    2, 2, 2,
      8,    9,8,5,6, 8,9,0,3)]
   /// <remarks><see cref="TestRefs.TensorReduceRank"/></remarks>
   [Theory] public void TensorReduceRank(params int[] data) {
      int read = 0, pos = 0;                                      Read(data, ref pos, ref read);
      var redRank = data[pos];                                    Read(data, ref pos, ref read);
      var emtInx = data[pos];                                     Read(data, ref pos, ref read);
      var tnrStruc = new Span<int>(data, pos, read).ToArray();    Read(data, ref pos, ref read);
      var tnr = TnrInt.FromFlatSpec(
         new Span<int>(data, pos, read), tnrStruc);               Read(data, ref pos, ref read);
      var expResStruc = new Span<int>(data, pos, read)
         .ToArray();                                              Read(data, ref pos, ref read);
      var expRes = TnrInt.FromFlatSpec(
         new Span<int>(data, pos, read), expResStruc);
      var res = tnr.ReduceRank(redRank, emtInx);
      Assert.True(res.Equals(expRes));
   }

   [InlineData(
      1,    1,                                  // slotInx1
      1,    2,                                  // natInx2
      3,    2, 2, 2,                            // tnrStruc
      8,    3,6, 2,5,  6,0, 4,7,                // tnr
      1,    2,                                  // expResStruc 
      2,    7, 13 )]
   [InlineData(
      1,    1,                                  // slotInx1
      1,    2,                                  // slotInx2
      3,    2, 2, 2,                            // tnrStruc
      8,    4,2, 7,5,  6,2, 6,4,                // tnr
      1,    2,                                  // expResStruc 
      2,    10, 6 )]
   [InlineData(
      1,    1,                                  // slotInx1
      1,    2,                                  // slotInx2
      3,    2, 2, 2,                            // tnrStruc
      8,    24,51, 30,42, 18,39, 22,32,         // tnr
      1,    2,                                  // expResStruc
      2,    46, 83 )]
   [InlineData(
      1,    1,                                  // slotInx1
      1,    2,                                  // slotInx2
      3,    2, 2, 2,                            // tnrStruc
      8,    24,42, 36,36, 30,63, 38,52,         // tnr
      1,    2,                                  // expResStruc
      2,    62,94 )]
   [InlineData(
      1,    1,                                  // slotInx1
      1,    3,                                  // slotInx2
      3,    2, 2, 2,                            // tnrStruc
      8,    3,6, 2,5,  6,0, 4,7,         // tnr
      1,    2,                                  // expResStruc
      2,    3,9 )]
   [InlineData(
      1,    1,                                  // slotInx1
      1,    3,                                  // slotInx2
      3,    2, 2, 2,                            // tnrStruc
      8,    4,2, 7,5,  6,2, 6,4,         // tnr
      1,    2,                                  // expResStruc
      2,    6,11 )]
   [InlineData(
      1,    1,                                  // slotInx1
      1,    3,                                  // slotInx2
      3,    2, 2, 2,                            // tnrStruc
      8,    24,51, 30,42, 18,39, 22,32,         // tnr
      1,    2,                                  // expResStruc
      2,    63,62 )]
   [InlineData(
      1,    1,                                  // slotInx1
      1,    3,                                  // slotInx2
      3,    2, 2, 2,                            // tnrStruc
      8,    24,42, 36,36, 30,63, 38,52,         // tnr
      1,    2,                                  // expResStruc
      2,    87,88 )]
   [InlineData(
      1,    2,                                  // slotInx1
      1,    3,                                  // slotInx2
      3,    2, 2, 2,                            // tnrStruc
      8,    3,6, 2,5,  6,0, 4,7,                // tnr
      1,    2,                                  // expResStruc
      2,    8,13 )]
   [InlineData(
      1,    2,                                  // slotInx1
      1,    3,                                  // slotInx2
      3,    2, 2, 2,                            // tnrStruc
      8,    4,2, 7,5,  6,2, 6,4,                // tnr
      1,    2,                                  // expResStruc
      2,    9,10 )]
   [InlineData(
      1,    2,                                  // slotInx1
      1,    3,                                  // slotInx2
      3,    2, 2, 2,                            // tnrStruc
      8,    24,51, 30,42, 18,39, 22,32,                // tnr
      1,    2,                                  // expResStruc
      2,    66,50 )]
   [InlineData(
      1,    2,                                  // slotInx1
      1,    3,                                  // slotInx2
      3,    2, 2, 2,                            // tnrStruc
      8,    24,42, 36,36, 30,63, 38,52,                // tnr
      1,    2,                                  // expResStruc
      2,    60,82 )]
      [InlineData(
      1,    3,                                                       // slotInx1
      1,    4,                                                       // slotInx2
      4,    2, 2, 2, 2,                                                 // tnrStruc
      16,   5,5, 3,2,  7,1, 6,5,   0,0, 7,5,  4,2, 8,9,                // tnr
      2,    2, 2,                                                       // expResStruc
      4,    7,12, 5,13 )]
   /// <remarks> <see cref="TestRefs.TensorSelfContract"/> </remarks>
   [Theory] public void TensorSelfContract(params int[] data) {
      int read = 0, pos = 0;                                         Read(data, ref pos, ref read);
      var slotInx1 = data[pos];                                      Read(data, ref pos, ref read);
      var slotInx2 = data[pos];                                      Read(data, ref pos, ref read);
      var tnrStruc = new Span<int>(data, pos, read).ToArray();       Read(data, ref pos, ref read);
      var tnr = TnrInt.FromFlatSpec(
         new Span<int>(data, pos, read), tnrStruc);                  Read(data, ref pos, ref read);
      var expResStruc = new Span<int>(data, pos, read)
         .ToArray();                                                 Read(data, ref pos, ref read);
      var expRes = TnrInt.FromFlatSpec(
         new Span<int>(data, pos, read), expResStruc);
      var res = tnr.SelfContract(slotInx1, slotInx2);
      Assert.True(res.Equals(expRes));
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
   /// <remarks> <see cref="TestRefs.TensorProduct"/> </remarks>
   [Theory] public void TensorProduct(params int[] data) {
      int pos = 0;
      var rank1 = data[pos];                                   pos += 1;
      var structure1 = data.Skip(pos).Take(rank1).ToArray();   pos += rank1;
      var rank2 = data[pos];                                   pos += 1;
      var structure2 = data.Skip(pos).Take(rank2).ToArray();   pos += rank2; int count1 = 1; foreach(int val in structure1) count1 *= val; int count2 = 1; foreach(int val in structure2) count2 *= val;
      var span1 = data.Skip(pos).Take(count1)
         .ToArray().AsSpan();                                  pos += count1;
      var operand1 = Tensor<int,IA>.FromFlatSpec(
         span1, structure1);
      var span2 = data.Skip(pos).Take(count2)
         .ToArray().AsSpan();                                  pos += count2;
      var operand2 = Tensor<int,IA>.FromFlatSpec(
         span2, structure2);
      var res = operand1.TnrProduct(operand2);
      var count = count1 * count2;
      var span3 = data.Skip(pos).Take(count).ToArray().AsSpan();
      var structure3 = structure1.Concat(structure2).ToArray();
      var expRes = Tensor<int,IA>.FromFlatSpec(span3, structure3);
      Assert.True(res.Equals(expRes));
   }
   [InlineData(
      4,    2,2,2,2,                                                 // First structure.
      16,   5,3, 7,5,  2,1, 0,9,  9,9, 3,2,  7,4, 6,8,               // First values.
      4,    2,2,2,2,                                                 // Second structure.
      16,   9,3, 2,5,  7,1, 3,6,  0,0, 2,0,  4,3, 7,9,               // Second values.
      1,    1,                                                       // Indices to reach the first subtensor to multiply.
      2,    0,1,                                                     // Indices to reach the second subtensor to multiply.
      5,    2,2,2,2,2,                                               // Expected result structure.
      //    expRes: (9,9, 3,2,  7,4, 6,8) x (7,1, 3,6)               // Below: expected result.
      32,   63,9, 27,54,  63,9, 27,54,   21,3, 9,18,  14,2, 6,12,    49,7, 21,42,  28,4, 12,24,   42,6, 18,36,  56,8, 24,48
   )]
   [Theory] public void NonTopTnrProduct(params int[] data) {
      int read = 0, pos = 0;                                                        Read(data, ref pos, ref read);
      var struc1 = new Span<int>(data, pos, read).ToArray();                        Read(data, ref pos, ref read);
      var tnr1 = TnrInt.FromFlatSpec(new Span<int>(data, pos, read), struc1);       Read(data, ref pos, ref read);
      var struc2 = new Span<int>(data, pos, read).ToArray();                        Read(data, ref pos, ref read);
      var tnr2 = TnrInt.FromFlatSpec(new Span<int>(data, pos, read), struc2);       Read(data, ref pos, ref read);
      var inxs1 = new Span<int>(data, pos, read).ToArray();                         Read(data, ref pos, ref read);
      var inxs2 = new Span<int>(data, pos, read).ToArray();                         Read(data, ref pos, ref read);
      var expStruc = new Span<int>(data, pos, read).ToArray();                      Read(data, ref pos, ref read);
      var expRes = TnrInt.FromFlatSpec(new Span<int>(data, pos, read), expStruc);
      var subTnr1 = tnr1[VoidsInt.Tnr, inxs1];
      var subTnr2 = tnr2[VoidsInt.Tnr, inxs2];
      var res = subTnr1.TnrProduct(subTnr2);
      Assert.True(res.Equals(expRes));
   }
   [InlineData(5,3,2, 7,6,9, 0,4,2,  13)]
   [InlineData(3,1,0, 4,2,8, 7,2,3,  8)]
   [InlineData(8,4,2, 11,8,17, 7,6,5,  21)]
   /// <remarks> <see cref="TestRefs.TensorSelfContractR2"/> </remarks>
   [Theory] public void SelfContractR2(params int[] data) {
      var span1 = new Span<int>(data, 0, 9);
      var tnr1 = TnrInt.FromFlatSpec(span1, 3,3);
      var res = tnr1.SelfContractR2();
      var expRes = data[9];
      Assert.True(res == expRes);
   }

   [InlineData(2, 1, 3, 5, 2, 3, 21)]
   [InlineData(2, 1, 3, 0, 2, 3, 11)]
   [InlineData(9, 8, 1, 2, 6, 7, 73)]
   /// <remarks> <see cref="TestRefs.Op_VectorDotVector"/> </remarks>
   [Theory] public void Op_VectorDotVector(params int[] data) {
      var vec1 = VecInt.FromFlatSpec(data.AsSpan<int>(0,3));
      var vec2 = VecInt.FromFlatSpec(data.AsSpan<int>(3,3));
      var expRes = data[6];
      var res = vec1 * vec2;
      Assert.True(res == expRes);
   } 

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
   [InlineData(
      0, 0, 0,
      5, 2, 3,
      2, 1, 0,

      1, 2, 3,
      2, 1, 4,
      3, 4, 1,

      1, 2, 3,
      7, 3, 7,
      5, 5, 1 )]
   /// <remarks> <see cref="TestRefs.Op_TensorAddition"/> </remarks>
   [Theory] public void Op_TensorAddition(params int[] data) {
      var tnr1 = TnrInt.FromFlatSpec(data.AsSpan<int>(0,9), 3,3);
      var tnr2 = TnrInt.FromFlatSpec(data.AsSpan<int>(9,9), 3,3);
      var tnr3 = tnr1 + tnr2;
      var expMat = TnrInt.FromFlatSpec(data.AsSpan<int>(18,9), 3,3);
      Assert.True(tnr3.Equals(expMat));
   }
   
   [InlineData(
      1, 2, 3,
      2, 1, 4,
      3, 4, 1,

      0, 0, 0,
      5, 2, 3,
      2, 1, 0,

         1, 2, 3,
      -3,-1, 1,
         1, 3, 1 )]
   [InlineData(
      0, 0, 0,
      5, 2, 3,
      2, 1, 0,
      
      1, 2, 3,
      2, 1, 4,
      3, 4, 1,

      -1,-2,-3,
         3, 1,-1,
      -1,-3,-1 )]
   // Test cases described besides the inline data.
   [InlineData(            // Subtract a non-zero tensor from non-zero and get a non-zero result.
      5,3,2,
      7,6,9,
      0,4,2,

      3,1,0,
      4,2,8,
      7,2,3,

         2, 2, 2,
         3, 4, 1,
      -7, 2,-1)]
   [InlineData(            // Subtract a non-zero tensor from a zero (line) tensor and get a non-zero result.
      0,0,0,
      7,6,9,
      0,4,2,

      3,1,0,
      4,2,8,
      7,2,3,

      -3,-1, 0,
         3, 4, 1,
      -7, 2,-1)]
   [InlineData(            // Subtract a zero (line) tensor from a zero tensor and get a non-zero result.
      0,0,0,
      7,6,9,
      0,4,2,

      3,1,0,
      4,2,8,
      0,0,0,

      -3,-1, 0,
         3, 4, 1,
         0, 4, 2)]
   [InlineData(            // Subtract a non-zero tensor from a zero (line) tensor and get a zero (line) result.
      0,0,0,
      4,2,8,
      0,4,2,

      3,1,0,
      4,2,8,
      0,0,0,

      -3,-1, 0,
         0, 0, 0,
         0, 4, 2)]
   [InlineData(
      1,2,3,
      0,0,0,
      7,8,9,

      1,2,3,
      0,0,0,
      7,8,9,

      0,0,0,
      0,0,0,
      0,0,0
   )]
   /// <remarks> <see cref="TestRefs.Op_TensorSubtraction"/> </remarks>
   [Theory] public void Op_TensorSubtraction(params int[] data) {
      var tnr1 = TnrInt.FromFlatSpec(data.AsSpan<int>(0,9), 3,3);
      var tnr2 = TnrInt.FromFlatSpec(data.AsSpan<int>(9,9), 3,3);
      var tnr3 = tnr1 - tnr2;
      var expMat = TnrInt.FromFlatSpec(data.AsSpan<int>(18,9), 3,3);
      Assert.True(tnr3.Equals(expMat));
   }

   [InlineData(
      5, 3, 0, 2, 6,
      30, 18, 0, 12)]
   /// <remarks> <see cref="TestRefs.Op_ScalarVectorMultiplication"/> </remarks>
   [Theory] public void Op_ScalarVectorMultiplication(params int[] data) {
      var num = data[4];
      var vec = VecInt.FromFlatSpec(data.AsSpan<int>(0,4));
      var res = num * vec;
      var expRes = VecInt.FromFlatSpec(data.AsSpan<int>(5,4));
      Assert.True(res.Equals(expRes));
   }

   [InlineData(
      5, 3, 0, 2, 6,
      30, 18, 0, 12)]
   /// <remarks> <see cref="TestRefs.VectorMul"/> </remarks>
   [Theory] public void VectorMul(params int[] data) {
      var num = data[4];
      var vec = VecInt.FromFlatSpec(data.AsSpan<int>(0,4));
      vec.Mul(num);
      var expRes = VecInt.FromFlatSpec(data.AsSpan<int>(5,4));
      Assert.True(vec.Equals(expRes));
   }

   [InlineData(
      2, 6, 0,
      3, 7, 1,
      6, 0, 4,
      3,
      6, 18, 0,
      9, 21, 3,
      18, 0, 12
   )]
   /// <remarks> <see cref="TestRefs.Op_ScalarTensorMultiplication"/> </remarks>
   [Theory] public void Op_ScalarTensorMultiplication(params int[] data) {
      var slc1 = new Span<int>(data, 0, 9);
      var slc2 = new Span<int>(data, 10, 9);
      var mat = TnrInt.FromFlatSpec(slc1, 3,3);
      var num = data[9];
      var res = num * mat;
      var expRes = TnrInt.FromFlatSpec(slc2,3,3);
      Assert.True(res.Equals(expRes));
   }

   [InlineData(
      2, 6, 0,
      3, 7, 1,
      6, 0, 4,
      3,
      6, 18, 0,
      9, 21, 3,
      18, 0, 12
   )]
   /// <remarks> <see cref="TestRefs.TensorMul"/> </remarks>
   [Theory] public void TensorMul(params int[] data) {
      var slc1 = new Span<int>(data, 0, 9);
      var slc2 = new Span<int>(data, 10, 9);
      var tnr = TnrInt.FromFlatSpec(slc1, 3,3);
      var num = data[9];
      tnr.Mul(num);
      var expRes = TnrInt.FromFlatSpec(slc2,3,3);
      Assert.True(tnr.Equals(expRes));
   }

   [InlineData(
      1,    2,                                     // enumRank
      3,    2,2,3,                                 // tnrStruc
      12,   5,5,3, 2,7,1,  6,5,0, 0,7,5,           // tnr
      2,    2,3,                                   // expResStruc
      6,    11,10,3, 2,14,6)]                      // expRes
   [InlineData(
      1,    2,
      4,    2, 2, 2, 2,
      16,   5,5, 3,2,  7,1, 6,5,   0,0, 7,5,  4,2, 8,9,           // R4: 2,2,2,2
      2,    2, 2,
      4,    16,8, 24,21)]                                         // R2: 2,2
   /// <remarks> <see cref="TestRefs.TensorEnumerateRank"/> </remarks>
   [Theory] public void TensorEnumerateRank(params int[] data) {
      int read = 0, pos = 0;                                                  Read(data, ref pos, ref read);
      int enumRank = data[pos];                                               Read(data, ref pos, ref read);
      var tnrStruc = new Span<int>(data, pos, read).ToArray();                Read(data, ref pos, ref read);
      var tnr = TnrInt.FromFlatSpec(data.AsSpan(pos, read), tnrStruc);     Read(data, ref pos, ref read);
      var expResStrucArr = new Span<int>(data, pos, read).ToArray();
      var expResStruc = expResStrucArr.ToList();                              Read(data, ref pos, ref read);
      var expRes = TnrInt.FromFlatSpec(data.AsSpan(pos, read),
         expResStrucArr);
      var res = new TnrInt(expResStruc);
      var rankCollection = tnr.EnumerateRank(enumRank);
      foreach(var tnr2 in rankCollection)
         res = res + tnr2;
      Assert.True(res.Equals(expRes));
   }

   [InlineData(
      1.01, 2.63, 3.21,
      5.56, 7.62, 5.45,
      6.50, 2.23, 7.66,

      1.02, 2.64, 3.21,
      5.55, 7.61, 5.44,
      6.51, 2.22, 7.65 )]
   /// <remarks> <see cref="TestRefs.TensorEquals"/> </remarks>
   [Theory] public void TensorEquals(params double[] data) {
      var tnr1 = Tnr.FromFlatSpec(data.AsSpan<dbl>(0,9), 3,3);
      var tnr2 = Tnr.FromFlatSpec(data.AsSpan<dbl>(9,9), 3,3);
      Assert.True(tnr1.Equals(tnr2, 0.02));
   }

   [InlineData(
      4,    5,0,2,1,             // vec
      2,    0,5,                 // read1
      2,    1,0,                 // read2
      2,    2,2,                 // read3
      2,    3,1,                  // read4
      2,    2,6,                 // wrt1
      2,    1,3,                 // wrt2
      2,    3,0                  // wrt3
   )]
   /// <remarks> <see cref="TestRefs.VectorIndexer"/> </remarks>
   [Theory] public void VectorIndexer(params int[] data) {
      int read = 0, pos = 0;                                      Read(data, ref pos, ref read);
      var vec = VecInt.FromFlatSpec(data.AsSpan(pos, read));   Read(data, ref pos, ref read);
      var read1 = new Span<int>(data, pos, read).ToArray();       Read(data, ref pos, ref read);
      var read2 = new Span<int>(data, pos, read).ToArray();       Read(data, ref pos, ref read);
      var read3 = new Span<int>(data, pos, read).ToArray();       Read(data, ref pos, ref read);
      var read4 = new Span<int>(data, pos, read).ToArray();       Read(data, ref pos, ref read);
      var wrt1 = new Span<int>(data, pos, read).ToArray();        Read(data, ref pos, ref read);
      var wrt2 = new Span<int>(data, pos, read).ToArray();        Read(data, ref pos, ref read);
      var wrt3 = new Span<int>(data, pos, read).ToArray();
      Assert.True(vec[read1[0]].Equals(read1[1]));
      Assert.True(vec[read2[0]].Equals(read2[1]));
      Assert.True(vec[read3[0]].Equals(read3[1]));
      Assert.True(vec[read4[0]].Equals(read4[1]));
      vec[wrt1[0]] = wrt1[1];
      Assert.True(vec[wrt1[0]].Equals(wrt1[1]));
      vec[wrt2[0]] = wrt2[1];
      Assert.True(vec[wrt2[0]].Equals(wrt2[1]));
      vec[wrt3[0]] = wrt3[1];
      Assert.True(vec[wrt3[0]].Equals(wrt3[1]));
   }

   [InlineData(
      4,    2,2,2,2,                                              // tnrStruc
      16,   4,0, 2,1,  9,5, 3,6,   7,0, 0,0,  3,9, 2,3,           // tnr
      4,    0,0,0,0,                                              // inxs1
      1,    4,                                                    // expRes1
      4,    0,1,0,0,                                              // inxs2
      1,    9,                                                    // expRes2
      4,    1,1,1,0,                                              // inxs3
      1,    2,                                                    // expRes3
      4,    1,1,1,1,                                              // inxs4
      1,    3,                                                     // expRes4
      4,    0,1,1,0,                                              // wrtInxs1
      1,    0,                                                    // expResW1
      4,    1,0,1,0,                                              // wrtInxs1
      1,    4                                                      // expResW2
   )]
   /// <remarks> <see cref="TestRefs.TensorTauIndexer"/> </remarks>
   [Theory] public void TensorTauIndexer(params int[] data) {
      int read = 0, pos = 0;  Read(data, ref pos, ref read);
      var tnrStruc = new Span<int>(data, pos, read).ToArray();          Read(data, ref pos, ref read);
      var tnr = TnrInt.FromFlatSpec(data.AsSpan(pos, read),
         tnrStruc);                                                     Read(data, ref pos, ref read);
      var inxs1 = new Span<int>(data, pos, read).ToArray();             Read(data, ref pos, ref read);
      int expRes1 = data[pos];                                          Read(data, ref pos, ref read);
      var inxs2 = new Span<int>(data, pos, read).ToArray();             Read(data, ref pos, ref read);
      int expRes2 = data[pos];                                          Read(data, ref pos, ref read);
      var inxs3 = new Span<int>(data, pos, read).ToArray();             Read(data, ref pos, ref read);
      int expRes3 = data[pos];                                          Read(data, ref pos, ref read);
      var inxs4 = new Span<int>(data, pos, read).ToArray();             Read(data, ref pos, ref read);
      int expRes4 = data[pos];
      Assert.True(tnr[inxs1].Equals(expRes1));
      Assert.True(tnr[inxs2].Equals(expRes2));
      Assert.True(tnr[inxs3].Equals(expRes3));
      Assert.True(tnr[inxs4].Equals(expRes4));                          Read(data, ref pos, ref read);
      var wrtInxs1 = new Span<int>(data, pos, read).ToArray();          Read(data, ref pos, ref read);
      int expResW1 = data[pos];
      tnr[wrtInxs1] = expResW1;
      Assert.True(tnr[wrtInxs1].Equals(expResW1));                      Read(data, ref pos, ref read);
      var wrtInxs2 = new Span<int>(data, pos, read).ToArray();          Read(data, ref pos, ref read);
      int expResW2 = data[pos];
      tnr[wrtInxs2] = expResW2;
      Assert.True(tnr[wrtInxs2].Equals(expResW2));
   }

   [InlineData(
      4,    2,2,2,2,                                              // tnrStruc
      16,   4,0, 2,1,  9,5, 3,6,   7,0, 0,0,  3,9, 2,3,           // tnr
      3,    0,0,0,                                                // inxs1
      2,    4,0,                                                  // expRes1
      3,    0,1,0,                                                // inxs2
      2,    9,5,                                                  // expRes2
      3,    1,1,1,                                                // inxs3
      2,    2,3,                                                  // expRes3
      3,    0,1,1,                                                // inxs4
      2,    3,6,                                                   // expRes4
      3,    0,1,1,                                                // wrtInxs1
      2,    0,0,                                                  // wrtVec1
      3,    1,1,1,                                                // wrtInxs2
      2,    0,5     )]                                              // wrtVec2
   /// <remarks> <see cref="TestRefs.TensorVectorIndexer"/> </remarks>
   [Theory] public void TensorVectorIndexer(params int[] data) {
      int read = 0, pos = 0;                                               Read(data, ref pos, ref read);
      var tnrStruc = new Span<int>(data, pos, read).ToArray();             Read(data, ref pos, ref read);
      var tnr = TnrInt.FromFlatSpec(
         data.AsSpan(pos, read), tnrStruc);                                Read(data, ref pos, ref read);
      var inxs1 = new Span<int>(data, pos, read).ToArray();                Read(data, ref pos, ref read);
      var expRes1 = VecInt.FromFlatSpec(data.AsSpan(pos,read));         Read(data, ref pos, ref read);
      var inxs2 = new Span<int>(data, pos, read).ToArray();                Read(data, ref pos, ref read);
      var expRes2 = VecInt.FromFlatSpec(data.AsSpan(pos,read));         Read(data, ref pos, ref read);
      var inxs3 = new Span<int>(data, pos, read).ToArray();                Read(data, ref pos, ref read);
      var expRes3 = VecInt.FromFlatSpec(data.AsSpan(pos,read));         Read(data, ref pos, ref read);
      var inxs4 = new Span<int>(data, pos, read).ToArray();                Read(data, ref pos, ref read);
      var expRes4 = VecInt.FromFlatSpec(data.AsSpan(pos,read));
      Assert.True(tnr[VoidsInt.Vec, inxs1].Equals(expRes1));
      Assert.True(tnr[VoidsInt.Vec, inxs2].Equals(expRes2));
      Assert.True(tnr[VoidsInt.Vec, inxs3].Equals(expRes3));
      Assert.True(tnr[VoidsInt.Vec, inxs4].Equals(expRes4));               Read(data, ref pos, ref read);
      var wrtInxs1 = new Span<int>(data, pos, read).ToArray();             Read(data, ref pos, ref read);
      var wrtVec1 = VecInt.FromFlatSpec(data.AsSpan(pos,read));
      tnr[VoidsInt.Vec, wrtInxs1] = wrtVec1;
      Assert.True(tnr[VoidsInt.Vec, wrtInxs1].Equals(wrtVec1));            Read(data, ref pos, ref read);
      var wrtInxs2 = new Span<int>(data, pos, read).ToArray();             Read(data, ref pos, ref read);
      var wrtVec2 = VecInt.FromFlatSpec(data.AsSpan(pos,read));
      tnr[VoidsInt.Vec, wrtInxs2] = wrtVec2;
      Assert.True(tnr[VoidsInt.Vec, wrtInxs2].Equals(wrtVec2));
   }

   [InlineData(
      4,    2,2,2,2,                                              // tnrStruc
      2,    2,2,                                                  // expResStruc
      16,   4,0, 2,1,  9,5, 3,6,   7,0, 0,0,  3,9, 2,3,           // tnr
      2,    0,0,                                                  // inxs1
      4,    4,0, 2,1,                                             // expRes1
      2,    0,1,                                                  // inxs2
      4,    9,5, 3,6,                                             // expRes2
      2,    1,1,                                                  // inxs3
      4,    3,9, 2,3,                                             // expRes3
      2,    1,0,                                                  // inxs4
      4,    7,0, 0,0,                                             // expRes4
      2,    1,0,                                                  // wrtInxs1
      4,    0,0, 0,0,                                             // wrtTnr1
      2,    1,1,                                                  // wrtInxs2
      4,    8,3, 0,4 )]                                           // wrtTnr2
   /// <remarks> <see cref="TestRefs.TensorTensorIndexer"/> </remarks>
   [Theory] public void TensorTensorIndexer(params int[] data) {
      int read = 0, pos = 0;                                                        Read(data, ref pos, ref read);
      var tnrStruc = new Span<int>(data, pos, read).ToArray();                      Read(data, ref pos, ref read);
      var expResStruc = new Span<int>(data, pos, read).ToArray();                   Read(data, ref pos, ref read);
      var tnr = TnrInt.FromFlatSpec(data.AsSpan(pos, read), tnrStruc);           Read(data, ref pos, ref read);
      var inxs1 = new Span<int>(data, pos, read).ToArray();                         Read(data, ref pos, ref read);
      var expRes1 = TnrInt.FromFlatSpec(data.AsSpan(pos,read), expResStruc);     Read(data, ref pos, ref read);
      var inxs2 = new Span<int>(data, pos, read).ToArray();                         Read(data, ref pos, ref read);
      var expRes2 = TnrInt.FromFlatSpec(data.AsSpan(pos,read), expResStruc);     Read(data, ref pos, ref read);
      var inxs3 = new Span<int>(data, pos, read).ToArray();                         Read(data, ref pos, ref read);
      var expRes3 = TnrInt.FromFlatSpec(data.AsSpan(pos,read), expResStruc);     Read(data, ref pos, ref read);
      var inxs4 = new Span<int>(data, pos, read).ToArray();                         Read(data, ref pos, ref read);
      var expRes4 = TnrInt.FromFlatSpec(data.AsSpan(pos,read), expResStruc);
      var res1 = tnr[VoidsInt.Tnr, inxs1];
      Assert.True(res1.Equals(expRes1));
      Assert.True(tnr[VoidsInt.Tnr, inxs2].Equals(expRes2));
      Assert.True(tnr[VoidsInt.Tnr, inxs3].Equals(expRes3));
      Assert.True(tnr[VoidsInt.Tnr, inxs4].Equals(expRes4));                        Read(data, ref pos, ref read);
      var wrtInxs1 = new Span<int>(data, pos, read).ToArray();                      Read(data, ref pos, ref read);
      var wrtTnr1 = TnrInt.FromFlatSpec(data.AsSpan(pos,read), expResStruc);
      tnr[VoidsInt.Tnr, wrtInxs1] = wrtTnr1;
      Assert.True(tnr[VoidsInt.Tnr, wrtInxs1].Equals(wrtTnr1));                     Read(data, ref pos, ref read);
      var wrtInxs2 = new Span<int>(data, pos, read).ToArray();                      Read(data, ref pos, ref read);
      var wrtTnr2 = TnrInt.FromFlatSpec(data.AsSpan(pos,read), expResStruc);
      tnr[VoidsInt.Tnr, wrtInxs2] = wrtTnr2;
      Assert.True(tnr[VoidsInt.Tnr, wrtInxs2].Equals(wrtTnr2));
   }

   [InlineData(
      3,    5,0,4,                        // vec1
      3,    0,9,8,                        // vec2
      2,    3,3,                          // expStruc
      9,    0,45,40, 0,0,0, 0,36,32 )]    // expRes
   [InlineData(
      3,    0,0,0,                        // vec1
      3,    0,9,8,                        // vec2
      2,    3,3,                          // expStruc
      9,    0,0,0, 0,0,0, 0,0,0  )]       // expRes
   [InlineData(
      3,    5,0,4,                        // vec1
      3,    0,0,0,                        // vec2
      2,    3,3,                          // expStruc
      9,    0,0,0, 0,0,0, 0,0,0  )]       // expRes
   /// <remarks> <see cref="TestRefs.VectorTnrProductVector"/> </remarks>
   [Theory] public void VectorTnrProductVector(params int[] data) {
      int read = 0, pos = 0;                                                        Read(data, ref pos, ref read);
      var vec1 = VecInt.FromFlatSpec(data.AsSpan(pos,read));                     Read(data, ref pos, ref read);
      var vec2 = VecInt.FromFlatSpec(data.AsSpan(pos,read));                     Read(data, ref pos, ref read);
      var expStruc = new Span<int>(data, pos, read).ToArray();                      Read(data, ref pos, ref read);
      var expRes = TnrInt.FromFlatSpec(data.AsSpan(pos, read), expStruc);
      var res = vec1.TnrProduct(vec2);
      Assert.True(res.Equals(expRes));
   }
}
}