using System;
using System.Collections.Generic;
using System.Linq;
using Fluid.Internals.Numerics;
namespace Fluid.Internals.Collections {


/// <summary>A tensor with specified rank and specified dimension which holds direct subordinates of type τ.</summary>
/// <typeparam name="τ">Type of direct subordinates.</typeparam>
/// <typeparam name="α">Type of arithmetic.</typeparam>
public partial class Tensor<τ,α> : TensorBase<Tensor<τ,α>>, IEquatable<Tensor<τ,α>>
where τ : IEquatable<τ>, IComparable<τ>, new()
where α : IArithmetic<τ>, new() {
   /// <summary>Void tensor.</summary>
   public static readonly Tensor<τ,α> T = Factory.TopTensor<τ,α>(new List<int>{0,0}, 0);
   /// <summary>Constructor with redundancy, used internally.</summary>
   /// <param name="strc">Structure (absorbed).</param>
   /// <param name="rank">Rank.</param>
   /// <param name="sup">Direct superior.</param>
   /// <param name="cap">Capacity of internal dictionary.</param>
   protected Tensor(List<int> strc, int rank, Tensor<τ,α>? sup, int cap) : base(cap) {
      Structure = strc;
      Rank = rank;
      Superior = sup;
   }
   /// <summary>Constructor for a top tensor (null superior). For creating tensors of rank 1 use Vector's constructor.</summary>
   /// <param name="strc">Structure (absorbed).</param>
   /// <param name="cap">Capacity of internal dictionary.</param>
   internal Tensor(List<int> strc, int cap = 6) : this(strc, strc.Count, null, cap) { }
   /// <summary>Constructor for a non-top tensor (non-null superior). Assumes superior's structure is initialized.</summary>
   /// <param name="sup">Direct superior.</param>
   /// <param name="cap">Capacity of internal dictionary.</param>
   internal Tensor(Tensor<τ,α> sup, int cap = 6) : this(sup.Structure,
   sup.Rank - 1, sup, cap) { }

   /// <summary>Adds specified tensor as subordinate and appropriatelly sets its Superior and Structure.</summary>
   /// <param name="inx">Index at which the tensor will be added.</param>
   /// <param name="tnr">Tensor to add.</param>
   internal void AddPlus(int inx, Tensor<τ,α> tnr) {
      tnr.Superior = this;
      tnr.Structure = Structure;
      base.Add(inx, tnr);
   }
   /// <summary>Adds tnr to caller. Does not assign the caller's superstructure to tnr or check if the added tensor is empty.</summary>
   /// <param name="key">Index.</param>
   /// <param name="tnr">Tensor.</param>
   new internal void Add(int key, Tensor<τ,α> tnr) =>
      base.Add(key, tnr);

   /// <summary>Adds tnr to caller only if it is not empty. Assigns the caller's superstructure to tnr.</summary>
   /// <param name="key">Index.</param>
   /// <param name="tnr">Tensor.</param>
   internal void AddPlusIfNotEmpty(int key, Tensor<τ,α> tnr) {
      if(tnr.Count != 0)
         AddPlus(key, tnr);
   }
   
   /// <summary>A substructure as unevaluated instructions ready for enumeration.</summary>
   internal IEnumerable<int> GetSubstructure() =>
      Structure.Skip(SubStrcInx);
   /// <summary>Packs substructure into a new structure. Use for new top tensors.</summary>
   internal List<int> CopySubstructure() =>
      GetSubstructure().ToList();
      
   /// <summary>Copies substructure from specified tensor directly into Structure.</summary>
   /// <param name="tnr">Source.</param>
   protected void AssignStructFromSubStruct(Tensor<τ,α> tnr) {          // FIXME: Remove this after the copy specs fixes.
      Structure.Clear();
      var subStruct = tnr.Structure.Skip(tnr.SubStrcInx);
      foreach(var emt in subStruct) {
         Structure.Add(emt); }
   }

   // /// <summary>Transforms from slot index (in the order written by hand, e.g. A^ijk ==> 1,2,3) to rank index (as situated in the hierarchy, e.g. A^ijk ==> 2,1,0).</summary>
   // /// <param name="rankInx">Rank index as situated in the hierarchy. Higher number equates to being higher in the hierarchy.</param>
   // int ToSlotInx(int rankInx) =>
   //    ChangeRankNotation(Structure.Count, rankInx);

   // /// <summary>Transforms from rank index (as situated in the hierarchy, e.g. A^ijk ==> 2,1,0) to slot index (in the order written by hand, e.g. A^ijk ==> 1,2,3).</summary>
   // /// <param name="slotInx">Slot index.</param>
   // /// <remarks>Implementation is actually identical to the one in the ToNaturalInx method.</remarks>
   // int ToRankInx(int slotInx) =>
   //    ChangeRankNotation(Structure.Count, slotInx);
   
   /// <summary>A method that transorms between slot index and rank index (works both ways).</summary>
   /// <param name="topRankInx">Rank of the top-most tensor in the hierarchy.</param>
   /// <param name="slotOrRankInx">Slot (e.g. A^ijk ==> 1,2,3) or rank (e.g. A^ijk ==> 2,1,0) index.</param>
   static int ChangeRankNotation(int topRankInx, int slotOrRankInx) =>
      topRankInx - slotOrRankInx;

   /// <summary>A method that transorms between slot index and rank index (works both ways).</summary>
   /// <param name="topRankInx">Rank of the top-most tensor in the hierarchy.</param>
   /// <param name="slotOrRankInx">Slot (e.g. A^ijk ==> 1,2,3) or rank (e.g. A^ijk ==> 2,1,0) index.</param>
   static int ChangeRankNotation(Tensor<τ,α> topTnr, int slotOrRankInx) =>
      topTnr.Rank - slotOrRankInx;


}
}