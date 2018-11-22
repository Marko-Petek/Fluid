using System;
using SC = System.Collections;
using SCG = System.Collections.Generic;

namespace Fluid.Internals.Collections
{
    public abstract class ListBase<T> : SCG.IList<T>
    {
        protected static SCG.EqualityComparer<T> _comparer;

        protected T[] _elements;
        protected int _count;
        public int Count => _count;
        public bool IsReadOnly => false;


        static ListBase() {
            _comparer = SCG.EqualityComparer<T>.Default;
        }

        public ListBase(int capacity) {
            _elements = new T[capacity];
            _count = 0;
        }

        public ListBase() : this(4) { }

        public abstract T this[int index] { get; set; }
        public abstract void Add(T element);
        public abstract void AddRange(SCG.IList<T> elements);
        public abstract void Insert(int index, T element);
        public abstract bool Remove(T element);
        public abstract void RemoveAt(int index);
        public abstract void Clear();

        public int IndexOf(T element) {
            for (int i = 0; i < _count; i++) {
                if (_comparer.Equals(_elements[i], element)) return i;
            }
            return -1;
        }
        /// <summary>Searches for an element and returns true if it is found.</summary>
        public bool Contains(T element) {
            for (int i = 0; i < _count; i++) {
                if (_comparer.Equals(_elements[i], element)) return true;
            }
            return false;
        }
        /// <summary>Copies the contents of the List to an array. Starts filling the array at the specified index.</summary>
        public void CopyTo(T[] destinationArray, int arrayStartIndex) {
            if (destinationArray == null) throw new ArgumentNullException("The passed in array is null.");

            if (arrayStartIndex < 0) throw new IndexOutOfRangeException("Negative arrayStartIndex.");
            else if (destinationArray.Length - arrayStartIndex + 1 > _count) {
                for (int i = 0; i < _count; i++) {
                    destinationArray[arrayStartIndex + i] = _elements[i];
                }
            }
            else throw new ArgumentException("arrayStartIndex too large.");
        }

        public SCG.IEnumerator<T> GetEnumerator() {
            for (int i = 0; i < _count; i++)
                yield return _elements[i];
        }

        SC.IEnumerator SC.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}
