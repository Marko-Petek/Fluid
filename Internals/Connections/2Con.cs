namespace Fluid.Internals.Connections {
  /// <summary>2-connection is a type that defines methods that operate between two nodes (data objects) changing their state.</summary>
  /// <typeparam name="τ">Node T type.</typeparam>
  /// <typeparam name="υ">Node U type</typeparam>
  public abstract class Con<τ,υ>  where τ : class  where υ : class {
   protected abstract τ? T { get; }
   protected abstract υ? U { get; }

}
}