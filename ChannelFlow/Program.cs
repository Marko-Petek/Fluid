using System;
using static System.Console;

using Fluid.Internals.Collections;
using static Fluid.Internals.Development.AppReporter;
using static Fluid.Internals.Development.AppReporter.VerbositySettings;

namespace Fluid.ChannelFlow
{    
    class Program
    {
        static void Main(string[] args)
        {
            try {
                #if REPORT
                    AppReporter.Start();
                    AppReporter.OutputSettings |= OutputSettingsEnum.WriteFilePath | OutputSettingsEnum.WriteLineNumber;
                    AppReporter.VerbositySetting = Obnoxious;
                #endif

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
                Report($"Exception occured:  {exc.Message}");
                Report($"Stack trace:  {exc.StackTrace}");
                throw exc;                                          // Rethrow.
            }
        }
        // TODO: Set verbosity of unnecessary reports to Verbose or Obnoxious.
        // TODO: Lear Regular Expressions to parse better.
    }
}