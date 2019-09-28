using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


using Fluid.Internals.Development;
using IO = Fluid.Internals.IO;
using Fluid.Internals.Numerics;

using static Fluid.Internals.Development.Reporter;

namespace Fluid.Internals {

public static class Toolbox {
   static object _locker = new object();
   public static string DebugTag = "";

   public static IO.FileReader FileReader { get; set; }
   public static IO.FileWriter FileWriter { get; set; }
   public static IO.Console Writer { get; set; }
   static Reporter _Reporter;
   /// <summary>Reporter. Logs and writes messages to Console.</summary>
   public static Reporter R {                     // Reported.Write can only be called from one thread at once.
      get {
         lock(_locker)
            return _Reporter; }
      set {
         lock(_locker)
            _Reporter = value; }
   }
   public static RandomNumberGen Rng { get; set; }
}

}