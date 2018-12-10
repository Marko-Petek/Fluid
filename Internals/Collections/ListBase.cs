using System;
using SC = System.Collections;
using SCG = System.Collections.Generic;

namespace Fluid.Internals.Collections
{
    public abstract class ListBase<T> : SCG.IList<T>
    {
        /// <summary>Comparer used by IndexOf(), Contains(), Remove() methods.</summary>
        protected static SCG.EqualityComparer<T> _Comparer;
        /// <summary>Comparer used by IndexOf(), Contains(), Remove() methods.</summary>
        public static SCG.EqualityComparer<T> Comparer => _Comparer;

        /// <summary>Internal storage array.</summary>
        protected T[] _E;
        /// <summary>Number of elements inside internal storage array.</summary>
        protected int _Count;
        /// <summary>Number of elements inside internal storage array.</summary>
        public int Count => _Count;
        /// <summary>Index of element at which most recent operation took place.</summary>
        protected int _RecentIndex;
        /// <summary>Index of element at which most recent operation took place.</summary>
        public int RecentIndex => _RecentIndex;
        public bool IsReadOnly => false;

        /// <summary>In derived classes we can set another comparer (also in static constructor).</summary>
        static ListBase() {
            _Comparer = SCG.EqualityComparer<T>.Default;
        }

        /// <summary>Create a ListBase with specified initial capacity of internal array.</summary><param name="capacity">Initial capacity of internal array. If < 0 then internal array is not constructed.</param>
        public ListBase(int capacity = 6) {
            _Count = 0;
            _RecentIndex = 0;

            if(capacity > 0)
                _E = new T[capacity];
        }

        public abstract T this[int index] { get; set; }
        public abstract void Add(T element);
        public abstract void AddRange(SCG.IList<T> elements);
        public abstract void Insert(int index, T element);
        public abstract bool Remove(T element);
        public abstract void RemoveAt(int index);
        public abstract void Clear();

        /// <summary>Performs a linear search, starting at index 0, and returns first element that matches, based on type T's comparer.</summary><param name="element">Element to search for.</param>
        public int IndexOf(T element) {

            for (int i = 0; i < Count; i++) {

                if (Comparer.Equals(_E[i], element))
                    return i;
            }
            return -1;
        }
        /// <summary>Searches for an element using type T's comparer and returns its index. Returns -1 if element not found.</summary><param name="elm">Element to find.</param><param name="stopAfter">Number of elements after which the search stops.</param><param name="start">Index at which to start searching.</param><param name="srchDir">Direction in which to start searching (either -1 or +1).</param>
        public int IndexOf(T elm, int stopAfter, int start = 0, int srchDir = 1) {
            int modIndex;                                                           // Wrapped around index.
            int modStart = Count - 1 + start;                                         // Additive constant used when calculating modIndex. Add Count-1 to operand so we don't get negative numbers.
            
            for (int i = 0; i < stopAfter; i++) {
                modIndex = (modStart + srchDir * i) % Count;

                if (Comparer.Equals(_E[modIndex], elm))
                    return i;
            }
            return -1;
        }
        /// <summary>Searches for an element using type T's comparer and returns its index. Returns -1 if element not found.</summary><param name="elm">Element to find.</param><param name="start">Index at which to start searching.</param><param name="srchDir">Direction in which to start searching (either -1 or +1).</param>
        public int IndexOf(T elm, int start = 0, int srchDir = 1) => IndexOf(elm, Count, start, srchDir);
        /// <summary>Searches for an element using type T's comparer and returns its index. Returns -1 if element not found.</summary><param name="elm">Element to find.</param><param name="srchDir">Direction in which to start searching (either -1 or +1).</param>
        public int IndexOf(T elm, int srchDir) => IndexOf(elm, Count, RecentIndex, srchDir);            // TODO: Comment arguments' effects in summary.

        /// <summary>Searches for an element and returns true if it is found.</summary>
        public bool Contains(T element) {
            for (int i = 0; i < _Count; i++) {
                if (_Comparer.Equals(_E[i], element)) return true;
            }
            return false;
        }
        /// <summary>Copies the contents of the List to an array. Starts filling the array at the specified index.</summary>
        public void CopyTo(T[] destinationArray, int arrayStartIndex) {
            if (destinationArray == null) throw new ArgumentNullException("The passed in array is null.");

            if (arrayStartIndex < 0) throw new IndexOutOfRangeException("Negative arrayStartIndex.");
            else if (destinationArray.Length - arrayStartIndex + 1 > _Count) {
                for (int i = 0; i < _Count; i++) {
                    destinationArray[arrayStartIndex + i] = _E[i];
                }
            }
            else throw new ArgumentException("arrayStartIndex too large.");
        }

        public SCG.IEnumerator<T> GetEnumerator() {
            foreach(T element in _E)
                yield return element;
        }

        SC.IEnumerator SC.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}
