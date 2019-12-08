using System.Collections.Generic;

namespace cscc.Parsing.Ast
{
    public class CompoundStatementAstNode : StatementAstNode
    {
        public IReadOnlyList<DeclarationAstNode> DeclarationList { get; }
        public IReadOnlyList<StatementAstNode> StatementList { get; }  
        public CompoundStatementAstNode(
            IReadOnlyList<DeclarationAstNode> declarationList,
            IReadOnlyList<StatementAstNode> statementList,
            int line, int column)
            : base(line, column)
        {
            DeclarationList = declarationList;
            StatementList = statementList;
        }
    }
}