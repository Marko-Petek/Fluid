using System;
using System.Diagnostics.CodeAnalysis;

namespace Fluid.Internals.Networks.Custom {

public abstract class EquatableManagedList<T> : ManagedList<T>,
IEquatable<EquatableManagedList<T>>
where T : struct, IEquatable<T> {
      /// <summary>Create ManagedList with default initial capacity of internal array.</summary>
      public EquatableManagedList() : base() { }
      /// <summary>Create ManagedList with specified initial capacity of internal array.</summary><param name="capacity">Initial capacity of internal array.</param>
      public EquatableManagedList(int capacity) : base(capacity) { }
      /// <summary>Create a copy of ManagedList.</summary><param name="sourceList">Source ManagedList to copy.</param>
      public EquatableManagedList(EquatableManagedList<T> sourceList) : base(sourceList) { }

      /// <summary>Compare all values inside list and return true if all match.</summary><param name="other">List to compare to.</param>
      public bool Equals([AllowNull] EquatableManagedList<T> other) {
         if(other != null)
            if(other.Count == Count) {
               for(int i = 0; i < Count; ++i)
                  if(!_E[i].Equals(other._E[i]))
                     return false;
               return true; }
         return false;
      }
}
}