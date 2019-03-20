using System;
using Xunit;

using Fluid.Internals;
using TB = Fluid.Internals.Toolbox;

namespace Fluid.Tests
{
    public class IOTests
    {
        [Fact] public void ReadAndParseArray() {
            TB.FileReader.SetDirAndFile("Tests/", "array1d", ".txt");
            var inputArray = TB.FileReader.ReadDoubleArray1d();
            var actualArray = new double[] {0, 7, 3, 8, 2, 4, 9, 11, 15};
            Assert.True(inputArray.Equals(actualArray, 0.000001));
        }
    }
}