using System;
//using SCG = System.Collections.Generic;

using Fluid.Internals.Development;

namespace Fluid.Internals.Collections
{
    /// <summary>A member of IntSparseRow.</summary>
    public struct IntSparseElm : IEquatable<IntSparseElm>
    {
        /// <summary>Index of column where element would be situated inside explicitly written out row.</summary>
		int _VirtIndex;
        /// <summary>Index of column where element would be situated inside explicitly written out row.</summary>
        public int VirtIndex  { get => _VirtIndex; set => _VirtIndex = value; }
        /// <summary>Value of SparseElement.</summary>
        public int _Value;
        /// <summary>Value of SparseElement.</summary>
        public int Value { get => _Value; set => _Value = value; } 


        /// <summary>Create a SparseElement with a specified explicit index and value.</summary><param name="virtIndex">Explicit index of SparseElement in sparse row.</param><param name="value">Value of SparseElement.</param>
		public IntSparseElm(int virtIndex, int value) {
            _VirtIndex = virtIndex;
            _Value = value;
        }

        /// <summary>Create a copy of specified SparseElement with VirtIndex -1, since it does not reside inside a SparseRow.</summary><param name="source">Source element to copy.</param>
        public IntSparseElm(IntSparseElm source) : this(source.VirtIndex, source.Value) {
        }

        /// <summary>Create a new SparseElement as a sum of two other SparseElements, with VirtIndex of left operand.</summary><param name="left">Left SparseElement operand.</param><param name="right">Right SparseElement operand.</param>
        public static IntSparseElm operator + (IntSparseElm left, IntSparseElm right) {
            return new IntSparseElm(left.VirtIndex, left.Value + right.Value);
        }

        /// <summary>Create a new SparseElement as a difference of two other SparseElements, with VirtIndex of left operand.</summary><param name="left">Left SparseElement operand.</param><param name="right">Right SparseElement operand.</param>
        public static IntSparseElm operator - (IntSparseElm left, IntSparseElm right) {
            return new IntSparseElm(left.VirtIndex, left.Value - right.Value);
        }

        /// <summary>Create a new SparseElement as a product of two other SparseElements, with VirtIndex of left operand.</summary><param name="left">Left SparseElement operand.</param><param name="right">Right SparseElement operand.</param>
        public static IntSparseElm operator * (IntSparseElm left, IntSparseElm right) {
            return new IntSparseElm(left.VirtIndex, left.Value * right.Value);
        }

        /// <summary>Returns true if two values match, otherwise returns false.</summary><param name="other">SparseElement to compare to.</param>
        public bool Equals(IntSparseElm other) {
            if(_Value == other._Value)
                return true;
            else 
                return false;
        }

        /// <summary>Writes out SparseElm in form: {VirtIndex, Value}</summary>
        public override string ToString() {
            return $"{{{_VirtIndex}, {_Value}}}";
        }
    }
}