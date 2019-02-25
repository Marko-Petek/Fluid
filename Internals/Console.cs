using System;
using System.IO;
using SysConsole = System.Console;

namespace Fluid.Internals
{
    public class Console
    {
        TextWriter _TW;
        TextWriter TW => _TW;

        public Console() {
            _TW = SysConsole.Out;
        }

        public void Write<T>(T[] array1d) => IO.Write(array1d, TW);
        public void Write<T>(T[][] array2d) => IO.Write(array2d, TW);
        public void Write<T>(T[][][] array3d) => IO.Write(array3d, TW);
        public void Write<T>(T[][][][] array4d) => IO.Write(array4d, TW);
        public void Write<T>(T[][][][][] array5d) => IO.Write(array5d, TW);
        public void Write<T>(T[][][][][][] array6d) => IO.Write(array6d, TW);
}