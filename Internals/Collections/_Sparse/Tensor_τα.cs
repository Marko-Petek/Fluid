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
      /// <summary>Hierarchy's dimensional structure. E.g.: {3,2,6,5} specifies structure of a tensor of 4th rank with first rank equal to 5 and last rank to 3.</summary>
      public int[] Structure { get; protected set; }
      /// <summary>Rank specifies the height (level) in the hierarchy on which the tensor sits. It equals the number of levels that exist below it. It tells us how many indices we must specify before we reach the value level.</summary>
      public int Rank { get; protected set; }
      /// <summary>Superior: a tensor directly above in the hierarchy. Null if this is the highest rank tensor.</summary>
      public Tensor<τ,α> Sup { get; protected set; }

      public static readonly CopySpecs DefaultCopySpec = new CopySpecs();
      public static CopySpecs CopySpec = DefaultCopySpec;
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
      public Tensor(Tensor<τ,α> sup, int cap) : this(sup.Structure, sup.Rank - 1, sup, cap) { }
      /// <summary>Creates a deep copy of specified tensor. You can optionally specify which meta-fields (Structure, Rank ...) to copy.</summary>
      /// <param name="src">Source tensor to copy.</param>
      /// <param name="extraCap">How much more top rank space should the new tensor have (newCap = oldCap + extraCap).</param>
      /// <param name="cs">Exact specification of fields to copy. Default is all.</param>
      public Tensor(Tensor<τ,α> src, int extraCap, CopySpecs cs = CopySpecs.All) :
       base(src.Count + extraCap) {
         Copy(src, this, cs);
      }
      /// <summary>Creates a tensor as a deep copy of this one: same Rank, Structure and Superior.</summary>
      public virtual Tensor<τ,α> Copy() {
         var res = new Tensor<τ,α>(Count);
         Copy(this, res);
         return res;
      }
      /// <summary>Make a shallow or deep copy of a tensor. Set CopySpec field for fine tunning.</summary>
      /// <param name="src">Copy source.</param>
      /// <param name="tgt">Copy target.</param>
      /// <param name="cs">Exact specification of what fields to copy. Default is all.</param>
      public static void Copy(in Tensor<τ,α> src, Tensor<τ,α> tgt) {
         TB.Assert.True(src.Rank > 1,
            "Tensors's rank has to be at least 2 to be copied via this method.");
         CopyMetaFields(src, tgt, in CopySpec.MCSpec, in CopySpec.SCSpec);
         if((CopySpec.MSpec & MainSpecs.Vals) == MainSpecs.Vals) {
            int endRank = CopySpec.EndRank;
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
               if(inx.Length > 1) {                         // At least a 2nd rank tensor. TODO: Override on vector.
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
            var res = new Tensor<τ,α>(t2, t2.Count + 4, CopySpecs.All);                // Must be a copy.
            if(t1.Rank > 2) {
               foreach(var int_tnr1 in t1) {
                  if(t2.TryGetValue(int_tnr1.Key, out var tnr2Val)) {
                     var subRes = Recursion(int_tnr1.Value, tnr2Val);
                     res.Add(int_tnr1.Key, subRes); } } }
            else {
               foreach(var int_tnr1 in t1) {
                  if(t2.TryGetValue(int_tnr1.Key, out var tnr2Val)) {
                     var vec1 = (Vector<τ,α>) int_tnr1.Value;
                     var vec2 = (Vector<τ,α>) tnr2Val;
                     res.Add(int_tnr1.Key, vec1 + vec2); } } }
            return res;
         }
      }
      /// <summary>Multiply tensor with a scalar.</summary>
      /// <param name="scl">Scalar.</param>
      /// <param name="tnr">Tensor.</param>
      public static Tensor<τ,α> operator * (τ scl, Tensor<τ,α> tnr) {

         Tensor<τ,α> Recursion(Tensor<τ,α> src) {
            var res = new Tensor<τ,α>(src, 0, CopySpecs.AllExceptVals);  // TODO: wE NEED A SHALLOW COPY.
            if(src.Rank > 2) {                                       // Subordinates are tensors.
               foreach (var kv in src) {
                   = new Tensor<τ,α>(kv.Value.Structure, kv.Value.Rank, kv.Value.Sup, kv.Value.Count);
                  var subTnr = Recursion(kv.Value);
                  subTnr.Structure = kv.Value.Structure;
                  subTnr.Sup = res;

                  tgt1.Add(kv.Key, subTnr); } }
            else if(src.Rank == 2) {                                 // Subordinates are vectors.
               foreach (var kv in src)
                  tgt1.Add(kv.Key, new Vector<τ,α>((Vector<τ,α>) kv.Value)); }
            else
               throw new InvalidOperationException(
                  "Tensors's rank has to be at least 2 to be copied via this method.");
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
      public Tensor<τ,α> TnrProduct(Tensor<τ,α> tnr2) {

      }
      /// <summary>Contracts (generalized trace) two tensors over specified ranks. Ranks are specified intuitively - in the order indices are written out (sacrificing formal correctness), e.g.: (ijkl)(mnip) is specified as a (0,2) contraction, not a (3,1) contraction.</summary>
      /// <param name="inx1">Index (rank) on this tensor over which to to contract.</param>
      /// <param name="tnr2">Other tensor.</param>
      /// <param name="inx2">Index (rank) on other tensor over which to contract.</param>
      /// <returns></returns>
      public Tensor<τ,α> Contract(int inx1, Tensor<τ,α> tnr2, int inx2) {
         TB.Assert.AreEqual(Structure[inx1], tnr2.Structure[inx2],
            "Rank dimensions at specified indices must be equal when contracting.");
         int nRanks1 = Structure.Length;
         int nRanks2 = tnr2.Structure.Length;
         int dim = Structure[inx1];             // Dimension of rank we're contracting.
         //var parSeq1 = Enumerable.Range(0, dim).Select(i => {
         //      Array.Copy(Structure, new int[nRanks1], nRanks1)
         //   }
         for(int i = 0, n = dim - 1; i < n; ++i) {

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

      public readonly struct CopySpecs {
         /// <summary>General CopySpec with available options: Meta (copy meta fields, specify which in MCSpec) and Vals (copy values, either shallowly or deeply - specify with VCDepth).</summary>
         public readonly MainSpecs MSpec;
         /// <summary>Spcify which meta fields to copy (effective only if CSpec.Meta = 1): Structure, Rank, Superior.</summary>
         public readonly MetaSpecs MCSpec;
         /// <summary>Specify how to copy Structure field (effective only if MCSpec.Structure = 1): TrueCopy (create a new Structure array on heap), RefCopy (copy only a reference to Structure array on source Tensor.)</summary>
         public readonly StructureSpecs SCSpec;
         /// <summary>Rank at which copying stops (inclusive bound, effective only if CSPec.Vals = 1)</summary>
         public readonly int EndRank;
         
         public CopySpecs(MainSpecs ms = MainSpecs.All, MetaSpecs mcs = MetaSpecs.All,
         StructureSpecs scs = StructureSpecs.TrueCopy, int endRank = 0) {
            MSpec = ms;
            MCSpec = mcs;
            SCSpec = scs;
            EndRank = endRank;
         }
      }
      /// <summary>Possible CopySpec settings.</summary>
      [Flags] public enum MainSpecs {
         Meta  = 1,
         Vals  = 1 << 1,
         All = Meta | Vals
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