using System;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using static System.Math;

using Fluid.Internals;
using TB = Fluid.Internals.Toolbox;

namespace Fluid.Seminar.NodeCreation {
   partial class Program {
      public static void StringReplace() {
         string input = @"{
Polygon[{nodes[[1, 1]], nodes[[1, 4]], nodes[[4, 4]], 
   nodes[[4, 1]]}],
Polygon[{nodes[[7, 1]], nodes[[7, 4]], nodes[[10, 4]], 
   nodes[[10, 1]]}],
Polygon[{nodes[[4, 4]], nodes[[4, 7]], nodes[[7, 7]], 
   nodes[[7, 4]]}],
Polygon[{nodes[[1, 7]], nodes[[1, 10]], nodes[[4, 10]], 
   nodes[[4, 7]]}],
Polygon[{nodes[[7, 7]], nodes[[7, 10]], nodes[[10, 10]], 
   nodes[[10, 7]]}]
};";

         string pattern = @"nodes";
         string modified = Regex.Replace(input, pattern, "transNodes");
         TB.FileWriter.SetFile("Seminar/NodeCreation/modifiedString.txt");
         TB.FileWriter.Write(modified);
      }
   }
}