namespace Fluid.Internals.Shapes {

/// <summary>A 1-ShapeP1 defines methods that operate on a single permanent object (corner), changing its state.</summary>
/// <typeparam name="π1">Type of permanent Corner1.</typeparam>
public interface I1ShapeP1<π1> 
where π1 : class {
   /// <summary>Permanent corner 1.</summary>
   π1 Corner1 { get; }

}
}