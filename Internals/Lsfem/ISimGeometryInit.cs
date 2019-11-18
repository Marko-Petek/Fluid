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

public interface ISimGeometryInit : ISimInit {
   /// <summary>A cartesian array of positions. [i][j][{x,y}]</summary>
   dbl[][][] OrdPos { get; }


   /// <summary>2D lists of nodes lying inside a block. Return the "one after last" node index.</summary>
   (int newCurrGInx, Dictionary<string,PE[][]>) CreatePatches();
   /// <summary>1D lists of nodes shared across blocks.</summary>
   /// <param name="currGInx">The "one after last" index returned by CreatePatches method.</param>
   Dictionary<string,PE[]> CreateJoints(int currGInx);
   /// <summary>Custom logic here depends on the way the blocks are joined together. Do not forget to trim excess space.</summary>
   Element[] CreateElements();
   /// <summary>Create constrained variables at desired positions.</summary>
   Tnr CreateConstraints();
}
}