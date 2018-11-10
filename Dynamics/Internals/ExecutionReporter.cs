using System;
using System.IO;
using static System.Console;
using static System.Math;
using static Fluid.Dynamics.Internals.ExecutionReporter.OutputSettingsEnum;
using static Fluid.Dynamics.Internals.ExecutionReporter.VerbositySettings;

namespace Fluid.Dynamics.Internals
{
    /// <summary>Writes program's progression either to a file, to console or both.</summary>
    public static class ExecutionReporter
    {
        [Flags]
        public enum OutputSettingsEnum
        {
            WriteToConsole = 1 << 0,
            WriteToFile    = 1 << 1
        }

        /// <summary>Determines which messages will be displayed by Reporter..</summary>
        public enum VerbositySettings
        {
            Silent, Scarce, Moderate, Verbose, Obnoxious
        }

        /// <summary>Initialize AppReporter as set to write to console and file.</summary>
        static ExecutionReporter() {
            OutputSettings = new OutputSettingsEnum();
            OutputSettings |= WriteToConsole;
            OutputSettings |= WriteToFile;
            Writer = new StreamWriter(path: new FileInfo("log.txt").FullName, append: true);
            TimeOfLastReport = DateTime.Now;
            VerbositySetting = Moderate;
        }


        /// <summary>Determines Reporter's output channel: console or file or both.</summary>
        public static OutputSettingsEnum OutputSettings { get; set; }
        /// <summary>Determines which messages get displayed by Reporter.</summary>
        public static VerbositySettings VerbositySetting { get; set; }

        static StreamWriter Writer { get; }

        /// <summary>Time of most recently displayed message.</summary>
        static DateTime TimeOfLastReport { get; set; }

        public static void Start() {
            WriteLine($"\n{TimeOfLastReport.ToShortDateString()}");
            Write($"{TimeOfLastReport.ToShortTimeString()}  Program started.");

            Writer.WriteLine($"\n\n{TimeOfLastReport.ToShortDateString()}");
            Writer.Write($"{TimeOfLastReport.ToShortTimeString()}  Program started.");
            Writer.Flush();

        }

        /// <summary>Writes a string to console, to file or both, but only if specified verbosity is below the threshold.</summary><param name="str">String to write.</param><param name="verbosity">Sets lowest threshold at which message is still displayed.</param>
        public static void Report(string str, VerbositySettings verbosity = Moderate) {

            if(verbosity <= VerbositySetting) {                                             // Only display message if verbosity is below threshold.
                bool dateChanged = false;
                TimeSpan elapsedFromLastReport = DateTimeOffset.Now - TimeOfLastReport;     // We wish to write time elapsed from last report at end of report.

                if(elapsedFromLastReport > TimeSpan.FromMinutes(1.0)) {                     // We do not wish to write two identical time stamps in a row. Set a flag (dateChanged) which signals that at least a minute has passed from last report and we should write a new time stamp.
                    TimeOfLastReport = DateTime.Now;
                    dateChanged = true;
                }
                string dateStr = TimeOfLastReport.ToShortTimeString();

                if((OutputSettings & WriteToConsole) == WriteToConsole) {
                    WriteLine($"  ({elapsedFromLastReport.TotalSeconds.ToString("G3")} s)");

                    if(dateChanged) {                                                       // Write time stamp.
                        Write(dateStr.PadRight(dateStr.Length + 2));
                    }
                    else {
                        Write(new string(' ', dateStr.Length + 2));                         // Write empty space the length of time stamp.
                    }
                    Write(str.Wrap());
                }
                
                if((OutputSettings & WriteToFile) == WriteToFile) {
                    Writer.WriteLine($"  ({elapsedFromLastReport.TotalSeconds.ToString("G3")} s)");

                    if(dateChanged) {                                                       // Write time stamp.
                        Writer.Write(dateStr.PadRight(dateStr.Length + 2));
                    }
                    else {
                        Writer.Write(new string(' ', dateStr.Length + 2));                  // Write empty space the length of time stamp.
                    }
                    Writer.Write(str.Wrap());
                    Writer.Flush();
                }
            }
        }

        /// <summary>Wraps a single line string (no new lines within) to a multi-line string of specified width.</summary><param name="sourceString">Single line string.</param><param name="wrapLength">Maximum character count inside a single line.</param><param name="firstLineIndent">N spaces prepended to first line.</param><param name="subsequentIndents">N spaces prepended to all subsequent lines.</param>
        static string Wrap(this string sourceString, int wrapLength = 75, int firstLineIndent = 0, int subsequentIndents = 9) {
            double nLinesFloat = (double)sourceString.Length / wrapLength;                              // How many times source string fits into wrapWidth.
            int nLines = (int)Ceiling(nLinesFloat);                                                     // How many lines there actually are.

            if(sourceString.Length > wrapLength) {
                char[] chars = new char[firstLineIndent + sourceString.Length + (nLines-1)*(1 + subsequentIndents)];                // Length of original + space for new lines and leading spaces.                                                                                                  

                for(int i = 0; i < firstLineIndent; ++i) {                                                              // Add leading spaces to first line.
                    chars[i] = ' ';
                }
                sourceString.CopyTo(0, chars, firstLineIndent, wrapLength - firstLineIndent);
                int charIndex = wrapLength;                                                                         // Character in chars array which we are currently modifying. Next is zeroth character on second line.

                for(int i = 1; i < nLines - 1; ++i) {                                                               // Fill subsequent lines, save for last.
                    chars[charIndex] = '\n';                                                                        // Add new line.
                    ++charIndex;

                    for(int j = 0; j < subsequentIndents; ++j) {                                                    // Add indent.
                        chars[charIndex] = ' ';
                        ++charIndex;
                    }
                    sourceString.CopyTo(i * wrapLength, chars, charIndex, wrapLength);                              // Add characters from source string.
                    charIndex += wrapLength;
                }
                // Take care of last line.
                chars[charIndex] = '\n';                                                                        // Add new line.
                ++charIndex;

                for(int j = 0; j < subsequentIndents; ++j) {                                                    // Add indent.
                    chars[charIndex] = ' ';
                    ++charIndex;
                }
                sourceString.CopyTo((wrapLength)*(nLines-1), chars, charIndex, sourceString.Length - (nLines - 1)*wrapLength);

                return new string(chars);
            }
            else return sourceString;
        }
    }

    
}