using System;
using System.Text;
using System.Linq;
using SCG = System.Collections.Generic;
using TB = Fluid.Internals.Toolbox;
using static Fluid.Internals.Numerics.MatOps;

using Fluid.Internals.Numerics;

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
      public Tensor<τ,α> Sup { get; protected set; }

      public static class CopySpecs {
         public static readonly CopySpecStruct Default = new CopySpecStruct();
         public static readonly CopySpecStruct AddSubtract = new CopySpecStruct(
            GeneralSpecs.Both, MetaSpecs.All, StructureSpecs.TrueCopy, 0, 4);
         //public static readonly CopySpecStruct ScalarMultiply = new CopySpecStruct(
         //   GeneralSpecs.Meta, MetaSpecs.All, StructureSpecs.TrueCopy);
         public static readonly CopySpecStruct ScalarMultiply = new CopySpecStruct(
            GeneralSpecs.Meta, MetaSpecs.All, StructureSpecs.RefCopy);
         public static readonly CopySpecStruct ElimRank3 = new CopySpecStruct(          // Used on ElimRank method.
            GeneralSpecs.Both, MetaSpecs.Rank);
         public static readonly CopySpecStruct ElimRank2 = new CopySpecStruct(          // Used on ElimRank method.
            GeneralSpecs.Both, MetaSpecs.Rank | MetaSpecs.Sup);
         /// <summary>Copy values and rank.</summary>
         public static readonly CopySpecStruct ElimRank = new CopySpecStruct(
            GeneralSpecs.Both, MetaSpecs.Rank);
      }
      protected Tensor(int cap) : base(cap) { }
      protected Tensor(int[] structure, int rank, Tensor<τ,α> sup, int cap) : base(cap) {
         Structure = structure ?? null;
         Rank = rank;
         Sup = sup ?? null;
      }
      /// <summary>Creates a top tensor with specified structure and initial capacity. Rank is assigned as the length of structure array.</summary>
      /// <param name="structure">Specifies dimension of each rank.</param>
      /// <param name="cap">Initially assigned memory.</param>
      public Tensor(int[] structure, int cap = 6) : this(structure, structure.Length, null, cap) { }
      /// <summary>Creates a non-top tensor with specified superior and initial capacity. Rank is assigned as one less that superior.</summary>
      /// <param name="sup">Tensor directly above in hierarchy.</param>
      /// <param name="cap">Initially assigned memory.</param>
      public Tensor(Tensor<τ,α> sup, int cap) : this(sup.Structure, sup.Rank - 1, sup ?? null, cap) { }
      /// <summary>Creates a deep copy of specified tensor. You can optionally specify which meta-fields (Structure, Rank ...) to copy.</summary>
      /// <param name="src">Source tensor to copy.</param>
      /// <param name="extraCap">How much more top rank space should the new tensor have (newCap = oldCap + extraCap).</param>
      /// <param name="cs">Exact specification of fields to copy. Default is all.</param>
      public Tensor(in Tensor<τ,α> src, in CopySpecStruct cs) :
       base(src.Count + cs.ExtraCapacity) {
         Copy(src, this, in cs);
      }
      public Tensor(in Tensor<τ,α> src) : this(in src, in CopySpecs.Default) { }
      /// <summary>Creates a tensor as a deep copy of this one: same Rank, Structure and Superior.</summary>
      public virtual Tensor<τ,α> Copy(in CopySpecStruct cs) {
         // if(Rank == 1) {
         //    var res = new Vector<τ,α>(Count + cs.ExtraCapacity);
         //    var thisVector = (Vector<τ,α>) this;
         //    Vector<τ,α>.Copy(thisVector, res, in cs);
         //    return res; }
         // else { }
         var res = new Tensor<τ,α>(Count + cs.ExtraCapacity);
         Copy(this, res, in cs);
         return res;
      }
      /// <summary>Make a shallow or deep copy of a tensor. Set CopySpec field for fine tunning, ensure proper capacity of the target tensor.</summary>
      /// <param name="src">Copy source.</param>
      /// <param name="tgt">Copy target.</param>
      /// <param name="cs">Exact specification of what fields to copy. Default is all.</param>
      public static void Copy(in Tensor<τ,α> src, Tensor<τ,α> tgt, in CopySpecStruct cs) {
         TB.Assert.True(src.Rank > 1,
            "Tensors's rank has to be at least 2 to be copied via this method.");
         CopyMetaFields(src, tgt, in cs.MetaFields, in cs.Structure);
         if((cs.General & GeneralSpecs.Vals) == GeneralSpecs.Vals) {
            int endRank = cs.EndRank;
            Recursion(src, tgt);

            void Recursion(Tensor<τ,α> src1, Tensor<τ,α> tgt1) {
            if(src1.Rank > 2) {                                       // Subordinates are tensors.
               foreach (var kv in src1) {
                  var tnr = new Tensor<τ,α>(kv.Value.Structure, kv.Value.Rank, kv.Value.Sup, kv.Value.Count);
                  if(src1.Rank > endRank)
                     Recursion(kv.Value, tnr);
                  tnr.Structure = tgt1.Structure;
                  tnr.Rank = tgt1.Rank - 1;
                  tnr.Sup = tgt1;
                  tgt1.Add(kv.Key, tnr); } }
            else if(src1.Rank == 2) {                                 // Subordinates are vectors.
               foreach (var kv in src1)
                  tgt1.Add(kv.Key, new Vector<τ,α>((Vector<τ,α>) kv.Value)); }
            else
               throw new InvalidOperationException(
                  "Tensors's rank has to be at least 2 to be copied via this method.");
            }
         }
      }
      public static void CopyMetaFields(Tensor<τ,α> src, Tensor<τ,α> tgt, in MetaSpecs mcs,
      in StructureSpecs scs) {
         if((mcs & MetaSpecs.Structure) == MetaSpecs.Structure) {
            if((scs & StructureSpecs.RefCopy) == StructureSpecs.RefCopy)
               Array.Copy(src.Structure, tgt.Structure, src.Structure.Length);
            else
               tgt.Structure = src.Structure; }
         if((mcs & MetaSpecs.Rank) == MetaSpecs.Rank)
            tgt.Rank = src.Rank;
         if((mcs & MetaSpecs.Sup) == MetaSpecs.Sup)
            tgt.Sup = src.Sup ?? null;
      }
      public static Tensor<τ,α> CreateFromArray(τ[][] arr) {
         int nRows = arr.Length;
         int nCols = arr[0].Length;
         var tnr = new Tensor<τ,α>(new int[2] {nRows, nCols});
         for(int i = 0; i < nRows; ++i)
            for(int j = 0; j < nCols; ++j)
               tnr[i,j] = arr[i][j];
         return tnr;
      }
      public static Tensor<τ,α> CreateFromArray(τ[] arr, int allRows, int startRow,
         int nRows, int startCol, int nCols, int width, int height, int startRowInx = 0, int startColInx = 0) {
            int allCols = arr.Length/allRows;
            var tnr = new Tensor<τ,α>(new int[2] {height, width});
            for(int i = startRow, k = startRowInx; i < startRow + nRows; ++i, ++k)
               for(int j = startCol, l = startColInx; j < startCol + nCols; ++j, ++l)
                  tnr[k,l] = arr[i*allCols + j];
            return tnr;
      }
      public static Tensor<τ,α> CreateFromArray(τ[] arr, int allRows, int startRow,
         int nRows, int startCol, int nCols) =>
            CreateFromArray(arr, allRows, startRow, nRows, startCol, nCols, nCols, nRows);

      public static Tensor<τ,α> CreateFromSpan(Span<τ> slice, int nRows) {
         int nCols = slice.Length / nRows;
         var tnr = new Tensor<τ,α>(new int[2] {nRows, nCols});
         for(int i = 0; i < nRows; ++i)
            for(int j = 0; j < nCols; ++j)
               tnr[i,j] = slice[i*nCols + j];
         return tnr;
      }
      public static Tensor<τ,α> CreateFromFlatSpec(Span<τ> slice, params int[] structure) {
         int tnrRank = structure.Length;
         TB.Assert.True(tnrRank > 1, "You are trying to create a rank 1 tensor = vector. This method is not intended for creation of vectors.");
         return Recursion(slice, 0);

         Tensor<τ,α> Recursion(Span<τ> slc, int dim) {       // Specifiy slice and the structure dimension (natural rank index) to which it belongs.
            int trueRank = NatRankToTrueRank(structure.Length, dim);
            int nIter = structure[dim];                              // As many iterations as it is the size of the dimension.
            int nEmtsInSlice = slc.Length / nIter;
            if(trueRank > 1) {
               var res = new Tensor<τ,α>(structure, trueRank, null, structure[dim]);
               for(int i = 0; i < nIter; ++i) {                      // Over each tensor. Create new slices and run recursion on them.
                  var newSlc = slc.Slice(i*nEmtsInSlice, nEmtsInSlice);
                  var newTnr = Recursion(newSlc, dim + 1);
                  newTnr.Sup = res;
                  newTnr.Structure = structure;
                  res.Add(i, newTnr); }
               return res; }
            else                                                  // We are at rank 1 = vector rank.
               return Vector<τ,α>.CreateFromSpan(slc);
         }
      }
      /// <summary>Transforms from natural rank index (in the order as written by hand, e.g. A^ijk ==> i -> 0, k -> 2) to true rank index (as situated in the hierarchy, e.g. i from previous example has index 2, k has 0).</summary>
      /// <param name="trueInx">Rank index as situated in the hierarchy. Higher number equates to being higher in the hierarchy.</param>
      int ToNatRank(int trueInx) =>
         Structure.Length - trueInx;
      /// <summary>Transforms from true rank index (as situated in the hierarchy, i.e. higher number equates to being higher in the hierarchy) to true rank index (in the order as written by hand, e.g. A^ijk ==> i -> 0, k -> 2).</summary>
      /// <param name="naturalInx">Rank index as written by hand, e.g. A^ijk ==> i -> 0, k -> 2.</param>
      /// <remarks>Implementation is actually identical to the one in the ToNaturalInx method.</remarks>
      int ToTrueRank(int natInx) => NatRankToTrueRank(Structure.Length, natInx);
      static int NatRankToTrueRank(int nRanks, int natInx) =>
         nRanks - natInx;
      public Tensor<τ,α> this[uint overloadDummy, params int[] inx] {
         get {
            Tensor<τ,α> tnr = this;
            for(int i = 0; i < inx.Length; ++i) {
               if(!tnr.TryGetValue(inx[i], out tnr))
                  return null; }
            return tnr; }                                // No problem with tnr being null. We return above.
         set {
            Tensor<τ,α> tnr = this;
            if(value != null) {
               int n = inx.Length - 1;
               for(int i = 0; i < n; ++i) {
                  if(!tnr.TryGetValue(inx[i], out tnr))
                     tnr = new Tensor<τ,α>(Structure, tnr.Rank - 1, tnr, 6);
                     tnr.Sup.Add(inx[i], tnr); }
               var dict = (TensorBase<Tensor<τ,α>>) tnr;
               dict[inx[n]] = value; }
            else {
               for(int i = 0; i < inx.Length; ++i) {
                  if(!tnr.TryGetValue(inx[i], out tnr))
                     return; }
               tnr.Sup.Remove(inx[inx.Length - 1]); } }
      }
      public Vector<τ,α> this[short overloadDummy, params int[] inx] {
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
                     tnr.Sup.Add(inx[i], tnr); }}
               var dict = (TensorBase<Tensor<τ,α>>) tnr;                      // We do not check that it is a vector beforehand. It is assumed that the user used indexer correctly.
               dict[inx[n]] = value; }
            else {
               int n = inx.Length;                 // Last chooses vector.
               for(int i = 0; i < n; ++i) {
                  if(!tnr.TryGetValue(inx[i], out tnr))
                     return; }
               tnr.Sup.Remove(inx[n - 1]); } }     // Vector.Superior.Remove
      }
      public virtual τ this[params int[] inx] {
         get {
            Tensor<τ,α> tnr = this;
            int n = inx.Length - 2;
            for(int i = 0; i < n; ++i) {
               if(!tnr.TryGetValue(inx[i], out tnr))
                  return default; }
            if(tnr.TryGetValue(inx[n], out tnr)) {         // No probelm with null.
               var vec = (Vector<τ,α>)tnr;                  // Same.
               vec.Vals.TryGetValue(inx[n + 1], out τ val);
               return val; }
            else
               return default; }
         set {
            Tensor<τ,α> tnr = this;
            Tensor<τ,α> tnr2;                               // Temporary to avoid null problem below.
            Vector<τ,α> vec;
            if(!value.Equals(default)) {
               if(inx.Length > 1) {                         // At least a 2nd rank tensor.
                  int n = inx.Length - 2;
                  for(int i = 0; i < n; ++i) {                    // This loop is entered only for a 3rd rank tensor or above.
                     if(tnr.TryGetValue(inx[i], out tnr2)) {
                        tnr = tnr2; }
                     else {
                        tnr = new Tensor<τ,α>(Structure, tnr.Rank - 1, tnr, 6);
                        tnr.Sup.Add(inx[i], tnr); }}
                  if(tnr.TryGetValue(inx[n], out tnr2)) {                  // Does vector exist?
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
      /// <summary>Plus operator for two tensors.</summary><param name="tnr1">Left operand.</param><param name="tnr2">Right operand.</param>
      public static Tensor<τ,α> operator + (Tensor<τ,α> tnr1, Tensor<τ,α> tnr2) {
         TB.Assert.True(tnr1.Structure.Equals<int, IA>(tnr2.Structure));
         return Recursion(tnr1, tnr2);

         Tensor<τ,α> Recursion(Tensor<τ,α> t1, Tensor<τ,α> t2) {
            var res = new Tensor<τ,α>(t2, CopySpecs.AddSubtract);            // Must be a deep copy with a bit of extra capacity.
            if(t1.Rank > 2) {
               foreach(var int_tnr1 in t1) {
                  if(t2.TryGetValue(int_tnr1.Key, out var tnr2Val)) {
                     var subRes = Recursion(int_tnr1.Value, tnr2Val);
                     res.Add(int_tnr1.Key, subRes); } } }
            else {
               foreach(var int_tnr1 in t1) {
                  var vec1 = (Vector<τ,α>) int_tnr1.Value;
                  if(t2.TryGetValue(int_tnr1.Key, out var tnr2Val)) {      // Entry exists in t2, we must sum.
                     var vec2 = (Vector<τ,α>) tnr2Val;
                     var resAsBase = (TensorBase<Tensor<τ,α>>)res;
                     resAsBase[int_tnr1.Key] = vec1 + vec2; }
                  else {
                     res.Add(int_tnr1.Key, int_tnr1.Value); } } }          // Entry does not exist in t2, simply Add.
            return res;
         }
      }
      /// <summary>Multiply tensor with a scalar.</summary>
      /// <param name="scal">Scalar.</param>
      /// <param name="tnr">Tensor.</param>
      public static Tensor<τ,α> operator * (τ scal, Tensor<τ,α> tnr) {
         return Recursion(tnr);

         Tensor<τ,α> Recursion(in Tensor<τ,α> src) {
            var res = new Tensor<τ,α>(src, CopySpecs.ScalarMultiply);            // We copy only meta fields (whereby we copy Structure by value).
            if(src.Rank > 2) {                                       // Subordinates are tensors.
               foreach (var kv in src)
                  res.Add(kv.Key, Recursion(kv.Value)); }
            else if(src.Rank == 2) {                                 // Subordinates are vectors.
               foreach (var kv in src)
                  res.Add(kv.Key, scal*((Vector<τ,α>) kv.Value)); }
            else
               throw new InvalidOperationException(
                  "Tensors's rank has to be at least 2 to be copied via this method.");
            return res;
         }
      }
      ///// <summary>Minus operator for two tensors.</summary><param name="tnr1">Left operand.</param><param name="tnr2">Right operand.</param>
      //public static Tensor<τ,α> operator - (Tensor<τ,α> tnr1, Tensor<τ,α> tnr2) {
      //   TB.Assert.True(tnr1.Structure.Equals<int, IA>(tnr2.Structure));
      //   return Recursion(tnr1, tnr2);

      //   Tensor<τ,α> Recursion(Tensor<τ,α> t1, Tensor<τ,α> t2) {
      //      Tensor<τ,α> res = new Tensor<τ,α>(t1, t1.Count + 4);        // Must be a copy.
      //      if(t1.Rank > 2) {
      //         foreach(var int_tnr2 in t2) {
      //            if(t1.TryGetValue(int_tnr2.Key, out var tnr2Val)) {
      //               var subRes = Recursion(int_tnr2.Value, tnr2Val);
      //               res.Add(int_tnr2.Key, subRes); } } }
      //      else {
      //         foreach(var int_tnr1 in t2) {
      //            if(t2.TryGetValue(int_tnr1.Key, out var tnr2Val)) {
      //               var vec1 = (Vector<τ,α>) int_tnr1.Value;
      //               var vec2 = (Vector<τ,α>) tnr2Val;
      //               res.Add(int_tnr1.Key, vec1 + vec2); } } }
      //      return res;
      //   }
      //}
      /// <summary>Calculates tensor product of this tensor (left-hand operand) with another tensor (right-hand operand).</summary>
      /// <param name="tnr2">Right-hand operand.</param>
      public Tensor<τ,α> TnrProduct(Tensor<τ,α> tnr2) {  // TODO: Implement Tensor Product on tensor.
         throw new NotImplementedException();
      }
      /// <summary>Eliminates a single rank out of a tensor by choosing a single subtensor at that rank and making it take the place of its direct superior (thus discarding all other subtensors at that rank). The resulting tensor has therefore its rank reduced by one.</summary>
      /// <param name="natElimRank">Index of rank to be eliminated in natural notation.</param>
      /// <param name="emtInx">Element index in that rank.</param>
      public Tensor<τ,α> ElimRank(int natElimRank, int emtInx) {
         // 1) Create a new structure. New tensor's rank will be one less. Skip the eliminated rank.
         // 2) Start copying recursively until you reach one rank above the rank to be eliminated. When copying, lower the Rank of each tensor by 1.
         // 3) Do the following for each tensor ('tnr1') situated one rank above the rank to be eliminated with superior 'sup1': First, remove 'tnr1' from 'sup1', then pick a tensor one rank below at element index 'emtInx' ('tnr2') and assign all its subordinates to 'sup1' at the same indices. For the reassigned tensors, do not touch their rank values (or of any tensors below it).
         // 4) You will have to handle a special case when the rank being eliminated is the value rank (lowest lying rank).
         // 5) There, you will have to recreate 'sup1' as a vector. And add to its Vals field.
         // Notice: What about when you have a tensor of 2nd rank? Or when the rank being eliminated is the top rank?
         int elimRank = ToTrueRank(natElimRank);
         TB.Assert.True(elimRank > 0, "You can only eliminate a rank above 0.");
         var newStructureL = Structure.Take(natElimRank);
         var newStructureR = Structure.Skip(natElimRank + 1);
         var newStructure = newStructureL.Concat(newStructureR).ToArray();    // Created a new structure. Assign it to new host tensor.
         if(elimRank > 1)
            return Recursion2(this);
         else
            return Recursion1(this);

         Tensor<τ,α> Recursion2(Tensor<τ,α> src) {                          // When elimRank is at least 2.
            if(src.Rank > elimRank + 1) {
               var res = new Tensor<τ,α>(newStructure, src.Rank - 1, src.Sup, src.Count);
               if(src.Rank > elimRank + 2) {                              // More than 2 above elimRank.
                  foreach(var int_tnr in src)
                     res.Add(int_tnr.Key, Recursion2(int_tnr.Value)); }
               else {                                                      // 2 above elimRank.
                  foreach(var int_tnr in src) {
                     var subTnr = Recursion2(int_tnr.Value);
                     res.Remove(int_tnr.Key);
                     if(subTnr != null)
                        res.Add(int_tnr.Key, subTnr); } }
               return res; }
            else {                                                         // 1 above elimRank.
               if(src.TryGetValue(emtInx, out var selectedTnr)) {
                  var tnrCopy = new Tensor<τ,α>(selectedTnr, in CopySpecs.ElimRank);          // Copy the rest deeply, with rank.
                  tnrCopy.Structure = newStructure;
                  tnrCopy.Sup = src.Sup ?? null;
                  return tnrCopy; }
               else 
               return null; }
            // var res = new Tensor<τ,α>(src.Count);                 // We have to copy the superior and capacity.
            // res.Sup = src.Sup ?? null;
            // res.Rank = src.Rank - 1;                                       // Reduce the rank by 1.
            // res.Structure = newStructure;                                  // Assign new structure  by ref.
            // if(src.Rank > elimRank + 1) {                                   // If we are still above 'sup1'. Can happen only on a rank 3 tensor.
            //    foreach(var int_tnr in src)
            //       res.Add(int_tnr.Key, Recursion2(int_tnr.Value));
            //    return res; }
            // else {                                                            // We are at level of 'sup1'.
            //    if(src.TryGetValue(emtInx, out Tensor<τ,α> elimTnr)) {         //Elim rank is guaranteed to be at least 2 so we can do all reassignments through the lens of Tensor class.
            //       foreach(var int_tnr in elimTnr) {
            //          var tnrAtEmt = new Tensor<τ,α>(int_tnr.Value, in CopySpecs.ElimRank);   // Copy the rest deeply with vals and rank.
            //          tnrAtEmt.Structure = newStructure;                                      // Specify new structure.
            //          tnrAtEmt.Sup = res;                                                     // And new superior.
            //          res.Add(int_tnr.Key, tnrAtEmt); } }
            //    return res; }
         }

         Tensor<τ,α> Recursion1(Tensor<τ,α> src) {         // When elimRank is 1.
            if(src.Rank > 2) {
               var res = new Tensor<τ,α>(newStructure, src.Rank - 1, src.Sup, src.Count);
               if(src.Rank > 3) {
                  foreach(var int_tnr in src)
                     res.Add(int_tnr.Key, Recursion1(int_tnr.Value)); }
               else {
                  foreach(var int_tnr in src) {
                     var subTnr = Recursion1(int_tnr.Value);
                     res.Remove(int_tnr.Key);
                     if(subTnr != null)
                        res.Add(int_tnr.Key, subTnr); } }
               return res; }
            else {                                       // src.Rank = 2 Remove this rank 2 from src.Sup. Choose only one vector from src and add it.
               if(src.TryGetValue(emtInx, out var selectedVec)) {
                  var vecCopy = new Vector<τ,α>((Vector<τ,α>) selectedVec, in CopySpecs.ElimRank);          // Copy the rest deeply, with rank.
                  vecCopy.Structure = newStructure;
                  vecCopy.Sup = src.Sup ?? null;
                  return vecCopy; }
               else 
                  return null; }
         }
      }
      /// <summary>Contracts  two tensors over specified (rank) indices. Indices are specified intuitively - in the order they are written out (sacrificing consistency with regards to rank indexing in this class, chosen so that the value rank has the lowest index (zero). CLarification by example: Contraction writen as A^(ijkl)B^(mnip) is specified as a (0,2) contraction of A and B (not a (3,1) contraction).</summary>
      /// <param name="inx1">Index on this tensor over which to to contract.</param>
      /// <param name="tnr2">Other tensor.</param>
      /// <param name="inx2">Index on other tensor over which to contract (the rank on tnr2 that this index represents must have the same dimension as the rank on tnr1 represented by inx1).</param>
      /// <remarks>Tensor contraction is a generalization of trace, which can further be viewed as a generalization of dot product.</remarks>
      public Tensor<τ,α> Contract(int inx1, Tensor<τ,α> tnr2, int inx2) {
         // 1) Take into account: What if the tensors are both part of another higher rank tensor and have the contraction indices specified relative to them?
         // 2) Decision: Returned contracted tensor will be its own independent tensor (it will be top rank).
         // 3) Note: It is most intuitive to treat the two tensors being contracted (one of rank R1, another of rank R2) as one tensor of rank R1+ R2, where you properly remap the rank indices and do the multiplication on rank 0 elements on demand.
         // 4) On the other hand, you do not want to use the indexer to perform the calculation. It's best to use the enumerator. Therefore, the approach in point 3 is not good.
         // 5) Write an Absorb method where you specify the rank index at which to absorb and an index of the tensor inside that rank that will get absorbed (thus, all other tensors at that rank being discarded).
         throw new NotImplementedException();
         int hostTopRank1 = Structure.Length - 1,                                      // host = tensor at the top of the hierarchy.
             hostTopRank2 = tnr2.Structure.Length - 1,
             hostRankDif1 = hostTopRank1 - Rank,
             hostRankDif2 = hostTopRank2 - tnr2.Rank;
         int[] structure1, structure2;                                                 // Reshaped as if tnr1 and tnr2 were top-most tensors.
         if(hostRankDif1 > 0)
            structure1 = Structure.Skip(hostRankDif1).ToArray();                       // Create a new, trimmed Structure array for tnr1.
         else
            structure1 = Structure;
         if(hostRankDif2 > 0)
            structure2 = tnr2.Structure.Skip(hostRankDif2).ToArray();
         else
            structure2 = tnr2.Structure;
         TB.Assert.AreEqual(structure1[inx1], structure2[inx2],                        // Check that the dimensions of contracted ranks are equal.
            "Rank dimensions at specified indices must be equal when contracting.");
         int cDim = Structure[inx1];            // Dimension of rank we're contracting.
         //var parSeq1 = Enumerable.Range(0, dim).Select(i => {
         //      Array.Copy(Structure, new int[nRanks1], nRanks1)
         //   }
         for(int i = 0; i < cDim; ++i) {

         }
      }
      /// <summary>Check two tensors for equality.</summary><param name="tnr2">Other tensor.</param>
      public bool Equals(Tensor<τ,α> tnr2) {
         Structure.Equals<int, IntArithmetic>(tnr2.Structure);
         return TnrRecursion(this, tnr2);

         bool TnrRecursion(Tensor<τ,α> sup1, Tensor<τ,α> sup2) {
            if(sup1.Rank > 2) {
               foreach(var inx_subTnr1 in sup1) {
                  if(sup2.TryGetValue(inx_subTnr1.Key, out var subTnr2))
                     return TnrRecursion(inx_subTnr1.Value, subTnr2);
                  else
                     return false; } }
            else
               return VecRecursion(sup1, sup2);
            throw new InvalidOperationException("We shouldn't be here.");
         }

         bool VecRecursion(Tensor<τ,α> sup1, Tensor<τ,α> sup2) {
            foreach(var inx_subTnr1 in sup1) {
               var vec = (Vector<τ,α>) inx_subTnr1.Value;
               sup2.TryGetValue(inx_subTnr1.Key, out var subTnr2);
               var vec2 = (Vector<τ,α>) subTnr2;
               if(!vec.Equals(vec2))
                  return false; }
            return true;
         }
      }

      public bool Equals(Tensor<τ,α> tnr2, τ eps) {
         Structure.Equals<int, IntArithmetic>(tnr2.Structure);
         return TnrRecursion(this, tnr2);

         bool TnrRecursion(Tensor<τ,α> sup1, Tensor<τ,α> sup2) {
            if(sup1.Rank > 2) {
               foreach(var inx_subTnr1 in sup1) {
                  if(sup2.TryGetValue(inx_subTnr1.Key, out var subTnr2))
                     return TnrRecursion(inx_subTnr1.Value, subTnr2);
                  else
                     return false; } }
            else
               return VecRecursion(sup1, sup2);
            throw new InvalidOperationException("We shouldn't be here.");
         }

         bool VecRecursion(Tensor<τ,α> sup1, Tensor<τ,α> sup2) {
            foreach(var inx_subTnr1 in sup1) {
               var vec = (Vector<τ,α>) inx_subTnr1.Value;
               sup2.TryGetValue(inx_subTnr1.Key, out var subTnr2);
               var vec2 = (Vector<τ,α>) subTnr2;
               if(!vec.Equals(vec2, eps))
                  return false; }
            return true;
         }                                                         // All values agree within tolerance.
      }
      #if false // TODO: Implement Merge, Swap and ToString on Tensor.
      /// <summary>Append specified vector to caller.</summary>
      /// <param name="appTnr">Vector to append.</param>
      public void MergeWith(Tensor<τ> appTnr) {
         Dim += appTnr.Dim;                                      // Readjust width.
         foreach(var kvPair in appTnr)
            this[kvPair.Key] = kvPair.Value;
      }
      /// <summary>Swap two R1 elements specified by indices.</summary>
      /// <param name="inx1">First element index.</param>
      /// <param name="inx2">Second element index.</param>
      public void Swap(int inx1, int inx2) {
         bool firstExists = TryGetValue(inx1, out τ val1);
         bool secondExists = TryGetValue(inx2, out τ val2);
         if(firstExists) {
            if(secondExists) {
               base[inx1] = val2;
               base[inx2] = val1; }
            else {
               Remove(inx1);                                   // Element at inx1 becomes 0 and is removed.
               Add(inx2, val1); } }
         else if(secondExists) {
            Add(inx1, val2);
            Remove(inx2); }                                   // Else nothing happens, both are 0.
      }
      /// <summary>Apply element swaps as specified by a swap vector.</summary>
      /// <param name="swapVec">Vector where an element at index i with integer value j instructs a permutation i->j.</param>
      public void ApplySwaps(Tensor<int> swapVec) {
         foreach(var kVPair in swapVec)
            Swap(kVPair.Key, kVPair.Value);   
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
      #endif
      /// <summary>Structure that tells Tensor's Copy method how to copy a tensor.</summary>
      public readonly struct CopySpecStruct {
         /// <summary>General CopySpec specifies whether to copy only values, only meta fields or both. Contains flags: Meta (copy meta fields, specify which in MetaFields), Vals (copy values, either shallowly or deeply, specify with EndRank).</summary>
         public readonly GeneralSpecs General;
         /// <summary>MetaFields CopySpec specifies which meta fields to copy. Contains flags: Structure, Rank, Superior.</summary>
         /// <remarks>Effective only if General.Meta flag is active.</remarks>
         public readonly MetaSpecs MetaFields;
         /// <summary>Structure CopySpec specifies whether  to genuinely copy the Structure int[] array or only copy the reference.  Contains flags: TrueCopy (creates a new Structure array on the heap), RefCopy (copies only a reference to the Structure array on source).</summary>
         /// <remarks>Effective only if MetaFields.Structure flag is active.</remarks>
         public readonly StructureSpecs Structure;
         /// <summary>Lowest rank at which copying of values stops.</summary>
         /// <remarks>Effective only if General.Vals flag is active. EndRank is an inclusive lower bound.</remarks>
         public readonly int EndRank;
         public readonly int ExtraCapacity;
         
         public CopySpecStruct(GeneralSpecs gs = GeneralSpecs.Both, MetaSpecs ms = MetaSpecs.All,
         StructureSpecs ss = StructureSpecs.TrueCopy, int endRank = 0, int extraCapacity = 0) {
            General = gs;
            MetaFields = ms;
            Structure = ss;
            EndRank = endRank;
            ExtraCapacity = extraCapacity;
         }
      }
      /// <summary>Possible CopySpec settings.</summary>
      [Flags] public enum GeneralSpecs {
         Meta  = 1,
         Vals  = 1 << 1,
         Both = Meta | Vals
      }
      /// <summary>Possible StructureCopySpec settings.</summary>
      [Flags] public enum StructureSpecs {
         TrueCopy = 1,
         RefCopy  = 1 << 1
      }
      /// <summary>Possible MetaCopySpec settings.</summary>
      [Flags] public enum MetaSpecs {
         None        = 0,
         Structure   = 1,
         Rank        = 1 << 1,
         Sup         = 1 << 2,
         All = Structure | Rank | Sup,
         AllExceptSup = All & ~Sup
      }
   }
}