using System;
using System.Globalization;
using static System.Math;

using Fluid.Internals.Mesh;

namespace Fluid.Internals.Numerics {
   /// <summary>A 2D vector in Cartesian coordinates.</summary>
   public struct Vec2 {
      public static readonly Vec2 NullVector = new Vec2();
      /// <summary>X component.</summary>
      public double _X;
      /// <summary>Y component.</summary>
      public double _Y;

      /// <summary>Create a 2D vector with specified x and y components.</summary><param name="x">X component.</param><param name="y">Y component.</param>
      public Vec2(double x, double y) {
         _X = x;
         _Y = y;
      }
      /// <summary>Create a 2D position vector from pos1 to pos2.</summary><param name="pos1">Position at tail.</param><param name="pos2">Position at head.</param>
      public Vec2(in Pos pos1, in Pos pos2) {
         _X = pos2.X - pos1.X;
         _Y = pos2.Y - pos1.Y;
      }

      /// <summary>Euclidian norm.</summary>
      public double Norm() => Sqrt(_X*_X + _Y*_Y);
      /// <summary>Square of Euclidian norm.</summary>
      public double NormSqd() => _X*_X + _Y*_Y;
      /// <summary>Trims this vector to unit size.</summary>
      public void Normalize() {
         double norm = Norm();
         _X /= norm;
         _Y /= norm;
      }
      /// <summary>Calculate cross product in 2D with result pointing in z direction and having the calculated magnitude.</summary><param name="other">Vector on right side of cross product.</param>
      public double Cross(in Vec2 other) => _X*other._Y - _Y*other._X;
      
      public override string ToString() => $"{{{_X.ToString(CultureInfo.InvariantCulture)}, {_Y.ToString(CultureInfo.InvariantCulture)}}}";
   }
}