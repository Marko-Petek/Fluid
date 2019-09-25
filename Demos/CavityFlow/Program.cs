using System;

using static Fluid.Internals.Toolbox;
using static Fluid.Internals.Development.Reporter;

namespace Fluid.Demos.CavityFlow {
   /// <summary>Driven cavity flow problem is a very simple problem with which methods are initially tested.</summary>
   class Program {
      static void Main() {
         EntryPointSetup("Starting CavityFlow.",
            () => Run1(), VerbositySettings.Moderate);
         System.Console.WriteLine("Finished.");
      } 

      static void Run1() {
         var cavFlow = new CavityFlow(0.1, 0.1, 3) ;
      }
   }
}
