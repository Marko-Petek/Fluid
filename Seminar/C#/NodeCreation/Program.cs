using System;
using System.Text;
using System.Linq;
using static System.Math;

using Fluid.Internals;
using TB = Fluid.Internals.Toolbox;

namespace Fluid.Seminar.NodeCreation
{
    class Program
    {
        static void NodeCreation() {

            var nodes = (                                                                   // Create a table of Nodes for instructive example.
                from row in Enumerable.Range(0,10)
                select (
                    from col in Enumerable.Range(0,10)
                    select new double[] {-3.0 + col * (6.0/9), -3.0 + row * (6.0/9)}
                ).ToArray()
            ).ToArray();

            TB.Reporter.Write("Created nodes array.");
            TB.FileWriter.SetFile("Seminar/Mathematica/nodes.txt", false);
            TB.FileWriter.WriteLine(nodes);
            TB.Reporter.Write($"Written nodes array to {TB.FileWriter.File.FullName}");
            TB.Rng.SetRange(-0.16, 0.16);                                                     // Set RNG range.

            var transNodes = (
                from row in nodes.Take(9).Skip(1)
                select (
                    from col in row.Take(9).Skip(1)
                    select new double[] {col[0] + TB.Rng.Double(), col[1] + TB.Rng.Double()}
                ).ToArray()
            ).ToArray();

            var transNodes2 = (
                transNodes.Select( (row,i) =>
                    Enumerable.Repeat(nodes[i+1][0], 1).Concat(row).Append(nodes[i+1][9]).ToArray()
                )
            ).ToArray();

            var transNodes3 = (
                Enumerable.Repeat(nodes[0], 1).Concat(transNodes2).Append(nodes[9]).ToArray()
            ).ToArray();

            TB.Reporter.Write("Created transNodes array.");
            TB.FileWriter.SetFile("Seminar/Mathematica/transNodes.txt", false);
            TB.FileWriter.WriteLine(transNodes3);
            TB.Reporter.Write($"Written transNodes array to {TB.FileWriter.File.FullName}");
        }



        static void Main(string[] args) {
            try {  
                NodeCreation();
            }
            catch(Exception exc) {
                TB.Reporter.Write($"Exception occured:  {exc.Message}");
                TB.Reporter.Write($"Stack trace:  {exc.StackTrace}");
                throw exc;                                                      // Rethrow.
            }
            finally {
                TB.Reporter.Write("Exiting application.");
                TB.FileWriter.Flush();
            }
        }
    }
}
