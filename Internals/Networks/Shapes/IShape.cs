using System.Collections.Generic;

namespace Fluid.Internals.Networks.Shapes {

/// <summary>A collection shape. Stores multiple nodes.</summary>
/// <typeparam name="τ">Type of weights.</typeparam>
/// <typeparam name="τ">Type of algebra.</typeparam>
public interface IShape<τ,α> {
   /// <summary>Node IDs.</summary>
   List<int> NIDs { get; }
   /// <summary>Shape weight.</summary>
   τ Wght { get; }
}
}