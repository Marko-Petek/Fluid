using System;
using System.Collections.Generic;
using Fluid.Internals.Algebras;
using Fluid.Internals.Connections;

namespace Fluid.Internals.Networks {

/// <summary>A type that can form networks with other Nodes. We split the definitions of ClassNode (peers that are reference types) and StructNode (peers that are value types) to allow linter to track nullability.</summary>
/// <typeparam name="τ">Peer type. Must be a non-nullable value type.</typeparam>
/// <typeparam name="α">Algebra between peers.</typeparam>
public interface IMetricClassNode<τ,α> : IClassNode<τ,α>
where τ : class, IEquatable<τ>, IComparable<τ>
where α : IAlgebra<τ?>, new() {

   /// <summary>Peers, that is, neighboring nodes. Available via a metric (for now only integer metric).</summary>
   new IDictionary<int,IMetricClassNode<τ,α>> Peers { get; }


   // Redirection of parent property.
   IEnumerable<IClassNode<τ,α>> IClassNode<τ,α>.Peers => Peers.Values;
   IEnumerable<INode<τ,α>> INode<τ,α>.Peers => Peers.Values;


}
}