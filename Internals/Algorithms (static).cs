using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using static System.Math;
using Fluid.Internals.Numerics;
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
   /// <summary>Convert a double array to Vec2.</summary>
   /// <param name="pos">A 2D array.</param>
   public static Vec2 ToVec2(this double[] pos) =>
      new Vec2(in pos);
}
}