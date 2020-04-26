namespace Fluid.Internals.Shapes {

/// <summary>A 3-ShapeP3 defines methods that operate between three transient objects (corners), changing their state.</summary>
/// <typeparam name="π1">Type of permanent corner 1.</typeparam>
/// <typeparam name="π2">Type of permanent corner 2.</typeparam>
/// <typeparam name="π3">Type of permanent corner 3.</typeparam>
public interface I3ShapeP3<π1,π2,π3> : I2ShapeP2<π1,π2>
where π1 : class  where π2 : class  where π3 : class {
   /// <summary>Permanent corner 3.</summary>
   π3 Corner3 { get; }

}
}