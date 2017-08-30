using Sprache;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MSBuildExpressionParser
{
    /// <summary>
    ///     Parsing for MSBuild expression syntax.
    /// </summary>
    static class MSBuildSyntax
    {
        /// <summary>
        ///     Parsers for well-known tokens.
        /// </summary>
        public static class Tokens
        {
            /// <summary>
            ///     An identifier (1 letter followed by 0 or more letters / digits).
            /// </summary>
            public static readonly Parser<string> Identifier = Parse.Identifier(Parse.Letter, Parse.LetterOrDigit);

            /// <summary>
            ///     A qualified identifier (one or more identifiers, delimited by ".").
            /// </summary>
            public static readonly Parser<string> QualifiedIdentifier =
                from identifiers in Identifier.DelimitedBy(Parse.Char('.'))
                select string.Join(".", identifiers);

            /// <summary>
            ///     A single quote, "'".
            /// </summary>
            public static readonly Parser<char> SingleQuote = Parse.Char('\'');

            /// <summary>
            ///     An escaped character.
            /// </summary>
            public static readonly Parser<char> EscapedCharacter = Parse.Char('\\').Then(_ => Parse.AnyChar);

            /// <summary>
            ///     The opening part of an MSBuild evaluation expression, "$(".
            /// </summary>
            public static readonly Parser<IEnumerable<char>> EvalOpen = Parse.String("$(");

            /// <summary>
            ///     The closing part of an MSBuild evaluation expression, ")".
            /// </summary>
            public static readonly Parser<char> EvalClose = Parse.Char(')');

            /// <summary>
            ///     The opening part of an MSBuild type-reference expression, "[".
            /// </summary>
            public static readonly Parser<char> TypeRefOpen = Parse.Char('[');

            /// <summary>
            ///     The closing part of an MSBuild type-reference expression, "]::".
            /// </summary>
            public static readonly Parser<IEnumerable<char>> TypeRefClose = Parse.String("]::");

            /// <summary>
            ///     Parsers for well-known operator tokens.
            /// </summary>
            public static class Operator
            {
                /// <summary>
                ///     Parser for the equality operator, "==".
                /// </summary>
                public static Parser<string> Equal = Parse.String("==").Text();

                /// <summary>
                ///     Parser for the inequality operator, "1=".
                /// </summary>
                public static Parser<string> NotEqual = Parse.String("==").Text();

                /// <summary>
                ///     Parser for the logical-OR operator, "Or".
                /// </summary>
                public static Parser<string> LogicalOr = Parse.String("Or").Text();

                /// <summary>
                ///     Parser for the logical-AND operator, "And".
                /// </summary>
                public static Parser<string> LogicalAnd = Parse.String("And").Text();

                /// <summary>
                ///     Parser for binary operator tokens, "==" or "!=" or "Or" or "And".
                /// </summary>
                public static readonly Parser<Node> Binary = Parse.Positioned(
                    from operatorText in Equal.Or(NotEqual).Or(LogicalOr).Or(LogicalAnd).Text()
                    select new Node
                    {
                        NodeType = NodeType.BinaryOperator,
                        Value = operatorText
                    }
                );

                /// <summary>
                ///     Parser for unary operators ("!").
                /// </summary>
                public static readonly Parser<Node> Unary = Parse.Positioned(
                    from operatorText in Parse.Char('!')
                    select new Node
                    {
                        NodeType = NodeType.BinaryOperator,
                        Value = operatorText.ToString()
                    }
                );
            }
        }

        /// <summary>
        ///     Parse contiguous whitespace.
        /// </summary>
        public static readonly Parser<Node> Whitespace = Parse.Positioned(
            from ws in Parse.WhiteSpace.Many().Text()
            select new Node
            {
                NodeType = NodeType.Whitespace,
                Value = ws
            }
        );

        /// <summary>
        ///     Parse an identifier, "A123".
        /// </summary>
        public static readonly Parser<Node> Identifier = Parse.Positioned(
            from identifier in Tokens.Identifier
            select new Node
            {
                NodeType = NodeType.Identifier,
                Value = identifier
            }
        );

        /// <summary>
        ///     Parse a qualified identifier, "A123.B456".
        /// </summary>
        public static readonly Parser<Node> QualifiedIdentifier = Parse.Positioned(
            from identifier in Tokens.QualifiedIdentifier
            select new Node
            {
                NodeType = NodeType.Identifier,
                Value = identifier
            }
        );

        /// <summary>
        ///     Parse an MSBuild evaluation expression, "$(xxx)".
        /// </summary>
        public static readonly Parser<Node> Eval = Parse.Positioned(
            from content in Parse.Contained(
                TypeRef.Or(Identifier).Many(),
                open: Tokens.EvalOpen,
                close: Tokens.EvalClose
            )
            select new Node
            {
                NodeType = NodeType.Eval,
                Children = content.ToArray()
            }
        );

        /// <summary>
        ///     Parse an MSBuild type-reference expression, "[xxx]::".
        /// </summary>
        public static Parser<Node> TypeRef = Parse.Positioned(
            from open in Tokens.TypeRefOpen
            from typeName in Tokens.QualifiedIdentifier
            from close in Tokens.TypeRefClose
            select new Node
            {
                NodeType = NodeType.TypeRef,
                Value = typeName
            }
        );

        /// <summary>
        ///     Parse contiguous characters in a string that have no special meaning.
        /// </summary>
        public static readonly Parser<Node> StringCharacters = Parse.Positioned(
            from characters in
                Tokens.EscapedCharacter.Or(
                    Parse.AnyChar
                        .Except(Tokens.SingleQuote)
                        .Except(Tokens.EvalOpen)
                )
                .AtLeastOnce()
                .Text()
            select new Node
            {
                NodeType = NodeType.StringCharacters,
                Value = characters
            }
        );

        /// <summary>
        ///     Parse a quoted string, "'xxx'".
        /// </summary>
        public static readonly Parser<Node> QuotedString = Parse.Positioned(
            from content in Parse.Contained(
                Eval.Or(StringCharacters).Many(),
                open: Tokens.SingleQuote,
                close: Tokens.SingleQuote
            )
            select new Node
            {
                NodeType = NodeType.QuotedString,
                Children = content.ToArray()
            }
        );

        /// <summary>
        ///     Parse a binary expression.
        /// </summary>
        public static readonly Parser<Node> Binary = Parse.Positioned(
            Parse.ChainOperator(Tokens.Operator.Binary, Eval.Or(QuotedString),
                (op, left, right) =>
                {
                    op.Children = new Node[] { left, right };

                    return op;
                }
            )
        );

        /// <summary>
        ///     Parse an MSBuild expression.
        /// </summary>
        /// <param name="expression">
        ///     The expression to parse.
        /// </param>
        /// <returns>
        ///     A sequence of <see cref="Node"/>s representing the syntax tree.
        /// </returns>
        public static IEnumerable<Node> ParseExpression(string expression)
        {
            IResult<IEnumerable<Node>> result = QuotedString.Or(Eval).Or(Whitespace).Many().TryParse(expression);
            if (result.WasSuccessful)
                return result.Value;

            throw new FormatException(
                result.Message
                + "\nExpectations: ["
                + String.Join(",", result.Expectations.Select(
                    expectation => String.Format("\"{0}\"", expectation)
                ))
                + "]"
            );
        }
    }
}
