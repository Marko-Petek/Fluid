using System;
using System.Collections.Generic;

using Fluid.Internals.Numerics;
using My = Fluid.Internals.Collections.Custom;
using dbl = System.Double;

namespace Fluid.Internals.Lsfem {
   public abstract class Simulation {
      protected Mesh Mesh { get; }
      /// <summary>2D lists of nodes lying inside a block.</summary>
      protected Dictionary<string,PseudoElement[][]> Patches { get; set; }
      /// <summary>1D lists of nodes shared across blocks.</summary>
      protected Dictionary<string,PseudoElement[]> Joints { get; set; }
      /// <summary>Centers of mass of elements. Created right after elements are created.</summary>
      protected double[][] COMs { get; set; }
      /// <summary></summary>
      public abstract Dictionary<string,PseudoElement[][]> CreatePatches();
      /// <summary></summary>
      public abstract Dictionary<string,PseudoElement[]> CreateJoints();

      protected void CreateNodes() {
         Patches = CreatePatches();
         Joints = CreateJoints();
         int gInx = 0;
         foreach(var patch in Patches.Values) {          // Global indices on PseudoElements are not yet set. We set them now and add positions to Mesh.
            foreach(var pseudoRow in patch) {
               foreach(var pseudoEmt in pseudoRow) {
                  for(int i = 0; i < pseudoEmt.LInx.Length; ++i) {
                     pseudoEmt.GInx[i] = gInx;
                     Mesh.P.Add(pseudoEmt.Pos[i]);
                     ++gInx; } } } }
         foreach(var joint in Joints.Values) {           // We also set global indices on Joints and add their positions to Mesh.
               foreach(var pseudoEmt in joint) {
                  for(int i = 0; i < pseudoEmt.LInx.Length; ++i) {
                     pseudoEmt.GInx[i] = gInx;
                     Mesh.P.Add(pseudoEmt.Pos[i]);
                     ++gInx; } } }
      }
      /// <summary>User must supply custom logic here. The logic here depends on the way the blocks are joined together.</summary>
      protected abstract My.List<Element> CreateElements();
      /// <summary>Create the list of COMs after the Elements are created.</summary>
      protected double[][] CreateCOMs() {
         int nEmts = Mesh.Elements.Count;
         var coms = new double[nEmts][];
         dbl xCom, yCom;
         for(int i = 0; i < nEmts; ++i) {       // For each Element.
            xCom = 0.0;
            yCom = 0.0;
            for(int j = 0; j < 4; ++j) {              // Corner nodes.
               xCom += Mesh.Elements[i].P(3*j).X;
               yCom += Mesh.Elements[i].P(3*j).Y; }
            xCom /= 4;
            yCom /= 4;
            coms[i] = new double[2] { xCom, yCom }; }
         return coms;
      }

      public Simulation() {
         Mesh = new Mesh();
      }
   }
}