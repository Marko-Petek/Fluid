using System;
using System.Collections.Generic;
using dbl = System.Double;
using DA = Fluid.Internals.Numerics.DblArithmetic;
using My = Fluid.Internals.Collections.Custom;
namespace Fluid.Internals.Lsfem {
   using Vec = Fluid.Internals.Collections.Vector<dbl,DA>;
   using Tnr = Fluid.Internals.Collections.Tensor<dbl, DA>;
   using Voids = Voids<dbl,DA>;
   using Lst = List<int>;
   using VecLst = My.List<Vec2>;
   using PE = PseudoElement;

interface ISimInit {
   (int newCurrGInx, Dictionary<string,PE[][]>) CreatePatches(int currGinx);
}
}