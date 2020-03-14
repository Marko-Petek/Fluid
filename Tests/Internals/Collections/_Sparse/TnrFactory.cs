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
public class TnrFactory {

   #region InlindeData
   [InlineData(1,7,                    // Rank 1 tensor.
      6,5,3,8,0,1,4)]
   [InlineData(2,3,3,                  // Rank 2 tensor.
      6,5,3,  8,0,1,  4,3,1)]
   [InlineData(3,3,2,3,                // Rank 3 tensor.
      6,5,3,  8,0,1,     4,3,1,  6,0,9,     1,3,7,  2,7,7)]
   [InlineData(4,2,2,3,3,                // Rank 4 tensor.
      6,5,3,  8,0,1,  4,3,1,     6,0,9,  1,3,7,  2,7,7,        1,1,3,  6,5,4,  0,0,4,     2,8,5,  3,3,5,  7,5,3)]
   #endregion
   [Theory] public void CopyAsTopTnr(params int[] o) {
      int topRank = o[0];
      var struc = o.Skip(1).Take(topRank).ToArray();
      int count = 1;                                        // How many emts to take into slice.
      foreach(int emt in struc)
         count *= emt;
      var slc = new Span<int>(o, 1 + topRank, count);
      var tnr = TopTnrFromSpan<int,IA>(slc, struc);
      var tnrCpy = tnr.CopyAsTopTnr();
      Assert.True(tnr.Equalsβ<int,IA>(tnrCpy));
   }

}
}