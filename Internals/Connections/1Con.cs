namespace Fluid.Internals.Connections {
   
/// <summary>1-connection is a type that defines methods that operate on a single node (data object) changing its state.</summary>
/// <typeparam name="τ">Node T type.</typeparam>
public abstract class Con<τ>  where τ : class {
   protected abstract τ? T { get; }

}
}