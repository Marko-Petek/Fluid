﻿using System;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using static System.Math;

using Fluid.Internals;
using TB = Fluid.Internals.Toolbox;

namespace Fluid.Seminar.NodeCreation
{
    class Program
    {
        static void NodeCreation() {

            var nodes = (                                                                       // 10 by 10 array of nodes on a regular lattice.
                Enumerable.Range(0,10).Select( row =>
                    Enumerable.Range(0,10).Select( col =>
                        new double[] {-3.0 + col * (6.0/9), -3.0 + row * (6.0/9)}
                    ).ToArray()
                ).ToArray()
            ).ToArray();
            string dir = "Seminar/Mathematica/";
            string ext = ".txt";
            TB.FileWriter.SetFile(dir + nameof(nodes) + ext);
            TB.FileWriter.WriteLine(nodes);

            var nodesM = (                                                                      // Only the mid 2 by 2 = 4 nodes on a regular lattice.
                nodes.Take(7).Skip(3).Select(col =>
                    col.Take(7).Skip(3)
                    .ToArray()
                ).ToArray()
            ).ToArray();
            TB.FileWriter.SetFile(dir + nameof(nodesM) + ext);
            TB.FileWriter.WriteLine(nodesM);

            TB.Rng.SetRange(-0.6, 0.6);

            var transNodesCM = (                                                            // C = element corner nodes (only nodes on corners of elements)
                nodes.Skip(3).Take(4).Where( (col, j) => j % 3 == 0 ).Select( row =>                        // Pick only those nodes that we will be moving. Take 2 inner rows (3,6).
                    row.Skip(3).Take(4).Where( (col, j) => j % 3 == 0).Select( col =>                      // Take 2 inner nodes from each row (3,6).
                        new double[] {col[0] + TB.Rng.Double(), col[1] + TB.Rng.Double()}
                    ).ToArray()
                ).ToArray()
            ).ToArray();
            TB.FileWriter.SetFile(dir + nameof(transNodesCM) + ext);
            TB.FileWriter.WriteLine(transNodesCM);

            var transNodesC1 = (                                                             // Reassemble the rows (including static nodes).
                transNodesCM.Select( (row,i) =>                                              // Take the newly created array.
                    new double[][] {nodes[3*i + 1][0]}.Concat(row).Append(nodes[3*i + 1][9]).       // Attach a static node to the left and a static node to the right of each row.
                    ToArray()
                )
            ).ToArray();
            TB.FileWriter.SetFile(dir + nameof(transNodesC1) + ext);
            TB.FileWriter.WriteLine(transNodesC1);

            var transNodesC = (                                                                          // Reassemble the array (including filtered static rows).
                new double[][][] {
                    nodes[0].Where( (col, i) => i % 3 == 0 ).ToArray()          // Append first filtered row.
                }.    
                Concat(transNodesC1).
                Append(nodes[9].Where( (col, i) => i % 3 == 0 ).ToArray())      // Append last filtered row.
            ).ToArray();
            TB.FileWriter.SetFile(dir + nameof(transNodesC) + ext);
            TB.FileWriter.WriteLine(transNodesC);

            var transNodesCByElms = Enumerable.Range(0,3).Select( row =>                                // Groups of 4 corner nodes for each element.
                Enumerable.Range(0,3).Select( col =>
                    new double[][] {
                        (double[])transNodesC[row][col].Clone(),
                        (double[])transNodesC[row][col + 1].Clone(),
                        (double[])transNodesC[row + 1][col + 1].Clone(),
                        (double[])transNodesC[row + 1][col].Clone()
                    }
                ).ToArray()
            ).ToArray();
            TB.FileWriter.SetFile(dir + nameof(transNodesCByElms) + ext);
            TB.FileWriter.WriteLine(transNodesCByElms);

            var transNodesByElms = transNodesCByElms.Select( row =>
                row.Select( elm =>
                    new double[][][] {
                        new double[][] {elm[0], TrueCoords(new double[] {-1.0/3, -1}, elm), TrueCoords(new double[] {1.0/3, -1}, elm), elm[1]},
                        new double[][] {TrueCoords(new double[] {-1, -1.0/3}, elm), TrueCoords(new double[] {-1.0/3, -1.0/3}, elm), TrueCoords(new double[] {1.0/3, -1.0/3}, elm), TrueCoords(new double[] {1, -1.0/3}, elm)},
                        new double[][] {TrueCoords(new double[] {-1, 1.0/3}, elm), TrueCoords(new double[] {-1.0/3, 1.0/3}, elm), TrueCoords(new double[] {1.0/3, 1.0/3}, elm), TrueCoords(new double[] {1, 1.0/3}, elm)},
                        new double[][] {elm[3], TrueCoords(new double[] {-1.0/3, 1}, elm), TrueCoords(new double[] {1.0/3, 1}, elm), elm[2]}
                    }
                ).ToArray()
            ).ToArray();
            TB.FileWriter.SetFile(dir + nameof(transNodesByElms) + ext);
            TB.FileWriter.WriteLine(transNodesByElms);

            var transNodes = Enumerable.Range(0,10).Select( row =>                  // Create an empty 10 by 10 array.
                Enumerable.Range(0,10).Select( col =>
                    new double[2] {0.0, 0.0}
                ).ToArray()
            ).ToArray();

            Enumerable.Range(0,3).Select( i =>                                    // This will fill the lower left part of the new array.
                Enumerable.Range(0,3).Select( j =>
                    Enumerable.Range(0,3).Select( k =>
                        Enumerable.Range(0,3).Select( m => 
                            transNodes[3*i + k][3*j + m] = (double[])transNodesByElms[i][j][k][m].Clone()
                        ).ToArray()
                    ).ToArray()
                ).ToArray()
            ).ToArray();

            Enumerable.Range(0,3).Select( i =>                                      // Over 3 elements in last row.
                Enumerable.Range(0,3).Select( j => {                                  // Over 3 nodes in last row of each element.
                    transNodes[9][3*i + j] = transNodesByElms[2][i][3][j];          // Add the upper most row.
                    transNodes[3*i + j][9] = transNodesByElms[i][2][j][3];          // Add the right most col.
                    return j;
                }).ToArray()
            ).ToArray();

            transNodes[9][9] = transNodesByElms[2][2][3][3];                        // Assign the last corner element.
            TB.FileWriter.SetFile(dir + nameof(transNodes) + ext);
            TB.FileWriter.WriteLine(transNodes);


            var transNodesM = transNodes.Where( (row, i) => i > 2 && i < 7          // Take only middle nodes.
                ).Where( (col, j) => j > 2 && j < 7).ToArray();

            var nodes3dF = (
                nodes.Select( row =>
                    row.Select( col =>
                        new double[] {col[0], col[1], 0.0}
                     ).ToArray()
                ).ToArray()
            ).ToArray();
            TB.FileWriter.SetFile(dir + nameof(nodes3dF) + ext);
            TB.FileWriter.WriteLine(nodes3dF);

            var transNodes3dF = (
                transNodes.Select( row =>
                    row.Select( col =>
                        new double[] {col[0], col[1], 0.0}
                     ).ToArray()
                ).ToArray()
            ).ToArray();
            TB.FileWriter.SetFile(dir + nameof(transNodes3dF) + ext);
            TB.FileWriter.WriteLine(transNodes3dF);

            var nodes3dMF = (
                nodes.Take(7).Skip(3).Select( row =>
                    row.Take(7).Skip(3).Select( col =>
                        new double[] {col[0], col[1], 0.0}
                    ).ToArray()
                ).ToArray()
            ).ToArray();
            TB.FileWriter.SetFile(dir + nameof(nodes3dMF) + ext);
            TB.FileWriter.WriteLine(nodes3dMF);

            var nodes3dMT = (
                nodes.Take(7).Skip(3).Select( row =>
                    row.Take(7).Skip(3).Select( col =>
                        new double[] {col[0], col[1], 1.0}
                    ).ToArray()
                ).ToArray()
            ).ToArray();
            TB.FileWriter.SetFile(dir + nameof(nodes3dMT) + ext);
            TB.FileWriter.WriteLine(nodes3dMT);

            var transNodes3dMF = (
                transNodesM.Select( row =>
                    row.Select( col =>
                        new double[] {col[0], col[1], 0.0}
                    ).ToArray()
                ).ToArray()
            ).ToArray();
            TB.FileWriter.SetFile(dir + nameof(transNodes3dMF) + ext);
            TB.FileWriter.WriteLine(transNodes3dMF);

            var transNodes3dMT = (
                transNodesM.Select( row =>
                    row.Select( col =>
                        new double[] {col[0], col[1], 1.0}
                    ).ToArray()
                ).ToArray()
            ).ToArray();
            TB.FileWriter.SetFile(dir + nameof(transNodes3dMT) + ext);
            TB.FileWriter.WriteLine(transNodes3dMT);
        }

