using System;
using System.Collections.Generic;
using Fluid.Internals.Networks.Nodes;
using Fluid.Internals.Networks.Shapes;

namespace Fluid.Internals.Networks {
/// <summary>A network is a list of node dictionaries that represent subnetworks.</summary>
/// <typeparam name="τ">Type of weights.</typeparam>
/// <typeparam name="α">Type of algebra.</typeparam>
public class Network<τ,α> : List<INodeDict<τ,α>>, INetwork<τ,α> {
   
}
}