#nullable enable
using System;
using dbl = System.Double;

using Fluid.Internals.Numerics;

namespace Fluid.Internals.Lsfem {
   public delegate dbl F2Da(in dbl x, in dbl y);
   public delegate dbl F2Db(in Vec2 pos);
}
#nullable restore