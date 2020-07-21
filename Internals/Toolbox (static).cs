using System;
using System.Diagnostics;

using Fluid.Internals.Development;
using Fluid.Internals.IO;
using Fluid.Internals.Numerics;
using static Fluid.Internals.Development.Reporter;

namespace Fluid.Internals {
public static class Toolbox {
   public static string DebugTag = "";
   static Tools T { get; } = new Tools();
   public static FConsole Writer => T.Writer;
   public static FileWriter FWriter => T.FileWriter;
   public static FileReader FReader => T.FileReader;
   public static RNG Rng => T.Rng;
   public static void A(string str) => T.S.A(str);
   public static string Y(string str) => T.S.Y(str);

   public static Reporter Reporter => T.Reporter;

   /// <summary>Shortcut to Report method.</summary>
   /// <param name="s">Text to report.</param>
   [Conditional("REPORT")]
   public static void R(string s, VerbositySettings verbosity = VerbositySettings.Moderate)
      => T.Reporter.R(s, verbosity);

}
}