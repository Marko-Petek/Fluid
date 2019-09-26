using System;
using System.Diagnostics;
using System.Text;

using CavityFlow = Fluid.Runnables.CavityFlow;
using Fluid.Internals.Development;
using IO = Fluid.Internals.IO;
using Fluid.Internals.Numerics;
using static Fluid.Internals.Toolbox;

namespace Fluid.Main {
class Program {
   static void Main(string[] args) {
      try {
         Initialize();
         CavityFlow.Entry.Point();
         R.R($"Define constants: {DefineConstants.ToString()}"); }
      catch(Exception exc) {
         R.R($"Exception occured: {exc.Message}");
         R.R($"Stack trace:{exc.StackTrace}");
         throw exc; }
      finally {
         R.R("Exiting application.");
         FileWriter.Flush(); }
   }
   static void Initialize() {
      System.Console.OutputEncoding = Encoding.UTF8;
      Writer = new IO.Console();
      FileReader = new IO.FileReader();
      FileWriter = new IO.FileWriter();
      R = new Reporter();
      Rng = new RandomNumberGen();
      AddReportIfDefined();
   }

   
   public static StringBuilder DefineConstants { get; } = new StringBuilder(100);

   [Conditional("REPORT")] static void AddReportIfDefined() {
      DefineConstants.Append(" REPORT ");
   }
}

}