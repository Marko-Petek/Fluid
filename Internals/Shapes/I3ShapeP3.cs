namespace Fluid.Internals.Shapes {

/// <summary>A 3-ShapeP3 defines methods that operate between three transient objects (nodes), changing their state.</summary>
/// <typeparam name="π1">Type of permanent node 1.</typeparam>
/// <typeparam name="π2">Type of permanent node 2.</typeparam>
/// <typeparam name="π3">Type of permanent node 3.</typeparam>
public interface I3ShapeP3<π1,π2,π3> : I2ShapeP2<π1,π2>
where π1 : class  where π2 : class  where π3 : class {
   /// <summary>Permanent node 3.</summary>
   π3 Node3 { get; }

}
}