using System;
using System.Collections.Generic;
using Fluid.Internals.Algebras;
using Fluid.Internals.Shapes;

namespace Fluid.Internals.Networks {

/// <summary>A type that can form networks with other Nodes.</summary>
/// <typeparam name="ω">Weight type. Must be a non-nullable value type.</typeparam>
/// <typeparam name="ωα">Weight algebra.</typeparam>
/// <typeparam name="ω">Connection length type. Must be a non-nullable value type.</typeparam>
/// <typeparam name="ωα">Connection length algebra.</typeparam>
public interface ISpaceStructNode<ω,ωα,λ,λα> : IStructNode<ω,ωα>
where ω : struct, IEquatable<ω>, IComparable<ω>
where ωα : IAlgebra<ω>, new()
where λ : struct, IEquatable<λ>, IComparable<λ>
where λα : IAlgebra<λ>, new() {
   /// <summary>Peers, that is, neighboring nodes. Available via integer metric.</summary>
   new IDictionary<ISpaceStructNode<ω,ωα,λ,λα>,λ> Peers { get; }

   // Redirection of parent property.
   IEnumerable<IStructNode<ω,ωα>> IStructNode<ω,ωα>.Peers => Peers.Keys;
   IEnumerable<INode<ω,ωα>> INode<ω,ωα>.Peers => Peers.Keys;


}
}