namespace Fluid.Internals.Connections {
public interface IConP<τ>  where τ : class {
   
   /// <summary>Permanent node.</summary>
   protected τ T { get; }
}

}