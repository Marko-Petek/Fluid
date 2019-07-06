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
using SCG = System.Collections.Generic;
using TB = Fluid.Internals.Toolbox;
using static Fluid.Internals.Numerics.MatOps;
using Fluid.Internals.Numerics;
using Fluid.TestRef;

//FIXME: Check what scalar multiplication has broken.

namespace Fluid.Internals.Collections {
   using IA = IntArithmetic;
   /// <summary>A tensor with specified rank and specified dimension which holds direct subordinates of type τ.</summary>
   /// <typeparam name="τ">Type of direct subordinates.</typeparam>
   public class Tensor<τ,α> : TensorBase<Tensor<τ,α>>, IEquatable<Tensor<τ,α>>
   where τ : IEquatable<τ>, IComparable<τ>, new()
   where α : IArithmetic<τ>, new() {
      /// <summary>Hierarchy's dimensional structure. First element specifies host's (tensor highest in hierarchy) rank, while last element specifies the rank of values. E.g.: {3,2,6,5} specifies structure of a tensor of 4th rank with first rank equal to 5 and last rank to 3.</summary>
      public int[] Structure { get; protected set; }
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
      internal Tensor(int[] structure, int rank, Tensor<τ,α> sup, int cap) : base(cap) {
         Structure = structure ?? null;
         Rank = rank;
         Superior = sup ?? null;
      }
      /// <summary>Creates a top tensor with specified structure and initial capacity. Rank is assigned as the length of structure array.</summary>
      /// <param name="structure">Specifies dimension of each rank.</param>
      /// <param name="cap">Initially assigned memory.</param>
      public Tensor(int[] structure, int cap = 6) : this(structure, structure.Length, null, cap) { }
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
      /// <summary>Adds tnr to caller and assigns to it the caller's superstructure.</summary>
      /// <param name="key">Index.</param>
      /// <param name="tnr">Tensor to add.</param>
      new public void Add(int key, Tensor<τ,α> tnr) {
         tnr.Superior = this;
         tnr.Structure = Structure;
         base.Add(key, tnr);
      }
      /// <summary>Adds tnr to caller, but leaves its superstructure untouched.</summary>
      /// <param name="key">Index.</param>
      /// <param name="tnr">Tensor to add.</param>
      protected void AddOnly(int key, Tensor<τ,α> tnr) =>
         base.Add(key, tnr);

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
      /// <param name="src">Copy source.</param>
      /// <param name="tgt">Copy target.</param>
      /// <param name="cs">Exact specification of what fields to copy. Default is all.</param>
      public static void Copy(in Tensor<τ,α> src, Tensor<τ,α> tgt, in CopySpecStruct cs) {
         TB.Assert.True(src.Rank > 1,
            "Tensors's rank has to be at least 2 to be copied via this method.");
         CopyMetaFields(src, tgt, in cs.NonValueFieldsSpec, in cs.StructureSpec);
         if((cs.FieldsSpec & WhichFields.OnlyValues) == WhichFields.OnlyValues) {
            int endRank = cs.EndRank;
            Recursion(src, tgt);

            void Recursion(Tensor<τ,α> src1, Tensor<τ,α> tgt1) {
            if(src1.Rank > 2) {                                       // Subordinates are tensors.
               foreach (var kv in src1) {
                  var tnr = new Tensor<τ,α>(kv.Value.Structure, kv.Value.Rank, kv.Value.Superior, kv.Value.Count);
                  tnr.Rank = tgt1.Rank - 1;
                  tgt1.Add(kv.Key, tnr);
                  if(src1.Rank > endRank)
                     Recursion(kv.Value, tnr); } }
            else if(src1.Rank == 2) {                                 // Subordinates are vectors.
               foreach(var kv in src1) {
                  var valAsVec = (Vector<τ,α>) kv.Value;
                  var vecCopy = new Vector<τ,α>();
                  tgt1.Add(kv.Key, vecCopy);
                  vecCopy.Vals = new SCG.Dictionary<int,τ>(valAsVec.Vals);
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
            else {
               tgt.Structure = new int[src.Structure.Length];
               Array.Copy(src.Structure, tgt.Structure, src.Structure.Length); } }
         if((mcs & WhichNonValueFields.Rank) == WhichNonValueFields.Rank)
            tgt.Rank = src.Rank;
         if((mcs & WhichNonValueFields.Superior) == WhichNonValueFields.Superior)
            tgt.Superior = src.Superior ?? null;
      }
      /// <summary>Packs only the part of the Structure below this tensor into a new Structure.</summary>
      protected int[] CopySubstructure() {
         int structInx = Structure.Length - Rank;           // TopRank - Rank --> Index where substructure begins.
         return Structure.Skip(structInx).ToArray();
      }
      /// <summary>Creates a tensor with specified structure from values provided within a Span.</summary>
      /// <param name="slice">Span of values.</param>
      /// <param name="structure">Structure of new tensor.</param>
      public static Tensor<τ,α> FromFlatSpec(Span<τ> slice, params int[] structure) {
         int tnrRank = structure.Length;
         if(tnrRank == 1)
            return Vector<τ,α>.CreateFromFlatSpec(slice);
         else
            return Recursion(slice, 0);

         Tensor<τ,α> Recursion(Span<τ> slc, int dim) {       // Specifiy slice and the structure dimension (natural rank index) to which it belongs.
            int trueRank = ChangeRankNotation(structure.Length, dim);
            int nIter = structure[dim];                              // As many iterations as it is the size of the dimension.
            int nEmtsInSlice = slc.Length / nIter;
            if(trueRank > 1) {
               var res = new Tensor<τ,α>(structure, trueRank, null, structure[dim]);
               for(int i = 0; i < nIter; ++i) {                      // Over each tensor. Create new slices and run recursion on them.
                  var newSlc = slc.Slice(i*nEmtsInSlice, nEmtsInSlice);
                  var newTnr = Recursion(newSlc, dim + 1);
                  if(newTnr.Count != 0)
                     res.Add(i, newTnr); }
               return res; }
            else                                                  // We are at rank 1 = vector rank.
               return Vector<τ,α>.CreateFromFlatSpec(slc);
         }
      }
      /// <summary>Create an empty top rank (unassigned superior) tensor with specified structure.</summary>
      /// <param name="cap">Capacity.</param>
      /// <param name="structure">Structure whose reference will be absorbed into the new tensor.</param>
      public static Tensor<τ,α> CreateEmpty(int cap, params int[] structure) {
         int tnrRank = structure.Length;
         if(tnrRank == 1)
            return Vector<τ,α>.CreateEmpty(cap, structure);
         else
            return new Tensor<τ,α>(structure, tnrRank, null, cap);
      }
      /// <summary>Transforms from slot index (in the order written by hand, e.g. A^ijk ==> 1,2,3) to rank index (as situated in the hierarchy, e.g. A^ijk ==> 2,1,0).</summary>
      /// <param name="rankInx">Rank index as situated in the hierarchy. Higher number equates to being higher in the hierarchy.</param>
      int ToSlotInx(int rankInx) =>
         ChangeRankNotation(Structure.Length, rankInx);
      /// <summary>Transforms from rank index (as situated in the hierarchy, e.g. A^ijk ==> 2,1,0) to slot index (in the order written by hand, e.g. A^ijk ==> 1,2,3).</summary>
      /// <param name="slotInx">Slot index.</param>
      /// <remarks>Implementation is actually identical to the one in the ToNaturalInx method.</remarks>
      int ToRankInx(int slotInx) =>
         ChangeRankNotation(Structure.Length, slotInx);
      
      /// <summary>A method that transorms between slot index and rank index (works both ways).</summary>
      /// <param name="topRankInx">Rank of the top-most tensor in the hierarchy.</param>
      /// <param name="slotOrRankInx">Slot or rank index.</param>
      static int ChangeRankNotation(int topRankInx, int slotOrRankInx) =>
         topRankInx - slotOrRankInx;
      /// <summary>Tensor getting/setting indexer.</summary>
      /// <param name="overloadDummy">Type uint of first dummy argument specifies that we know we will be getting/setting a Tensor.</param>
      /// <param name="inxs">A set of indices specifiying which Tensor we want to set/get. The set length must not reach all the way out to scalar rank.</param>
      public Tensor<τ,α> this[uint overloadDummy, params int[] inxs] {
         get {
            Tensor<τ,α> tnr = this;
            for(int i = 0; i < inxs.Length; ++i) {
               if(!TryGetValue(inxs[i], out tnr))
                  return null; }
            return tnr; }                                // No problem with tnr being null. We return above.
         set {
            Tensor<τ,α> tnr = this;
            if(value != null) {
               int n = inxs.Length - 1;
               for(int i = 0; i < n; ++i) {
                  if(!tnr.TryGetValue(inxs[i], out tnr))
                     tnr = new Tensor<τ,α>(Structure, tnr.Rank - 1, tnr, 6);
                     tnr.Superior.Add(inxs[i], tnr); }
               var dict = (TensorBase<Tensor<τ,α>>) tnr;
               dict[inxs[n]] = value; }
            else {
               for(int i = 0; i < inxs.Length; ++i) {
                  if(!tnr.TryGetValue(inxs[i], out tnr))
                     return; }
               tnr.Superior.Remove(inxs[inxs.Length - 1]); } }
      }
      /// <summary>Vector getting/setting indexer.</summary>
      /// <param name="overloadDummy">Type float of first dummy argument specifies that we know we will be getting/setting a Vector.</param>
      /// <param name="inxs">A set of indices specifiying which Vector we want to set/get. The set length must reach exactly to Vector rank.</param>
      public Vector<τ,α> this[float overloadDummy, params int[] inx] {
         get {
            Tensor<τ,α> tnr = this;
            int n = inx.Length - 1;
            for(int i = 0; i < n; ++i) {
               if(!tnr.TryGetValue(inx[i], out tnr))
                  return null; }
            if(tnr.TryGetValue(inx[n], out tnr))         // No probelm with null.
               return (Vector<τ,α>)tnr;                  // Same.
            else
               return null; }
         set {
            Tensor<τ,α> tnr = this;
            if(value != null) {
               int n = inx.Length - 1;                      // One before last chooses tensor, last chooses vector.
               for(int i = 0; i < n; ++i) {
                  if(tnr.TryGetValue(inx[i], out Tensor<τ,α> tnr2)) {
                     tnr = tnr2; }
                  else {
                     tnr = new Tensor<τ,α>(Structure, tnr.Rank - 1, tnr, 6);
                     tnr.Superior.Add(inx[i], tnr); }}
               var dict = (TensorBase<Tensor<τ,α>>) tnr;                      // We do not check that it is a vector beforehand. It is assumed that the user used indexer correctly.
               dict[inx[n]] = value; }
            else {
               int n = inx.Length;                 // Last chooses vector.
               for(int i = 0; i < n; ++i) {
                  if(!tnr.TryGetValue(inx[i], out tnr))
                     return; }
               tnr.Superior.Remove(inx[n - 1]); } }     // Vector.Superior.Remove
      }
      /// <summary>Scalar getting/setting indexer.</summary>
      /// <param name="inxs">A set of indices specifiying which scalar we want to set/get. The set length must reach exactly to scalar rank.</param>
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
                        tnr.Superior.Add(inx[i], tnr); }}
                  if(tnr.TryGetValue(inx[n], out tnr2)) {                           // Does vector exist?
                     vec = (Vector<τ,α>) tnr2; }
                  else {
                     vec = new Vector<τ,α>(Structure, tnr, 4); 
                     tnr.Add(inx[n], vec); }
                  vec.Vals[inx[n + 1]] = value; } }
            else {
               int n = inx.Length - 1;
               for(int i = 0; i < n; ++i) {
                  if(!tnr.TryGetValue(inx[i], out tnr))
                     return; }
               tnr.Remove(inx[n]); } }
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
      /// <summary>Each subtensor of visitSrc is visited, then its existence is verified in tgt. If it does not exist in tgt then the whole subtensor is simply copied from visitSrc and added to tgt. Otherwise, the whole process repeats for the subtensor. When stopRank is reached the action is performed.</summary>
      /// <param name="recurseSrc"></param>
      /// <param name="recurseTgt"></param>
      /// <param name="inclStopRank"></param>
      /// <param name="onNonExistEquivalent"></param>
      /// <param name="onStopRank"></param>
      protected static void SimultaneousRecurse(
         Tensor<τ,α> recurseSrc, Tensor<τ,α> recurseTgt, int inclStopRank,
         Action<Tensor<τ,α>, Tensor<τ,α>, int>  onNonExistEquivalent,
         Action<Tensor<τ,α>, Tensor<τ,α>>       onStopRank)
      {
         TB.Assert.True(inclStopRank > 1, "InclusiveStopRank has to be at least 2, because method deals exclusively with tensors.");
         VisitThenAct(recurseSrc, recurseTgt);

         void VisitThenAct(Tensor<τ,α> aVisitSrc, Tensor<τ,α> aTgt) {
            if(aVisitSrc.Rank > inclStopRank) {
               foreach(var int_aSubVisitSrc in aVisitSrc) {
                  if(aTgt.TryGetValue(int_aSubVisitSrc.Key, out var aSubTgt)) {     // Subtensor exists in aTgt.
                     VisitThenAct(int_aSubVisitSrc.Value, aSubTgt); }
                  else                                                              // Subtensor does not exist 
                     onNonExistEquivalent?.Invoke(aVisitSrc, aTgt,
                        int_aSubVisitSrc.Key); } }
            else                                                                    // inclusive StopRank reached.
               onStopRank?.Invoke(aVisitSrc, aTgt);
         }
      }

      /// <summary>Each subtensor of visitSrc is visited, it is assumed that the corresponding tensor does not exist in visitTgt. If it does not exist in tgt then the whole subtensor is simply copied from visitSrc and added to tgt. Otherwise, the whole process repeats for the subtensor. When stopRank is reached the action is performed.</summary>
      /// <param name="visitSrc"></param>
      /// <param name="visitTgt"></param>
      /// <param name="inclusiveStopRank"></param>
      /// <param name="onStopRank"></param>
      protected static void RecursiveVisitEmptyTgt(Tensor<τ,α> visitSrc, Tensor<τ,α> visitTgt,
      int inclusiveStopRank, Func<Tensor<τ,α>,Tensor<τ,α>,int,Tensor<τ,α>> onVisit,
      Action<Tensor<τ,α>,Tensor<τ,α>> onStopRank) {
         TB.Assert.True(inclusiveStopRank > 1, "InclusiveStopRank has to be at least 2, because method deals exclusively with tensors.");
         VisitThenAct(visitSrc, visitTgt);

         void VisitThenAct(Tensor<τ,α> src, Tensor<τ,α> tgt) {
            if(src.Rank > inclusiveStopRank) {
               foreach(var int_aSubVisitSrc in src) {
                  var subTgt = onVisit.Invoke(int_aSubVisitSrc.Value, tgt,
                     int_aSubVisitSrc.Key);                                      // Careful: source subtensor, target tensor 1 rank higher.
                  VisitThenAct(int_aSubVisitSrc.Value, subTgt); } }
            else                                                                    // inclusive StopRank reached.
               onStopRank?.Invoke(src, tgt);
         }
      }
      /// <summary>Creates a new tensor which is a negation of tnr1. The tensor is created as top rank, given its own substructure and no superstructure.</summary>
      /// <param name="tnr1">Operand.</param>
      /// <remarks><see cref="TestRefs.Op_TensorNegation"/></remarks>
      public static Tensor<τ,α> operator -(Tensor<τ,α> tnr1) {
         int[] newStructure = tnr1.CopySubstructure();
         var res = new Tensor<τ,α>(newStructure, tnr1.Rank, null, tnr1.Count);
         RecursiveVisitEmptyTgt(tnr1, res, 2,
            onVisit: (subSrc, tgt, inx) => {
               var subTgt = new Tensor<τ,α>(subSrc.Rank, subSrc.Count);
               tgt.Add(inx, subTgt);
               return subTgt; },
            onStopRank: (srcR2, tgtR2) => {
               foreach(var int_srcR1 in srcR2) {
                  tgtR2.Add(int_srcR1.Key, - ((Vector<τ,α>) int_srcR1.Value)); } });
         return res;
      }
      /// <summary>Creates a new tensor which is a sum of the two operands. The tensor is created as top rank, given its own substructure and no superstructure.</summary>
      /// <param name="tnr1">Left operand.</param>
      /// <param name="tnr2">Right operand.</param>
      /// <remarks> <see cref="TestRefs.Op_TensorAddition"/> </remarks>
      public static Tensor<τ,α> operator + (Tensor<τ,α> tnr1, Tensor<τ,α> tnr2) {
         ThrowOnSubstructureMismatch(tnr1, tnr2);
         int[] newStructure = tnr1.CopySubstructure();
         var res = new Tensor<τ,α>(tnr2, CopySpecs.S322_04);          // Create a copy of second tensor.
         res.Structure = newStructure;                               // Assign to it a new structure.
         SimultaneousRecurse(tnr1, res, 2,
            onNonExistEquivalent: (supTnr1, supTnr2, key) => {    // Simply copy and add.
               var srcCopy = new Tensor<τ,α>(((TensorBase<Tensor<τ,α>>) supTnr1)[key],
                  in CopySpecs.S322_04);
                  supTnr2.Add(key, srcCopy); },
            onStopRank: (stopTnr1R2, stopTnr2R2) => {                    // stopTnrs are rank 2.
               foreach(var int_tnr1R1 in stopTnr1R2) {
                  if(stopTnr2R2.TryGetValue(int_tnr1R1.Key, out var tnr2R1)) {         // Same subtensor exists in target. Sum them and overwrite the one on target.
                     var sum = (Vector<τ,α>) int_tnr1R1.Value + (Vector<τ,α>) tnr2R1;        // FIXME: Careful! Must use method that preserves structure, or reassign structure a line below.
                     stopTnr2R2[1f, int_tnr1R1.Key] = sum; }
                  else {                                                               // Same subtensor does not exist on target. Copy branch from source.
                     var srcCopy = new Vector<τ,α>((Vector<τ,α>) int_tnr1R1.Value,
                        in CopySpecs.S322_04);
                     stopTnr2R2.Add(int_tnr1R1.Key, srcCopy); } } } );
         return res;
      }
      /// <summary>Creates a new tensor which is a difference of the two operands. The tensor is created as top rank, given its own substructure and no superstructure.</summary>
      /// <param name="tnr1">Left operand.</param>
      /// <param name="tnr2">Right operand.</param>
      /// <remarks> <see cref="TestRefs.Op_TensorSubtraction"/> </remarks>
      public static Tensor<τ,α> operator - (Tensor<τ,α> tnr1, Tensor<τ,α> tnr2) {
         ThrowOnSubstructureMismatch(tnr1, tnr2);
         int[] newStructure = tnr1.CopySubstructure();
         var res = new Tensor<τ,α>(tnr1, CopySpecs.S322_04);          // Create a copy of first tensor.
         res.Structure = newStructure;                               // Assign to it a new structure.
         SimultaneousRecurse(tnr2, res, 2,                     // Source now is tnr2, opposed to + operator where it was tnr1.
            onNonExistEquivalent: (supTnr2, supTnr1, key) => {    // Simply copy, negate and add. Could be done more optimally. BUt I'll leave it like that for now, because it's simpler.
               var srcCopy = new Tensor<τ,α>(((TensorBase<Tensor<τ,α>>) supTnr2)[key],
                  in CopySpecs.S322_04);                                                  // FIXME: Is this bad?
                  srcCopy.Negate();
                  supTnr1.Add(key, srcCopy); },
            onStopRank: (stopTnr2R2, stopTnr1R2) => {                    // stopTnrs are rank 2.
               foreach(var int_tnr2R1 in stopTnr2R2) {
                  if(stopTnr1R2.TryGetValue(int_tnr2R1.Key, out var tnr1R1)) {         // Same subtensor exists in target. Sum them and overwrite the one on target.
                     var dif = (Vector<τ,α>) tnr1R1 - (Vector<τ,α>) int_tnr2R1.Value;
                     stopTnr1R2[1f, int_tnr2R1.Key] = dif; }
                  else {                                                               // Same subtensor does not exist on target. Copy branch from source and negate it.
                     var srcCopy = new Vector<τ,α>((Vector<τ,α>) int_tnr2R1.Value,
                        in CopySpecs.S322_04);
                        srcCopy.Negate();
                     stopTnr1R2.Add(int_tnr2R1.Key, srcCopy); } } } );
         return res;
      }
      /// <summary>Sums tnr2 into the caller. Don't use the tnr2 reference afterwards.</summary>
      /// <param name="tnr2">Sumand whose elements will be absorbed into the caller and shouldn't be used afterwards.</param>
      /// <remarks> <see cref="TestRefs.TensorAdd"/> </remarks>
      public void Sum(Tensor<τ,α> tnr2) {
         Tensor<τ,α> tnr1 = this;
         ThrowOnSubstructureMismatch(tnr1, tnr2);
         Recursion(tnr1, tnr2);

         void Recursion(Tensor<τ,α> t1, Tensor<τ,α> t2) {
            if(t2.Rank > 2) {
               foreach(var int_subTnr2 in t2) {
                  if(t1.TryGetValue(int_subTnr2.Key, out var subTnr1))            // Equivalent subtensor exists in T1.
                     Recursion(subTnr1, int_subTnr2.Value);
                  else                                                      // Equivalent subtensor does not exist in T1. Absorb the subtensor from T2 and add it.
                     t1.Add(int_subTnr2.Key, int_subTnr2.Value); } }
            else if(t2.Rank == 2) {
               foreach(var int_subTnr2 in t2) {
                  //var vec2 = (Vector<τ,α>) int_subTnr2.Value;
                  if(t1.TryGetValue(int_subTnr2.Key, out var subTnr1)) {      // Entry exists in t1, we must sum.
                     var vec1 = (Vector<τ,α>) subTnr1;
                     var vec2 = (Vector<τ,α>) int_subTnr2.Value;
                     vec1.Sum(vec2); }
                  else {
                     t1.Add(int_subTnr2.Key, int_subTnr2.Value); } } }          // Entry does not exist in t2, simply Add.
            else {                                                            // We have a vector.
               var vec1 = (Vector<τ,α>) t1;
               var vec2 = (Vector<τ,α>) t2;
               t1.Sum(t2);
            }
         }
      }
      /// <summary>Subtracts tnr2 from the caller. Tnr2 is still usable afterwards.</summary>
      /// <param name="tnr2">Minuend which will be subtracted from the caller. Minuend is still usable after the operation.</param>
      public void Sub(Tensor<τ,α> tnr2) { // TODO: Test Sub method and implement it for vector and test it.
         Tensor<τ,α> tnr1 = this;
         ThrowOnSubstructureMismatch(tnr1, tnr2);

         void Recursion(Tensor<τ,α> t1, Tensor<τ,α> t2) {
            if(t2.Rank > 2) {
               foreach(var int_subT2 in t2) {
                  if(t1.TryGetValue(int_subT2.Key, out var subT1))            // Equivalent subtensor exists in T1.
                     Recursion(subT1, int_subT2.Value);
                  else                                                      // Equivalent subtensor does not exist in T1. Negate the subtensor from T2 and add it.
                     t1.Add(int_subT2.Key, -int_subT2.Value); } }
            else if(t2.Rank == 2) {
               foreach(var int_subTnr2 in t2) {
                  //var vec2 = (Vector<τ,α>) int_subTnr2.Value;
                  if(t1.TryGetValue(int_subTnr2.Key, out var subTnr1)) {      // Entry exists in t1, we must sum.
                     var vec1 = (Vector<τ,α>) subTnr1;
                     var vec2 = (Vector<τ,α>) int_subTnr2.Value;
                     vec1.Sum(vec2); }
                  else {
                     t1.Add(int_subTnr2.Key, int_subTnr2.Value); } } }          // Entry does not exist in t2, simply Add.
            else {                                                            // We have a vector.
               var vec1 = (Vector<τ,α>) t1;
               var vec2 = (Vector<τ,α>) t2;
               t1.Sum(t2);
            }
         }
      }
      /// <summary>Creates a new tensor which is a product of a scalar and a tensor. The tensor is created as top rank, given its own substructure and no superstructure.</summary>
      /// <param name="scal">Scalar.</param>
      /// <param name="tnr">Tensor.</param>
      /// <remarks> <see cref="TestRefs.Op_ScalarTensorMultiplication"/> </remarks>
      public static Tensor<τ,α> operator * (τ scal, Tensor<τ,α> tnr) {                             // FIXME: Check all references
         int[] newStructure = tnr.CopySubstructure();                                              // New substructure.
         return Recursion(tnr);

         Tensor<τ,α> Recursion(in Tensor<τ,α> src) {
            var res = new Tensor<τ,α>(newStructure, tnr.Rank, null, tnr.Count);
            if(src.Rank > 2) {                                       // Subordinates are tensors.
               foreach (var kv in src) {
                  var subTnr = Recursion(kv.Value);
                  res.Add(kv.Key, subTnr); } }
            else if(src.Rank == 2) {                                 // Subordinates are vectors.
               foreach (var kv in src)
                  res.Add(kv.Key, scal*((Vector<τ,α>) kv.Value)); }
            else
               return scal*((Vector<τ,α>) src);
            return res;
         }
      }
      /// <summary>Calculates tensor product of this tensor (left-hand operand) with another tensor (right-hand operand).</summary>
      /// <param name="tnr2">Right-hand operand.</param>
      /// <remarks> <see cref="TestRefs.TensorProduct"/> </remarks>
      public virtual Tensor<τ,α> TnrProduct(Tensor<τ,α> tnr2) {                     // TODO: Make sure this work for all combinations: (tnr,tnr), (tnr,vec), (vec,tnr), (vec,vec)
         // Overriden on vector when first operand is a vector.
         // 1) Descend to rank 1 through a recursion and then delete that vector.
         // 2) Substitute it with a tensor of rank tnr2.Rank + 1 whose entries are tnr2s multiplied by the corresponding scalar that used to preside there in the old vector.
         int newRank = Rank + tnr2.Rank;
         var newStructure = Structure.Concat(tnr2.Structure).ToArray();
         return Recursion(this, newRank);

         Tensor<τ,α> Recursion(Tensor<τ,α> src, int resRank) {
            var res = new Tensor<τ,α>(newStructure, resRank, null, src.Count);
            if(src.Rank > 2) {                                                    // Only copy. Adjust ranks.
               foreach(var int_tnr in src)
                  res.Add(int_tnr.Key, Recursion(int_tnr.Value, resRank - 1)); }
            else {                              // We are now at tensor which contains vectors.
               foreach(var int_vec in src) {    // Substitute each vector wiht a new tensor.
                  var vec = (Vector<τ,α>) int_vec.Value;
                  var subTnr = new Tensor<τ,α>(newStructure, resRank - 1, null, src.Count);
                  foreach(var int_val in vec.Vals)
                     subTnr.Add(int_val.Key, TensorExtensions<τ,α>.Mul(int_val.Value, tnr2, subTnr)); // ScalMul so that the correct superior propagares downwards.
                  res.Add(int_vec.Key, subTnr); } }
            return res;
         }
      }
      /// <summary>Eliminates a single rank out of a tensor by choosing a single subtensor at that rank and making it take the place of its direct superior (thus discarding all other subtensors at that rank). The resulting tensor has therefore its rank reduced by one.</summary>
      /// <param name="elimRank"> True, zero-based rank index of rank to be eliminated.</param>
      /// <param name="emtInx">Zero-based element index in that rank in favor of which the elimination will take place.</param>
      /// <remarks><see cref="TestRefs.TensorReduceRank"/></remarks>
      public Tensor<τ,α> ReduceRank(int elimRank, int emtInx) {
         TB.Assert.True(elimRank < Rank && elimRank > -1, "You can only eliminate a non-negative rank greater than or equal to top rank.");
         var newStructureL = Structure.Take(elimRank);
         var newStructureR = Structure.Skip(elimRank + 1);
         var newStructure = newStructureL.Concat(newStructureR).ToArray();    // Created a new structure. Assign it to new host tensor.

         if(elimRank == Rank - 1) {                                                    // 1 rank exists above elimRank == Pick one from elimRank and return it.
            if(Rank > 1) {                                                             // Result is Tensor, top tensor at least rank 2.
               if(TryGetValue(emtInx, out var subTnr)) {                                   // Sought after subordinate exists.
                  var newTnr = subTnr.Copy(in CopySpecs.S322_04);                          // Works properly even for Vector.
                  newTnr.Structure = newStructure;
                  return newTnr;}
               else
                  return CreateEmpty(0, newStructure); }                                // Return empty tensor.
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
         TB.Assert.True(src.Rank == 2, "This method is intended for rank 2 tensors only.");
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
         TB.Assert.True(src.Rank > 2, "This method is applicable to rank 3 and higher tensors.");
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

      protected (int[] struc, int rankInx1, int rankInx2, int conDim) ContractPart1(
      Tensor<τ,α> tnr2, int slotInx1, int slotInx2) {
         // 1) First eliminate, creating new tensors. Then add them together using tensor product.
         int[] struc1 = Structure, 
               struc2 = tnr2.Structure;
         int rank1 = struc1.Length,
             rank2 = struc2.Length;
         TB.Assert.True(rank1 == Rank && rank2 == tnr2.Rank,
            "One of the tensors is not top rank.");
         TB.Assert.AreEqual(struc1[slotInx1 - 1], struc2[slotInx2 - 1],              // Check that the dimensions of contracted ranks are equal.
            "Rank dimensions at specified indices must be equal.");
         int   conDim = Structure[slotInx1 - 1],                                // Dimension of rank we're contracting.
               rankInx1 = ToRankInx(slotInx1),
               rankInx2 = tnr2.ToRankInx(slotInx2);
         var struc3_1 = struc1.Where((emt, i) => i != (slotInx1 - 1));
         var struc3_2 = struc2.Where((emt, i) => i != (slotInx2 - 1));
         var struc3 = struc3_1.Concat(struc3_2).ToArray();                 // New structure.
         return (struc3, rankInx1, rankInx2, conDim);
      }
      
      public Tensor<τ,α> ContractPart2(Tensor<τ,α> tnr2, int rankInx1, int rankInx2, int[] struc3, int conDim) {
         // 1) First eliminate, creating new tensors. Then add them together using tensor product.
         if(Rank > 1) {
            if(tnr2.Rank > 1) {                                // First tensor is rank 2 or more.
               Tensor<τ,α> elimTnr1, elimTnr2, sumand, sum;
               sum = new Tensor<τ,α>(struc3);                                    // Set sum to a zero tensor.
               for(int i = 0; i < conDim; ++i) {
                  elimTnr1 = ReduceRank(rankInx1, i);
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
               if(Rank == 2) {                                    // Result will be vector.
                  Vector<τ,α> elimVec, sumand, sum;
                  sum = new Vector<τ,α>(struc3, null, 4);
                  for(int i = 0; i < conDim; ++i) {
                     elimVec = (Vector<τ,α>) ReduceRank(rankInx1, i);
                     if(elimVec.Count != 0 && vec.Vals.TryGetValue(i, out var val)) {
                        sumand = val*elimVec;
                        sum.Sum(sumand); } }
                  if(sum.Vals.Count != 0)
                     return sum;
                  else
                     return CreateEmpty(0, struc3); }
               else {                                             // Result will be tensor.
                  Tensor<τ,α> elimTnr1, sumand, sum;
                  sum = new Tensor<τ,α>(struc3);
                  for(int i = 0; i < conDim; ++i) {
                     elimTnr1 = ReduceRank(rankInx1, i);
                     if(elimTnr1.Count != 0 && vec.Vals.TryGetValue(i, out var val)) {
                        sumand = val*elimTnr1;
                        sum.Sum(sumand); } }
                  if(sum.Count != 0)
                     return sum;
                  else
                     return CreateEmpty(0, struc3); } } }
         else {                                                   // First tensor is rank 1 (a vector).
            var vec1 = (Vector<τ,α>) this;
            return vec1.ContractPart2(tnr2, rankInx2, struc3, conDim);}
      }

      /// <summary>Contracts two tensors over specified natural rank indices. Example: Contraction writen as A^(ijkl)B^(mnip) is specified as a (0,2) contraction of A and B, not a (3,1) contraction.</summary>
      /// <param name="tnr2">Tensor 2.</param>
      /// <param name="slotInx1">One-based natural index on this tensor over which to contract.</param>
      /// <param name="slotInx2">One-based natural index on tensor 2 over which to contract (it must hold: dim(rank(inx1)) = dim(rank(inx2)).</param>
      /// <remarks>Tensor contraction is a generalization of trace, which can further be viewed as a generalization of dot product.</remarks>
      public Tensor<τ,α> Contract(Tensor<τ,α> tnr2, int slotInx1, int slotInx2) {
         (int[] struc3, int rankInx1, int rankInx2, int conDim) = ContractPart1(tnr2, slotInx1, slotInx2);
         return ContractPart2(tnr2, rankInx1, rankInx2, struc3, conDim);
      }
      /// <summary>Contracts across the two slot indices on a rank 2 tensor.</summary>
      /// <remarks> <see cref="TestRefs.TensorSelfContractR2"/> </remarks>
      public τ SelfContractR2() {
         TB.Assert.True(Rank == 2, "Tensor rank has to be 2 for this method.");
         TB.Assert.True(Structure[0] == Structure[1], "Corresponding dimensions have to be equal.");
         τ result = default;
         foreach(var int_vec in this) {
            var vec = (Vector<τ,α>) int_vec.Value;
            if(vec.Vals.TryGetValue(int_vec.Key, out τ val))
               result = O<τ,α>.A.Sum(result, val); }
         return result;
      }

      public Vector<τ,α> SelfContractR3(int natInx1, int natInx2) {
         TB.Assert.True(Rank == 3, "Tensor rank has to be 3 for this method.");
         TB.Assert.True(Structure[natInx1 - 1] == Structure[natInx2 - 1],
            "Corresponding dimensions have to be equal.");
         Vector<τ,α> res = new Vector<τ,α>(new int[] {Structure[2]}, null, 4);
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
         TB.Assert.True(Rank > 2, "This method is not applicable to rank 2 tensors.");
         TB.Assert.True(Structure[slotInx1 - 1] == Structure[slotInx2 - 1],
            "Dimensions of contracted slots have to be equal.");
         if(Rank > 3) {
            var newStruct1 = Structure.Take(slotInx1 - 1);
            var newStruct2 = Structure.Take(slotInx2 - 1).Skip(slotInx1);
            var newStruct3 = Structure.Skip(slotInx2);
            var newStruct = newStruct1.Concat(newStruct2).Concat(newStruct3).ToArray();
            var res = new Tensor<τ,α>(newStruct, Rank - 2, null, Count);
            var rankInx1 = ToRankInx(slotInx1);
            var rankInx2 = ToRankInx(slotInx2);
            int dimRank = Structure[slotInx1 - 1];                // Dimension of contracted rank.
            for(int i = 0; i < dimRank; ++i) {                    // Over each element inside contracted ranks.
               var step1Tnr = ReduceRank(rankInx2, i);
               var sumand = step1Tnr.ReduceRank(rankInx1 - 1, i);
               res.Sum(sumand); }
            return res;
         }
         else
            return SelfContractR3(slotInx1, slotInx2);
      }
      /// <summary>Compares substructures of two equal rank tensors and throws an exception if they mismatch.</summary>
      /// <param name="tnr1">First tensor.</param>
      /// <param name="tnr2">Second tensor.</param>
      public static void ThrowOnSubstructureMismatch(Tensor<τ,α> tnr1, Tensor<τ,α> tnr2) {
         TB.Assert.True(tnr1.Rank == tnr2.Rank);                                    // First, ranks must match.
         int topRank1 = tnr1.Structure.Length;                                      // We have to check that all dimensions below current ranks match.
         int topRank2 = tnr2.Structure.Length;
         var structInx1 = topRank1 - tnr1.Rank;                         // Index in structure array.
         var structInx2 = topRank2 - tnr2.Rank;
         for(int i = structInx1, j = structInx2; i < topRank1; ++i, ++j) {
            if(tnr1.Structure[i] != tnr2.Structure[j])
               throw new InvalidOperationException("Tensor addition: structures do not match."); }
      }

      public SCG.IEnumerable<Tensor<τ,α>> EnumerateRank(int rankInx) {
         TB.Assert.True(rankInx > 1, "This method applies only to ranks that hold pure tensors.");
         if(Rank > rankInx + 1) {
            foreach(var subTnr in Recursion(this))
               yield return subTnr; }
         else {
            foreach(var int_subTnr in this)
               yield return int_subTnr.Value; }

         SCG.IEnumerable<Tensor<τ,α>> Recursion(Tensor<τ,α> src) {
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
                  var sub2 = sup2[1u, inx_sub1.Key];
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
               var vec2 = sup2R2[1f, inx_sub1R1.Key];
               if(!vec1.Equals(vec2))
                  return false; }
            return true;
         }
      }

      public bool Equals(Tensor<τ,α> tnr2, τ eps) {
         ThrowOnSubstructureMismatch(this, tnr2);
         return TnrRecursion(this, tnr2);

         bool TnrRecursion(Tensor<τ,α> sup1, Tensor<τ,α> sup2) {
            if(!sup1.Keys.OrderBy(key => key).SequenceEqual(sup2.Keys.OrderBy(key => key)))
               return false;                                                                    // Keys have to match. This is crucial.
            if(sup1.Rank > 2) {
               foreach(var inx_sub1 in sup1) {
                  var sub2 = sup2[1u, inx_sub1.Key];
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
               var vec2 = sup2R2[1f, inx_sub1R1.Key];
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
         public static readonly CopySpecStruct S342_00 = new CopySpecStruct();
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