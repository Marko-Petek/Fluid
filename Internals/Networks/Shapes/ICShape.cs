using System.Collections.Generic;

namespace Fluid.Internals.Networks.Shapes {

/// <summary>A collection shape. Stores multiple nodes.</summary>
/// <typeparam name="τ">Type of stored nodes.</typeparam>
public interface ICShape<τ,α> : IDictionary<int,INode<τ,α>>
where τ : INode<τ,α> {
   

}
}