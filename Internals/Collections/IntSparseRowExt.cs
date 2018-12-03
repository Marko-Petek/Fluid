using System;

using Fluid.Internals.Development;

namespace Fluid.Internals.Collections
{
	public class IntSparseRowExt : IntSparseRow
	{
		/// <summary>Default value of a non-existing element.</summary>
        protected readonly int _DefaultValue;
        /// <summary>Default value of a non-existing element.</summary>
        public int DefaultValue => _DefaultValue;

		/// <summary>Create a SparseRow with specified width it would have in explicit form, specified default value and specified initial capacity.</summary><param name="width">Width it would have in explicit form.</param><param name="defaultValue">Default value of non-existing entries.</param><param name="capacity">Capacity of internal array. If <= 0 internal array is not created.</param>
		public IntSparseRowExt(int width, int defaultValue = 1, int capacity = 6) : base(width, capacity) {
			Assert.NotEquals(ref defaultValue, 0, nameof(defaultValue), $"Use type {nameof(IntSparseRow)} instead.");	// One should use type IntSparseRow when DefaultValue = 0. Better performance.
			_DefaultValue = defaultValue;
		}

		/// <summary>Create a copy of specified IntSparseRowExt.</summary><param name="source">Source IntSparseRowExt to copy.</param>
        public IntSparseRowExt(IntSparseRowExt source) : base(source) {
            _DefaultValue = source._DefaultValue;
        }

		/// <summary>Create a new IntSparseRowExt by absorbing specified source array.</summary><param name="source">Source array to absorb.</param>
        public static IntSparseRowExt CreateFromArray(IntSparseElm[] source, int defaultValue = 1) {
			int count = source.Length;
			IntSparseElm lastElm = source[count - 1];
            int width = lastElm.VirtIndex + 1;
            var intSparseRowExt = new IntSparseRowExt(
				lastElm.VirtIndex + 1,					// Take smallest possible width.
				defaultValue,
				-1);									// Negative capacity means: do not construct an internal array.
            intSparseRowExt._E = source;                // Absorb provided array.
            return intSparseRowExt;
        }
	}
}
