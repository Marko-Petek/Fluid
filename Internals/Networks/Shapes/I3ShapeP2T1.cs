namespace Fluid.Internals.Networks.Shapes {

/// <summary>A 3-ShapeP2T1 defines methods that operate between three nodes (2 permanent, 1 transient), changing their state.</summary>
/// <typeparam name="π1">Type of permanent node 1.</typeparam>
/// <typeparam name="π2">Type of permanent node 2.</typeparam>
/// <typeparam name="τ3">Type of transient node 3.</typeparam>
public interface I3ShapeP2T1<π1,π2,τ3> : I2ShapeP2<π1,π2>
where π1 : class  where π2 : class  where τ3 : class {
   /// <summary>Transient corner 3.</summary>
   τ3? Node3 { get; }

}
}