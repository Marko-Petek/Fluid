using System;

using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Fluid.Internals.Development;
using IO = Fluid.Internals.IO;
using Fluid.Internals.Numerics;
using Fluid.Internals.Text;

namespace Fluid.Internals {
public class Tools {
   object _locker = new object();
   public IO.FileReader FileReader { get; set; }
   public IO.FileWriter FileWriter { get; set; }
   public IO.Console Writer { get; set; }
   Reporter _Reporter;
   /// <summary>Reporter. Logs and writes messages to Console.</summary>
   public Reporter Reporter {                     // Reported.Write can only be called from one thread at once.
      get { lock(_locker) return _Reporter; }
      private set {  lock(_locker) _Reporter = value; }
   }
   public RNG Rng { get; internal set; }
   public Stringer S { get; internal set; }

   public Tools() {
      Writer = new IO.Console();
      FileReader = new IO.FileReader();
      FileWriter = new IO.FileWriter();
      _Reporter = new Reporter(Writer);
      Rng = new RNG();
      S = new Stringer(350);
   }

}

}