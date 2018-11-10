using System;
using Fluid.Dynamics.Internals;
using static System.Console;
using static Fluid.Dynamics.Internals.ExecutionReporter;
using static Fluid.Dynamics.Internals.ExecutionReporter.VerbositySettings;

namespace Fluid.ChannelFlow
{
    class Program
    {
        static void Main(string[] args)
        {
            #if REPORT
            ExecutionReporter.Start();
            ExecutionReporter.VerbositySetting = Obnoxious;
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
            // TODO: Implement logging (and console messages) to view progression of calculations.
        }
    }
}