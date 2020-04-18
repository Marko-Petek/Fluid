using System;
using System.Collections.Generic;
using Fluid.Internals.Algebras;
using Fluid.Internals.Connections;

namespace Fluid.Internals.Networks {

/// <summary>A type that can form networks with other Nodes.</summary>
/// <typeparam name="ω">Weight type. Must be a reference type.</typeparam>
/// <typeparam name="ωα">Weight algebra.</typeparam>
/// <typeparam name="λ">Connection length type. Must be a non-nullable value type.</typeparam>
/// <typeparam name="λα">Connection length algebra.</typeparam>
public interface ISpaceClassNode<ω,ωα,λ,λα> : IClassNode<ω,ωα>
where ω : class, IEquatable<ω>, IComparable<ω>
where ωα : IAlgebra<ω?>, new()
where λ : struct, IEquatable<λ>, IComparable<λ>
where λα : IAlgebra<λ>, new() {

   /// <summary>Neighboring nodes together with connection length.</summary>
   new IDictionary<ISpaceClassNode<ω,ωα,λ,λα>,λ> Peers { get; }


   // Redirection of parent property.
   IEnumerable<IClassNode<ω,ωα>> IClassNode<ω,ωα>.Peers => Peers.Keys;
   IEnumerable<INode<ω,ωα>> INode<ω,ωα>.Peers => Peers.Keys;


}
}