using System;
using System.Linq;
using System.Diagnostics;
using static Fluid.Internals.Toolbox;
using Markdig;
namespace Fluid.Runnables.Markdig {

   /// <summary>Fiddle with stuff.</summary>
public static class Entry {
   public static int Point() {
      
      Reporter.WriteLine();
      var str = "A Markdown string with **emphasis**.";
      var res = Markdown.ToHtml(str);
      Console.WriteLine($"We are parsing the following string with Markdown to HTML:\n{str}");
      Console.WriteLine($"Our HTML is:\n{res}");
      // string word = Console.ReadLine();
      // switch (word) {
      //    case "ok": Console.WriteLine("Good."); break;
      //    case "no": Console.WriteLine("Why not?"); break;
      //    default: Console.WriteLine("Bye."); break; }
      Reporter.WriteLine();
      return 0;
   }

}

}
