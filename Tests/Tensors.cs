using System;
using System.Linq;
using System.Threading;
using Xunit;
using Fluid.Internals.Collections;
using Fluid.Internals.Numerics;
using SCG = System.Collections.Generic;
using TB = Fluid.Internals.Toolbox;
using Fluid.TestRef;

namespace Fluid.Tests {
   using dbl = Double;
   using Tensor = Tensor<double,DblArithmetic>;
   using Vector = Vector<double,DblArithmetic>;
   using TensorInt = Tensor<int,IntArithmetic>;
   using VectorInt = Vector<int,IntArithmetic>;
   using IA = IntArithmetic;
   
   public partial class Thread3 {      //TODO: Log timing of methods for a large number of operations and save results.
      static Thread3() {
         TB.EntryPointSetup("Starting Thread3 tests.");
      }

      [InlineData(1,7,                    // Rank 1 tensor.
         6,5,3,8,0,1,4)]
      [InlineData(2,3,3,                  // Rank 2 tensor.
         6,5,3,8,0,1,4,3,1)]
      [InlineData(3,3,2,3,                // Rank 3 tensor.
         6,5,3,8,0,1,4,3,1,6,0,9,1,3,7,2,7,7)]
      [InlineData(4,2,2,3,3,                // Rank 3 tensor.
         6,5,3,8,0,1,4,3,1,6,0,9,1,3,7,2,7,7,1,1,3,6,5,4,0,0,4,2,8,5,3,3,5,7,5,3)]
      /// <remarks><see cref="TestRefs.TensorCopy"/></remarks>
      [Theory] public void TensorCopy(params int[] data) {
         int topRank = data[0];
         var struc = data.Skip(1).Take(topRank).ToArray();
         int count = 1;                                        // How many emts to take into slice.
         foreach(int emt in struc)
            count *= emt;
         var slc = new Span<int>(data, 1 + topRank, count);
         var tnr = Tensor<int,IA>.FromFlatSpec(slc, struc);
         var tnrCpy = tnr.Copy(new TensorInt.CopySpecStruct(TensorInt.WhichFields.ValuesAndNonValueFields, TensorInt.WhichNonValueFields.All, TensorInt.HowToCopyStructure.CreateNewStructure));
         Assert.True(tnr.Equals(tnrCpy));
      }

      [InlineData(5,3,2, 7,3,9, 12,6,11)]
      [InlineData(1, 0, 2,   2, 3, 1,  3, 3, 3)]
      [InlineData(1, 3, 2,   2, 0, 1,  3, 3, 3)]
      /// <remarks><see cref="TestRefs.VectorAdd"/></remarks>
      [Theory] public void VectorAdd(params int[] data) {
         var vec1 = VectorInt.CreateFromFlatSpec(data.AsSpan<int>(0,3));
         var vec2 = VectorInt.CreateFromFlatSpec(data.AsSpan<int>(3,3));
         var vec3 = VectorInt.CreateFromFlatSpec(data.AsSpan<int>(6,3));
         vec1.Add(vec2);
         Assert.True(vec1.Equals(vec3));
      }

      [InlineData(1, 3, 2,   2, 3, 1,  3, 6, 3)]
      [InlineData(1, 0, 2,   2, 3, 1,  3, 3, 3)]
      [InlineData(1, 3, 2,   2, 0, 1,  3, 3, 3)]
      /// <remarks><see cref="TestRefs.Op_VectorAddition"/></remarks>
      [Theory] public void Op_VectorAddition(params int[] data) {
         var vec1 = VectorInt.CreateFromFlatSpec(data.AsSpan<int>(0,3));
         var vec2 = VectorInt.CreateFromFlatSpec(data.AsSpan<int>(3,3));
         var res = vec1 + vec2;
         var expRes = VectorInt.CreateFromFlatSpec(data.AsSpan<int>(6,3));
         Assert.True(res.Equals(expRes));
      }
      
      [InlineData(1, 3, 2,   2, 3, 1,  -1, 0, 1)]
      [InlineData(1, 0, 2,   2, 3, 1,  -1,-3, 1)]
      [InlineData(1, 3, 2,   2, 0, 1,  -1, 3, 1)]
      /// <remarks><see cref="TestRefs.Op_VectorSubtraction"/></remarks>
      [Theory] public void Op_VectorSubtraction(params int[] data) {
         var vec1 = VectorInt.CreateFromFlatSpec(data.AsSpan<int>(0,3));
         var vec2 = VectorInt.CreateFromFlatSpec(data.AsSpan<int>(3,3));
         var res = vec1 - vec2;
         var expRes = VectorInt.CreateFromFlatSpec(data.AsSpan<int>(6,3));
         Assert.True(res.Equals(expRes));
      }
      
