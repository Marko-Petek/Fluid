using System;
using static System.Math;
using dbl = System.Double;
using f2da = Fluid.Internals.Numerics.Func2DArithmetic;

namespace Fluid.Internals.Numerics {
   /// <summary>A custom function of 2 arguments on the domain [-1,1]x[-1,1].</summary>
   public class F2D : IEquatable<F2D>, IComparable<F2D> {
      public static F2D One { get; } = new F2D((x,y) => 1.0);
      public static F2D Zero { get; } = new F2D((x,y) => 0.0);
      /// <summary>A 7th order Gauss quadrature integrator.</summary>
      public static Quadrature2D Integrator { get; } = new Quadrature2D(One);
      bool _FChanged = true;
      dbl _Norm;
      public dbl Norm {
         get {
            if(_FChanged)
               _Norm = CalcNorm();
            return _Norm; }
      }

      int _Id;
      /// <summary>A (highly-likely) unique ID based on Func return value.</summary>
      int Id { 
         get {
            if(_FChanged)
               _Id = GetHashCode();
            return _Id; }
      }
     
      /// <summary>The Func delegate.</summary>
      public Func<dbl,dbl,dbl> F { get; }

      /// <summary>Create a custom function of 2 arguments-</summary>
      /// <param name="func">Delegate instance.</param>
      public F2D(Func<dbl,dbl,dbl> func) {
         F = func;
      }
      /// <summary>Create an identity.</summary>
      public F2D() : this((x,y) => 1.0) {

      }


      public dbl this[dbl x, dbl y] {
         get => F(x,y);
      }

      public static F2D operator *(F2D f1, F2D f2) {                          // FIXME: Streamline Func2D
         if(f1.Equals(One)) {

         }
         return new F2D( (x,y) => f1[x,y] * f2[x,y] );
      }
      public static F2D TripProd(F2D f1, F2D f2, F2D f3) =>
         new F2D( (x,y) => f1[x,y]*f2[x,y]*f3[x,y] );
      public static F2D QuadProd(F2D f1, F2D f2, F2D f3, F2D f4) =>
         new F2D( (x,y) => f1[x,y]*f2[x,y]*f3[x,y]*f4[x,y] );
      public static F2D QuintProd(F2D f1, F2D f2, F2D f3, F2D f4, F2D f5) =>
         new F2D( (x,y) => f1[x,y]*f2[x,y]*f3[x,y]*f4[x,y]*f5[x,y] );

      double CalcNorm() {
         Integrator.F = this * this;                  // Integrate square.
         return Sqrt(Integrator.Integrate());
      }
      /// <summary>Generates a (highly-likely) unique ID based on Func return value. The ID is generated as an integral over the domain.</summary>
      public override int GetHashCode() {
         dbl div = Pow(10, (int) Log10(Norm));
         return (int) (Norm/div * Pow(2,28));           // To get the maximum possible number of digits.
      }
      /// <summary>Compares identities of two functions.</summary>
      /// <param name="f">The other function.</param>
      public bool Equals(F2D f) {
         if(Id == f.Id)
            return true;
         else
            return false;
      }
      public int CompareTo(F2D f) {
         throw new NotImplementedException("Cannot (yet) directly compare two functions.");
      }
   }
}