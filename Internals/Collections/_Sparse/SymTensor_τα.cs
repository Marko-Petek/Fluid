using System;
using System.Collections.Generic;
using Fluid.Internals.Numerics;
using static Fluid.Internals.Ops;

namespace Fluid.Internals.Collections {
   // TODO: SymTensor: Reimplement contraction, TnrProduct, addition, subtraction.
   /// <summary>A symmetric tensor that holds in memory only the entries "above diagonal".</summary>
   /// <typeparam name="τ">Type of direct subordinates.</typeparam>
   /// <typeparam name="α">Type of arithmetic.</typeparam>
   public class SymTensor<τ,α> : Tensor<τ,α>, IEquatable<Tensor<τ,α>>
   where τ : IEquatable<τ>, IComparable<τ>, new()
   where α : IArithmetic<τ>, new() {
      /// <summary>Pairs of symmetric indices.</summary>
      public (int Inx1,int Inx2)[] Symmetries { get; }
      public SymTensor(List<int> structure, (int,int)[] syms, int cap = 6) : base(structure, cap) { }

      public SymTensor(Tensor<τ,α> sup, (int,int)[] syms, int cap) : base(sup, cap) { }
      
      public override τ this[params int[] inx] {
         get {
            RemapIndices(inx);
            return base[inx]; }
         set {
            RemapIndices(inx);
            base[inx] = value; }
      }
      /// <summary>Remaps indices if they were specified in the incorrect order. Method modifies argument.</summary>
      /// <param name="inx">Indices that will be remapped.</param>
      bool RemapIndices(int[] inx) {
         bool remapped = false;
         foreach(var symPair in Symmetries) {
            ref int inx1 = ref inx[symPair.Inx1];
            ref int inx2 = ref inx[symPair.Inx2];
            if(inx1 > inx2) {
               Swap(ref inx1, ref inx2);
               if(!remapped)
                  remapped = true; } }
         return remapped;
      }
   }
}