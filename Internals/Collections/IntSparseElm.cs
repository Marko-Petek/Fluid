using System;
//using SCG = System.Collections.Generic;

using Fluid.Internals.Development;

namespace Fluid.Internals.Collections
{
    /// <summary>A member of SparseRow.</summary>
    public struct IntSparseElm : IEquatable<IntSparseElm>
    {
        /// <summary>Index of column where element would be situated inside explicitly written out row.</summary>
		int _VirtIndex;
        /// <summary>Index of column where element would be situated inside explicitly written out row.</summary>
        public int VirtIndex  =>  _VirtIndex;
        /// <summary>Value of SparseElement.</summary>
        public int _Value;
        /// <summary>Value of SparseElement.</summary>
        public int Value { get => _Value; set => _Value = value; } 


        /// <summary>Create a SparseElement with a specified explicit index and value.</summary><param name="virtIndex">Explicit index of SparseElement in sparse row.</param><param name="value">Value of SparseElement.</param>
		public IntSparseElm(int virtIndex, int value) {
            _VirtIndex = virtIndex;
            _Value = value;
        }

        /// <summary>Create a SparseElement with specified explicit column index and specified value.</summary><param name="virtIndex">Explicit column index.</param><param name="value">Specified value.</param>
		public IntSparseElm(int realIndex, int virtIndex, int value) {
            _VirtIndex = virtIndex;
            _Value = value;
        }

        /// <summary>Create a copy of specified SparseElement with index -1, since it does not reside inside a SparseRow.</summary><param name="source">Source element to copy.</param>
        public IntSparseElm(IntSparseElm source) : this(source.VirtIndex, source.Value) {
        }

        /// <summary>Create a new SparseElement with identical explicit index and index -1.</summary><param name="left">Left SparseElement operand.</param><param name="right">Right SparseElement operand.</param>
        public static IntSparseElm operator + (IntSparseElm left, IntSparseElm right) {
            Assert.AreEqual(left.VirtIndex, right.VirtIndex);                           // Check that the indices match.
            return new IntSparseElm(-1, left.VirtIndex, left.Value + right.Value);
        }

        public static IntSparseElm operator - (IntSparseElm left, IntSparseElm right) {
            Assert.AreEqual(left.VirtIndex, right.VirtIndex);                           // Check that the indices match.
            return new IntSparseElm(-1, left.VirtIndex, left.Value - right.Value);
        }

        /// <summary>Create a new SparseElement with identical explicit index and index -1.</summary><param name="left">Left SparseElement operand.</param><param name="right">Right SparseElement operand.</param>
        public static IntSparseElm operator * (IntSparseElm left, IntSparseElm right) {
            Assert.AreEqual(left.VirtIndex, right.VirtIndex);                           // Check that the indices match.
            return new IntSparseElm(-1, left.VirtIndex, left.Value * right.Value);
        }

        /// <summary>Returns true if two values match, otherwise returns false.</summary><param name="other">SparseElement to compare to.</param>
        public bool Equals(IntSparseElm other) {
            if(_Value == other._Value)
                return true;
            else 
                return false;
        }

        public override string ToString() {
            return $"{{{_VirtIndex}: {_Value}}}";
        }
    }
}