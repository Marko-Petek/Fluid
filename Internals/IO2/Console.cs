using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using Fluid.Internals.Connections;
using static Fluid.Internals.Tools;
namespace Fluid.Internals.IO {

/// <summary>Contains methods which write out nicely formatted values to console.</summary>
public class Console {
   public int BufferWidth => System.Console.IsOutputRedirected ? 125 : System.Console.BufferWidth;
   /// <summary>TextWriter belonging to System.Console.</summary>
   public TextWriter TW { get; set; }
   public static StringBuilder DefineConstants { get; } = new StringBuilder(100);

   public Console() {
      System.Console.OutputEncoding = Encoding.UTF8;
      TW = System.Console.IsOutputRedirected ? new DebugTextWriter() : System.Console.Out;
      WriteLine($"OutputRedirected: {System.Console.IsOutputRedirected}");
      AddReportIfDefined();
      WriteLine($"Define constants: {DefineConstants.ToString()}");                    // This has to be relayed without relying on Reporter.
   }

   [Conditional("REPORT")] static void AddReportIfDefined() {
      DefineConstants.Append(" REPORT ");
   }

   
}
}