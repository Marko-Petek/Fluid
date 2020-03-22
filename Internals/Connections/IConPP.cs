namespace Fluid.Internals.Connections {

  /// <summary>2-connection is a type that defines methods that operate between two nodes (data objects) changing their state. P connection is a connection between two permanent nodes.</summary>
  /// <typeparam name="τ">Node T type.</typeparam>
  /// <typeparam name="υ">Node U type</typeparam>
  public interface IConPP<τ,υ> : IConP<τ>  where τ : class  where υ : class {
   /// <summary>Permanent node 2.</summary>
   protected υ U { get; }
}
}