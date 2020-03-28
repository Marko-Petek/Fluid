using System;
using System.Collections.Generic;
using Fluid.Internals.Connections;

namespace Fluid.Internals.Collections {

/// <summary>A type that can form networks with other Nodes. A weight is also an integral part of a node, but we don't include it as a property here because we need to split definitions according to class/struct to get the null checks functionality.</summary>
public interface INode<τ> {
   IEnumerable<INode<τ>> Peers {get; }
}
}