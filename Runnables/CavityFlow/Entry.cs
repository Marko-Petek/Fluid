using System;
using System.Linq;
using System.Diagnostics;
using static Fluid.Internals.Toolbox;
namespace Fluid.Runnables.CavityFlow {

   /// <summary>Driven cavity flow problem is a very simple problem with which methods are initially tested.</summary>
public static class Entry {
   public static int Point() {
      var cavFlow = CavityFlow.Create(0.1, 0.1, 3) ;
      return 0;
   }


   /// <summary>Create nodes that we use inside FemVisualization2d.nb and FemVisualization3d.nb</summary>
   public static void CreateNodes() {
      var nodes =                                                                       // 10 by 10 array of nodes on a regular lattice.
         Enumerable.Range(0,10).Select( row =>
            Enumerable.Range(0,10).Select( col =>
               new double[] {-3.0 + col * (6.0/9), -3.0 + row * (6.0/9)}
            ).ToArray()
         ).ToArray();
      T.FileWriter.SetDirAndFile("Seminar/Mathematica/", nameof(nodes), ".txt");
      T.FileWriter.WriteLine(nodes);
      var nodesM =                                                                      // Only the mid 2 by 2 = 4 nodes on a regular lattice.
         nodes.Take(7).Skip(3).Select(col =>
            col.Take(7).Skip(3)
            .ToArray()
         ).ToArray();
      T.FileWriter.WriteLine(nodesM, nameof(nodesM));
      T.Rng.SetRange(-0.6, 0.6);                                                // Set RNG range.
      var nodesC =                                                                    // Only corners of elements.
         nodes.Where( (row, i) => i % 3 == 0 )
         .Select( row =>
            row.Where( (col, j) => j % 3 == 0 ).ToArray()
         ).ToArray();
      T.FileWriter.WriteLine(nodesC, nameof(nodesC));
      var transNodesCM =                                                            // C = element corner nodes (only nodes on corners of elements)
         nodesC.Take(3).Skip(1).Select( row =>                        // Pick only those nodes that we will be moving. Take 2 inner rows (3,6).
            row.Take(3).Skip(1).Select( col =>                      // Take 2 inner nodes from each row (3,6).
               col.Select( coord => coord + Rng.Dbl ).ToArray()
            ).ToArray()
         ).ToArray();
      T.FileWriter.WriteLine(transNodesCM, nameof(transNodesCM));
   }


   /// <summary>Once transformed nodes have been created, generate array combinations appropriate for Mathematica plotting.</summary>
   public static void CreateDerivedData() {
      T.FileReader.SetDirAndFile("Seminar/Mathematica", "transNodesCM", ".txt");
      var transNodesCM = (double[][][]) T.FileReader.ReadArray<double>();
     T.FileReader.SetFile("nodesC", ".txt");
      var nodesC = (double[][][])T.FileReader.ReadArray<double>();
      var transNodesC1 =                                                            // Reassemble the rows (including static nodes).
         transNodesCM.Select( (row, i) =>                                              // Take the newly created array.
            new double[][] {nodesC[i + 1][0]}.Concat(row).Append(nodesC[i + 1][3]).ToArray()       // Attach a static node to the left and a static node to the right of each row.
         ).ToArray();
     T.FileWriter.SetDirAndFile("Seminar/Mathematica/", nameof(transNodesC1), ".txt");        // Have to set dir for the first time.
     T.FileWriter.WriteLine(transNodesC1);
      var transNodesC = transNodesC1.Prepend(nodesC[0]).Append(nodesC[3]).ToArray();      // Reassemble the array (including filtered static rows).
     T.FileWriter.WriteLine(transNodesC, nameof(transNodesC));
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
     T.FileWriter.WriteLine(transNodesCByElms, nameof(transNodesCByElms));
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
     T.FileWriter.WriteLine(transNodesByElms, nameof(transNodesByElms));
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
     T.FileWriter.WriteLine(transNodes, nameof(transNodes));
      var transNodesM = 
         transNodes.Where( (row, i) => i > 2 && i < 7)          // Take only middle nodes.
         .Select( row =>
            row.Where( (col, j) => j > 2 && j < 7)
            .ToArray()
         ).ToArray();
     T.FileWriter.WriteLine(transNodesM, nameof(transNodesM));
     T.FileReader.SetFile("nodes", ".txt");
      var nodes = (double[][][])T.FileReader.ReadArray<double>();
      var nodes3dF =
         nodes.Select( row =>
            row.Select( col =>
               col.Append(0.0).ToArray()
            ).ToArray()
         ).ToArray();
     T.FileWriter.WriteLine(nodes3dF, nameof(nodes3dF));
      var transNodes3dF =
         transNodes.Select( row =>
            row.Select( col =>
               col.Append(0.0).ToArray()
            ).ToArray()
         ).ToArray();
     T.FileWriter.WriteLine(transNodes3dF, nameof(transNodes3dF));
      var nodes3dMF =
         nodes.Take(7).Skip(3).Select( row =>
            row.Take(7).Skip(3).Select( col =>
               col.Append(0.0).ToArray()
            ).ToArray()
         ).ToArray();
     T.FileWriter.WriteLine(nodes3dMF, nameof(nodes3dMF));
      var nodes3dMT =
         nodes.Take(7).Skip(3).Select( row =>
            row.Take(7).Skip(3).Select( col =>
               col.Append(1.0).ToArray()
            ).ToArray()
         ).ToArray();
     T.FileWriter.WriteLine(nodes3dMT, nameof(nodes3dMT));
      var transNodes3dMF =
         transNodesM.Select( row =>
            row.Select( col =>
               col.Append(0.0).ToArray()
            ).ToArray()
         ).ToArray();
     T.FileWriter.WriteLine(transNodes3dMF, nameof(transNodes3dMF));
      var transNodes3dMT =
         transNodesM.Select( row =>
            row.Select( col =>
               col.Append(1.0).ToArray()
            ).ToArray()
         ).ToArray();
     T.FileWriter.WriteLine(transNodes3dMT, nameof(transNodes3dMT));
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
      for(int i = 0; i < 2; ++i)
         for(int j = 0; j < 4; ++j)
            trueCoords[i] += S[j] * quadCorners[j][i];
      return trueCoords;
   }
}

}
