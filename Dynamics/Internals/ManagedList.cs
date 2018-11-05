using System;
using System.Collections.Generic;
using static Fluid.Dynamics.Internals.ArrayHelper;

namespace Fluid.Dynamics.Internals
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
                        if (!_comparer.Equals(_elements[index], default(T))) {     // If there is a pre-existing element at the index in question.
                            BeforeElementExit(index);
                        }
                        _elements[index] = value;
                        if (!_comparer.Equals(value, default(T))) {                 // If the newly assigned value is not null.
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
            EnsureArrayCapacity(ref _elements, _count + 1);
            _elements[_count] = element;
            _count++;
            if (!_comparer.Equals(element, default(T))) {                 // If the newly assigned value is not null.
                AfterElementEntry(_count - 1);
            }
        }

        public override void AddRange(IList<T> elements) {
            EnsureArrayCapacity(ref _elements, _count + elements.Count);
            for (int i = 0; i < elements.Count; i++) {
                _elements[_count + i] = elements[i];
                if (!_comparer.Equals(elements[i], default(T))) {                 // If the newly assigned value is not null.
                    AfterElementEntry(_count + i);
                }
            }
            _count += elements.Count;
        }
        /// <summary>Insert an element at a desired index.</summary>
        public override void Insert(int index, T element) {
            if (index < 0) throw new IndexOutOfRangeException("Negative index.");
            else if (index <= _count) {
                EnsureArrayCapacity(ref _elements, _count + 1);
                for (int i = _count; i > index; i--) {                // Shift all consequent members by one. Makes room for a member.
                    _elements[i] = _elements[i - 1];
                }
                _elements[index] = element;
                _count++;
                if (!_comparer.Equals(element, default(T))) {                 // If the newly assigned value is not null.
                    AfterElementEntry(index);
                }
            }
            else throw new IndexOutOfRangeException("Index too large.");
        }
        /// <summary>Searches for and removes the specified element from this Subsequence. Returns true if successfully removed.</summary>
        public override bool Remove(T element) {
            for (int i = 0; i < _count; i++) {
                if (_comparer.Equals(_elements[i], element)) {                   // If the element has been found.
                    BeforeElementExit(i);
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
                if (!_comparer.Equals(_elements[index], default(T))) {     // If there is a pre-existing element at the index in question.
                    BeforeElementExit(index);
                }
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
                if (!_comparer.Equals(_elements[i], default(T))) {     // If there is a pre-existing element at the index in question.
                    BeforeElementExit(i);
                }
                _elements[i] = default(T);
            }
            _count = 0;
        }
    }
}
