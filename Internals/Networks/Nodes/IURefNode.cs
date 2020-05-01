using System;
using System.Collections.Generic;
using Fluid.Internals.Algebras;
using Fluid.Internals.Networks.Shapes;

namespace Fluid.Internals.Networks.Nodes {

/// <summary>A type that can form networks with other Nodes that hold reference types. "Unit" prefix means that the value it holds is a unit (simplifies algebra operations).</summary>
/// <typeparam name="τ">Peer type. Must be a nullable reference type.</typeparam>
/// <typeparam name="α">Algebra between peers.</typeparam>
public interface IUClassNode<τ,α> : IRefNode<τ,α>
where τ : class, IEquatable<τ>, IComparable<τ>
where α : IAlgebra<τ?>, new() {

  τ INode<τ,α>.Wght => NullA<τ,α>.O.Unit()!;        // Unit should never be defined as null.
   
}
}