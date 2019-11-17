using System;
using System.Collections.Generic;
using Fluid.Internals.Numerics;
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

public interface ISimInit {
   (Tnr initA, Tnr initFs) InitializeSecondaries(int nPos, int nfPos, int nVar);
}
}