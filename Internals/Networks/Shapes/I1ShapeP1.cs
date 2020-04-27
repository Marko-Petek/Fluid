namespace Fluid.Internals.Networks.Shapes {

/// <summary>A 1-ShapeP1 defines methods that operate on a single permanent object (node), changing its state.</summary>
/// <typeparam name="π1">Type of permanent Node1.</typeparam>
public interface I1ShapeP1<π1> 
where π1 : class {
   /// <summary>Permanent node 1.</summary>
   π1 Node1 { get; }

}
}