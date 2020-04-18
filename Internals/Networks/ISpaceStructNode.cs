using System;
using System.Collections.Generic;
using Fluid.Internals.Algebras;
using Fluid.Internals.Connections;

namespace Fluid.Internals.Networks {

/// <summary>A type that can form networks with other Nodes.</summary>
/// <typeparam name="τ">Weight type. Must be a non-nullable value type.</typeparam>
/// <typeparam name="α">Weight algebra.</typeparam>
public interface ISpaceStructNode<τ,α,σ,β> : IStructNode<τ,α>
where τ : struct, IEquatable<τ>, IComparable<τ>
where α : IAlgebra<τ>, new()
where σ : struct, IEquatable<σ>, IComparable<σ>
where β : IAlgebra<τ>, new() {

   /// <summary>Peers, that is, neighboring nodes. Available via integer metric.</summary>
   new IDictionary<ISpaceStructNode<τ,α,σ,β>,σ> Peers { get; }


   // Redirection of parent property.
   IEnumerable<IStructNode<τ,α>> IStructNode<τ,α>.Peers => Peers.Keys;
   IEnumerable<INode<τ,α>> INode<τ,α>.Peers => Peers.Keys;


}
}