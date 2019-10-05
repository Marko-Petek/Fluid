using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using static System.Char;
using SCG = System.Collections.Generic;

using TB = Fluid.Internals.Toolbox;                  // For Toolbox.
using Fluid.Internals.Collections;
using Fluid.Internals.Numerics;
using static Fluid.Internals.Algorithms;

namespace Fluid.Internals.IO {
   public static partial class IO {
      public static void Write<T>(SCG.IEnumerable<T> ienum, TextWriter tw) {
         tw.Write(ienum.ToString());
      }
      public static void WriteLine<T>(SCG.IEnumerable<T> ienum, TextWriter tw) {
         Write(ienum, tw);
         tw.WriteLine();
      }
   }
}