#nullable enable
using System;
using Fluid.Internals.Numerics;
namespace Fluid.Internals.Lsfem {
/// <summary>Singleton than manages the Sim.</summary>
public abstract class SimManager<σ>
where σ : Sim {
   static SimManager<σ> I { get; }
   static Func<ISimInit, SimManager<σ>> Factory { get; set; }
   /// <summary>Associated simulation.</summary>
   σ Sim { get; }
   /// <summary>Solver that solves the linear system using Conjugate Gradients.</summary>
   ConjGradsSolver Solver { get; }


   private SimManager(Func<ISimInit, SimManager<σ>> simFactory) {
      Factory = FactoryMethod;
   }


   public static SimManager<σ> Create(ISimInit si) =>
      (I) switch {
         SimManager<σ> sm => sm,
         _ => Factory(si) };
   
   abstract protected SimManager<σ> FactoryMethod(ISimInit si);
}
}
#nullable restore