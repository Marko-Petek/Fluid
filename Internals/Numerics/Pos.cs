using System;
using System.Globalization;

namespace Fluid.Internals.Numerics
{
   /// <summary>Holds position as an x-y pair.</summary>
   public struct Pos {
      public double X { get; set; }
      public double Y { get; set; }
      
      /// <summary>Create an x-y value pair.</summary><param name="x">X component.</param><param name="y">Y Component.</param>
      public Pos(double x, double y) {
         X = x;
         Y = y;
      }
      public Pos(in Pos pos) {
         X = pos.X;
         Y = pos.Y;
      }

      public bool Equals(in Pos other) {
         if(X == other.X && Y == other.Y)
            return true;
         else
            return false;
      }
      /// <summary>Check's whether this Position is inside a simple polygon defined by vertices specified in CCW direction.</summary><param name="ll">Position of lower left vertex.</param>
      public bool IsInsidePolygon(Pos[] vertices) {
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

         void ActOnCondition(in Pos vt1, in Pos vt2) {
            if(vt1.Y <= y)
               if(vt2.Y > y && (vt1.X - x)*(vt2.Y - y) - (vt1.Y - y)*(vt2.X - x) > 0)
                  ++oddIfTrue;
            else
               if(vt2.Y <= y && (vt1.X - x)*(vt2.Y - y) - (vt1.Y - y)*(vt2.X - x) < 0)
                  --oddIfTrue;
         }
      }

      public override string ToString() => $"{{{X.ToString(CultureInfo.InvariantCulture)}, {Y.ToString(CultureInfo.InvariantCulture)}}}";
   }
}