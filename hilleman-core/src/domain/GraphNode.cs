using System;
using System.Collections.Generic;
using System.Linq;

namespace com.bitscopic.hilleman.core.domain
{
    public class GraphNode<T>
    {
        public T value;

        public GraphNode() { }

        public GraphNode(T value)
        {
            this.value = value;
        }

        public void addNeighbor(GraphNode<T> node)
        {
            if (_neighbors == null)
            {
                _neighbors = new List<GraphNode<T>>();
            }

            _neighbors.Add(node);
        }

        List<GraphNode<T>> _neighbors;

        public List<GraphNode<T>> getNeighbors()
        {
            return _neighbors;
        }

        /// <summary>
        /// Checks for match in neighbors using T.Equals(neighbor) function
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public bool isNeighbor(GraphNode<T> t)
        {
            if (_neighbors == null || _neighbors.Count == 0)
            {
                return false;
            }

            return _neighbors.Any(neighbor => neighbor.value.Equals(t.value));
        }

        public bool isNeighbor(T t)
        {
            if (_neighbors == null || _neighbors.Count == 0)
            {
                return false;
            }

            return _neighbors.Any(neighbor => neighbor.value.Equals(t));
        }

    }
}