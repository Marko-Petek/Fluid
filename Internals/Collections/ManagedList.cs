using System;
using SCG = System.Collections.Generic;

using static Fluid.Internals.ArrayOperations;

namespace Fluid.Internals.Collections
{
    public abstract class ManagedList<T> : List<T>
    {
        /// <summary>Create ManagedList with default initial capacity of internal array.</summary>
        public ManagedList() : base() { }

        /// <summary>Create ManagedList with specified initial capacity of internal array.</summary><param name="capacity">Initial capacity of internal array.</param>
        public ManagedList(int capacity) : base(capacity) { }

        /// <summary>Create a copy of ManagedList.</summary><param name="sourceList">Source ManagedList to copy.</param>
        public ManagedList(ManagedList<T> sourceList) : base(sourceList) {
        }

        /// <summary>Assign any properties of the element that has just entered an internal array of the List
        /// at the specified index via Add or Insert.</summary>
        protected abstract void AfterElementEntry(int index);
        /// <summary>Assign any properties of the element that is about to leave an internal array of the List
        /// at the specified index via Remove, RemoveAt or Clear.</summary>
        protected abstract void BeforeElementExit(int index);

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
                        if (!_Comparer.Equals(_E[index], default(T))) {     // If there is a pre-existing element at the index in question.
                            BeforeElementExit(index);
                        }
                        _E[index] = value;
                        if (!_Comparer.Equals(value, default(T))) {                 // If the newly assigned value is not null.
                            AfterElementEntry(index);
                        }
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
            _Count++;
            if (!_Comparer.Equals(element, default(T))) {                 // If the newly assigned value is not null.
                AfterElementEntry(_Count - 1);
            }
        }

        public override void AddRange(SCG.IList<T> elements) {
            EnsureArrayCapacity(ref _E, _Count + elements.Count);
            for (int i = 0; i < elements.Count; i++) {
                _E[_Count + i] = elements[i];
                if (!_Comparer.Equals(elements[i], default(T))) {                 // If the newly assigned value is not null.
                    AfterElementEntry(_Count + i);
                }
            }
            _Count += elements.Count;
        }
        /// <summary>Insert an element at a desired index.</summary>
        public override void Insert(int index, T element) {
            if (index < 0) throw new IndexOutOfRangeException("Negative index.");
            else if (index <= _Count) {
                EnsureArrayCapacity(ref _E, _Count + 1);
                for (int i = _Count; i > index; i--) {                // Shift all consequent members by one. Makes room for a member.
                    _E[i] = _E[i - 1];
                }
                _E[index] = element;
                _Count++;
                if (!_Comparer.Equals(element, default(T))) {                 // If the newly assigned value is not null.
                    AfterElementEntry(index);
                }
            }
            else throw new IndexOutOfRangeException("Index too large.");
        }
        /// <summary>Searches for and removes the specified element from this Subsequence. Returns true if successfully removed.</summary>
        public override bool Remove(T element) {
            for (int i = 0; i < _Count; i++) {
                if (_Comparer.Equals(_E[i], element)) {                   // If the element has been found.
                    BeforeElementExit(i);
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
                if (!_Comparer.Equals(_E[index], default(T))) {     // If there is a pre-existing element at the index in question.
                    BeforeElementExit(index);
                }
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
                if (!_Comparer.Equals(_E[i], default(T))) {     // If there is a pre-existing element at the index in question.
                    BeforeElementExit(i);
                }
                _E[i] = default(T);
            }
            _Count = 0;
        }
    }
}
