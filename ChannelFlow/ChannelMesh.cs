using System;
using System.IO;
using static System.Math;

using Fluid.Internals.Collections;
using Fluid.Internals.Meshing;
using Fluid.Internals.Numerics;
using TB = Fluid.Internals.Toolbox;
using static Fluid.Internals.Development.AppReporter.VerbositySettings;

namespace Fluid.ChannelFlow {
   using SparseMat = SparseMat<double,DblArithmetic>;
   using SparseRow = SparseRow<double,DblArithmetic>;
   /// <summary>Block structured mesh covering whole channel. Channel length is 4 times its width.</summary>
   public class ChannelMesh : BlockStructuredMesh {
      /// <summary>SubMesh right of square submesh.</summary>
      protected RightBlock RightBlock { get; }
      /// <summary>North quarter of square submesh.</summary>
      protected NorthBlock NorthBlock { get; }
      /// <summary>South quarter of square submesh.</summary>
      protected SouthBlock SouthBlock { get; }
      /// <summary>West quarter of square submesh.</summary>
      protected WestBlock WestBlock { get; }
      /// <summary>East quarter of square submesh.</summary>
      protected EastBlock EastBlock { get; }
      /// <summary>Channel width in real-world units.</summary>
      public double Width { get; protected set; }
      /// <summary>Obstruction diameter in terms of channel width.</summary>
      public double RelObstructionDiameter { get; protected set; }
      /// <summary>Number of elements per square submesh side.</summary>
      public int ElementDensity { get; }
      /// <summary>Corners of right rectangular block.</summary>
      protected Tetragon RightRect { get; }
      /// <summary>Corners of left square block.</summary>
      public Tetragon LeftSquare { get; protected set; }
      /// <summary>Corners of largest possible square inside circular obstruction.</summary>
      public Tetragon ObstructionRect { get; protected set; }
      /// <summary>Number of positions that nodes reside at.</summary>
      public int NPos { get; set; }
      /// <summary>Obstruction's center x coordinate.</summary>
      protected double ObstructionX => 0.5 * Width;
      /// <summary>Obstruction's center y coordinate.</summary>
      protected double ObstructionY => 0.5 * Width;
      /// <summary>Obstruction diameter in real-world units.</summary>
      public double ObstructionDiameter => RelObstructionDiameter * Width;
      /// <summary>Obstruction radius in real-world units.</summary>
      public double ObstructionRadius => 0.5 * ObstructionDiameter;


      /// <summary>Constructs main mesh covering whole channel.</summary><remarks>Parameters cannot yet be changed because currently, element integrals have to be computed outside (e.g.static by Mathematica) which requires manual setup.</remarks>
      public ChannelMesh(ChannelCylinderSystem channelFlow, double width = 1.0,
         double relObstructionDiameter = 0.25, int elementDensity = 20) : base(8) {
            NPos = 0;
            Width = width;
            RelObstructionDiameter = relObstructionDiameter;
            ElementDensity = elementDensity;
            LeftSquare = new Tetragon(                                                // Fixed.
               0.0, 0.0,
               Width, 0.0, 
               Width, Width,
               0.0, Width );
            double tiltedRadiusX = 0.5 * Sqrt(2.0) * ObstructionRadius;                // Obstruction's radius tilted at 45 deg to x axis, projected onto x axis.
            ObstructionRect = new Tetragon(                                           // Fixed.
               ObstructionX - tiltedRadiusX, ObstructionY - tiltedRadiusX,
               ObstructionX + tiltedRadiusX, ObstructionY - tiltedRadiusX,
               ObstructionX + tiltedRadiusX, ObstructionY + tiltedRadiusX,
               ObstructionX - tiltedRadiusX, ObstructionY + tiltedRadiusX );
            RightRect = new Tetragon(                                                 // Fixed.
               Width, 0.0,
               4 * Width, 0.0,
               4 * Width, Width,
               Width, Width );
            G = new MeshNode[15620];                                            TB.Reporter.Write($"Created global array of nodes of length {G.Length}.", Verbose); TB.Reporter.Write("Constructing SouthBlock. Passing ChannelMesh and ChannelFlow as arguments.", Verbose);
            SouthBlock = new SouthBlock(this, channelFlow);                          TB.Reporter.Write("Constructing WestBlock.", Verbose);
            WestBlock = new WestBlock(this, channelFlow, SouthBlock);                TB.Reporter.Write("Constructing NorthBlock.", Verbose);
            NorthBlock = new NorthBlock(this, channelFlow, WestBlock);               /* Integral values get imported with static constructor of ObstructionBlock. */  TB.Reporter.Write("Constructing EastBlock.", Verbose);
            EastBlock = new EastBlock(this, channelFlow, NorthBlock, SouthBlock);    TB.Reporter.Write("Constructing RightBlock.", Verbose);
            RightBlock = new RightBlock(this, channelFlow, EastBlock,
               RightRect._LL.X, RightRect._LL.Y,
               RightRect._UR.X, RightRect._UR.Y,
               20, 60 );                                                             TB.Reporter.Write("Writing corner node positions of elements for the purpose of drawing a mesh.", Verbose);
            WriteCornerPositionsOfRectElement();
      }

