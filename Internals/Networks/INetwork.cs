using System;
using System.Collections.Generic;
using Fluid.Internals.Networks.Nodes;
using Fluid.Internals.Networks.Shapes;

namespace Fluid.Internals.Networks {
/// <summary>A network is a list of node dictionaries that represent subnetworks.</summary>
/// <typeparam name="τ">Type of weights.</typeparam>
/// <typeparam name="α">Type of algebra.</typeparam>
public interface INetwork<τ,α> : IList<INodeDict<τ,α>> {
   /// <summary>Network rank is how many subnetworks exist.</summary>
   int Rank => Count;
}
}