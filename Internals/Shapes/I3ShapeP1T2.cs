namespace Fluid.Internals.Shapes {

/// <summary>A 3-ShapeP1T2 defines methods that operate between three corners (1 permanent, 2 transient), changing their state.</summary>
/// <typeparam name="π1">Type of permanent Corner1.</typeparam>
/// <typeparam name="τ2">Type of transient Corner2.</typeparam>
/// <typeparam name="τ3">Type of transient Corner3.</typeparam>
public interface I3ShapeP1T2<π1,τ2,τ3> : I2ShapeP1T1<π1,τ2>
where π1 : class  where τ2 : class  where τ3 : class {
   /// <summary>Transient corner 3.</summary>
   τ3? Corner3 { get; }
   
}
}