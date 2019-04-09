using System;

using Fluid.Internals.Collections;
using Fluid.Internals.Numerics;

namespace Fluid.Internals.Meshing
{
    /// <summary>A mesh made of quadrilateral elements.</summary>
    public abstract class BlockStructuredMesh
    {
        /// <summary>Number of independent variables (= number of equations).</summary>
        protected int _variableCount;
        /// <summary>Global vector of node values. Each Node[] array represents variable values at a single position.</summary>
        protected MeshNode[] _nodes;
        /// <summary>Number of constrained variables.</summary>
        protected int _constraintCount;

        /// <summary>Global vector of node values. Each Node[] array represents variable values at a single position.</summary>
        public MeshNode[] Nodes => _nodes;
        /// <summary>Number of independent variables (= number of equations).</summary>
        public int GetVariableCount() => _variableCount;
        /// <summary>Global vector of nodes (positions, variable values, variable constrainedness).</summary>
        public ref MeshNode Node(int index) => ref _nodes[index];
        /// <summary>Toatal number of constrained variables.</summary>
        public int GetConstraintCount() => _constraintCount;
        /// <summary>Set total number of constrained variables.</summary>
        public void SetConstraintCount(int value) => _constraintCount = value;


        // /// <summary>Create a block-structured mesh.</summary>
        public BlockStructuredMesh(int variableCount) {
            _variableCount = variableCount;
        }

        /// <summary>Create a SparseRow out of Nodes array. Simply flatten list and copy it to SparseRow.</summary>
        public SparseRowDouble NodesArrayToSparseRow() {
            int rowWidth = _nodes.Length * _variableCount;
            var sparseRow = new SparseRowDouble(rowWidth, rowWidth);
            int index = 0;

            for(int i = 0; i < _nodes.Length; ++i) {

                for(int j = 0; j < _variableCount; ++j) {
                    index = 8*i + j;
                    sparseRow[index] = _nodes[i].Var(j)._value;
                }
            }
            return sparseRow;
        }

        /// <summary>Find solution value of specified variables at specified point.</summary><param name="pos">Sought after position.</param><param name="vars">Indices of variables we wish to retrieve.</param>S
        public abstract double[] Solution(ref Pos pos, params int[] vars);
    }
}