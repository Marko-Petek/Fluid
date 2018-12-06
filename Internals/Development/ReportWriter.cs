using System.IO;
using System.Text;
using SCG = System.Collections.Generic;

using Fluid.Internals.Collections;

namespace Fluid.Internals.Development
{
    /// <summary>Stores a Run in form (array of lines) that easily yields an aesthetic output table.</summary>
    internal class ReportWriter
    {
        /// <summary>Width of column inside report table where message is displayed.</summary>
        const int DefaultMessageWidth = 90;
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
        /// <summary>Concatenates strings based on OutputSettings.</summary>
        StringBuilder StrBuilder { get; } = new StringBuilder(500);                       // TODO: After you're done with this class, check if this is needed at all.
        
        StreamWriter Writer { get; } = new StreamWriter(new FileInfo("log.txt").FullName, true);



        /// <summary>Create an object which transforms a Run into a formatted string (table).</summary>
        public ReportWriter() {
            _ColWidths = _DefaultColWidths;
            _ColPos = new int[5];

            for(int i = 0, currPos = 0; i < 5; ++i) {   // Initialize col pos elements.
                _ColPos[i] = currPos;
                currPos += _ColWidths[i] + 2;           // Cell border = 2 cols thick.
            }
        }

        /// <summary>Take a Message and return formatted lines.</summary><param name="message">Message to format.</param>
        List<string> FormatMessage(Message message) {

        }

        // TODO: 5.XII. Finish a method which wraps a single column and turns it into a string list.
        //List<string> Process    !!!!WRAP!!!!
    }
}