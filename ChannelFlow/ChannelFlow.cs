using System;

using Fluid.Internals.Collections;
using Fluid.Internals.Numerics;
using Msh = Fluid.Internals.Meshing;
using static Fluid.Internals.Development.AppReporter;
using static Fluid.Internals.Development.AppReporter.VerbositySettings;
using static Fluid.ChannelFlow.Program;

namespace Fluid.ChannelFlow
{
    /// <summary>Sets up a linear system based on our input data and solves it.</summary>
    public class ChannelFlow
    {
        /// <summary>FlowSolver's step length for marching in time.</summary>
        double _dt;
        /// <summary>Fluid's viscosity.</summary>
        double _viscosity;
        /// <summary>Peak velocity at inlet (mid point across). Velocity profile at inlet is parabolic.</summary>
        double _peakInletVelocity;
        /// <summary>Solution represents a change of values from prvious time to new time.</summary>
        SparseRowDouble _solution;
        /// <summary>Holds positions of nodes.</summary>
        ChannelMesh _channelMesh;
        /// <summary>Channel width.</summary>
        double _width;
        /// <summary>Swaps that have to be made in solution vector and forcing vector to move constrained nodes to bottom most rows. These same swaps have to occurr also to columns of StiffnessMatrix.</summary>
        SparseMatInt _swapMatrix;
        /// <summary>FlowSolver's step length for marching in time.</summary>
        public double GetDt() => _dt;
        /// <summary>Fluid's viscosity.</summary>
        public double GetViscosity() => _viscosity;
        /// <summary>Peak velocity at inlet (mid point across). Velocity profile at inlet is parabolic.</summary>
        public double GetPeakInletVelocity() => _peakInletVelocity;


        /// <summary>Create a channel flow to solve.</summary><param name="peakInletVelocity">Peak velocity of fluid at inlet. Profile is parabolic, so peak velocity is situated in the middle.</param><param name="dt">Time step.</param>
        public ChannelFlow(double peakInletVelocity, double dt, double viscosity) {
                                                                                                Reporter.Write($"Creating ChannelFlow with peak inlet velocity {peakInletVelocity} and viscosity {viscosity}.", Verbose);
            _peakInletVelocity = peakInletVelocity;                                             Reporter.Write($"Integration step dt = {dt}.", Verbose);
            _dt = dt;
            _viscosity = viscosity;                                                             Reporter.Write("Passing freshly created ChannelFlow to ChannelMesh'es constructor.", Obnoxious);
            _channelMesh = new ChannelMesh(this);                                               // Initial setup is now already present on _nodes list.
            _width = _channelMesh.GetWidth();                                                   Reporter.Write("ChannelMesh constructed. Boundary conditions applied to each SubMesh. Merging all SubMeshes into a single SparseRow.", Verbose);
            _solution = _channelMesh.NodesArrayToSparseRow();                                   /* Take 0 field as initial solution. */  Reporter.Write("Creating a swap matrix which will specify which rows have to be swapped to bring boundary nodes towards end of SparseRow which we will be solving.", Verbose);
            _swapMatrix = CreateSwapMatrix();
        }

        // Reorder PreviousSolution

        /// <summary>Inlet x-component of velocity (u) as a function of y.</summary><param name="y">Coordinate across width of channel.</param>
        double InletU(double y) {
            return (4 * _peakInletVelocity * y / _width) * (1 - y / _width);
        }

        /// <summary>Inlet partial derivative of x-component of velocity in y direction (b) as a function of y.</summary><param name="y">Distance from lower side of channel.</param>
        double InletB(double y) {
            return (4 * _peakInletVelocity / _width) * (1 - y / _width);
        }

