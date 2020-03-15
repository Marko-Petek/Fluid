namespace Fluid.Internals.Connections {

  /// <summary>Connection between two T nodes.</summary>
  /// <typeparam name="τ">Node T type.</typeparam>
  /// <typeparam name="υ">Node U type</typeparam>
  public abstract class ConTT<τ,υ> : Con<τ,υ>  where τ : class  where υ : class {
   /// <summary>Transient node 1.</summary>
   protected override τ? T { get; }
   /// <summary>Transient node 2.</summary>
   protected override υ? U { get; }
}
}