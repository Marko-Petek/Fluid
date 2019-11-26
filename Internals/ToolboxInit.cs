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

public class ToolboxInit : IToolboxInit {
   public (IO.Console, IO.FileReader, IO.FileWriter, Reporter, RNG, Stringer) Initialize() {
      System.Console.OutputEncoding = Encoding.UTF8;
      var console = new IO.Console();
      var fileReader = new IO.FileReader();
      var fileWriter = new IO.FileWriter();
      var reporter = new Reporter();
      var rng = new RNG();
      var s = new Stringer(350);
      return (console, fileReader, fileWriter, reporter, rng, s);
   }
}
}