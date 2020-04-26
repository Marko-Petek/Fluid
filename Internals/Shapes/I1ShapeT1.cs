namespace Fluid.Internals.Shapes {

/// <summary>A 1-ShapeT1 defines methods that operate on a single transient object (corner), changing its state.</summary>
/// <typeparam name="τ1">Type of transient Corner1.</typeparam>
public interface I1ShapeT1<τ1>
where τ1 : class {
   /// <summary>Transient corner 1.</summary>
   τ1? Corner1 { get; }
   
}
}