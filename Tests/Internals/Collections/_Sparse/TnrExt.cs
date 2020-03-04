using System;
using dbl = System.Double;
using System.Linq;
using Xunit;
using Fluid.Internals;
using Fluid.Internals.Collections;
using static Fluid.Internals.Collections.TnrFactory;
using static Fluid.Tests.Utils;
using IA = Fluid.Internals.Collections.IntArithmetic;
using DA = Fluid.Internals.Collections.DblArithmetic;

namespace Fluid.Tests.Internals.Collections {
   using IntTnr = Tnr<int,IA>;
   using IntVec = Vec<int,IA>;
public class TnrExt {
   #region InlineData
   [InlineData(
      5,3,2,  7,6,9,  0,4,2,
      3,1,0,  4,2,8,  7,2,3,
      8,4,2,  11,8,17,  7,6,5)]
   [InlineData(
      0,0,0,  7,6,9,  0,4,2,
      3,1,0,  4,2,8,  7,2,3,
      3,1,0,  11, 8, 17,  7,6,5)]
   [InlineData(
      0,0,0,  7,6,9,  0,4,2,
      3,1,0,  4,2,8,  0,0,0,
      3,1,0,  11,8,17,  0,4,2)]
   [InlineData(
      0,0,0,  7,6,9,  0,4,2,
      3,1,0,  4,2,8,  0,-4,-2,
      3,1,0,  11,8,17,  0,0,0)]
   [InlineData(
      0,0,0,  0,0,0,  0,0,0,
      3,1,0,  4,2,8,  0,-4,-2,
      3,1,0,  4,2,8,  0,-4,-2)]
   #endregion
   [Theory] public void SumInto(params int[] o) {
      var span1 = new Span<int>(o, 0, 9);
      var span2 = new Span<int>(o, 9, 9);
      var span3 = new Span<int>(o, 18, 9);
      var t1 = TopTnrFromSpan<int,IA>(span1, new int[] {3,3});
      var t2 = TopTnrFromSpan<int,IA>(span2, new int[] {3,3});
      var t3 = TopTnrFromSpan<int,IA>(span3, new int[] {3,3});
      t1 = t1.SumInto(t2);
      Assert.True(t1.EqualS(t3));
   }
   #region InlineData
   [InlineData( // Subtract a non-zero tensor from non-zero and get a non-zero result.
      5,3,2,  7,6,9,  0,4,2,
      3,1,0,  4,2,8,  7,2,3,
      2,2,2,  3,4,1,  -7, 2,-1)]
   [InlineData( // Subtract a non-zero tensor from a zero (line) tensor and get a non-zero result.
      0,0,0,  7,6,9,  0,4,2,
      3,1,0,  4,2,8,  7,2,3,
      -3,-1,0,  3,4,1,  -7,2,-1)]
   [InlineData( // Subtract a zero (line) tensor from a zero tensor and get a non-zero result.
      0,0,0,  7,6,9,  0,4,2,
      3,1,0,  4,2,8,  0,0,0,
      -3,-1, 0,  3,4,1,  0,4,2)]
   [InlineData( // Subtract a non-zero tensor from a zero (line) tensor and get a zero (line) result.
      0,0,0,  4,2,8,  0,4,2,
      3,1,0,  4,2,8,  0,0,0,
      -3,-1,0,  0,0,0,  0,4,2)]
   [InlineData( // Subtract non-zero from non-zero. Get zero.
      1,2,3,  0,0,0,  7,8,9,
      1,2,3,  0,0,0,  7,8,9,
      0,0,0,  0,0,0,  0,0,0
   )]
   #endregion
   [Theory] public void SubInto(params int[] o) {
      var span1 = new Span<int>(o, 0, 9);
      var span2 = new Span<int>(o, 9, 9);
      var span3 = new Span<int>(o, 18, 9);
      var t1 = TopTnrFromSpan<int,IA>(span1, new int[] {3,3});
      var t2 = TopTnrFromSpan<int,IA>(span2, new int[] {3,3});
      var t3 = TopTnrFromSpan<int,IA>(span3, new int[] {3,3});
      t1 = t1.SubInto(t2);
      Assert.True(t1.EqualS(t3));
   }

   #region InlineData
   [InlineData(
      3,0,7,  2,1,0,  0,4,8,
      -3,0,-7,  -2,-1,0,  0,-4,-8 )]
   [InlineData(
      0,0,0,  0,0,0,  0,0,0,
      0,0,0,  0,0,0,  0,0,0 )]
   #endregion
   [Theory] public void NegateInto(params int[] o) {
      var t = TopTnrFromSpan<int,IA>(o.AsSpan<int>(0,9), 3,3);
      var expRes = TopTnrFromSpan<int,IA>(o.AsSpan<int>(9,9), 3,3);
      t = t.NegateInto();
      Assert.True(t.EqualS(expRes));
   }

   #region InlineData
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
      16,   26,16, 30,14,  59,37, 66,32,   52,32, 60,28,  49,35, 42,28 )] // expRes
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
   #endregion
   [Theory] public void ContractTop(params int[] o) {          var (pos, read) = Read(o);
      var ctrInxs = new Span<int>(o, pos, read);               (pos, read) = Read(o, pos, read);
      var strc1 = new Span<int>(o, pos, read).ToArray();       (pos, read) = Read(o, pos, read);
      var t1 = TopTnrFromSpan<int,IA>(
         new Span<int>(o, pos, read), strc1);                  (pos, read) = Read(o, pos, read);
      var strc2 = new Span<int>(o, pos, read).ToArray();       (pos, read) = Read(o, pos, read);
      var t2 = TopTnrFromSpan<int,IA>(
         new Span<int>(o, pos, read), strc2);                  (pos, read) = Read(o, pos, read);
      var strcExpRes = new Span<int>(o, pos, read).ToArray();  (pos, read) = Read(o, pos, read);
      var expRes = TopTnrFromSpan<int,IA>( 
         new Span<int>(o, pos, read), strcExpRes);
      var res = t1.ContractTop(t2, ctrInxs[0], ctrInxs[1]);
      Assert.True(res.EqualS(expRes));
   }
}
}