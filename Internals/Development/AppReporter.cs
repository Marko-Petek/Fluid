using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Text;
using System.Linq;
using static System.Math;

using static Fluid.Internals.Development.AppReporter.VerbositySettings;
using static Fluid.Internals.Operations;

namespace Fluid.Internals.Development
{
    /// <summary>Writes program's progression either to a file, to console or both.</summary>
    public class AppReporter
    {
        /// <summary>Stores report messages for a single application run.</summary>
        Report _Report;

        /// <summary>Determines AppReporter's output channel: console or file or both.</summary>
        public OutputSettings Output { get; set; }
        /// <summary>Determines which messages get displayed by AppReporter.</summary>
        public VerbositySettings Verbosity { get; set; }
        /// <summary>Stores report messages for a single application run.</summary>
        public Report Report => _Report;
        /// <summary>Writes out messages to console or file as an aesthetic output table.</summary>
        ReportWriter ReportWriter { get; set; }
        /// <summary>Time of AppReporter construction (usually application start time).</summary>
        public DateTime StartTime { get; }


        /// <summary>Initialize AppReporter as set to write to console and file.</summary>
        public AppReporter(VerbositySettings verbosity,
        [CallerFilePath] string path = null, [CallerMemberName] string caller = null, [CallerLineNumber] int line = 0) {

            Output = new OutputSettings();
            Output |= (OutputSettings.Console | OutputSettings.File);
            Verbosity = verbosity;
            StartTime = DateTime.Now;
            _Report = new Report(StartTime);
            ReportWriter = new ReportWriter(this);

            var initMessage = new Message(StartTime, "Program started.", path, caller, line);            // Write initial message.
            Report.AddMessage(initMessage);
            ReportWriter.WriteMessage(0);   
                      // Write initial message (Program started, etc.)
        }

        /// <summary>Writes a string to console, to file or both, but only if specified verbosity is below the threshold.</summary><param name="text">String to write.</param><param name="verbosity">Sets lowest threshold at which message is still displayed.</param>
        [Conditional("REPORT")]
        public void Write(string text, VerbositySettings verbosity = Moderate,
        [CallerFilePath] string path = null, [CallerMemberName] string caller = null, [CallerLineNumber] int line = 0) {

            if(verbosity <= Verbosity) {                                                    // Only write if verbosity is below threshold.
                var message = new Message(DateTime.Now, text, path, caller, line);          // Construct message.
                int newMessageIndex = Report.Messages.Count;                                // New message index will equal count.
                Report.AddMessage(message);
                ReportWriter.WriteMessage(newMessageIndex);
            }
        }

        // TODO: Write a method that nicely displays an exception.

        [Flags]
        /// <summary>Determines where output is written to.</summary>
        public enum OutputSettings
        {
            Console  = 1 << 0,
            File     = 1 << 1
        }

        /// <summary>Determines which messages will be displayed by Reporter..</summary>
        public enum VerbositySettings
        {
            Silent, Scarce, Moderate, Verbose, Obnoxious
        }
    }
}