namespace Fluid.Internals.Shapes {

/// <summary>A 3-ShapeP2T1 defines methods that operate between three corners (2 permanent, 1 transient), changing their state.</summary>
/// <typeparam name="π1">Type of permanent Corner1.</typeparam>
/// <typeparam name="π2">Type of permanent Corner2.</typeparam>
/// <typeparam name="τ3">Type of transient Corner3.</typeparam>
public interface I3ShapeP2T1<π1,π2,τ3> : I2ShapeP2<π1,π2>
where π1 : class  where π2 : class  where τ3 : class {
   /// <summary>Transient corner 3.</summary>
   τ3? Corner3 { get; }

}
}