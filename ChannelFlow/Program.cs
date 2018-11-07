using System;
using Fluid.Dynamics.Internals;
using static System.Console;
using static Fluid.Dynamics.Internals.AppReporter;
using static Fluid.Dynamics.Internals.AppReporter.VerbositySettings;

namespace Fluid.ChannelFlow
{
    class Program
    {
        static void Main(string[] args)
        {
            AppReporter.Start();
            AppReporter.VerbositySetting = Obnoxious
            ;

            var flow = new ChannelFlow(1.0, 0.05, 0.001);    // Start with velocity 1.0.
            Report("Solving for a single time step and writing solution to NodeArray.");
            flow.SolveNextAndAddToNodeArray();
            Report("Write solution to file.");
            flow.WriteSolution(5);
            // TODO: Implement logging (and console messages) to view progression of calculations.
        }
    }
}