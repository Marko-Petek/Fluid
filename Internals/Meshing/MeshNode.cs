using System;

using Fluid.Internals.Numerics;

namespace Fluid.Internals.Meshing {
   public class MeshNode : IEquatable<MeshNode> {
      /// <summary>Node's position (x,y).</summary>
      public Pos Pos;
      /// <summary>An array of doubles. One double for each variable.</summary>
      public Variable[] Vars;
      public double X => Pos.X;
      public double Y => Pos.Y;


      /// <summary>Create a Node with specified coordinates and 0.0 values for variables, all of which are set unconstrained.</summary><param name="x">Node's x position.</param><param name="y">Node's y position.</param><param name="nVars">Number of variables.</param>
      public MeshNode(double x, double y, int nVars) {
         Pos = new Pos(x,y);
         Vars = new Variable[nVars];
      }
      public MeshNode(Pos pos, int nVars) {
         Pos = new Pos(pos.X, pos.Y);
         Vars = new Variable[nVars];
      }
      public MeshNode(Pos pos, Variable[] vars) {
         Pos = pos;
         Vars = vars;
      }

      public static MeshNode operator + (MeshNode left, MeshNode right) {
         if(left.Pos.Equals(right.Pos)) {
            var result = new MeshNode(left.Pos, 8);
            for(int varInx = 0; varInx < 8; ++varInx)
               result.Vars[varInx].Val = left.Vars[varInx].Val + right.Vars[varInx].Val;
            return result; }
         else
            throw new ArgumentException("The two nodes being summed are not at the same position.");
      }
      /// <summary>Compare two nodes based on positions.</summary><param name="other">Other Node to compare to.</param>
      public bool Equals(MeshNode other) {
         if(Pos.Equals(in other.Pos))
            return true;
         return false;
      }
   }
}