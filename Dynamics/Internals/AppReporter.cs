using System;
using System.IO;
using static System.Console;
using static Fluid.Dynamics.Internals.AppReporter.OutputSettingsEnum;
using static Fluid.Dynamics.Internals.AppReporter.VerbositySettings;

namespace Fluid.Dynamics.Internals
{
    /// <summary>Writes program's progression either to a file, to console or both.</summary>
    public static class AppReporter
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
        static AppReporter() {
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

            Writer.WriteLine($"\n{TimeOfLastReport.ToShortDateString()}");
            Writer.Write($"{TimeOfLastReport.ToShortTimeString()}  Program started.");
            Writer.Flush();

        }

        /// <summary>Writes a string to console, to file or both, but only if specified verbosity is below the threshold.</summary><param name="str">String to write.</param><param name="verbosity">Sets lowest threshold at which message is still displayed.</param>
        public static void Report(string str, VerbositySettings verbosity) {

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
                    Write(str.EnforceMaxWidth(100));
                }
                
                if((OutputSettings & WriteToFile) == WriteToFile) {
                    Writer.WriteLine($"  ({elapsedFromLastReport.TotalSeconds.ToString("G3")} s)");

                    if(dateChanged) {                                                       // Write time stamp.
                        Writer.Write(dateStr.PadRight(dateStr.Length + 2));
                    }
                    else {
                        Writer.Write(new string(' ', dateStr.Length + 2));                  // Write empty space the length of time stamp.
                    }
                    Writer.Write(str.EnforceMaxWidth(100));
                    Writer.Flush();
                }
            }
        }

        public static void Report(string str) {
            Report(str, Moderate);
        }

        public static void ReportToConsole(string str) {
            WriteLine(str);
        }

        public static void ReportToFile(string str) {
            Writer.WriteLine(str);
        }

        /// <summary>Makes provided string viable for display within a screen of specified width.</summary><param name="str">Provided string.</param><param name="maxWidth">Width of screen area.</param>
        static string EnforceMaxWidth(this string str, int maxWidth) {
            int length = str.Length;
            double relativeWidth = (double)length / maxWidth;

            if(relativeWidth > 1) {
                char[] wrappedStr = new char[length + (int)relativeWidth * 10];              // Length of original + space for new lines and leading spaces.
                int newDestinationIndex = 0;

                for(int i = 1; i < (int)relativeWidth; ++i) {                            // Insert new lines. (except last)
                    str.CopyTo(i * maxWidth, wrappedStr, newDestinationIndex, maxWidth);
                    newDestinationIndex += maxWidth;
                    wrappedStr[newDestinationIndex] = '\n';
                    ++newDestinationIndex;

                    for(int j = 0; j < 9; ++j) {                         // Add leading space.
                        wrappedStr[newDestinationIndex] = ' ';
                        ++newDestinationIndex;
                    }
                }                                                       // TODO: Fix this method. It is overwriting things.

                // Take care of last line.
                int lastLineCount = length - (int)relativeWidth * maxWidth;
                str.CopyTo(length - maxWidth - 1, wrappedStr, wrappedStr.Length - maxWidth - 1, lastLineCount);

                return new string(wrappedStr);
            }
            else return str;
        }
    }

    
}