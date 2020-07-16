using System;
using System.Diagnostics;
using Fluid.Internals.Numerics;
using static Fluid.Internals.Development.Reporter;

namespace Fluid.Internals {
public static class Toolbox {
   public static string DebugTag = "";
   static Tools T { get; } = new Tools();
   public static IO.FConsole Writer => T.Writer;
   public static IO.FileWriter FWriter => T.FileWriter;
   public static IO.FileReader FReader => T.FileReader;
   public static RNG Rng => T.Rng;
   public static void A(string str) => T.S.A(str);
   public static string Y(string str) => T.S.Y(str);

   /// <summary>Shortcut to Report method.</summary>
   /// <param name="s">Text to report.</param>
   [Conditional("REPORT")]
   public static void R(string s, VerbositySettings verbosity = VerbositySettings.Moderate)
      => T.Reporter.R(s, verbosity);

}
}