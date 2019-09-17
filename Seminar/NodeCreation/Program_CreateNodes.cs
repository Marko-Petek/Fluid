using System;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using static System.Math;

using Fluid.Internals;
using static Fluid.Internals.Toolbox;

namespace Fluid.Seminar.NodeCreation {
   partial class Program {
      /// <summary>Create nodes that we use inside FemVisualization2d.nb and FemVisualization3d.nb</summary>
      static void CreateNodes() {
         var nodes =                                                                       // 10 by 10 array of nodes on a regular lattice.
            Enumerable.Range(0,10).Select( row =>
               Enumerable.Range(0,10).Select( col =>
                  new double[] {-3.0 + col * (6.0/9), -3.0 + row * (6.0/9)}
               ).ToArray()
            ).ToArray();
         FileWriter.SetDirAndFile("Seminar/Mathematica/", nameof(nodes), ".txt");
         FileWriter.WriteLine(nodes);
         var nodesM =                                                                      // Only the mid 2 by 2 = 4 nodes on a regular lattice.
            nodes.Take(7).Skip(3).Select(col =>
               col.Take(7).Skip(3)
               .ToArray()
            ).ToArray();
         FileWriter.WriteLine(nodesM, nameof(nodesM));
         Rng.SetRange(-0.6, 0.6);                                                // Set RNG range.
         var nodesC =                                                                    // Only corners of elements.
            nodes.Where( (row, i) => i % 3 == 0 )
            .Select( row =>
               row.Where( (col, j) => j % 3 == 0 ).ToArray()
            ).ToArray();
         FileWriter.WriteLine(nodesC, nameof(nodesC));
         var transNodesCM =                                                            // C = element corner nodes (only nodes on corners of elements)
            nodesC.Take(3).Skip(1).Select( row =>                        // Pick only those nodes that we will be moving. Take 2 inner rows (3,6).
               row.Take(3).Skip(1).Select( col =>                      // Take 2 inner nodes from each row (3,6).
                  col.Select( coord => coord + Rng.Dbl ).ToArray()
               ).ToArray()
            ).ToArray();
         FileWriter.WriteLine(transNodesCM, nameof(transNodesCM));
}  }  }