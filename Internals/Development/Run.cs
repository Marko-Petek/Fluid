using System;

using Fluid.Internals.Collections;

namespace Fluid.Internals.Development
{
    /// <summary>Data storage for report messages inside a single application run.</summary>
    public class Run
    {

        /// <summary>Start time of application run.</summary>
        DateTime _Time;
        /// <summary>All reports made during an application run.</summary>
        List<Report> _Reports;
        
        /// <summary>Start time of application run.</summary>
        public DateTime Time => _Time;
        /// <summary>All reports made during an application run.</summary>
        public List<Report> Reports => _Reports;

        /// <summary>Add a new report to internal list.</summary><param name="msg">Report message.</param><param name="path">Relative path to file from which call to Report() was made.</param><param name="caller">Method from which Report() was called.</param><param name="line">Line number in file from which call to Report was made.</param>
        public void AddReport(string msg, string path, string caller, int line) {
            var report = new Report(DateTime.Now - Time, msg, path, caller, line);
            Reports.Add(report);
        }
    }
}