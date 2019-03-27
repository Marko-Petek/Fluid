using System;
using System.Linq;
using System.Text.RegularExpressions;
using static System.Math;

using Fluid.Internals.Collections;

namespace Fluid.Internals
{
    public static class Operations
    {
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
                sharedCode();
            }
            endCode();
            sharedCode();
        }

        /// <summary>N-1 times repeat {sharedCode, loopCode} and finally {sharedCode, endCode} once.</summary><param name="N">Number of pair calls.</param><param name="sharedCode">Called inside for loop and at end as first in pair.</param><param name="loopCode">Called only inside for loop as second in pair.</param><param name="endCode">Called only at end as second in pair.</param>
        public static void For2(int N, Action sharedCode, Action loopCode, Action endCode) {

            for(int i = 0; i < N-1; ++i) {
                sharedCode();
                loopCode();
            }
            sharedCode();
            endCode();
        }

        /// <summary>N-1 times repeat {sharedCode, loopCode} and finally {sharedCode, endCode} once.</summary><param name="N">Number of pair calls.</param><param name="sharedCode">Called inside for loop and at end as first in pair.</param><param name="loopCode">Called only inside for loop as second in pair.</param><param name="endCode">Called only at end as second in pair.</param>
        public static void For3(int init, int N, Action sharedCode, Action<int> loopCode, Action endCode) {

            for(int i = init; i < N-1; ++i) {
                sharedCode();
                loopCode(i);
            }
            sharedCode();
            endCode();
        }

        /// <summary>N-1 times repeat {sharedCode, loopCode} and finally {sharedCode, endCode} once.</summary><param name="N">Number of pair calls.</param><param name="sharedCode">Called inside for loop and at end as first in pair.</param><param name="loopCode">Called only inside for loop as second in pair.</param><param name="endCode">Called only at end as second in pair.</param>
        public static void For4(int init, int N, Action<int> sharedCode, Action<int> loopCode, Action endCode) {

            for(int i = init; i < N-1; ++i) {
                sharedCode(i);
                loopCode(i);
            }
            sharedCode(N-1);
            endCode();
        }

        /// <summary>Wraps a single line string (no new lines within) to a multi-line string of specified width.</summary><param name="sourceString">Single line string.</param><param name="wrapLength">Maximum character count inside a single line.</param><param name="firstLineIndent">N spaces prepended to first line.</param><param name="subsequentIndents">N spaces prepended to all subsequent lines.</param>
        public static string Wrap(this string sourceString, int wrapLength = 85, int firstLineIndent = 0, int subsequentIndents = 0) {
            double nLinesFloat = (double)sourceString.Length / wrapLength;                              // How many times source string fits into wrapWidth.
            int nLines = (int)Ceiling(nLinesFloat);                                                     // How many lines there actually are.

            if(sourceString.Length > wrapLength) {
                char[] chars = new char[firstLineIndent + sourceString.Length + (nLines-1)*(1 + subsequentIndents)];                // Length of original + space for new lines and leading spaces.                                                                                                  

                for(int i = 0; i < firstLineIndent; ++i) {                                                              // Add leading spaces to first line.
                    chars[i] = ' ';
                }
                sourceString.CopyTo(0, chars, firstLineIndent, wrapLength - firstLineIndent);
                int charIndex = wrapLength;                                                                         // Character in chars array which we are currently modifying. Next is zeroth character on second line.

                For3(1, nLines,
                    sharedCode: () => {
                        chars[charIndex] = '\n';                                                                        // Add new line.
                        ++charIndex;

                        for(int j = 0; j < subsequentIndents; ++j) {                                                    // Add indent.
                            chars[charIndex] = ' ';
                            ++charIndex;
                        }
                    },
                    loopCode: (int i) => {
                        sourceString.CopyTo(i * wrapLength, chars, charIndex, wrapLength);                              // Add characters from source string.
                        charIndex += wrapLength;
                    },
                    endCode: () => {
                        sourceString.CopyTo((wrapLength)*(nLines-1), chars, charIndex, sourceString.Length - (nLines - 1)*wrapLength);
                    }
                );
                return new string(chars);
            }
            else return sourceString;
        }
        
        /// <summary>Wraps a one line string to multiple lines and returns result as a list of strings (one element is one line).</summary><param name="source">Source string.</param><param name="wrapLength">Wrap length.</param>
        public static List<string> WrapToLines(this string source, int wrapLength = 60) {
            double nLinesFloat = (double)source.Length / wrapLength;                            // How many times source string fits into wrapWidth.
            int nLines = (int)Ceiling(nLinesFloat);                                             // How many lines there actually are.
            var lines = new List<string>(nLines);

            if(source.Length > wrapLength) {                                                    // More than one line, start wrapping.

                for(int i = 0; i < nLines-1; ++i) {                                             // Add first N-1 lines which are full.
                    lines.Add(source.Substring(i*wrapLength, wrapLength));
                }
                int lastLineStart = (nLines-1)*wrapLength;
                lines.Add(source.Substring(lastLineStart, source.Length - lastLineStart));                 // Add last line.
                
            }
            else                                                                                 // One line only, simply return it.
                lines.Add(source);
            
            return lines;
        }

        /// <summary>The string representation must already be in the correct form.</summary><param name="str">String representation of array.</param>
        public static double[] ParseLowestArrayDimToDouble(string str) {
            var matches = Regex.Matches(str, @"(?<=\{)\d+\.?\d*(?=})");
            var array = matches.Select( match => Double.Parse(match.Value)).ToArray();
            return array;
        }

        /// <summary>See Fluid/Sketches/ParseHigherArrayDim.svg.</summary>
        public static (int[] rectLvls, int[][] nonRectLvls) GetArrayDimStructure(string str) {

            throw new NotImplementedException();

            // MatchCollection matches;
            // string

            // do {
            //     matches = Regex.Matches(str, @"");
            // }
            // while();
        }
    }
}