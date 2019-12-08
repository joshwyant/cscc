using System;
using cscc.Parsing.Ast;
using static cscc.Lexing.Terminal;
using cscc.Lexing;

namespace cscc.Translation
{
    public abstract class DefaultAstVisitor : IAstVisitor
    {
        #region Concrete visitors
        public void Visit(AstNode n)
        {
            switch (n)
            {
                case DeclarationAstNode node:
                    VisitDeclaration(node);
                    break;
                case DeclaratorAstNode node:
                    VisitDeclarator(node);
                    break;
                case EnumeratorAstNode node:
                    VisitEnumerator(node);
                    break;
                case ExpressionAstNode node:
                    VisitExpression(node);
                    break;
                case LabelAstNode node:
                    VisitLabel(node);
                    break;
                case ParameterDeclarationAstNode node:
                    VisitParameterDeclaration(node);
                    break;
                case ParameterTypeListAstNode node:
                    VisitParameterTypeList(node);
                    break;
                case PointerAstNode node:
                    VisitPointer(node);
                    break;
                case SpecifierAstNode node:
                    VisitSpecifier(node);
                    break;
                case StatementAstNode node:
                    VisitStatement(node);
                    break;
                case TypeNameAstNode node:
                    VisitTypeName(node);
                    break;
                default:
                    throw new NotImplementedException(n.GetType().Name);
            }
        }
        public void VisitAdditiveExpression(AdditiveExpressionAstNode node)
        {
            switch (node.Terminal)
            {
                case Plus:
                    VisitAdditionExpression(node);
                    break;
                case Minus:
                    VisitSubtractionExpression(node);
                    break;
                default:
                    throw new NotImplementedException(System.Enum.GetName(typeof(Terminal), node.Terminal));
            }
        }
        public void VisitAssignmentExpression(AssignmentExpressionAstNode node)
        {
            switch (node.Terminal)
            {
                case Assign:
                    VisitAssignment(node);
                    break;
                case MultiplyAssign:
                    VisitMultiplyAssignment(node);
                    break;
                case DivideAssign:
                    VisitDivideAssignment(node);
                    break;
                case ModAssign:
                    VisitModAssignment(node);
                    break;
                case AddAssign:
                    VisitAddAssignment(node);
                    break;
                case SubtractAssign:
                    VisitSubtractAssignment(node);
                    break;
                case ShiftLeftAssign:
                    VisitShiftLeftAssignment(node);
                    break;
                case ShiftRightAssign:
                    VisitShiftRightAssignment(node);
                    break;
                case AndAssign:
                    VisitAndAssignment(node);
                    break;
                case XorAssign:
                    VisitXorAssignment(node);
                    break;
                case OrAssign:
                    VisitOrAssignment(node);
                    break;
                default:
                    throw new NotImplementedException(System.Enum.GetName(typeof(Terminal), node.Terminal));
            }
        }
        public void VisitBinaryExpression(BinaryExpressionAstNode n)
        {
            switch (n)
            {
                case AdditiveExpressionAstNode node:
                    VisitAdditiveExpression(node);
                    break;
                case AndExpressionAstNode node:
                    VisitAndExpression(node);
                    break;
                case AssignmentExpressionAstNode node:
                    VisitAssignmentExpression(node);
                    break;
                case EqualityExpressionAstNode node:
                    VisitEqualityExpression(node);
                    break;
                case LogicalAndExpressionAstNode node:
                    VisitLogicalAndExpression(node);
                    break;
                case LogicalOrExpressionAstNode node:
                    VisitLogicalOrExpression(node);
                    break;
                case MultiplicativeExpressionAstNode node:
                    VisitMultiplicativeExpression(node);
                    break;
                case OrExpressionAstNode node:
                    VisitOrExpression(node);
                    break;
                case RelationalExpressionAstNode node:
                    VisitRelationalExpression(node);
                    break;
                case ShiftExpressionAstNode node:
                    VisitShiftExpression(node);
                    break;
                case XorExpressionAstNode node:
                    VisitXorExpression(node);
                    break;
                default:
                    throw new NotImplementedException(n.GetType().Name);
            }
        }
        public void VisitConstantExpression(ConstantExpressionAstNode n)
        {
            switch (n)
            {
                case CharacterConstantAstNode node:
                    VisitCharacterConstant(node);
                    break;
                case EnumerationConstantAstNode node:
                    VisitEnumerationConstant(node);
                    break;
                case FloatingConstantAstNode node:
                    VisitFloatingConstant(node);
                    break;
                case IntegerConstantAstNode node:
                    VisitIntegerConstant(node);
                    break;
                default:
                    throw new NotImplementedException(n.GetType().Name);
            }
        }
        public void VisitDeclaration(DeclarationAstNode n)
        {
            switch (n)
            {
                case StructDeclarationAstNode node:
                    VisitStructDeclaration(node);
                    break;
                case VariableDeclarationAstNode node:
                    VisitVariableDeclaration(node);
                    break;
                default:
                    throw new NotImplementedException(n.GetType().Name);
            }
        }
        public void VisitDeclarator(DeclaratorAstNode n)
        {
            switch (n)
            {
                case BitFieldDeclaratorAstNode node:
                    VisitBitFieldDeclarator(node);
                    break;
                case IdentifierDeclaratorAstNode node:
                    VisitIdentifierDeclarator(node);
                    break;
                case IndexedDeclaratorAstNode node:
                    VisitIndexedDeclarator(node);
                    break;
                case InitDeclaratorAstNode node:
                    VisitInitDeclarator(node);
                    break;
                case NestedDeclaratorAstNode node:
                    VisitNestedDeclarator(node);
                    break;
                case OldStyleParameterizedDeclaratorAstNode node:
                    VisitOldStyleParameterizedDeclarator(node);
                    break;
                case ParameterizedDeclaratorAstNode node:
                    VisitParameterizedDeclarator(node);
                    break;
                case PointerDeclaratorAstNode node:
                    VisitPointerDeclarator(node);
                    break;
                default:
                    throw new NotImplementedException(n.GetType().Name);
            }
        }
        public void VisitEqualityExpression(EqualityExpressionAstNode node)
        {
            switch (node.Terminal)
            {
                case DoubleEquals:
                    VisitEquality(node);
                    break;
                case NotEqual:
                    VisitInequality(node);
                    break;
                default:
                    throw new NotImplementedException(System.Enum.GetName(typeof(Terminal), node.Terminal));
            }
        }
        public void VisitExpression(ExpressionAstNode n)
        {
            switch (n)
            {
                case BinaryExpressionAstNode node:
                    VisitBinaryExpression(node);
                    break;
                case CastExpressionAstNode node:
                    VisitCastExpression(node);
                    break;
                case ConditionalExpressionAstNode node:
                    VisitConditionalExpression(node);
                    break;
                case ConstantExpressionAstNode node:
                    VisitConstantExpression(node);
                    break;
                case ExpressionListAstNode node:
                    VisitExpressionList(node);
                    break;
                case IdentifierAstNode node:
                    VisitIdentifier(node);
                    break;
                case InitializerAstNode node:
                    VisitInitializer(node);
                    break;
                case PostfixCallExpressionAstNode node:
                    VisitPostfixCallExpression(node);
                    break;
                case PostfixExpressionAstNode node:
                    VisitPostfixExpression(node);
                    break;
                case PostfixIndexerExpressionAstNode node:
                    VisitPostfixIndexerExpression(node);
                    break;
                case PostfixMemberAccessExpressionAstNode node:
                    VisitPostfixMemberAccessExpression(node);
                    break;
                case PostfixPointerAccessExpressionAstNode node:
                    VisitPostfixPointerAccessExpression(node);
                    break;
                case PostfixUnaryExpressionAstNode node:
                    VisitPostfixUnaryExpression(node);
                    break;
                case SizeofTypeExpressionAstNode node:
                    VisitSizeofTypeExpression(node);
                    break;
                case StringLiteralAstNode node:
                    VisitStringLiteral(node);
                    break;
                case UnaryExpressionAstNode node:
                    VisitUnaryExpression(node);
                    break;
                default:
                    throw new NotImplementedException(n.GetType().Name);
            }
        }
        public void VisitJumpStatement(JumpStatementAstNode n)
        {
            switch (n)
            {
                case BreakStatementAstNode node:
                    VisitBreakStatement(node);
                    break;
                case ContinueStatementAstNode node:
                    VisitContinueStatement(node);
                    break;
                case GotoStatementAstNode node:
                    VisitGotoStatement(node);
                    break;
                case ReturnStatementAstNode node:
                    VisitReturnStatement(node);
                    break;
                default:
                    throw new NotImplementedException(n.GetType().Name);
            }
        }
        public void VisitLabel(LabelAstNode n)
        {
            switch (n)
            {
                case CaseLabelAstNode node:
                    VisitCaseLabel(node);
                    break;
                case DefaultLabelAstNode node:
                    VisitDefaultLabel(node);
                    break;
                case IdentifierLabelAstNode node:
                    VisitIdentifierLabel(node);
                    break;
                default:
                    throw new NotImplementedException(n.GetType().Name);
            }
        }
        public void VisitMultiplicativeExpression(MultiplicativeExpressionAstNode node)
        {
            switch (node.Terminal)
            {
                case Star:
                    VisitMultiply(node);
                    break;
                case Slash:
                    VisitDivide(node);
                    break;
                case Percent:
                    VisitMod(node);
                    break;
                default:
                    throw new NotImplementedException(System.Enum.GetName(typeof(Terminal), node.Terminal));
            }
        }
        public void VisitPostfixExpression(PostfixExpressionAstNode node)
        {
            switch (node.Terminal)
            {
                case Increment:
                    VisitPostfixIncrement(node);
                    break;
                case Decrement:
                    VisitPostfixDecrement(node);
                    break;
                default:
                    throw new NotImplementedException(System.Enum.GetName(typeof(Terminal), node.Terminal));
            }
        }
        public void VisitRelationalExpression(RelationalExpressionAstNode node)
        {
            switch (node.Terminal)
            {
                case LessThan:
                    VisitLessThan(node);
                    break;
                case LessThanOrEqual:
                    VisitLessThanOrEqual(node);
                    break;
                case GreaterThan:
                    VisitGreaterThan(node);
                    break;
                case GreaterThanOrEqual:
                    VisitGreaterThanOrEqual(node);
                    break;
                default:
                    throw new NotImplementedException(System.Enum.GetName(typeof(Terminal), node.Terminal));
            }
        }
        public void VisitShiftExpression(ShiftExpressionAstNode node)
        {
            switch (node.Terminal)
            {
                case ShiftLeft:
                    VisitShiftLeft(node);
                    break;
                case ShiftRight:
                    VisitShiftRight(node);
                    break;
                default:
                    throw new NotImplementedException(System.Enum.GetName(typeof(Terminal), node.Terminal));
            }
        }
        public void VisitSpecifier(SpecifierAstNode n)
        {
            switch (n)
            {
                case EnumSpecifierAstNode node:
                    VisitEnumSpecifier(node);
                    break;
                case StorageClassSpecifierAstNode node:
                    VisitStorageClassSpecifier(node);
                    break;
                case StructOrUnionSpecifierAstNode node:
                    VisitStructOrUnionSpecifier(node);
                    break;
                case TypedefNameAstNode node:
                    VisitTypedefName(node);
                    break;
                case TypeQualifierAstNode node:
                    VisitTypeQualifier(node);
                    break;
                case TypeSpecifierAstNode node:
                    VisitTypeSpecifier(node);
                    break;
                default:
                    throw new NotImplementedException(n.GetType().Name);
            }
        }
        public void VisitStatement(StatementAstNode n)
        {
            switch (n)
            {
                case CompoundStatementAstNode node:
                    VisitCompoundStatement(node);
                    break;
                case DeclarationStatementAstNode node:
                    VisitDeclarationStatement(node);
                    break;
                case DoStatementAstNode node:
                    VisitDoStatement(node);
                    break;
                case ExpressionStatementAstNode node:
                    VisitExpressionStatement(node);
                    break;
                case ForStatementAstNode node:
                    VisitForStatement(node);
                    break;
                case IfStatementAstNode node:
                    VisitIfStatement(node);
                    break;
                case JumpStatementAstNode node:
                    VisitJumpStatement(node);
                    break;
                case LabeledStatementAstNode node:
                    VisitLabeledStatement(node);
                    break;
                case SwitchStatementAstNode node:
                    VisitSwitchStatement(node);
                    break;
                case WhileStatementAstNode node:
                    VisitWhileStatement(node);
                    break;
                default:
                    throw new NotImplementedException(n.GetType().Name);
            }
        }
        public void VisitStructOrUnionSpecifier(StructOrUnionSpecifierAstNode n)
        {
            switch (n)
            {
                case StructAstNode node:
                    VisitStruct(node);
                    break;
                case UnionAstNode node:
                    VisitUnion(node);
                    break;
                default:
                    throw new NotImplementedException(n.GetType().Name);
            }
        }
        public void VisitUnaryExpression(UnaryExpressionAstNode node)
        {
            switch (node.Terminal)
            {
                case Increment:
                    VisitIncrementExpression(node);
                    break;
                case Decrement:
                    VisitDecrementExpression(node);
                    break;
                case Sizeof:
                    VisitSizeofExpression(node);
                    break;
                case Ampersand:
                    VisitAddressOfExpression(node);
                    break;
                case Star:
                    VisitPointerDereferenceExpression(node);
                    break;
                case Plus:
                    VisitPlusExpression(node);
                    break;
                case Minus:
                    VisitMinusExpression(node);
                    break;
                case Tilde:
                    VisitComplementExpression(node);
                    break;
                case Bang:
                    VisitNegationExpression(node);
                    break;
                default:
                    throw new NotImplementedException(System.Enum.GetName(typeof(Terminal), node.Terminal));
            }
        }
        #endregion

