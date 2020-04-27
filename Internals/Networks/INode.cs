using System;
using System.Collections.Generic;
using Fluid.Internals.Networks.Shapes;

namespace Fluid.Internals.Networks {

/// <summary>A type that can form networks with other Nodes. A weight is also an integral part of a node, but we don't include it as a property here because we need to split definitions according to class/struct to get the null checks functionality.</summary>
/// <typeparam name="τ">Peer type.</typeparam>
/// <typeparam name="α">Algebra between peers.</typeparam>
public interface INode<τ,α> {

   /// <summary>Peers, that is, neighboring nodes.</summary>
   IEnumerable<INode<τ,α>> Peers {get; }
   /// <summary>Node weight. Null node weight is forbidden, that would be equivalent to no node at all. Any connections through that node would be void.</summary>
   τ Wght { get; }
}
}