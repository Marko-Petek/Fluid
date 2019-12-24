using System;
using System.Collections.Generic;
using System.Linq;
using Fluid.Internals.Numerics;
namespace Fluid.Internals.Collections {


/// <summary>A tensor with specified rank and specified dimension which holds direct subordinates of type τ.</summary>
/// <typeparam name="τ">Type of direct subordinates.</typeparam>
/// <typeparam name="α">Type of arithmetic.</typeparam>
public partial class Tensor<τ,α> : TensorBase<Tensor<τ,α>>, IEquatable<Tensor<τ,α>>
where τ : IEquatable<τ>, new()
where α : IArithmetic<τ>, new() {
   internal Tensor(int cap) : base(cap) { }
   /// <summary>Assigns rank and capacity only.</summary>
   /// <param name="rank">Tensor's rank.</param>
   /// <param name="cap">Initial capacity of internal storage.</param>
   internal Tensor(int rank, int cap) : this(cap) {
      Rank = rank;
   }
   /// <summary>Assigns structure by ref, rank, superior and capacity.</summary>
   /// <param name="structure">Structure array.</param>
   /// <param name="rank">Rank.</param>
   /// <param name="sup">Superior.</param>
   /// <param name="cap">Capacity.</param>
   internal Tensor(List<int> structure, int rank, Tensor<τ,α> sup, int cap) : base(cap) {
      Structure = structure;
      Rank = rank;
      Superior = sup;
   }
   /// <summary>Creates a top tensor with specified structure and initial capacity. Rank is assigned as the length of structure array.</summary>
   /// <param name="structure">Specifies dimension of each rank.</param>
   /// <param name="cap">Initially assigned memory.</param>
   internal Tensor(List<int> structure, int cap = 6) : this(structure, structure.Count, Voids<τ,α>.Vec, cap) { }
   /// <summary>Creates a non-top tensor with specified superior and initial capacity. Rank is assigned as one less that superior.</summary>
   /// <param name="sup">Tensor directly above in hierarchy.</param>
   /// <param name="cap">Initially assigned memory.</param>
   internal Tensor(Tensor<τ,α> sup, int cap) : this(sup.Structure, sup.Rank - 1, sup, cap) { }
   /// <summary>Creates a deep copy of specified tensor. You can optionally specify which meta-fields (Structure, Rank, Superior) to copy.</summary>
   /// <param name="src">Source tensor to copy.</param>
   /// <param name="cs">Exact specification of fields to copy.</param>
   internal Tensor(in Tensor<τ,α> src, in CopySpecStruct cs) :
      base(src.Count + cs.ExtraCapacity) {
      TensorFactory<τ,α>.Copy(src, this, in cs);
   }
   /// <summary>Creates a new tensor from src by copying values and rank, leaving structure and superior unassigned.</summary>
   /// <param name="src">Tensor to copy.</param>
   internal Tensor(in Tensor<τ,α> src) : this(in src, in CopySpecs.S322_00) { }
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