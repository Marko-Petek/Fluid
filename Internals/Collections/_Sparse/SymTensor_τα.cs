using System;
using System.Collections.Generic;
using dbl = System.Double;
using DA = Fluid.Internals.Numerics.DblArithmetic;
using Fluid.Internals.Numerics;
using static Fluid.Internals.Algorithms;
using F2DA = Fluid.Internals.Numerics.Func2DArithmetic;

namespace Fluid.Internals.Collections {
   using FTnr = Tensor<F2D,F2DA>;
   using static Fluid.Internals.Numerics.O<F2D,F2DA>;
   // TODO: SymTensor: Reimplement contraction, TnrProduct, addition, subtraction.
   /// <summary>A symmetric tensor that holds in memory only the entries "above diagonal".</summary>
   /// <typeparam name="τ">Type of direct subordinates.</typeparam>
   /// <typeparam name="α">Type of arithmetic.</typeparam>
   public class SymTensor<τ,α> : Tensor<τ,α>, IEquatable<Tensor<τ,α>>
   where τ : IEquatable<τ>, IComparable<τ>, new()
   where α : IArithmetic<τ>, new() {
#nullable disable
      /// <summary>Pairs of symmetric indices.</summary>
      public (int Inx1,int Inx2)[] Symmetries { get; }
#nullable enable
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
      // TODO: Generalize CreateAsQuadProd with recursion.
      /// <summary>Creates a SymTensor as a (multiple) tensor product of a 2nd rank tensor with itself.</summary>
      /// <param name="tnr">Tensor product constituent.</param>
      /// <param name="n">Number of times the constituent appears.</param>
      /// <remarks>For now this is specialized for the case of a rank two tensor.</remarks>
      public static SymTensor<dbl,DA> CreateAsQuadProd(FTnr tnr, F2D det,
      Func<F2D,dbl> integrate) {
         var prodTnr = new SymTensor<dbl,DA>(new List<int> {36,36,36,36},                             // Create product tensor.
            new (int,int)[] {(0,1), (0,2), (0,3), (1,2), (1,3), (2,3)}, 36);
         for(int i1 = 0; i1 < 12; ++i1) { for(int j1 = 0; j1 < 3; ++j1) {                             // Over 12 e-nodes, tnr index 1. Over 3 basis funcs at the e-node.
            for(int i2 = i1; i2 < 12; ++i2) { for(int j2 = 0; j2 < 3; ++j2) {
               for(int i3 = i2; i3 < 12; ++i3) { for(int j3 = 0; j3 < 3; ++j3) {
                  for(int i4 = i3; i4 < 12; ++i4) { for(int j4 = 0; j4 < 3; ++j4) {
                     F2D prod = F2D.QuintProd(tnr[i1,j1], tnr[i2,j2],
                        tnr[i3,j3], tnr[i4,j4], A.Abs(det));
                     prodTnr[3*i1+j1, 3*i2+j2, 3*i3+j3, 3*i4+j4] = integrate(prod);
         }} }} }} }}
         return prodTnr;
      }
      // TODO: Generalize CreateAsTripProd with recursion.
      /// <summary>Creates a SymTensor as a (multiple) tensor product of a tensor with itself.</summary>
      /// <param name="tnr">Tensor product constituent.</param>
      /// <param name="n">Number of times the constituent appears.</param>
      /// <remarks>For now this is specialized for the case of a rank two tensor.</remarks>
      public static SymTensor<dbl,DA> CreateAsTripProd(FTnr tnr, F2D det,
      Func<F2D,dbl> integrate) {
         var prodTnr = new SymTensor<dbl,DA>(new List<int> {36,36,36},                             // Create product tensor.
            new (int,int)[] {(0,1), (0,2), (0,3), (1,2), (1,3), (2,3)}, 36);
         for(int i1 = 0; i1 < 12; ++i1) { for(int j1 = 0; j1 < 3; ++j1) {                          // Over 12 e-nodes, tnr index 1. Over 3 basis funcs at the e-node.
            for(int i2 = i1; i2 < 12; ++i2) { for(int j2 = 0; j2 < 3; ++j2) {
               for(int i3 = i2; i3 < 12; ++i3) { for(int j3 = 0; j3 < 3; ++j3) {
                  F2D prod = F2D.QuadProd(tnr[i1,j1], tnr[i2,j2],
                     tnr[i3,j3], A.Abs(det));
                  prodTnr[3*i1+j1, 3*i2+j2, 3*i3+j3] = integrate(prod);
         }} }} }}
         return prodTnr;
      }
   }
}