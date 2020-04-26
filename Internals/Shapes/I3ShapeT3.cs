namespace Fluid.Internals.Shapes {
/// <summary>A 3-ShapeT3 defines methods that operate between three transient objects (corners), changing their state.</summary>
/// <typeparam name="τ1">Type of transient Corner1.</typeparam>
/// <typeparam name="τ2">Type of transient Corner2.</typeparam>
/// <typeparam name="τ3">Type of transient Corner3.</typeparam>
public interface I3ShapeT3<τ1,τ2,τ3> : I2ShapeT2<τ1,τ2>
where τ1 : class   where τ2 : class   where τ3 : class {
   /// <summary>Transient corner 3.</summary>
   τ3? Corner3 { get; }

}
}