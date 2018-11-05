using System;

namespace Fluid.Dynamics.Internals
{
    public abstract class EquatableManagedList<T> : ManagedList<T>, IEquatable<EquatableManagedList<T>>
    where T : IEquatable<T> {


        /// <summary>Create ManagedList with default initial capacity of internal array.</summary>
        public EquatableManagedList() : base() { }

        /// <summary>Create ManagedList with specified initial capacity of internal array.</summary><param name="capacity">Initial capacity of internal array.</param>
        public EquatableManagedList(int capacity) : base(capacity) { }

        /// <summary>Create a copy of ManagedList.</summary><param name="sourceList">Source ManagedList to copy.</param>
        public EquatableManagedList(EquatableManagedList<T> sourceList) : base(sourceList) {
        }

        /// <summary>Compare all values inside list and return true if all match.</summary><param name="other">List to compare to.</param>
        public bool Equals(EquatableManagedList<T> other) {

            if(other != null) {
                if(other.Count == _count) {

                    for(int i = 0; i < _count; ++i) {
                        
                        if(!_elements[i].Equals(other._elements[i])) {
                            return false;
                        }
                    }
                    return true;
                }
            }
            return false;
        }
    }
}