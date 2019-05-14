using System;
using SCG = System.Collections.Generic;

using Fluid.Internals.Collections;
using Fluid.Internals.Numerics;
using Fluid.Internals.Meshing;
using TB = Fluid.Internals.Toolbox;
using static Fluid.Internals.Development.AppReporter.VerbositySettings;

namespace Fluid.ChannelFlow {
   using dbl = Double;
   using Tensor = Tensor<double,DblArithmetic>;
   using Vector = Vector<double,DblArithmetic>;
   using TensorInt = Tensor<int,IntArithmetic>;
   /// <summary>Sets up a linear system based on our input data and solves it.</summary>
   public class ChannelCylinderSystem {
      /// <summary>FlowSolver's step length for marching in time.</summary>
      public dbl Dt { get; protected set; }
      /// <summary>Fluid's viscosity.</summary>
      public dbl Viscosity { get; protected set; }
      /// <summary>Peak velocity at inlet (mid point across). Velocity profile at inlet is parabolic.</summary>
      public dbl PeakInletVelocity { get; protected set; }
      /// <summary>Solution represents a change of values from prvious time to new time.</summary>
      public Vector SolVec { get; protected set; }
      /// <summary>Holds positions of nodes.</summary>
      ChannelMesh Mesh { get; }
      /// <summary>Channel width.</summary>
      dbl Width { get; }
      /// <summary>Swaps that have to be made in solution vector and forcing vector to move constrained nodes to bottom most rows. These same swaps have to occurr also to columns of StiffnessMatrix.</summary>
      SCG.Dictionary<int,int> SwapDict { get; }


      /// <summary>Create a channel flow to solve.</summary><param name="peakInletVelocity">Peak velocity of fluid at inlet. Profile is parabolic, so peak velocity is situated in the middle.</param><param name="dt">Time step.</param>
      public ChannelCylinderSystem(dbl peakInletVelocity, dbl dt, dbl viscosity) {         TB.Reporter.Write($"Creating ChannelFlow with peak inlet velocity {peakInletVelocity} and viscosity {viscosity}.", Verbose);
         PeakInletVelocity = peakInletVelocity;                                           TB.Reporter.Write($"Integration step dt = {dt}.", Verbose);
         Dt = dt;
         Viscosity = viscosity;                                                           TB.Reporter.Write("Passing freshly created ChannelFlow to ChannelMesh'es constructor.", Obnoxious);
         Mesh = new ChannelMesh(this);                                             // Initial setup is now already present on _nodes list.
         Width = Mesh.Width;                                                       TB.Reporter.Write("ChannelMesh constructed. Boundary conditions applied to each SubMesh. Merging all SubMeshes into a single SparseRow.", Verbose);
         SolVec = Mesh.NodesArrayToSparseRow();                                  /* Take 0 field as initial solution. */  TB.Reporter.Write("Creating a swap matrix which will specify which rows have to be swapped to bring boundary nodes towards end of SparseRow which we will be solving.", Verbose);
         SwapDict = CreateSwapDict();
      }
      // Reorder PreviousSolution

