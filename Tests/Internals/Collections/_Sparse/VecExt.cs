using System;
using dbl = System.Double;
using System.Linq;
using Xunit;
using Fluid.Internals;
using Fluid.Internals.Tensors;
using static Fluid.Internals.Tensors.TnrFactory;
using static Fluid.Tests.Utils;
using Fluid.Internals.Algebras;

namespace Fluid.Tests.Internals.Collections {
   using TnrI = Tnr<int,IntA>;
   using VecI = Vec<int,IntA>;
public class VecExt {
   
   #region InlineData
   [InlineData(5, 3, 2,   7, 3, 9,  12,6,11)]
   [InlineData(1, 3, 2,   2, 3, 1,  3, 6, 3)]
   [InlineData(1, 0, 2,   2, 3, 1,  3, 3, 3)]
   [InlineData(1, 3, 2,   2, 0, 1,  3, 3, 3)]
   [InlineData(1, 3, 2,   -1,-3,-2,  0, 0, 0)]
   #endregion
   [Theory] public void SumInto(params int[] o) {
      var v1 = TopVecFromSpan<int,IntA>(o.AsSpan<int>(0,3));
      var v2 = TopVecFromSpan<int,IntA>(o.AsSpan<int>(3,3));
      v1 = v1.SumInto(v2);
      var expRes = TopVecFromSpan<int,IntA>(o.AsSpan<int>(6,3));
      Assert.True(v1.Equalsβ(expRes));
   }

   #region InlineData
   [InlineData(1,3,2,  2,3,1,  -1,0,1)]
   [InlineData(1,0,2,  2,3,1,  -1,-3,1)]
   [InlineData(1,3,2,  2,0,1,  -1,3,1)]
   [InlineData(1,3,2,  1,3,2,  0,0,0)]
   #endregion
   [Theory] public void SubInto(params int[] o) {
      var v1 = TopVecFromSpan<int,IntA>(o.AsSpan<int>(0,3));
      var v2 = TopVecFromSpan<int,IntA>(o.AsSpan<int>(3,3));
      v1 = v1.SubInto(v2);
      var expRes = TopVecFromSpan<int,IntA>(o.AsSpan<int>(6,3));
      Assert.True(v1.Equalsβ(expRes));
   }

}
}