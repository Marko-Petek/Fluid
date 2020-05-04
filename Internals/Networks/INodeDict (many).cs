using System;
using System.Collections.Generic;
using Fluid.Internals.Networks.Nodes;
using Fluid.Internals.Algebras;

namespace Fluid.Internals.Networks {

/// <summary>Dictionary: ID ~~> Node.</summary>
/// <typeparam name="τ">Weight type.</typeparam>
/// <typeparam name="α">Weight algebra type.</typeparam>
public interface INodeDict<τ,α> : IDictionary<int,INode<τ,α>> {

}
/// <summary>Dictionary: ID ~~> Node.</summary>
/// <typeparam name="τ">Weight type.</typeparam>
/// <typeparam name="α">Weight algebra type.</typeparam>
public interface IRefNodeDict<τ,α> : IDictionary<int,IRefNode<τ,α>>
where τ : class, IEquatable<τ>, IComparable<τ>
where α : IAlgebra<τ?>, new() {

}
/// <summary>Dictionary: ID ~~> Node.</summary>
/// <typeparam name="τ">Weight type.</typeparam>
/// <typeparam name="α">Weight algebra type.</typeparam>
public interface IValNodeDict<τ,α> : IDictionary<int,IValNode<τ,α>>
where τ : struct, IEquatable<τ>, IComparable<τ>
where α : IAlgebra<τ>, new() {

}
}