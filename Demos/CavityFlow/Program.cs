using System;

namespace CavityFlow {
   /// <summary>Driven cavity flow problem is a very simple problem with which methods are initially tested.</summary>
   class Program {
      static void Main() => TB.EntryPointSetup("Starting CavityFlow.",
         () => Config01(), VerbositySettings.Moderate);

      static void Config01() {
         var system = new ChannelCylinderSystem(1.0, 0.05, 0.001);    // Start with velocity 1.0.
         TB.Reporter.Write("Solving for a single time step and writing solution to NodeArray.", VerbositySettings.Moderate);
         system.SolveNextAndAddToNodeArray();
         TB.Reporter.Write("Writing solution to file.");
         TB.FileWriter.SetDirAndFile("ChannelFlow/Results", "solution", ".txt");
         //TB.FileWriter.Write<SCG.IEnumerable<SCG.KeyValuePair<int,double>>>(system.SolVec);
      }
   }
}
