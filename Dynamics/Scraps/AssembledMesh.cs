#if false
using System;
using static System.Math;

namespace FluidDynamics
{
    /// <summary>Block structured mesh covering whole channel.</summary>
    public class MasterMesh : Mesh
    {
        /// <summary>Minimum distance of obstruction from wall in terms of obstruction diameter.</summary>
        const double MinObstructionWallSpacing = 0.1;
        /// <summary>Minimum distace of obstruction from inlet/outlet in terms of obstruction diameter.</summary>
        const double MinObstructionInletSpacing = 2.0;
        /// <summary>Minimum channel length-to-width ratio.</summary>
        const double MinLengthToWidthRatio = 2.0;

        SubMesh LeftSubMesh { get; }
        /// <summary>SubMesh right of central square mesh.</summary>
        SubMesh RightSubMesh { get; }
        /// <summary>North quarter of central square mesh.</summary>
        SubMesh NorthSubMesh { get; }
        /// <summary>South quarter of central square mesh.</summary>
        SubMesh SouthSubMesh { get; }
        /// <summary>West quarter of central square mesh.</summary>
        SubMesh WestSubMesh { get; }
        /// <summary>East quarter of central square mesh.</summary>
        SubMesh EastSubMesh { get; }
        /// <summary>Auxiliary SubMesh stretching length of channel, above other submeshes.</summary>
        SubMesh UpperAuxSubMesh { get; }
        /// <summary>Auxiliary SubMesh stretching length of channel, below other submeshes.</summary>
        SubMesh LowerAuxSubMesh { get; }
        /// <summary>Absolute channel length.</summary>
        public double Length { get; }
        /// <summary>Absolute channel width.</summary>
        public double Width { get; }
        /// <summary>Obstruction diameter in terms of channel width.</summary>
        public double RelObstructionDiameter { get; }
        /// <summary>Obstruction's x position in terms of channel length.</summary>
        public double RelXPosOfObstruction { get; }
        /// <summary>Obstruction's y position in terms of channel width.</summary>
        public double RelYPosOfObstruction { get; }
        /// <summary>Linear density of elements at obstruction's boundary (circumference as unit length). Must be multiple of 4.</summary>
        public int MaxDensity { get; }
        /// <summary>Linear density of elements at inlet (obstruction circumference as unit length).</summary>
        public int MinDensity { get; }
        // /// <summary>Distance (in terms of obstruction circumference) where MaxElementDensity falls to MinElementDensity.</summary>
        // public double RelDensityFallOffDistance { get; }

        double XPosOfObstruction { get => RelXPosOfObstruction * Length; }
        double YPosOfObstruction { get => RelYPosOfObstruction * Width; }
        double ObstructionDiameter { get => RelObstructionDiameter * Width; }
        double ObstructionRadius { get => 0.5 * RelObstructionDiameter * Width; }
        double ObstructionCircumference { get => PI * ObstructionDiameter; }
        double SquareSide { get => ObstructionCircumference / MinDensity; }
        // /// <summary>Distance from obstruction's outer boundary to square side.</summary>
        // double DensityFallOffDistance { get => RelDensityFallOffDistance * ObstructionCircumference; }


        Corners MidStripVertices { get; }
        Corners CentralSquareVertices { get; }
        Corners InnerCircleVertices { get; }


