using System;

using Fluid.Internals.Collections;

namespace Fluid.Internals.Development
{
    /// <summary>Data storage for report messages inside a single application run.</summary>
    internal class Report
    {
        /// <summary>Time of most recently displayed message.</summary>
        static DateTime TimeOfLastReport { get; set; } = DateTime.Now;


        /// <summary>Start time of application run.</summary>
        DateTime _StartTime;
        /// <summary>All messages written within a single report (an application run).</summary>
        List<Message> _Messages;

        
        /// <summary>Start time of application run.</summary>
        public DateTime StartTime => _StartTime;
        /// <summary>All messages written within a single report (an application run).</summary>
        public List<Message> Messages => _Messages;


        /// <summary>Create a Report with specified application start time.</summary><param name="startTime">Application start time.</param>
        public Report(DateTime startTime) {
            _StartTime = startTime;
        }


        /// <summary>Add a message to internal list of messages.</summary><param name="text">Report message.</param><param name="path">Relative path to file from which call to Report() was made.</param><param name="caller">Method from which Report() was called.</param><param name="line">Line number in file from which call to Report was made.</param>
        public void AddMessage(DateTime time, string text, string path, string caller, int line) {
            var message = new Message(time - StartTime, text, path, caller, line);
            Messages.Add(message);
        }
    }
}