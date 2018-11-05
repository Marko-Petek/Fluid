using System;
using Fluid.Dynamics.Internals;
using static System.Console;

namespace Fluid.ChannelFlow
{
    class Program
    {
        static void Main(string[] args)
        {
            var flow = new ChannelFlow(1.0, 0.05, 0.001);    // Start with velocity 1.0.
            flow.SolveNextAndAddToNodeArray();
            flow.WriteSolution(5);
            // TODO: Implement logging (and console messages) to view progression of calculations.
        }
    }
}