namespace Fluid.Internals.Connections {

  /// <summary>Connection between a P node and a T node.</summary>
  /// <typeparam name="τ">Node T type.</typeparam>
  /// <typeparam name="υ">Node U type</typeparam>
  public interface IConPT<τ,υ> : IConP<τ>, IConT<υ>  where τ : class  where υ : class {
     
}
}