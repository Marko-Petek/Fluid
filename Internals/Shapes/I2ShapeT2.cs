namespace Fluid.Internals.Shapes {

/// <summary>A 2-ShapeT2 defines methods that operate between two transient objects (corners), changing their state.</summary>
/// <typeparam name="τ1">Type of transient Corner1.</typeparam>
/// <typeparam name="τ2">Type of transient Corner2.</typeparam>
public interface I2ShapeT2<τ1,τ2> : I1ShapeT1<τ1>
where τ1 : class  where τ2 : class {
   /// <summary>Transient corner 2.</summary>
   τ2? Corner2 { get; }

}
}