using System;
using System.Diagnostics;
using System.Text;

using Tests = Fluid.Tests;
using CavityFlow = Fluid.Runnables.CavityFlow;

using Fluid.Internals.Development;
using IO = Fluid.Internals.IO;
using Fluid.Internals.Numerics;
using Fluid.Internals.Text;
using static Fluid.Internals.Toolbox;

namespace Fluid.Main {
/// <summary>For any program of Fluid, execution starts here. This is a perfect point for initialization of utilities inside Toolbox.</summary>
class Program {
   static void Main(string[] args) {            // args[0] has to be the name of the project without the extension.
      try {
         Initialize();
         int result = args[0] switch {
            "Tests" => Tests.Entry.Point(args),
            "CavityFlow" => CavityFlow.Entry.Point(),
            _ => -1 }; }
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
      S = new Stringer(350);
   }
}

}