        /// <summary>Constructs main mesh covering whole channel.</summary>
        public MasterMesh(double length = 100.0, double width = 30.0, double relXPosOfObstruction = 0.25,
        double relYPosOfObstruction = 0.40, double relObstructionDiameter = 0.25, int maxDensity = 40, int minDensity = 10)
         : base(0.0, width, length, width, 0.0, 0.0, length, 0.0) {
            
            #region Arguments' validity check
            if(length < MinLengthToWidthRatio * width)
                throw new ArgumentException($"Width needs to be at least {MinLengthToWidthRatio} times the height.",
                nameof(length));

            if(relXPosOfObstruction < (width / length) * relObstructionDiameter * MinObstructionInletSpacing)
                throw new ArgumentException(@"Obstruction too close to inlet.", nameof(relXPosOfObstruction));

            if(relXPosOfObstruction > 1 - (width / length) * relObstructionDiameter * MinObstructionInletSpacing)
                throw new ArgumentException(@"Obstruction too close to outlet.", nameof(relXPosOfObstruction));
            
            if(relYPosOfObstruction > 1 - (0.5 + MinObstructionWallSpacing) * relObstructionDiameter)
                throw new ArgumentException(@"Obstruction too close to north wall.", nameof(relYPosOfObstruction));

            if(relYPosOfObstruction < (0.5 + MinObstructionWallSpacing) * relObstructionDiameter)
                throw new ArgumentException("Obstruction too close to south wall.", nameof(relYPosOfObstruction));
            #endregion

            Width = width;
            Length = length;
            RelXPosOfObstruction = relXPosOfObstruction;
            RelYPosOfObstruction = relYPosOfObstruction;
            RelObstructionDiameter = relObstructionDiameter;
            MaxDensity = maxDensity;
            MinDensity = minDensity;
            //RelDensityFallOffDistance = relDensityFallOffDistance;

            if(Min(Width - YPosOfObstruction, YPosOfObstruction) < 0.5 * SquareSide ||                               // Element density must fall of to its min value before it reaches nearest wall.
               Min(Length - XPosOfObstruction, XPosOfObstruction) < 0.5 * SquareSide)
                throw new ArgumentException("MinDensity too small for given geometry.",
                nameof(minDensity));

            MidStripVertices = new Corners(0.0 , YPosOfObstruction + 0.5 * SquareSide,
                                            Length, YPosOfObstruction + 0.5 * SquareSide,
                                            0.0, YPosOfObstruction - 0.5 * SquareSide,
                                            Length, YPosOfObstruction - 0.5 * SquareSide);

            CentralSquareVertices = new Corners(XPosOfObstruction - 0.5 * SquareSide, YPosOfObstruction + 0.5 * SquareSide,
                                                 XPosOfObstruction + 0.5 * SquareSide, YPosOfObstruction + 0.5 * SquareSide,
                                                 XPosOfObstruction - 0.5 * SquareSide, YPosOfObstruction - 0.5 * SquareSide,
                                                 XPosOfObstruction + 0.5 * SquareSide, YPosOfObstruction - 0.5 * SquareSide );

            double tiltedRadiusX = 0.5*Sqrt(2.0) * ObstructionRadius;                                                   // Obstruction's radius tilted at 45 deg to x axis, projected onto x axis.
            InnerCircleVertices = new Corners(XPosOfObstruction - tiltedRadiusX, YPosOfObstruction + tiltedRadiusX,
                                               XPosOfObstruction + tiltedRadiusX, YPosOfObstruction + tiltedRadiusX,
                                               XPosOfObstruction - tiltedRadiusX, YPosOfObstruction - tiltedRadiusX,
                                               XPosOfObstruction + tiltedRadiusX, YPosOfObstruction - tiltedRadiusX);
        }

        SubMesh GetLeftSubMesh() {
            var x1 = 0.0;
            var x2 = XPosOfObstruction - 0.5 * SquareSide;
            var y1 = MidStripVertices.SW[1];
            var y2 = MidStripVertices.NW[1];

            var leftSubMesh = new SubMesh(x1,y2, x2, y2, x1,y1, x2,y1,
                                          s => new double[] {s * x2, y2},
                                          s => new double[] {s * x2, y1},
                                          s => new double[] {x1, y1 + s * (y2-y1)},
                                          s => new double[] {x2, y1 + s * (y2-y1)},
                                          x2, x2, y2-y1, y2-y1);

            return leftSubMesh;
        }

