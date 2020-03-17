using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using dbl = System.Double;

using Fluid.Internals;
using Fluid.Internals.Numerics;
using static Fluid.Internals.Tools;
namespace Fluid.Internals.Collections {
using static TnrFactory;
   
/// <summary>A vector with specified dimension which holds values of type τ. Those can use arithmetic defined inside type α.</summary>
/// <typeparam name="τ">Type of values.</typeparam>
/// <typeparam name="α">Type defining arithmetic between values.</typeparam>
public class RefVec<τ,α> : RefTnr<τ,α>, IEquatable<RefVec<τ,α>>
where τ : class, IEquatable<τ>, IComparable<τ>
where α : IArithmetic<τ?>, new() {

   /// <summary>Void vector.</summary>
   public static readonly RefVec<τ,α> V = RefTnrFactory.TopRefVec<τ,α>(0,0);
   /// <summary>Constructor with redundancy, used internally.</summary>
   /// <param name="strc">Structure (absorbed).</param>
   /// <param name="sup">Direct superior.</param>
   /// <param name="cap">Capacity of internal dictionary.</param>
   internal RefVec(List<int> strc, RefTnr<τ,α>? sup, int cap) :
   base(strc, 1, sup, 0) {                                                     // Zero capacity for dictionary holding tensors.
      Scals = new Dictionary<int, τ>(cap);
   }
   /// <summary>Creates a non-top vector with specified superior and initial capacity. Does not add the new vector to its superior or check whether the superior is rank 2.</summary>
   /// <param name="sup">Direct superior.</param>
   /// <param name="cap">Capacity of internal dictionary.</param>
   internal RefVec(RefTnr<τ,α> sup, int cap) : this(sup.Strc, sup, cap) { }
   
   /// <summary>Creates a top vector (null superior) with specified dimension and initial capacity.</summary>
   /// <param name="dim">Dimension. Number of spots available for values.</param>
   /// <param name="cap">Capacity of internal dictionary.</param>
   internal RefVec(int dim, int cap) : this(new List<int> {dim}, null, cap) { }

   /// <summary>Adds entry to internal dictionary without checking if it is equal to zero.</summary>
   /// <param name="key">Index.</param>
   /// <param name="val">Value.</param>
   internal void Add(int key, τ val) =>
      Scals.Add(key, val);

   internal bool TryGetValue(int inx, out τ? scal) =>
      Scals.TryGetValue(inx, out scal);

   public override int Count => Scals.Count;
   
   /// <summary>Scalars. An extra wrapped Dictionary which holds vector elements.</summary>
   public Dictionary<int,τ> Scals { get; internal set; }


   /// <summary>Indexer. Check for empty parent after operation.</summary>
   new public τ? this[int i] {
      get {
         Scals.TryGetValue(i, out τ? val);
         return val; }                                // Return value here is never null.
      set {
         if(value != null) {
            Scals[i] = value; }
         else
            Scals.Remove(i); }
   }

   
   public τ GetNonNullScal(int inx) {
      if(TryGetValue(inx, out τ? ns))
         return ns!;                         // This has to be non-null at this point.
      else
         throw new NullReferenceException("Expected a non-null tensor at specified location.");
   }

   
   /// <summary>Sum two vectors, return new vector as a result.</summary>
   /// <param name="vec1">Left operand.</param>
   /// <param name="vec2">Right operand.</param>
   public static RefVec<τ,α>? operator + (RefVec<τ,α>? vec1, RefVec<τ,α>? vec2) =>
      vec1.SumTop(vec2);
   
   /// <summary>Subtract two vectors. Returns new top vector as result.</summary>
   /// <param name="vec1">Left operand.</param>
   /// <param name="vec2">Right operand.</param>
   public static RefVec<τ,α>? operator - (RefVec<τ,α>? vec1, RefVec<τ,α>? vec2) =>
      vec1.SubTop(vec2);

   
   
   /// <summary>Negate operator. Creates a new vector with its own substructure.</summary>
   /// <param name="vec">Vector to negate.</param>
   public static RefVec<τ,α>? operator - (RefVec<τ,α>? vec) =>
      vec.Negate();
   public static RefVec<τ,α>? operator * (τ scal, RefVec<τ,α> vec) =>
      vec.MulTop(scal);
   

   
   /// <summary>Dot (scalar) product.</summary>
   public static τ? operator *(RefVec<τ,α>? v1, RefVec<τ,α>? v2) =>
      v1.ContractTop(v2);

   

   public bool Equals(RefVec<τ,α> v2) {
      if(!Scals.Keys.OrderBy(key => key).SequenceEqual(v2.Scals.Keys.OrderBy(key => key)))    // Keys have to match.
         return false;
      foreach(var (i,s1) in Scals) {
         τ s2 = GetNonNullScal(i);                       // If the key check passed, return here cannot be null.
         if(!s1.Equals(s2))        // Fetch did not suceed or values are not equal.
            return false; }
      return true;
   }

   
   /// <summary>So that foreach statements work properly.</summary>
   new public IEnumerator<KeyValuePair<int,τ>> GetEnumerator() {
      foreach(var kv in Scals)
         yield return kv;
   }

   public override string ToString() {
      var sb = new StringBuilder(2*Count);
      sb.Append("{");
      foreach(var emt in Scals) {
         sb.Append($"{emt.ToString()}, ");
      }
      sb.Remove(sb.Length - 2, 2);
      sb.Append("}");
      return sb.ToString();
   }
   /// <summary>Converts a sparse Vector to a regular array.</summary>
   public τ[] ToArray() {
      var arr = new τ[Dim];
      foreach(var (i,s) in Scals)
         arr[i] = s;
      return arr;
   }
}
}