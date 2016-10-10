using System;
using System.Collections.Generic;

namespace AdvancedDataStructures
{
    public class DisjointSetForest
    {
        private Node[] _nodes = new Node[4];

        /// <summary>
        /// Gets the count of disjoint sets that are currently entered in this forest.
        /// </summary>
        public int NumDisjointSets { get; private set; }
        public int NumNodes { get; private set; }

        public int Add()
        {
            if (NumNodes >= _nodes.Length)
                Array.Resize(ref _nodes, NumNodes * 2);

            int index = NumNodes;
            _nodes[index].Parent = index;
            NumDisjointSets++;
            NumNodes++;

            return index;
        }

        public int FindSet(int node)
        {
            if (node < 0 || node >= _nodes.Length)
                throw new ArgumentOutOfRangeException(nameof(node), node,
                                                      "Node must be positive and less than number of nodes");

            return FindSetInternal(node);
        }

        private int FindSetInternal(int node)
        {
            int parent = _nodes[node].Parent;
            if (parent != node)
                _nodes[node].Parent = parent = FindSetInternal(parent);

            return parent;
        }

        public bool Union(int x, int y)
        {
            x = FindSet(x);
            y = FindSet(y);

            if (x == y)
                return false;

            // Make smallest a child of the largest
            if (_nodes[x].Rank > _nodes[y].Rank)
                _nodes[y].Parent = x;
            else
            {
                _nodes[x].Parent = y;
                if (_nodes[x].Rank == _nodes[y].Rank)
                    _nodes[y].Rank++;
            }

            NumDisjointSets--;
            return true;
        }

        private struct Node
        {
            public int Parent;
            public int Rank;
        }
    }
}