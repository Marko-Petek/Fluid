#if false
using System;
using System.Text;
using System.Linq;
using SCG = System.Collections.Generic;

using Fluid.Internals.Numerics;

namespace Fluid.Internals.Collections {
   /// <summary>A rank 1 (R1) tensor with specified dimension of its only rank which holds values of type τ. Type τ can use arithmetic defined inside type α.</summary>
   /// <typeparam name="τ">Type of R1 elements.</typeparam>
   /// <typeparam name="α">Type that defines arithmetic between values of type τ.</typeparam>
   public class Tensor1<τ,α> : Tensor<τ>,
      IEquatable<Tensor1<τ,α>>                                         // So that we can equate two rank 1 tensors via the Equals method.
      where τ : IEquatable<τ>, IComparable<τ>, new()
      where α : IArithmetic<τ>, new() {
         /// <summary>Contains arithmetic methods.</summary>
         static α Arith { get; } = new α();
         /// <summary>Creates a R1 tensor with arithmetic α, with initial capacity of 1 and does not assign its R1 dimension. You must do it manually.</summary>
         protected Tensor1() : base() {}
         /// <summary>Creates a R1 tensor with arithmetic α, with specified R1 dimension and initial capacity.</summary>
         /// <param name="dimR1">Rank 1 dimension.</param>
         /// <param name="cap">Initially assigned memory.</param>
         public Tensor1(int dimR1, int cap = 6) : base(dimR1, cap) { }
         /// <summary>Factory method that creates a R1 tensor with arithmetic α, with specified R1 dimension and initial capacity.</summary>
         /// <param name="dimR1">Rank 1 dimension.</param>
         /// <param name="cap">Initially assigned memory.</param>
         new public static Tensor1<τ,α> Create(int dimR1, int cap = 6) => new Tensor1<τ,α>(dimR1, cap);
         /// <summary>Creates a R1 tensor with arithmetic α as a copy of another.</summary>
         /// <param name="src">R1 tensor to copy.</param>
         public Tensor1(Tensor1<τ,α> src) : this(src.Dim, src.Count) {
            foreach(var pair in src)
               Add(pair.Key, pair.Value);
         }
      /// <summary>Factory method that creates a R1 tensor with arithmetic α as a copy of another.</summary>
      /// <param name="src">R1 tensor to copy.</param>
      public static Tensor1<τ,α> Create(Tensor1<τ,α> src) => new Tensor1<τ,α>(src);
      
      
   }
}
#endif