#if true
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
      /// <summary>Creates a R1 tensor with arithmetic α, with specified dimension from an array.</summary>
      /// <param name="arr">Array to copy.</param>
      /// <param name="srtArrInx">Index of array element at which copying begins.</param>
      /// <param name="nArrEmts">How many consecutive array elements to copy.</param>
      /// <param name="firstR1Inx">What index to assign to first element copied to tensor.</param>
      /// <param name="dimR1">Tensor's R1 dimension.</param>
      new public static Tensor1<τ,α> CreateFromArray(τ[] arr, int srtArrInx, int nArrEmts,      // srt = start
         int firstR1Inx, int dimR1) {
            var tnr1 = Create(dimR1, arr.Length);
            for(int i = srtArrInx, j = firstR1Inx; i < srtArrInx + nArrEmts; ++i, ++j)
               tnr1[j] = arr[i];
            return tnr1;
      }
      /// <summary>Factory method that creates a R1 tensor with arithmetic α from an array. Tensor's R1 dimension equals number of copied elements.</summary>
      /// <param name="arr">Array to copy.</param>
      /// <param name="srtArrInx">Index of array element at which copying begins.</param>
      /// <param name="nArrEmts">How many consecutive array elements to copy. Also the R1 dimension of tensor.</param>
      /// <param name="firstR1Inx">What index to assign to first element copied to tensor.</param>
      new public static Tensor1<τ,α> CreateFromArray(τ[] arr, int srtArrInx, int nArrEmts,
            int firstR1Inx) => CreateFromArray(arr, srtArrInx, nArrEmts, firstR1Inx, nArrEmts);
      /// <summary>Factory method that creates a R1 tensor with arithmetic α from an array. Tensor's R1 dimension equals number of copied elements. Index of first copied element is set to 0.</summary>
      /// <param name="arr">Array to copy.</param>
      /// <param name="srtArrInx">Index of array element at which copying begins.</param>
      /// <param name="nArrEmts">How many consecutive array elements to copy. Also the R1 dimension of tensor.</param>
      new public static Tensor1<τ,α> CreateFromArray(τ[] arr, int srtArrInx, int nArrEmts) =>
            CreateFromArray(arr, srtArrInx, nArrEmts, 0);
      
   }
}
#endif