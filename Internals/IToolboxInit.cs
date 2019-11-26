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

public interface IToolboxInit {
   (IO.Console, IO.FileReader, IO.FileWriter, Reporter, RNG, Stringer) Initialize();
}
}