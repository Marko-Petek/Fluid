using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Runtime.CompilerServices;
using SCG = System.Collections.Generic;

using Fluid.Internals.Collections;
using Fluid.Internals.IO;
using static Fluid.Internals.Development.AppReporter;
using static Fluid.Internals.Operations;
using TB = Fluid.Internals.Toolbox;

namespace Fluid.Internals.Development
{
    /// <summary>Writes out messages to console or file as an aesthetic output table.</summary>
    internal class ReportWriter : IDisposable
    {
        /// <summary>Number of columns (deduced from DefaultColWidths).</summary>
        readonly int _NCols;
        /// <summary>Widths of columns</summary>
        int[] _ColWidths;
        /// <summary>Previously (most recently) reported time.</summary>
        System.DateTime _PrevReportedTime;


        /// <summary>StringBuilder.</summary>
        StringBuilder StrBuilder { get; } = new StringBuilder(500);
        /// <summary>Writes messages to file.</summary><param name="FileInfo("log.txt"">Log file.</param>
        FileWriter FileWriter { get; }

        AppReporter AppReporter { get; }



        /// <summary>Create an object which writes out messages to console or file as an aesthetic output table.</summary>
        public ReportWriter(AppReporter appReporter) {
            int buffer;

            if(System.Console.BufferWidth > _NCols * 2 - 12 + 36 + 20)
                buffer = System.Console.BufferWidth - _NCols * 2 - 12;              // spaces, some slack
            else
                buffer = 36 + 60;
            int remainder = buffer - 8 - 8 - 16 - 4;
            int textWidth = (int)(0.66*remainder);
            int pathWidth = remainder - textWidth;

            _ColWidths = new int[] {8, textWidth, 8, pathWidth, 16, 4};
            _NCols = _ColWidths.Length;
            FileWriter = new FileWriter("", "report", ".txt", true);
            AppReporter = appReporter;
            _PrevReportedTime = AppReporter.StartTime;
        }

        /// <summary>Take a Message and return formatted lines.</summary><param name="message">Message to format.</param>
        List<string> FormatMessage(int i) {
            var message = AppReporter.Report.Messages[i];
            var prevMessageTime = (i != 0) ? AppReporter.Report.Messages[i-1].Time : message.Time;
            List<string> wrappedTime;

            if(i == 0 || message.Time - _PrevReportedTime > System.TimeSpan.FromMinutes(1))                       // Only display time if it differs from previous time.
                wrappedTime = message.Time.ToShortTimeString().WrapToLines(_ColWidths[0]);      // Wrap one-line strings. Format TimeSpan then wrap resulting string.
            else
                wrappedTime = new List<string> { " " };

            _PrevReportedTime = message.Time;
            var wrappedText = message.Text.WrapToLines(_ColWidths[1]);
            System.TimeSpan dt = (message.Time - prevMessageTime);
            var wrappedDT = dt.TotalSeconds.ToString("G3").WrapToLines(_ColWidths[2]);      // Wrap one-line strings. Format TimeSpan then wrap resulting string.
            var relPath = Regex.Match(message.Path, @"/Fluid/.*").ToString();
            relPath = relPath.Remove(0, 7);
            var wrappedPath = relPath.WrapToLines(_ColWidths[3]);
            var wrappedCaller = message.Caller.WrapToLines(_ColWidths[4]);
            var wrappedLine = message.Line.ToString().WrapToLines(_ColWidths[5]);
            
            var listOfLists = new List<List<string>>(_NCols) { wrappedTime, wrappedText, wrappedDT, wrappedPath, wrappedCaller, wrappedLine };     // Find how many lines has the element with most lines.
            var listOfLengths = listOfLists.Select(elm => elm.Count);
            int maxNLines = listOfLengths.Max();
            var formattedLines = new List<string>(maxNLines);                                                                   // Result is going to appear here.

            for(int j = 0; j < maxNLines; ++j) {                                            // Over all lines.
                StrBuilder.Clear();

                for(int k = 0; k < _NCols; ++k) {                                                // Over all columns.
                    
                    if(listOfLists[k].Count > j) {                                          // List<string> j has an element to output at line i.
                        StrBuilder.Append(listOfLists[k][j].PadRight(_ColWidths[k] + 2));
                    }
                    else {
                        StrBuilder.Append(new string(' ', _ColWidths[k] + 2));                 // Add empty space
                    }
                }
                formattedLines.Add(StrBuilder.ToString());
            }
            return formattedLines;
        }

        /// <summary>Write a message to all outputs. Used by AppReporter.</summary><param name="i">Index of message inside Report.</param>
        public void WriteMessage(int i) {
            var formattedLines = FormatMessage(i);

            if((AppReporter.Output & OutputSettings.Console) == OutputSettings.Console) {
                foreach(var line in formattedLines)
                    TB.Console.WriteLine(line);
            }

            if((AppReporter.Output & OutputSettings.File) == OutputSettings.File) {
                
                foreach(var line in formattedLines)
                    FileWriter.WriteLine(line);         // TODO: use TB.Drive

                FileWriter.Flush();
            }
        }

        public void Dispose() {
            FileWriter.Dispose();
            System.GC.SuppressFinalize(this);
        }

        ~ReportWriter() => Dispose();
    }
}