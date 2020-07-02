using System;
using System.Collections.Generic;
using System.Linq;

namespace com.bitscopic.hilleman.core.domain
{
    public class DirectedGraph<T>
    {
        List<GraphNode<T>> _nodes;

        internal List<GraphNode<T>> getAllNodes()
        {
            return _nodes;
        }

        public void addNode(GraphNode<T> node)
        {
            if (_nodes == null)
            {
                _nodes = new List<GraphNode<T>>();
            }

            _nodes.Add(node);
        }

        public GraphNode<T> findNode(GraphNode<T> t)
        {
            if (_nodes == null || _nodes.Count == 0)
            {
                throw new ArgumentException("Graph nodes have not been initialized");
            }

            foreach (GraphNode<T> node in _nodes)
            {
                if (node.value.Equals(t.value))
                {
                    return node;
                }
            }

            throw new ArgumentException("Graph node doesn't exist");
        }

        public GraphNode<T> findNode(T t)
        {
            return this.findNode(new GraphNode<T>(t));
        }
    }
}