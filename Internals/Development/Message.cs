using System;

namespace Fluid.Internals.Development
{
    /// <summary>A report message together with time stamp and call site info.</summary>
    public class Message
    {
        /// <summary>Time passed since application started.</summary>
        TimeSpan _DT;
        /// <summary>Message text.</summary>
        string _Text;
        /// <summary>Relative path to file from which call to AppReporter.Write was made.</summary>
        string _Path;
        /// <summary>Method from which AppReporter.Write was called.</summary>
        string _Caller;
        /// <summary>Line number in file from which call to AppReporter.Write was made.</summary>
        int _Line;

        /// <summary>Time passed since application started.</summary>
        public TimeSpan DT => _DT;
        /// <summary>Report message.</summary>
        public string Msg => _Text;
        /// <summary>Relative path to file from which call to AppReporter.Write was made.</summary>
        public string Path => _Path;
        /// <summary>Method from which AppReporter.Write was called.</summary>
        public string Caller => _Caller;
        /// <summary>Line number in file from which call to AppReporter.Write was made.</summary>
        public int Line => _Line;

        /// <summary>Create a Report.</summary><param name="dt">Time passed since run start.</param><param name="text">Message.</param><param name="path">Relative path to file.</param><param name="caller">Calling method from which call to Report() was made.</param><param name="line">Line in file from which call to Report() was made.</param>
        public Message(TimeSpan dt, string text, string path, string caller, int line) {
            _DT = dt;
            _Text = text;
            _Path = path;
            _Caller = caller;
            _Line = line;
        }
    }
}