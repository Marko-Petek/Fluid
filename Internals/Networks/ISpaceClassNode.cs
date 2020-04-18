using System;
using System.Collections.Generic;
using Fluid.Internals.Algebras;
using Fluid.Internals.Connections;

namespace Fluid.Internals.Networks {

/// <summary>A type that can form networks with other Nodes.</summary>
/// <typeparam name="τ">Weight type. Must be a reference type.</typeparam>
/// <typeparam name="α">Weight algebra.</typeparam>
/// <typeparam name="σ">Connection length type. Must be a non-nullable value type.</typeparam>
/// <typeparam name="β">Connection length algebra.</typeparam>
public interface ISpaceClassNode<τ,α,σ,β> : IClassNode<τ,α>
where τ : class, IEquatable<τ>, IComparable<τ>
where α : IAlgebra<τ?>, new()
where σ : struct, IEquatable<σ>, IComparable<σ>
where β : IAlgebra<τ>, new() {

   /// <summary>Neighboring nodes together with connection length.</summary>
   new IDictionary<ISpaceClassNode<τ,α,σ,β>,σ> Peers { get; }


   // Redirection of parent property.
   IEnumerable<IClassNode<τ,α>> IClassNode<τ,α>.Peers => Peers.Keys;
   IEnumerable<INode<τ,α>> INode<τ,α>.Peers => Peers.Keys;


}
}