using System;
using SCG = System.Collections.Generic;

using static Fluid.Internals.ArrayOperations;

namespace Fluid.Internals.Collections
{
    // IEquatable<List<T>>

    public class List<T> : ListBase<T>
    {
        /// <summary>Create list with default initial capacity of internal array.</summary>
        public List() : base() { }

        /// <summary>Create list with specified initial capacity of internal array.</summary><param name="capacity">Initial capacity of internal array</param>
        public List(int capacity = 6) : base(capacity) { }

        /// <summary>Create a copy of specified source list.</summary><param name="source">Source list to copy.</param>
        public List(List<T> source) : base(source.Count != 0 ? source.Count : 6) {
            Array.Copy(source._E, _E, source.Count);
        }

        /// <summary>Create a copy of specified source array as a list (convert to list).</summary><param name="source">Source array to copy from.</param>
        public List(T[] source) : base(source.Length) {
            Array.Copy(source, _E, source.Length);
        }

        /// <summary>Create a new list by absorbing in it a specified source array.</summary><param name="source">Source array to adopt.</param>
        public static List<T> CreateFromArray(T[] source) {
            var list = new List<T>(0);
            list._E = source;
            return list;
        }

        /// <summary>Absorb a specified source array as internal array.</summary><param name="source">Source array to absorb.</param>
        public void AbsorbArray(T[] source) {
            _E = source;
            _RecentIndex = 0;
        }

        /// <summary>Get a reference to element at specified index.</summary><param name="index">Element index.</param><returns>Element in list at specified index.</returns>
        public ref T E(int index) => ref _E[index];   // Bounds checking already performed by the runtime.

        /// <summary>Indexer</summary>
        public override T this[int index] {
            get {
                if (index > -1) {
                    if (index < _Count) {
                        return _E[index];
                    }
                    else throw new IndexOutOfRangeException("Index too large.");
                }
                else throw new IndexOutOfRangeException("Negative index.");
            }

            set {
                if (index > -1) {
                    if (index < _Count) {
                        _E[index] = value;
                    }
                    else throw new IndexOutOfRangeException("Index too big.");
                }
                else throw new IndexOutOfRangeException("Negative index.");
            }
        }
        /// <summary>Adds an element to the end of the list.</summary>
        public override void Add(T element) {
            EnsureArrayCapacity(ref _E, _Count + 1);
            _E[_Count] = element;
            ++_Count;
        }

        public override void AddRange(SCG.IList<T> elements) {
            EnsureArrayCapacity(ref _E, _Count + elements.Count);
            for (int i = 0; i < elements.Count; i++) {
                _E[_Count + i] = elements[i];
            }
            _Count += elements.Count;
        }
        /// <summary>Insert an element at a desired index. Element currently at that index moves a step forward.</summary>
        public override void Insert(int index, T element) {
            if (index < 0) throw new IndexOutOfRangeException("Negative index.");
            else if (index < _Count) {
                EnsureArrayCapacity(ref _E, _Count + 1);
                for (int i = _Count; i > index; i--) {                // Shift all consequent members by one. Makes room for a member.
                    _E[i] = _E[i - 1];
                }
                _E[index] = element;
                _Count++;
            }
            else throw new IndexOutOfRangeException("Index too large.");
        }
        /// <summary>Searches for and removes the specified element from this Subsequence. Returns true if successfully removed.</summary>
        public override bool Remove(T element) {
            for (int i = 0; i < _Count; i++) {
                if (_Comparer.Equals(_E[i], element)) {                   // If the element has been found.
                    for (int j = i; j < _Count - 1; j++) {
                        _E[j] = _E[j + 1];          // Shift elements, writing over the removed element.
                    }
                    _E[_Count - 1] = default(T);         // Reset the last Instruction.
                    _Count--;                                // Adjust the Count.
                    return true;
                }
            }
            return false;
        }
        /// <summary>Remove an element at the specified index.</summary>
        public override void RemoveAt(int index) {
            if (index < 0) throw new IndexOutOfRangeException("Negative index.");
            else if (index < _Count) {
                for (int i = index; i < _Count - 1; i++) {
                    _E[i] = _E[i + 1];
                }
                _E[_Count - 1] = default(T);
                _Count--;
            }
            else throw new IndexOutOfRangeException("Index too large.");
        }
        /// <summary>Clears the internal array without changing its capacity.</summary>
        public override void Clear() {
            for (int i = 0; i < _Count; i++) {
                _E[i] = default(T);
            }
            _Count = 0;
        }

        /// <summary>If specified index is out of range of internal array, we put it inside range.</summary><param name="index">Index which we want to conform.</param>
        protected void ValidateIndex(ref int index) {

            if(index <= 0) {
                index = 0;
            }                       
            else if(index >= _Count) {
                index = _Count - 1;
            }
        }

        /// <summary>Removes specified range from List and returns it.</summary><param name="j">Inclusive starting index.</param><param name="k">Inclusive ending index.</param>
        public List<T> RemoveRange(int j, int k) {
            int removedCount = k - j + 1;
            T[] removed = new T[removedCount];
            
            for(int i = 0; i < removedCount; ++i) {             // Construct array that we will return.
                removed[i] = _E[j + i];
            }
            for(int i = k + 1; i < _Count; ++i) {               // Refill hole. Shift elements remaining on right side of hole (removed range) to right.
                _E[i - removedCount] = _E[i];
            }
            _Count = _Count - removedCount;                     // Changing count, no need to zero elements at end.
            return CreateFromArray(removed);
        }

        /// <summary>Trim any excess space left in internal array.</summary>
        public void TrimExcessSpace() {
            int excess = _E.Length - _Count;
            if(excess > 0) {
                var newElements = new T[_Count];
                Array.Copy(_E, newElements, _Count);
                _E = newElements;
            }
        }
    }
}
