using System;
using System.Collections.Generic;
using Fluid.Internals.Algebras;
using Fluid.Internals.Connections;

namespace Fluid.Internals.Networks {

/// <summary>A type that can form networks with other Nodes that hold reference types. "Unit" prefix means that the value it holds is a unit (simplifies algebra operations).</summary>
/// <typeparam name="τ">Peer type. Must be a nullable reference type.</typeparam>
/// <typeparam name="α">Algebra between peers.</typeparam>
public interface IUnitClassNode<τ,α> : IClassNode<τ,α>
where τ : class, IEquatable<τ>, IComparable<τ>
where α : IAlgebra<τ?>, new() {

  new τ Wght => Nullable<τ,α>.O.Unit()!;        // Unit should never be defined as null.
   
}
}