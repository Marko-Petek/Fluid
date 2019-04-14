using System;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using static System.Math;

using Fluid.Internals;
using TB = Fluid.Internals.Toolbox;

namespace Fluid.Seminar.NodeCreation {
   partial class Program {
      static void Main(string[] args) => TB.EntryPointSetup(() => CreateDerivedData());  
   }
}
