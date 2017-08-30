namespace MSBuildExpressionParser
{
    /// <summary>
    ///     Well-known node types.
    /// </summary>
    enum NodeType
    {
        /// <summary>
        ///     A quoted string, "'xxx'".
        /// </summary>
        QuotedString,

        /// <summary>
        ///     Contiguous characters in a quoted string.
        /// </summary>
        StringCharacters,

        /// <summary>
        ///     An identifier, "xxx123" or "xxx123.yyy456".
        /// </summary>
        Identifier,

        /// <summary>
        ///     An MSBuild evaluation expression, "$(xxx)".
        /// </summary>
        Eval,

        /// <summary>
        ///     An MSBuild type-reference expression, "[xxx]::".
        /// </summary>
        TypeRef,

        /// <summary>
        ///     A binary operator, "==" or "!=" or "||" or "&amp;&amp;".
        /// </summary>
        BinaryOperator, // TODO: Actually support this

        /// <summary>
        ///     A unary operator, "!".
        /// </summary>
        UnaryOperator,  // TODO: Actually support this

        /// <summary>
        ///     Contiguious whitespace.
        /// </summary>
        Whitespace
    }
}
