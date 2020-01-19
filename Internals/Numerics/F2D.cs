using System;
using static System.Math;
using dbl = System.Double;
using f2da = Fluid.Internals.Numerics.Func2DArithmetic;

namespace Fluid.Internals.Numerics {
   /// <summary>A custom function of 2 arguments on the domain [-1,1]x[-1,1]. Note: always use One as 1 and Zero as 0 because the equality checks for those inside operations rely on reference comparisons.</summary>
   public class F2D : IEquatable<F2D>, IComparable<F2D> {
      public static F2D One { get; } = new F2D((x,y) => 1.0);
      public static F2D Zero { get; } = new F2D((x,y) => 0.0);
      /// <summary>A 3rd order Gauss quadrature integrator.</summary>
      public static Quadrature2D Integrator { get; } = new Quadrature2D(3, One);
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
      public F2D() : this((x,y) => 1.0) {

      }


      public dbl this[dbl x, dbl y] {
         get => F(x,y);
      }
      public static F2D operator *(F2D f1, F2D f2) {                          // FIXME: Streamline Func2D
         if(f1 == One)
            return f2;
         else if(f2 == One) 
            return f1;
         else if(f1 == Zero || f2 == Zero)
            return Zero;
         else
            return new F2D( (x,y) => f1[x,y] * f2[x,y] );
      }
      public static F2D TripProd(F2D f1, F2D f2, F2D f3) {
         if(f1 == One)
            return f2 * f3;
         else if(f2 == One)
            return f1 * f3;
         else if(f3 == One)
            return f1 * f2;
         else if(f1 == Zero || f2 == Zero || f2 == Zero)
            return Zero;
         else
            return new F2D( (x,y) => f1[x,y]*f2[x,y]*f3[x,y] );
      }

      public static F2D QuadProd(F2D f1, F2D f2, F2D f3, F2D f4) {
         if(f1 == One)
            return TripProd(f2, f3, f4);
         else if(f2 == One)
           return TripProd(f1, f3, f4);
         else if(f3 == One)
            return TripProd(f1, f2, f4);
         else if(f4 == One)
            return TripProd(f1, f2, f3);
         else if(f1 == Zero || f2 == Zero || f3 == Zero || f4 == Zero)
            return Zero;
         else
            return new F2D( (x,y) => f1[x,y]*f2[x,y]*f3[x,y]*f4[x,y] );
      }
         

      public static F2D QuintProd(F2D f1, F2D f2, F2D f3, F2D f4, F2D f5) {
         if(f1 == One)
            return QuadProd(f2, f3, f4, f5);
         else if(f2 == One)
           return QuadProd(f1, f3, f4, f5);
         else if(f3 == One)
            return QuadProd(f1, f2, f4, f5);
         else if(f4 == One)
            return QuadProd(f1, f2, f3, f5);
         else if(f5 == One)
            return QuadProd(f1, f2, f3, f4);
         else if(f1 == Zero || f2 == Zero || f3 == Zero || f4 == Zero || f5 == Zero)
            return Zero;
         else
            return new F2D( (x,y) => f1[x,y]*f2[x,y]*f3[x,y]*f4[x,y]*f5[x,y] );
      }
         

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
         if(Norm > f.Norm)
            return 1;
         if(Norm == f.Norm)
            return 0;
         else
            return -1;
      }
   }
}