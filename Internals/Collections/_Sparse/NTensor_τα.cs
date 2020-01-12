using System;
using System.Collections.Generic;
using System.Linq;
using Fluid.Internals.Numerics;
namespace Fluid.Internals.Collections {


/// <summary>Tensor which holds values. Further implements IComparable.</summary>
/// <typeparam name="τ">Numeric type.</typeparam>
/// <typeparam name="α">Artihmetic type.</typeparam>
public class NTensor<τ,α> : Tensor<τ,α>
where τ : struct, IEquatable<τ>, IComparable<τ>
where α : IArithmetic<τ>, new() {
   /// <summary>Void value tensor.</summary>
   public static readonly Tensor<τ,α> NT = Factory.TopNTensor<τ,α>(new List<int>{0,0}, 0);
   /// <summary>Constructor with redundancy, used internally.</summary>
   /// <param name="strc">Structure (absorbed).</param>
   /// <param name="rank">Rank.</param>
   /// <param name="sup">Direct superior.</param>
   /// <param name="cap">Capacity of internal dictionary.</param>
   protected NTensor(List<int> strc, int rank, Tensor<τ,α>? sup, int cap) :
   base(strc, rank, sup, cap) {
      Structure = strc;
      Rank = rank;
      Superior = sup;
   }
   /// <summary>Constructor for a top tensor (null superior). For creating tensors of rank 1 use Vector's constructor.</summary>
   /// <param name="strc">Structure (absorbed).</param>
   /// <param name="cap">Capacity of internal dictionary.</param>
   internal NTensor(List<int> strc, int cap = 6) : this(strc, strc.Count, null, cap) { }
   /// <summary>Constructor for a non-top tensor (non-null superior). Assumes superior's structure is initialized.</summary>
   /// <param name="sup">Direct superior.</param>
   /// <param name="cap">Capacity of internal dictionary.</param>
   internal NTensor(Tensor<τ,α> sup, int cap = 6) : this(sup.Structure,
   sup.Rank - 1, sup, cap) { }


   /// <remarks> <see cref="TestRefs.TensorEquals"/> </remarks>
   public bool Equals(NTensor<τ,α> tnr2, τ eps) {
      Assume.True(DoSubstructuresMatch(this, tnr2),
         () => "Tensor substructures do not match on equality comparison.");
      return TnrRecursion(this, tnr2);

      bool TnrRecursion(NTensor<τ,α> sup1, NTensor<τ,α> sup2) {
         if(!sup1.Keys.OrderBy(key => key).SequenceEqual(sup2.Keys.OrderBy(key => key)))
            return false;                                                                    // Keys have to match. This is crucial.
         if(sup1.Rank > 2) {
            foreach(var inx_sub1 in sup1) {
               int inx = inx_sub1.Key;
               var sub1 = inx_sub1.Value;
               var sub2 = sup2[NTensor<τ,α>.NT, inx];
               return TnrRecursion(inx_sub1.Value, sub2); }
            return true; }                                                                   // Both are empty.
         else
            return VecRecursion(sup1, sup2); }

      bool VecRecursion(NTensor<τ,α> sup1R2, NTensor<τ,α> sup2R2) {
         if(!sup1R2.Keys.OrderBy(key => key).SequenceEqual(sup2R2.Keys.OrderBy(key => key)))
            return false;                                                                    // Keys have to match. This is crucial.
         foreach(var inx_sub1R1 in sup1R2) {
            var vec1 = (NVector<τ,α>) inx_sub1R1.Value;
            var vec2 = sup2R2[NVector<τ,α>.NV, inx_sub1R1.Key];
            if(!vec1.Equals(vec2, eps))
               return false; }
         return true; }                                                         // All values agree within tolerance.
   }
}


}