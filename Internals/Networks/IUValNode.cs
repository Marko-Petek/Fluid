using System;
using System.Collections.Generic;
using Fluid.Internals.Algebras;
using Fluid.Internals.Networks.Shapes;

namespace Fluid.Internals.Networks {

/// <summary>A type that can form networks with other Nodes. We split the definitions of ClassNode (peers that are reference types) and StructNode (peers that are value types) to allow linter to track nullability.</summary>
/// <typeparam name="τ">Peer type. Must be a non-nullable value type.</typeparam>
/// <typeparam name="α">Algebra between peers.</typeparam>
public interface IUStructNode<τ,α> : IValNode<τ,α>
where τ : struct, IEquatable<τ>, IComparable<τ>
where α : IAlgebra<τ>, new() {

   /// <summary>Peers, that is, neighboring nodes.</summary>
   new IEnumerable<IValNode<τ,α>> Peers {get; }

   τ INode<τ,α>.Wght => NonNullA<τ,α>.O.Unit();
}
}