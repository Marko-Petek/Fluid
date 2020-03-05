/*  
 ██████╗  █████╗ ███╗   ██╗██╗  ██╗    ██╗███╗   ██╗██████╗ ██╗ ██████╗███████╗███████╗
 ██╔══██╗██╔══██╗████╗  ██║██║ ██╔╝    ██║████╗  ██║██╔══██╗██║██╔════╝██╔════╝██╔════╝
 ██████╔╝███████║██╔██╗ ██║█████╔╝     ██║██╔██╗ ██║██║  ██║██║██║     █████╗  ███████╗
 ██╔══██╗██╔══██║██║╚██╗██║██╔═██╗     ██║██║╚██╗██║██║  ██║██║██║     ██╔══╝  ╚════██║
 ██║  ██║██║  ██║██║ ╚████║██║  ██╗    ██║██║ ╚████║██████╔╝██║╚██████╗███████╗███████║
 ╚═╝  ╚═╝╚═╝  ╚═╝╚═╝  ╚═══╝╚═╝  ╚═╝    ╚═╝╚═╝  ╚═══╝╚═════╝ ╚═╝ ╚═════╝╚══════╝╚══════╝
   We have two notations for rank indices. Let N be the tensor's top rank, therefore there are N slots:
   - Slot notation:
      [1, N] which is how mathematicians would assign ordering to tensor's slots.
   - Rank notation:
      [N, 1] where the value corresponds to the rank of the tensor, that is:

         Slot 1 belongs to a tensor of rank N    (gives access to subtensors of rank N-1).
         Slot 2 belongs to a tensor of rank N-1. (gives access to subtensors of rank N-2).
         Slot 3 belongs to a tensor of rank N-2. (gives access to subtensors of rank N-3).
         ...
         Slot N-2 belongs to a tensor of rank 3. (gives access to subtensors of rank 2).
         Slot N-1 belongs to a tensor of rank 2. (gives access to subvectors of rank 1).
         Slot N belongs to a tensor of rank 1    (gives access to values of rank 0).

   Relation is therefore: R = N - S + 1  or  S = N - R + 1.

    
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
using Fluid.Internals;
using Fluid.Internals.Numerics;
using Fluid.Internals.Text;
using Fluid.TestRef;
namespace Fluid.Internals.Collections {
using static TnrExt;


/// <summary>A tensor with specified rank and specified dimension which holds direct subordinates of type τ.</summary>
/// <typeparam name="τ">Type of direct subordinates.</typeparam>
/// <typeparam name="α">Type of arithmetic.</typeparam>
public class Tnr<τ,α> : TnrBase<Tnr<τ,α>>, IEquatable<Tnr<τ,α>>
where τ : IEquatable<τ>, IComparable<τ>
where α : IArithmetic<τ>, new() {
   
   /// <summary>Dimensions of tensor's ranks as a list. E.g.: {3,2,6,5} is read as: {{rank 4, dim 3}, {rank 3, dim 2}, {rank 2, dim 6}, {rank 1, dim 5}}.</summary>
   public List<int> Strc { get; internal set; }
   /// <summary>Rank specifies the height (level) in the hierarchy on which the tensor sits. It equals the number of levels that exist below it. It tells us how many indices we must specify before we reach the value level. Rank notation: [0, N-1] where the value corresponds to the rank of tensors held by that slot.</summary>
   public int Rank { get; internal set; }
   /// <summary>Rank of the top tensor. Highest rank in the hierarchy.</summary>
   public int TopRank => Strc.Count;
   /// <summary>Slot notation: [1, N] which is how mathematicians would assign ordering to tensor's slots.</summary>
   public int Slot =>
      ChangeRankNotation(TopRank, Rank);
   /// <summary>Superior: a tensor directly above in the hierarchy. VoidTnr if this is the highest rank tensor.</summary>
   public Tnr<τ,α>? Superior { get; internal set; }
   /// <summary>Number of entries (non-zeros) in Tensor. Works even if the reference points to a Vector.</summary>
   public new virtual int Count => base.Count;
   /// <summary>Tensor dimension. The number of (potential) subtensors.</summary>
   public int Dim => Strc[StrcInx];
   /// <summary>Index in Structure where substructure begins (structure a non-top tensor would have if it was top).</summary>
   public int StrcInx => Strc.Count - Rank;
   /// <summary>Substructure (the structure a non-top tensor would have if it was top).</summary>
   public List<int> Substrc => EnumSubstrc().ToList();


   /// <summary>Void tensor.</summary>
   public static readonly Tnr<τ,α> T = TnrFactory.TopTensor<τ,α>(new List<int>{0,0}, 0);

   /// <summary>Constructor with redundancy, used internally.</summary>
   /// <param name="strc">Structure (absorbed).</param>
   /// <param name="rank">Rank.</param>
   /// <param name="sup">Direct superior.</param>
   /// <param name="cap">Capacity of internal dictionary.</param>
   protected Tnr(List<int> strc, int rank, Tnr<τ,α>? sup, int cap) : base(cap) {
      Strc = strc;
      Rank = rank;
      Superior = sup;
   }
   /// <summary>Constructor for a top tensor (null superior). For creating tensors of rank 1 use Vector's constructor.</summary>
   /// <param name="strc">Structure (absorbed).</param>
   /// <param name="cap">Capacity of internal dictionary.</param>
   internal Tnr(List<int> strc, int cap = 6) : this(strc, strc.Count, null, cap) { }
   /// <summary>Constructor for a non-top tensor (non-null superior). Assumes superior's structure is initialized.</summary>
   /// <param name="sup">Direct superior.</param>
   /// <param name="cap">Capacity of internal dictionary.</param>
   internal Tnr(Tnr<τ,α> sup, int cap = 6) : this(sup.Strc,
   sup.Rank - 1, sup, cap) { }

   /// <summary>Adds specified tensor as subordinate and appropriatelly sets its Superior and Structure.</summary>
   /// <param name="inx">Index at which the tensor will be added.</param>
   /// <param name="tnr">Tensor to add.</param>
   internal void AddSubTnr(int inx, Tnr<τ,α> tnr) {
      tnr.Superior = this;
      tnr.Strc = Strc;
      base.Add(inx, tnr);
   }

   internal void SetSubTnr(int inx, Tnr<τ,α>? tnr) {
      if(tnr != null) {
         base[inx] = tnr;
         tnr.Superior = this;
         tnr.Strc = Strc; }
      else
         Remove(inx);
   }
   
   /// <summary>A substructure as unevaluated instructions ready for enumeration.</summary>
   internal IEnumerable<int> EnumSubstrc() =>
      Strc.Skip(StrcInx);
   /// <summary>Packs substructure into a new structure. Use for new top tensors.</summary>
   internal List<int> CopySubstrc() =>
      EnumSubstrc().ToList();
      
   /// <summary>Copies substructure from specified tensor directly into Structure.</summary>
   /// <param name="tnr">Source.</param>
   protected void AssignStructFromSubStruct(Tnr<τ,α> tnr) {          // FIXME: Remove this after the copy specs fixes.
      Strc.Clear();
      var subStruct = tnr.Strc.Skip(tnr.StrcInx);
      foreach(var emt in subStruct) {
         Strc.Add(emt); }
   }
   
   /// <summary>A method that transorms between slot index and rank index (works both ways).</summary>
   /// <param name="topRankInx">Rank of the top-most tensor in the hierarchy.</param>
   /// <param name="slotOrRankInx">Slot (e.g. A^ijk ==> 1,2,3) or rank (e.g. A^ijk ==> 2,1,0) index.</param>
   static int ChangeRankNotation(int topRankInx, int slotOrRankInx) =>
      topRankInx - slotOrRankInx + 1;


   /// <summary>Tensor getting/setting indexer.</summary>
   /// <param name="overloadDummy">Type uint of first dummy argument specifies that we know we will be getting/setting a Tensor.</param>
   /// <param name="inxs">A set of indices specifiying which Tensor we want to set/get. The set length must not reach all the way out to scalar rank.</param>
   /// <remarks> <see cref="TestRefs.TensorTensorIndexer"/> </remarks>
   public Tnr<τ,α>? this[Tnr<τ,α> overloadDummy, params int[] inxs] {
      get {
         Tnr<τ,α>? tnr = this;
         for(int i = 0; i < inxs.Length; ++i) {
            if(!tnr.TryGetValue(inxs[i], out tnr))
               return null; }
         return tnr; }                                // No problem with tnr being null. We return above.
      set {
         Tnr<τ,α>? tnr = this;
         if(value != null) {       
            int n = inxs.Length - 1;
            for(int i = 0; i < n; ++i) {
               if(!tnr.TryGetValue(inxs[i], out tnr)) {                         // Crucial line: out tnr becomes the subtensor if found, if not it is created
                  tnr = new Tnr<τ,α>(tnr!, 6); //new Tensor<τ,α>(Structure, tnr!.Rank - 1, tnr, 6);
                  tnr.Superior!.AddSubTnr(inxs[i], tnr); } }
            var dict = (TnrBase<Tnr<τ,α>>) tnr;                            // tnr is now the proper subtensor.
            value.Superior = tnr;                                             // Crucial: to make the added tensor truly a part of this tensor, we must set proper superior and structure.
            value.Strc = Strc;
            dict[inxs[n]] = value; }
         else {
            for(int i = 0; i < inxs.Length; ++i) {
               if(!tnr.TryGetValue(inxs[i], out tnr))
                  return; }
            tnr.Superior!.Remove(inxs[inxs.Length - 1]); } }
   }

   public Tnr<τ,α>? GetT(params int[] inxs) {
      Tnr<τ,α>? tnr = this;
      for(int i = 0; i < inxs.Length; ++i) {
         if(!tnr.TryGetValue(inxs[i], out tnr))
            return null; }
      return tnr;
   }

   public void SetT(Tnr<τ,α>? t, params int[] inxs) {
      Tnr<τ,α>? tnr = this;
      if(t != null) {       
         int n = inxs.Length - 1;
         for(int i = 0; i < n; ++i) {
            if(!tnr.TryGetValue(inxs[i], out tnr)) {                         // Crucial line: out tnr becomes the subtensor if found, if not it is created.
               tnr = new Tnr<τ,α>(tnr!, 6); //new Tensor<τ,α>(Structure, tnr!.Rank - 1, tnr, 6);
               tnr.Superior!.AddSubTnr(inxs[i], tnr); } }
         var dict = (TnrBase<Tnr<τ,α>>) tnr;                            // tnr is now the proper subtensor.
         t.Superior = tnr;                                             // Crucial: to make the added tensor truly a part of this tensor, we must set proper superior and structure.
         t.Strc = Strc;
         dict[inxs[n]] = t; }
      else {
         for(int i = 0; i < inxs.Length; ++i) {
            if(!tnr.TryGetValue(inxs[i], out tnr))                   // t is null and the corresponding substructure does not exist. Leave as is.
               return; }
         tnr.Superior!.Remove(inxs[inxs.Length - 1]); }              // Corresponding tensor exists. Remove.
   }

   public Tnr<τ,α> GetNonNullTnr(params int[] inxs) {
      Tnr<τ,α>? tnr = this;
      for(int i = 0; i < inxs.Length; ++i) {
         if(!tnr.TryGetValue(inxs[i], out tnr))
            throw new NullReferenceException("Expected a non-null tensor at specified location."); }
      return tnr;
   }

   /// <summary>Vector getting/setting indexer. Use tnr[1f, 3] = null to remove an entry, should it exist.</summary>
   /// <param name="overloadDummy">Type float of first dummy argument specifies that we know we will be getting/setting a Vector.</param>
   /// <param name="inxs">A set of indices specifiying which Vector we want to set/get. The set length must reach exactly to Vector rank.</param>
   /// <remarks> <see cref="TestRefs.TensorVectorIndexer"/> </remarks>
   public Vec<τ,α>? this[Vec<τ,α> overloadDummy, params int[] inx] {
      get {
         Tnr<τ,α>? tnr = this;
         int n = inx.Length - 1;
         for(int i = 0; i < n; ++i) {
            if(!tnr.TryGetValue(inx[i], out tnr))
               return null; }
         if(tnr.TryGetValue(inx[n], out tnr))                                 // No problem with null.
            return (Vec<τ,α>)tnr;                                          // Same.
         else
            return null; }
      set {
         Tnr<τ,α>? tnr = this;
         if(value != null) {
            int n = inx.Length - 1;                                           // Entry one before last chooses tensor, last chooses vector.
            for(int i = 0; i < n; ++i) {
               if(tnr.TryGetValue(inx[i], out Tnr<τ,α>? tnr2)) {
                  tnr = tnr2; }
               else {                                                         // Tensor does not exist in an intermediate rank.
                  tnr = new Tnr<τ,α>(tnr, 4);                              //new Tensor<τ,α>(Structure, tnr.Rank - 1, tnr, 4);
                  tnr.Superior!.Add(inx[i], tnr); } }
            var dict = (TnrBase<Tnr<τ,α>>) tnr;                         // Tnr now refers to either a prexisting R2 tensor or a freshly created one.
            value.Superior = tnr;                                             // Crucial: to make the added vector truly a part of this tensor, we must set proper superior and structure.
            value.Strc = Strc;
            dict[inx[n]] = value; }                                           // We do not check that the value is a vector beforehand. It is assumed that the user used indexer correctly.
         else {
            int n = inx.Length;                                               // Last entry chooses vector.
            for(int i = 0; i < n; ++i) {
               if(!tnr.TryGetValue(inx[i], out tnr))
                  return; }
            tnr.Superior!.Remove(inx[n - 1]); } }     // Vector.Superior.Remove
   }

   public Vec<τ,α>? GetV(params int[] inx) {
      Tnr<τ,α>? tnr = this;
      int n = inx.Length - 1;
      for(int i = 0; i < n; ++i) {
         if(!tnr.TryGetValue(inx[i], out tnr))
            return null; }
      if(tnr.TryGetValue(inx[n], out tnr))                                 // No problem with null.
         return (Vec<τ,α>)tnr;                                          // Same.
      else
         return null;
   }

   public void SetV(Vec<τ,α>? v, params int[] inx) {
      Tnr<τ,α>? tnr = this;
      if(v != null) {
         int n = inx.Length - 1;                                           // Entry one before last chooses tensor, last chooses vector.
         for(int i = 0; i < n; ++i) {
            if(tnr.TryGetValue(inx[i], out Tnr<τ,α>? tnr2)) {
               tnr = tnr2; }
            else {                                                         // Tensor does not exist in an intermediate rank.
               tnr = new Tnr<τ,α>(tnr, 4);                              //new Tensor<τ,α>(Structure, tnr.Rank - 1, tnr, 4);
               tnr.Superior!.Add(inx[i], tnr); } }
         var dict = (TnrBase<Tnr<τ,α>>) tnr;                         // Tnr now refers to either a prexisting R2 tensor or a freshly created one.
         v.Superior = tnr;                                             // Crucial: to make the added vector truly a part of this tensor, we must set proper superior and structure.
         v.Strc = Strc;
         dict[inx[n]] = v; }                                           // We do not check that the value is a vector beforehand. It is assumed that the user used indexer correctly.
      else {
         int n = inx.Length;                                               // Last entry chooses vector.
         for(int i = 0; i < n; ++i) {
            if(!tnr.TryGetValue(inx[i], out tnr))
               return; }
         tnr.Superior!.Remove(inx[n - 1]); }
   }

   public Vec<τ,α> GetNonNullVec(params int[] inx) {
      Tnr<τ,α>? tnr = this;
      int n = inx.Length - 1;
      for(int i = 0; i < n; ++i) {
         if(!tnr.TryGetValue(inx[i], out tnr))
            throw new NullReferenceException("Expected a non-null tensor at specified location."); }
      if(tnr.TryGetValue(inx[n], out tnr))
         return (Vec<τ,α>)tnr;
      else
         throw new NullReferenceException("Expected a non-null vector at specified location.");;
   }

   /// <summary>Scalar getting/setting indexer.</summary>
   /// <param name="inxs">A set of indices specifiying which scalar we want to set/get. The set length must reach exactly to scalar rank.</param>
   /// <remarks> <see cref="TestRefs.TensorTauIndexer"/> </remarks>
   public virtual τ this[params int[] inx] {
      get {
         Tnr<τ,α>? tnr = this;
         int n = inx.Length - 2;
         for(int i = 0; i < n; ++i) {
            if(!tnr.TryGetValue(inx[i], out tnr))
               return NonNullable<τ,α>.O.Zero(); }
         if(tnr.TryGetValue(inx[n], out tnr)) {                                  // No probelm with null.
            var vec = (Vec<τ,α>)tnr;                                          // Same.
            if(vec.Scals.TryGetValue(inx[n + 1], out τ val))
               return val; }
         return NonNullable<τ,α>.O.Zero(); }
      set {
         Tnr<τ,α>? tnr = this;
         Tnr<τ,α>? tnr2;                                                       // Temporary to avoid null problem below.
         Vec<τ,α> vec;
         if(!value.Equals(NonNullable<τ,α>.O.Zero())) {
            if(inx.Length > 1) {                                                 // At least a 2nd rank tensor.
               int n = inx.Length - 2;
               for(int i = 0; i < n; ++i) {                                      // This loop is entered only for a 3rd rank tensor or above.
                  if(tnr.TryGetValue(inx[i], out tnr2)) {
                     tnr = tnr2; }
                  else {
                     tnr = new Tnr<τ,α>(tnr, 6);                              //new Tensor<τ,α>(Structure, tnr.Rank - 1, tnr, 6);
                     tnr.Superior!.Add(inx[i], tnr); }}
               if(tnr.TryGetValue(inx[n], out tnr2)) {                           // Does vector exist?
                  vec = (Vec<τ,α>) tnr2; }
               else {
                  vec = new Vec<τ,α>(Strc, tnr, 4); 
                  tnr.Add(inx[n], vec); }
               vec.Scals[inx[n + 1]] = value; } }
         else {
            int n = inx.Length - 1;
            for(int i = 0; i < n; ++i) {
               if(!tnr.TryGetValue(inx[i], out tnr))
                  return; }
            vec = (Vec<τ,α>) tnr;
            vec.Scals.Remove(inx[n]); } }
   }
   
   /// <summary>UNARY NEGATE. Creates a new tensor which is a negation of tnr1. The tensor is created as top rank, given its own substructure and no superstructure.</summary>
   /// <param name="t">Operand.</param>
   /// <remarks><see cref="TestRefs.Op_TensorNegation"/></remarks>
   public static Tnr<τ,α>? operator -(Tnr<τ,α>? t) =>
      t.NegateTop();

   /// <summary>Creates a new tensor which is a sum of the two operands. The tensor is created as top rank, given its own substructure and no superstructure.</summary>
   /// <param name="t1">Left operand.</param>
   /// <param name="t2">Right operand.</param>
   /// <remarks> <see cref="TestRefs.Op_TensorAddition"/> </remarks>
   public static Tnr<τ,α>? operator + (Tnr<τ,α>? t1, Tnr<τ,α>? t2) =>
      t1.SumTop(t2);

   /// <summary>Creates a new tensor which is a difference of the two operands. The tensor is created as top rank, given its own substructure and no superstructure.</summary>
   /// <param name="t1">Left operand.</param>
   /// <param name="t2">Right operand.</param>
   /// <remarks>First, we create our result tensor as a copy of tnr1. Then we take tnr2 as recursion dictator (yielding subtensors subTnr2) and we look for equivalent subtensors on tnr1 (call them subTnr1). If no equivalent is found, we negate subTnr2 and add it to a proper place in the result tensor, otherwise we subtract subTnr2 from subTnr1 and add that to result. Such a subtraction can yield a zero tensor (without entries) in which case we make sure we remove it (the empty tensor) from the result.
   /// Tests: <see cref="TestRefs.Op_TensorSubtraction"/> </remarks>
   public static Tnr<τ,α>? operator - (Tnr<τ,α>? t1, Tnr<τ,α>? t2) =>
      t1.SubTop(t2);

   
   /// <summary>Creates a new tensor which is a product of a scalar and a tensor. The tensor is created as top rank, given its own substructure and no superstructure.</summary>
   /// <param name="scal">Scalar.</param>
   /// <param name="aTnr">Tensor.</param>
   /// <remarks> <see cref="TestRefs.Op_ScalarTensorMultiplication"/> </remarks>
   public static Tnr<τ,α>? operator * (τ scal, Tnr<τ,α>? aTnr) =>
      aTnr.MulTop(scal);
   

   /// <summary>Checks whether all subordinates down the line have at least one value down their line. Returns a sequence of indices that lead to the problem if there is one, otherwise returns null.</summary>
   public List<int>? CheckIntegrity() {                           // TODO: Test CheckIntegrity.
      var errLog = new List<int>(Rank);                           // There can be at most &Rank indicies.
      if(Recursion(this))
         return null;
      else
         return errLog;

      bool Recursion(Tnr<τ,α> tnr) {
         if(tnr.Rank > 1) {
            if(tnr.Count > 0) {
               errLog.Add(-1);
               foreach(var sInx_sTnr in tnr) {
                  int sInx = sInx_sTnr.Key;
                  var sTnr = sInx_sTnr.Value;
                  errLog[tnr.Slot - 1] = sInx;
                  var result = Recursion(sTnr);
                  if(!result)
                     return false;
                  else
                     return true; }
               throw new Exception("Count returned more than 0, but the enumerator could not enumerate anything."); }
            else
               return false; }
         else
            return (tnr.Count > 0 ? true : false); }
   }


   /// <remarks> <see cref="TestRefs.TensorEnumerateRank"/> </remarks>
   public IEnumerable<Tnr<τ,α>> EnumerateRank(int rankInx) {
      Assume.True(rankInx > 1, () =>
         "This method applies only to ranks that hold pure tensors.");
      if(Rank > rankInx + 1) {
         foreach(var subTnr in Recursion(this))
            yield return subTnr; }
      else {
         foreach(var int_subTnr in this)
            yield return int_subTnr.Value; }

      IEnumerable<Tnr<τ,α>> Recursion(Tnr<τ,α> src) {
         foreach(var int_tnr in src) {
            if(int_tnr.Value.Rank > rankInx + 1) {
               foreach(var subTnr in Recursion(int_tnr.Value))
                  yield return subTnr; }
            else {
               foreach(var int_subTnr in int_tnr.Value)
                  yield return int_subTnr.Value; } } }
   }

   

   /// <summary>Compares substructures and values.</summary>
   /// <param name="tnr2">Tensor to compare to.</param>
   public bool Equals(Tnr<τ,α> tnr2) =>
      this.EqualS<τ,α>(tnr2);

   
   
   /// <summary>Create a string of all non-zero elements in form {{key1, val1}, {key2, val2}, ..., {keyN,valN}}.</summary>
   public override string ToString() {
      StringBuilder sb = new StringBuilder(72);
      sb.Append("{");
      foreach(var (i,st) in this.OrderBy( kvPair => kvPair.Key ))
         sb.Append($"{{{i},{st}}}, ");
      int length = sb.Length;
      sb.Remove(length - 2, 2);
      sb.Append("}");
      return sb.ToString();
   }
}

}