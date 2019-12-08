using CParser.Parsing.Ast;

namespace CParser.Translation
{
    // Implements DefaultAstVisitor using empty visitors,
    // so that inheritors may implement a subset of the visitors.
    public class EmptyAstVisitor : DefaultAstVisitor
    {
        public override void VisitAddAssignment(AssignmentExpressionAstNode node)
        {
        }

        public override void VisitAdditionExpression(AdditiveExpressionAstNode node)
        {
        }

        public override void VisitAddressOfExpression(UnaryExpressionAstNode node)
        {
        }

        public override void VisitAndAssignment(AssignmentExpressionAstNode node)
        {
        }

        public override void VisitAndExpression(AndExpressionAstNode node)
        {
        }

        public override void VisitAssignment(AssignmentExpressionAstNode node)
        {
        }

        public override void VisitBitFieldDeclarator(BitFieldDeclaratorAstNode node)
        {
        }

        public override void VisitBreakStatement(BreakStatementAstNode node)
        {
        }

        public override void VisitCaseLabel(CaseLabelAstNode node)
        {
        }

        public override void VisitCastExpression(CastExpressionAstNode node)
        {
        }

        public override void VisitCharacterConstant(CharacterConstantAstNode node)
        {
        }

        public override void VisitComplementExpression(UnaryExpressionAstNode node)
        {
        }

        public override void VisitCompoundStatement(CompoundStatementAstNode node)
        {
        }

        public override void VisitConditionalExpression(ConditionalExpressionAstNode node)
        {
        }

        public override void VisitContinueStatement(ContinueStatementAstNode node)
        {
        }

        public override void VisitDeclarationStatement(DeclarationStatementAstNode node)
        {
        }

        public override void VisitDecrementExpression(UnaryExpressionAstNode node)
        {
        }

        public override void VisitDefaultLabel(DefaultLabelAstNode node)
        {
        }

        public override void VisitDivide(MultiplicativeExpressionAstNode node)
        {
        }

        public override void VisitDivideAssignment(AssignmentExpressionAstNode node)
        {
        }

        public override void VisitDoStatement(DoStatementAstNode node)
        {
        }

        public override void VisitEnumerationConstant(EnumerationConstantAstNode node)
        {
        }

        public override void VisitEnumerator(EnumeratorAstNode node)
        {
        }

        public override void VisitEnumSpecifier(EnumSpecifierAstNode node)
        {
        }

        public override void VisitEquality(EqualityExpressionAstNode node)
        {
        }

        public override void VisitExpression(InitializerExpressionAstNode node)
        {
        }

        public override void VisitExpressionList(ExpressionListAstNode node)
        {
        }

        public override void VisitExpressionStatement(ExpressionStatementAstNode node)
        {
        }

        public override void VisitFloatingConstant(FloatingConstantAstNode node)
        {
        }

        public override void VisitForStatement(ForStatementAstNode node)
        {
        }

        public override void VisitFunctionDefinition(FunctionDefinitionAstNode node)
        {
        }

        public override void VisitGotoStatement(GotoStatementAstNode node)
        {
        }

        public override void VisitGreaterThan(RelationalExpressionAstNode node)
        {
        }

        public override void VisitGreaterThanOrEqual(RelationalExpressionAstNode node)
        {
        }

        public override void VisitIdentifier(IdentifierAstNode node)
        {
        }

        public override void VisitIdentifierDeclarator(IdentifierDeclaratorAstNode node)
        {
        }

        public override void VisitIdentifierLabel(IdentifierLabelAstNode node)
        {
        }

        public override void VisitIfStatement(IfStatementAstNode node)
        {
        }

        public override void VisitIncrementExpression(UnaryExpressionAstNode node)
        {
        }

        public override void VisitIndexedDeclarator(IndexedDeclaratorAstNode node)
        {
        }

        public override void VisitInequality(EqualityExpressionAstNode node)
        {
        }

        public override void VisitInitDeclarator(InitDeclaratorAstNode node)
        {
        }

        public override void VisitInitializer(InitializerAstNode node)
        {
        }

        public override void VisitInitializerList(InitializerListAstNode node)
        {
        }

        public override void VisitIntegerConstant(IntegerConstantAstNode node)
        {
        }

        public override void VisitLabeledStatement(LabeledStatementAstNode node)
        {
        }

        public override void VisitLessThan(RelationalExpressionAstNode node)
        {
        }

        public override void VisitLessThanOrEqual(RelationalExpressionAstNode node)
        {
        }

