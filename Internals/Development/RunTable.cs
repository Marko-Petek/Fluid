using SCG = System.Collections.Generic;

using Fluid.Internals.Collections;

namespace Fluid.Internals.Development
{
    /// <summary>Can transform a Run into formatted form (table), appropriate for output to console or file.</summary>
    public class RunTable
    {
        /// <summary>Maps column names to their indices.</summary><typeparam name="string">Column name.</typeparam><typeparam name="int">Column index.</typeparam>
        readonly SCG.Dictionary<string, int> _ColIndices = new SCG.Dictionary<string, int>() {
            {"DT", 0},
            {"Msg", 1},
            {"Path", 2},
            {"Caller", 3},
            {"Line", 4}
        };
        /// <summary>Default column widths</summary>
        readonly int[] _DefaultColWidths = new int[] {8, 85, 60, 25, 4};


        /// <summary>Widths of columns</summary>
        int[] _ColWidths;
        /// <summary>Starting positions of columns.</summary>
        int[] _ColPos;
        /// <summary>Contains all reports created during a single application run.</summary>
        Run _Run;
        /// <summary>Output of a single run, line by line.</summary>
        List<string> _Lines;
        /// <summary>Index of most recently formatted report.</summary>
        int _RecentReport;


        /// <summary>Output of a single run, line by line.</summary>
        public List<string> Lines => _Lines;


        /// <summary>Create an object which transforms a Run into a formatted string (table).</summary>
        public RunTable(Run run) {
            _ColWidths = _DefaultColWidths;
            _ColPos = new int[5];
            int currPos = 0;

            for(int i = 0; i < 5; ++i) {                // Initialize col pos elements.
                _ColPos[i] = currPos;
                currPos += _ColWidths[i] + 2;           // Cell border = 2 cols thick.
            }
            _Run = run;
            _Lines = new List<string>();
            _RecentReport = -1;
        }

        /// <summary>Formats yet unadded Reports, adds formatted strings representing lines to internal Lines list and returns index range of updated lines.</summary>
        public (int,int) ProcessUnaddedReports() {

        }

        /// <summary>Take Report and return formatted lines.</summary>
        /// <param name="report">Report to format to lines.</param>
        List<string> ProcessReport(Report report) {

        }

        // TODO: 5.XII. Finisha method which wraps a single column and turns it into a string list.
        //List<string> Process    !!!!WRAP!!!!
    }
}