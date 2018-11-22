using System;
using SCG = System.Collections.Generic;

using Fluid.Internals.Collections;
using static Fluid.Internals.Development.ExceptionHelper;

namespace Fluid.Internals.Collections
{
    /// <summary>A member of SparseRow.</summary>
    public struct SparseElement<T> : IEquatable<SparseElement<T>> where T : struct, IEquatable<T>
    {
        /// <summary>Comparer to check equality of two values of type T.</summary>
        public static SCG.EqualityComparer<T> EqualityComparer = SCG.EqualityComparer<T>.Default;

        /// <summary>True index inside SparseRow list.</summary>
        int _index;
        /// <summary>Index of column where element would be situated inside explicit row.</summary>
        int _explicitIndex;
        /// <summary>Value of SparseElement.</summary>
        T _value;

        /// <summary>True index inside SparseRow list.</summary>
        public int  GetIndex()          =>  _index;
        /// <summary>Set true index inside SparseRow list.</summary><param name="index">True index inside SparseRow list.</param>
        public int SetIndex(int index)  =>  _index = index;
        /// <summary>Index of column where element would be situated inside explicit row.</summary>
        public int  GetExplicitIndex()  =>  _explicitIndex;
        /// <summary>Value of SparseElement.</summary>
        public T    GetValue()          =>  _value;
        /// <summary>Set value of SparseElement.</summary>
        public T    SetValue(T value)   =>  _value = value;  


        /// <summary>Create a SparseElement with a specified explicit index and value.</summary><param name="explicitIndex">Explicit index of SparseElement in sparse row.</param><param name="value">Value of SparseElement.</param>
        public SparseElement(int explicitIndex, T value) {
            _index = 0;
            _explicitIndex = explicitIndex;
            _value = value;
        }

        /// <summary>Create a SparseElement with specified explicit column index and specified value.</summary><param name="explicitIndex">Explicit column index.</param><param name="value">Specified value.</param>
        public SparseElement(int index, int explicitIndex, T value) {
            _index = index;
            _explicitIndex = explicitIndex;
            _value = value;
        }

        /// <summary>Create a copy of specified SparseElement with index -1, since it does not reside inside a SparseRow.</summary><param name="sourceElement">Source element to copy.</param>
        public SparseElement(SparseElement<T> sourceElement) : this(sourceElement._index, sourceElement._explicitIndex, sourceElement._value) {
        }

        /// <summary>Create a new SparseElement with identical explicit index and index -1.</summary><param name="left">Left SparseElement operand.</param><param name="right">Right SparseElement operand.</param>
        public static SparseElement<T> operator + (SparseElement<T> left, SparseElement<T> right) {
            CheckEquality<int>(left.GetExplicitIndex(), right.GetExplicitIndex(), IntComparer);        // Check that the indices match.
            return new SparseElement<T>(-1, left._explicitIndex, (dynamic)left._value + right._value);     // Hackjob: dynamic orders compiler not to complain.
        }

        public static SparseElement<T> operator - (SparseElement<T> left, SparseElement<T> right) {
            CheckEquality<int>(left.GetExplicitIndex(), right.GetExplicitIndex(), IntComparer);        // Check that the indices match.
            return new SparseElement<T>(-1, left._explicitIndex, (dynamic)left._value - right._value);     // Hackjob: dynamic orders compiler not to complain.
        }

        /// <summary>Create a new SparseElement with identical explicit index and index -1.</summary><param name="left">Left SparseElement operand.</param><param name="right">Right SparseElement operand.</param>
        public static SparseElement<T> operator * (SparseElement<T> left, SparseElement<T> right) {
            CheckEquality<int>(left.GetExplicitIndex(), right.GetExplicitIndex(), IntComparer);        // Check that the indices match.
            return new SparseElement<T>(-1, left._explicitIndex, (dynamic)left._value * right._value);     // Hackjob: dynamic orders compiler not to complain.
        }

        /// <summary>Returns true if two values match, otherwise returns false.</summary><param name="other">SparseElement to compare to.</param>
        public bool Equals(SparseElement<T> other) {
            if(_value.Equals(other._value)) {
                return true;
            }
            else {
                return false;
            }
        }

        public override string ToString() {
            return $"{{{_explicitIndex}: {_value}}}";
        }
    }
}