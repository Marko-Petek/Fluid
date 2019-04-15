using System;

using Fluid.Internals.Collections;
using Fluid.Internals.Numerics;
using Fluid.Internals.Meshing;
using TB = Fluid.Internals.Toolbox;
using static Fluid.Internals.Development.AppReporter.VerbositySettings;

namespace Fluid.ChannelFlow {
   using SparseMat = SparseMat<double,DblArithmetic>;
   using SparseRow = SparseRow<double,DblArithmetic>;
   using SparseMatInt = SparseMat<int,IntArithmetic>;
   /// <summary>Sets up a linear system based on our input data and solves it.</summary>
   public class ChannelFlow {
      /// <summary>FlowSolver's step length for marching in time.</summary>
      public double Dt { get; protected set; }
      /// <summary>Fluid's viscosity.</summary>
      public double Viscosity { get; protected set; }
      /// <summary>Peak velocity at inlet (mid point across). Velocity profile at inlet is parabolic.</summary>
      public double PeakInletVelocity { get; protected set; }
      /// <summary>Solution represents a change of values from prvious time to new time.</summary>
      SparseRow Solution { get; set; }
      /// <summary>Holds positions of nodes.</summary>
      ChannelMesh ChannelMesh { get; }
      /// <summary>Channel width.</summary>
      double Width { get; }
      /// <summary>Swaps that have to be made in solution vector and forcing vector to move constrained nodes to bottom most rows. These same swaps have to occurr also to columns of StiffnessMatrix.</summary>
      SparseMatInt SwapMatrix { get; }


      /// <summary>Create a channel flow to solve.</summary><param name="peakInletVelocity">Peak velocity of fluid at inlet. Profile is parabolic, so peak velocity is situated in the middle.</param><param name="dt">Time step.</param>
      public ChannelFlow(double peakInletVelocity, double dt, double viscosity) {         TB.Reporter.Write($"Creating ChannelFlow with peak inlet velocity {peakInletVelocity} and viscosity {viscosity}.", Verbose);
         PeakInletVelocity = peakInletVelocity;                                           TB.Reporter.Write($"Integration step dt = {dt}.", Verbose);
         Dt = dt;
         Viscosity = viscosity;                                                           TB.Reporter.Write("Passing freshly created ChannelFlow to ChannelMesh'es constructor.", Obnoxious);
         ChannelMesh = new ChannelMesh(this);                                             // Initial setup is now already present on _nodes list.
         Width = ChannelMesh.Width;                                                       TB.Reporter.Write("ChannelMesh constructed. Boundary conditions applied to each SubMesh. Merging all SubMeshes into a single SparseRow.", Verbose);
         Solution = ChannelMesh.NodesArrayToSparseRow();                                  /* Take 0 field as initial solution. */  TB.Reporter.Write("Creating a swap matrix which will specify which rows have to be swapped to bring boundary nodes towards end of SparseRow which we will be solving.", Verbose);
         SwapMatrix = CreateSwapMatrix();
      }
      // Reorder PreviousSolution