        public static void StringReplace() {
            string input = @"{
  	Polygon[{nodes[[1, 1]], nodes[[1, 4]], nodes[[4, 4]], 
    nodes[[4, 1]]}],
  	Polygon[{nodes[[7, 1]], nodes[[7, 4]], nodes[[10, 4]], 
    nodes[[10, 1]]}],
  	Polygon[{nodes[[4, 4]], nodes[[4, 7]], nodes[[7, 7]], 
    nodes[[7, 4]]}],
  	Polygon[{nodes[[1, 7]], nodes[[1, 10]], nodes[[4, 10]], 
    nodes[[4, 7]]}],
  	Polygon[{nodes[[7, 7]], nodes[[7, 10]], nodes[[10, 10]], 
    nodes[[10, 7]]}]
  };";

            string pattern = @"nodes";
            string modified = Regex.Replace(input, pattern, "transNodes");
            TB.FileWriter.SetFile("Seminar/NodeCreation/modifiedString.txt");
            TB.FileWriter.Write(modified);
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

        /// <summary>First shape function.</summary>
        static double S1(params double[] refCoords) => 0.25*(1 - refCoords[0])*(1 - refCoords[1]);   // Epsilon, eta.
        /// <summary>Second shape function.</summary>
        static double S2(params double[] refCoords) => 0.25*(1 + refCoords[0])*(1 - refCoords[1]);
        /// <summary>Third shape function.</summary>
        static double S3(params double[] refCoords) => 0.25*(1 + refCoords[0])*(1 + refCoords[1]);
        /// <summary>Fourth shape function.</summary>
        static double S4(params double[] refCoords) => 0.25*(1 - refCoords[0])*(1 + refCoords[1]);
    
        static double[] TrueCoords(double[] refCoords, params double[][] quadCorners) {
            double[] S = new double[] {S1(refCoords), S2(refCoords), S3(refCoords), S4(refCoords)}; 
            var trueCoords = new double[2];

            for(int i = 0; i < 2; ++i) {
                for(int j = 0; j < 4; ++j) {
                    trueCoords[i] += S[j] * quadCorners[j][i];
                }
            }
            return trueCoords;
        }
    }
}
