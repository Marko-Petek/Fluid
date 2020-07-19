using System;
using System.Linq;
using System.Diagnostics;
using static Fluid.Internals.Toolbox;
namespace Fluid.Runnables.Fiddle {

   /// <summary>Fiddle with stuff.</summary>
public static class Entry {
   public static int Point() {
      
      Reporter.WriteLine();
      Console.WriteLine("     x ");
      Console.WriteLine("    xxx ");
      Console.WriteLine("   xxxxx");
      Console.WriteLine("  xxxxxxx");
      Console.WriteLine(" xxxxxxxxx");
      Reporter.WriteLine();

      return 0;
   }

}

}
