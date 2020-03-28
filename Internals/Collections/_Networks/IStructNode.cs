using System;
using System.Collections.Generic;
using Fluid.Internals.Connections;

namespace Fluid.Internals.Collections {

/// <summary>A type that can form networks with other Nodes. We split the definitions of ValNode and RefNode to allow intellisense to track nullability.</summary>
public interface IStructNode<τ> : INode<τ>  where τ : struct {
   /// <summary>Node weight.</summary>
   τ? Wght { get; }
}
}