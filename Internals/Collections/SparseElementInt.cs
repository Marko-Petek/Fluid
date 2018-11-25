using System;
using SCG = System.Collections.Generic;

using Fluid.Internals.Collections;
using Fluid.Internals.Development;

namespace Fluid.Internals.Collections
{
    /// <summary>A member of SparseRow.</summary>
    public struct SparseElementInt : IEquatable<SparseElementInt>
    {
        // /// <summary>Comparer to check equality of two values of type T.</summary>
        // public static SCG.EqualityComparer<T> _EqualityComparer = SCG.EqualityComparer<T>.Default;

        /// <summary>True index inside SparseRow list.</summary>
        int _RealIndex;
        /// <summary>True index inside SparseRow list.</summary>
        public int RealIndex { get => _RealIndex; set => _RealIndex = value; }
        /// <summary>Index of column where element would be situated inside explicitly written out row.</summary>
        int _ImagIndex;
        /// <summary>Index of column where element would be situated inside explicitly written out row.</summary>
        public int ImagIndex  =>  _ImagIndex;
        /// <summary>Value of SparseElement.</summary>
        public int _Value;
        /// <summary>Value of SparseElement.</summary>
        public int Value { get => _Value; set => _Value = value; } 


        /// <summary>Create a SparseElement with a specified explicit index and value.</summary><param name="imagIndex">Explicit index of SparseElement in sparse row.</param><param name="value">Value of SparseElement.</param>
        public SparseElementInt(int imagIndex, int value) {
            _RealIndex = 0;
            _ImagIndex = imagIndex;
            _Value = value;
        }

        /// <summary>Create a SparseElement with specified explicit column index and specified value.</summary><param name="imagIndex">Explicit column index.</param><param name="value">Specified value.</param>
        public SparseElementInt(int realIndex, int imagIndex, int value) {
            _RealIndex = realIndex;
            _ImagIndex = imagIndex;
            _Value = value;
        }

        /// <summary>Create a copy of specified SparseElement with index -1, since it does not reside inside a SparseRow.</summary><param name="source">Source element to copy.</param>
        public SparseElementInt(SparseElementInt source) : this(source.RealIndex, source.ImagIndex, source.Value) {
        }

        /// <summary>Create a new SparseElement with identical explicit index and index -1.</summary><param name="left">Left SparseElement operand.</param><param name="right">Right SparseElement operand.</param>
        public static SparseElementInt operator + (SparseElementInt left, SparseElementInt right) {
            Assert.AreEqual(left.ImagIndex, right.ImagIndex);                           // Check that the indices match.
            return new SparseElementInt(-1, left.ImagIndex, left.Value + right.Value);
        }

        public static SparseElementInt operator - (SparseElementInt left, SparseElementInt right) {
            Assert.AreEqual(left.ImagIndex, right.ImagIndex);                           // Check that the indices match.
            return new SparseElementInt(-1, left.ImagIndex, left.Value - right.Value);
        }

        /// <summary>Create a new SparseElement with identical explicit index and index -1.</summary><param name="left">Left SparseElement operand.</param><param name="right">Right SparseElement operand.</param>
        public static SparseElementInt operator * (SparseElementInt left, SparseElementInt right) {
            Assert.AreEqual(left.ImagIndex, right.ImagIndex);                           // Check that the indices match.
            return new SparseElementInt(-1, left.ImagIndex, left.Value * right.Value);
        }

        /// <summary>Returns true if two values match, otherwise returns false.</summary><param name="other">SparseElement to compare to.</param>
        public bool Equals(SparseElementInt other) {
            if(_Value == other._Value)
                return true;
            else 
                return false;
        }

        public override string ToString() {
            return $"{{{_ImagIndex}: {_Value}}}";
        }
    }
}