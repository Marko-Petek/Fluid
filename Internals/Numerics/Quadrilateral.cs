using System;
using static System.Math;

using Fluid.Internals.Meshing;
using static Fluid.Internals.Numerics.MatrixOperations;

namespace Fluid.Internals.Numerics
{
    /// <summary>A quadrilateral element.</summary>
    public struct Quadrilateral
    {
        /// <summary>Lower left vertex position in terms of x and y.</summary>
        public Pos _lL;
        /// <summary>Lower right vertex position in terms of x and y..</summary>
        public Pos _lR;
        /// <summary>Upper right vertex position in terms of x and y..</summary>
        public Pos _uR;
        /// <summary>Upper left vertex position in terms of x and y..</summary>
        public Pos _uL;

        /// <summary>Create an instance which holds Element's vertex positions.</summary><param name="lL">Lower left vertex position.</param><param name="lR">Lower right vertex position.</param><param name="uR">Upper right vertex position.</param><param name="uL">Upper left vertex position.</param>
        public Quadrilateral(ref Pos lL, ref Pos lR, ref Pos uR, ref Pos uL) {
            _lL = lL;
            _lR = lR;
            _uR = uR;
            _uL = uL;
        }

        public Quadrilateral(double lLX, double lLY, double lRX, double lRY, double uRX, double uRY, double uLX, double uLY) {
            _lL = new Pos(lLX, lLY);
            _lR = new Pos(lRX, lRY);
            _uR = new Pos(uRX, uRY);
            _uL = new Pos(uLX, uLY);
        }

        /// <summary>Calculate ksi and eta coordinates inside element using inverse transformations R and T.</summary><param name="pos">Position in terms of global x and y.</param>
        public Pos ReferenceSquareCoords(ref Pos pos) {
            double a = FuncA(ref pos);
            double b = FuncB(ref pos);
            double c = FuncC(ref pos);
            double detMALessMB = Subtract(MA(), MB()).Det();
            double detNALessNB = Subtract(NA(), NB()).Det();
            double ksi = 0.0;
            double eta = 0.0;

            if(pos._x * pos._y >= 0) {                                          // Quadrants I and III.

                if(Abs(detMALessMB) > 10E-7)                                    // Opposing sides are not too parallel.
                    ksi = (-b + Sqrt(b*b + c)) / detMALessMB;
                else                                                            // Opposing sides are virtually parallel. Use simplified model to preserve precision.
                    ksi = SimplifiedKsi(ref pos);
                
                if(Abs(detNALessNB) > 10E-7)                                    // Opposing sides are not too parallel.
                    eta = (-a - Sqrt(b*b + c)) / detNALessNB;
                else                                                            // Opposing sides are virtually parallel.
                    eta = SimplifiedEta(ref pos);
            }
            else {                                                              // Quadrants II and IV.
                if(Abs(detMALessMB) > 10E-7)                                    // Opposing sides are not too parallel.
                    ksi = (-b - Sqrt(b*b + c)) / detMALessMB;
                else                                                            // Opposing sides are virtually parallel. Use simplified model to preserve precision.
                    ksi = SimplifiedKsi(ref pos);

                if(Abs(detNALessNB) > 10E-7)                                    // Opposing sides are not too parallel.
                    eta = (-a + Sqrt(b*b + c)) / detNALessNB;
                else                                                            // Opposing sides are virtually parallel.
                    eta = SimplifiedEta(ref pos);
            }
            return new Pos(ksi, eta);
        }

        double FuncA(ref Pos pos) =>
            pos._x * MG().Tr() - pos._y * MF().Tr() + NA().Det() - NB().Det();

        double FuncB(ref Pos pos) =>
            pos._x * MG().Tr() - pos._y * MF().Tr() + MC().Det() + MD().Det();

        double FuncC(ref Pos pos) =>
            Subtract(MA(), MB()).Det() * (2*pos._x*MH().Tr() - 2*pos._y*MJ().Tr() + Add(MA(), MB()).Det());

        // Matrices to compute inverse transformation of specified element.
        double[][] MA() => new double[2][] {    new double[2] {_uL._x, _lR._x},
                                                new double[2] {_uL._y, _lR._y}  };

        double[][] MB() => new double[2][] {    new double[2] {_uR._x, _lL._x},
                                                new double[2] {_uR._y, _lL._y}  };

        double[][] MC() => new double[2][] {    new double[2] {_lR._x, _lL._x},
                                                new double[2] {_uL._y, _uR._y}  };

        double[][] MD() => new double[2][] {    new double[2] {_uL._x, _uR._x},
                                                new double[2] {_lR._y, _lL._y}  };

        double[][] MF() => new double[2][] {    new double[2] {_lR._x - _lL._x, 0.0},
                                                new double[2] {0.0 , _uL._x - _uR._x}   };

        double[][] MG() => new double[2][] {    new double[2] {_lR._y - _lL._y, 0.0},
                                                new double[2] {0.0 , _uL._y - _uR._y}   };

        double[][] MH() => new double[2][] {    new double[2] {_uL._y + _uR._y, 0.0},
                                                new double[2] {0.0 , -_lR._y - _lL._y}  };

        double[][] MJ() => new double[2][] {    new double[2] {_uL._x + _uR._x, 0.0},
                                                new double[2] {0.0 , -_lR._x - _lL._x}  };

        double[][] NA() => new double[2][] {    new double[2] {_uL._x, _uR._x},
                                                new double[2] {_uL._y, _uR._y}  };

        double[][] NB() => new double[2][] {    new double[2] {_lL._x, _lR._x},
                                                new double[2] {_lL._y, _lR._y}  };

        /// <summary>Distance of specified point P to a line going thorugh lower edge.</summary><param name="P">Specified point.</param>
        double DistanceToLowerEdge(ref Pos P) {
            var lowerEdgeVector = new Vector2(ref _lL, ref _lR);    // Vector from lower left to lower right vertex.
            lowerEdgeVector.Normalize();
            var posVector = new Vector2(ref _lL, ref P);            // Choose a point Q on lower edge: we choose LowerLeft vertex. Then take our specified point P and create a vector.
            return Abs(lowerEdgeVector.Cross(ref posVector));       // Take cross product of the two which will give you desired distance.
        }

        /// <summary>Distance of specified point P to a line going thorugh left edge.</summary><param name="P">Specified point.</param>
        double DistanceToLeftEdge(ref Pos P) {
            var leftEdgeVector = new Vector2(ref _lL, ref _uL);     // Vector from lower left to lower right vertex.
            leftEdgeVector.Normalize();
            var posVector = new Vector2(ref _lL, ref P);            // Choose a point Q on left edge: we choose LowerLeft vertex. Then take our specified point P and create a vector.
            return Abs(leftEdgeVector.Cross(ref posVector));       // Take cross product of the two which will give you desired distance.
        }

        double SimplifiedKsi(ref Pos pos) {
            double wholeStretchDist = DistanceToLeftEdge(ref _lR);      // Distance between parallel edges.
            double posDistance = DistanceToLeftEdge(ref pos);           // Distance of pos from left edge.
            return 2.0*(posDistance / wholeStretchDist) - 1.0;          // Transform to [-1,+1] interval.
        }

        double SimplifiedEta(ref Pos pos) {
            double wholeStretchDist = DistanceToLowerEdge(ref _uL);
            double posDistance = DistanceToLowerEdge(ref pos);
            return 2.0*(posDistance / wholeStretchDist) - 1.0;
        }
    }
}