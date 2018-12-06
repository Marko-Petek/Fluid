using System;
using static System.Console;

using Fluid.Internals.Collections;
using Fluid.Internals.Development;
using static Fluid.Internals.Development.AppReporter;
#if REPORT
using static Program.Reporter;
#endif

namespace Fluid.ChannelFlow
{    
    class Program
    {
#if REPORT
        /// <summary>Program's main reporter.</summary>
        static AppReporter _Reporter;
        /// <summary>Program's main reporter.</summary>
        public static AppReporter Reporter => _Reporter;


        public Program() {
            _Reporter = new AppReporter(VerbositySettings.Obnoxious);
        }
#endif

        static void Main(string[] args)
        {
            try {
                var flow = new ChannelFlow(1.0, 0.05, 0.001);    // Start with velocity 1.0.
#if REPORT
                Report("Solving for a single time step and writing solution to NodeArray.");
#endif
                flow.SolveNextAndAddToNodeArray();
#if REPORT
                Report("Writing solution to file.");
#endif
                flow.WriteSolution(5);
            }
            catch(Exception exc) {
#if REPORT
                Report($"Exception occured:  {exc.Message}");
                Report($"Stack trace:  {exc.StackTrace}");
#endif
                throw exc;                                          // Rethrow.
            }
        }
        // TODO: Set verbosity of unnecessary reports to Verbose or Obnoxious.
    }
}