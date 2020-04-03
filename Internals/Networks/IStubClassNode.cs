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

   /// <summary>Peers are now nodes with no other connections (equivalent to vector).</summary>
   new IEnumerable<τ> Peers { get; }

}
}