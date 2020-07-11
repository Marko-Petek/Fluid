#nullable disable
using System;
using System.Collections.Generic;

namespace Fluid.Internals.Networks {

/// <summary>A node in a network keyed by a value type key. Can form bonds with other Nodes.</summary>
/// <typeparam name="τ">Value type key used to identify a node in a network.</typeparam>
public interface INode<τ> {
   /// <summary>A network to which the node belongs.</summary>
   INetwork<τ> Network { get; }
   /// <summary>Node's unique ID inside network.</summary>
   τ Id { get; }
   /// <summary></summary>
   τ[] Bond { get; }

}
}
#nullable restore