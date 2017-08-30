using System;

namespace MSBuildExpressionParser
{
    /// <summary>
    ///     Demo of using Sprache to parse MSBuild expression.
    /// </summary>
    /// <remarks>
    ///     TODO: Item group and item metadata syntax.
    /// </remarks>
    static class Program
    {
        /// <summary>
        ///     The main program entry-point.
        /// </summary>
        static void Main()
        {
            try
            {
                const string testData = " 'Foo == Bar' 'a'  'Diddly O\\'Dee' $(Foo) $([MSBuild]::Hello) 'This is $(Me) talking $(BarBaz)' '$([Foo]::Diddly)'   '     a ' ";
                foreach (Node node in MSBuildSyntax.ParseExpression(testData))
                    DumpNode(node, depth: 0);
            }
            catch (Exception unexpectedError)
            {
                Console.WriteLine(unexpectedError);
            }
        }

        /// <summary>
        ///     Dump a <see cref="Node"/>'s details to the console.
        /// </summary>
        /// <param name="node">
        ///     The <see cref="Node"/>.
        /// </param>
        /// <param name="depth">
        ///     The node's depth in the tree.
        /// </param>
        static void DumpNode(Node node, int depth)
        {
            if (node.Children != null)
            {
                Console.WriteLine("{0}{1}:",
                    new String(' ', depth * 2),
                    node.NodeType
                );

                foreach (Node childNode in node.Children)
                    DumpNode(childNode, depth + 1);
            }
            else
            {
                Console.WriteLine("{0}{1} '{2}' ({3}..{4})",
                    new String(' ', depth * 2),
                    node.NodeType,
                    node.Value,
                    node.Start,
                    node.End
                );
            }
        }
    }
}