        SubMesh GetRightSubMesh() {
            var x1 = CentralSquareVertices.NE[0];
            var x2 = MidStripVertices.SE[0];
            var y1 = MidStripVertices.SE[1];
            var y2 = MidStripVertices.NE[1];

            var rightSubMesh = new SubMesh(x1,y2, x2, y2, x1,y1, x2,y1,
                                           s => new double[] {x1 + s * (x2-x1), y2},
                                           s => new double[] {x1 + s * (x2-x1), y1},
                                           s => new double[] {x1, y1 + s * (y2-y1)},
                                           s => new double[] {x2, y1 + s * (y2-y1)},
                                           x2-x1, x2-x1, y2-y1, y2-y1);
        
            return rightSubMesh;
        }

        SubMesh GetNorthSubMesh() {
            var nw = CentralSquareVertices.NW;
            var ne = CentralSquareVertices.NE;
            var sw = InnerCircleVertices.NW;
            var se = InnerCircleVertices.NE;
            var halfMoonHeight = (1 - 0.5*Sqrt(2))*ObstructionRadius;
            var diagonalProjection = 0.5*(SquareSide - Sqrt(2)*ObstructionRadius);
            var nArc = SquareSide;
            var sArc = 0.25 * ObstructionCircumference;
            var wArc = 0.5*Sqrt(2) * SquareSide - ObstructionRadius;

            var northSubMesh = new SubMesh(nw[0],nw[1], ne[0],ne[1], sw[0],sw[1], se[0],se[1],
                                           s => new double[] {nw[0] + s * SquareSide, nw[1]},
                                           s => new double[] {sw[0] + (se[0]-sw[0]) * (0.5 + Cos((PI/4)*(3-2*s)) / Sqrt(2)),
                                                              sw[1] + halfMoonHeight * (Sin((PI/4)*(3-2*s)) - 1/Sqrt(2)) / (1 - 1/Sqrt(2))},
                                           s => new double[] {sw[0] - s * diagonalProjection, sw[1] + s * diagonalProjection},
                                           s => new double[] {se[0] + s * diagonalProjection, se[1] + s * diagonalProjection},
                                           nArc, sArc, wArc, wArc);
        
            return northSubMesh;
        }

        SubMesh GetSouthSubMesh() {
            var nw = InnerCircleVertices.SW;
            var ne = InnerCircleVertices.SE;
            var sw = CentralSquareVertices.SW;
            var se = CentralSquareVertices.SE;
            var halfMoonHeight = (1 - 0.5*Sqrt(2))*ObstructionRadius;
            var diagonalProjection = 0.5*(SquareSide - Sqrt(2)*ObstructionRadius);
            var nArc = 0.25 * ObstructionCircumference;
            var sArc = SquareSide;
            var wArc = 0.5*Sqrt(2) * SquareSide - ObstructionRadius;

            var southSubMesh = new SubMesh(nw[0],nw[1], ne[0],ne[1], sw[0],sw[1], se[0],se[1],
                                           s => new double[] {nw[0] + (ne[0]-nw[0]) * (0.5 + Cos((PI/4)*(3-2*s)) / Sqrt(2)),
                                                              nw[1] - halfMoonHeight * (Sin((PI/4)*(3-2*s)) - 1/Sqrt(2)) / (1 - 1/Sqrt(2))},
                                           s => new double[] {sw[0] + s * SquareSide, sw[1]},
                                           s => new double[] {sw[0] + s * diagonalProjection, sw[1] + s * diagonalProjection},
                                           s => new double[] {se[0] - s * diagonalProjection, se[1] + s * diagonalProjection},
                                           nArc, sArc, wArc, wArc);
        
            return southSubMesh;
        }

