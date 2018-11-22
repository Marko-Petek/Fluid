using System;

namespace Fluid.Internals.Collections
{
    public class EquatableList<T> : List<T>, IEquatable<List<T>>
    where T : IEquatable<T> {


        /// <summary>Create list with default initial capacity of internal array.</summary>
        public EquatableList() : base() { }

        /// <summary>Create list with specified initial capacity of internal array.</summary><param name="capacity">Initial capacity of internal array</param>
        public EquatableList(int capacity) : base(capacity) { }

        /// <summary>Create a copy of specified source list.</summary><param name="sourceList">Source list to copy from.</param>
        public EquatableList(EquatableList<T> sourceList) : base(sourceList.Count) {
            Array.Copy(sourceList._elements, _elements, sourceList.Count);
        }


        public bool Equals(List<T> other) {
            var thisList = (ListBase<T>) this;
            var otherList = (ListBase<T>) other;
            return thisList.Equals(other);
        }
    }
}