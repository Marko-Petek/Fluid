namespace Fluid.Internals.Connections {
public interface IConT<τ>  where τ : class {
   
   /// <summary>Permanent node.</summary>
   protected τ? T { get; }
   
}
}