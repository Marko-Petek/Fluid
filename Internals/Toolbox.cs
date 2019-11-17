#nullable enable
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Fluid.Internals.Development;
using IO = Fluid.Internals.IO;
using Fluid.Internals.Numerics;
using Fluid.Internals.Text;
using static Fluid.Internals.Development.Reporter;
namespace Fluid.Internals {
public class Toolbox {
   public static Toolbox T { get; set; }
   static object _locker = new object();
   public static string DebugTag = "";

   public IO.FileReader FileReader { get; set; }
   public IO.FileWriter FileWriter { get; set; }
   public IO.Console Writer { get; set; }
   Reporter _Reporter;
   /// <summary>Reporter. Logs and writes messages to Console.</summary>
   public Reporter Reporter {                     // Reported.Write can only be called from one thread at once.
      get { lock(_locker) return _Reporter; }
      private set {  lock(_locker) _Reporter = value; }
   }
   public void R(string s) => Reporter.R(s);
   public RNG Rng { get; set; }
   public Stringer S { get; set; }

   public Toolbox(IToolboxInit toolboxInit) {
      (Writer, FileReader, FileWriter, _Reporter, Rng, S) = toolboxInit.Initialize();
   }

}

}
#nullable restore