#if false
using System;

using System.Collections.Generic;

namespace Fluid.Internals.Collections
{
    /// <summary>A network with nodes of type T.</summary><typeparam name="T">Type of nodes.</typeparam>
    public abstract class Network<T>
    {
        protected HashSet<T> _Nodes;
        public HashSet<T> Nodes => _Nodes;


        public Network() {
            _Nodes = new HashSet<T>();
        }

        /// <summary>Add a node to network.</summary><param name="node">Node to add.</param>
        public virtual void Add(T node) {
            Nodes.Add(node);
        }
        
        /// <summary>Add multiple nodes to network.</summary><param name="nodes">Nodes to add.</param>
        public virtual void Add(IEnumerable<T> nodes) {
                
            foreach(var node in nodes)
                Nodes.Add(node);
        }
    }
}
#endif