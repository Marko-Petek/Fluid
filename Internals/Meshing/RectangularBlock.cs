using System;

using Fluid.Internals.Collections;
using Fluid.Internals.Numerics;
using static Fluid.Internals.Meshing.QuadElement;

namespace Fluid.Internals.Meshing
{
    /// <summary>Able to create a rectangular submesh from provided border Nodes.</summary>
    public abstract class RectangularBlock : MeshBlock
    {
        /// <summary>Lower left corner coordinates.</summary>
        Pos _lL;
        /// <summary>Upper right corner coordinates.</summary>
        Pos _uR;
        /// <summary>MeshBlock corner positions.</summary>
        Pos[] _vertices;
        /// <summary>Rectangle's width.</summary>
        double _width;
        /// <summary>Rectangle's height.</summary>
        double _height;
        /// <summary>Element's width.</summary>
        double _colWidth;
        /// <summary>Element's height.</summary>
        double _rowHeight;

        /// <summary>Rectangle's width.</summary>
        public double GetWidth() => _width;
        /// <summary>Rectangle's height.</summary>
        public double GetHeight() => _height;

        /// <summary>Create a Cartesian mesh block.</summary><param name="mainMesh">Mesh block's owner.</param><param name="llx">Lower left corner x coordinate.</param><param name="lly">Lower left corner y coordinate.</param><param name="urx">Upper right corner x coordinate.</param><param name="ury">Upper right corner y coordinate.</param><param name="rowCount">Number of elements in y direction.</param><param name="columnCount">Number of elements in x direction.</param>
        public RectangularBlock(BlockStructuredMesh mainMesh,
            double llx, double lly, double urx, double ury,
            int rowCount, int columnCount)
        : base(mainMesh) {
            
            _lL = new Pos(llx, lly);
            _uR = new Pos(urx, ury);
            _vertices = new Pos[4] {
                _lL,
                new Pos(_uR._x, _lL._y),
                _uR,
                new Pos(_lL._x, _uR._y)
            };
            _height = ury - lly;
            _width = urx - llx;
            _rowCount = rowCount;
            _colCount = columnCount;
            _rowHeight = _height / _rowCount;
            _colWidth = _width / _colCount;
        }

        protected override void CreateNodes() {

            double yTwoThirdsAbove, yThirdAbove, y, x, xThirdRight, xTwoThirdsRight;
            
            _nodes = new Node[_rowCount + 1][][];                                       // 60 node rows +1 for top row of nodes
            int nVars = _mainMesh.GetVariableCount();

            for(int row = 0; row < _rowCount; ++row) {                                          // Move vertically.
                _nodes[row] = new Node[_colCount + 1][];
                y = _lL._y + row * _rowHeight;
                yThirdAbove = y + _rowHeight / 3.0;
                yTwoThirdsAbove = y + 2 * _rowHeight / 3.0;

                for(int col = 0; col < _colCount; ++col) {
                    x = _lL._x + col * _colWidth;
                    xThirdRight = x + _colWidth / 3.0;
                    xTwoThirdsRight = x + 2 * _colWidth / 3.0;
                    _nodes[row][col] = new Node[] {
                        new Node(x, yTwoThirdsAbove, nVars), new Node(x, yThirdAbove, nVars),
                        new Node(x,y,nVars), new Node(xThirdRight, y, nVars), new Node(xTwoThirdsRight, y, nVars)
                    };
                }
                x = _uR._x;                                                       // Add right-most column.
                _nodes[row][_colCount] = new Node[] {
                    new Node(x, yTwoThirdsAbove, nVars), new Node(x, yThirdAbove, nVars),
                    new Node(x, y, nVars), new Node(Double.NaN, Double.NaN, 0),
                    new Node(Double.NaN, Double.NaN, 0)
                };
            }
            y = _uR._y;                                                           // Add upper-most row.
            _nodes[_rowCount] = new Node[_colCount + 1][];

            for(int col = 0; col < _colCount; ++col) {
                x = _lL._y + col * _colWidth;
                xThirdRight = x + _colWidth / 3.0;
                xTwoThirdsRight = x + 2 * _colWidth / 3.0;
                _nodes[_rowCount][col] = new Node[] {
                    new Node(Double.NaN, Double.NaN, 0), new Node(Double.NaN, Double.NaN, 0),
                    new Node(x,y, nVars), new Node(xThirdRight, y, nVars),
                    new Node(xTwoThirdsRight, y, nVars)
                };
            }
            _nodes[_rowCount][_colCount] = new Node[] {                                 // Add one last point.
                new Node(Double.NaN, Double.NaN, 0), new Node(Double.NaN, Double.NaN, 0),
                new Node(_uR._x, _uR._y, nVars),
                new Node(Double.NaN, Double.NaN, 0), new Node(Double.NaN, Double.NaN, 0)
            };
        }

        /// <summary>Find solution value of specified variables at specified point.</summary><param name="pos">X and y coordinates of desired point.</param><param name="vars">Indices of variables we wish to retrieve.</param>
        public override double[] Solution(ref Pos pos, params int[] vars) {
            double blockX = pos._x - _lL._x;                            // Local coordinates with lower left corner as origin.
            double blockY = pos._y - _lL._y;
            double normX = blockX / _width;                             // Get normalized x coordinate: interval [0,1].
            double normY = blockY / _height;
            int xCount = (int)(blockX / _colWidth);                     // Calculate how many times element's width fits in normX.
            int yCount = (int)(blockY / _rowHeight);
            double ksi = (blockX - xCount*_colWidth) / _colWidth;
            double eta = (blockY - yCount*_rowHeight) / _rowHeight;
            int nVars = vars.Length;
            double[] result = new double[nVars];

            for(int var = 0; var < nVars; ++var) {                        // For each specified variable.
                for(int node = 0; node < 12; ++node) {
                    result[var] += Phi[node](ksi,eta) * GetNodeStd(yCount, xCount, node).Var(vars[var])._value;
                }
            }
            return result;
        }

        /// <summary>Determine whether a point is inside this Rectangle.</summary><param name="pos">Point's position.</param>
        public override bool IsPointInside(ref Pos pos) {
            return pos.IsInsidePolygon(ref _vertices);
        }
    }
}