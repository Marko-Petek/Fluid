using System;
using System.Collections.Generic;
using Fluid.Internals.Networks.Nodes;
using Fluid.Internals.Networks.Shapes;

namespace Fluid.Internals.Collections {
/// <summary>A network is a dictionary of nodes and shapes.</summary>
/// <typeparam name="τ">Type of node weights.</typeparam>
/// <typeparam name="α">Type of algebra.</typeparam>
public interface INetwork<τ,α> {
   Dictionary<int,INode<τ,α>> Nodes { get; }
   Dictionary<int,IShape> Shapes { get; }
}
}