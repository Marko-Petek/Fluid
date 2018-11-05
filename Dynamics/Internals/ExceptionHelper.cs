using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Fluid.Dynamics.Internals
{
    public static class ExceptionHelper
    {
        /// <summary>Throw IndexOutOfRangeException if necessary.</summary><param name="index">Index to be checked.</param><param name="lowerBound">Inclusive lower bound.</param><param name="upperBound">Exclusive upper bound.</param><param name="callerName">Omit, provided by compiler. Name of calling method.</param>
        public static void CheckIndexValidity(int index, int lowerBound, int upperBound, [CallerMemberName] string callerName = null) {
            if(index < lowerBound) {
                throw new IndexOutOfRangeException($"Index too small inside method {callerName}");
            }
            else if(index >= upperBound) {
                throw new IndexOutOfRangeException($"Index too large inside method {callerName}");
            }
        }

        /// <summary>Throw ArgumentException if two specified fields are not equal.</summary><param name="field1">First field to compare.</param><param name="field2">Second field to compare.</param><param name="comparer">Arbiter of equality.</param><param name="callerName">Omit, filled by compiler.</param><typeparam name="T">Type of compared fields.</typeparam>
        public static void CheckFieldsEquivalence<T>(T field1, T field2, EqualityComparer<T> comparer, [CallerMemberName] string callerName = null) {
            if(!comparer.Equals(field1, field2)) {
                throw new ArgumentException($"Fields used inside {callerName} not equal.");
            }
        }

        public static EqualityComparer<int> IntComparer = EqualityComparer<int>.Default;
    }
}