namespace Fluid.Internals.Connections {

  /// <summary>Connection between two P nodes.</summary>
  /// <typeparam name="τ">Node T type.</typeparam>
  /// <typeparam name="υ">Node U type</typeparam>
  public abstract class ConPP<τ,υ> : Con<τ,υ>  where τ : class  where υ : class {
   /// <summary>Permanent node 1.</summary>
   protected override τ T { get; }
   /// <summary>Permanent node 2.</summary>
   protected override υ U { get; }
}
}