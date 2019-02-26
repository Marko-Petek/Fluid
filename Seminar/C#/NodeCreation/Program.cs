using System;
using System.Text;
using System.Linq;
using static System.Linq.Enumerable;
using Fluid.Internals;
using TB = Fluid.Internals.Toolbox;

namespace Fluid.Seminar.NodeCreation
{
    class Program
    {
        static void NodeCreation() {
            var nodes =                                                 // Create all element nodes for instructive example.
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

                TB.Reporter.Write("Created an array!");
                TB.FileWriter.SetFile("Seminar/Mathematica/instructiveNodes.txt", false);
                TB.FileWriter.WriteLine(nodes);
        }



        static void Main(string[] args) {
            try {  
                NodeCreation();
            }
            catch(Exception exc) {
                TB.Reporter.Write($"Exception occured:  {exc.Message}");
                TB.Reporter.Write($"Stack trace:  {exc.StackTrace}");
                throw exc;                                          // Rethrow.
            }
            finally {
                TB.Reporter.Write("Exiting application.");
                TB.FileWriter.Flush();
            }
        }
    }
}
