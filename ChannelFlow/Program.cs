using System;
using System.Text;
using static System.Console;

using Fluid.Internals.Collections;
using Fluid.Internals.Development;
using TB = Fluid.Internals.Toolbox;

namespace Fluid.ChannelFlow {    
   class Program {
      static void Main(string[] args) => TB.EntryPointSetup(() => Config01());

      static void Config01() {
         var flow = new ChannelFlow(1.0, 0.05, 0.001);    // Start with velocity 1.0.
         TB.Reporter.Write("Solving for a single time step and writing solution to NodeArray.");
         flow.SolveNextAndAddToNodeArray();
         TB.Reporter.Write("Writing solution to file.");
         flow.WriteSolution(5);
      } 
   }
}