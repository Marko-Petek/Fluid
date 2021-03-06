using System;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

using Fluid.Internals.IO;
using Fluid.Internals.Algebras;
using Fluid.Internals.Numerics;
using Fluid.Internals.Networks.Old;
using static Fluid.Internals.Toolbox;
using static Fluid.Internals.Numerics.MatOps;
namespace Fluid.Tests {
using dbl = Double;

public class InputOutput {
   [Fact] public void ReadAndParseArray() {
         FReader.SetDirAndFile("Tests/", "array1d", ".txt");
         var inputArray = (double[]) FReader.ReadArray<double>();
         var actualArray = new double[] {0, 7, 3, 8, 2, 4, 9, 11, 15};
         Assert.True(inputArray.Equals<dbl,DblA>(actualArray, 0.000001));
   }

   [Fact] public void HierarchyOutput() {
      var node4 = new RankedNode();
      var node5 = new RankedNode(node4);
      var node6 = new ValueNode<int>(node5, 9);
      var node7 = new ValueNode<int>(node5, 7);
      var node1 = new RankedNode(node4);
      var node2 = new ValueNode<int>(node1, 6);
      var node3 = new ValueNode<int>(node1, 3);
      var node8 = new RankedNode(node4);
      var node9 = new ValueNode<int>(node8, 5);
      var node10 = new ValueNode<int>(node8, 2);
      var hier = new Hierarchy<int>(node4);
      // T.FileWriter.SetDirAndFile("Tests", nameof(hier), ".txt");
      // T.FileWriter.WriteLine(hier);
      hier.MakeTopNode(node5);
      // T.FileWriter.WriteLine(hier);
      hier.MakeTopNode(node4);
      // T.FileWriter.WriteLine(hier);
      // T.FileWriter.Flush();
      var strw = new StringWriter();
      strw.Write<int>(hier);
      Assert.True(strw.ToString() == "{{9, 7}, {6, 3}, {5, 2}}");
   }

   [Fact] public void HierarchyInput() {
      FReader.SetDirAndFile("Tests", "hierToRead", ".txt");
      var hierWriteBack = FReader.ReadHierarchy<double>();
      var strw = new StringWriter();
      strw.Write<dbl>(hierWriteBack);
      Assert.True(strw.ToString() == "{{9, 7}, {6, 3}, {5, 2}}");
      var array = hierWriteBack.ConvertToArray();
      if(array != null) {
         var result = (double[][]) array;
         var expected = new double[][] {
            new double[] {9,7},
            new double[] {6,3},
            new double[] {5,2} };
         Assert.True(result.Equals<dbl,DblA>(expected, 0.000001)); }
      else
         Assert.True(false, "Could not convert hierarchy to array.");
   }

   [Fact] public void HierarchyInput2() {
      FReader.SetDirAndFile("Tests", "hierToRead2", ".txt");
      var hierWriteBack = FReader.ReadHierarchy<double>();
      var array = hierWriteBack.ConvertToArray();
      if(array != null) {
         var result = (double[][][]) array;
         var expected = new double[][][] {
            new double[][] {
               new double[] {7,3},
               new double[] {0,5} },
            new double[][] {
               new double[] {3,5},
               new double[] {4,2},
               new double[] {8,1} },
            new double[][] {
               new double[] {11,88},
               new double[] {33,56},
               new double[] {96,28},
               new double[] {28,51} } };
         Assert.True(result.Equals<dbl,DblA>(expected, 0.000001)); }
      else
         Assert.True(false, "Could not convert hierarchy to array.");
   }
}
}