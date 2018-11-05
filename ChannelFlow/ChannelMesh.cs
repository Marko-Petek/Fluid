using System;
using System.IO;
using Fluid.Dynamics.Internals;
using Fluid.Dynamics.Meshing;
using Fluid.Dynamics.Numerics;
using static System.Math;

namespace Fluid.ChannelFlow
{
    /// <summary>Block structured mesh covering whole channel. Channel length is 4 times its width.</summary>
    public class ChannelMesh : BlockStructuredMesh
    {
        /// <summary>SubMesh right of square submesh.</summary>
        RightBlock _rightBlock;
        /// <summary>North quarter of square submesh.</summary>
        NorthBlock _northBlock;
        /// <summary>South quarter of square submesh.</summary>
        SouthBlock _southBlock;
        /// <summary>West quarter of square submesh.</summary>
        WestBlock _westBlock;
        /// <summary>East quarter of square submesh.</summary>
        EastBlock _eastBlock;
        /// <summary>Channel width in real-world units.</summary>
        protected double _width;
        /// <summary>Obstruction diameter in terms of channel width.</summary>
        protected double _relObstructionDiameter;
        /// <summary>Number of elements per square submesh side.</summary>
        protected int _elementDensity;
        /// <summary>Corners of right rectangular block.</summary>
        Quadrilateral _rightRect;
        /// <summary>Corners of left square block.</summary>
        Quadrilateral _leftSquare;
        /// <summary>Corners of largest possible square inside circular obstruction.</summary>
        Quadrilateral _obstructionRect;
        /// <summary>Number of positions that nodes reside at.</summary>
        int _positionCount;
        

        /// <summary>Number of elements per square submesh side.</summary>
        public int GetElementDensity() => _elementDensity;
        /// <summary>Obstruction's center x coordinate.</summary>
        double GetObstructionX() => 0.5 * _width;
        /// <summary>Obstruction's center y coordinate.</summary>
        double GetObstructionY() => 0.5 * _width;
        /// <summary>Obstruction diameter in terms of channel width.</summary>
        public double GetRelObstructionDiameter() => _relObstructionDiameter;
        /// <summary>Obstruction diameter in real-world units.</summary>
        public double GetObstructionDiameter() => _relObstructionDiameter * _width;
        /// <summary>Obstruction radius in real-world units.</summary>
        public double GetObstructionRadius() => 0.5 * GetObstructionDiameter();
        /// <summary>Obstruction circumference in real-world units.</summary>
        public double GetObstructionCircumference() => PI * GetObstructionDiameter();
        /// <summary>Channel width in real-world units.</summary>
        public double GetWidth() => _width;
        /// <summary>Corners of right rectangular block.</summary>
        public Quadrilateral GetRightRect() => _rightRect;
        /// <summary>Corners of left square block.</summary>
        public Quadrilateral GetLeftSquare() => _leftSquare;
        /// <summary>Corners of largest possible square inside circular obstruction.</summary>
        public Quadrilateral GetObstructionRect() => _obstructionRect;
        /// <summary>North quarter of square submesh.</summary>
        public NorthBlock GetNorthBlock() => _northBlock;
        /// <summary>South quarter of square submesh.</summary>
        public SouthBlock GetSouthBlock() => _southBlock;
        /// <summary>West quarter of square submesh.</summary>
        public WestBlock GetWestBlock() => _westBlock;
        /// <summary>East quarter of square submesh.</summary>
        public EastBlock GetEastBlock() => _eastBlock;
        /// <summary>SubMesh right of square submesh.</summary>
        public RectangularBlock GetRightBlock() => _rightBlock;
        /// <summary>Number of positions that nodes reside at.</summary>
        public int GetPositionCount() => _positionCount;
        /// <summary>Set number of positions that nodes reside at.</summary>
        public int SetPositionCount(int value) => _positionCount = value;

