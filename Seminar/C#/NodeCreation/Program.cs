using System;
using System.Text;
using System.Linq;
using static System.Linq.Enumerable;
using Fluid.Internals;
using Fluid.Internals.Development;
using static Fluid.Internals.Toolbox;

namespace Fluid.Seminar.NodeCreation
{
    class Program
    {
        static void Main(string[] args) {
            try {  
                var nodes = 
                Range(0,3).Select(row =>
                    Range(0,3).Select(col =>
                        new int[][] {
                            new int[] {-3 + 2*col,      -3 + 2*row + 1},
                            new int[] {-3 + 2*col + 1,  -3 + 2*row + 1},
                            new int[] {-3 + 2*col + 1,  -3 + 2*row},
                            new int[] {-3 + 2*col,      -3 + 2*row}
                        }
                    ).ToArray()
                ).ToArray();

                Reporter.Write("Created an array!");
                Drive.Write(nodes);
            }
            catch(Exception exc) {
                Reporter.Write($"Exception occured:  {exc.Message}");
                Reporter.Write($"Stack trace:  {exc.StackTrace}");
                throw exc;                                          // Rethrow.
            }
        }
    }
}
