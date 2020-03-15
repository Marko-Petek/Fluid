namespace Fluid.Internals.Connections {
public abstract class ConP<τ> : Con<τ>  where τ : class {
   
   /// <summary>Permanent node.</summary>
   protected override τ T { get; }
   
   public ConP()
}

}