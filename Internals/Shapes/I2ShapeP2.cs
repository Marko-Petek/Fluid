namespace Fluid.Internals.Shapes {

/// <summary>A 2-ShapeP2 defines methods that operate between two permanent objects (corners), changing their state.</summary>
/// <typeparam name="π1">Type of permanent corner 1.</typeparam>
/// <typeparam name="π2">Type of permanent corner 2.</typeparam>
public interface I2ShapeP2<π1,π2> : I1ShapeP1<π1>
where π1 : class  where π2 : class {
   /// <summary>Permanent corner 2.</summary>
   π2 Corner2 { get; }
}
}