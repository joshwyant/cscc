using cscc.Parsing.Ast;

namespace cscc.Translation
{
    interface IAstVisitor
    {
        void VisitAdditiveExpression(AdditiveExpressionAstNode node);
        void VisitAndExpression(AndExpressionAstNode node);
        void VisitAssignmentExpression(AssignmentExpressionAstNode node);
        void VisitBinaryExpression(BinaryExpressionAstNode node);
        // ... TODO
    }
}