        /// <summary>Solves for field changes in time step dt and adds changes to Node[] array on _channelMesh.</summary>
        public void SolveNextAndAddToNodeArray() {
                                                                                                            Reporter.Write("Assembling stiffness matrix from node data.");
            var stiffnessMatrix = _channelMesh.AssembleStiffnessMatrix(this);                               /* Acquire stiffness matrix from existing values. */ Reporter.Write("Assembling forcing vector from node data.");
            var forcingVector = _channelMesh.AssembleForcingVector(this);                                   /* Acquire forcing vector. */ Reporter.Write("Applying column swaps to stiffnes matrix to bring constrained nodes to bottom.");
            stiffnessMatrix.ApplyColSwaps(_swapMatrix);                                                  // Swap columns so that constrained variables end up at right side.
            int stiffnessMatrixWidth = stiffnessMatrix.Width;                                          Reporter.Write("Applying column swaps to forcing vector to bring constrained nodes to bottom.");
            forcingVector.ApplySwaps(_swapMatrix);                                                         // Swap elements so that constrained elements end up at end.
            int forcingVectorWidth = forcingVector.Width;
            int nConstraints = _channelMesh.GetConstraintCount();                                           Reporter.Write("Splitting stiffness matrix along connstrained nodes column.");
            var stiffnessMatrixRight = stiffnessMatrix.SplitAtCol(stiffnessMatrixWidth - nConstraints);  /* Split stiffness matrix vertically. Remember right part. */ Reporter.Write("Splitting stiffness matrix along connstrained nodes row.");
            stiffnessMatrix.SplitAtRow(stiffnessMatrixWidth - nConstraints);                                /* Further split stiffness matrix horizontally. */ Reporter.Write("Applying swaps to solution SparseRow.");
            _solution.ApplySwaps(_swapMatrix);                                                              /* Apply swaps, do not forget to unswap after done. */ Reporter.Write("Splitting previous solution SparseRow.");
            var solutionLower = _solution.SplitAt(stiffnessMatrixWidth - nConstraints);                     /* Split also previos solution vector. Remeber lower part. */ Reporter.Write("Splitting forcing vector.");
            forcingVector.SplitAt(stiffnessMatrixWidth - nConstraints);                                     /* Also split forcing vector. */ Reporter.Write("Constructing modified forcing vector.");
            forcingVector = forcingVector - stiffnessMatrixRight * solutionLower;                           /* Construct modified forcing vector which we will need to solve linear system. */ Reporter.Write("Initiating ConjugateGradients solver.");
            var solver = new ConjugateGradients(stiffnessMatrix, forcingVector);                            Reporter.Write("Solving.");
            _solution = solver.GetSolution(_solution, 0.0001);                                              Reporter.Write("Merging solution with previously split values.");
            _solution.MergeWith(solutionLower);                                                             /* 1) Merge solution with solutionLower. */ Reporter.Write("Unswapping solution.");
            _solution.ApplySwaps(_swapMatrix);                                                              /* 2) Unswap solution. */ Reporter.Write("Adding changes to node array on channel mesh.");
            _solution.UpdateNodeArray(_channelMesh.Nodes);                                                  // 3) Add changes to Node[] array on _channelMesh.
        }

        /// <summary>Creates a matrix whose entries indicate which solution vector rows should be swapped with one another.</summary>
        SparseMatInt CreateSwapMatrix() {
            int posCount = _channelMesh.GetPositionCount();
            int nVars = _channelMesh.GetVariableCount();
            int width = posCount * nVars;
            var matrix = new SparseMatInt(width, width, 2000);

            for(int front = 0, back = width - 1; front <= back; ++front) {
                int frontPosIndex = front / nVars;                                  // Is rounded down.
                int frontVarIndex = front % nVars;
                int backPosIndex = back / nVars;
                int backVarIndex = back % nVars;
                ref var frontNode = ref _channelMesh.Node(frontPosIndex);
                ref var backNode = ref _channelMesh.Node(backPosIndex);

                if(frontNode.Var(frontVarIndex)._constrained) {                     // Check whether node at the front is constrained.
                    while(backNode.Var(backVarIndex)._constrained) {                // Move to an unconstrained variable.
                        --back;
                        backPosIndex = back / nVars;
                        backVarIndex = back % nVars;
                        backNode = ref _channelMesh.Node(backPosIndex);
                    }
                    matrix[front][back] = 1;                                    // Indicate that the two elements need to be swapped.
                }
            }
            return matrix;
        }

        // Robne pogoje dobiš preko vektorja vsiljevanja na začetku vsakega (časovnega) koraka.
        // Na začetku moraš podati vse vrednosti u,v,a,b,c,p,q,r v vseh vozlih. Takšne, kot jih podaš v vozlih označenih
        // kot Constrained, bodo tam tudi ostale. Vrednosti na vseh ostalih lokacijah predstavljajo začetno ugibanje,
        // ki ga bo Solver po nekaj časovnih korakih razvil v dinamiko sistema (učinek robnih pogojev se bo razlezel po polju).
        // Začetne 

        // Robnih vozlov je : 2*(20*3 - 1) + 4*(20*3 - 1) + 2*(20*3 + 1)   +   4 * (20*3 + 1)   + 2 * (60*3)
    
        public void WriteSolution(int samplingDensity) => _channelMesh.WriteVelocityField(samplingDensity);
    }

    public static class SparseRowHelper
    {
        /// <summary>Transfer values from this SparseRow to specified nodes array.</summary><param name="sparseRow">Source SparseRow of values.</param><param name="nodes">Receiving Node[] array of values.</param>
        public static void UpdateNodeArray(this SparseRow<double> sparseRow, Msh.Node[] nodes) {
            int nodeCount = nodes.Length;
            int nVars = nodes[0].Vars.Length;

            for(int i = 0; i < nodeCount; ++i) {

                for(int j = 0; j < nVars; ++j) {
                    nodes[i].Var(j)._value += sparseRow[nVars*i + j];
                }
            }
        }
    }
}