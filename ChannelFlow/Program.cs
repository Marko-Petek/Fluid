using System;
using static System.Console;

using Fluid.Internals.Collections;
using Fluid.Internals.Development;
using static Fluid.Internals.Development.AppReporter;
using static Fluid.ChannelFlow.Program;

namespace Fluid.ChannelFlow
{    
    class Program
    {
        /// <summary>Program's main reporter.</summary>
        static AppReporter _Reporter;
        /// <summary>Program's main reporter.</summary>
        public static AppReporter Reporter => _Reporter;

        static void Main(string[] args)
        {
            try {
                _Reporter = new AppReporter(VerbositySettings.Obnoxious);
                var flow = new ChannelFlow(1.0, 0.05, 0.001);    // Start with velocity 1.0.
                Reporter.Write("Solving for a single time step and writing solution to NodeArray.");
                flow.SolveNextAndAddToNodeArray();
                Reporter.Write("Writing solution to file.");
                flow.WriteSolution(5);
            }
            catch(Exception exc) {
                Reporter.Write($"Exception occured:  {exc.Message}");
                Reporter.Write($"Stack trace:  {exc.StackTrace}");
                throw exc;                                          // Rethrow.
            }
        }
        // TODO: Set verbosity of unnecessary reports to Verbose or Obnoxious.
    }
}