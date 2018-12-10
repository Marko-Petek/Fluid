using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Globalization;
using System.Runtime.CompilerServices;
using SCG = System.Collections.Generic;

using Fluid.Internals.Collections;
using static Fluid.Internals.Development.AppReporter;
using static Fluid.Internals.Operations;

namespace Fluid.Internals.Development
{
    /// <summary>Writes out messages to console or file as an aesthetic output table.</summary>
    internal class ReportWriter
    {
        /// <summary>Width of column inside report table where message is displayed.</summary>
        const int DefaultMessageWidth = 90;
        /// <summary>Maps column names to their indices.</summary><typeparam name="string">Column name.</typeparam><typeparam name="int">Column index.</typeparam>
        readonly SCG.Dictionary<string, int> _ColPositions;
        /// <summary>Default column widths</summary>
        readonly int[] _DefaultColWidths = new int[] {8, 85, 60, 25, 4};
        /// <summary>Widths of columns</summary>
        int[] _ColWidths;


        /// <summary>StringBuilder.</summary>
        StringBuilder StrBuilder { get; } = new StringBuilder(500);                       // TODO: After you're done with this class, check if this is needed at all.
        /// <summary>Writes messages to file.</summary><param name="FileInfo("log.txt"">Log file.</param>
        StreamWriter Writer { get; }

        AppReporter AppReporter { get; }



        /// <summary>Create an object which writes out messages to console or file as an aesthetic output table.</summary>
        public ReportWriter(AppReporter appReporter) {
            _ColWidths = _DefaultColWidths;
            _ColPositions = new SCG.Dictionary<string, int>(5);
            int pos = 0;
            _ColPositions["DT"] = pos;
            pos += _ColWidths[0] + 2;
            _ColPositions["Msg"] = pos;
            pos += _ColWidths[1] + 2;
            _ColPositions["Path"] = pos;
            pos += _ColWidths[2] + 2;
            _ColPositions["Caller"] = pos;
            pos += _ColWidths[3] + 2;
            _ColPositions["Line"] = pos;

            Writer = new StreamWriter(new FileInfo("log.txt").FullName, true);
            AppReporter = appReporter;
        }

        /// <summary>Take a Message and return formatted lines.</summary><param name="message">Message to format.</param>
        List<string> FormatMessage(Message message) {
            var wrappedDT = message.DT.TotalSeconds.ToString("G3").WrapToLines(_ColWidths[0]);      // Wrap one-line strings. Format TimeSpan then wrap resulting string.
            var wrappedMsg = message.Msg.WrapToLines(_ColWidths[1]);
            var wrappedPath = message.Path.WrapToLines(_ColWidths[2]);
            var wrappedCaller = message.Caller.WrapToLines(_ColWidths[3]);
            var wrappedLine = message.Line.ToString().WrapToLines(_ColWidths[4]);
            
            var listOfLists = new List<List<string>>(5) { wrappedDT, wrappedMsg, wrappedPath, wrappedCaller, wrappedLine };     // Find how many lines has the element with most lines.
            var listOfLengths = listOfLists.Select((element) => element.Count);
            int maxNLines = listOfLengths.Max();
            var formattedLines = new List<string>(maxNLines);                                                                   // Result is going to appear here.

            for(int i = 0; i < maxNLines; ++i) {                                            // Over all lines.
                StrBuilder.Clear();

                for(int j = 0; j < 5; ++j) {                                                // Over all columns.
                    
                    if(listOfLists[j].Count > i) {                                          // List<string> j has an element to output at line i.
                        StrBuilder.Append(listOfLists[j][i].PadRight(_ColWidths[j] + 2));
                    }
                    else {
                        StrBuilder.Append(new string(' ', _ColWidths[j] + 2));                 // Add empty space
                    }
                }
                formattedLines.Add(StrBuilder.ToString());
            }
            return formattedLines;
        }

        /// <summary>Write a message to all outputs. Used by AppReporter.</summary><param name="message">Message.</param>
        public void WriteMessage(Message message) {
            var formattedLines = FormatMessage(message);

            if((AppReporter.Output & OutputSettings.Console) == OutputSettings.Console) {
                foreach(var line in formattedLines)
                    Console.WriteLine(line);
            }

            if((AppReporter.Output & OutputSettings.File) == OutputSettings.File) {
                
                foreach(var line in formattedLines)
                    Writer.WriteLine(line);

                Writer.Flush();
            }
        }

        ~ReportWriter() {
            Writer.Dispose();
        }
    }
}