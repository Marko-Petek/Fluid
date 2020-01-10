using System;
using dbl = System.Double;
using f2da = Fluid.Internals.Numerics.Func2DArithmetic;

namespace Fluid.Internals.Numerics {
   /// <summary>A custom function of 2 arguments-</summary>
   public class Func2D : IEquatable<Func2D>, IComparable<Func2D> {
      public static Func2D One { get; } = new Func2D((x,y) => 1.0);
      public static Func2D Zero { get; } = new Func2D((x,y) => 0.0);
      /// <summary>The Func delegate backup field.</summary>
      Func<dbl,dbl,dbl> _F;
      /// <summary>A (highly-likely) unique ID based on Func return value.</summary>
      public int Id { get; private set; }
      /// <summary>The Func delegate.</summary>
      public Func<dbl,dbl,dbl> F { 
         get => F;
         set {
            _F = value;
            Id = GetHashCode(); }
      }


      /// <summary>Create an identity.</summary>
      public Func2D() : this((x,y) => 1.0) { }
      /// <summary>Create a custom function of 2 arguments-</summary>
      /// <param name="func">Delegate instance.</param>
      public Func2D(Func<dbl,dbl,dbl> func) {
         _F = func;
         Id = GetHashCode();
      }


      public dbl this[dbl x, dbl y] {
         get => F(x,y);
      }

      public static Func2D operator *(Func2D f1, Func2D f2) {                          // FIXME: Streamline Func2D
         if(f1.Equals(One)) {

         }
         return new Func2D( (x,y) => f1[x,y] * f2[x,y] );
      }
      public static Func2D TripProd(Func2D f1, Func2D f2, Func2D f3) =>
         new Func2D( (x,y) => f1[x,y]*f2[x,y]*f3[x,y] );
      public static Func2D QuadProd(Func2D f1, Func2D f2, Func2D f3, Func2D f4) =>
         new Func2D( (x,y) => f1[x,y]*f2[x,y]*f3[x,y]*f4[x,y] );
      public static Func2D QuintProd(Func2D f1, Func2D f2, Func2D f3, Func2D f4, Func2D f5) =>
         new Func2D( (x,y) => f1[x,y]*f2[x,y]*f3[x,y]*f4[x,y]*f5[x,y] );
      /// <summary>Generates a (highly-likely) unique ID based on Func return value.</summary>
      public override int GetHashCode() =>
         (int) (F(1,2) + F(2,3) + F(3,5) + F(5,7) + F(Math.E, Math.PI));
      /// <summary>Compares identities of two functions.</summary>
      /// <param name="f">The other function.</param>
      public bool Equals(Func2D f) {
         if(Id == f.Id)
            return true;
         else
            return false;
      }
      public int CompareTo(Func2D f) {
         throw new NotImplementedException("Cannot (yet) directly compare two functions.");
      }
   }
}