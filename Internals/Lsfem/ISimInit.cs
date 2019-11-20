using System.Collections;
using System.Collections.Generic;
using Fluid.Internals.Numerics;
using dbl = System.Double;
using DA = Fluid.Internals.Numerics.DblArithmetic;
using My = Fluid.Internals.Collections.Custom;
namespace Fluid.Internals.Lsfem {
   using Vec = Fluid.Internals.Collections.Vector<dbl,DA>;
   using Tnr = Fluid.Internals.Collections.Tensor<dbl, DA>;
   using PE = PseudoElement;

public interface ISimInit {
   (Tnr initA, Tnr initFs) InitializeSecondaries(int nPos, int nfPos, int nVar);
   /// <summary>A cartesian array of positions. [i][j][{x,y}]</summary>
   dbl[][][] CreateOrdPos();
   /// <summary>2D lists of nodes lying inside a block. Return the "one after last" node index.</summary>
   /// <param name="ordPos">A cartesian array of positions. [i][j][{x,y}]</param>
   (int newCurrGInx, Dictionary<string,PE[][]>) CreatePatches(dbl[][][] ordPos);
   /// <summary>1D lists of nodes shared across blocks.</summary>
   /// <param name="currGInx">The "one after last" index returned by CreatePatches method.</param>
   /// <param name="ordPos">A cartesian array of positions. [i][j][{x,y}]</param>
   Dictionary<string,PE[]> CreateJoints(int currGInx, dbl[][][] ordPos);
   /// <summary>Custom logic here depends on the way the blocks are joined together. Do not forget to trim excess space.</summary>
   Element[] CreateElements(
   Dictionary<string,PE[][]> patches, Dictionary<string,PE[]> joints);
   /// <summary>Create constrained variables at desired positions.</summary>
   (Tnr uc, Constraints bits) CreateConstraints(Dictionary<string,PE[][]> patches,
   Dictionary<string,PE[]> joints, int nPos, int nVar);
}
}