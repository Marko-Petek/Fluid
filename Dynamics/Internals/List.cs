using System;
using System.Collections.Generic;
using static Fluid.Dynamics.Internals.ArrayHelper;

namespace Fluid.Dynamics.Internals
{
    // IEquatable<List<T>>

    public class List<T> : ListBase<T>
    {
        /// <summary>Create list with default initial capacity of internal array.</summary>
        public List() : base() { }

        /// <summary>Create list with specified initial capacity of internal array.</summary><param name="capacity">Initial capacity of internal array</param>
        public List(int capacity) : base(capacity) { }

        /// <summary>Create a copy of specified source list.</summary><param name="sourceList">Source list to copy from.</param>
        public List(List<T> sourceList) : base(sourceList.Count) {
            Array.Copy(sourceList._elements, _elements, sourceList.Count);
        }

        /// <summary>Create a new list by adopting specified source array.</summary><param name="source">Source array to adopt.</param>
        public List<T> CreateFromArray(T[] source) {
            var list = new List<T>(0);
            list._elements = source;
            return list;
        }

        /// <summary>Create a copy of specified source array, but as a list.</summary><param name="sourceArray">Source array to copy from.</param>
        public List(T[] sourceArray) : base(sourceArray.Length) {
            Array.Copy(sourceArray, _elements, sourceArray.Length);
        }

        /// <summary>Get a reference to element at specified index.</summary><param name="index">Element index.</param><returns>Element in list at specified index.</returns>
        public ref T Get(int index) {
            if (index > -1) {
                if (index < _count) {
                    return ref _elements[index];
                }
                else throw new IndexOutOfRangeException("Index too large.");
            }
            else throw new IndexOutOfRangeException("Negative index.");
        }

        /// <summary>Indexer</summary>
        public override T this[int index] {
            get {
                if (index > -1) {
                    if (index < _count) {
                        return _elements[index];
                    }
                    else throw new IndexOutOfRangeException("Index too large.");
                }
                else throw new IndexOutOfRangeException("Negative index.");
            }

            set {
                if (index > -1) {
                    if (index < _count) {
                        _elements[index] = value;
                    }
                    else throw new IndexOutOfRangeException("Index too big.");
                }
                else throw new IndexOutOfRangeException("Negative index.");
            }
        }
        /// <summary>Adds an element to the end of the list.</summary>
        public override void Add(T element) {
            EnsureArrayCapacity(ref _elements, _count + 1);
            _elements[_count] = element;
            _count++;
        }

        public override void AddRange(IList<T> elements) {
            EnsureArrayCapacity(ref _elements, _count + elements.Count);
            for (int i = 0; i < elements.Count; i++) {
                _elements[_count + i] = elements[i];
            }
            _count += elements.Count;
        }
        /// <summary>Insert an element at a desired index. Element currently at that index moves a step forward.</summary>
        public override void Insert(int index, T element) {
            if (index < 0) throw new IndexOutOfRangeException("Negative index.");
            else if (index < _count) {
                EnsureArrayCapacity(ref _elements, _count + 1);
                for (int i = _count; i > index; i--) {                // Shift all consequent members by one. Makes room for a member.
                    _elements[i] = _elements[i - 1];
                }
                _elements[index] = element;
                _count++;
            }
            else throw new IndexOutOfRangeException("Index too large.");
        }
        /// <summary>Searches for and removes the specified element from this Subsequence. Returns true if successfully removed.</summary>
        public override bool Remove(T element) {
            for (int i = 0; i < _count; i++) {
                if (_comparer.Equals(_elements[i], element)) {                   // If the element has been found.
                    for (int j = i; j < _count - 1; j++) {
                        _elements[j] = _elements[j + 1];          // Shift elements, writing over the removed element.
                    }
                    _elements[_count - 1] = default(T);         // Reset the last Instruction.
                    _count--;                                // Adjust the Count.
                    return true;
                }
            }
            return false;
        }
        /// <summary>Remove an element at the specified index.</summary>
        public override void RemoveAt(int index) {
            if (index < 0) throw new IndexOutOfRangeException("Negative index.");
            else if (index < _count) {
                for (int i = index; i < _count - 1; i++) {
                    _elements[i] = _elements[i + 1];
                }
                _elements[_count - 1] = default(T);
                _count--;
            }
            else throw new IndexOutOfRangeException("Index too large.");
        }
        /// <summary>Clears the internal array without changing its capacity.</summary>
        public override void Clear() {
            for (int i = 0; i < _count; i++) {
                _elements[i] = default(T);
            }
            _count = 0;
        }

        /// <summary>If specified index is out of range of internal array, we put it inside range.</summary><param name="index">Index which we want to conform.</param>
        protected void PutIndexInRange(ref int index) {

            if(index <= 0) {
                index = 0;
            }                       
            else if(index >= _count) {
                index = _count - 1;
            }
        }

        /// <summary>Removes specified range from List and returns it.</summary><param name="j">Inclusive starting index.</param><param name="k">Inclusive ending index.</param>
        public List<T> RemoveRange(int j, int k) {
            int removedCount = k - j + 1;
            T[] removed = new T[removedCount];
            
            for(int i = 0; i < removedCount; ++i) {             // Construct array that we will return.
                removed[i] = _elements[j + i];
            }
            for(int i = k + 1; i < _count; ++i) {               // Refill hole. Shift elements remaining on right side of hole (removed range) to right.
                _elements[i - removedCount] = _elements[i];
            }
            _count = _count - removedCount;                     // Changing count, no need to zero elements at end.
            return CreateFromArray(removed);
        }

        /// <summary>Trim any excess space left in internal array.</summary>
        public void TrimExcessSpace() {
            int excess = _elements.Length - _count;
            if(excess > 0) {
                var newElements = new T[_count];
                Array.Copy(_elements, newElements, _count);
                _elements = newElements;
            }
        }
    }
}
