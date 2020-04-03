using System;
using System.Text;
using static System.Console;
using SCG = System.Collections.Generic;

using Fluid.Internals.Networks;
using Fluid.Internals.Development;
using static Fluid.Internals.Toolbox;

namespace Fluid.Runnables.ChannelFlow {    
public static class Entry {
   static int Point() {
      R("Writing solution to file.");
      FileWriter.SetDirAndFile("ChannelFlow/Results", "solution", ".txt");
      return 0;
   }
}
}