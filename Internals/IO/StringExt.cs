using System.Collections.Generic;
using static System.Math;

namespace Fluid.Internals.IO {
public static class StringExt {
   /// <summary>Wraps a one line string to multiple lines and returns result as a list of strings (one element is one line).</summary><param name="str">Source string.</param><param name="wrapLgh">Wrap length.</param>
   public static List<string> WrapToLines(this string str, int wrapLgh = 60) {
      double nLinesDbl = (double) str.Length / wrapLgh;                            // How many times source string fits into wrapWidth.
      int nLines = (int)Ceiling(nLinesDbl);                                             // How many lines there actually are.
      var lines = new List<string>(nLines);
      if(str.Length > wrapLgh) {                                                    // More than one line, start wrapping.
         for(int i = 0; i < nLines-1; ++i)                                             // Add first N-1 lines which are full.
            lines.Add(str.Substring(i*wrapLgh, wrapLgh));
         int lastLineStart = (nLines-1)*wrapLgh;
         lines.Add(str.Substring(lastLineStart, str.Length - lastLineStart)); }                // Add last line.
      else                                                                                 // One line only, simply return it.
         lines.Add(str);
      return lines;
   }
}
}

