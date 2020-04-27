namespace Fluid.Internals.Networks.Shapes {
/// <summary>A 3-ShapeT3 defines methods that operate between three transient objects (nodes), changing their state.</summary>
/// <typeparam name="τ1">Type of transient node 1.</typeparam>
/// <typeparam name="τ2">Type of transient node 2.</typeparam>
/// <typeparam name="τ3">Type of transient node 3.</typeparam>
public interface I3ShapeT3<τ1,τ2,τ3> : I2ShapeT2<τ1,τ2>
where τ1 : class   where τ2 : class   where τ3 : class {
   /// <summary>Transient node 3.</summary>
   τ3? Node3 { get; }

}
}