using System.Collections.Generic;

namespace cscc.Parsing.Ast
{
    class ParameterTypeListAstNode : AstNode
    {
        public IReadOnlyList<ParameterDeclarationAstNode>? ParameterList { get; }
        public bool VarArgs { get; }
        public ParameterTypeListAstNode(
            IReadOnlyList<ParameterDeclarationAstNode>? parameterList,
            bool varArgs, int line, int column)
            : base(line, column)
        {
            ParameterList = parameterList;
            VarArgs = varArgs;
        }
    }
}