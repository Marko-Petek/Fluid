using System;

namespace Fluid.Dynamics.Internals
{
    public static class GeneralHelper
    {
        /// <summary>Swap two items.</summary><param name="first">First item.</param><param name="second">Second item.</param>
        public static void Swap<T>(ref T first, ref T second) {
            T temp = first;
            first = second;
            second = temp;
        }
    }
}