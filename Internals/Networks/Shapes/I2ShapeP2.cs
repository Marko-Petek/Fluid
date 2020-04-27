namespace Fluid.Internals.Networks.Shapes {

/// <summary>A 2-ShapeP2 defines methods that operate between two permanent objects (nodes), changing their state.</summary>
/// <typeparam name="π1">Type of permanent node 1.</typeparam>
/// <typeparam name="π2">Type of permanent node 2.</typeparam>
public interface I2ShapeP2<π1,π2> : I1ShapeP1<π1>
where π1 : class  where π2 : class {
   /// <summary>Permanent node 2.</summary>
   π2 Node2 { get; }
}
}