        SubMesh GetWestSubMesh() {
            var nw = CentralSquareVertices.NW;
            var ne = InnerCircleVertices.NW;
            var sw = CentralSquareVertices.SW;
            var se = InnerCircleVertices.SW;
            var halfMoonWidth = (1 - 0.5*Sqrt(2))*ObstructionRadius;
            var diagonalProjection = 0.5*(SquareSide - Sqrt(2)*ObstructionRadius);
            var nArc = 0.5*Sqrt(2) * SquareSide - ObstructionRadius;
            var wArc = SquareSide;
            var eArc = 0.25 * ObstructionCircumference;

            var westSubMesh = new SubMesh(nw[0],nw[1], ne[0],ne[1], sw[0],sw[1], se[0],se[1],
                                          s => new double[] {nw[0] + s * diagonalProjection, nw[1] - s * diagonalProjection},
                                          s => new double[] {sw[0] + s * diagonalProjection, sw[1] + s * diagonalProjection},
                                          s => new double[] {sw[0], sw[1] + s * SquareSide},
                                          s => new double[] {se[0] - halfMoonWidth * (Sin((PI/4)*(3-2*s)) - 1/Sqrt(2)) / (1 - 1/Sqrt(2)),
                                                             se[1] + (ne[1]-se[1]) * (0.5 + Cos((PI/4)*(3-2*s)) / Sqrt(2))},
                                          nArc, nArc, wArc, eArc);

            return westSubMesh;
        }

        SubMesh GetEastSubMesh() {
            var nw = InnerCircleVertices.NE;
            var ne = CentralSquareVertices.NE;
            var sw = InnerCircleVertices.SE;
            var se = CentralSquareVertices.SE;
            var halfMoonWidth = (1 - 0.5*Sqrt(2))*ObstructionRadius;
            var diagonalProjection = 0.5*(SquareSide - Sqrt(2)*ObstructionRadius);
            var nArc = 0.5*Sqrt(2) * SquareSide - ObstructionRadius;
            var wArc = 0.25 * ObstructionCircumference;
            var eArc = SquareSide;

            var eastSubMesh = new SubMesh(nw[0],nw[1], ne[0],ne[1], sw[0],sw[1], se[0],se[1],
                                          s => new double[] {nw[0] + s * diagonalProjection, nw[1] + s * diagonalProjection},
                                          s => new double[] {sw[0] + s * diagonalProjection, sw[1] - s * diagonalProjection},
                                          s => new double[] {sw[0] + halfMoonWidth * (Sin((PI/4)*(3-2*s)) - 1/Sqrt(2)) / (1 - 1/Sqrt(2)),
                                                             sw[1] + (nw[1]-sw[1]) * (0.5 + Cos((PI/4)*(3-2*s)) / Sqrt(2))},
                                          s => new double[] {se[0], se[1] + s * SquareSide},
                                          nArc, nArc, wArc, eArc);

            int yElementCount = MaxDensity / 4;

            return eastSubMesh;
        }

        SubMesh GetUpperAuxSubMesh() {
            var nw = Corners.NW;
            var ne = Corners.NE;
            var sw = MidStripVertices.NW;
            var se = MidStripVertices.NE;

            var upperAuxSubMesh = new SubMesh(nw[0],nw[1], ne[0],ne[1], sw[0],sw[1], se[0],se[1],
                                              s => new double[] {nw[0] + s * (ne[0]-nw[0]), nw[1]},
                                              s => new double[] {nw[0] + s * (ne[0]-nw[0]), sw[1]},
                                              s => new double[] {sw[0], sw[1] + s * (nw[1]-sw[1])},
                                              s => new double[] {se[1], se[1] + s * (ne[1]-se[1])},
                                              ne[0]-nw[0], ne[0]-nw[0], nw[1]-sw[1], nw[1]-sw[1]);

            return upperAuxSubMesh;
        }

        SubMesh GetLowerAuxSubMesh() {
            var nw = MidStripVertices.SW;
            var ne = MidStripVertices.SE;
            var sw = Corners.SW;
            var se = Corners.SE;

            var lowerAuxSubMesh = new SubMesh(nw[0],nw[1], ne[0],ne[1], sw[0],sw[1], se[0],se[1],
                                              s => new double[] {nw[0] + s * (ne[0]-nw[0]), nw[1]},
                                              s => new double[] {nw[0] + s * (ne[0]-nw[0]), sw[1]},
                                              s => new double[] {sw[0], sw[1] + s * (nw[1]-sw[1])},
                                              s => new double[] {se[1], se[1] + s * (ne[1]-se[1])},
                                              ne[0]-nw[0], ne[0]-nw[0], nw[1]-sw[1], nw[1]-sw[1]);

            return lowerAuxSubMesh;
        }
    }
}
#endif