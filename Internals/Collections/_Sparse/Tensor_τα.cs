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
using Fluid.Internals;
using Fluid.Internals.Numerics;
using Fluid.Internals.Text;
using Fluid.TestRef;
namespace Fluid.Internals.Collections {


/// <summary>A tensor with specified rank and specified dimension which holds direct subordinates of type τ.</summary>
/// <typeparam name="τ">Type of direct subordinates.</typeparam>
/// <typeparam name="α">Type of arithmetic.</typeparam>
public partial class Tensor<τ,α> : TensorBase<Tensor<τ,α>>, IEquatable<Tensor<τ,α>>
where τ : IEquatable<τ>, IComparable<τ>
where α : IArithmetic<τ>, new() {
   
   
   /// <summary>Hierarchy's dimensional structure. First element specifies host's (tensor highest in hierarchy) rank, while last element specifies the rank of values. E.g.: {3,2,6,5} specifies structure of a tensor of 4th rank with first rank dimension equal to 5 and fourth rank dimension to 3. Setter works properly on non-top tensors. It must change the reference.</summary>
   public List<int> Structure { get; internal set; }
   /// <summary>Rank specifies the height (level) in the hierarchy on which the tensor sits. It equals the number of levels that exist below it. It tells us how many indices we must specify before we reach the value level. Rank notation: [0, N-1] where the value corresponds to the rank of tensors held by that slot.</summary>
   public int Rank { get; internal set; }
   /// <summary>Slot notation: [1, N] which is how mathematicians would assign ordering to tensor's slots.</summary>
   public int Slot =>
      ChangeRankNotation(Structure.Count, Rank);
   /// <summary>Superior: a tensor directly above in the hierarchy. VoidTnr if this is the highest rank tensor.</summary>
   public Tensor<τ,α>? Superior { get; internal set; }
   /// <summary>Number of entries (non-zeros) in Tensor. Works even if the reference points to a Vector.</summary>
   public new int Count => CountInternal;
   /// <summary>Virtual Count override so that it works also on Vectors when looking at them as Tensors.</summary>
   protected virtual int CountInternal => base.Count;
   /// <summary>The number of available spots one rank lower.</summary>
   public int Dim => Structure[Slot];
   /// <summary>Index of Structure list where substructure begins (the structure a non-top tensor would have if it was top).</summary>
   public int SubStrcInx => Structure.Count - Rank;
   /// <summary>Substructure (the structure a non-top tensor would have if it was top) as a traversal rule.</summary>
   public IEnumerable<int> SubStructure => Structure.Skip(SubStrcInx);


   /// <summary>Tensor getting/setting indexer.</summary>
   /// <param name="overloadDummy">Type uint of first dummy argument specifies that we know we will be getting/setting a Tensor.</param>
   /// <param name="inxs">A set of indices specifiying which Tensor we want to set/get. The set length must not reach all the way out to scalar rank.</param>
   /// <remarks> <see cref="TestRefs.TensorTensorIndexer"/> </remarks>
   public Tensor<τ,α>? this[Tensor<τ,α> overloadDummy, params int[] inxs] {
      get {
         Tensor<τ,α>? tnr = this;
         for(int i = 0; i < inxs.Length; ++i) {
            if(!tnr.TryGetValue(inxs[i], out tnr))
               return null; }
         return tnr; }                                // No problem with tnr being null. We return above.
      set {
         Tensor<τ,α>? tnr = this;
         if(value != null) {       
            int n = inxs.Length - 1;
            for(int i = 0; i < n; ++i) {
               if(!tnr.TryGetValue(inxs[i], out tnr)) {                         // Crucial line: out tnr becomes the subtensor if found, if not it is created
                  tnr = new Tensor<τ,α>(tnr!, 6); //new Tensor<τ,α>(Structure, tnr!.Rank - 1, tnr, 6);
                  tnr.Superior!.AddPlus(inxs[i], tnr); } }
            var dict = (TensorBase<Tensor<τ,α>>) tnr;                            // tnr is now the proper subtensor.
            value.Superior = tnr;                                             // Crucial: to make the added tensor truly a part of this tensor, we must set proper superior and structure.
            value.Structure = Structure;
            dict[inxs[n]] = value; }
         else {
            for(int i = 0; i < inxs.Length; ++i) {
               if(!tnr.TryGetValue(inxs[i], out tnr))
                  return; }
            tnr.Superior!.Remove(inxs[inxs.Length - 1]); } }
   }
   /// <summary>Vector getting/setting indexer. Use tnr[1f, 3] = null to remove an entry, should it exist.</summary>
   /// <param name="overloadDummy">Type float of first dummy argument specifies that we know we will be getting/setting a Vector.</param>
   /// <param name="inxs">A set of indices specifiying which Vector we want to set/get. The set length must reach exactly to Vector rank.</param>
   /// <remarks> <see cref="TestRefs.TensorVectorIndexer"/> </remarks>
   public Vector<τ,α>? this[Vector<τ,α> overloadDummy, params int[] inx] {
      get {
         Tensor<τ,α>? tnr = this;
         int n = inx.Length - 1;
         for(int i = 0; i < n; ++i) {
            if(!tnr.TryGetValue(inx[i], out tnr))
               return null; }
         if(tnr.TryGetValue(inx[n], out tnr))                                 // No problem with null.
            return (Vector<τ,α>)tnr;                                          // Same.
         else
            return null; }
      set {
         Tensor<τ,α>? tnr = this;
         if(value != null) {
            int n = inx.Length - 1;                                           // Entry one before last chooses tensor, last chooses vector.
            for(int i = 0; i < n; ++i) {
               if(tnr.TryGetValue(inx[i], out Tensor<τ,α>? tnr2)) {
                  tnr = tnr2; }
               else {                                                         // Tensor does not exist in an intermediate rank.
                  tnr = new Tensor<τ,α>(tnr, 4);                              //new Tensor<τ,α>(Structure, tnr.Rank - 1, tnr, 4);
                  tnr.Superior!.Add(inx[i], tnr); } }
            var dict = (TensorBase<Tensor<τ,α>>) tnr;                         // Tnr now refers to either a prexisting R2 tensor or a freshly created one.
            value.Superior = tnr;                                             // Crucial: to make the added vector truly a part of this tensor, we must set proper superior and structure.
            value.Structure = Structure;
            dict[inx[n]] = value; }                                           // We do not check that the value is a vector beforehand. It is assumed that the user used indexer correctly.
         else {
            int n = inx.Length;                                               // Last entry chooses vector.
            for(int i = 0; i < n; ++i) {
               if(!tnr.TryGetValue(inx[i], out tnr))
                  return; }
            tnr.Superior!.Remove(inx[n - 1]); } }     // Vector.Superior.Remove
   }
   /// <summary>Scalar getting/setting indexer.</summary>
   /// <param name="inxs">A set of indices specifiying which scalar we want to set/get. The set length must reach exactly to scalar rank.</param>
   /// <remarks> <see cref="TestRefs.TensorTauIndexer"/> </remarks>
   public virtual τ this[params int[] inx] {
      get {
         Tensor<τ,α>? tnr = this;
         int n = inx.Length - 2;
         for(int i = 0; i < n; ++i) {
            if(!tnr.TryGetValue(inx[i], out tnr))
               return O<τ,α>.A.Zero(); }
         if(tnr.TryGetValue(inx[n], out tnr)) {                                  // No probelm with null.
            var vec = (Vector<τ,α>)tnr;                                          // Same.
            if(vec.Scals.TryGetValue(inx[n + 1], out τ val))
               return val; }
         return O<τ,α>.A.Zero(); }
      set {
         Tensor<τ,α>? tnr = this;
         Tensor<τ,α>? tnr2;                                                       // Temporary to avoid null problem below.
         Vector<τ,α> vec;
         if(!value.Equals(default(τ))) {
            if(inx.Length > 1) {                                                 // At least a 2nd rank tensor.
               int n = inx.Length - 2;
               for(int i = 0; i < n; ++i) {                                      // This loop is entered only for a 3rd rank tensor or above.
                  if(tnr.TryGetValue(inx[i], out tnr2)) {
                     tnr = tnr2; }
                  else {
                     tnr = new Tensor<τ,α>(tnr, 6);                              //new Tensor<τ,α>(Structure, tnr.Rank - 1, tnr, 6);
                     tnr.Superior!.Add(inx[i], tnr); }}
               if(tnr.TryGetValue(inx[n], out tnr2)) {                           // Does vector exist?
                  vec = (Vector<τ,α>) tnr2; }
               else {
                  vec = new Vector<τ,α>(Structure, tnr, 4); 
                  tnr.Add(inx[n], vec); }
               vec.Scals[inx[n + 1]] = value; } }
         else {
            int n = inx.Length - 1;
            for(int i = 0; i < n; ++i) {
               if(!tnr.TryGetValue(inx[i], out tnr))
                  return; }
            vec = (Vector<τ,α>) tnr;
            vec.Scals.Remove(inx[n]); } }
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
         vec.Negate(); } }
   }
   
   /// <summary>UNARY NEGATE. Creates a new tensor which is a negation of tnr1. The tensor is created as top rank, given its own substructure and no superstructure.</summary>
   /// <param name="tnr1">Operand.</param>
   /// <remarks><see cref="TestRefs.Op_TensorNegation"/></remarks>
   public static Tensor<τ,α> operator -(Tensor<τ,α> tnr1) {                      // FIXME: Unary Negate.
      throw new NotImplementedException();
   }
   /// <summary>Creates a new tensor which is a sum of the two operands. The tensor is created as top rank, given its own substructure and no superstructure.</summary>
   /// <param name="tnr1">Left operand.</param>
   /// <param name="tnr2">Right operand.</param>
   /// <remarks> <see cref="TestRefs.Op_TensorAddition"/> </remarks>
   public static Tensor<τ,α> operator + (Tensor<τ,α> tnr1, Tensor<τ,α> tnr2) {
      Assume.True(DoSubstructuresMatch(tnr1, tnr2),
         () => "Tensor substructures do not match on addition.");
      //var newStructure = tnr1.CopySubstructure();
      var res = new Tensor<τ,α>(tnr2, CopySpecs.S320_04);          // Create a copy of second tensor.
      res.AssignStructFromSubStruct(tnr2);                               // Assign to it a new structure.
      // SimultaneousRecurse(tnr1, res, 2,
      //    onNoEquivalent: (key, subTnr1, supTnr2) => {    // Simply copy and add.
      //       var subTnr2 = new Tensor<τ,α>(subTnr1, in CopySpecs.S322_04);
      //       supTnr2.Add(key, subTnr2); },
      //    onStopRank: (tnr1R2, tnr2R2) => {                    // stopTnrs are rank 2.
      //       foreach(var int_tnr1R1 in tnr1R2) {
      //          int vecKey = int_tnr1R1.Key;
      //          var vec1 = (Vector<τ,α>) int_tnr1R1.Value;
      //          if(tnr2R2.TryGetValue(vecKey, out var tnr2R1)) {         // Same subtensor exists in target. Sum them and overwrite the one on target.
      //             var vec2 = (Vector<τ,α>) tnr2R1;
      //             var sumVec = vec1 + vec2;
      //             tnr2R2[Vector<τ,α>.Ex, vecKey] = sumVec; }
      //          else {                                                               // Same subtensor does not exist on target. Copy branch from source.
      //             var sumVec = new Vector<τ,α>(vec1, in CopySpecs.S322_04);
      //             tnr2R2.Add(vecKey, sumVec); } } } );
      return res;
   }
   /// <summary>Creates a new tensor which is a difference of the two operands. The tensor is created as top rank, given its own substructure and no superstructure.</summary>
   /// <param name="tnr1">Left operand.</param>
   /// <param name="tnr2">Right operand.</param>
   /// <remarks>First, we create our result tensor as a copy of tnr1. Then we take tnr2 as recursion dictator (yielding subtensors subTnr2) and we look for equivalent subtensors on tnr1 (call them subTnr1). If no equivalent is found, we negate subTnr2 and add it to a proper place in the result tensor, otherwise we subtract subTnr2 from subTnr1 and add that to result. Such a subtraction can yield a zero tensor (without entries) in which case we make sure we remove it (the empty tensor) from the result.
   /// Tests: <see cref="TestRefs.Op_TensorSubtraction"/> </remarks>
   public static Tensor<τ,α> operator - (Tensor<τ,α> tnr1, Tensor<τ,α> tnr2) {      // FIXME: Minus for tensors.
      Assume.True(DoSubstructuresMatch(tnr1, tnr2),
         () => "Tensor substructures do not match on subtraction.");
      var res = new Tensor<τ,α>(tnr1, CopySpecs.S320_04);                           // Result tnr starts as a copy of tnr1.
      res.AssignStructFromSubStruct(tnr1);
      // SimultaneousRecurse<(bool,int,Tensor<τ,α>,int,Tensor<τ,α>)>(tnr2, res, 2,                         // tnr2 = recursion dictator, res = recursion peer       (bool,Tnr,Tnr) = (isNowEmpty, highTnr, lowTnr)
      //    onEmptySrc: () => (true, 0, null, 0, null),
      //    onResurface: ( tup ) => {
      //       if(!tup.Item1)                                                          // subTnr not empty, subTnr present as item4 with index item3.
      //          tup.Item2[Tensor<τ,α>.Ex, tup.Item3] = tup.Item4;
      //    },
      //    onNoEquivalent: (key, subTnr2, supRes) => {    // Simply copy, negate and add. Could be done more optimally. BUt I'll leave it like that for now, because it's simpler.
      //       var subTnr2Copy = new Tensor<τ,α>(subTnr2, in CopySpecs.S322_04);
      //          subTnr2Copy.Negate();
      //          supRes.Add(key, subTnr2Copy);
      //    },
      //    onStopRank: (tnr2R2, res1R2) => {                    // stopTnrs are rank 2.
      //       foreach(var int_tnr2R1 in tnr2R2) {
      //          if(res1R2.TryGetValue(int_tnr2R1.Key, out var res1R1)) {         // Same subtensor exists in target. Sum them and overwrite the one on target.
      //             int subKey = int_tnr2R1.Key;
      //             var subVec2 = (Vector<τ,α>) int_tnr2R1.Value;
      //             var difVec = (Vector<τ,α>) res1R1 - subVec2;
      //             res1R2[Vector<τ,α>.Ex, subKey] = difVec; }                     // FIXME: This can be zero. What then?
      //          else {                                                               // Same subtensor does not exist on target. Copy branch from source and negate it.
      //             var srcCopy = new Vector<τ,α>((Vector<τ,α>) int_tnr2R1.Value,
      //                in CopySpecs.S322_04);
      //             srcCopy.Negate();
      //             res1R2.Add(int_tnr2R1.Key, srcCopy); } }
      //    } );
      return res;
   }
   /// <summary>Destructively sums tnr1 and tnr2. Don't use references afterwards. Returns a top tensor (null superior).</summary>
   /// <param name="tnr">&this tensor will be absorbed into tnr.</param>
   /// <remarks> <see cref="TestRefs.TensorSum"/> </remarks>
   public Tensor<τ,α>? SumInto(Tensor<τ,α> tnr) {
      Assume.True(DoSubstructuresMatch(tnr, this),
         () => "Tensor substructures do not match on Sum.");
      Recursion(tnr, this);

      void Recursion(Tensor<τ,α> t1, Tensor<τ,α> t2) {
         if(t2.Rank > 2) {
            foreach(var int_st2 in t2) {
               int sInx = int_st2.Key;
               var st2 = int_st2.Value;
               if(t1.TryGetValue(sInx, out var st1)) {                        // Equivalent subtensor exists in T1.
                  Recursion(st1, st2);
                  if(st1.Count == 0)
                     t1.Remove(sInx); }
               else                                                                 // Equivalent subtensor does not exist in T1. Absorb the subtensor from T2 and add it.
                  t1.AddPlus(sInx, st2); } }
         else if(t2.Rank == 2) {
            foreach(var int_st2 in t2) {
               int sInx = int_st2.Key;
               var st2 = int_st2.Value;
               if(t1.TryGetValue(sInx, out var subTnr1)) {                        // Entry exists in t1, we must sum.
                  var sv1 = (Vector<τ,α>) subTnr1;
                  var sv2 = (Vector<τ,α>) st2;
                  sv1.Sum(sv2);
                  if(sv1.Count == 0)
                     t1.Remove(sInx); }                                           // Crucial to remove if subvector has been anihilated.
               else {
                  t1.AddPlus(sInx, st2); } } }                                // Entry does not exist in t1, simply Add.
         else {                                                                     // We have a vector.
            var vec1 = (Vector<τ,α>) t1;
            var vec2 = (Vector<τ,α>) t2;
            Vector<τ,α>.SumM(vec1, vec2);
         }
      }
   }
   /// <summary>Subtracts tnr2 from the caller. Tnr2 is still usable afterwards.</summary>
   /// <param name="aTnr2">Minuend which will be subtracted from the caller. Minuend is still usable after the operation.</param>
   /// <remarks><see cref="TestRefs.TensorSub"/></remarks>
   public void Sub(Tensor<τ,α> aTnr2) {
      Tensor<τ,α> aTnr1 = this;
      Assume.True(DoSubstructuresMatch(aTnr1, aTnr2),
         () => "Tensor substructures do not match on Sub.");
      Recursion(aTnr1, aTnr2);

      void Recursion(Tensor<τ,α> tnr1, Tensor<τ,α> tnr2) {
         if(tnr2.Rank > 2) {
            foreach(var int_subTnr2 in tnr2) {
               int subKey = int_subTnr2.Key;
               var subTnr2 = int_subTnr2.Value;
               if(tnr1.TryGetValue(subKey, out var subTnr1))            // Equivalent subtensor exists in T1.
                  Recursion(subTnr1, subTnr2);
               else                                                      // Equivalent subtensor does not exist in T1. Negate the subtensor from T2 and add it.
                  tnr1.AddPlus(subKey,-subTnr2); } }
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
                  tnr1.AddPlus(subKey, -subVec2); } } }          // Entry does not exist in t2, simply Add.
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
         var res = new Tensor<τ,α>(newStructure, aTnr.Count);        //new Tensor<τ,α>(newStructure, aTnr.Rank, Voids<τ,α>.Vec, aTnr.Count);
         if(tnr.Rank > 2) {                                       // Subordinates are tensors.
            foreach (var int_subTnr in tnr) {
               int subKey = int_subTnr.Key;
               var subTnr = int_subTnr.Value;
               var subRes = Recursion(subTnr);
               res.AddPlus(subKey, subRes); } }
         else if(tnr.Rank == 2) {                                 // Subordinates are vectors.
            foreach (var int_subTnr in tnr) {
               int subKey = int_subTnr.Key;
               var subVec = (Vector<τ,α>) int_subTnr.Value;
               res.AddPlus(subKey, scal*subVec); } }
         else
            return scal*((Vector<τ,α>) tnr);
         return res;
      }
   }
   /// <summary>Calculates tensor product of this tensor (left-hand operand) with another tensor (right-hand operand).</summary>
   /// <param name="aTnr2">Right-hand operand.</param>
   /// <remarks> <see cref="TestRefs.TensorProduct"/> </remarks>
   public virtual Tensor<τ,α> TnrProductTop(Tensor<τ,α> aTnr2) {
      // Overriden on vector when first operand is a vector.
      // 1) Descend to rank 1 through a recursion and then delete that vector.
      // 2) Substitute it with a tensor of rank tnr2.Rank + 1 whose entries are tnr2s multiplied by the corresponding scalar that used to preside there in the old vector.
      int newRank = Rank + aTnr2.Rank;
      var struct1 = CopySubstructure();
      var struct2 = aTnr2.CopySubstructure();
      var newStructure = struct1.Concat(struct2).ToList();
      return Recursion(this, newRank);

      Tensor<τ,α> Recursion(Tensor<τ,α> tnr1, int resRank) {
         var res = new Tensor<τ,α>(newStructure, resRank, tnr1.Superior, tnr1.Count);
         if(tnr1.Rank > 2) {                                                    // Only copy. Adjust ranks.
            foreach(var int_subTnr1 in tnr1) {
               int subKey = int_subTnr1.Key;
               var subTnr1 = int_subTnr1.Value;
               res.AddPlus(subKey, Recursion(subTnr1, resRank - 1)); } }
         else {                                                            // We are now at tensor which contains vectors.
            foreach(var int_subTnr1R1 in tnr1) {                           // Substitute each vector with a new tensor.
               int subKeyR1 = int_subTnr1R1.Key;
               var subVec1 = (Vector<τ,α>) int_subTnr1R1.Value;
               var newSubTnr = new Tensor<τ,α>(newStructure, resRank - 1, tnr1, subVec1.Count);
               foreach(var int_subVal1 in subVec1.Scals) {
                  int subKeyR0 = int_subVal1.Key;
                  var subVal1 = int_subVal1.Value;
                  newSubTnr.AddPlus(subKeyR0, subVal1*aTnr2); }
               res.AddPlus(subKeyR1, newSubTnr); } }
         return res;
      }
   }
   /// <summary>Eliminates a specific rank n by choosing a single tensor at that rank and substituting it in place of its direct superior (thus discarding all other tensors at rank n). The resulting tensor is a new tensor of reduced rank (method is non-destructive).</summary>
   /// <param name="r"> Rank index (zero-based) of the rank to eliminate.</param>
   /// <param name="e">Element index (zero-based) in that rank in favor of which the elimination will take place.</param>
   /// <remarks>Test: <see cref="TestRefs.TensorReduceRank"/></remarks>
   public Tensor<τ,α> ReduceRank(int r, int e) {                                    // FIXME: Method must properly reassign superiors.
         Assume.True(r < Rank && r > -1, () =>
         Toolbox.T.S.Y("You can only eliminate a rank in range [0, TopRank).") );           // TODO: Check superiors and structures for all methods.
      var strcL = Structure.Take(r);
      var strcR = Structure.Skip(r + 1);
      var strc = strcL.Concat(strcR).ToList();                                      // Structure is rebuilt. We won't copy it.
      if(r == Rank - 1) {                                                           // Only one rank exists above rank n. Pick one tensor from rank n and return it.
         if(Rank > 1) {                                                             // Tensor to be reduced is at least rank two.
            if(TryGetValue(e, out var subTnr)) {                                    // Element exists.
               var newTnr = subTnr.Copy(in CopySpecs.S320_04);                      // We don't copy superior because result is top tensor.
               newTnr.Structure = strc;
               return newTnr; }
            else
               return Factory<τ,α>.CreateEmptyTensor(0, strc.Count, strc); }  // Return empty tensor.
         else                                                                       // Rank <= 1: impossible.
            throw new ArgumentException(
               "Cannot eliminate rank 1 or lower on rank 1 tensor."); }
      else if(r > 1) {                                                       // At least two ranks exist above elimRank & elimRank is at least 2. Obviously applicable only to Rank 4 or higher tensors.
         var res = new Tensor<τ,α>(strc, Rank - 1, Voids<τ,α>.Vec, Count);
         if(Rank > 3) {                                                              // No special treatment due to Vector needed.
            RecursiveCopyAndElim(this, res, e, r + 2);
            return res; }
         else
            throw new ArgumentException("Cannot eliminate rank 2 or above on rank 1,2,3 tensor with this branch."); }
      else if(r == 1) {                                                      // At least two ranks exist above elimRank & elimRank is 1. Obviously applicable only to rank 3 or higher tensors.
         var res = new Tensor<τ,α>(strc, Rank - 1, Voids<τ,α>.Vec, Count);
         if(Rank > 2) {                                                             // Result is tensor.
            RecursiveCopyAndElim(this, res, e, 1);
            return res; }
         else
            throw new ArgumentException("Cannot eliminate rank 1 on rank 1,2 tensor with this branch."); }
      else {                                          // At least two ranks exist above elimRank & elimRank is 0. Obviously applicable only to rank 2 or higher tensors.
         if(Rank > 2) {                               // Result is tensor. Choose one value from each vector in subordinate rank 2 tensors, build a new vector and add those values to it. Then add that vector to superior rank 3 tensor.
            var res = new Tensor<τ,α>(strc, Rank - 1, Voids<τ,α>.Vec, Count);
            ElimR0_R3Plus(this, res, e);
            return res; }
         else if(Rank == 2) {
            var res = new Vector<τ,α>(strc, Voids<τ,α>.Vec, 4);
            ElimR0_R2(this, res, e);
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
               tgt.AddPlus(int_tnr.Key, subTnr); } }
      else {                                                             // We have reached rank directly above rank scheduled for elimination: eliminate.
         foreach(var int_tnr in src) {
            if(int_tnr.Value.TryGetValue(emtInx, out var subTnr)) {
               var subTnrCopy = subTnr.Copy(in CopySpecs.S320_04);
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
         if(subVec.Scals.TryGetValue(emtInx, out var val))
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
               tgt.AddPlus(int_tnr.Key, subTnr); } }
      else {                                                                  // src.Rank == 3.
         foreach(var int_tnr in src) {
            var newVec = new Vector<τ,α>(tgt, 4);
            ElimR0_R2(int_tnr.Value, newVec, emtInx);
            tgt.AddPlus(int_tnr.Key, newVec); } } }

   protected static (List<int> struc, int rank1, int rank2, int conDim) ContractPart1(
   Tensor<τ,α> tnr1, Tensor<τ,α> tnr2, int slot1, int slot2) {
      // 1) First eliminate, creating new tensors. Then add them together using tensor product.
      List<int>   struc1 = tnr1.Structure, 
                  struc2 = tnr2.Structure;
      int rank1 = struc1.Count,
            rank2 = struc2.Count;
      Assume.True(rank1 == tnr1.Rank && rank2 == tnr2.Rank,
         () => "One of the tensors is not top rank.");
      Assume.AreEqual(struc1[slot1 - 1], struc2[slot2 - 1],              // Check that the dimensions of contracted ranks are equal.
         "Rank dimensions at specified indices must be equal.");
      int   conDim = tnr1.Structure[slot1 - 1],                                // Dimension of rank we're contracting.
            rankInx1 = ChangeRankNotation(tnr1, slot1),
            rankInx2 = ChangeRankNotation(tnr2, slot2);
      var struc3_1 = struc1.Where((emt, i) => i != (slot1 - 1));
      var struc3_2 = struc2.Where((emt, i) => i != (slot2 - 1));
      var struc3 = struc3_1.Concat(struc3_2).ToList();                 // New structure.
      return (struc3, rankInx1, rankInx2, conDim);
   }
   
   public static Tensor<τ,α> ContractPart2(Tensor<τ,α> tnr1, Tensor<τ,α> tnr2,
   int rankInx1, int rankInx2, List<int> struc3, int conDim) {
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
               sum = new Vector<τ,α>(struc3, Voids<τ,α>.Vec, 4);
               for(int i = 0; i < conDim; ++i) {
                  elimVec = (Vector<τ,α>) tnr1.ReduceRank(rankInx1, i);
                  if(elimVec.Count != 0 && vec.Scals.TryGetValue(i, out var val)) {
                     sumand = val*elimVec;
                     sum.Sum(sumand); } }
               if(sum.Scals.Count != 0)
                  return sum;
               else
                  return Factory<τ,α>.CreateEmptyTensor(0, 1, struc3); }
            else {                                             // Result will be tensor.
               Tensor<τ,α> elimTnr1, sumand, sum;
               sum = new Tensor<τ,α>(struc3);
               for(int i = 0; i < conDim; ++i) {
                  elimTnr1 = tnr1.ReduceRank(rankInx1, i);
                  if(elimTnr1.Count != 0 && vec.Scals.TryGetValue(i, out var val)) {
                     sumand = val*elimTnr1;
                     sum.Sum(sumand); } }
               if(sum.Count != 0)
                  return sum;
               else
                  return Factory<τ,α>.CreateEmptyTensor(0, struc3.Count, struc3); } } }
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
      (List<int> struc3, int rank1, int rank2, int conDim) = ContractPart1(tnr1, tnr2, slotInx1, slotInx2);
      return ContractPart2(tnr1, tnr2, rank1, rank2, struc3, conDim);
   }
   /// <summary>Contracts across the two slot indices on a rank 2 tensor.</summary>
   /// <remarks> <see cref="TestRefs.TensorSelfContractR2"/> </remarks>
   public τ SelfContractR2() {
      Assume.True(Rank == 2, () => "Tensor rank has to be 2 for this method.");
      Assume.True(Structure[0] == Structure[1], () =>
         "Corresponding dimensions have to be equal.");
      τ result = O<τ,α>.A.Zero();
      foreach(var int_vec in this) {
         var vec = (Vector<τ,α>) int_vec.Value;
         if(vec.Scals.TryGetValue(int_vec.Key, out τ val))
            result = O<τ,α>.A.Sum(result, val); }
      return result;
   }

   public Vector<τ,α> SelfContractR3(int slot1, int slot2) {
      Assume.True(Rank == 3, () => "Tensor rank has to be 3 for this method.");
      Assume.True(Structure[slot1 - 1] == Structure[slot2 - 1], () =>
         "Corresponding dimensions have to be equal.");
      Vector<τ,α> res = new Vector<τ,α>(new List<int> {Structure[2]}, Voids<τ,α>.Vec, 4);
      int rank1 = ChangeRankNotation(this, slot1);
      int rank2 = ChangeRankNotation(this, slot2);
      if(slot1 == 1) {
         if(slot2 == 2) {
            foreach(var int_tnr in this) {
               if(int_tnr.Value.TryGetValue(int_tnr.Key, out var subTnr)) {
                  var vec = (Vector<τ,α>) subTnr;
                  res.Sum(vec); } } }
         if(slot2 == 3) {
            foreach(var int_tnr in this) {
               foreach(var int_subTnr in int_tnr.Value) {
                  var subVec = (Vector<τ,α>) int_subTnr.Value;
                  if(subVec.Scals.TryGetValue(int_tnr.Key, out τ val))
                     res.Scals[int_subTnr.Key] = O<τ,α>.A.Sum(res[int_subTnr.Key], val); } } } }
      else if(slot1 == 2) {                   // natInx2 == 3
         foreach(var int_tnr in this) {
            foreach(var int_subTnr in int_tnr.Value) {
               var subVec = (Vector<τ,α>) int_subTnr.Value;
               if(subVec.Scals.TryGetValue(int_subTnr.Key, out τ val))
                  res.Scals[int_tnr.Key] = O<τ,α>.A.Sum(res[int_tnr.Key], val); } } }
      return res;
   }
   /// <summary>Contracts across two slot indices on a single tensor of at least rank 3.</summary>
   /// <param name="slot1">Slot index 1.</param>
   /// <param name="slot2">Slot index 2.</param>
   /// <remarks><see cref="TestRefs.TensorSelfContract"/></remarks>
   public Tensor<τ,α> SelfContract(int slot1, int slot2) {
      Assume.True(Rank > 2, () =>
         "This method is not applicable to rank 2 tensors.");
      Assume.True(Structure[slot1 - 1] == Structure[slot2 - 1], () =>
         "Dimensions of contracted slots have to be equal.");
      if(Rank > 3) {
         var newStruct1 = Structure.Take(slot1 - 1);
         var newStruct2 = Structure.Take(slot2 - 1).Skip(slot1);
         var newStruct3 = Structure.Skip(slot2);
         var newStruct = newStruct1.Concat(newStruct2).Concat(newStruct3).ToList();
         var res = new Tensor<τ,α>(newStruct, Rank - 2, Voids<τ,α>.Vec, Count);
         int rank1 = ChangeRankNotation(this, slot1);
         int rank2 = ChangeRankNotation(this, slot2);
         int dimRank = Structure[slot1 - 1];                // Dimension of contracted rank.
         for(int i = 0; i < dimRank; ++i) {                    // Over each element inside contracted ranks.
            var step1Tnr = ReduceRank(rank2, i);
            var sumand = step1Tnr.ReduceRank(rank1 - 1, i);
            res.Sum(sumand); }
         return res; }
      else
         return SelfContractR3(slot1, slot2);
   }

   /// <summary>Checks whether all subordinates down the line have at least one value down their line. Returns a sequence of indices that lead to the problem if there is one, otherwise returns null.</summary>
   public List<int>? CheckIntegrity() {                           // TODO: Test CheckIntegrity.
      var errLog = new List<int>(Rank);                           // There can be at most &Rank indicies.
      if(Recursion(this))
         return null;
      else
         return errLog;

      bool Recursion(Tensor<τ,α> tnr) {
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

   /// <summary>Compares substructures of two equal rank tensors.</summary>
   /// <param name="tnr1">First tensor.</param>
   /// <param name="tnr2">Second tensor.</param>
   public static bool DoSubstructuresMatch(Tensor<τ,α> tnr1, Tensor<τ,α> tnr2) {
      Assume.True(tnr1.Rank == tnr2.Rank, () => "Tensor ranks do not match.");                                    // First, ranks must match.
      return tnr1.SubStructure.SequenceEqual(tnr2.SubStructure);
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
                  yield return int_subTnr.Value; } } }
   }

   /// <summary>Static implementation to allow for null comparison. If two tensors are null they are equal.</summary>
   /// <param name="tnr1">Tensor 1.</param>
   /// <param name="tnr2">Tensor 2.</param>
   public static bool AreEqual(Tensor<τ,α>? tnr1, Tensor<τ,α>? tnr2) {
      if(tnr1 == null) {                                                            // If both are null, return true. If only one of them is null, return false.
         if(tnr2 == null)
            return true;
         else
            return false; }
      else if(tnr2 == null)
         return false;
      if(!DoSubstructuresMatch(tnr1, tnr2))                                         // If substructures mismatch, they are not equal.
         return false;
      return TnrRecursion(tnr1, tnr2);

      bool TnrRecursion(Tensor<τ,α> sup1, Tensor<τ,α> sup2) {                       // Recursion must be entered with non-null tensors.
         if(!sup1.Keys.OrderBy(key => key).SequenceEqual(sup2.Keys.OrderBy(key => key)))
            return false;                                                                    // Keys have to match. This is crucial.
         if(sup1.Rank > 2) {
            foreach(var inx_sub1 in sup1) {
               var sub2 = sup2[T, inx_sub1.Key];
               return TnrRecursion(inx_sub1.Value, sub2); }
            return true; }                                                                   // Both are empty.
         else
            return VecRecursion(sup1, sup2); }

      bool VecRecursion(Tensor<τ,α> sup1R2, Tensor<τ,α> sup2R2) {
         if(!sup1R2.Keys.OrderBy(key => key).SequenceEqual(sup2R2.Keys.OrderBy(key => key)))
            return false;                                                                    // Keys have to match. This is crucial.
         foreach(var inx_sub1R1 in sup1R2) {
            var vec1 = (Vector<τ,α>) inx_sub1R1.Value;
            var vec2 = sup2R2[Voids<τ,α>.Vec, inx_sub1R1.Key];
            if(!vec1.Equals(vec2))
               return false; }
         return true; }
   }

   /// <summary>Check two tensors for equality.</summary>
   /// <param name="tnr2">Other tensor.</param>
   public bool Equals(Tensor<τ,α> tnr2) =>
      AreEqual(this, tnr2);

   /// <remarks> <see cref="TestRefs.TensorEquals"/> </remarks>
   public bool Equals(Tensor<τ,α> tnr2, τ eps) {
      Assume.True(DoSubstructuresMatch(this, tnr2),
         () => "Tensor substructures do not match on equality comparison.");
      return TnrRecursion(this, tnr2);

      bool TnrRecursion(Tensor<τ,α> sup1, Tensor<τ,α> sup2) {
         if(!sup1.Keys.OrderBy(key => key).SequenceEqual(sup2.Keys.OrderBy(key => key)))
            return false;                                                                    // Keys have to match. This is crucial.
         if(sup1.Rank > 2) {
            foreach(var inx_sub1 in sup1) {
               int inx = inx_sub1.Key;
               var sub1 = inx_sub1.Value;
               var sub2 = sup2[Tensor<τ,α>.T, inx];
               return TnrRecursion(inx_sub1.Value, sub2); }
            return true; }                                                                   // Both are empty.
         else
            return VecRecursion(sup1, sup2); }

      bool VecRecursion(Tensor<τ,α> sup1R2, Tensor<τ,α> sup2R2) {
         if(!sup1R2.Keys.OrderBy(key => key).SequenceEqual(sup2R2.Keys.OrderBy(key => key)))
            return false;                                                                    // Keys have to match. This is crucial.
         foreach(var inx_sub1R1 in sup1R2) {
            var vec1 = (Vector<τ,α>) inx_sub1R1.Value;
            var vec2 = sup2R2[Vector<τ,α>.V, inx_sub1R1.Key];
            if(!vec1.Equals(vec2, eps))
               return false; }
         return true; }                                                         // All values agree within tolerance.
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
}

}