using System;
using System.Linq;

namespace Fluid.Internals.Networks.Old {
   public class ValueNode<T> : RankedNode {
      T _Value;
      public T Value => _Value;

      public ValueNode(RankedNode leader, T value) : base(leader) {
         _Value = value;
      }
   }
}