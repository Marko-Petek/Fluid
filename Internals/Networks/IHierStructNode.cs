using System;
using System.Collections.Generic;
using Fluid.Internals.Algebras;
using Fluid.Internals.Networks.Shapes;

namespace Fluid.Internals.Networks {

/// <summary>A type that can form networks with other Nodes. We split the definitions of ClassNode (peers that are reference types) and StructNode (peers that are value types) to allow linter to track nullability.</summary>
/// <typeparam name="τ">Peer type. Must be a non-nullable value type.</typeparam>
/// <typeparam name="α">Algebra between peers.</typeparam>
public interface IHierStructNode<τ,α> : IValNode<τ,α>
where τ : struct, IEquatable<τ>, IComparable<τ>
where α : IAlgebra<τ>, new() {
   /// <summary>Stubs superior.</summary>
   IHierStructNode<τ,α> Sup { get; }
   /// <summary>Subordinates.</summary>
   IEnumerable<IHierStructNode<τ,α>> Subs { get; }

   new IEnumerable<IHierStructNode<τ,α>> Peers {
      get {
         yield return Sup;
         foreach(var sub in Subs)
            yield return sub; }
   }

   // Redirection of parent Peers.
   IEnumerable<IValNode<τ,α>> IValNode<τ,α>.Peers => Peers;
   // Redirection of parent property to an exception.
   IEnumerable<INode<τ,α>> INode<τ,α>.Peers => throw new InvalidOperationException($"You tried to access peer nodes of a stub node through the {nameof(INode<τ,α>)} interface. A stub nodes has only values as peers, not other proper nodes.");
}
}