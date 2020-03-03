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
public class Vec {
   [InlineData(1, 3, 2,   2, 3, 1,  3, 6, 3)]
   [InlineData(1, 0, 2,   2, 3, 1,  3, 3, 3)]
   [InlineData(1, 3, 2,   2, 0, 1,  3, 3, 3)]
   [InlineData(1, 3, 2,  -1,-3,-2,  0, 0, 0)]
   [Theory] public void Op_Addition(params int[] o) {
      var v1 = TopVecFromSpan<int,IA>(o.AsSpan<int>(0,3));
      var v2 = TopVecFromSpan<int,IA>(o.AsSpan<int>(3,3));
      var res = v1 + v2;
      var expRes = TopVecFromSpan<int,IA>(o.AsSpan<int>(6,3));
      Assert.True(res.Equals<int,IA>(expRes));
   }
}
}