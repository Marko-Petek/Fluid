using System;
using static System.Math;

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
        public static void ForV1(int N, Action loopCode, Action sharedCode, Action endCode) {

            for(int i = 0; i < N-1; ++i) {
                loopCode();
                sharedCode();
            }
            endCode();
            sharedCode();
        }

        /// <summary>N-1 times repeat {sharedCode, loopCode} and finally {sharedCode, endCode} once.</summary><param name="N">Number of pair calls.</param><param name="sharedCode">Called inside for loop and at end as first in pair.</param><param name="loopCode">Called only inside for loop as second in pair.</param><param name="endCode">Called only at end as second in pair.</param>
        public static void ForV2(int N, Action sharedCode, Action loopCode, Action endCode) {

            for(int i = 0; i < N-1; ++i) {
                sharedCode();
                loopCode();
            }
            sharedCode();
            endCode();
        }

        /// <summary>N-1 times repeat {sharedCode, loopCode} and finally {sharedCode, endCode} once.</summary><param name="N">Number of pair calls.</param><param name="sharedCode">Called inside for loop and at end as first in pair.</param><param name="loopCode">Called only inside for loop as second in pair.</param><param name="endCode">Called only at end as second in pair.</param>
        public static void ForV3(int i0, int N, Action sharedCode, Action<int> loopCode, Action endCode) {

            for(int i = i0; i < N-1; ++i) {
                sharedCode();
                loopCode(i);
            }
            sharedCode();
            endCode();
        }

        /// <summary>Wraps a single line string (no new lines within) to a multi-line string of specified width.</summary><param name="sourceString">Single line string.</param><param name="wrapLength">Maximum character count inside a single line.</param><param name="firstLineIndent">N spaces prepended to first line.</param><param name="subsequentIndents">N spaces prepended to all subsequent lines.</param>
        static string Wrap(this string sourceString, int wrapLength = 85, int firstLineIndent = 0, int subsequentIndents = 0) {
            double nLinesFloat = (double)sourceString.Length / wrapLength;                              // How many times source string fits into wrapWidth.
            int nLines = (int)Ceiling(nLinesFloat);                                                     // How many lines there actually are.

            if(sourceString.Length > wrapLength) {
                char[] chars = new char[firstLineIndent + sourceString.Length + (nLines-1)*(1 + subsequentIndents)];                // Length of original + space for new lines and leading spaces.                                                                                                  

                for(int i = 0; i < firstLineIndent; ++i) {                                                              // Add leading spaces to first line.
                    chars[i] = ' ';
                }
                sourceString.CopyTo(0, chars, firstLineIndent, wrapLength - firstLineIndent);
                int charIndex = wrapLength;                                                                         // Character in chars array which we are currently modifying. Next is zeroth character on second line.

                ForV3(1, nLines,
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
    }
}