      [InlineData(
         5,3,2,
         7,6,9,
         0,4,2,

         3,1,0,
         4,2,8,
         7,2,3,

           8 , 4,  2,
          11,  8, 17,
           7,  6,  5)]
      /// <remarks><see cref="TestRefs.TensorAdd"/></remarks>
      [Theory] public void TensorAdd(params int[] data) {
         var span1 = new Span<int>(data, 0, 9);
         var span2 = new Span<int>(data, 9, 9);
         var span3 = new Span<int>(data, 18, 9);
         var tnr1 = TensorInt.FromFlatSpec(span1, new int[] {3,3});
         var tnr2 = TensorInt.FromFlatSpec(span2, new int[] {3,3});
         var tnr3 = TensorInt.FromFlatSpec(span3, new int[] {3,3});
         tnr1.Add(tnr2);
         Assert.True(tnr1.Equals(tnr3));
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
         var tnr = TensorInt.FromFlatSpec(data.AsSpan<int>(0,9), 3,3);
         var expRes = TensorInt.FromFlatSpec(data.AsSpan<int>(9,9), 3,3);
         var res = -tnr;
         Assert.True(res.Equals(expRes));
      }

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
      [Theory] public void TensorContract(params int[] data) {
         int read = 0, pos = 0;
         Read(data, ref pos, ref read);
         var ctrInxs = new Span<int>(data, pos, read);                                    // Read contract indices.
         Read(data, ref pos, ref read);
         var struc1 = new Span<int>(data, pos, read).ToArray();                           // Read 1st tnr structure.
         Read(data, ref pos, ref read);
         var tnr1 = TensorInt.FromFlatSpec(new Span<int>(data, pos, read), struc1); // Read 1st tnr.
         Read(data, ref pos, ref read);
         var struc2 = new Span<int>(data, pos, read).ToArray();                           // Read 2nd tnr structure.
         Read(data, ref pos, ref read);
         var tnr2 = TensorInt.FromFlatSpec(new Span<int>(data, pos, read), struc2); // Read 2nd tnr.
         Read(data, ref pos, ref read);
         var strucExpRes = new Span<int>(data, pos, read).ToArray();                      // Read expected result structure.
         Read(data, ref pos, ref read);
         var expRes = TensorInt.FromFlatSpec(                                       // Read expected result.   
            new Span<int>(data, pos, read), strucExpRes);
         var res = tnr1.Contract(tnr2, ctrInxs[0], ctrInxs[1]);
         Assert.True(res.Equals(expRes));
      }

      void Read(int[] data, ref int pos, ref int read) {
         pos += read;
         read = data[pos];
         ++pos;
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
         int read = 0, pos = 0;
         Read(data, ref pos, ref read);
         var redRank = data[pos];
         Read(data, ref pos, ref read);
         var emtInx = data[pos];
         Read(data, ref pos, ref read);
         var tnrStruc = new Span<int>(data, pos, read).ToArray();
         Read(data, ref pos, ref read);
         var tnr = TensorInt.FromFlatSpec(new Span<int>(data, pos, read), tnrStruc);
         Read(data, ref pos, ref read);
         var expResStruc = new Span<int>(data, pos, read).ToArray();
         Read(data, ref pos, ref read);
         var expRes = TensorInt.FromFlatSpec(new Span<int>(data, pos, read), expResStruc);
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
         int read = 0, pos = 0;
         Read(data, ref pos, ref read);
         var slotInx1 = data[pos];
         Read(data, ref pos, ref read);
         var slotInx2 = data[pos];
         Read(data, ref pos, ref read);
         var tnrStruc = new Span<int>(data, pos, read).ToArray();
         Read(data, ref pos, ref read);
         var tnr = TensorInt.FromFlatSpec(new Span<int>(data, pos, read), tnrStruc);
         Read(data, ref pos, ref read);
         var expResStruc = new Span<int>(data, pos, read).ToArray();
         Read(data, ref pos, ref read);
         var expRes = TensorInt.FromFlatSpec(new Span<int>(data, pos, read), expResStruc);
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
         var operand1 = Tensor<int,IA>.FromFlatSpec(span1, structure1);
         var span2 = data.Skip(pos).Take(count2).ToArray().AsSpan();
         pos += count2;
         var operand2 = Tensor<int,IA>.FromFlatSpec(span2, structure2);
         var res = operand1.TnrProduct(operand2);
         var count = count1 * count2;
         var span3 = data.Skip(pos).Take(count).ToArray().AsSpan();
         var structure3 = structure1.Concat(structure2).ToArray();
         var expRes = Tensor<int,IA>.FromFlatSpec(span3, structure3);
         Assert.True(res.Equals(expRes));
      }
      [InlineData(5,3,2, 7,6,9, 0,4,2,  13)]
      [InlineData(3,1,0, 4,2,8, 7,2,3,  8)]
      [InlineData(8,4,2, 11,8,17, 7,6,5,  21)]
      /// <remarks> <see cref="TestRefs.TensorSelfContractR2"/> </remarks>
      [Theory] public void SelfContractR2(params int[] data) {
         var span1 = new Span<int>(data, 0, 9);
         var tnr1 = TensorInt.FromFlatSpec(span1, 3,3);
         var res = tnr1.SelfContractR2();
         var expRes = data[9];
         Assert.True(res == expRes);
      }

