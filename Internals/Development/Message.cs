#nullable enable
using System;

namespace Fluid.Internals.Development {
   /// <summary>A report message together with time stamp and call site info.</summary>
   public class Message {
      /// <summary>Time passed since application started.</summary>
      DateTime _Time;
      /// <summary>Time passed since application started.</summary>
      public DateTime Time => _Time;
      /// <summary>Message text.</summary>
      public string Text { get; protected set; }
      /// <summary>Relative path to file from which call to AppReporter.Write was made.</summary>
      public string Path { get; protected set; }
      /// <summary>Method from which AppReporter.Write was called.</summary>
      public string Caller { get; protected set; }
      /// <summary>Line number in file from which call to AppReporter.Write was made.</summary>
      public int Line { get; protected set; }

      /// <summary>Create a Report.</summary><param name="dt">Time passed since run start.</param><param name="text">Message.</param><param name="path">Relative path to file.</param><param name="caller">Calling method from which call to Report() was made.</param><param name="line">Line in file from which call to Report() was made.</param>
      public Message(DateTime time, string text, string path, string caller, int line) {
         _Time = time;
         Text = text;
         Path = path;
         Caller = caller;
         Line = line;
      }
   }
}
#nullable restore