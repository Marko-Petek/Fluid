namespace Fluid.Internals.Connections {

  /// <summary>Connection between two T nodes.</summary>
  /// <typeparam name="τ">Node T type.</typeparam>
  /// <typeparam name="υ">Node U type</typeparam>
  public interface IConTT<τ,υ> : IConT<τ>  where τ : class  where υ : class {
   /// <summary>Transient node 2.</summary>
   protected υ? U { get; }
}
}