      /// <summary>Inlet x-component of velocity (u) as a function of y.</summary><param name="y">Coordinate across width of channel.</param>
      double InletU(double y) => (4 * PeakInletVelocity * y / Width) * (1 - y / Width);
      /// <summary>Inlet partial derivative of x-component of velocity in y direction (b) as a function of y.</summary><param name="y">Distance from lower side of channel.</param>
      double InletB(double y) => (4 * PeakInletVelocity / Width) * (1 - y / Width);
      /// <summary>Solves for field changes in time step dt and adds changes to Node[] array on _channelMesh.</summary>
      public void SolveNextAndAddToNodeArray() {                                                         TB.Reporter.Write("Assembling stiffness matrix from node data.");
         var sfsMatrix = ChannelMesh.AssembleSfsMatrix(this);                                /* Acquire stiffness matrix from existing values. */ TB.Reporter.Write("Assembling forcing vector from node data.");
         var forcingVector = ChannelMesh.AssembleFcgVector(this);                                    /* Acquire forcing vector. */ TB.Reporter.Write("Applying column swaps to stiffnes matrix to bring constrained nodes to bottom.");
         // FIXME: sfsMatrix.ApplyColSwaps(SwapMatrix);                                                      // Swap columns so that constrained variables end up at right side.
         int stiffnessMatrixWidth = sfsMatrix.Width;                                               TB.Reporter.Write("Applying column swaps to forcing vector to bring constrained nodes to bottom.");
         forcingVector.ApplySwaps(SwapMatrix);                                                           // Swap elements so that constrained elements end up at end.
         int forcingVectorWidth = forcingVector.Width;
         int nConstraints = ChannelMesh.NConstraints;                                            TB.Reporter.Write("Splitting stiffness matrix along constrained nodes column.");
         var sfsMatrixRight = sfsMatrix.SplitAtCol(stiffnessMatrixWidth - nConstraints);     /* Split stiffness matrix vertically. Remember right part. */ TB.Reporter.Write("Splitting stiffness matrix along connstrained nodes row.");
         sfsMatrix.SplitAtRow(stiffnessMatrixWidth - nConstraints);                                /* Further split stiffness matrix horizontally. */ TB.Reporter.Write("Applying swaps to solution SparseRow.");
         Solution.ApplySwaps(SwapMatrix);                                                                /* Apply swaps, do not forget to unswap after done. */ TB.Reporter.Write("Splitting previous solution SparseRow.");
         var solutionLower = Solution.SplitAt(stiffnessMatrixWidth - nConstraints);                      /* Split also previos solution vector. Remeber lower part. */ TB.Reporter.Write("Splitting forcing vector.");
         forcingVector.SplitAt(stiffnessMatrixWidth - nConstraints);                                     /* Also split forcing vector. */ TB.Reporter.Write("Constructing modified forcing vector.");
         forcingVector = forcingVector - sfsMatrixRight * solutionLower;                           /* Construct modified forcing vector which we will need to solve linear system. */ TB.Reporter.Write("Initiating ConjugateGradients solver.");
         var solver = new ConjGradsSolver(sfsMatrix, forcingVector);                            TB.Reporter.Write("Solving.");
         Solution = solver.GetSolution(Solution, 0.0001);                                                TB.Reporter.Write("Merging solution with previously split values.");
         Solution.MergeWith(solutionLower);                                                              /* 1) Merge solution with solutionLower. */ TB.Reporter.Write("Unswapping solution.");
         Solution.ApplySwaps(SwapMatrix);                                                                /* 2) Unswap solution. */ TB.Reporter.Write("Adding changes to node array on channel mesh.");
         Solution.UpdateNodeArray(ChannelMesh.Nodes);                                                    // 3) Add changes to Node[] array on _channelMesh.
      }

      /// <summary>Creates a matrix whose entries indicate which solution vector rows should be swapped with one another.</summary>
      SparseMatInt CreateSwapMatrix() {
         int posCount = ChannelMesh.PositionCount;
         int nVars = ChannelMesh.NVars;
         int width = posCount * nVars;
         var matrix = new SparseMatInt(width, width, 2000);
         for(int front = 0, back = width - 1; front <= back; ++front) {
            int frontPosIndex = front / nVars;                                  // Is rounded down.
            int frontVarIndex = front % nVars;
            int backPosIndex = back / nVars;
            int backVarIndex = back % nVars;
            ref var frontNode = ref ChannelMesh.Node(frontPosIndex);
            ref var backNode = ref ChannelMesh.Node(backPosIndex);
            if(frontNode.Var(frontVarIndex).Constrained) {                     // Check whether node at the front is constrained.
               while(backNode.Var(backVarIndex).Constrained) {                // Move to an unconstrained variable.
                  --back;
                  backPosIndex = back / nVars;
                  backVarIndex = back % nVars;
                  backNode = ref ChannelMesh.Node(backPosIndex); }
               matrix[front][back] = 1; } }                                  // Indicate that the two elements need to be swapped.}
         return matrix;
      }

      // Robne pogoje dobiš preko vektorja vsiljevanja na začetku vsakega (časovnega) koraka.
      // Na začetku moraš podati vse vrednosti u,v,a,b,c,p,q,r v vseh vozlih. Takšne, kot jih podaš v vozlih označenih
      // kot Constrained, bodo tam tudi ostale. Vrednosti na vseh ostalih lokacijah predstavljajo začetno ugibanje,
      // ki ga bo Solver po nekaj časovnih korakih razvil v dinamiko sistema (učinek robnih pogojev se bo razlezel po polju).
      // Začetne 

      // Robnih vozlov je : 2*(20*3 - 1) + 4*(20*3 - 1) + 2*(20*3 + 1)   +   4 * (20*3 + 1)   + 2 * (60*3)
      // public void WriteSolution(int samplingDensity) => ChannelMesh.WriteVelocityField(samplingDensity);
   }
}