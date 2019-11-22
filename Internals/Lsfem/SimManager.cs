#nullable enable
using dbl = System.Double;
using System;
using Fluid.Internals.Numerics;
using DA = Fluid.Internals.Numerics.DblArithmetic;
namespace Fluid.Internals.Lsfem {
using Tnr = Fluid.Internals.Collections.Tensor<dbl, DA>;

/// <summary>Singleton than manages the Sim.</summary>
public abstract class SimManager<σ>
where σ : Sim {
   public static SimManager<σ> I { get; }
   static Func<ISimInit, SimManager<σ>> Factory { get; set; }
   /// <summary>Associated simulation.</summary>
   abstract public σ Sim { get; }
   /// <summary>Solver that solves the linear system using Conjugate Gradients.</summary>
   abstract protected ConjGradsSolver Solver { get; }
   public double Epsilon { get; protected set; }

   /// <summary>Override to create Sim within.</summary>
   /// <param name="simFactory"></param>
   protected SimManager(Func<ISimInit, SimManager<σ>> simFactory, dbl epsilon) {
      Factory = FactoryMethod;
      Epsilon = epsilon;
   }

   /// <summary>Creates a new SimManager if one does not already exists, or else, returns the existing one.</summary>
   /// <param name="si"></param>
   public static SimManager<σ> Create(ISimInit si) =>
      (I) switch {
         SimManager<σ> sm => sm,
         _ => Factory(si) };
   
   /// <summary>Override to return a SimManager with a particular type σ : Sim.</summary>
   /// <param name="si"></param>
   abstract protected SimManager<σ> FactoryMethod(ISimInit si);
   /// <summary></summary>
   /// <param name="u0"></param>
   public Tnr AdvanceDynamics(Tnr u0) =>
      Solver.Solve(u0, Epsilon);
   
}
}
#nullable restore