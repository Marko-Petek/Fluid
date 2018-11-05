using System;
using System.Globalization;

namespace Fluid.Dynamics.Numerics
{
    /// <summary>Holds position as an x-y pair.</summary>
        public struct Pos : IEquatable<Pos>
        {
            public double _x;
            public double _y;
            
            /// <summary>Create an x-y value pair.</summary><param name="x">X component.</param><param name="y">Y Component.</param>
            public Pos(double x, double y) {
                _x = x;
                _y = y;
            }

            public Pos(ref Pos pos) {
                _x = pos._x;
                _y = pos._y;
            }

            public bool Equals(Pos other) {
                if(_x == other._x && _y == other._y) {
                    return true;
                }
                else {
                    return false;
                }
            }

            // /// <summary>Check's whether this Position is inside a simple quadrilateral defined by 4 vertices (Positions) specified in CCW direction.</summary><param name="ll">Position of lower left vertex.</param><param name="lr">Position of lower right vertex.</param><param name="ur">Position of upper right vertex.</param><param name="ul">Position of upper left vertex.</param>
            // public bool IsInsideQuadrilateral(ref Pos ll, ref Pos lr, ref Pos ur, ref Pos ul) {
            //     int oddIfTrue = 0;
            //     var x = _x;
            //     var y = _y;
            //     IncrementIfRayCrossesEdge(ref ll, ref lr);
            //     IncrementIfRayCrossesEdge(ref lr, ref ur);
            //     IncrementIfRayCrossesEdge(ref ur, ref ul);
            //     IncrementIfRayCrossesEdge(ref ul, ref ll);

            //     if(oddIfTrue % 2 == 0) {
            //         return true;
            //     }
            //     else {
            //         return false;
            //     }


            //     void IncrementIfRayCrossesEdge(ref Pos pos1, ref Pos pos2) {
            //         double w = (pos2._y-y) * ((pos1._x-x)*(pos2._y-y) - (pos1._y-y)*(pos2._x-x));
            //         if(w >= 0) {
            //             ++oddIfTrue;
            //         }
            //     }
            // }

            /// <summary>Check's whether this Position is inside a simple polygon defined by vertices specified in CCW direction.</summary><param name="ll">Position of lower left vertex.</param>
            public bool IsInsidePolygon(ref Pos[] vertices) {
                int oddIfTrue = 0;
                var x = _x;
                var y = _y;
                int nVertices = vertices.Length;

                for(int vtx = 0; vtx < nVertices - 1; ++vtx) {
                    ActOnCondition(ref vertices[vtx], ref vertices[vtx+1]);
                }
                ActOnCondition(ref vertices[nVertices-1], ref vertices[0]);                                  // Last edge (from last vertex to first).

                if(oddIfTrue % 2 == 0) {
                    return false;
                }
                else {
                    return true;
                }


                void ActOnCondition(ref Pos vt1, ref Pos vt2) {
                    if(vt1._y <= y) {
                        if(vt2._y > y
                        && (vt1._x - x)*(vt2._y - y) - (vt1._y - y)*(vt2._x - x) > 0)
                            ++oddIfTrue;
                    }
                    else {
                        if(vt2._y <= y
                        && (vt1._x - x)*(vt2._y - y) - (vt1._y - y)*(vt2._x - x) < 0)
                            --oddIfTrue;
                    }
                }
            }

            public override string ToString() => $"{{{_x.ToString(CultureInfo.InvariantCulture)}, {_y.ToString(CultureInfo.InvariantCulture)}}}";
        }
}