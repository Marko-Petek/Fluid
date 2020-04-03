using System;
namespace Fluid.Internals.Networks.Custom {

public class EquatableList<T> : List<T>, IEquatable<EquatableList<T>>
   where T : struct, IEquatable<T> {
      /// <summary>Create list with specified initial capacity of internal array.</summary><param name="capacity">Initial capacity of internal array</param>
      public EquatableList(int capacity = 6) : base(capacity) { }
      /// <summary>Create a copy of specified source list.</summary><param name="sourceList">Source list to copy from.</param>
      public EquatableList(EquatableList<T> sourceList) : base(sourceList.Count) {
         Array.Copy(sourceList._E, _E, sourceList.Count);
      }

      /// <summary>Compares contents of two EquatableLists element by element and returns true only if all elements match.</summary><param name="other">List to compare to.</param>
      public bool Equals(EquatableList<T> other) {
         if(Count == other.Count) {
            for(int i = 0; i < Count; ++i)
               if(!this[i].Equals(other[i]))
                  return false;                   // If one of the elements is different. Lists are not equal.
            return true; }                           // Looped through all elements, all were equal. Lists are equal.
         else
            return false;
      }
}
}