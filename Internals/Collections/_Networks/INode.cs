using System;
using System.Collections.Generic;
using Fluid.Internals.Connections;

namespace Fluid.Internals.Collections {

/// <summary>A type that can form networks with other Nodes.</summary>
public interface INode<τ> {
   IEnumerable<INode<τ>> Peers {get; }
}
}