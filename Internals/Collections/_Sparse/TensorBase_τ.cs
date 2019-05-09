using System;
using System.Collections.Generic;

namespace Fluid.Internals.Collections {
   /// <summary>A tensor with specified rank and specified dimension</summary>
   public abstract class TensorBase<τ> : Dictionary<int,τ> where τ : TensorBase<τ> {
      /// <summary>Rank specifies the height (level) in the hierarchy on which the tensor sits. It equals the number of levels that exist below it. It tells us how many indices we must specify before we reach the value level.</summary>
      public int Rank { get; protected set; }
      /// <summary>Dimension: equals tensor's number of direct subordinates (tensors of one rank less).</summary>
      public int Dim { get; protected set; }

      public TensorBase(int rank, int cap = 6) : base(cap) {
         Rank = rank;
      }
      public TensorBase(int rank, int dim, int cap = 6) : this(rank, cap) {
         Dim = dim;
      }
   }
}