      /// <summary>Inlet x-component of velocity (u) as a function of y.</summary><param name="y">Coordinate across width of channel.</param>
      dbl InletU(dbl y) => (4 * PeakInletVelocity * y / Width) * (1 - y / Width);
      /// <summary>Inlet partial derivative of x-component of velocity in y direction (b) as a function of y.</summary><param name="y">Distance from lower side of channel.</param>
      dbl InletB(dbl y) => (4 * PeakInletVelocity / Width) * (1 - y / Width);
      /// <summary>Solves for field changes in time step dt and adds changes to Node[] array on _channelMesh.</summary>
      public void SolveNextAndAddToNodeArray() {
         throw new NotImplementedException();
#if false
         TB.Reporter.Write("Assembling stiffness matrix from node data.");
         var stfsMat = Mesh.AssembleSfsMatrix(this);                                /* Acquire stiffness matrix from existing values. */ TB.Reporter.Write("Assembling forcing vector from node data.");
         var frcgVec = Mesh.AssembleFcgVector(this);                                    /* Acquire forcing vector. */ TB.Reporter.Write("Applying column swaps to stiffnes matrix to bring constrained nodes to bottom.");
         stfsMat.ApplyColSwaps(SwapDict);                                                      // Swap columns so that constrained variables end up at right side.
         int stfsMatW = stfsMat.Dim1; TB.Reporter.Write("Applying column swaps to forcing vector to bring constrained nodes to bottom.");
         frcgVec.ApplySwaps(SwapDict);                                                           // Swap elements so that constrained elements end up at end.
         int frcgVecW = frcgVec.Width;
         int nConts = Mesh.NConstraints; TB.Reporter.Write("Splitting stiffness matrix along constrained nodes column.");
         var stfsMatRight = stfsMat.SplitAtCol(stfsMatW - nConts);     /* Split stiffness matrix vertically. Remember right part. */ TB.Reporter.Write("Splitting stiffness matrix along connstrained nodes row.");
         stfsMat.SplitAtRow(stfsMatW - nConts);                                /* Further split stiffness matrix horizontally. */ TB.Reporter.Write("Applying swaps to solution SparseRow.");
         SolVec.ApplySwaps(SwapDict);                                                                /* Apply swaps, do not forget to unswap after done. */ TB.Reporter.Write("Splitting previous solution SparseRow.");
         var solVecLower = SolVec.SplitAt(stfsMatW - nConts);                      /* Split also previos solution vector. Remeber lower part. */ TB.Reporter.Write("Splitting forcing vector.");
         frcgVec.SplitAt(stfsMatW - nConts);                                     /* Also split forcing vector. */ TB.Reporter.Write("Constructing modified forcing vector.");
         frcgVec = frcgVec - stfsMatRight * solVecLower;                           /* Construct modified forcing vector which we will need to solve linear system. */ TB.Reporter.Write("Initiating ConjugateGradients solver.");
         var solver = new ConjGradsSolver(stfsMat, frcgVec); TB.Reporter.Write("Solving.");
         SolVec = solver.Solve(SolVec, 0.0001); TB.Reporter.Write("Merging solution with previously split values.");
         SolVec.MergeWith(solVecLower);                                                              /* 1) Merge solution with solutionLower. */ TB.Reporter.Write("Unswapping solution.");
         SolVec.ApplySwaps(SwapDict);                                                                /* 2) Unswap solution. */ TB.Reporter.Write("Adding changes to node array on channel mesh.");
         SolVec.UpdateNodeArray(Mesh.G);                                                    // 3) Add changes to Node[] array on _channelMesh.

#endif
      }

      /// <summary>Creates a dictionary whose entries indicate which solution vector rows should be swapped with one another.</summary>
      SCG.Dictionary<int,int> CreateSwapDict() {
         int posCount = Mesh.NPos;
         int nVars = Mesh.NVars;
         int width = posCount * nVars;
         var swapDict = new SCG.Dictionary<int,int>(2000);
         for(int topGInx = 0, lowGInx = width - 1; topGInx <= lowGInx; ++topGInx) {
            int topBeltInx = topGInx / nVars;                                  // Is rounded down.
            int topVarInx = topGInx % nVars;
            int lowBeltInx = lowGInx / nVars;
            int lowVarInx = lowGInx % nVars;
            var topG = Mesh.G[topBeltInx];
            var lowG = Mesh.G[lowBeltInx];
            if(topG.Vars[topVarInx].Constrained) {                     // Check whether node at the front is constrained.
               while(lowG.Vars[lowVarInx].Constrained) {                // Move to an unconstrained variable.
                  --lowGInx;
                  lowBeltInx = lowGInx / nVars;
                  lowVarInx = lowGInx % nVars;
                  lowG = Mesh.G[lowBeltInx]; }
               swapDict.Add(topGInx, lowGInx); } }                                  // Indicate that the two elements need to be swapped.}
         return swapDict;
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