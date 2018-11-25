using System;
using SC = System.Collections;
using SCG = System.Collections.Generic;

namespace Fluid.Internals.Collections
{
    public abstract class ListBase<T> : SCG.IList<T>
    {
        protected static SCG.EqualityComparer<T> _Comparer;

        /// <summary>Internal storage array.</summary>
        protected T[] _E;
        /// <summary>Number of elements inside internal storage array.</summary>
        protected int _Count;
        /// <summary>Number of elements inside internal storage array.</summary>
        public int Count => _Count;
        public bool IsReadOnly => false;


        static ListBase() {
            _Comparer = SCG.EqualityComparer<T>.Default;
        }

        public ListBase(int capacity) {
            _E = new T[capacity];
            _Count = 0;
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
            for (int i = 0; i < _Count; i++) {
                if (_Comparer.Equals(_E[i], element)) return i;
            }
            return -1;
        }
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
            for (int i = 0; i < _Count; i++)
                yield return _E[i];
        }

        SC.IEnumerator SC.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}
