using System;

using static Fluid.Internals.Toolbox;
using static Fluid.Internals.Development.Reporter;

namespace CavityFlow {
   /// <summary>Driven cavity flow problem is a very simple problem with which methods are initially tested.</summary>
   class Program {
      static void Main() => EntryPointSetup("Starting CavityFlow.",
         () => Run1(), VerbositySettings.Moderate);

      static void Run1() {
      }
   }
}