        /// <summary>Constructs main mesh covering whole channel.</summary><remarks>Parameters cannot yet be changed because currently, element integrals have to be computed outside (e.g.static by Mathematica) which requires manual setup.</remarks>
        public ChannelMesh(ChannelFlow channelFlow, double width = 1.0, double relObstructionDiameter = 0.25, int elementDensity = 20)
        : base(8) {

            _positionCount = 0;
            _width = width;
            _relObstructionDiameter = relObstructionDiameter;
            _elementDensity = elementDensity;
            _leftSquare = new Quadrilateral(                                            // Fixed.
                0.0, 0.0,
                _width, 0.0, 
                _width, _width,
                0.0, _width
            );
            double tiltedRadiusX = 0.5 * Sqrt(2.0) * GetObstructionRadius();                                                   // Obstruction's radius tilted at 45 deg to x axis, projected onto x axis.
            _obstructionRect = new Quadrilateral(                                       // Fixed.
                GetObstructionX() - tiltedRadiusX, GetObstructionY() - tiltedRadiusX,
                GetObstructionX() + tiltedRadiusX, GetObstructionY() - tiltedRadiusX,
                GetObstructionX() + tiltedRadiusX, GetObstructionY() + tiltedRadiusX,
                GetObstructionX() - tiltedRadiusX, GetObstructionY() + tiltedRadiusX
            );
            _rightRect = new Quadrilateral(                                             // Fixed.
                _width, 0.0,
                4 * _width, 0.0,
                4 * _width, _width,
                _width, _width
            );
            _nodes = new Node[15620];
            _southBlock = new SouthBlock(this, channelFlow);
            _westBlock = new WestBlock(this, channelFlow, _southBlock);
            _northBlock = new NorthBlock(this, channelFlow, _westBlock);                        // Integral values get imported with static constructor of ObstructionBlock.
            _eastBlock = new EastBlock(this, channelFlow, _northBlock, _southBlock);
            _rightBlock = new RightBlock(this, channelFlow, _eastBlock,
                _rightRect._lL._x, _rightRect._lL._y,
                _rightRect._uR._x, _rightRect._uR._y,
                20, 60);
            
            WriteCornerPositionsOfRectElement();
        }

        /// <summary>Assemble global stiffness matrix by going over each element of each block.</summary>
        public SparseMatrix<double> AssembleStiffnessMatrix(ChannelFlow channelFlow) {
            var stiffnessMatrix = new SparseMatrix<double>(15_620 * 8, 15_620 * 8, 10_000);
            double dt = channelFlow.GetDt();
            double viscosity = channelFlow.GetViscosity();
            _southBlock.AddContributionsToStiffnessMatrix(stiffnessMatrix, dt, viscosity);
            _westBlock.AddContributionsToStiffnessMatrix(stiffnessMatrix, dt, viscosity);
            _southBlock.AddContributionsToStiffnessMatrix(stiffnessMatrix, dt, viscosity);
            _eastBlock.AddContributionsToStiffnessMatrix(stiffnessMatrix, dt, viscosity);
            _rightBlock.AddContributionsToStiffnessMatrix(stiffnessMatrix, dt, viscosity);

            return stiffnessMatrix;
        }

        public SparseRow<double> AssembleForcingVector(ChannelFlow channelFlow) {
            var forcingVector = new SparseRow<double>(15_620 * 8, 10_000);
            double dt = channelFlow.GetDt();
            double viscosity = channelFlow.GetViscosity();
            _southBlock.AddContributionsToForcingVector(forcingVector, dt, viscosity);
            _westBlock.AddContributionsToForcingVector(forcingVector, dt, viscosity);
            _northBlock.AddContributionsToForcingVector(forcingVector, dt, viscosity);
            _eastBlock.AddContributionsToForcingVector(forcingVector, dt, viscosity);
            _rightBlock.AddContributionsToForcingVector(forcingVector, dt, viscosity);

            return forcingVector;
        }

        public void WriteCornerPositionsOfRectElement() {
            var node1 = _rightBlock.GetNodeStd(1,1,0).GetPos();
            var node4 = _rightBlock.GetNodeStd(1,1,3).GetPos();
            var node7 = _rightBlock.GetNodeStd(1,1,6).GetPos();
            var node10 = _rightBlock.GetNodeStd(1,1,9).GetPos();

            var fileInfo = new FileInfo("ChannelFlow/Results/rectElement.txt");

            using(var sw = new StreamWriter(fileInfo.FullName)) {
                sw.WriteLine($"{{{node1},");
                sw.WriteLine($"{node4},");
                sw.WriteLine($"{node7},");
                sw.WriteLine($"{node10}}}");
            }
        }

        /// <summary>Returns solution (only specified variables) at any specified point inside solution domain which is [0,width]x[0,4*width]. Returns a set of double.NaN values for a point outside the domain.</summary><param name="pos">Position in terms of x ans y.</param><param name="vars">Desired variables in terms of variable indices.</param>
        public override double[] Solution(ref Pos pos, params int[] vars) {

            if(_southBlock.IsPointInside(ref pos)) {                            // First determine if specified point is inside any block at all.
                return _southBlock.Solution(ref pos, vars);
            }
            else if(_eastBlock.IsPointInside(ref pos)) {
                return _eastBlock.Solution(ref pos, vars);
            }
            else if(_northBlock.IsPointInside(ref pos)) {
                return _northBlock.Solution(ref pos, vars);
            }
            else if(_westBlock.IsPointInside(ref pos)) {
                return _westBlock.Solution(ref pos, vars);
            }
            else if(_rightBlock.IsPointInside(ref pos)) {
                return _rightBlock.Solution(ref pos, vars);
            }
            else {                                                              // Return Double.NaN for points outside domain.
                var outsideDomain = new double[vars.Length];
                for(int i = 0; i < vars.Length; ++i) {
                    outsideDomain[i] = double.NaN;
                }
                return outsideDomain;
            }
        }
    }
}