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

   #region InlindeData
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
   #endregion
   [Theory] public void Indexer(params int[] o) {                        var (pos, read) = Read(o);
      var v = TopVecFromSpan<int,IA>(o.AsSpan(pos, read)) ??
         throw new NullReferenceException("Generated vector was null.");       (pos, read) = Read(o, pos, read);
      var gv1 = new Span<int>(o, pos, read).ToArray();                         (pos, read) = Read(o, pos, read);
      var gv2 = new Span<int>(o, pos, read).ToArray();                         (pos, read) = Read(o, pos, read);
      var gv3 = new Span<int>(o, pos, read).ToArray();                         (pos, read) = Read(o, pos, read);
      var gv4 = new Span<int>(o, pos, read).ToArray();                         (pos, read) = Read(o, pos, read);
      var sv1 = new Span<int>(o, pos, read).ToArray();                         (pos, read) = Read(o, pos, read);
      var sv2 = new Span<int>(o, pos, read).ToArray();                         (pos, read) = Read(o, pos, read);
      var sv3 = new Span<int>(o, pos, read).ToArray();
      Assert.True( v[gv1[0]] == gv1[1] );
      Assert.True( v[gv2[0]] == gv2[1] );
      Assert.True( v[gv3[0]] == gv3[1] );
      Assert.True( v[gv4[0]] == gv4[1] );
      v[sv1[0]] = sv1[1];
      Assert.True( v[sv1[0]] == sv1[1] );
      v[sv2[0]] = sv2[1];
      Assert.True( v[sv2[0]] == sv2[1] );
      v[sv3[0]] = sv3[1];
      Assert.True( v[sv3[0]] == sv3[1] );
   }

   #region InlindeData
   [InlineData(1, 3, 2,   2, 3, 1,  3, 6, 3)]
   [InlineData(1, 0, 2,   2, 3, 1,  3, 3, 3)]
   [InlineData(1, 3, 2,   2, 0, 1,  3, 3, 3)]
   [InlineData(1, 3, 2,  -1,-3,-2,  0, 0, 0)]
   #endregion
   [Theory] public void Op_Addition(params int[] o) {
      var v1 = TopVecFromSpan<int,IA>(o.AsSpan<int>(0,3));
      var v2 = TopVecFromSpan<int,IA>(o.AsSpan<int>(3,3));
      var res = v1 + v2;
      var expRes = TopVecFromSpan<int,IA>(o.AsSpan<int>(6,3));
      Assert.True(res.Equals<int,IA>(expRes));
   }

   #region InlindeData
   [InlineData(1, 3, 2,   2, 3, 1,  -1, 0, 1)]
   [InlineData(1, 0, 2,   2, 3, 1,  -1,-3, 1)]
   [InlineData(1, 3, 2,   1, 3, 2,   0, 0, 0)]
   #endregion
   [Theory] public void Op_Subtraction(params int[] o) {
      var vec1 = TopVecFromSpan<int,IA>(o.AsSpan<int>(0,3));
      var vec2 = TopVecFromSpan<int,IA>(o.AsSpan<int>(3,3));
      var res = vec1 - vec2;
      var expRes = TopVecFromSpan<int,IA>(o.AsSpan<int>(6,3));
      Assert.True( res.Equals<int,IA>(expRes) );
   }

}
}