using System;
using SCG = System.Collections.Generic;

using Fluid.Internals.Collections;
using Fluid.Internals.Development;

namespace Fluid.Internals.Collections
{
    /// <summary>A member of SparseRow.</summary>
    public struct SparseElm<T> : IEquatable<SparseElm<T>> where T : struct, IEquatable<T>
    {
        /// <summary>Comparer to check equality of two values of type T.</summary>
        public static SCG.EqualityComparer<T> _EqualityComparer = SCG.EqualityComparer<T>.Default;

        /// <summary>True index inside SparseRow list.</summary>
        int _RealIndex;
        /// <summary>True index inside SparseRow list.</summary>
        public int RealIndex { get => _RealIndex; set => _RealIndex = value; }
        /// <summary>Index of column where element would be situated inside explicitly written out row.</summary>
        int _ImagIndex;
        /// <summary>Index of column where element would be situated inside explicitly written out row.</summary>
        public int  ImagIndex()  =>  _ImagIndex;
        /// <summary>Value of SparseElement.</summary>
        T _Value;
        /// <summary>Value of SparseElement.</summary>
        public T Value { get => _Value; set => _Value = value; } 


        /// <summary>Create a SparseElement with a specified explicit index and value.</summary><param name="imagIndex">Explicit index of SparseElement in sparse row.</param><param name="value">Value of SparseElement.</param>
        public SparseElm(int imagIndex, T value) {
            _RealIndex = 0;
            _ImagIndex = imagIndex;
            _Value = value;
        }

        /// <summary>Create a SparseElement with specified explicit column index and specified value.</summary><param name="imagIndex">Explicit column index.</param><param name="value">Specified value.</param>
        public SparseElm(int realIndex, int imagIndex, T value) {
            _RealIndex = realIndex;
            _ImagIndex = imagIndex;
            _Value = value;
        }

        /// <summary>Create a copy of specified SparseElement with index -1, since it does not reside inside a SparseRow.</summary><param name="sourceElement">Source element to copy.</param>
        public SparseElm(SparseElm<T> sourceElement) : this(sourceElement._RealIndex, sourceElement._ImagIndex, sourceElement._Value) {
        }

        /// <summary>Create a new SparseElement with identical explicit index and index -1.</summary><param name="left">Left SparseElement operand.</param><param name="right">Right SparseElement operand.</param>
        public static SparseElm<T> operator + (SparseElm<T> left, SparseElm<T> right) {
            Assert.AreEqual(left._ImagIndex, right._ImagIndex);        // Check that the indices match.
            return new SparseElm<T>(-1, left._ImagIndex, (dynamic)left._Value + right._Value);     // Hackjob: dynamic orders compiler not to complain.
        }

        public static SparseElm<T> operator - (SparseElm<T> left, SparseElm<T> right) {
            Assert.AreEqual(left.ImagIndex(), right.ImagIndex());        // Check that the indices match.
            return new SparseElm<T>(-1, left._ImagIndex, (dynamic)left._Value - right._Value);     // Hackjob: dynamic orders compiler not to complain.
        }

        /// <summary>Create a new SparseElement with identical explicit index and index -1.</summary><param name="left">Left SparseElement operand.</param><param name="right">Right SparseElement operand.</param>
        public static SparseElm<T> operator * (SparseElm<T> left, SparseElm<T> right) {
            Assert.AreEqual(left.ImagIndex(), right.ImagIndex());        // Check that the indices match.
            return new SparseElm<T>(-1, left._ImagIndex, (dynamic)left._Value * right._Value);     // Hackjob: dynamic orders compiler not to complain.
        }

        /// <summary>Returns true if two values match, otherwise returns false.</summary><param name="other">SparseElement to compare to.</param>
        public bool Equals(SparseElm<T> other) {
            if(_Value.Equals(other._Value)) {
                return true;
            }
            else {
                return false;
            }
        }

        public override string ToString() {
            return $"{{{_ImagIndex}: {_Value}}}";
        }
    }
}