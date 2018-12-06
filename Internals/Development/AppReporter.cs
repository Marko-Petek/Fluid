using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using static System.Console;
using static System.Math;

using static Fluid.Internals.Development.AppReporter.OutputSettings;
using static Fluid.Internals.Development.AppReporter.VerbositySettings;
using static Fluid.Internals.Operations;

namespace Fluid.Internals.Development
{
    /// <summary>Writes program's progression either to a file, to console or both.</summary>
    public class AppReporter
    {
        /// <summary>Determines AppReporter's output channel: console or file or both.</summary>
        public OutputSettings Output { get; set; }
        /// <summary>Determines which messages get displayed by AppReporter.</summary>
        public VerbositySettings Verbosity { get; set; }
        /// <summary>Can write aesthetically pleasing output to console or file.</summary>
        ReportWriter ReportWriter { get; set; }


        /// <summary>Initialize AppReporter as set to write to console and file.</summary>
        public AppReporter(VerbositySettings verbosity) {
            Output = new OutputSettings();
            Output |= WriteToConsole | WriteToFile;
            Verbosity = verbosity;
            ReportWriter = new ReportWriter()
        }
        

        /// <summary>Marks time of program start. Needs to be called inside Main().</summary>
        public static void Start() {
            // Move to constructor.
            WriteLine($"\n{TimeOfLastReport.ToShortDateString()}");
            Write($"{TimeOfLastReport.ToShortTimeString()}  Program started.");

            Writer.WriteLine($"\n\n{TimeOfLastReport.ToShortDateString()}");
            Writer.Write($"{TimeOfLastReport.ToShortTimeString()}  Program started.");
            Writer.Flush();

        }

        /// <summary>Writes a string to console, to file or both, but only if specified verbosity is below the threshold.</summary><param name="msg">String to write.</param><param name="verbosity">Sets lowest threshold at which message is still displayed.</param>
        public static void Report(string msg, VerbositySettings verbosity = Moderate, [CallerFilePath] string path = null,
            [CallerMemberName] string caller = null, [CallerLineNumber] int line = 0
            ) {

            if(verbosity <= Verbosity) {                                             // Only report if verbosity is below threshold.
                TimeSpan elapsed = DateTimeOffset.Now - TimeOfLastReport;     // We wish to write time elapsed from last report at end of report.

                if(elapsed > TimeSpan.FromMinutes(1.0)) {                     // We do not wish to write two identical time stamps in a row. Set a flag (dateChanged) which signals that at least a minute has passed from last report and we should write a new time stamp.
                    TimeOfLastReport = DateTime.Now;
                    dateChanged = true;
                }
                string dateStr = TimeOfLastReport.ToShortTimeString();
                StrBuilder.Clear();
                StrBuilder.Append(msg);                                                     // Append string we wish to report.

                // TODO: Append file path, caller name and line number.
                // if((OutputSettings & WriteFilePath) == WriteFilePath) {
                //     StrBuilder.Append($"  File: {filePath}");
                // }

                // if((OutputSettings & WriteCallerName) == WriteCallerName) {
                //     StrBuilder.Append($"  Caller: {callerName}");
                // }

                // if((OutputSettings & WriteLineNumber) == WriteLineNumber) {
                //     StrBuilder.Append($"  Line: {lineNumber}");
                // }
                msg = StrBuilder.ToString();

                if((Output & WriteToConsole) == WriteToConsole) {
                    WriteLine($"  ({elapsed.TotalSeconds.ToString("G3")} s)");

                    if(dateChanged) {                                                       // Write time stamp.
                        Write(dateStr.PadRight(dateStr.Length + 2));
                    }
                    else {
                        Write(new string(' ', dateStr.Length + 2));                         // Write empty space the length of time stamp.
                    }
                    Write(msg.Wrap());
                }
                
                if((Output & WriteToFile) == WriteToFile) {
                    Writer.WriteLine($"  ({elapsed.TotalSeconds.ToString("G3")} s)");

                    if(dateChanged) {                                                       // Write time stamp.
                        Writer.Write(dateStr.PadRight(dateStr.Length + 2));
                    }
                    else {
                        Writer.Write(new string(' ', dateStr.Length + 2));                  // Write empty space the length of time stamp.
                    }
                    Writer.Write(msg.Wrap());
                    Writer.Flush();
                }
            }
        }




        [Flags]
        /// <summary>Determines where output is written to.</summary>
        public enum OutputSettings
        {
            WriteToConsole  = 1 << 0,
            WriteToFile     = 1 << 1
        }

        /// <summary>Determines which messages will be displayed by Reporter..</summary>
        public enum VerbositySettings
        {
            Silent, Scarce, Moderate, Verbose, Obnoxious
        }
    }
}