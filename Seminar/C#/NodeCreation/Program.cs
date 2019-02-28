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

            var mainNodes2 = (                          // Create a table of Nodes for instructive example.
                from row in Range(-5,11)
                select (
                    from col in Range(-5,11)
                    select new double[] {col * (3.0/5), row * (3.0/5)}
                ).ToArray()
            ).ToArray();

            TB.Reporter.Write("Created an array!");
            TB.FileWriter.SetFile("Seminar/Mathematica/instructiveNodes.txt", false);
            TB.FileWriter.WriteLine(mainNodes2);
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
