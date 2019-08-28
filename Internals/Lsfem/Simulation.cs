using System;
using System.Collections.Generic;
using static System.Math;

using Fluid.Internals.Numerics;
using My = Fluid.Internals.Collections.Custom;
using static Fluid.Internals.Toolbox;
using dbl = System.Double;
using dA = Fluid.Internals.Numerics.DblArithmetic;

using Supercluster.KDTree;

namespace Fluid.Internals.Lsfem {
   
   using Vec = Fluid.Internals.Collections.Vector<dbl,dA>;
   using Tnr = Fluid.Internals.Collections.Tensor<dbl, dA>;
   
   public abstract class Simulation {
      /// <summary>The Simulation.</summary>
      public static Simulation Sim { get; protected set; }
      /// <summary>Number of independent variables (= number of equations).</summary>
      public int N_m { get; internal set; }
      /// <summary>Dynamics tensor (stiffness matrix), 4th rank.</summary>
      public Tnr K { get; internal set; }
      /// <summary>Forcing tensor, 2nd rank.</summary>
      public Tnr F { get; internal set; }
      /// <summary>Stiffness tensor, 4th rank.
      /// Node (1), Derivative (2), 1st index of element matrix (3), 2nd index of element matrix (4).</summary>
      public Tnr A { get; internal set; }
      
      /// <summary>Free node values tensor, 2nd rank.</summary>
      public Tnr Uf { get; internal set; }
      /// <summary>Constrained node values tensor, 2nd rank.</summary>
      public Tnr Uc { get; internal set; }
      public Vec U(Vec dummy, int inx) {
         Vec u = Uf[dummy, inx];
         if(u != null)
            return u;
         else return Uc[dummy, inx];
      }
      /// <summary>Nodes values tensor, 2nd rank. Returns value at desired index regardless of which tensor it resides in.</summary>
      /// <param name="inxs">A set of indices that extends all the way down to value rank.</param>
      public dbl U(params int[] inxs) {
         dbl u = Uf[inxs];
         if(u != 0.0)
            return u;
         else return Uc[inxs];
      }
      protected Mesh Mesh { get; }
      /// <summary>2D lists of nodes lying inside a block.</summary>
      protected Dictionary<string,PseudoElement[][]> Patches { get; set; }
      /// <summary>1D lists of nodes shared across blocks.</summary>
      protected Dictionary<string,PseudoElement[]> Joints { get; set; }
      /// <summary></summary>
      public abstract Dictionary<string,PseudoElement[][]> CreatePatches();
      /// <summary></summary>
      public abstract Dictionary<string,PseudoElement[]> CreateJoints();
      protected ConjGradsSolver Solver { get; }
      // TODO: Set up boundary conditions.

      protected void CreateNodes() {                                    R.R("Creating Patches.");
         Patches = CreatePatches();                                     R.R("Creating Joints.");
         Joints = CreateJoints();
         int gInx = 0;                                                  R.R("Adding nodes to Mesh.");
         foreach(var patch in Patches.Values) {                         // Global indices on PseudoElements are not yet set. We set them now and add positions to Mesh.
            foreach(var pseudoRow in patch) {
               foreach(var pseudoEmt in pseudoRow) {
                  for(int i = 0; i < pseudoEmt.LInx.Length; ++i) {
                     pseudoEmt.GInx[i] = gInx;
                     Mesh.Pos.Add(pseudoEmt.Pos[i]);
                     ++gInx; } } } }
         foreach(var joint in Joints.Values) {                          // We also set global indices on Joints and add their positions to Mesh.
               foreach(var pseudoEmt in joint) {
                  for(int i = 0; i < pseudoEmt.LInx.Length; ++i) {
                     pseudoEmt.GInx[i] = gInx;
                     Mesh.Pos.Add(pseudoEmt.Pos[i]);
                     ++gInx; } } }
      }
      /// <summary>User must supply custom logic here. The logic here depends on the way the blocks are joined together.</summary>
      protected abstract My.List<Element> CreateElements();
      /// <summary>Create the list of COMs after the Elements are created.</summary>
      protected double[][] CreateCOMs(IList<Element> elements) {
         int nEmts = elements.Count;
         var coms = new double[nEmts][];
         dbl xCom, yCom;
         for(int i = 0; i < nEmts; ++i) {       // For each Element.
            xCom = 0.0;
            yCom = 0.0;
            for(int j = 0; j < 4; ++j) {              // Corner nodes.
               xCom += elements[i].Pos(3*j).X;
               yCom += elements[i].Pos(3*j).Y; }
            xCom /= 4;
            yCom /= 4;
            coms[i] = new double[2] { xCom, yCom }; }
         return coms;
      }
      /// <summary>Elements are found in InternalNodeArray.</summary>
      protected KDTree<double,Element> CreateElementTree() {               R.R("Creating a list of Elements.");
         var emts = CreateElements();                                      R.R("Creating a list of Centers of Mass.");
         var coms = CreateCOMs(emts);                                      R.R("Creating KDTree of Elements vs. CoMs positions.");
         emts.TrimExcessSpace();                                           // Trim excess space in array for proper operation of KDTree.
         var emtTree = new KDTree<double,Element>(2, coms, emts._E,
            (r1,r2) => Sqrt(Pow(r2[0]-r1[0], 2.0) + Pow(r2[1]-r1[1], 2.0)));
         return emtTree;
      }

      protected Tnr CalcQuadrupleOverlaps(IList<Element> emts) {

      }

      protected Tnr CalcTripleOverlaps(IList<Element> emts) {

      }

      public Simulation() {
         Sim = this;
         Mesh = new Mesh();
      }

      /// <summary>Find solution value at specified point.</summary><param name="pos">Sought after position.</param><param name="vars">Indices of variables we wish to retrieve.</param>S
      public abstract double[] Solution(in Vec2 pos);
   }
}