        #region Abstract visitors
        public abstract void VisitAdditionExpression(AdditiveExpressionAstNode node);
        public abstract void VisitSubtractionExpression(AdditiveExpressionAstNode node);
        public abstract void VisitAndExpression(AndExpressionAstNode node);
        public abstract void VisitAssignment(AssignmentExpressionAstNode node);
        public abstract void VisitMultiplyAssignment(AssignmentExpressionAstNode node);
        public abstract void VisitDivideAssignment(AssignmentExpressionAstNode node);
        public abstract void VisitModAssignment(AssignmentExpressionAstNode node);
        public abstract void VisitAddAssignment(AssignmentExpressionAstNode node);
        public abstract void VisitSubtractAssignment(AssignmentExpressionAstNode node);
        public abstract void VisitShiftLeftAssignment(AssignmentExpressionAstNode node);
        public abstract void VisitShiftRightAssignment(AssignmentExpressionAstNode node);
        public abstract void VisitAndAssignment(AssignmentExpressionAstNode node);
        public abstract void VisitXorAssignment(AssignmentExpressionAstNode node);
        public abstract void VisitOrAssignment(AssignmentExpressionAstNode node);
        public abstract void VisitBitFieldDeclarator(BitFieldDeclaratorAstNode node);
        public abstract void VisitBreakStatement(BreakStatementAstNode node);
        public abstract void VisitCaseLabel(CaseLabelAstNode node);
        public abstract void VisitCastExpression(CastExpressionAstNode node);
        public abstract void VisitCharacterConstant(CharacterConstantAstNode node);
        public abstract void VisitCompoundStatement(CompoundStatementAstNode node);
        public abstract void VisitConditionalExpression(ConditionalExpressionAstNode node);
        public abstract void VisitContinueStatement(ContinueStatementAstNode node);
        public abstract void VisitDeclarationStatement(DeclarationStatementAstNode node);
        public abstract void VisitDefaultLabel(DefaultLabelAstNode node);
        public abstract void VisitDoStatement(DoStatementAstNode node);
        public abstract void VisitEnumerationConstant(EnumerationConstantAstNode node);
        public abstract void VisitEnumerator(EnumeratorAstNode node);
        public abstract void VisitEnumSpecifier(EnumSpecifierAstNode node);
        public abstract void VisitEquality(EqualityExpressionAstNode node);
        public abstract void VisitInequality(EqualityExpressionAstNode node);
        public abstract void VisitExpression(InitializerExpressionAstNode node);
        public abstract void VisitExpressionList(ExpressionListAstNode node);
        public abstract void VisitExpressionStatement(ExpressionStatementAstNode node);
        public abstract void VisitFloatingConstant(FloatingConstantAstNode node);
        public abstract void VisitForStatement(ForStatementAstNode node);
        public abstract void VisitFunctionDefinition(FunctionDefinitionAstNode node);
        public abstract void VisitGotoStatement(GotoStatementAstNode node);
        public abstract void VisitIdentifier(IdentifierAstNode node);
        public abstract void VisitIdentifierDeclarator(IdentifierDeclaratorAstNode node);
        public abstract void VisitIdentifierLabel(IdentifierLabelAstNode node);
        public abstract void VisitIfStatement(IfStatementAstNode node);
        public abstract void VisitIndexedDeclarator(IndexedDeclaratorAstNode node);
        public abstract void VisitInitDeclarator(InitDeclaratorAstNode node);
        public abstract void VisitInitializer(InitializerAstNode node);
        public abstract void VisitInitializerList(InitializerListAstNode node);
        public abstract void VisitIntegerConstant(IntegerConstantAstNode node);
        public abstract void VisitLabeledStatement(LabeledStatementAstNode node);
        public abstract void VisitLogicalAndExpression(LogicalAndExpressionAstNode node);
        public abstract void VisitLogicalOrExpression(LogicalOrExpressionAstNode node);
        public abstract void VisitMultiply(MultiplicativeExpressionAstNode node);
        public abstract void VisitDivide(MultiplicativeExpressionAstNode node);
        public abstract void VisitMod(MultiplicativeExpressionAstNode node);
        public abstract void VisitNestedDeclarator(NestedDeclaratorAstNode node);
        public abstract void VisitOldStyleParameterizedDeclarator(OldStyleParameterizedDeclaratorAstNode node);
        public abstract void VisitOrExpression(OrExpressionAstNode node);
        public abstract void VisitParameterDeclaration(ParameterDeclarationAstNode node);
        public abstract void VisitParameterizedDeclarator(ParameterizedDeclaratorAstNode node);
        public abstract void VisitParameterTypeList(ParameterTypeListAstNode node);
        public abstract void VisitPointer(PointerAstNode node);
        public abstract void VisitPointerDeclarator(PointerDeclaratorAstNode node);
        public abstract void VisitPostfixCallExpression(PostfixCallExpressionAstNode node);
        public abstract void VisitPostfixIncrement(PostfixExpressionAstNode node);
        public abstract void VisitPostfixDecrement(PostfixExpressionAstNode node);
        public abstract void VisitPostfixIndexerExpression(PostfixIndexerExpressionAstNode node);
        public abstract void VisitPostfixMemberAccessExpression(PostfixMemberAccessExpressionAstNode node);
        public abstract void VisitPostfixPointerAccessExpression(PostfixPointerAccessExpressionAstNode node);
        public abstract void VisitPostfixUnaryExpression(PostfixUnaryExpressionAstNode node);
        public abstract void VisitLessThan(RelationalExpressionAstNode node);
        public abstract void VisitLessThanOrEqual(RelationalExpressionAstNode node);
        public abstract void VisitGreaterThan(RelationalExpressionAstNode node);
        public abstract void VisitGreaterThanOrEqual(RelationalExpressionAstNode node);
        public abstract void VisitReturnStatement(ReturnStatementAstNode node);
        public abstract void VisitShiftLeft(ShiftExpressionAstNode node);
        public abstract void VisitShiftRight(ShiftExpressionAstNode node);
        public abstract void VisitSizeofTypeExpression(SizeofTypeExpressionAstNode node);
        public abstract void VisitStorageClassSpecifier(StorageClassSpecifierAstNode node);
        public abstract void VisitStringLiteral(StringLiteralAstNode node);
        public abstract void VisitStruct(StructAstNode node);
        public abstract void VisitStructDeclaration(StructDeclarationAstNode node);
        public abstract void VisitSwitchStatement(SwitchStatementAstNode node);
        public abstract void VisitTypedefName(TypedefNameAstNode node);
        public abstract void VisitTypeName(TypeNameAstNode node);
        public abstract void VisitTypeQualifier(TypeQualifierAstNode node);
        public abstract void VisitTypeSpecifier(TypeSpecifierAstNode node);
        public abstract void VisitNegationExpression(UnaryExpressionAstNode node);
        public abstract void VisitComplementExpression(UnaryExpressionAstNode node);
        public abstract void VisitMinusExpression(UnaryExpressionAstNode node);
        public abstract void VisitPlusExpression(UnaryExpressionAstNode node);
        public abstract void VisitPointerDereferenceExpression(UnaryExpressionAstNode node);
        public abstract void VisitAddressOfExpression(UnaryExpressionAstNode node);
        public abstract void VisitSizeofExpression(UnaryExpressionAstNode node);
        public abstract void VisitDecrementExpression(UnaryExpressionAstNode node);
        public abstract void VisitIncrementExpression(UnaryExpressionAstNode node);
        public abstract void VisitUnion(UnionAstNode node);
        public abstract void VisitVariableDeclaration(VariableDeclarationAstNode node);
        public abstract void VisitWhileStatement(WhileStatementAstNode node);
        public abstract void VisitXorExpression(XorExpressionAstNode node);
        #endregion
    }
}