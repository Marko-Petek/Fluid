using System;
using System.Collections.Generic;
using Fluid.Internals.Algebras;
using Fluid.Internals.Connections;

namespace Fluid.Internals.Networks {

/// <summary>A type that can form networks with other Nodes. We split the definitions of ClassNode (peers that are reference types) and StructNode (peers that are value types) to allow linter to track nullability.</summary>
/// <typeparam name="τ">Peer type. Must be a nullable reference type.</typeparam>
/// <typeparam name="α">Algebra between peers.</typeparam>
public interface IStubClassNode<τ,α> : IClassNode<τ,α>
where τ : class, IEquatable<τ>, IComparable<τ>
where α : IAlgebra<τ?>, new() {
   /// <summary>Stubs superior.</summary>
   IClassNode<τ,α> Sup { get; }
   /// <summary>Peers are now nodes with no other connections (equivalent to vector).</summary>
   new IEnumerable<τ> Peers { get; }

   // Redirection of parent property to an exception.
   IEnumerable<INode<τ,α>> INode<τ,α>.Peers => throw new InvalidOperationException($"You tried to access peer nodes of a stub node through the {nameof(INode<τ,α>)} interface. A stub nodes has only values as peers, not other proper nodes.");
}
}