      /// <remarks> <see cref="TestRefs.Op_VectorDotVector"/> </remarks>
      [InlineData(2, 1, 3, 5, 2, 3, 21)]
      [InlineData(2, 1, 3, 0, 2, 3, 11)]
      [InlineData(9, 8, 1, 2, 6, 7, 73)]
      [Theory] public void Op_VectorDotVector(params int[] data) {
        var vec1 = VectorInt.CreateFromFlatSpec(data.AsSpan<int>(0,3));
        var vec2 = VectorInt.CreateFromFlatSpec(data.AsSpan<int>(3,3));
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
         var tnr1 = TensorInt.FromFlatSpec(data.AsSpan<int>(0,9), 3,3);
         var tnr2 = TensorInt.FromFlatSpec(data.AsSpan<int>(9,9), 3,3);
         var tnr3 = tnr1 + tnr2;
         var expMat = TensorInt.FromFlatSpec(data.AsSpan<int>(18,9), 3,3);
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
      /// <remarks> <see cref="TestRefs.Op_TensorSubtraction"/> </remarks>
      [Theory] public void Op_TensorSubtraction(params int[] data) {
         var tnr1 = TensorInt.FromFlatSpec(data.AsSpan<int>(0,9), 3,3);
         var tnr2 = TensorInt.FromFlatSpec(data.AsSpan<int>(9,9), 3,3);
         var tnr3 = tnr1 - tnr2;
         var expMat = TensorInt.FromFlatSpec(data.AsSpan<int>(18,9), 3,3);
         Assert.True(tnr3.Equals(expMat));
      }
      ///// <summary>Dot a 2nd rank tensor with a vector.</summary>
      [InlineData(
         1, 2, 3,
         2, 1, 4,
         3, 4, 1,

         2, 1, 3, 13, 17, 13
      )]
      [InlineData(
         0, 0, 0,
         5, 2, 3,
         2, 1, 0,

         5, 2, 3, 0, 38, 12)]
      [Theory] public void TnrDotVec(params int[] data) {                  // TODO: Move to Contract tests.
         var tnr = TensorInt.FromFlatSpec(data.AsSpan<int>(0,9), 3,3);
         var vec = VectorInt.CreateFromFlatSpec(data.AsSpan<int>(9,3));
         var expRes = VectorInt.CreateFromFlatSpec(data.AsSpan<int>(12,3));
         var res = (VectorInt) tnr.Contract(vec, 2, 1);
         Assert.True(res.Equals(expRes));
      }

      [InlineData(
         3, 1, 9, 4,
         8, 5, 3, 2,

         9, 7, 3, 1, 65, 118)]
      [InlineData(
         3, 1, 9, 4,
         8, 5, 3, 2,

         9, 0, 3, 1, 58, 83)]
      [InlineData(
         3, 1, 9, 0,
         8, 0, 3, 2,

         9, 7, 3, 1, 61, 83)]
      [InlineData(
         3, 1, 9, 0,
         8, 0, 3, 2,

         9, 7, 0, 1, 34, 74)]
      [Theory] public void TnrDotVecAsym(params int[] data) {                          // TODO: Move to Contract tests.
         var tnr = TensorInt.FromFlatSpec(data.AsSpan<int>(0,8), 2,4);
         var vec = VectorInt.CreateFromFlatSpec(data.AsSpan<int>(8,4));
         var expRes = VectorInt.CreateFromFlatSpec(data.AsSpan<int>(12,2));
         var res = tnr.Contract(vec,2,1);
         Assert.True(res.Equals(expRes));
      }

      [InlineData(
         1, 2, 3,
         2, 1, 4,
         3, 4, 1,

         2, 1, 3, 13, 17, 13)]
      [InlineData(
         2, 6, 1,
         3, 9, 4,
         7, 1, 6,

         3, 1, 5, 44, 32, 37)]
      [Theory] public void VecDotTnr(params int[] data) {                                    // TODO: Move to Contract tests.
         var tnr = TensorInt.FromFlatSpec(data.AsSpan<int>(0,9), 3,3);
         var vec = VectorInt.CreateFromFlatSpec(data.AsSpan<int>(9,3));
         var expRes = VectorInt.CreateFromFlatSpec(data.AsSpan<int>(12,3));
         var res = vec.Contract(tnr,1);
         Assert.True(res.Equals(expRes));
      }

      [InlineData(
         5, 3, 0, 2, 6,
         30, 18, 0, 12)]
      /// <remarks> <see cref="TestRefs.Op_ScalarVectorMultiplication"/> </remarks>
      [Theory] public void Op_ScalarVectorMultiplication(params int[] data) {
         var num = data[4];
         var vec = VectorInt.CreateFromFlatSpec(data.AsSpan<int>(0,4));
         var res = num * vec;
         var expRes = VectorInt.CreateFromFlatSpec(data.AsSpan<int>(5,4));
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
      /// <remarks> <see cref="TestRefs.Op_ScalarTensorMultiplication"/> </remarks>
      [Theory] public void Op_ScalarTensorMultiplication(params int[] data) {
         var slc1 = new Span<int>(data, 0, 9);
         var slc2 = new Span<int>(data, 10, 9);
         var mat = TensorInt.FromFlatSpec(slc1, 3,3);
         var num = data[9];
         var res = num * mat;
         var expRes = TensorInt.FromFlatSpec(slc2,3,3);
         Assert.True(res.Equals(expRes));
      }

      [InlineData(
         1,    2,                                     // enumRank
         3,    2,2,3,                                 // tnrStruc
         12,   5,5,3, 2,7,1,  6,5,0, 0,7,5,           // tnr
         2,    2,3,                                   // expResStruc
         6,    11,10,3, 2,14,6)]                      // expRes
      [Theory] public void TensorEnumerateRank(params int[] data) {
         int read = 0, pos = 0;
         Read(data, ref pos, ref read);
         int enumRank = data[pos];
         Read(data, ref pos, ref read);
         var tnrStruc = new Span<int>(data, pos, read).ToArray();
         Read(data, ref pos, ref read);
         var tnr = TensorInt.FromFlatSpec(data.AsSpan(pos, read), tnrStruc);
         Read(data, ref pos, ref read);
         var expResStruc = new Span<int>(data, pos, read).ToArray();
         Read(data, ref pos, ref read);
         var expRes = TensorInt.FromFlatSpec(data.AsSpan(pos, read), expResStruc);
         var res = new TensorInt(expResStruc);
         var rankCollection = tnr.EnumerateRank(enumRank);
         foreach(var tnr2 in rankCollection)
            res = res + tnr2;                                        // FIXME: Error occurs here.
         Assert.True(res.Equals(expRes));
      }

      [InlineData(
         1,    2,                                     // enumRank
         3,    2,2,3,                                 // tnrStruc
         12,   5,5,3, 2,7,1,  6,5,0, 0,7,5,           // tnr
         2,    2,3,                                   // expResStruc
         6,    11,10,3, 2,14,6)]                      // expRes
      [Theory] public void RankEnumerator1(params int[] data) {
         var tnr = TensorInt.FromFlatSpec(data.AsSpan(0,12), 2,2,3);
         var res = new TensorInt(new int[] {2,3});
         var rankCollection = tnr.EnumerateRank(2);
         foreach(var tnr2 in rankCollection)
            res = res + tnr2;
         var expRes = TensorInt.FromFlatSpec(data.AsSpan(12,6), 2,3);
         Assert.True(res.Equals(expRes));
      }

       [InlineData(
         5,5, 3,2,  7,1, 6,5,   0,0, 7,5,  4,2, 8,9,           // R4: 2,2,2,2
         16,8, 24,21)]                                         // R2: 2,2
       [Theory] public void RankEnumerator2(params int[] data) {
          var tnr = TensorInt.FromFlatSpec(data.AsSpan(0,16), 2,2,2,2);
          var res = new TensorInt(new int[] {2,2});
          var rankCollection = tnr.EnumerateRank(2);
          foreach(var tnr2 in rankCollection)
             res = res + tnr2;
          var expRes = TensorInt.FromFlatSpec(data.AsSpan(16,4), 2,2);
          Assert.True(res.Equals(expRes));
       }

      [InlineData(
         1.01, 2.63, 3.21,
         5.56, 7.62, 5.45,
         6.50, 2.23, 7.66,

         1.02, 2.64, 3.21,
         5.55, 7.61, 5.44,
         6.51, 2.22, 7.65 )]
      [Theory] public void TnrEquals(params double[] data) {
         var tnr1 = Tensor.FromFlatSpec(data.AsSpan<dbl>(0,9), 3,3);
         var tnr2 = Tensor.FromFlatSpec(data.AsSpan<dbl>(9,9), 3,3);
         Assert.True(tnr1.Equals(tnr2, 0.02));
      }
   }
}