      /// <summary>Assemble global stiffness matrix by going over each element of each block.</summary>
      public SparseMat AssembleSfsMatrix(ChannelCylinderSystem channelFlow) {         TB.Reporter.Write("Constructing stiffnes matrix as a sparse matrix.");
         var sfsMatrix = new SparseMat(15_620 * 8, 15_620 * 8, 10_000);
         double dt = channelFlow.Dt;
         double viscosity = channelFlow.Viscosity;                          TB.Reporter.Write("Adding stiffness matrix contributions of SouthBlock.");
         SouthBlock.AddContribsToSfsMatrix(sfsMatrix, dt, viscosity);       TB.Reporter.Write("Adding stiffness matrix contributions of WestBlock.");
         WestBlock.AddContribsToSfsMatrix(sfsMatrix, dt, viscosity);        TB.Reporter.Write("Adding stiffness matrix contributions of NorthBlock.");
         NorthBlock.AddContribsToSfsMatrix(sfsMatrix, dt, viscosity);       TB.Reporter.Write("Adding stiffness matrix contributions of EastBlock.");
         EastBlock.AddContribsToSfsMatrix(sfsMatrix, dt, viscosity);        TB.Reporter.Write("Adding stiffness matrix contributions of RightBlock.");
         RightBlock.AddContribsToSfsMatrix(sfsMatrix, dt, viscosity);
         return sfsMatrix;
      }
      public SparseRow AssembleFcgVector(ChannelCylinderSystem channelFlow) {
         var fcgVector = new SparseRow(15_620 * 8, 10_000);
         double dt = channelFlow.Dt;
         double viscosity = channelFlow.Viscosity;
         SouthBlock.AddContribsToFcgVector(fcgVector, dt, viscosity);
         WestBlock.AddContribsToFcgVector(fcgVector, dt, viscosity);
         NorthBlock.AddContribsToFcgVector(fcgVector, dt, viscosity);
         EastBlock.AddContribsToFcgVector(fcgVector, dt, viscosity);
         RightBlock.AddContribsToFcgVector(fcgVector, dt, viscosity);
         return fcgVector;
      }
      public void WriteCornerPositionsOfRectElement() {
         var node1 = RightBlock.NodeStd(1,1,0).Pos;
         var node4 = RightBlock.NodeStd(1,1,3).Pos;
         var node7 = RightBlock.NodeStd(1,1,6).Pos;
         var node10 = RightBlock.NodeStd(1,1,9).Pos;
         var fileInfo = new FileInfo("./Results/rectElement.txt");
         using(var sw = new StreamWriter(fileInfo.FullName)) {
            sw.WriteLine($"{{{node1},");
            sw.WriteLine($"{node4},");
            sw.WriteLine($"{node7},");
            sw.WriteLine($"{node10}}}"); }
      }
      /// <summary>Returns solution (only specified variables) at any specified point inside solution domain which is [0,width]x[0,4*width]. Returns a set of double.NaN values for a point outside the domain.</summary><param name="pos">Position in terms of x ans y.</param><param name="vars">Desired variables in terms of variable indices.</param>
      public override double[] Solution(in Pos pos, params int[] vars) {
         if(SouthBlock.IsPointInside(in pos))                                // First determine if specified point is inside any block at all.
            return SouthBlock.Solution(in pos, vars);
         else if(EastBlock.IsPointInside(in pos))
            return EastBlock.Solution(in pos, vars);
         else if(NorthBlock.IsPointInside(in pos))
            return NorthBlock.Solution(in pos, vars);
         else if(WestBlock.IsPointInside(in pos))
            return WestBlock.Solution(in pos, vars);
         else if(RightBlock.IsPointInside(in pos))
            return RightBlock.Solution(in pos, vars);
         else {                                                               // Return Double.NaN for points outside domain.
            var outsideDomain = new double[vars.Length];
            for(int i = 0; i < vars.Length; ++i)
               outsideDomain[i] = double.NaN;
            return outsideDomain; }
      }
   }
}