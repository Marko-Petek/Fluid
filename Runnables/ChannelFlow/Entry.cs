using System;
using System.Text;
using static System.Console;
using SCG = System.Collections.Generic;

using Fluid.Internals.Collections;
using Fluid.Internals.Development;
using TB = Fluid.Internals.Toolbox;
using static Fluid.Internals.Development.Reporter;

namespace Fluid.Runnables.ChannelFlow {    
   public static class Entry {
      static void Point() {
         // var system = new ChannelCylinderSystem(1.0, 0.05, 0.001);    // Start with velocity 1.0.
         // TB.Reporter.Write("Solving for a single time step and writing solution to NodeArray.", VerbositySettings.Moderate);
         // system.SolveNextAndAddToNodeArray();
         TB.R.R("Writing solution to file.");
         TB.FileWriter.SetDirAndFile("ChannelFlow/Results", "solution", ".txt");
         //TB.FileWriter.Write<SCG.IEnumerable<SCG.KeyValuePair<int,double>>>(system.SolVec);
      }
   }
}