#if false
using System;
using System.Collections.Generic;

namespace FluidDynamics
{
    /// <summary>Able to create a rectangular submesh from provided border Nodes.</summary>
    public class RectangularMesh : Mesh
    {
        /// <summary>Contains border nodes of all sub-meshes.</summary>
        MasterMesh MasterMesh { get; }

        /// <summary>Construct a rectangular mesh by specifying indices of Nodes inside MasterMesh'es Nodes list.</summary><param name="nwCornerX">North-western corner's X coordinate.</param><param name="nwCornerY">North-western corner's Y coordinate.</param><param name="seCornerX">South-eastern corner's X coordinate.</param><param name="seCornerY">South-eastern corner's Y coordinate.</param>
        public RectangularMesh(List<int> northBorderNodes, List<int> westBorderNodes,
                               List<int> southBorderNodes, List<int> eastBorderNodes) :
                               base(northBorderNodes, westBorderNodes, southBorderNodes, eastBorderNodes) {

            int xNodeCount = northBorderNodes.Count;
            int yNodeCount = westBorderNodes.Count;

            #region Argument checking
            if((xNodeCount - 1) % 3 != 0) {                   // xNodeCout has to be of form 3n + 1.
                throw new ArgumentException($"{nameof(northBorderNodes)} count is not of the form 3n + 1. Unable to generate {nameof(RectangularMesh)}.");
            }
            if((yNodeCount - 1) % 3 != 0) {
                throw new ArgumentException($"{nameof(westBorderNodes)} count is not of the form 3n + 1. Unable to generate {nameof(RectangularMesh)}.");
            }
            if(xNodeCount != southBorderNodes.Count) {
                throw new ArgumentException($"{nameof(southBorderNodes)} count is not equal to {nameof(northBorderNodes)} count.");
            }
            if(yNodeCount != eastBorderNodes.Count) {
                throw new ArgumentException($"{nameof(eastBorderNodes)} count is not equal to {nameof(westBorderNodes)} count.");
            }
            #endregion

            int xThrees = xNodeCount / 3;      // e.g. 7 ~~> 2;    0+0, 0+1, 0+2,    3*1+0, 3*1+1, 3*1+2,    3*2+0, 3*2+1
            int yThrees = yNodeCount / 3;

            for(int i = 0; i < xThrees; ++i) {
                for(int j = 0; j < yNodeCount; ++j) {

                }
            }
        }

        /// <summary>Returns all nodes in structured form, using TFI on provided boundary Nodes.</summary><param name="northBoundaryNodes">Physical coordinates of Nodes on northern boundary of SubMesh.</param><param name="southBoundaryNodes">Physical coordinates of Nodes on southern boundary of SubMesh.</param><param name="westBoundaryNodes">Physical coordinates of Nodes on western boundary of SubMesh.</param><param name="eastBoundaryNodes">Physical coordinates of Nodes on eastern boundary of SubMesh.</param><returns>Structured mesh of Nodes in physical space.</returns>
        protected List<List<Node>> GetNodes(List<Node> northBoundaryNodes, List<Node> westBoundaryNodes) {
            var nodes = new List<List<double[]>>(ksiNodesCount);
            double dKsi = 1.0 / (ksiNodesCount - 1);
            double dEta = 1.0 / (etaNodesCount - 1);
            double eta, ksi;
            double x, y;
            

            for(int i = 0; i < ksiNodesCount; ++i) {
                nodes.Add(new List<double[]>(etaNodesCount));
                ksi = i * dKsi;

                for(int j = 0; j < etaNodesCount; ++j) {
                    eta = j * dEta;

                    x = (1 - ksi)*GetWNode(eta)[0]  +  ksi*GetENode(eta)[0]  +  (1 - eta)*GetSNode(ksi)[0]  +  eta*GetNNode(ksi)[0]  -  
                    (1 - ksi)*(1 - eta)*GetWNode(0.0)[0] - (1 - ksi)*eta*GetWNode(1.0)[0] - (1 - eta)*ksi*GetENode(0.0)[0] -
                    ksi*eta*GetENode(1.0)[0];

                    y = (1 - ksi)*GetWNode(eta)[1]  +  ksi*GetENode(eta)[1]  +  (1 - eta)*GetSNode(ksi)[1]  +  eta*GetNNode(ksi)[1]  -  
                    (1 - ksi)*(1 - eta)*GetWNode(0.0)[1] - (1 - ksi)*eta*GetWNode(1.0)[1] - (1 - eta)*ksi*GetENode(0.0)[1] -
                    ksi*eta*GetENode(1.0)[1];

                    nodes[i].Add(new double[] {x, y});
                }
            }

            return nodes;
        }
    }
}
#endif