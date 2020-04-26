namespace Fluid.Internals.Shapes {

/// <summary>A 2-ShapeP1T1 defines methods that operate between one permanent and one transient corner, changing their state.</summary>
/// <typeparam name="π1">Type of permanent Corner1.</typeparam>
/// <typeparam name="τ2">Type of transient Corner2.</typeparam>
public interface I2ShapeP1T1<π1,τ2> : I1ShapeP1<π1>, I1ShapeT1<τ2>
where π1 : class  where τ2 : class {
     
}
}