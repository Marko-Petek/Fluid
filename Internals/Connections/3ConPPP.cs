namespace Fluid.Internals.Connections {
  /// <summary>3-connection is a type that defines methods that can operate between three nodes (data objects) changing their state.</summary>
  /// <typeparam name="τ">Node T type.</typeparam>
  /// <typeparam name="υ">Node U type</typeparam>
  /// <typeparam name="φ">Node V type</typeparam>
  public abstract class ConPPP<τ,υ,φ> : Con<τ,υ,φ>  where τ : class  where υ : class  where φ : class {
   protected override τ T { get; }
   protected override υ U { get; }
   protected override φ V { get; }

}
}