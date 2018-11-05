using System;
using Fluid.Dynamics.Numerics;

namespace Fluid.Dynamics.Meshing
{
    public struct Node : IEquatable<Node>
    {
        /// <summary>Node's position (x,y).</summary>
        public Pos _pos;
        /// <summary>An array of doubles. One double for each variable.</summary>
        Variable[] _vars;

        /// <summary>Node's position (x,y).</summary>
        public Pos GetPos() => _pos;
        /// <summary>Set Node's position (x,y).</summary>
        public void SetPos(double x, double y) {
            _pos._x = x;
            _pos._y = y;
        }
        public double GetX() => _pos._x;
        public double GetY() => _pos._y;
        /// <summary>Get variable array.</summary>
        public Variable[] Vars => _vars;
        /// <summary>Constrainedness of variable at specified index.</summary><param name="varIndex">Variable index.</param>
        public ref bool Constrainedness(int varIndex) => ref _vars[varIndex]._constrained;
        /// <summary>Value of variable at specified index.</summary><param name="varIndex">Variable index.</param>
        public ref Variable Var(int varIndex) => ref _vars[varIndex];


        /// <summary>Create a Node with specified coordinates and 0.0 values for variables, all of which are set unconstrained.</summary><param name="x">Node's x position.</param><param name="y">Node's y position.</param><param name="nVars">Number of variables.</param>
        public Node(double x, double y, int nVars) {
            _pos = new Pos(x,y);
            _vars = new Variable[nVars];
        }

        public Node(Pos pos, int nVars) {
            _pos = new Pos(pos._x, pos._y);
            _vars = new Variable[nVars];
        }

        public Node(Pos pos, Variable[] vars) {
            _pos = pos;
            _vars = vars;
        }


        public static Node operator + (Node left, Node right) {

            if(left._pos.Equals(right._pos)) {
                var result = new Node(left._pos, 8);

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
        public bool Equals(Node other) {
            if(_pos.Equals(other._pos)) {
                return true;
            }
            return false;
        }
    }
}