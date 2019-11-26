using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using static System.Math;
using dbl = System.Double;
namespace Fluid.Internals.Numerics {

/// <summary>A 2D vector in Cartesian coordinates.</summary>
public readonly struct Vec2 : IEquatable<Vec2> {
   public static readonly Vec2 NullVector = new Vec2();
   /// <summary>X component.</summary>
   public readonly double X;
   /// <summary>Y component.</summary>
   public readonly double Y;


   /// <summary>Create a 2D vector with specified x and y components.</summary><param name="x">X component.</param><param name="y">Y component.</param>
   public Vec2(double x, double y) {
      X = x;
      Y = y;
   }
   public Vec2(in dbl[] p) {
      X = p[0];
      Y = p[1];
   }
   /// <summary>Creates a copy of a given 2D vector.</summary>
   /// <param name="vec"></param>
   public Vec2(in Vec2 vec) {
      X = vec.X;
      Y = vec.Y;
   }
   /// <summary>Create a 2D position vector from pos1 to pos2.</summary><param name="pos1">Position at tail.</param><param name="pos2">Position at head.</param>
   public Vec2(in Vec2 pos1, in Vec2 pos2) {
      X = pos2.X - pos1.X;
      Y = pos2.Y - pos1.Y;
   }

   /// <summary>Creates a rank 1, 2D double array from a Vec2.</summary>
   public dbl[] ToArray() => new dbl[2] {X,Y};
   /// <summary>Euclidian norm.</summary>
   public double Norm() => Sqrt(X*X + Y*Y);
   /// <summary>Square of Euclidian norm.</summary>
   public double NormSqd() => X*X + Y*Y;
   /// <summary>Trims this vector to unit size.</summary>
   public Vec2 Normalize() {
      double norm = Norm();
      return new Vec2(X/norm, Y/norm);
   }
   /// <summary>Calculate cross product in 2D with result pointing in z direction and having the calculated magnitude.</summary><param name="other">Vector on right side of cross product.</param>
   public double Cross(in Vec2 other) => X*other.Y - Y*other.X;

   /// <summary>Check's whether this Position is inside a simple polygon defined by vertices specified in CCW direction.</summary>
   /// <param name="vertices">Positions of vertices in CCW direction.</param>
   /// <remarks><see cref="TestRefs.PointInsidePolygon"/></remarks>
   public bool IsInsidePolygon(params Vec2[] vertices) {
      int oddIfTrue = 0;
      var x = X;
      var y = Y;
      int nVertices = vertices.Length;
      for(int vtx = 0; vtx < nVertices - 1; ++vtx)
         ActOnCondition(in vertices[vtx], in vertices[vtx+1]);
      ActOnCondition(in vertices[nVertices-1], in vertices[0]);           // Last edge (from last vertex to first).
      if(oddIfTrue % 2 == 0)
         return false;
      else
         return true;

      void ActOnCondition(in Vec2 vt1, in Vec2 vt2) {
         if(vt1.Y <= y) {
            if(vt2.Y > y && (vt1.X - x)*(vt2.Y - y) - (vt1.Y - y)*(vt2.X - x) > 0)
               ++oddIfTrue; }
         else
            if(vt2.Y <= y && (vt1.X - x)*(vt2.Y - y) - (vt1.Y - y)*(vt2.X - x) < 0)
               --oddIfTrue;
      }
   }
   /// <summary>Returns a non-evaluated delegate.</summary>
   /// <param name="cartArr">Ordered cartesian array.</param>
   public static IEnumerable<Vec2> FromCartesianArray(dbl[][][] cartArr) {
      return cartArr.SelectMany( i => i.Select( j => new Vec2(j) ) );
   }
   public static Vec2 Sum(params Vec2[] vecs) {
      double resX = 0.0, resY = 0.0;
      foreach(var vec in vecs) {
         resX += vec.X;
         resY += vec.Y; }
      return new Vec2(resX, resY);
   }
   public static Vec2 operator +(in Vec2 vec1, in Vec2 vec2) {
      return new Vec2(vec1.X + vec2.X, vec1.Y + vec2.Y);
   }
   public static Vec2 operator -(in Vec2 vec1, in Vec2 vec2) {
      return new Vec2(vec1.X - vec2.X, vec1.Y - vec2.Y);
   }
   public static Vec2 operator /(in Vec2 vec1, in double scal2) {
      return new Vec2(vec1.X/scal2, vec1.Y/scal2);
   }
   public bool Equals(Vec2 vec2) {
      if(X == vec2.X && Y == vec2.Y)
         return true;
      else
         return false;
   }
   public override string ToString() => $"{{{X.ToString(CultureInfo.InvariantCulture)}, {Y.ToString(CultureInfo.InvariantCulture)}}}";
}
}