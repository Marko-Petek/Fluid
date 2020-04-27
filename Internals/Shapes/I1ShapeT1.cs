namespace Fluid.Internals.Shapes {

/// <summary>A 1-ShapeT1 defines methods that operate on a single transient object (node), changing its state.</summary>
/// <typeparam name="τ1">Type of transient node 1.</typeparam>
public interface I1ShapeT1<τ1>
where τ1 : class {
   /// <summary>Transient node 1.</summary>
   τ1? Node1 { get; }
   
}
}