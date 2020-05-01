using System;
using System.Collections.Generic;
using Fluid.Internals.Algebras;
using Fluid.Internals.Networks.Shapes;

namespace Fluid.Internals.Networks.Nodes {

/// <summary>A type that can form networks with other Nodes. We split the definitions of ClassNode (peers that are reference types) and StructNode (peers that are value types) to allow linter to track nullability.</summary>
/// <typeparam name="τ">Peer type. Must be a nullable reference type.</typeparam>
/// <typeparam name="α">Algebra between peers.</typeparam>
public interface IRefNode<τ,α> : INode<τ,α>
where τ : class, IEquatable<τ>, IComparable<τ>
where α : IAlgebra<τ?>, new() {

   /// <summary>Peers, that is, neighboring nodes.</summary>
   new IEnumerable<IRefNode<τ,α>> Peers { get; }

   // Redirection of parent property.
   IEnumerable<INode<τ,α>> INode<τ,α>.Peers => Peers;
}
}