using System;
using System.Collections.Generic;
using Fluid.Internals.Algebras;
using Fluid.Internals.Connections;

namespace Fluid.Internals.Networks {

/// <summary>A type that can form networks with other Nodes. We split the definitions of ClassNode (peers that are reference types) and StructNode (peers that are value types) to allow linter to track nullability.</summary>
/// <typeparam name="τ">Peer type. Must be a nullable reference type.</typeparam>
/// <typeparam name="α">Algebra between peers.</typeparam>
public interface IMetricStubClassNode<τ,α> : IMetricClassNode<τ,α>
where τ : class, IEquatable<τ>, IComparable<τ>
where α : IAlgebra<τ?>, new() {

   /// <summary>Peers are now nodes with no other connections (equivalent to vector).</summary>
   new IDictionary<int,τ> Peers { get; }


   // Redirection of parent property to an exception.
   IEnumerable<INode<τ,α>> INode<τ,α>.Peers => throw new InvalidOperationException($"You tried to access peer nodes of a stub node through the {nameof(INode<τ,α>)} interface. A stub node has only values as peers, not other proper nodes.");
   IDictionary<int,IMetricClassNode<τ,α>> IMetricClassNode<τ,α>.Peers => throw new InvalidOperationException($"You tried to access peer nodes of a stub node through the {nameof(IMetricClassNode<τ,α>)} interface. A stub node has only values as peers, not other proper nodes.");
}
}