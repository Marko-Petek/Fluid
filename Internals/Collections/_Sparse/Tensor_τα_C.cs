using System;
using System.Collections.Generic;
using System.Linq;
using Fluid.Internals.Numerics;
namespace Fluid.Internals.Collections {


/// <summary>A tensor with specified rank and specified dimension which holds direct subordinates of type τ.</summary>
/// <typeparam name="τ">Type of direct subordinates.</typeparam>
/// <typeparam name="α">Type of arithmetic.</typeparam>
public partial class Tensor<τ,α> : TensorBase<Tensor<τ,α>>, IEquatable<Tensor<τ,α>>
where τ : struct, IEquatable<τ>, IComparable<τ>
where α : IArithmetic<τ> {
   /// <summary>Incomplete constructor. Initializes: internal dictionary with specified capacity. Does not initialize: structure, rank, superior.</summary>
   /// <param name="cap">Capacity of internal dictionary.</param>
   internal Tensor(int cap) : base(cap) {
   }
   /// <summary>Incomplete constructor. Initializes: internal dictionary with specified capacity, rank. Does not initialize: structure, superior.</summary>
   /// <param name="rank">Tensor's rank.</param>
   /// <param name="cap">Capacity of internal dictionary.</param>
   internal Tensor(int rank, int cap) : this(cap) {
      Rank = rank;
   }
   /// <summary>Complete, redundant constructor, used internally. Initializes: internal dictionary with specified capacity, structure, rank, superior.</summary>
   /// <param name="strc">Structure (absorbed).</param>
   /// <param name="rank">Rank.</param>
   /// <param name="sup">Direct superior.</param>
   /// <param name="cap">Capacity of internal dictionary.</param>
   protected Tensor(List<int> strc, int rank, Tensor<τ,α>? sup, int cap) : base(cap) {
      Structure = strc;
      Rank = rank;
      Superior = sup;
   }
   /// <summary>Complete constructor for a top tensor.</summary>
   /// <param name="strc">Structure (absorbed).</param>
   /// <param name="cap">Capacity of internal dictionary.</param>
   internal Tensor(List<int> strc, int cap = 6) : this(strc, strc.Count, null, cap) {
   }
   /// <summary>Complete constructor for a non-top tensor.</summary>
   /// <param name="sup">Direct superior.</param>
   /// <param name="cap">Capacity of internal dictionary.</param>
   internal Tensor(Tensor<τ,α> sup, int cap) : this(sup.Structure, sup.Rank - 1, sup, cap) {
   }
   /// <summary>Creates a deep copy of specified tensor. You can optionally specify which meta-fields (Structure, Rank, Superior) to copy.</summary>
   /// <param name="src">Source tensor to copy.</param>
   /// <param name="cs">Exact specification of fields to copy.</param>
   internal Tensor(in Tensor<τ,α> src, in CopySpecStruct cs) :
      base(src.Count + cs.ExtraCapacity) {
      TensorFactory<τ,α>.Copy(src, this, in cs);
   }
   /// <summary>Creates a new tensor from src by copying values and rank, leaving structure and superior unassigned.</summary>
   /// <param name="src">Tensor to copy.</param>
   internal Tensor(in Tensor<τ,α> src) : this(in src, in CopySpecs.S320_00) { }
   /// <summary>Adds tnr to caller and assigns the caller's superstructure to tnr.</summary>
   /// <param name="key">Index.</param>
   /// <param name="tnr">Tensor.</param>
   new internal void Add(int key, Tensor<τ,α> tnr) {
      tnr.Superior = this;
      tnr.Structure = Structure;
      base.Add(key, tnr);
   }
   /// <summary>Adds tnr to caller. Does not assign the caller's superstructure to tnr.</summary>
   /// <param name="key">Index.</param>
   /// <param name="tnr">Tensor.</param>
   internal void AddOnly(int key, Tensor<τ,α> tnr) =>
      base.Add(key, tnr);

   /// <summary>Adds tnr to caller only if it is not empty. Assigns the caller's superstructure to tnr.</summary>
   /// <param name="key">Index.</param>
   /// <param name="tnr">Tensor.</param>
   internal void AddOnlyNonEmpty(int key, Tensor<τ,α> tnr) {
      if(tnr.Count != 0)
         Add(key, tnr);
   }
   /// <summary>Creates tnr2 as a copy of tnr1. Specify what to copy with CopySpecs.</summary>
   /// <remarks><see cref="TestRefs.TensorCopy"/></remarks>
   public virtual Tensor<τ,α> Copy(in CopySpecStruct cs) {
      if (Rank == 1) {
         var res = new Vector<τ,α>(Count + cs.ExtraCapacity);
         var thisVector = (Vector<τ,α>)this;
         TensorFactory<τ,α>.Copy(thisVector, res, in cs);
         return res; }
      else {
         var res = new Tensor<τ,α>(Count + cs.ExtraCapacity);
         TensorFactory<τ,α>.Copy(this, res, in cs);
         return res; }
   }
   
   /// <summary>Packs only the part of the Structure below this tensor into a new Structure.</summary>
   internal List<int> CopySubstructure() {
      int structInx = Structure.Count - Rank;           // TopRank - Rank --> Index where substructure begins.
      return Structure.Skip(structInx).ToList();
   }

   protected void AssignStructFromSubStruct(Tensor<τ,α> tnr) {
      int structInx = tnr.Structure.Count - tnr.Rank;
      var subStruct = tnr.Structure.Skip(structInx);
      Structure.Clear();
      foreach(var emt in subStruct) {
         Structure.Add(emt); }
   }
   /// <summary>Transforms from slot index (in the order written by hand, e.g. A^ijk ==> 1,2,3) to rank index (as situated in the hierarchy, e.g. A^ijk ==> 2,1,0).</summary>
   /// <param name="rankInx">Rank index as situated in the hierarchy. Higher number equates to being higher in the hierarchy.</param>
   int ToSlotInx(int rankInx) =>
      ChangeRankNotation(Structure.Count, rankInx);
   /// <summary>Transforms from rank index (as situated in the hierarchy, e.g. A^ijk ==> 2,1,0) to slot index (in the order written by hand, e.g. A^ijk ==> 1,2,3).</summary>
   /// <param name="slotInx">Slot index.</param>
   /// <remarks>Implementation is actually identical to the one in the ToNaturalInx method.</remarks>
   int ToRankInx(int slotInx) =>
      ChangeRankNotation(Structure.Count, slotInx);
   
   /// <summary>A method that transorms between slot index and rank index (works both ways).</summary>
   /// <param name="topRankInx">Rank of the top-most tensor in the hierarchy.</param>
   /// <param name="slotOrRankInx">Slot or rank index.</param>
   static int ChangeRankNotation(int topRankInx, int slotOrRankInx) =>
      topRankInx - slotOrRankInx;


}
}