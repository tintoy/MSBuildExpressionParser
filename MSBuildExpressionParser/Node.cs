using Sprache;

namespace MSBuildExpressionParser
{
    /// <summary>
    ///     A node in the abstract syntax tree.
    /// </summary>
    class Node
            : IPositionAware<Node>
    {
        /// <summary>
        ///     The type of node.
        /// </summary>
        public NodeType NodeType { get; set; }

        /// <summary>
        ///     The node value (<c>null</c> if the node has children).
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        ///     The node's children (<c>null</c> if the node has no children).
        /// </summary>
        public Node[] Children { get; set; }

        /// <summary>
        ///     The node's absolute starting position in the source text.
        /// </summary>
        public int Start { get; set; }

        /// <summary>
        ///     The node's ending position in the source text.
        /// </summary>
        public int End { get; set; }

        /// <summary>
        ///     Set the node's position information (called by Sprache while parsing).
        /// </summary>
        /// <param name="startPosition">
        ///     The node's starting position.
        /// </param>
        /// <param name="length">
        ///     The node's length, in characters.
        /// </param>
        /// <returns>
        ///     The <see cref="Node"/>.
        /// </returns>
        Node IPositionAware<Node>.SetPos(Position startPosition, int length)
        {
            Start = startPosition.Pos;
            End = startPosition.Pos + length;

            return this;
        }
    }
}
