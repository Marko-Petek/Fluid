﻿using System;
using System.Diagnostics;
using System.Text;

using Tests = Fluid.Tests;
using CavityFlow = Fluid.Runnables.CavityFlow;

using Fluid.Internals.Development;
using IO = Fluid.Internals.IO;
using Fluid.Internals.Numerics;
using static Fluid.Internals.Toolbox;

namespace Fluid.Main {
class Program {
   static void Main(string[] args) {            // args[0] has to be the name of the project without the extension.
      try {
         Initialize();
         switch(args[0]) {
            case "Tests":
               Tests.Entry.Point(args);
               break;
            case "CavityFlow":
               CavityFlow.Entry.Point();
               break;
            default:
               break; } }
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
   }
}

}