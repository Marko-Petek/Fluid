#if false
using System;

namespace FluidDynamics
{
    public abstract class InterpolatedMesh : Mesh
    {
        /// <summary>Arc length of north edge.</summary>
        double NorthBoundaryLength { get; }
        /// <summary>Arc length of south edge.</summary>
        double SouthBoundaryLength { get; }
        /// <summary>Arc length of west edge.</summary>
        double WestBoundaryLength { get; }
        /// <summary>Arc length of east edge.</summary>
        double EastBoundaryLength { get; }

        public InterpolatedMesh(double nwCornerX, double nwCornerY, double neCornerX, double neCornerY,
                                   double swCornerX, double swCornerY, double seCornerX, double seCornerY,
                                   double nBoundaryLength, double sBoundaryLength, double wBoundaryLength, double eBoundaryLength) :
                                   base(nwCornerX, nwCornerY, neCornerX, neCornerY, swCornerX, swCornerY, seCornerX, seCornerY)  {

            NorthBoundaryLength = nBoundaryLength;
            SouthBoundaryLength = sBoundaryLength;
            WestBoundaryLength = wBoundaryLength;
            EastBoundaryLength = eBoundaryLength;
        }

        protected abstract double[] GetNorthBoundaryCoordinates(double ksi);
        protected abstract double[] GetSouthBoundaryCoordinates(double ksi);
        protected abstract double[] GetWestBoundaryCoordinates(double eta);
        protected abstract double[] GetEastBoundaryCoordinates(double eta);

        /// <summary>Take a point in unit square {(ksi, eta); ksi, eta ∊ [0,1]} and return a TFI transformed point in
        /// real space as Node.</summary><param name="ksi">Horizontal coordinate on unit square representation of SubMesh.</param> <param name="eta">Vertical coordinate on unit square representation of SubMesh.</param><returns>Coordinates in actual representation of SubMesh.</returns><param name="isFree">Indicates whether Node is free or constrained.</param>
        protected Node GetNode(double ksi, double eta, bool isFree) {
            var northBoundaryCoords = GetNorthBoundaryCoordinates(ksi);
            var southBoundaryCoords = GetSouthBoundaryCoordinates(ksi);
            var westBoundaryCoords = GetWestBoundaryCoordinates(eta);
            var eastBoundaryCoords = GetEastBoundaryCoordinates(eta);

            var x = (1 - ksi)*westBoundaryCoords[0]  +  ksi*eastBoundaryCoords[0]  + 
                    (1 - eta)*southBoundaryCoords[0]  +  eta*northBoundaryCoords[0]  -  
                    (1 - ksi)*(1 - eta)*Corners.SW[0] - (1 - ksi)*eta*Corners.NW[0] -
                    (1 - eta)*ksi*Corners.SE[0] - ksi*eta*Corners.NE[0];
            var y = (1 - ksi)*westBoundaryCoords[1]  +  ksi*eastBoundaryCoords[1]  + 
                    (1 - eta)*southBoundaryCoords[1]  +  eta*northBoundaryCoords[1]  -  
                    (1 - ksi)*(1 - eta)*Corners.SW[1] - (1 - ksi)*eta*Corners.NW[1] -
                    (1 - eta)*ksi*Corners.SE[1] - ksi*eta*Corners.NE[1];
            
            return new Node(x, y, isFree);
        }

        /// <summary>Returns all nodes in structured form, using TFI on provided boundary Nodes.</summary><param name="northBoundaryNodes">Physical coordinates of Nodes on northern boundary of SubMesh.</param><param name="southBoundaryNodes">Physical coordinates of Nodes on southern boundary of SubMesh.</param><param name="westBoundaryNodes">Physical coordinates of Nodes on western boundary of SubMesh.</param><param name="eastBoundaryNodes">Physical coordinates of Nodes on eastern boundary of SubMesh.</param><returns>Structured mesh of Nodes in physical space.</returns>
        protected List<List<Node>> GetNodes(List<Node> northBoundaryNodes, List<Node> southBoundaryNodes,
                                         List<Node> westBoundaryNodes, List<Node> eastBoundaryNodes) {

            
        }
    }
}


var southSubMesh = new MeshBlock(upperLeft[0],upperLeft[1], ne[0],ne[1], sw[0],sw[1], se[0],se[1],
                                           //s => new double[] {upperLeft[0] + (ne[0]-upperLeft[0]) * (0.5 + Cos((PI/4)*(3-2*s)) / Sqrt(2)),
                                            //                  upperLeft[1] - quarterMoonHeight * (Sin((PI/4)*(3-2*s)) - 1/Sqrt(2)) / (1 - 1/Sqrt(2))},
                                           //s => new double[] {sw[0] + s * SquareSide, sw[1]},
                                           s => new double[] {sw[0] + s * diagonalProjection, sw[1] + s * diagonalProjection},
                                           //s => new double[] {se[0] - s * diagonalProjection, se[1] + s * diagonalProjection},
                                           lowerArc, upperArc, leftArc, leftArc);
#endif