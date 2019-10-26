/*  
 ██████╗  █████╗ ███╗   ██╗██╗  ██╗    ██╗███╗   ██╗██████╗ ██╗ ██████╗███████╗███████╗
 ██╔══██╗██╔══██╗████╗  ██║██║ ██╔╝    ██║████╗  ██║██╔══██╗██║██╔════╝██╔════╝██╔════╝
 ██████╔╝███████║██╔██╗ ██║█████╔╝     ██║██╔██╗ ██║██║  ██║██║██║     █████╗  ███████╗
 ██╔══██╗██╔══██║██║╚██╗██║██╔═██╗     ██║██║╚██╗██║██║  ██║██║██║     ██╔══╝  ╚════██║
 ██║  ██║██║  ██║██║ ╚████║██║  ██╗    ██║██║ ╚████║██████╔╝██║╚██████╗███████╗███████║
 ╚═╝  ╚═╝╚═╝  ╚═╝╚═╝  ╚═══╝╚═╝  ╚═╝    ╚═╝╚═╝  ╚═══╝╚═════╝ ╚═╝ ╚═════╝╚══════╝╚══════╝
   We have two notations for rank indices. Let N be the tensor's top rank:
   - Slot notation:
      [1, N] which is how mathematicians would assign ordering to tensor's slots.
   - Rank notation:
      [0, N-1] where the value corresponds to the rank of tensors held by that slot.
   
   Relation between notations:
   Slot 1 holds tensors of rank N-1.
   Slot 2 holds tensors of rank N-2.
   ...
   Slot N-1 holds tensors of rank 1.
   Slot N holds tensors of rank 0.

   Relation is therefore: R = N - S  or  S = N - R.

    
 ██████╗  █████╗ ███╗   ██╗██╗  ██╗    ██████╗ ███████╗██████╗ ██╗   ██╗ ██████╗████████╗██╗ ██████╗ ███╗   ██╗
 ██╔══██╗██╔══██╗████╗  ██║██║ ██╔╝    ██╔══██╗██╔════╝██╔══██╗██║   ██║██╔════╝╚══██╔══╝██║██╔═══██╗████╗  ██║
 ██████╔╝███████║██╔██╗ ██║█████╔╝     ██████╔╝█████╗  ██║  ██║██║   ██║██║        ██║   ██║██║   ██║██╔██╗ ██║
 ██╔══██╗██╔══██║██║╚██╗██║██╔═██╗     ██╔══██╗██╔══╝  ██║  ██║██║   ██║██║        ██║   ██║██║   ██║██║╚██╗██║
 ██║  ██║██║  ██║██║ ╚████║██║  ██╗    ██║  ██║███████╗██████╔╝╚██████╔╝╚██████╗   ██║   ██║╚██████╔╝██║ ╚████║
 ╚═╝  ╚═╝╚═╝  ╚═╝╚═╝  ╚═══╝╚═╝  ╚═╝    ╚═╝  ╚═╝╚══════╝╚═════╝  ╚═════╝  ╚═════╝   ╚═╝   ╚═╝ ╚═════╝ ╚═╝  ╚═══╝
   Rank reduction reduces a tensor's rank(R) by 1, therefore input tensor has to be at least R2. We eliminate a single rank in favor of an element(E) inside that rank at a specific index. Let's say we eliminate R2 in favor of E3. Imagine the whole tensor as a hierarchy, specifically, imagine R4 tensors laid out in a line as nodes, below them all R3 tensors and below those all R2 tensors as nodes. R3 tensors are connected to their respective R4 superiors and R2 tensors to their respective R3 superiors. We stop by each R3 node and choose its subordinate R2E3 (its whole branch). Now we substitute the R3 tensor we stopped by, with the chosen R2 tensor. That means each rank 3 node is removed and replaced by the chosen R2 subordinate. Therefore R3 becomes R2 and R4 becomes R3 - the whole tensor's rank is reduced by 1.

   Lowest rank we can eliminate: 0. Choose a R0Ei element for each R1Ej, wipe out entire R1 line this way. If R0Ej had a superior, add R0E1 to it.
   Highest rank we can eliminate: N. Choose the RNEi element and return it.

   Elimination process is dependent on how many ranks exist abouve the eliminated rank i:
      2+: Choose a RiEx element on each RjEy, let RiEx take RjEy's place. Wipe out entire Rj line this way. Add RiEx to RjEy's superior RkEz.
      1: Choose a RiEx element on RNE0 (the only element) and return it.
      0: Throw exception.

 
 ███╗   ███╗ █████╗ ████████╗██╗  ██╗     ██████╗ ██████╗ ███████╗
 ████╗ ████║██╔══██╗╚══██╔══╝██║  ██║    ██╔═══██╗██╔══██╗██╔════╝
 ██╔████╔██║███████║   ██║   ███████║    ██║   ██║██████╔╝███████╗
 ██║╚██╔╝██║██╔══██║   ██║   ██╔══██║    ██║   ██║██╔═══╝ ╚════██║
 ██║ ╚═╝ ██║██║  ██║   ██║   ██║  ██║    ╚██████╔╝██║     ███████║
 ╚═╝     ╚═╝╚═╝  ╚═╝   ╚═╝   ╚═╝  ╚═╝     ╚═════╝ ╚═╝     ╚══════╝
   In general:
      - void methods modify caller and preserve its superstructure. They therefore return non-top rank tensors if the caller is a non-top tensor,
      - return value methods create a new tensor with identical substructure, but no superstructure. They return top rank tensors only. If the caller is a non-top tensor, it is treated as a top rank tensor.
*/

using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;

using static Fluid.Internals.Toolbox;
using static Fluid.Internals.Numerics.MatOps;
using Fluid.Internals.Numerics;
using Fluid.Internals.Text;
using Fluid.TestRef;

