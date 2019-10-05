using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using static System.Math;

namespace Fluid.Internals {
   public static class Algorithms {
      /// <summary>Swap two items.</summary><param name="first">First item.</param><param name="second">Second item.</param>
      public static void Swap<T>(ref T first, ref T second) {
         T temp = first;
         first = second;
         second = temp;
      }
      /// <summary>N-1 times repeat {loopCode, sharedCode} and finally {endCode, sharedCode} once.</summary><param name="N">Number of pair calls.</param><param name="loopCode">Called only inside for loop as first in pair.</param><param name="sharedCode">Called inside for loop and at end as second in pair.</param><param name="endCode">Called only at end as first in pair.</param>
      public static void For1(int N, Action loopCode, Action sharedCode, Action endCode) {
         for(int i = 0; i < N-1; ++i) {
            loopCode();
            sharedCode(); }
         endCode();
         sharedCode();
      }
      /// <summary>N-1 times repeat {sharedCode, loopCode} and finally {sharedCode, endCode} once.</summary><param name="N">Number of pair calls.</param><param name="sharedCode">Called inside for loop and at end as first in pair.</param><param name="loopCode">Called only inside for loop as second in pair.</param><param name="endCode">Called only at end as second in pair.</param>
      public static void For2(int N, Action sharedCode, Action loopCode, Action endCode) {
         for(int i = 0; i < N-1; ++i) {
            sharedCode();
            loopCode(); }
         sharedCode();
         endCode();
      }
      /// <summary>N-1 times repeat {sharedCode, loopCode} and finally {sharedCode, endCode} once.</summary><param name="N">Number of pair calls.</param><param name="sharedCode">Called inside for loop and at end as first in pair.</param><param name="loopCode">Called only inside for loop as second in pair.</param><param name="endCode">Called only at end as second in pair.</param>
      public static void For3(int init, int N, Action sharedCode, Action<int> loopCode, Action endCode) {
         for(int i = init; i < N-1; ++i) {
            sharedCode();
            loopCode(i); }
         sharedCode();
         endCode();
      }
      /// <summary>N-1 times repeat {sharedCode, loopCode} and finally {sharedCode, endCode} once.</summary><param name="N">Number of pair calls.</param><param name="sharedCode">Called inside for loop and at end as first in pair.</param><param name="loopCode">Called only inside for loop as second in pair.</param><param name="endCode">Called only at end as second in pair.</param>
      public static void For4(int init, int N, Action<int> sharedCode, Action<int> loopCode, Action endCode) {
         for(int i = init; i < N-1; ++i) {
            sharedCode(i);
            loopCode(i); }
         sharedCode(N-1);
         endCode();
      }
      /// <summary>Wraps a single line string (no new lines within) to a multi-line string of specified width.</summary><param name="str">Single line string.</param><param name="wrapLgh">Maximum character count inside a single line.</param><param name="fstIndent">N spaces prepended to first line.</param><param name="sqtIndents">N spaces prepended to all subsequent lines.</param>
      public static string Wrap(this string str, int wrapLgh = 85, int fstIndent = 0, int sqtIndents = 0) {
         double nLinesFloat = (double)str.Length / wrapLgh;                              // How many times source string fits into wrapWidth.
         int nLines = (int)Ceiling(nLinesFloat);                                                     // How many lines there actually are.
         if(str.Length > wrapLgh) {
            char[] chars = new char[fstIndent + str.Length + (nLines-1)*(1 + sqtIndents)];                // Length of original + space for new lines and leading spaces.                                                                                                  
            for(int i = 0; i < fstIndent; ++i)                                                              // Add leading spaces to first line.
               chars[i] = ' ';
            str.CopyTo(0, chars, fstIndent, wrapLgh - fstIndent);
            int charIndex = wrapLgh;                                                                         // Character in chars array which we are currently modifying. Next is zeroth character on second line.
            For3(1, nLines,
               sharedCode: () => {
                  chars[charIndex] = '\n';                                                                        // Add new line.
                  ++charIndex;
                  for(int j = 0; j < sqtIndents; ++j) {                                                    // Add indent.
                        chars[charIndex] = ' ';
                        ++charIndex; } },
               loopCode: (int i) => {
                  str.CopyTo(i * wrapLgh, chars, charIndex, wrapLgh);                              // Add characters from source string.
                  charIndex += wrapLgh; },
               endCode: () => {
                  str.CopyTo((wrapLgh)*(nLines-1), chars, charIndex, str.Length - (nLines - 1)*wrapLgh); } );
            return new string(chars); }
         else return str;
      }
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

      public static bool Equals(this double val1, double val2, double eps = 0.000001) {
         if(Abs(val1 - val2) > eps)
            return false;
         else
            return true;
      }
      // λ = lower level data, χ = higher level data
      // This rank data.
      /// <summary></summary>
      /// <param name="inclStopDepth">Tensors of this rank are provided to the onStopRank delegate.</param>
      /// <param name="onStopRank">Delegate is called when the rank of order inclStopRank is reached.</param>
      public static void Recursion<τ>(IEnumerable<τ> leader, IEnumerable<τ> follower,
      Func<KeyValuePair<int,IEnumerable<τ>>> onResurface,
      int inclStopDepth, Func<IEnumerable<τ>> onStopDepth) {
      //Func<Tensor<τ,α>, Tensor<τ,α>, ρ> onStopRank) 
         //Recurse(recurseSrc, recurseTgt);

         /// <summary>Returned values are from the current level of local Recurse.</summary>
         /// <param name="depth"></param>
         /// <param name="KeyValuePair<int"></param>
         /// <param name="lead"></param>
         /// <param name="folw"></param>
         /// <returns></returns>
         (KeyValuePair<int,IEnumerable<τ>>, KeyValuePair<int,IEnumerable<τ>>) Recurse(int depth, 
         KeyValuePair<int,IEnumerable<τ>> lead, KeyValuePair<int,IEnumerable<τ>> folw) {                        // Takes in inDat from above, returns outDat from its depth.
            if(depth <= inclStopDepth) {
               foreach(var int_subLead in lead) {
                  int subKey = int_subSrc.Key;
                  var subSrc = int_subSrc.Value;
                  if(tgt.TryGetValue(subKey, out var subTgt)) {                 // Subtensor exists in aTgt.
                     ρ resurfaceInfo = Recurse(subSrc, subTgt);
                     return onResurface(resurfaceInfo);
                  }
                  else                                                        // Equivalent tensor does not exist on tgt.
                     return onNoEquivalent(subKey, subSrc, tgt); }
               return onEmptySrc(); }
            else                                                              // inclusive StopRank reached.
               return onStopRank(src, tgt);
         }
      }
   }
}