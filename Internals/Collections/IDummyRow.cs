using System;

namespace Fluid.Internals.Collections
{
    internal interface IDummyRow<T>
    {
        SparseMat<T> Owner { get; set; }
        int Index { get; set; }
    }
}