using System;

using Fluid.Internals.Numerics;

namespace Fluid.Internals.Meshing {
   public struct MeshNode : IEquatable<MeshNode> {//TODO: Read more about ref structs.
      /// <summary>Node's position (x,y).</summary>
      public Pos _Pos;
      /// <summary>An array of doubles. One double for each variable.</summary>
      Variable[] _Vars;

      /// <summary>Node's position (x,y).</summary>
      public Pos GetPos() => _Pos;
      /// <summary>Set Node's position (x,y).</summary>
      public void SetPos(double x, double y) {
         _Pos.X = x;
         _Pos.Y = y;
      }
      public double GetX() => _Pos.X;
      public double GetY() => _Pos.Y;
      /// <summary>Get variable array.</summary>
      public Variable[] Vars => _Vars;
      /// <summary>Constrainedness of variable at specified index.</summary><param name="varIndex">Variable index.</param>
      public ref bool Constrainedness(int varIndex) => ref _Vars[varIndex]._constrained;
      /// <summary>Value of variable at specified index.</summary><param name="varIndex">Variable index.</param>
      public ref Variable Var(int varIndex) => ref _Vars[varIndex];


      /// <summary>Create a Node with specified coordinates and 0.0 values for variables, all of which are set unconstrained.</summary><param name="x">Node's x position.</param><param name="y">Node's y position.</param><param name="nVars">Number of variables.</param>
      public MeshNode(double x, double y, int nVars) {
         _Pos = new Pos(x,y);
         _Vars = new Variable[nVars];
      }

      public MeshNode(Pos pos, int nVars) {
         _Pos = new Pos(pos.X, pos.Y);
         _Vars = new Variable[nVars];
      }

      public MeshNode(Pos pos, Variable[] vars) {
         _Pos = pos;
         _Vars = vars;
      }


      public static MeshNode operator + (MeshNode left, MeshNode right) {

         if(left._Pos.Equals(right._Pos)) {
               var result = new MeshNode(left._Pos, 8);

               for(int var = 0; var < 8; ++var) {
                  result.Var(var)._value = left.Var(var)._value + right.Var(var)._value;
               }
               return result;
         }
         else {
               throw new ArgumentException("The two nodes being summed are not at the same position.");
         }
      }


      /// <summary>Contains value and a flag indicating whether a NodeValue is free or constrained.</summary><remarks>This is a value type. When assigning, always create a new one.</remarks>
      public struct Variable
      {   
         /// <summary>Value at node.</summary>
         public double _value;
         /// <summary>True if node is constrained.</summary>
         public bool _constrained;
      }

      /// <summary>Compare two nodes based on positions.</summary><param name="other">Other Node to compare to.</param>
      public bool Equals(MeshNode other) {
         if(_Pos.Equals(other._Pos)) {
               return true;
         }
         return false;
      }
   }
}