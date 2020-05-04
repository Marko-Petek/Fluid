using System;
using System.Collections.Generic;
using Fluid.Internals.Algebras;

namespace Fluid.Internals.Networks.Nodes {

   /// <summary>A type that can form networks with other Nodes. A weight is also an integral part of a node, but we don't include it as a property here because we need to split definitions according to class/struct to get the null checks functionality.</summary>
   /// <typeparam name="τ">Weight type.</typeparam>
   /// <typeparam name="α">Algebra type.</typeparam>
   public interface INode<τ,α> {
      /// <summary>Connections: viewed either as a shape grouping nodes, or as a node grouping shapes.</summary>
      IDictionary<int,HashSet<int>> Conns { get; }                   // (subnet ID, node IDs)
      /// <summary>Node weight. Null node weight is forbidden, that would be equivalent to no node at all. Any connections through that node would be void.</summary>
      τ Wght { get; }
   }

   /// <summary>A type that can form networks with other Nodes. We split the definitions of ClassNode (peers that are reference types) and StructNode (peers that are value types) to allow linter to track nullability.</summary>
   /// <typeparam name="τ">Peer type. Must be a nullable reference type.</typeparam>
   /// <typeparam name="α">Algebra between peers.</typeparam>
   public interface IRefNode<τ,α> : INode<τ,α>
   where τ : class, IEquatable<τ>, IComparable<τ>
   where α : IAlgebra<τ?>, new() {
      
   }

   /// <summary>A type that can form networks with other Nodes that hold reference types. "Unit" prefix means that the value it holds is a unit (simplifies algebra operations).</summary>
   /// <typeparam name="τ">Peer type. Must be a nullable reference type.</typeparam>
   /// <typeparam name="α">Algebra between peers.</typeparam>
   public interface IURefNode<τ,α> : IRefNode<τ,α>
   where τ : class, IEquatable<τ>, IComparable<τ>
   where α : IAlgebra<τ?>, new() {
      τ INode<τ,α>.Wght => NullA<τ,α>.O.Unit()!;        // Unit should never be defined as null.
   }

   /// <summary>A type that can form networks with other Nodes. We split the definitions of ClassNode (peers that are reference types) and StructNode (peers that are value types) to allow linter to track nullability.</summary>
   /// <typeparam name="τ">Peer type. Must be a non-nullable value type.</typeparam>
   /// <typeparam name="α">Algebra between peers.</typeparam>
   public interface IValNode<τ,α> : INode<τ,α>
   where τ : struct, IEquatable<τ>, IComparable<τ>
   where α : IAlgebra<τ>, new() {

   }

   /// <summary>A type that can form networks with other Nodes. We split the definitions of ClassNode (peers that are reference types) and StructNode (peers that are value types) to allow linter to track nullability.</summary>
   /// <typeparam name="τ">Peer type. Must be a non-nullable value type.</typeparam>
   /// <typeparam name="α">Algebra between peers.</typeparam>
   public interface IUStructNode<τ,α> : IValNode<τ,α>
   where τ : struct, IEquatable<τ>, IComparable<τ>
   where α : IAlgebra<τ>, new() {
      τ INode<τ,α>.Wght => NonNullA<τ,α>.O.Unit();
   }
}