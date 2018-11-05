using System;
using System.Globalization;
using Fluid.Dynamics.Meshing;
using static System.Math;

namespace Fluid.Dynamics.Numerics
{
    /// <summary>A 2D vector in Cartesian coordinates.</summary>
    public struct Vector2
    {
        public static readonly Vector2 NullVector = new Vector2();

        /// <summary>X component.</summary>
        public double _x;
        /// <summary>Y component.</summary>
        public double _y;

        /// <summary>Create a 2D vector with specified x and y components.</summary><param name="x">X component.</param><param name="y">Y component.</param>
        public Vector2(double x, double y) {
            _x = x;
            _y = y;
        }

        /// <summary>Create a 2D vector from pos1 to pos2.</summary><param name="pos1">Position at tail.</param><param name="pos2">Position at head.</param>
        public Vector2(ref Pos pos1, ref Pos pos2) {
            _x = pos2._x - pos1._x;
            _y = pos2._y - pos1._y;
        }

        /// <summary>Euclidian norm.</summary>
        public double Norm() => Sqrt(_x*_x + _y*_y);

        /// <summary>Square of Euclidian norm.</summary>
        public double NormSquared() => _x*_x + _y*_y;

        /// <summary>Trims this vector to unit size.</summary>
        public void Normalize() {
            double norm = Norm();
            _x /= norm;
            _y /= norm;
        }

        /// <summary>Calculate cross product in 2D with result pointing in z direction and having the calculated magnitude.</summary><param name="vector2">Vector on right side of cross product.</param>
        public double Cross(ref Vector2 vector2) => _x * vector2._y - _y * vector2._x;

        public override string ToString() => $"{{{_x.ToString(CultureInfo.InvariantCulture)}, {_y.ToString(CultureInfo.InvariantCulture)}}}";
    }
}