        public override void VisitLogicalAndExpression(LogicalAndExpressionAstNode node)
        {
        }

        public override void VisitLogicalOrExpression(LogicalOrExpressionAstNode node)
        {
        }

        public override void VisitMinusExpression(UnaryExpressionAstNode node)
        {
        }

        public override void VisitMod(MultiplicativeExpressionAstNode node)
        {
        }

        public override void VisitModAssignment(AssignmentExpressionAstNode node)
        {
        }

        public override void VisitMultiply(MultiplicativeExpressionAstNode node)
        {
        }

        public override void VisitMultiplyAssignment(AssignmentExpressionAstNode node)
        {
        }

        public override void VisitNegationExpression(UnaryExpressionAstNode node)
        {
        }

        public override void VisitNestedDeclarator(NestedDeclaratorAstNode node)
        {
        }

        public override void VisitOldStyleParameterizedDeclarator(OldStyleParameterizedDeclaratorAstNode node)
        {
        }

        public override void VisitOrAssignment(AssignmentExpressionAstNode node)
        {
        }

        public override void VisitOrExpression(OrExpressionAstNode node)
        {
        }

        public override void VisitParameterDeclaration(ParameterDeclarationAstNode node)
        {
        }

        public override void VisitParameterizedDeclarator(ParameterizedDeclaratorAstNode node)
        {
        }

        public override void VisitParameterTypeList(ParameterTypeListAstNode node)
        {
        }

        public override void VisitPlusExpression(UnaryExpressionAstNode node)
        {
        }

        public override void VisitPointer(PointerAstNode node)
        {
        }

        public override void VisitPointerDeclarator(PointerDeclaratorAstNode node)
        {
        }

        public override void VisitPointerDereferenceExpression(UnaryExpressionAstNode node)
        {
        }

        public override void VisitPostfixCallExpression(PostfixCallExpressionAstNode node)
        {
        }

        public override void VisitPostfixDecrement(PostfixExpressionAstNode node)
        {
        }

        public override void VisitPostfixIncrement(PostfixExpressionAstNode node)
        {
        }

        public override void VisitPostfixIndexerExpression(PostfixIndexerExpressionAstNode node)
        {
        }

        public override void VisitPostfixMemberAccessExpression(PostfixMemberAccessExpressionAstNode node)
        {
        }

        public override void VisitPostfixPointerAccessExpression(PostfixPointerAccessExpressionAstNode node)
        {
        }

        public override void VisitPostfixUnaryExpression(PostfixUnaryExpressionAstNode node)
        {
        }

        public override void VisitReturnStatement(ReturnStatementAstNode node)
        {
        }

        public override void VisitShiftLeft(ShiftExpressionAstNode node)
        {
        }

        public override void VisitShiftLeftAssignment(AssignmentExpressionAstNode node)
        {
        }

        public override void VisitShiftRight(ShiftExpressionAstNode node)
        {
        }

        public override void VisitShiftRightAssignment(AssignmentExpressionAstNode node)
        {
        }

        public override void VisitSizeofExpression(UnaryExpressionAstNode node)
        {
        }

        public override void VisitSizeofTypeExpression(SizeofTypeExpressionAstNode node)
        {
        }

        public override void VisitStorageClassSpecifier(StorageClassSpecifierAstNode node)
        {
        }

        public override void VisitStringLiteral(StringLiteralAstNode node)
        {
        }

        public override void VisitStruct(StructAstNode node)
        {
        }

        public override void VisitStructDeclaration(StructDeclarationAstNode node)
        {
        }

        public override void VisitSubtractAssignment(AssignmentExpressionAstNode node)
        {
        }

        public override void VisitSubtractionExpression(AdditiveExpressionAstNode node)
        {
        }

        public override void VisitSwitchStatement(SwitchStatementAstNode node)
        {
        }

        public override void VisitTypedefName(TypedefNameAstNode node)
        {
        }

        public override void VisitTypeName(TypeNameAstNode node)
        {
        }

        public override void VisitTypeQualifier(TypeQualifierAstNode node)
        {
        }

        public override void VisitTypeSpecifier(TypeSpecifierAstNode node)
        {
        }

        public override void VisitUnion(UnionAstNode node)
        {
        }

        public override void VisitVariableDeclaration(VariableDeclarationAstNode node)
        {
        }

        public override void VisitWhileStatement(WhileStatementAstNode node)
        {
        }

        public override void VisitXorAssignment(AssignmentExpressionAstNode node)
        {
        }

        public override void VisitXorExpression(XorExpressionAstNode node)
        {
        }
    }
}