using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Fluid.Internals.IO;
using static Fluid.Internals.Development.Reporter;
using dbl = System.Double;
using static Fluid.Internals.Tools;
using System.Collections.Specialized;

namespace Fluid.Internals.Development
{

  /// <summary>Writes out messages to console or file as an aesthetic output table.</summary>
  internal class ReportWriter : IDisposable {
   /// <summary>Widths of columns</summary>
   int[] ColWidths { get; }
   int NCols => ColWidths.Length;
   /// <summary>Previously (most recently) reported time.</summary>
   System.DateTime _PrevReportedTime;
   /// <summary>StringBuilder.</summary>
   StringBuilder StrBuilder { get; } = new StringBuilder(500);
   FConsole Writer { get; }
   /// <summary>Writes messages to file.</summary><param name="FileInfo("log.txt"">Log file.</param>
   FileWriter FileWriter { get; }
   Reporter AppReporter { get; }

   /// <summary>Create an object which writes out messages to console or file as an aesthetic output table.</summary>
   public ReportWriter(Reporter appReporter, FConsole writer) {
      Writer = writer;
      int maxWidth = 120;                                      // Maximum width so that the table is not over-stretched.
      int bufrWidth;                                           // Terminal width.
      int nCols = (int)Cols.LineNumber + 1;                    // Last entry + 1.
      if(writer.BufferWidth > maxWidth)                        // If the existing space is to wide, limit it.
         bufrWidth = maxWidth;
      else if(writer.BufferWidth > nCols * 2 - 12 + 36 + 20)            // Wrap each column at buffer width if buffer is wide enough.
         bufrWidth = writer.BufferWidth - 6 * 2 - 3;              // spaces, some slack
      else
         bufrWidth = 36 + 60;
      // First, assign width of columns with fixed widths.
      int msgrWidth = 11;
      int timeWidth = 5;
      int durationWidth = 8;
      int callerWidth = 20;
      int lineNumberWidth = 4;
      // Now, assign the remaining width to other columns.
      int remainder = bufrWidth - msgrWidth - timeWidth - durationWidth - callerWidth - lineNumberWidth;
      //
      dbl textToPathRatio = 0.55;
      int textWidth = (int)(textToPathRatio * remainder);
      int pathWidth = (int)((1.0 - textToPathRatio) * remainder);
      // Collect width values.
      ColWidths = new int[] { msgrWidth, timeWidth, textWidth, durationWidth, pathWidth, callerWidth, lineNumberWidth };
      FileWriter = new FileWriter("", "report", ".txt", true);
      AppReporter = appReporter;
      _PrevReportedTime = AppReporter.StartTime;
   }

   /// <summary>Take a Message and return formatted lines.</summary><param name="message">Message to format.</param>
   List<string> FormatMessage(int i) {
      var message = AppReporter.Report.Messages[i];
      List<string> wrappedMsngr = message.Messenger.WrapToLines(ColWidths[(int)Cols.Messenger]);
      var prevMessageTime = (i != 0) ? AppReporter.Report.Messages[i-1].Time : message.Time;
      List<string> wrappedTime;
      if(i == 0 || message.Time - _PrevReportedTime > TimeSpan.FromMinutes(1))                       // Only display time if it differs from previous time.
         wrappedTime = message.Time.ToShortTimeString().WrapToLines(ColWidths[(int)Cols.Time]);      // Wrap one-line strings. Format TimeSpan then wrap resulting string.
      else
         wrappedTime = new List<string> { " " };
      _PrevReportedTime = message.Time;
      var wrappedText = message.Text.WrapToLines(ColWidths[(int)Cols.Text]);
      System.TimeSpan dt = (message.Time - prevMessageTime);
      var wrappedDT = dt.TotalSeconds.ToString("G3").WrapToLines(ColWidths[(int)Cols.Duration]);      // Wrap one-line strings. Format TimeSpan then wrap resulting string.
      var relPath = Regex.Match(message.Path, @"Fluid.*").Value;
      relPath = relPath.Remove(0, 5);
      var wrappedPath = relPath.WrapToLines(ColWidths[(int)Cols.File]);
      var wrappedCaller = message.Caller.WrapToLines(ColWidths[(int)Cols.Caller]);
      var wrappedLine = message.Line.ToString().WrapToLines(ColWidths[(int)Cols.LineNumber]);
      var listOfLists = new List<List<string>>(NCols) { wrappedMsngr, wrappedTime, wrappedText, wrappedDT,
         wrappedPath, wrappedCaller, wrappedLine };                                          // Find how many lines has the element with most lines.
      var listOfLengths = listOfLists.Select(elm => elm.Count);
      int maxNLines = listOfLengths.Max();
      var formattedLines = new List<string>(maxNLines);                                                                   // Result is going to appear here.
      for(int j = 0; j < maxNLines; ++j) {                                            // Over all lines.
         StrBuilder.Clear();
         for(int k = 0; k < NCols; ++k)                                                // Over all columns.
            if(listOfLists[k].Count > j)                                          // List<string> j has an element to output at line i.
               StrBuilder.Append(listOfLists[k][j].PadRight(ColWidths[k] + 2));
            else
               StrBuilder.Append(new string(' ', ColWidths[k] + 2));                 // Add empty space
         formattedLines.Add(StrBuilder.ToString()); }
      return formattedLines;
   }
   /// <summary>Write a message to all outputs. Used by AppReporter.</summary><param name="i">Index of message inside Report.</param>
   public void WriteMessage(int i) {
      var formattedLines = FormatMessage(i);
      if(AppReporter.Output.HasFlag(OutputSettings.Console)) {
         foreach(var line in formattedLines)
            Writer.WriteLine(line); }
      if(AppReporter.Output.HasFlag(OutputSettings.File)) {
         foreach(var line in formattedLines)
            FileWriter.WriteLine(line);
         FileWriter.Flush(); }
   }

   public void Write(string s = "") {
      if(AppReporter.Output.HasFlag(OutputSettings.Console))
         Writer.Write(s);
      if(AppReporter.Output.HasFlag(OutputSettings.File)){
         FileWriter.Write(s);
         FileWriter.Flush(); }
   }

   public void WriteLine(string s = "") {
      Write(s + Environment.NewLine);
   }

   public void Dispose() {
      GC.SuppressFinalize(this);
      FileWriter?.Dispose();
   }
   ~ReportWriter() => Dispose();
}
}