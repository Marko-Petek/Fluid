using System;
using System.Text;
using Fluid.Internals.Development;
using IO = Fluid.Internals.IO;
using Fluid.Internals.Numerics;

using static Fluid.Internals.Development.AppReporter;

namespace Fluid.Internals {
   public static class Toolbox {
      public static Assert Assert { get; } = new Assert();
      public static IO.FileReader FileReader { get; } = new IO.FileReader();
      public static IO.FileWriter FileWriter { get; } = new IO.FileWriter();
      public static IO.Console Console { get; } = new IO.Console();
      public static AppReporter Reporter { get; } = new AppReporter(VerbositySettings.Moderate);
      public static Rng Rng { get; } = new Rng();

      public static void EntryPointSetup(Action main) {
         try {
            System.Console.OutputEncoding = Encoding.UTF8;
            main();
         }
         catch(Exception exc) {
            Reporter.Write($"Exception occured: {exc.Message}");
            Reporter.Write($"Stack trace:{exc.StackTrace}");
            throw exc;                                                      // Rethrow.
         }
         finally {
            Reporter.Write("Exiting application.");
            FileWriter.Flush();
         }
      }

      static Toolbox() {
         
      }
   }
}