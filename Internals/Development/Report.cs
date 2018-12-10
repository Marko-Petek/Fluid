using System;

using Fluid.Internals.Collections;

namespace Fluid.Internals.Development
{
    /// <summary>Stores report messages for a single application run. Receives orders from AppReporter. Is not responsible for producing output, see ReportWriter for that.</summary>
    public class Report
    {
        /// <summary>Start time of application run.</summary>
        DateTime _StartTime;
        /// <summary>All messages written within a single application run.</summary>
        List<Message> _Messages;
        /// <summary>Time of most recently displayed message.</summary>
        DateTime _TimeLastMessage;

        
        /// <summary>Start time of application.</summary>
        public DateTime StartTime => _StartTime;
        /// <summary>All messages written within a single application run.</summary>
        public List<Message> Messages => _Messages;
        /// <summary>Time of most recently displayed message.</summary>
        public DateTime TimeLastMessage => _TimeLastMessage;


        /// <summary>Create a Report with specified application start time.</summary><param name="startTime">Application start time.</param>
        public Report(DateTime startTime) {
            _StartTime = startTime;
            _Messages = new List<Message>(10);
            _TimeLastMessage = startTime;
        }


        // /// <summary>Add a message to internal list of messages.</summary><param name="text">Message text.</param><param name="path">Path to file from which call to Report() was made.</param><param name="caller">Method from which Report() was called.</param><param name="line">Line number in file from which call to Report was made.</param>
        // public void NewMessage(DateTime time, string text, string path, string caller, int line) {
        //     var message = new Message(time - StartTime, text, path, caller, line);
        //     Messages.Add(message);
        // }
    }
}