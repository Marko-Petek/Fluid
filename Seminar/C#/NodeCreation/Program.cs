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

            TB.Rng.SetRange(-0.19, 0.19);                                                     // Set RNG range.

            var transNodes1 = (
                nodes.Take(9).Skip(1).
                Select( row =>
                    row.Take(9).Skip(1).
                    Select( col =>
                        new double[] {col[0] + TB.Rng.Double(), col[1] + TB.Rng.Double()}
                    ).ToArray()
                ).ToArray()
            ).ToArray();

            var transNodes2 = (
                transNodes1.Select( (row,i) =>
                    new double[][] {nodes[i+1][0]}.
                    Concat(row).
                    Append(nodes[i+1][9]).
                    ToArray()
                )
            ).ToArray();

            var transNodes = (
                new double[][][] {nodes[0]}.
                Concat(transNodes2).
                Append(nodes[9]).
                ToArray()
            ).ToArray();

            var nodes3dF = (
                nodes.Select( row =>
                    row.Select( col =>
                        new double[] {col[0], col[1], 0.0}
                     ).ToArray()
                ).ToArray()
            ).ToArray();

            var transNodes3dF = (
                transNodes.Select( row =>
                    row.Select( col =>
                        new double[] {col[0], col[1], 0.0}
                     ).ToArray()
                ).ToArray()
            ).ToArray();

            var nodes3dMF = (
                nodes.Take(7).Skip(3).
                Select( row =>
                    row.Take(7).Skip(3).
                    Select( col =>
                        new double[] {col[0], col[1], 0.0}
                    ).ToArray()
                ).ToArray()
            ).ToArray();

            var nodes3dMT = (
                nodes.Take(7).Skip(3).
                Select( row =>
                    row.Take(7).Skip(3).
                    Select( col =>
                        new double[] {col[0], col[1], 1.0}
                    ).ToArray()
                ).ToArray()
            ).ToArray();

            var transNodes3dMF = (
                transNodes.Take(7).Skip(3).
                Select( row =>
                    row.Take(7).Skip(3).
                    Select( col =>
                        new double[] {col[0], col[1], 0.0}
                    ).ToArray()
                ).ToArray()
            ).ToArray();

            var transNodes3dMT = (
                transNodes.Take(7).Skip(3).
                Select( row =>
                    row.Take(7).Skip(3).
                    Select( col =>
                        new double[] {col[0], col[1], 1.0}
                    ).ToArray()
                ).ToArray()
            ).ToArray();

            TB.FileWriter.SetFile("Seminar/Mathematica/nodes.txt", false);
            TB.FileWriter.WriteLine(nodes);
            //TB.Reporter.Write($"Written nodes array to {TB.FileWriter.File.FullName}");

            TB.FileWriter.SetFile("Seminar/Mathematica/transNodes.txt", false);
            TB.FileWriter.WriteLine(transNodes);
            TB.Reporter.Write($"Written transNodes array to {TB.FileWriter.File.FullName}");

            TB.FileWriter.SetFile("Seminar/Mathematica/nodes3dF.txt", false);
            TB.FileWriter.WriteLine(nodes3dF);

            TB.FileWriter.SetFile("Seminar/Mathematica/transNodes3dF.txt", false);
            TB.FileWriter.WriteLine(transNodes3dF);

            TB.FileWriter.SetFile("Seminar/Mathematica/nodes3dMF.txt", false);
            TB.FileWriter.WriteLine(nodes3dMF);

            TB.FileWriter.SetFile("Seminar/Mathematica/nodes3dMT.txt", false);
            TB.FileWriter.WriteLine(nodes3dMT);

            TB.FileWriter.SetFile("Seminar/Mathematica/transNodes3dMF.txt", false);
            TB.FileWriter.WriteLine(transNodes3dMF);

            TB.FileWriter.SetFile("Seminar/Mathematica/transNodes3dMT.txt", false);
            TB.FileWriter.WriteLine(transNodes3dMT);
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
