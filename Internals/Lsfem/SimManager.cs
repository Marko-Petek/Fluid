using System;
namespace Fluid.Internals.Lsfem {
   /// <summary>Singleton than manages the Sim.</summary>
   public class SimManager {
      /// <summary>The one and only SimManager instance.</summary>
      public static SimManager I { get; }
      /// <summary>Associated simulation.</summary>
      public Sim Sim { get; }

      static SimManager() {
         I = new SimManager();
      }
   }
}