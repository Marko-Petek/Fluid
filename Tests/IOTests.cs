using System;
using Xunit;

using Fluid.Internals;
using Fluid.Internals.Collections;
using TB = Fluid.Internals.Toolbox;

namespace Fluid.Tests
{
    public class IOTests
    {
        // [Fact] public void ReadAndParseArray() {
        //     TB.FileReader.SetDirAndFile("Tests/", "array1d", ".txt");
        //     var inputArray = TB.FileReader.ReadDoubleArray1d();
        //     var actualArray = new double[] {0, 7, 3, 8, 2, 4, 9, 11, 15};
        //     Assert.True(inputArray.Equals(actualArray, 0.000001));
        // }

        [Fact] public void HierarchyOutput() {
            var node4 = new RankedNode();
            var node5 = new RankedNode(node4);
            var node6 = new ValueNode<int>(node5, 9);
            var node7 = new ValueNode<int>(node5, 7);
            var node1 = new RankedNode(node4);
            var node2 = new ValueNode<int>(node1, 6);
            var node3 = new ValueNode<int>(node1, 3);
            
            var hier = new Hierarchy<int>(node4);
            TB.FileWriter.SetDirAndFile("Tests", nameof(hier), ".txt");
            TB.FileWriter.WriteLine(hier);

            hier.MakeTopNode(node5);
            TB.FileWriter.WriteLine(hier);

            hier.MakeTopNode(node4);
            TB.FileWriter.WriteLine(hier);

            TB.FileWriter.Flush();
        }
    }
}