namespace Fluid.Internals.Collections {

using IA = IntArithmetic;
/// <summary>A tensor with specified rank and specified dimension which holds direct subordinates of type τ.</summary>
/// <typeparam name="τ">Type of direct subordinates.</typeparam>
/// <typeparam name="α">Type of arithmetic.</typeparam>
public class Tensor<τ,α> : TensorBase<Tensor<τ,α>>, IEquatable<Tensor<τ,α>>
where τ : IEquatable<τ>, new()
where α : IArithmetic<τ>, new() {
   /// <summary>Tensor example intended to be used as an overload dummy in indexers.</summary>
   public static Tensor<τ,α> Ex { get; } = new Tensor<τ,α>(0);
   /// <summary>Hierarchy's dimensional structure. First element specifies host's (tensor highest in hierarchy) rank, while last element specifies the rank of values. E.g.: {3,2,6,5} specifies structure of a tensor of 4th rank with first rank dimension equal to 5 and fourth rank dimension to 3. Setter works properly on non-top tensors. It must change the reference.</summary>
   public List<int> Structure { get; protected set; }
   /// <summary>Rank specifies the height (level) in the hierarchy on which the tensor sits. It equals the number of levels that exist below it. It tells us how many indices we must specify before we reach the value level.</summary>
   public int Rank { get; protected set; }
   /// <summary>Superior: a tensor directly above in the hierarchy. Null if this is the highest rank tensor.</summary>
   public Tensor<τ,α> Superior { get; protected set; }

   public new int Count => CountInternal;

   protected virtual int CountInternal => base.Count;

   protected Tensor(int cap) : base(cap) { }
   /// <summary>Assigns rank and capacity only.</summary>
   /// <param name="rank">Tensor's rank.</param>
   /// <param name="cap">Capacity.</param>
   protected Tensor(int rank, int cap) : this(cap) {
      Rank = rank;
   }
   /// <summary>Assigns structure by ref, rank, superior and capacity.</summary>
   /// <param name="structure">Structure array.</param>
   /// <param name="rank">Rank.</param>
   /// <param name="sup">Superior.</param>
   /// <param name="cap">Capacity.</param>
   internal Tensor(List<int> structure, int rank, Tensor<τ,α> sup, int cap) : base(cap) {
      Structure = structure ?? null;
      Rank = rank;
      Superior = sup ?? null;
   }
   /// <summary>Creates a top tensor with specified structure and initial capacity. Rank is assigned as the length of structure array.</summary>
   /// <param name="structure">Specifies dimension of each rank.</param>
   /// <param name="cap">Initially assigned memory.</param>
   public Tensor(List<int> structure, int cap = 6) : this(structure, structure.Count, null, cap) { }
   /// <summary>Creates a non-top tensor with specified superior and initial capacity. Rank is assigned as one less that superior.</summary>
   /// <param name="sup">Tensor directly above in hierarchy.</param>
   /// <param name="cap">Initially assigned memory.</param>
   public Tensor(Tensor<τ,α> sup, int cap) : this(sup.Structure, sup.Rank - 1, sup ?? null, cap) { }
   /// <summary>Creates a deep copy of specified tensor. You can optionally specify which meta-fields (Structure, Rank, Superior) to copy.</summary>
   /// <param name="src">Source tensor to copy.</param>
   /// <param name="cs">Exact specification of fields to copy.</param>
   public Tensor(in Tensor<τ,α> src, in CopySpecStruct cs) :
      base(src.Count + cs.ExtraCapacity) {
      Copy(src, this, in cs);
   }
   /// <summary>Creates a new tensor from src by copying values and rank, leaving structure and superior unassigned.</summary>
   /// <param name="src">Tensor to copy.</param>
   public Tensor(in Tensor<τ,α> src) : this(in src, in CopySpecs.S322_00) { }
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
   protected void AddOnly(int key, Tensor<τ,α> tnr) =>
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
         Vector<τ,α>.Copy(thisVector, res, in cs);
         return res; }
      else {
         var res = new Tensor<τ,α>(Count + cs.ExtraCapacity);
         Copy(this, res, in cs);
         return res; }
   }
   /// <summary>Make a shallow or deep copy of a tensor. Set CopySpec field for fine tunning, ensure proper capacity of the target tensor.</summary>
   /// <param name="aSrc">Copy source.</param>
   /// <param name="aTgt">Copy target.</param>
   /// <param name="cs">Exact specification of what fields to copy. Default is all.</param>
   public static void Copy(in Tensor<τ,α> aSrc, Tensor<τ,α> aTgt, in CopySpecStruct cs) {
      Assume.True(aSrc.Rank > 1, () =>
         "Tensors's rank has to be at least 2 to be copied via this method.");
      CopyMetaFields(aSrc, aTgt, in cs.NonValueFieldsSpec, in cs.StructureSpec);
      if((cs.FieldsSpec & WhichFields.OnlyValues) == WhichFields.OnlyValues) {
         var newStruc = aTgt.Structure;                                             // At this point top tensor tgt has a structure created by CopyMetaFields. It will be assigned to all subsequent subtensors.
         int endRank = cs.EndRank;
         Recursion(aSrc, aTgt);

         void Recursion(Tensor<τ,α> src, Tensor<τ,α> tgt) {
         if(src.Rank > 2) {                                       // Subordinates are tensors.
            foreach (var int_subSrc in src) {
               int subKey = int_subSrc.Key;
               var subSrc = int_subSrc.Value;
               var subTgt = new Tensor<τ,α>(newStruc, subSrc.Rank, tgt, subSrc.Count);
               tgt.AddOnly(subKey, subTgt);
               if(src.Rank > endRank)
                  Recursion(subSrc, subTgt); } }
         else if(src.Rank == 2) {                                 // Subordinates are vectors.
            foreach(var int_subSrc in src) {
               int subKey = int_subSrc.Key;
               var subVec = (Vector<τ,α>) int_subSrc.Value;
               var subTgt = new Vector<τ,α>(newStruc, tgt, subVec.Count);
               tgt.AddOnly(subKey, subTgt);
               subTgt.Vals = new Dictionary<int,τ>(subVec.Vals);
            } }
         else
            throw new InvalidOperationException(
               "Tensors's rank has to be at least 2 to be copied via this method."); }
      }
   }
   public static void CopyMetaFields(Tensor<τ,α> src, Tensor<τ,α> tgt, in WhichNonValueFields mcs,
   in HowToCopyStructure scs) {
      if((mcs & WhichNonValueFields.Structure) == WhichNonValueFields.Structure) {
         if((scs & HowToCopyStructure.ReferToOriginalStructure) == HowToCopyStructure.ReferToOriginalStructure)
            tgt.Structure = src.Structure;
         else
            tgt.Structure = new List<int>(src.Structure); }
      else {                                                                        // Create empty Structure, don't just assign null. This way we can change it and impact all subtensors.
         tgt.Structure = new List<int>(4); }
      if((mcs & WhichNonValueFields.Rank) == WhichNonValueFields.Rank)
         tgt.Rank = src.Rank;
      if((mcs & WhichNonValueFields.Superior) == WhichNonValueFields.Superior)
         tgt.Superior = src.Superior ?? null;
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
   /// <summary>Creates a tensor with specified structure from values provided within a Span.</summary>
   /// <param name="slice">Span of values.</param>
   /// <param name="structure">Structure of new tensor.</param>
   public static Tensor<τ,α> FromFlatSpec(Span<τ> slice, params int[] structure) {
      int tnrRank = structure.Length;
      if(tnrRank == 1)
         return Vector<τ,α>.FromFlatSpec(slice);
      else {
         var res = new Tensor<τ,α>(structure.ToList(), tnrRank, null, structure[0]);
         Recursion(slice, 0, res);
         return res;
      }

      void Recursion(Span<τ> slc, int slot, Tensor<τ,α> tgt) {       // Specifiy slice and the structure dimension (natural rank index) to which it belongs.
         //int tgtRank = //ChangeRankNotation(structure.Length, dim);
         int nIter = structure[slot];                              // As many iterations as it is the size of the dimension.
         int nEmtsInSlice = slc.Length / nIter;
         if(tgt.Rank > 2) {
            //var res = new Tensor<τ,α>(structure.ToList(), trueRank, null, structure[dim]);
            for(int i = 0; i < nIter; ++i) {                      // Over each tensor. Create new slices and run recursion on them.
               var newSlc = slc.Slice(i*nEmtsInSlice, nEmtsInSlice);
               var subTnr = new Tensor<τ,α>(tgt.Structure, tgt.Rank - 1, tgt, structure[slot]);
               Recursion(newSlc, slot + 1, subTnr);
               if(subTnr.Count != 0)
                  tgt.AddOnly(i, subTnr); } }
         else {                                                 // We are at rank 2, subrank = vector rank.
            for(int i = 0; i < nIter; ++i) {
               var newSlc = slc.Slice(i*nEmtsInSlice, nEmtsInSlice);
               var subVec = Vector<τ,α>.FromFlatSpec(newSlc, tgt.Structure, tgt);
               if(subVec.Count != 0)
                  tgt.AddOnly(i, subVec); } }
      }
   }
   /// <summary>Create an empty tensor with optionally specified structure and superior.</summary>
   /// <param name="cap">Capacity.</param>
   /// <param name="structure">Structure whose reference will be absorbed into the new tensor.</param>
   public static Tensor<τ,α> CreateEmpty(int cap, int rank,
      List<int> structure = null, Tensor<τ,α> sup = null)
   {
      if(rank == 1)
         return Vector<τ,α>.CreateEmpty(cap, structure, sup);
      else
         return new Tensor<τ,α>(structure, rank, sup, cap);
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

   /// <summary>Tensor getting/setting indexer.</summary>
   /// <param name="overloadDummy">Type uint of first dummy argument specifies that we know we will be getting/setting a Tensor.</param>
   /// <param name="inxs">A set of indices specifiying which Tensor we want to set/get. The set length must not reach all the way out to scalar rank.</param>
   /// <remarks> <see cref="TestRefs.TensorTensorIndexer"/> </remarks>
   public Tensor<τ,α> this[Tensor<τ,α> overloadDummy, params int[] inxs] {
      get {
         Tensor<τ,α> tnr = this;
         for(int i = 0; i < inxs.Length; ++i) {
            if(!tnr.TryGetValue(inxs[i], out tnr))
               return null; }
         return tnr; }                                // No problem with tnr being null. We return above.
      set {
         Tensor<τ,α> tnr = this;
         if(value != null) {
            int n = inxs.Length - 1;
            for(int i = 0; i < n; ++i) {
               if(!tnr.TryGetValue(inxs[i], out tnr)) {                         // Crucial line: out tnr becomes the subtensor if found, if not it is created
                  tnr = new Tensor<τ,α>(Structure, tnr.Rank - 1, tnr, 6);
                  tnr.Superior.Add(inxs[i], tnr); } }
            var dict = (TensorBase<Tensor<τ,α>>) tnr;                            // tnr is now the proper subtensor.
            value.Superior = tnr;                                             // Crucial: to make the added tensor truly a part of this tensor, we must set proper superior and structure.
            value.Structure = Structure;
            dict[inxs[n]] = value; }
         else {
            for(int i = 0; i < inxs.Length; ++i) {
               if(!tnr.TryGetValue(inxs[i], out tnr))
                  return; }
            tnr.Superior.Remove(inxs[inxs.Length - 1]); } }
   }
   /// <summary>Vector getting/setting indexer. Use tnr[1f, 3] = null to remove an entry, should it exist.</summary>
   /// <param name="overloadDummy">Type float of first dummy argument specifies that we know we will be getting/setting a Vector.</param>
   /// <param name="inxs">A set of indices specifiying which Vector we want to set/get. The set length must reach exactly to Vector rank.</param>
   /// <remarks> <see cref="TestRefs.TensorVectorIndexer"/> </remarks>
   public Vector<τ,α> this[Vector<τ,α> overloadDummy, params int[] inx] {
      get {
         Tensor<τ,α> tnr = this;
         int n = inx.Length - 1;
         for(int i = 0; i < n; ++i) {
            if(!tnr.TryGetValue(inx[i], out tnr))
               return null; }
         if(tnr.TryGetValue(inx[n], out tnr))                                 // No problem with null.
            return (Vector<τ,α>)tnr;                                          // Same.
         else
            return null; }
      set {
         Tensor<τ,α> tnr = this;
         if(value != null) {
            int n = inx.Length - 1;                                           // Entry one before last chooses tensor, last chooses vector.
            for(int i = 0; i < n; ++i) {
               if(tnr.TryGetValue(inx[i], out Tensor<τ,α> tnr2)) {
                  tnr = tnr2; }
               else {                                                         // Tensor does not exist in an intermediate rank.
                  tnr = new Tensor<τ,α>(Structure, tnr.Rank - 1, tnr, 4);
                  tnr.Superior.AddOnly(inx[i], tnr); } }
            var dict = (TensorBase<Tensor<τ,α>>) tnr;                         // Tnr now refers to either a prexisting R2 tensor or a freshly created one.
            value.Superior = tnr;                                             // Crucial: to make the added vector truly a part of this tensor, we must set proper superior and structure.
            value.Structure = Structure;
            dict[inx[n]] = value; }                                           // We do not check that the value is a vector beforehand. It is assumed that the user used indexer correctly.
         else {
            int n = inx.Length;                                               // Last entry chooses vector.
            for(int i = 0; i < n; ++i) {
               if(!tnr.TryGetValue(inx[i], out tnr))
                  return; }
            tnr.Superior.Remove(inx[n - 1]); } }     // Vector.Superior.Remove
   }
   /// <summary>Scalar getting/setting indexer.</summary>
   /// <param name="inxs">A set of indices specifiying which scalar we want to set/get. The set length must reach exactly to scalar rank.</param>
   /// <remarks> <see cref="TestRefs.TensorTauIndexer"/> </remarks>
   public virtual τ this[params int[] inx] {
      get {
         Tensor<τ,α> tnr = this;
         int n = inx.Length - 2;
         for(int i = 0; i < n; ++i) {
            if(!tnr.TryGetValue(inx[i], out tnr))
               return default; }
         if(tnr.TryGetValue(inx[n], out tnr)) {                                  // No probelm with null.
            var vec = (Vector<τ,α>)tnr;                                          // Same.
            vec.Vals.TryGetValue(inx[n + 1], out τ val);
            return val; }
         else
            return default; }
      set {
         Tensor<τ,α> tnr = this;
         Tensor<τ,α> tnr2;                                                       // Temporary to avoid null problem below.
         Vector<τ,α> vec;
         if(!value.Equals(default)) {
            if(inx.Length > 1) {                                                 // At least a 2nd rank tensor.
               int n = inx.Length - 2;
               for(int i = 0; i < n; ++i) {                                      // This loop is entered only for a 3rd rank tensor or above.
                  if(tnr.TryGetValue(inx[i], out tnr2)) {
                     tnr = tnr2; }
                  else {
                     tnr = new Tensor<τ,α>(Structure, tnr.Rank - 1, tnr, 6);
                     tnr.Superior.AddOnly(inx[i], tnr); }}
               if(tnr.TryGetValue(inx[n], out tnr2)) {                           // Does vector exist?
                  vec = (Vector<τ,α>) tnr2; }
               else {
                  vec = new Vector<τ,α>(Structure, tnr, 4); 
                  tnr.AddOnly(inx[n], vec); }
               vec.Vals[inx[n + 1]] = value; } }
         else {
            int n = inx.Length - 1;
            for(int i = 0; i < n; ++i) {
               if(!tnr.TryGetValue(inx[i], out tnr))
                  return; }
            vec = (Vector<τ,α>) tnr;
            vec.Vals.Remove(inx[n]); } }
   }
   /// <summary>Modifies this tensor by negating each element.</summary>
   public virtual void Negate() {
      Recursion(this);

      void Recursion(Tensor<τ,α> t1) {
         if(t1.Rank > 1) {
            foreach(var int_subT in t1)
               Recursion(int_subT.Value); }
         else {                                 // We are at the vector level.
            var vec = (Vector<τ,α>) t1;
            vec.Negate(); }
      }
   }
   /// <summary></summary>
   /// <param name="inclStopDepth">Tensors of this rank are provided to the onStopRank delegate.</param>
   /// <param name="onStopRank">Delegate is called when the rank of order inclStopRank is reached.</param>
   public static void Recursion(Tensor<τ,α> leader, Tensor<τ,α> follower,
   Action<int,Tensor<τ,α>,Tensor<τ,α>, int,Tensor<τ,α>,Tensor<τ,α>> descending,
   Action<int,Tensor<τ,α>,Tensor<τ,α>, int,Tensor<τ,α>,Tensor<τ,α>> ascending,            // take in from current level, put out from current level
   Func<int,Tensor<τ,α>,int,Tensor<τ,α>, (Tensor<τ,α>,bool)> onNoSubFolw,                   // Take in lead sub from current body and folw sup from above. Return sub folw and bool: whether to descend further.
   int inclStopRank,
   Func<int, Tensor<τ,α>, Tensor<τ,α>, (int, Tensor<τ,α>, Tensor<τ,α>)> onStopRank) {     // take from current, return from below.
      Assume.True(inclStopRank > 1, () => {
         S.A("InclusiveStopRank has to be at least 2, ");
         return S.Y("because method deals exclusively with tensors."); } );
      Descend(leader.Rank, 0, leader, follower);

      /// <summary>Returned values are from the corresponding body (from below). Input values are from above.</summary>
      (int, Tensor<τ,α>, Tensor<τ,α>) Descend(int rank, int inx,
      Tensor<τ,α> lead, Tensor<τ,α> folw) {                                               // Takes in inDat from above, returns outDat from its depth.
         if(rank > inclStopRank) {
            foreach(var inx_subLead in lead) {
               int subInx = inx_subLead.Key;
               var subLead = inx_subLead.Value;
               Tensor<τ,α> subFolw;
               if(folw.TryGetValue(subInx, out subFolw)) {
                  descending?.Invoke(inx, lead, folw, subInx, subLead, subFolw);
                  (int ssubInx, var ssubLead, var ssubFolw) =
                     Descend(rank - 1, subInx, subLead, subFolw);
                  ascending?.Invoke(subInx, subLead, subFolw,
                     ssubInx, ssubLead, ssubFolw); }
               else {
                  bool descend = false;                                    // Whether to continue descent.
                  (subFolw, descend) =
                     onNoSubFolw(subInx, subLead, inx, folw);
                  if(descend) {
                     descending?.Invoke(inx, lead, folw, subInx, subLead, subFolw);
                     (int ssubInx, var ssubLead, var ssubFolw) =
                        Descend(rank - 1, subInx, subLead, subFolw); }
                  ascending?.Invoke(subInx, subLead, subFolw,
                     ssubInx, ssubLead, ssubFolw);
               } }                                  // Do not descend further. Return subordinate and start ascending.
            throw new ArgumentException("Empty source tensor."); }                                        // empty 
         else                                                                             // inclusive StopRank reached.
            return onStopRank?.Invoke(inx, lead, folw) ??
               throw new InvalidOperationException(
                  "Reached StopRank, but no response method defined.");
      }
   }
   // /// <summary>RecurseSrc and recurseTgt are recursed simultaneously. RecurseSrc dictates the recursion. If at each step the equivalent tensor does not exist in tgt then onNoEquivalent is called and the recursion ceases afterwards. When rank of order inclStopRank is reached onStopRank is called and recursion is stopped.</summary>
   // /// <param name="recurseSrc">First tensor.</param>
   // /// <param name="recurseTgt">Second tensor.</param>
   // /// <param name="inclStopRank">Tensors of this rank are provided to the onStopRank delegate.</param>
   // /// <param name="onNoEquivalent">Delegate which creates a subTgt tensor on tgt. It is given the index, the subSrc subtensor and the tgt tensor to which the subTgt will be added.</param>
   // /// <param name="onStopRank">Delegate is called when the rank of order inclStopRank is reached.</param>
   // protected static void SimultaneousRecurse<λ,χ>(                         // λ = lower level data, χ = higher level data
   // Tensor<τ,α> recurseSrc, Tensor<τ,α> recurseTgt, int inclStopRank,
   // Func<ρ> onEmptySrc,
   // Func<λ,χ,λ> onResurface,
   // Func<int, Tensor<τ,α>, Tensor<τ,α>, ρ> onNoEquivalent,
   // Func<Tensor<τ,α>, Tensor<τ,α>, ρ> onStopRank) {
   //    Assume.True(inclStopRank > 1, () => {
   //       S.A("InclusiveStopRank has to be at least 2, ");
   //       S.A("because method deals exclusively with tensors.");
   //       return S.Y(); } );
   //    Recurse(recurseSrc, recurseTgt);

   //    ρ Recurse(Tensor<τ,α> src, Tensor<τ,α> tgt) {                        // ρ is type of info passed from the deeper level
   //       if(src.Rank > inclStopRank) {
   //          foreach(var int_subSrc in src) {
   //             int subKey = int_subSrc.Key;
   //             var subSrc = int_subSrc.Value;
   //             if(tgt.TryGetValue(subKey, out var subTgt)) {                 // Subtensor exists in aTgt.
   //                ρ resurfaceInfo = Recurse(subSrc, subTgt);
   //                return onResurface(resurfaceInfo);
   //             }
   //             else                                                        // Equivalent tensor does not exist on tgt.
   //                return onNoEquivalent(subKey, subSrc, tgt); }
   //          return onEmptySrc(); }
   //       else                                                              // inclusive StopRank reached.
   //          return onStopRank(src, tgt);
   //    }
   // }

   // /// <summary>RecurseSrc and recurseTgt are recursed simultaneously, but we know that the tgt is empty so we avoid the check for existence. RecurseSrc dictates the recursion. For each subtensor of src createSubTgt is called. When rank of order inclStopRank is reached onStopRank is called and recursion is stopped.</summary>
   // /// <param name="recurseSrc">First tensor.</param>
   // /// <param name="recurseTgt">Second, empty tensor.</param>
   // /// <param name="inclStopRank">Tensors of this rank are provided to the onStopRank delegate.</param>
   // /// <param name="createSubTgt">Delegate which creates a subTgt tensor on tgt. It is given the index, the subSrc subtensor and the tgt tensor to which the subTgt will be added.</param>
   // /// <param name="onStopRank">Delegate is called when the rank of order inclStopRank is reached.</param>
   // protected static void SimultRecurseEmptyTgt(
   //    Tensor<τ,α> recurseSrc, Tensor<τ,α> recurseTgt, int inclStopRank,
   //    Func<int, Tensor<τ,α>, Tensor<τ,α>, Tensor<τ,α>>   createSubTgt,
   //    Action<Tensor<τ,α>,Tensor<τ,α>>                    onStopRank)
   // {
   //    Assume.True(inclStopRank > 1, () => {
   //       S.A("InclusiveStopRank has to be at least 2, ");
   //       S.A("because method deals exclusively with tensors.");
   //       return S.Y(); } );
   //    Recurse(recurseSrc, recurseTgt);

   //    void Recurse(Tensor<τ,α> src, Tensor<τ,α> tgt) {
   //       if(src.Rank > inclStopRank) {
   //          foreach(var int_subSrc in src) {
   //             int subSrcKey = int_subSrc.Key;
   //             var subSrc = int_subSrc.Value;
   //             var subTgt = createSubTgt(subSrcKey, subSrc, tgt);       // Careful: tgt tensor is 1 rank higher than subSrc.
   //             Recurse(subSrc, subTgt); } }
   //       else                                                           // inclusive StopRank reached.
   //          onStopRank?.Invoke(src, tgt);
   //    }
   // }
   /// <summary>UNARY NEGATE. Creates a new tensor which is a negation of tnr1. The tensor is created as top rank, given its own substructure and no superstructure.</summary>
   /// <param name="tnr1">Operand.</param>
   /// <remarks><see cref="TestRefs.Op_TensorNegation"/></remarks>
   public static Tensor<τ,α> operator -(Tensor<τ,α> tnr1) {
      var newStructure = tnr1.CopySubstructure();
      var res = new Tensor<τ,α>(newStructure, tnr1.Rank, null, tnr1.Count);
      Recursion(tnr1, res,
         descending: (inx,lead,folw, subInx,subLead,subFolw) => ,
         ascending: null,

      )
      // SimultRecurseEmptyTgt(tnr1, res, 2,
      //    createSubTgt: (inx, subSrc, tgt) => {
      //       var subTgt = new Tensor<τ,α>(subSrc.Rank, subSrc.Count);
      //       tgt.Add(inx, subTgt);
      //       return subTgt; },
      //    onStopRank: (srcR2, tgtR2) => {
      //       foreach(var int_srcR1 in srcR2) {
      //          tgtR2.Add(int_srcR1.Key, - ((Vector<τ,α>) int_srcR1.Value)); } });
      // return res;
   }
   /// <summary>Creates a new tensor which is a sum of the two operands. The tensor is created as top rank, given its own substructure and no superstructure.</summary>
   /// <param name="tnr1">Left operand.</param>
   /// <param name="tnr2">Right operand.</param>
   /// <remarks> <see cref="TestRefs.Op_TensorAddition"/> </remarks>
   public static Tensor<τ,α> operator + (Tensor<τ,α> tnr1, Tensor<τ,α> tnr2) {
      ThrowOnSubstructureMismatch(tnr1, tnr2);
      //var newStructure = tnr1.CopySubstructure();
      var res = new Tensor<τ,α>(tnr2, CopySpecs.S322_04);          // Create a copy of second tensor.
      res.AssignStructFromSubStruct(tnr2);                               // Assign to it a new structure.
      SimultaneousRecurse(tnr1, res, 2,
         onNoEquivalent: (key, subTnr1, supTnr2) => {    // Simply copy and add.
            var subTnr2 = new Tensor<τ,α>(subTnr1, in CopySpecs.S322_04);
            supTnr2.Add(key, subTnr2); },
         onStopRank: (tnr1R2, tnr2R2) => {                    // stopTnrs are rank 2.
            foreach(var int_tnr1R1 in tnr1R2) {
               int vecKey = int_tnr1R1.Key;
               var vec1 = (Vector<τ,α>) int_tnr1R1.Value;
               if(tnr2R2.TryGetValue(vecKey, out var tnr2R1)) {         // Same subtensor exists in target. Sum them and overwrite the one on target.
                  var vec2 = (Vector<τ,α>) tnr2R1;
                  var sumVec = vec1 + vec2;
                  tnr2R2[Vector<τ,α>.Ex, vecKey] = sumVec; }
               else {                                                               // Same subtensor does not exist on target. Copy branch from source.
                  var sumVec = new Vector<τ,α>(vec1, in CopySpecs.S322_04);
                  tnr2R2.Add(vecKey, sumVec); } } } );
      return res;
   }
   /// <summary>Creates a new tensor which is a difference of the two operands. The tensor is created as top rank, given its own substructure and no superstructure.</summary>
   /// <param name="tnr1">Left operand.</param>
   /// <param name="tnr2">Right operand.</param>
   /// <remarks>First, we create our result tensor as a copy of tnr1. Then we take tnr2 as recursion dictator (yielding subtensors subTnr2) and we look for equivalent subtensors on tnr1 (call them subTnr1). If no equivalent is found, we negate subTnr2 and add it to a proper place in the result tensor, otherwise we subtract subTnr2 from subTnr1 and add that to result. Such a subtraction can yield a zero tensor (without entries) in which case we make sure we remove it (the empty tensor) from the result.
   /// Tests: <see cref="TestRefs.Op_TensorSubtraction"/> </remarks>
   public static Tensor<τ,α> operator - (Tensor<τ,α> tnr1, Tensor<τ,α> tnr2) {
      ThrowOnSubstructureMismatch(tnr1, tnr2);
      var res = new Tensor<τ,α>(tnr1, CopySpecs.S322_04);                           // Result tnr starts as a copy of tnr1.
      res.AssignStructFromSubStruct(tnr1);
      SimultaneousRecurse<(bool,int,Tensor<τ,α>,int,Tensor<τ,α>)>(tnr2, res, 2,                         // tnr2 = recursion dictator, res = recursion peer       (bool,Tnr,Tnr) = (isNowEmpty, highTnr, lowTnr)
         onEmptySrc: () => (true, 0, null, 0, null),
         onResurface: ( tup ) => {
            if(!tup.Item1)                                                          // subTnr not empty, subTnr present as item4 with index item3.
               tup.Item2[Tensor<τ,α>.Ex, tup.Item3] = tup.Item4;
         },
         onNoEquivalent: (key, subTnr2, supRes) => {    // Simply copy, negate and add. Could be done more optimally. BUt I'll leave it like that for now, because it's simpler.
            var subTnr2Copy = new Tensor<τ,α>(subTnr2, in CopySpecs.S322_04);
               subTnr2Copy.Negate();
               supRes.Add(key, subTnr2Copy);
         },
         onStopRank: (tnr2R2, res1R2) => {                    // stopTnrs are rank 2.
            foreach(var int_tnr2R1 in tnr2R2) {
               if(res1R2.TryGetValue(int_tnr2R1.Key, out var res1R1)) {         // Same subtensor exists in target. Sum them and overwrite the one on target.
                  int subKey = int_tnr2R1.Key;
                  var subVec2 = (Vector<τ,α>) int_tnr2R1.Value;
                  var difVec = (Vector<τ,α>) res1R1 - subVec2;
                  res1R2[Vector<τ,α>.Ex, subKey] = difVec; }                     // FIXME: This can be zero. What then?
               else {                                                               // Same subtensor does not exist on target. Copy branch from source and negate it.
                  var srcCopy = new Vector<τ,α>((Vector<τ,α>) int_tnr2R1.Value,
                     in CopySpecs.S322_04);
                  srcCopy.Negate();
                  res1R2.Add(int_tnr2R1.Key, srcCopy); } }
         } );
      return res;
   }
   /// <summary>Sums tnr2 into the caller. Don't use the tnr2 reference afterwards.</summary>
   /// <param name="tnr2">Sumand whose elements will be absorbed into the caller and shouldn't be used afterwards.</param>
   /// <remarks> <see cref="TestRefs.TensorSum"/> </remarks>
   public void Sum(Tensor<τ,α> tnr2) {
      Tensor<τ,α> tnr1 = this;
      ThrowOnSubstructureMismatch(tnr1, tnr2);
      Recursion(tnr1, tnr2);

      void Recursion(Tensor<τ,α> t1, Tensor<τ,α> t2) {
         if(t2.Rank > 2) {
            foreach(var int_subTnr2 in t2) {
               int subKey = int_subTnr2.Key;
               var subTnr2 = int_subTnr2.Value;
               if(t1.TryGetValue(subKey, out var subTnr1))            // Equivalent subtensor exists in T1.
                  Recursion(subTnr1, subTnr2);
               else                                                      // Equivalent subtensor does not exist in T1. Absorb the subtensor from T2 and add it.
                  t1.Add(subKey, subTnr2); } }
         else if(t2.Rank == 2) {
            foreach(var int_subTnr2 in t2) {
               //var vec2 = (Vector<τ,α>) int_subTnr2.Value;
               int subKey = int_subTnr2.Key;
               var subTnr2 = int_subTnr2.Value;
               if(t1.TryGetValue(subKey, out var subTnr1)) {      // Entry exists in t1, we must sum.
                  var subVec1 = (Vector<τ,α>) subTnr1;
                  var subVec2 = (Vector<τ,α>) subTnr2;
                  subVec1.Sum(subVec2);
                  if(subVec1.Count == 0)
                     t1.Remove(subKey); }                         // Crucial to remove if subvector has been anihilated.
               else {
                  t1.Add(subKey, subTnr2); } } }          // Entry does not exist in t2, simply Add.
         else {                                                            // We have a vector.
            var vec1 = (Vector<τ,α>) t1;
            var vec2 = (Vector<τ,α>) t2;
            t1.Sum(t2);
         }
      }
   }
   /// <summary>Subtracts tnr2 from the caller. Tnr2 is still usable afterwards.</summary>
   /// <param name="aTnr2">Minuend which will be subtracted from the caller. Minuend is still usable after the operation.</param>
   /// <remarks><see cref="TestRefs.TensorSub"/></remarks>
   public void Sub(Tensor<τ,α> aTnr2) {
      Tensor<τ,α> aTnr1 = this;
      ThrowOnSubstructureMismatch(aTnr1, aTnr2);
      Recursion(aTnr1, aTnr2);

      void Recursion(Tensor<τ,α> tnr1, Tensor<τ,α> tnr2) {
         if(tnr2.Rank > 2) {
            foreach(var int_subTnr2 in tnr2) {
               int subKey = int_subTnr2.Key;
               var subTnr2 = int_subTnr2.Value;
               if(tnr1.TryGetValue(subKey, out var subTnr1))            // Equivalent subtensor exists in T1.
                  Recursion(subTnr1, subTnr2);
               else                                                      // Equivalent subtensor does not exist in T1. Negate the subtensor from T2 and add it.
                  tnr1.Add(subKey,-subTnr2); } }
         else if(tnr2.Rank == 2) {
            foreach(var int_subTnr2 in tnr2) {
               int subKey = int_subTnr2.Key;
               var subVec2 = (Vector<τ,α>) int_subTnr2.Value;
               if(tnr1.TryGetValue(subKey, out var subTnr1)) {      // Entry exists in t1, we must sum.
                  var subVec1 = (Vector<τ,α>) subTnr1;
                  subVec1.Sub(subVec2);
                  if(subVec1.Count == 0)
                     tnr1.Remove(subKey); }                         // Crucial to remove if subvector has been anihilated.
               else {
                  tnr1.Add(subKey, -subVec2); } } }          // Entry does not exist in t2, simply Add.
         else {                                                            // We have a vector.
            var vec1 = (Vector<τ,α>) tnr1;
            var vec2 = (Vector<τ,α>) tnr2;
            vec1.Sum(vec2);
         }
      }
   }

   /// <summary>Multiplies caller with a scalar.</summary>
   /// <param name="scal">Scalar.</param>
   /// <remarks> <see cref="TestRefs.TensorMul"/> </remarks>
   public virtual void Mul(τ scal) {
      Recursion(this);

      void Recursion(Tensor<τ,α> tnr) {
         if(tnr.Rank > 2) {                                       // Subordinates are tensors.
            foreach (var int_subTnr in tnr) {
               var subTnr = int_subTnr.Value;
               Recursion(subTnr); } }
         else if(tnr.Rank == 2) {                                 // Subordinates are vectors.
            foreach (var int_subTnrR1 in tnr) {
               var subVec = (Vector<τ,α>) int_subTnrR1.Value;
               subVec.Mul(scal); } }
         else {
            var vec = (Vector<τ,α>) tnr;
            vec.Mul(scal); }
      }
   }
   /// <summary>Creates a new tensor which is a product of a scalar and a tensor. The tensor is created as top rank, given its own substructure and no superstructure.</summary>
   /// <param name="scal">Scalar.</param>
   /// <param name="aTnr">Tensor.</param>
   /// <remarks> <see cref="TestRefs.Op_ScalarTensorMultiplication"/> </remarks>
   public static Tensor<τ,α> operator * (τ scal, Tensor<τ,α> aTnr) {
      var newStructure = aTnr.CopySubstructure();                                              // New substructure.
      return Recursion(aTnr);

      Tensor<τ,α> Recursion(in Tensor<τ,α> tnr) {
         var res = new Tensor<τ,α>(newStructure, aTnr.Rank, null, aTnr.Count);
         if(tnr.Rank > 2) {                                       // Subordinates are tensors.
            foreach (var int_subTnr in tnr) {
               int subKey = int_subTnr.Key;
               var subTnr = int_subTnr.Value;
               var subRes = Recursion(subTnr);
               res.Add(subKey, subRes); } }
         else if(tnr.Rank == 2) {                                 // Subordinates are vectors.
            foreach (var int_subTnr in tnr) {
               int subKey = int_subTnr.Key;
               var subVec = (Vector<τ,α>) int_subTnr.Value;
               res.Add(subKey, scal*subVec); } }
         else
            return scal*((Vector<τ,α>) tnr);
         return res;
      }
   }
   /// <summary>Calculates tensor product of this tensor (left-hand operand) with another tensor (right-hand operand).</summary>
   /// <param name="aTnr2">Right-hand operand.</param>
   /// <remarks> <see cref="TestRefs.TensorProduct"/> </remarks>
   public virtual Tensor<τ,α> TnrProduct(Tensor<τ,α> aTnr2) {
      // Overriden on vector when first operand is a vector.
      // 1) Descend to rank 1 through a recursion and then delete that vector.
      // 2) Substitute it with a tensor of rank tnr2.Rank + 1 whose entries are tnr2s multiplied by the corresponding scalar that used to preside there in the old vector.
      int newRank = Rank + aTnr2.Rank;
      var struct1 = CopySubstructure();
      var struct2 = aTnr2.CopySubstructure();
      var newStructure = struct1.Concat(struct2).ToList();
      return Recursion(this, newRank);

      Tensor<τ,α> Recursion(Tensor<τ,α> tnr1, int resRank) {
         var res = new Tensor<τ,α>(newStructure, resRank, null, tnr1.Count);
         if(tnr1.Rank > 2) {                                                    // Only copy. Adjust ranks.
            foreach(var int_subTnr1 in tnr1) {
               int subKey = int_subTnr1.Key;
               var subTnr1 = int_subTnr1.Value;
               res.Add(subKey, Recursion(subTnr1, resRank - 1)); } }
         else {                              // We are now at tensor which contains vectors.
            foreach(var int_subTnr1R1 in tnr1) {    // Substitute each vector wiht a new tensor.
               int subKeyR1 = int_subTnr1R1.Key;
               var subVec1 = (Vector<τ,α>) int_subTnr1R1.Value;
               var newSubTnr = new Tensor<τ,α>(newStructure, resRank - 1, null, tnr1.Count);
               foreach(var int_subVal1 in subVec1.Vals) {
                  int subKeyR0 = int_subVal1.Key;
                  var subVal1 = int_subVal1.Value;
                  newSubTnr.Add(subKeyR0, subVal1*aTnr2); }
               res.Add(subKeyR1, newSubTnr); } }
         return res;
      }
   }
   /// <summary>Eliminates a single rank out of a tensor by choosing a single subtensor at that rank and making it take the place of its direct superior (thus discarding all other subtensors at that rank). The resulting tensor has therefore its rank reduced by one.</summary>
   /// <param name="elimRank"> True, zero-based rank index of rank to be eliminated.</param>
   /// <param name="emtInx">Zero-based element index in that rank in favor of which the elimination will take place.</param>
   /// <remarks><see cref="TestRefs.TensorReduceRank"/></remarks>
   public Tensor<τ,α> ReduceRank(int elimRank, int emtInx) {
      Assume.True(elimRank < Rank && elimRank > -1, () => {
         S.A("You can only eliminate a non-negative ");
         S.A("rank greater than or equal to top rank.");
         return S.Y(); } );
      var newStructureL = Structure.Take(elimRank);
      var newStructureR = Structure.Skip(elimRank + 1);
      var newStructure = newStructureL.Concat(newStructureR).ToList();    // Created a new structure. Assign it to new host tensor.

      if(elimRank == Rank - 1) {                                                    // 1 rank exists above elimRank == Pick one from elimRank and return it.
         if(Rank > 1) {                                                             // Result is Tensor, top tensor at least rank 2.
            if(TryGetValue(emtInx, out var subTnr)) {                                   // Sought after subordinate exists.
               var newTnr = subTnr.Copy(in CopySpecs.S322_04);                          // Works properly even for Vector.
               newTnr.Structure = newStructure;
               return newTnr;}
            else
               return CreateEmpty(0, newStructure.Count, newStructure); }                                // Return empty tensor.
         else                                                                                      // Rank <= 1: impossible.
            throw new ArgumentException("Cannot eliminate rank 1 or lower on rank 1 tensor."); }
      else if(elimRank > 1) {                                                       // At least two ranks exist above elimRank & elimRank is at least 2. Obviously applicable only to Rank 4 or higher tensors.
         var res = new Tensor<τ,α>(newStructure, Rank - 1, null, Count);
         if(Rank > 3) {                                                              // No special treatment due to Vector needed.
            RecursiveCopyAndElim(this, res, emtInx, elimRank + 2);
            return res; }
         else
            throw new ArgumentException("Cannot eliminate rank 2 or above on rank 1,2,3 tensor with this branch."); }
      else if(elimRank == 1) {                                                      // At least two ranks exist above elimRank & elimRank is 1. Obviously applicable only to rank 3 or higher tensors.
         var res = new Tensor<τ,α>(newStructure, Rank - 1, null, Count);
         if(Rank > 2) {                                                             // Result is tensor.
            RecursiveCopyAndElim(this, res, emtInx, 1);
            return res; }
         else
            throw new ArgumentException("Cannot eliminate rank 1 on rank 1,2 tensor with this branch."); }
      else {                                          // At least two ranks exist above elimRank & elimRank is 0. Obviously applicable only to rank 2 or higher tensors.
         if(Rank > 2) {                               // Result is tensor. Choose one value from each vector in subordinate rank 2 tensors, build a new vector and add those values to it. Then add that vector to superior rank 3 tensor.
            var res = new Tensor<τ,α>(newStructure, Rank - 1, null, Count);
            ElimR0_R3Plus(this, res, emtInx);
            return res; }
         else if(Rank == 2) {
            var res = new Vector<τ,α>(newStructure, null, 4);
            ElimR0_R2(this, res, emtInx);
            return res; }
         else
            throw new ArgumentException("Cannot eliminate rank 0 on rank 1 tensor with this branch."); }
   }

   // static void TnrElimination(Tensor<τ,α> src, Tensor<τ,α> tgt, int emtInx) {        // src is 2 ranks above elimRank and at least rank 3.
   //     }

   /// <summary>Can only be used to eliminate rank 3 or higher. Provided target has to be initiated one rank lower than source.</summary>
   /// <param name="src">Source tensor whose rank we are eliminating.</param>
   /// <param name="tgt">Target tensor. Has to be one rank lower than source.</param>
   /// <param name="emtInx">Element index in favor of which we are eliminating.</param>
   /// <param name="elimRank"></param>
   static void RecursiveCopyAndElim(Tensor<τ,α> src, Tensor<τ,α> tgt, int emtInx, int elimRank) {      // Recursively copies tensors.
      if(src.Rank > elimRank + 2) {                                    // We have not yet reached rank directly above rank scheduled for elimination: copy rank.
         foreach(var int_tnr in src) {
            var subTnr = new Tensor<τ,α>(tgt, src.Count);
            RecursiveCopyAndElim(int_tnr.Value, subTnr, emtInx, elimRank);
            if(subTnr.Count != 0)
               tgt.Add(int_tnr.Key, subTnr); } }
      else {                                                             // We have reached rank directly above rank scheduled for elimination: eliminate.
         foreach(var int_tnr in src) {
            if(int_tnr.Value.TryGetValue(emtInx, out var subTnr)) {
               var subTnrCopy = subTnr.Copy(in CopySpecs.S322_04);
               tgt.Add(int_tnr.Key, subTnrCopy); } } }
   }

   /// <summary>Eliminates rank 0 on a rank 2 tensor, resulting in a rank 1 tensor (vector),</summary>
   /// <param name="src">Rank 2 tensor.</param>
   /// <param name="tgt">Initialized result vector.</param>
   /// <param name="emtInx">Element index in favor of which the elimination will proceed.</param>
   public static void ElimR0_R2(Tensor<τ,α> src, Vector<τ,α> tgt, int emtInx) {
      Assume.True(src.Rank == 2, () =>
         "This method is intended for rank 2 tensors only.");
      foreach(var int_tnrR1 in src) {
         var subVec = (Vector<τ,α>) int_tnrR1.Value;
         if(subVec.Vals.TryGetValue(emtInx, out var val))
            tgt.Add(int_tnrR1.Key, val); }
   }
   /// <summary>Eliminate rank 0 on a rank 3 or higher tensor.</summary>
   /// <param name="src">Rank 3 or higher tensor.</param>
   /// <param name="tgt">Tensor one rank lower than source.</param>
   /// <param name="emtInx">Element index in favor of which to eliminate.</param>
   public static void ElimR0_R3Plus(Tensor<τ,α> src, Tensor<τ,α> tgt, int emtInx) {
      Assume.True(src.Rank > 2, () =>
         "This method is applicable to rank 3 and higher tensors.");
      if(src.Rank > 3) {
         foreach(var int_tnr in src) {
            var subTnr = new Tensor<τ,α>(tgt, src.Count);
            ElimR0_R3Plus(int_tnr.Value, subTnr, emtInx);
            if(subTnr.Count != 0)
               tgt.Add(int_tnr.Key, subTnr); } }
      else {                                                                  // src.Rank == 3.
         foreach(var int_tnr in src) {
            var newVec = new Vector<τ,α>(tgt, 4);
            ElimR0_R2(int_tnr.Value, newVec, emtInx);
            tgt.Add(int_tnr.Key, newVec); } } }

   protected static (List<int> struc, int rankInx1, int rankInx2, int conDim) ContractPart1(
   Tensor<τ,α> tnr1, Tensor<τ,α> tnr2, int slotInx1, int slotInx2) {
      // 1) First eliminate, creating new tensors. Then add them together using tensor product.
      List<int>   struc1 = tnr1.Structure, 
                  struc2 = tnr2.Structure;
      int rank1 = struc1.Count,
            rank2 = struc2.Count;
      Assume.True(rank1 == tnr1.Rank && rank2 == tnr2.Rank,
         () => "One of the tensors is not top rank.");
      Assume.AreEqual(struc1[slotInx1 - 1], struc2[slotInx2 - 1],              // Check that the dimensions of contracted ranks are equal.
         "Rank dimensions at specified indices must be equal.");
      int   conDim = tnr1.Structure[slotInx1 - 1],                                // Dimension of rank we're contracting.
            rankInx1 = tnr1.ToRankInx(slotInx1),
            rankInx2 = tnr2.ToRankInx(slotInx2);
      var struc3_1 = struc1.Where((emt, i) => i != (slotInx1 - 1));
      var struc3_2 = struc2.Where((emt, i) => i != (slotInx2 - 1));
      var struc3 = struc3_1.Concat(struc3_2).ToList();                 // New structure.
      return (struc3, rankInx1, rankInx2, conDim);
   }
   
   public static Tensor<τ,α> ContractPart2(Tensor<τ,α> tnr1, Tensor<τ,α> tnr2, int rankInx1, int rankInx2, List<int> struc3, int conDim) {
      // 1) First eliminate, creating new tensors. Then add them together using tensor product.
      if(tnr1.Rank > 1) {
         if(tnr2.Rank > 1) {                                // First tensor is rank 2 or more.
            Tensor<τ,α> elimTnr1, elimTnr2, sumand, sum;
            sum = new Tensor<τ,α>(struc3);                                    // Set sum to a zero tensor.
            for(int i = 0; i < conDim; ++i) {
               elimTnr1 = tnr1.ReduceRank(rankInx1, i);
               elimTnr2 = tnr2.ReduceRank(rankInx2, i);
               if(elimTnr1.Count != 0 && elimTnr2.Count != 0) {
                  sumand = elimTnr1.TnrProduct(elimTnr2);
                  sum.Sum(sumand); } }
            //if(sum.Count != 0)
            return sum;
            //else
            //   return null;
         }
         else {                                                // Second tensor is rank 1 (a vector).
            Vector<τ,α> vec = (Vector<τ,α>) tnr2;
            if(tnr1.Rank == 2) {                                    // Result will be vector.
               Vector<τ,α> elimVec, sumand, sum;
               sum = new Vector<τ,α>(struc3, null, 4);
               for(int i = 0; i < conDim; ++i) {
                  elimVec = (Vector<τ,α>) tnr1.ReduceRank(rankInx1, i);
                  if(elimVec.Count != 0 && vec.Vals.TryGetValue(i, out var val)) {
                     sumand = val*elimVec;
                     sum.Sum(sumand); } }
               if(sum.Vals.Count != 0)
                  return sum;
               else
                  return CreateEmpty(0, 1, struc3); }
            else {                                             // Result will be tensor.
               Tensor<τ,α> elimTnr1, sumand, sum;
               sum = new Tensor<τ,α>(struc3);
               for(int i = 0; i < conDim; ++i) {
                  elimTnr1 = tnr1.ReduceRank(rankInx1, i);
                  if(elimTnr1.Count != 0 && vec.Vals.TryGetValue(i, out var val)) {
                     sumand = val*elimTnr1;
                     sum.Sum(sumand); } }
               if(sum.Count != 0)
                  return sum;
               else
                  return CreateEmpty(0, struc3.Count, struc3); } } }
      else {                                                   // First tensor is rank 1 (a vector).
         var vec1 = (Vector<τ,α>) tnr1;
         return Vector<τ,α>.ContractPart2(vec1, tnr2, rankInx2, struc3, conDim);}
   }

   /// <summary>Contracts two tensors over specified natural rank indices. Example: Contraction writen as A^(ijkl)B^(mnip) is specified as a (0,2) contraction of A and B, not a (3,1) contraction. Tensor contraction is a generalization of trace, which can further be viewed as a generalization of dot product.</summary>
   /// <param name="tnr2">Tensor 2.</param>
   /// <param name="slotInx1">One-based natural index on this tensor over which to contract.</param>
   /// <param name="slotInx2">One-based natural index on tensor 2 over which to contract (it must hold: dim(rank(inx1)) = dim(rank(inx2)).</param>
   /// <remarks><see cref="TestRefs.TensorContract"/></remarks>
   public static Tensor<τ,α> Contract(Tensor<τ,α> tnr1, Tensor<τ,α> tnr2, int slotInx1, int slotInx2) {
      (List<int> struc3, int rankInx1, int rankInx2, int conDim) = ContractPart1(tnr1, tnr2, slotInx1, slotInx2);
      return ContractPart2(tnr1, tnr2, rankInx1, rankInx2, struc3, conDim);
   }
   /// <summary>Contracts across the two slot indices on a rank 2 tensor.</summary>
   /// <remarks> <see cref="TestRefs.TensorSelfContractR2"/> </remarks>
   public τ SelfContractR2() {
      Assume.True(Rank == 2, () => "Tensor rank has to be 2 for this method.");
      Assume.True(Structure[0] == Structure[1], () =>
         "Corresponding dimensions have to be equal.");
      τ result = default;
      foreach(var int_vec in this) {
         var vec = (Vector<τ,α>) int_vec.Value;
         if(vec.Vals.TryGetValue(int_vec.Key, out τ val))
            result = O<τ,α>.A.Sum(result, val); }
      return result;
   }

   public Vector<τ,α> SelfContractR3(int natInx1, int natInx2) {
      Assume.True(Rank == 3, () => "Tensor rank has to be 3 for this method.");
      Assume.True(Structure[natInx1 - 1] == Structure[natInx2 - 1], () =>
         "Corresponding dimensions have to be equal.");
      Vector<τ,α> res = new Vector<τ,α>(new List<int> {Structure[2]}, null, 4);
      int truInx1 = ToRankInx(natInx1);
      int truInx2 = ToRankInx(natInx2);
      if(natInx1 == 1) {
         if(natInx2 == 2) {
            foreach(var int_tnr in this) {
               if(int_tnr.Value.TryGetValue(int_tnr.Key, out var subTnr)) {
                  var vec = (Vector<τ,α>) subTnr;
                  res.Sum(vec); } } }
         if(natInx2 == 3) {
            foreach(var int_tnr in this) {
               foreach(var int_subTnr in int_tnr.Value) {
                  var subVec = (Vector<τ,α>) int_subTnr.Value;
                  if(subVec.Vals.TryGetValue(int_tnr.Key, out τ val))
                     res.Vals[int_subTnr.Key] = O<τ,α>.A.Sum(res[int_subTnr.Key], val); } } } }
      else if(natInx1 == 2) {                   // natInx2 == 3
         foreach(var int_tnr in this) {
            foreach(var int_subTnr in int_tnr.Value) {
               var subVec = (Vector<τ,α>) int_subTnr.Value;
               if(subVec.Vals.TryGetValue(int_subTnr.Key, out τ val))
                  res.Vals[int_tnr.Key] = O<τ,α>.A.Sum(res[int_tnr.Key], val); } } }
      return res;
   }
   /// <summary>Contracts across two slot indices on a single tensor of at least rank 3.</summary>
   /// <param name="slotInx1">Slot index 1.</param>
   /// <param name="slotInx2">Slot index 2.</param>
   /// <remarks><see cref="TestRefs.TensorSelfContract"/></remarks>
   public Tensor<τ,α> SelfContract(int slotInx1, int slotInx2) {
      Assume.True(Rank > 2, () =>
         "This method is not applicable to rank 2 tensors.");
      Assume.True(Structure[slotInx1 - 1] == Structure[slotInx2 - 1], () =>
         "Dimensions of contracted slots have to be equal.");
      if(Rank > 3) {
         var newStruct1 = Structure.Take(slotInx1 - 1);
         var newStruct2 = Structure.Take(slotInx2 - 1).Skip(slotInx1);
         var newStruct3 = Structure.Skip(slotInx2);
         var newStruct = newStruct1.Concat(newStruct2).Concat(newStruct3).ToList();
         var res = new Tensor<τ,α>(newStruct, Rank - 2, null, Count);
         var rankInx1 = ToRankInx(slotInx1);
         var rankInx2 = ToRankInx(slotInx2);
         int dimRank = Structure[slotInx1 - 1];                // Dimension of contracted rank.
         for(int i = 0; i < dimRank; ++i) {                    // Over each element inside contracted ranks.
            var step1Tnr = ReduceRank(rankInx2, i);
            var sumand = step1Tnr.ReduceRank(rankInx1 - 1, i);
            res.Sum(sumand); }
         return res; }
      else
         return SelfContractR3(slotInx1, slotInx2);
   }
   /// <summary>Compares substructures of two equal rank tensors and throws an exception if they mismatch.</summary>
   /// <param name="tnr1">First tensor.</param>
   /// <param name="tnr2">Second tensor.</param>
   public static void ThrowOnSubstructureMismatch(Tensor<τ,α> tnr1, Tensor<τ,α> tnr2) {
      Assume.True(tnr1.Rank == tnr2.Rank, () => "Tensor ranks do not match.");                                    // First, ranks must match.
      int topRank1 = tnr1.Structure.Count;                                      // We have to check that all dimensions below current ranks match.
      int topRank2 = tnr2.Structure.Count;
      var structInx1 = topRank1 - tnr1.Rank;                         // Index in structure array.
      var structInx2 = topRank2 - tnr2.Rank;
      for(int i = structInx1, j = structInx2; i < topRank1; ++i, ++j) {
         if(tnr1.Structure[i] != tnr2.Structure[j])
            throw new InvalidOperationException("Tensor addition: structures do not match."); }
   }

   /// <remarks> <see cref="TestRefs.TensorEnumerateRank"/> </remarks>
   public IEnumerable<Tensor<τ,α>> EnumerateRank(int rankInx) {
      Assume.True(rankInx > 1, () =>
         "This method applies only to ranks that hold pure tensors.");
      if(Rank > rankInx + 1) {
         foreach(var subTnr in Recursion(this))
            yield return subTnr; }
      else {
         foreach(var int_subTnr in this)
            yield return int_subTnr.Value; }

      IEnumerable<Tensor<τ,α>> Recursion(Tensor<τ,α> src) {
         foreach(var int_tnr in src) {
            if(int_tnr.Value.Rank > rankInx + 1) {
               foreach(var subTnr in Recursion(int_tnr.Value))
                  yield return subTnr; }
            else {
               foreach(var int_subTnr in int_tnr.Value)
                  yield return int_subTnr.Value; } }
      }
   }

   /// <summary>Check two tensors for equality.</summary><param name="tnr2">Other tensor.</param>
   public bool Equals(Tensor<τ,α> tnr2) {
      ThrowOnSubstructureMismatch(this, tnr2);
      return TnrRecursion(this, tnr2);

      bool TnrRecursion(Tensor<τ,α> sup1, Tensor<τ,α> sup2) {
         if(!sup1.Keys.OrderBy(key => key).SequenceEqual(sup2.Keys.OrderBy(key => key)))
            return false;                                                                    // Keys have to match. This is crucial.
         if(sup1.Rank > 2) {
            foreach(var inx_sub1 in sup1) {
               var sub2 = sup2[Tensor<τ,α>.Ex, inx_sub1.Key];
               return TnrRecursion(inx_sub1.Value, sub2); }
            return true; }                                                                   // Both are empty.
         else
            return VecRecursion(sup1, sup2);
      }

      bool VecRecursion(Tensor<τ,α> sup1R2, Tensor<τ,α> sup2R2) {
         if(!sup1R2.Keys.OrderBy(key => key).SequenceEqual(sup2R2.Keys.OrderBy(key => key)))
            return false;                                                                    // Keys have to match. This is crucial.
         foreach(var inx_sub1R1 in sup1R2) {
            var vec1 = (Vector<τ,α>) inx_sub1R1.Value;
            var vec2 = sup2R2[Vector<τ,α>.Ex, inx_sub1R1.Key];
            if(!vec1.Equals(vec2))
               return false; }
         return true;
      }
   }

   /// <remarks> <see cref="TestRefs.TensorEquals"/> </remarks>
   public bool Equals(Tensor<τ,α> tnr2, τ eps) {
      ThrowOnSubstructureMismatch(this, tnr2);
      return TnrRecursion(this, tnr2);

      bool TnrRecursion(Tensor<τ,α> sup1, Tensor<τ,α> sup2) {
         if(!sup1.Keys.OrderBy(key => key).SequenceEqual(sup2.Keys.OrderBy(key => key)))
            return false;                                                                    // Keys have to match. This is crucial.
         if(sup1.Rank > 2) {
            foreach(var inx_sub1 in sup1) {
               var sub2 = sup2[Vector<τ,α>.Ex, inx_sub1.Key];
               return TnrRecursion(inx_sub1.Value, sub2); }
            return true; }                                                                   // Both are empty.
         else
            return VecRecursion(sup1, sup2);
      }

      bool VecRecursion(Tensor<τ,α> sup1R2, Tensor<τ,α> sup2R2) {
         if(!sup1R2.Keys.OrderBy(key => key).SequenceEqual(sup2R2.Keys.OrderBy(key => key)))
            return false;                                                                    // Keys have to match. This is crucial.
         foreach(var inx_sub1R1 in sup1R2) {
            var vec1 = (Vector<τ,α>) inx_sub1R1.Value;
            var vec2 = sup2R2[Vector<τ,α>.Ex, inx_sub1R1.Key];
            if(!vec1.Equals(vec2, eps))
               return false; }
         return true;
      }                                                         // All values agree within tolerance.
   }
   /// <summary>Create a string of all non-zero elements in form {{key1, val1}, {key2, val2}, ..., {keyN,valN}}.</summary>
   public override string ToString() {
      StringBuilder sb = new StringBuilder(72);
      sb.Append("{");
      foreach(var elm in this.OrderBy( kvPair => kvPair.Key ))
         sb.Append($"{{{elm.Key.ToString()}, {elm.Value.ToString()}}}, ");
      int length = sb.Length;
      sb.Remove(length - 2, 2);
      sb.Append("}");
      return sb.ToString();
   }

   public static class CopySpecs {
      /// <summary>Copies values, rank, structure (brand new structure is created) and superior.</summary>
      public static readonly CopySpecStruct S342_00 = new CopySpecStruct(
         WhichFields.ValuesAndNonValueFields,
         WhichNonValueFields.All,
         HowToCopyStructure.CreateNewStructure,
         endRank: 0,
         extraCapacity: 0);
      /// <summary>Copies values (adds 4 extra capacity), rank, structure (brand new structure is created) and superior.</summary>
      public static readonly CopySpecStruct S342_04 = new CopySpecStruct(
         WhichFields.ValuesAndNonValueFields,
         WhichNonValueFields.All,
         HowToCopyStructure.CreateNewStructure,
         endRank: 0,
         extraCapacity: 4);
      /// <summary>Copy values and rank, leave Structure ans Superior unassigned.</summary>
      public static readonly CopySpecStruct S322_00 = new CopySpecStruct(
         WhichFields.ValuesAndNonValueFields,
         WhichNonValueFields.Rank,
         HowToCopyStructure.DoNotCopy,
         endRank: 0,
         extraCapacity: 0);
      /// <summary>Copy values (adds 4 extra capacity) and rank, leave Structure and Superior unassigned.</summary>
      public static readonly CopySpecStruct S322_04 = new CopySpecStruct(
         WhichFields.ValuesAndNonValueFields,
         WhichNonValueFields.Rank,
         HowToCopyStructure.DoNotCopy,
         endRank: 0,
         extraCapacity: 4);
      ///<summary>Copy values, rank, and structure (brand new created), but not superior.</summary>
      public static readonly CopySpecStruct S352_00 = new CopySpecStruct(
         WhichFields.ValuesAndNonValueFields,                              // 3
         WhichNonValueFields.AllExceptSuperior,                            // 5
         HowToCopyStructure.CreateNewStructure,                            // 2
         endRank: 0,                                                       // 0
         extraCapacity: 0                                                  // 0
      );
   }

   /// <summary>Structure that tells Tensor's Copy method how to copy a tensor.</summary>
   public readonly struct CopySpecStruct {
      /// <summary>Specify which types of fields to copy: value, non-value or both.</summary>
      public readonly WhichFields FieldsSpec;
      /// <summary>Specify which non-value fields to copy (and conversely, which to omit). Effective only if General.Meta flag is active.</summary>
      public readonly WhichNonValueFields NonValueFieldsSpec;
      /// <summary>Structure CopySpec specifies whether  to genuinely copy the Structure int[] array or only copy the reference.  Contains flags: TrueCopy (creates a new Structure array on the heap), RefCopy (copies only a reference to the Structure array on source).</summary>
      /// <remarks>Effective only if MetaFields.Structure flag is active.</remarks>
      public readonly HowToCopyStructure StructureSpec;
      /// <summary>Lowest rank at which copying of values stops.</summary>
      /// <remarks>Effective only if General.Vals flag is active. EndRank is an inclusive lower bound.</remarks>
      public readonly int EndRank;
      public readonly int ExtraCapacity;
      
      public CopySpecStruct(
         WhichFields whichFields = WhichFields.ValuesAndNonValueFields,
         WhichNonValueFields whichNonValueFields = WhichNonValueFields.All,
         HowToCopyStructure howToCopyStructure = HowToCopyStructure.CreateNewStructure,
         int endRank = 0,
         int extraCapacity = 0)
      {
         FieldsSpec = whichFields;
         NonValueFieldsSpec = whichNonValueFields;
         StructureSpec = howToCopyStructure;
         EndRank = endRank;
         ExtraCapacity = extraCapacity;
      }
   }
   /// <summary>Specify which types of fields to copy: value, non-value or both.</summary>
   [Flags] public enum WhichFields {
      /// <summary>(1) Copy only the value field (direct subtensors).</summary>
      OnlyValues  = 1,                                               // 1
      /// <summary>(2) Copy only the non-value fields (Structure, Rank, Superior).</summary>
      OnlyNonValueFields  = 1 << 1,                                  // 2
      /// <summary>(3) - Copy both the value field (direct subtensors) and non-value fields (Structure, Rank, Superior).</summary>
      ValuesAndNonValueFields = OnlyNonValueFields | OnlyValues      // 3
   }
   /// <summary>Specify which non-value fields to copy (and conversely, which to omit).</summary>
   [Flags] public enum WhichNonValueFields {
      /// <summary>0 - Copy no non-value fields.</summary>
      None               = 0,                               // 0
      /// <summary>(1) Copy only the Struture field, leaving Rank and Superior uninitialized.</summary>
      Structure          = 1,                               // 1
      /// <summary>(2) Copy only the Rank field, leaving Structure and Superior uninitialized..</summary>
      Rank               = 1 << 1,                          // 2
      /// <summary>(3) Copy only the Superior field, levaing Rank and Structure uninitialized.</summary>
      Superior           = 1 << 2,                          // 3
      /// <summary>(4) Copy all non-value fields: Structure, Rank and Superior.</summary>
      All                = Structure | Rank | Superior,     // 4
      /// <summary>(5) Copy the Rank and Structure fields, but leave Superior uninitialized.</summary>
      AllExceptSuperior  = All & ~Superior                  // 5
   }
   /// <summary>Specifies whether to make.</summary>
   [Flags] public enum HowToCopyStructure {
      /// <summary>(0) Do not copy structure.</summary>
      DoNotCopy = 0,
      /// <summary>(1) Copy only the reference to the existing structure on the original tensor.</summary>
      ReferToOriginalStructure  = 1,            // 1
      /// <summary>(2) Create a fresh copy of the structure on the original tensor and assign its reference to the field.</summary>
      CreateNewStructure = 1 << 1,              // 2
   }
}

}