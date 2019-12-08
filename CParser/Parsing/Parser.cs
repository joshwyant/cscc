using cscc.Translation;
using cscc.Helpers;
using cscc.Lexing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using cscc.Parsing.Ast;
using static cscc.Lexing.Terminal;
using static cscc.Translation.SymbolType;
using System.Threading.Tasks;

namespace cscc.Parsing
{
    class Parser : IAsyncEnumerable<AstNode>
    {
        public TranslationUnit TranslationUnit { get; }
        protected IStream<Token> InputStream { get; }

        public Parser(TranslationUnit tu, IStream<Token> input)
        {
            TranslationUnit = tu;
            InputStream = input;
        }

        protected async Task Error(string message)
        {
            var (line, column) = await Metrics();
            TranslationUnit.Errors.Add(new CompileError(line, column, message));
        }

        protected async Task Expect(Terminal t)
        {
            if ((await InputStream.Peek()).Kind == t)
            {
                await InputStream.Read();
            }
            else
            {
                await Error($"{t.Name()} expected");
            }
        }

        protected async Task<Token?> ExpectAndGet(Terminal t)
        {
            if ((await InputStream.Peek()).Kind == t)
            {
                return await InputStream.Read();
            }
            else
            {
                await Error($"{t.Name()} expected");
                return default;
            }
        }

        protected async Task<bool> Check(Terminal t)
        {
            if ((await InputStream.Peek()).Kind == t)
            {
                await InputStream.Read();
                return true;
            }
            else
            {
                return false;
            }
        }

        protected async Task<bool> PeekCheck(Terminal t)
        {
            if ((await InputStream.Peek()).Kind == t)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        protected async Task<Token?> CheckAndGet(Terminal t)
        {
            if ((await InputStream.Peek()).Kind == t)
            {
                return await InputStream.Read();
            }
            else
            {
                return default;
            }
        }

        protected async Task<T> requiredExpression<T>(Func<Task<T?>> func)
            where T : AstNode
        {
            return await required(func, "expression");
        }

        protected async Task<T> requiredStatement<T>(Func<Task<T?>> func)
            where T : AstNode
        {
            return await required(func, "statement");
        }

        protected async Task<T> required<T>(Func<Task<T?>> func, string what)
            where T : AstNode
        {
            var y = await func();
            if (y == default)
            {
                await Error($"{what} required");
                return default!;
            }
            return y!;
        }

        protected async Task<T> required<T>(T? y, string what)
            where T : AstNode
        {
            if (y == default)
            {
                await Error($"{what} required");
                return default!;
            }
            return y!;
        }

        protected async Task<T> requiredExpression<T>(T? y)
            where T : AstNode
        {
            return await required(y, "expression");
        }

        protected async Task<T> requiredStatement<T>(T? y)
            where T : AstNode
        {
            return await required(y, "statement");
        }

        protected async Task<(int line, int column)> Metrics()
        {
            var t = await InputStream.Peek();
            return (t.Line, t.Column);
        }

        protected virtual async IAsyncEnumerable<AstNode> Parse()
        {
            while (!await InputStream.Eof())
            {
                var d = await externalDeclaration();
                if (d != default)
                {
                    yield return d;
                }
                else
                {
                    await Error($"Expected statement or declaration, got {(await InputStream.Read()).Kind.Name()}");
                }
            }
        }

        protected async Task AddSymbol(SymbolTable table, SymbolType type, string name, AstNode value)
        {
            if (table.ContainsKey(name))
            {
                await Error($"Redefinition of symbol {name}");
                // TODO: distinguish between declarations and definitions
            }
            table.Add(name, new BasicSymbol(type, name, value));
        }

        protected async Task<DeclarationAstNode?> externalDeclaration()
        {
            return await functionDefinition() as DeclarationAstNode
                ?? await declaration() as DeclarationAstNode;
        }

        protected async Task<FunctionDefinitionAstNode?> functionDefinition()
        {
            var (line, column) = await Metrics();
            var specifiers = await declarationSpecifiers();
            var d = await declarator();
            if (d == default)
            {
                return default;
            }
            var dl = await declarationList();
            TranslationUnit.Labels = new SymbolTable(TranslationUnit.Labels);
            var cs = await compoundStatement();
            TranslationUnit.Labels = TranslationUnit.Labels.Parent!;
            return new FunctionDefinitionAstNode(specifiers, d, dl, await required(cs, "method body"), line, column);
        }

        protected async Task<VariableDeclarationAstNode?> declaration()
        {
            var (line, column) = await Metrics();
            var specifiers = await declarationSpecifiers();
            var list = await initDeclaratorList();
            if (specifiers == default && list == default)
            {
                return default;
            }
            var d = new VariableDeclarationAstNode(specifiers!, list, line, column);
            if (specifiers
                .OfType<StorageClassSpecifierAstNode>()
                .Cast<StorageClassSpecifierAstNode>()
                .Where(s => s.Terminal == Typedef)
                .Any())
            {
                foreach (var declarator in list!)
                {
                    if (declarator.Name != default)
                    {
                        await AddSymbol(
                            TranslationUnit.Symbols, 
                            TypedefSymbol, 
                            declarator.Name, 
                            d);
                    }
                    // TODO: Error if no name?
                }
            }
            return d;
        }

        protected async Task<List<DeclarationAstNode>> declarationList()
        {
            var list = new List<DeclarationAstNode>();
            DeclarationAstNode? d;
            while ((d = await declaration()) != default)
            {
                list.Add(d);
            }
            return list;
        }

        protected async Task<List<SpecifierAstNode>?> declarationSpecifiers()
        {
            var list = new List<SpecifierAstNode>();
            SpecifierAstNode? specifier;
            while (default != 
                (specifier = 
                    (await storageClassSpecifier()
                    ?? await typeSpecifier()
                    ?? await typeQualifier())))
            {
                list.Add(specifier);
            }
            return list.Any() ? list : default;
        }

        protected async Task<StorageClassSpecifierAstNode?> storageClassSpecifier()
        {
            var (line, column) = await Metrics();
            switch ((await InputStream.Peek()).Kind)
            {
                case Auto:
                case Register:
                case Static:
                case Extern:
                case Typedef:
                    return new StorageClassSpecifierAstNode((await InputStream.Read()).Kind, line, column);
                default:
                    return default;
            }
        }

        protected async Task<SpecifierAstNode?> typeSpecifier()
        {
            var (line, column) = await Metrics();
            switch ((await InputStream.Peek()).Kind)
            {
                case Auto:
                case Terminal.Char:
                case Terminal.Short:
                case Int:
                case Long:
                case Float:
                case Terminal.Double:
                case Signed:
                case Unsigned:
                    return new TypeSpecifierAstNode((await InputStream.Read()).Kind, line, column);
                default:
                    return await structOrUnionSpecifier() as SpecifierAstNode
                        ?? await enumSpecifier() as SpecifierAstNode
                        ?? await typedefName() as SpecifierAstNode;
            }
        }

        protected async Task<TypeQualifierAstNode?> typeQualifier()
        {
            var (line, column) = await Metrics();
            switch ((await InputStream.Peek()).Kind)
            {
                case Const:
                case Terminal.Volatile:
                    return new TypeQualifierAstNode((await InputStream.Read()).Kind, line, column);
                default:
                    return default;
            }
        }

        protected async Task<StructOrUnionSpecifierAstNode?> structOrUnionSpecifier()
        {
            var (line, column) = await Metrics();
            var structOrUnion = None;
            switch ((await InputStream.Peek()).Kind)
            {
                case Struct:
                case Union:
                    structOrUnion = (await InputStream.Read()).Kind;
                    break;
                default:
                    return default;
            }
            var optIdent = await CheckAndGet(Identifier) as ValueToken<string>;
            if (await Check(LeftBrace))
            {
                var list = await structDeclarationList();
                await Expect(RightBrace);
                var n = structOrUnion == Struct
                    ? new StructAstNode(optIdent?.Value, list, line, column) 
                        as StructOrUnionSpecifierAstNode
                    : new UnionAstNode(optIdent?.Value, list, line, column) 
                        as StructOrUnionSpecifierAstNode;
                if (optIdent != default)
                {
                    await AddSymbol(
                        TranslationUnit.Tags, 
                        structOrUnion == Struct ? StructTag : UnionTag, 
                        optIdent.Value, 
                        n);
                }
                return n;
            }
            else if (optIdent != default)
            {
                var n = structOrUnion == Struct
                    ? new StructAstNode(optIdent.Value, default, line, column) 
                        as StructOrUnionSpecifierAstNode
                    : new UnionAstNode(optIdent.Value, default, line, column) 
                        as StructOrUnionSpecifierAstNode;

                await AddSymbol(
                    TranslationUnit.Tags, 
                    structOrUnion == Struct ? StructTag : UnionTag, 
                    optIdent.Value, 
                    n);
                return n;
            }
            else
            {
                await Error("struct or union expected");
                return default;
            }
        }

        protected async Task<List<StructDeclarationAstNode>?> structDeclarationList()
        {
            var list = new List<StructDeclarationAstNode>();
            StructDeclarationAstNode decl;
            while (default != (decl = await structDeclaration()))
            {
                list.Add(decl);
            }
            return list.Any() ? list : default;
        }

        protected async Task<List<DeclaratorAstNode>?> initDeclaratorList()
        {
            var list = new List<DeclaratorAstNode>();
            DeclaratorAstNode? decl;
            while (default != (decl = await initDeclarator()))
            {
                list.Add(decl);
                if (!await Check(Comma))
                {
                    break;
                }
            }
            return list.Any() ? list : default;
        }

        protected async Task<DeclaratorAstNode?> initDeclarator()
        {
            var (line, column) = await Metrics();
            var d = await declarator();
            if (d != default && await Check(Assign))
            {
                return new InitDeclaratorAstNode(d, await required(initializer, "initializer"), line, column);
            }
            return d;
        }

        protected async Task<StructDeclarationAstNode> structDeclaration()
        {
            var (line, column) = await Metrics();
            var sqlist = await specifierQualifierList();
            var sdlist = await structDeclaratorList();
            await Expect(Semicolon);
            return new StructDeclarationAstNode(sqlist, sdlist, line, column);
        }

        protected async Task<List<SpecifierAstNode>> specifierQualifierList()
        {
            var list = new List<SpecifierAstNode>();
            SpecifierAstNode? sq;
            while (default != 
                (sq = (await typeSpecifier() as SpecifierAstNode
                    ?? await typeQualifier() as SpecifierAstNode)))
            {
                list.Add(sq);
            }
            if (!list.Any())
            {
                await Error("type specifier expected");
            }
            return list.Any() ? list : default!;
        }

        protected async Task<List<DeclaratorAstNode>?> structDeclaratorList()
        {
            var list = new List<DeclaratorAstNode>();
            DeclaratorAstNode? decl;
            while (default != (decl = await structDeclarator()))
            {
                list.Add(decl);
                if (!await Check(Comma))
                {
                    break;
                }
            }
            return list.Any() ? list : default;
        }

        protected async Task<DeclaratorAstNode?> structDeclarator()
        {
            var (line, column) = await Metrics();
            var d = await declarator();
            if (await Check(Colon))
            {
                // Named and unnamed bit fields
                return new BitFieldDeclaratorAstNode(d, await constantExpression(), line, column);
            }
            return d;
        }

        protected async Task<EnumSpecifierAstNode?> enumSpecifier()
        {
            var (line, column) = await Metrics();
            if (!await Check(Terminal.Enum))
            {
                return default;
            }
            EnumSpecifierAstNode? e = default;
            var optIdent = await CheckAndGet(Identifier) as ValueToken<string>;
            if (await Check(LeftBrace))
            {
                e = new EnumSpecifierAstNode(optIdent?.Value, await enumeratorList(), line, column);
                await Expect(RightBrace);
            }
            else if (optIdent != default)
            {
                e = new EnumSpecifierAstNode(optIdent.Value, default, line, column);
            }
            if (optIdent != default)
            {
                await AddSymbol(
                    TranslationUnit.Tags, 
                    EnumTag,
                    optIdent.Value, 
                    e!);
            }
            return e;
        }

        protected async Task<List<EnumeratorAstNode>> enumeratorList()
        {
            var list = new List<EnumeratorAstNode>();
            EnumeratorAstNode e;
            while (default != (e = await enumerator()))
            {
                list.Add(e);
                if (!await Check(Comma))
                {
                    break;
                }
            }
            if (!list.Any())
            {
                await Error("enumerator expected");
            }
            return list.Any() ? list : default!;
        }

        protected async Task<EnumeratorAstNode> enumerator()
        {
            var (line, column) = await Metrics();
            EnumeratorAstNode node;
            var ident = await ExpectAndGet(Identifier) as ValueToken<string>;
            if (ident == default)
            {
                return default!;
            }
            if (await Check(Assign))
            {
                node = new EnumeratorAstNode(ident.Value, await constantExpression(), line, column);
            }
            else
            {
                node = new EnumeratorAstNode(ident.Value, default, line, column);
            }
            await AddSymbol(
                TranslationUnit.Symbols, 
                EnumSymbol,
                ident.Value, 
                node);
            return node;
        }

        protected async Task<DeclaratorAstNode?> declarator()
        {
            var (line, column) = await Metrics();
            var p = await pointer();
            var d = await directDeclarator();
            if (p != default)
            {
                return new PointerDeclaratorAstNode(p, await required(d, "pointer type required"), line, column);
            }
            return d;
        }


        protected async Task<DeclaratorAstNode?> directDeclarator()
        {
            var (line, column) = await Metrics();
            DeclaratorAstNode? dd = default;
            var t = await InputStream.Peek();
            SymbolType type = DeclarationSymbol;
            switch (t.Kind)
            {
                case Identifier:
                    var ident = t as ValueToken<string>;
                    dd = new IdentifierDeclaratorAstNode(ident!.Value, line, column);
                    break;
                case LeftParen:
                    await InputStream.Read();
                    dd = new NestedDeclaratorAstNode(await required(declarator, "type name"), line, column);
                    await Expect(RightParen);
                    break;
                default:
                    return default;
            }
            while ((t = await InputStream.Peek()).Kind == LeftBracket || t.Kind == LeftParen)
            {
                await InputStream.Read();
                if (t.Kind == LeftBracket)
                {
                    dd = new IndexedDeclaratorAstNode(dd, await constantExpression(), line, column);
                    await Expect(RightBracket);
                }
                else // t.Kind == LeftParen
                {
                    var ptl = await parameterTypeList();
                    if (ptl != default)
                    {
                        dd = new ParameterizedDeclaratorAstNode(dd, ptl, line, column);
                    }
                    else
                    {
                        // Optional identifier list
                        dd = new OldStyleParameterizedDeclaratorAstNode(dd, await identifierList(), line, column);
                    }
                    await Expect(RightParen);
                    type = FunctionSymbol;
                }
            }
            if (dd?.Name != default)
            {
                await AddSymbol(
                    TranslationUnit.Symbols, 
                    type,
                    dd.Name, 
                    dd);
            }
            return dd;
        }

        protected async Task<PointerAstNode?> pointer()
        {
            PointerAstNode? p = default;
            var (line, column) = await Metrics();
            while (await PeekCheck(Star))
            {
                p = new PointerAstNode(await typeQualifierList(), p, line, column);
            }
            return p;
        }

        protected async Task<List<TypeQualifierAstNode>?> typeQualifierList()
        {
            var list = new List<TypeQualifierAstNode>();
            TypeQualifierAstNode? q;
            while (default != (q = (await typeQualifier())))
            {
                list.Add(q);
            }
            return list.Any() ? list : default;
        }

        protected async Task<ParameterTypeListAstNode> parameterTypeList()
        {
            var (line, column) = await Metrics();
            var pl = await parameterList();
            var varargs = false;
            if (await Check(Comma))
            {
                await Expect(Ellipsis);
                varargs = true;
            }
            return new ParameterTypeListAstNode(pl, varargs, line, column);
        }

        protected async Task<List<ParameterDeclarationAstNode>?> parameterList()
        {
            var list = new List<ParameterDeclarationAstNode>();
            ParameterDeclarationAstNode? p;
            while (default != (p = await parameterDeclaration()))
            {
                list.Add(p);
                Token? comma;
                if ((comma = await CheckAndGet(Comma)) == default)
                {
                    break;
                }
                if (await PeekCheck(Ellipsis))
                {
                    InputStream.PutBack(comma);
                }
            }
            return list.Any() ? list : default;
        }

        protected async Task<ParameterDeclarationAstNode> parameterDeclaration()
        {
            var (line, column) = await Metrics();
            var specs = await declarationSpecifiers();
            var d = await declarator();
            if (d != default)
            {
                return new ParameterDeclarationAstNode(specs, await required(d, "parameter"), line, column);
            }
            else
            {
                var absDeclOpt = await abstractDeclarator();
                return new ParameterDeclarationAstNode(specs, absDeclOpt, line, column);
            }
        }

        protected async Task<List<string>?> identifierList()
        {
            var list = new List<string>();
            ValueToken<string>? ident;
            while (default != (ident = await InputStream.Peek() as ValueToken<string>) && ident.Kind == Identifier)
            {
                await InputStream.Read();
                list.Add(ident.Value);
                if (!await Check(Comma))
                {
                    break;
                }
            }
            return list.Any() ? list : default;
        }

        protected async Task<InitializerAstNode?> initializer()
        {
            var (line, column) = await Metrics();
            var a = await assignmentExpression();
            if (a != default)
            {
                return new InitializerExpressionAstNode(a, line, column);
            }
            if (!await Check(LeftBrace))
            {
                return default;
            }
            var list = await initializerList();
            await Check(Comma); // Optional
            await Expect(RightBrace);
            return new InitializerListAstNode(list, line, column);
        }

        protected async Task<List<InitializerAstNode>?> initializerList()
        {
            var list = new List<InitializerAstNode>();
            InitializerAstNode? i;
            while (default != (i = await initializer()))
            {
                list.Add(i);
                if (!await Check(Comma))
                {
                    break;
                }
            }
            return list.Any() ? list : default;
        }

        protected async Task<TypeNameAstNode> typeName()
        {
            var (line, column) = await Metrics();
            var sql = await specifierQualifierList();
            var adOpt = await abstractDeclarator();
            return new TypeNameAstNode(sql, adOpt, line, column);
        }

        protected async Task<DeclaratorAstNode> abstractDeclarator()
        {
            var (line, column) = await Metrics();
            var pOpt = await pointer();
            var dad = await directAbstractDeclarator();
            if (pOpt != default)
            {
                return new PointerDeclaratorAstNode(pOpt, dad, line, column);
            }
            else if (dad != default)
            {
                return dad;
            }
            else
            {
                await Error($"typename expected");
                return default!;
            }
        }

        protected async Task<DeclaratorAstNode?> directAbstractDeclarator()
        {
            var (line, column) = await Metrics();
            DeclaratorAstNode? dadOpt = default;
            Token t;
            while ((t = await InputStream.Peek()).Kind == LeftBracket || t.Kind == LeftParen)
            {
                await InputStream.Read();
                if (t.Kind == LeftBracket)
                {
                    dadOpt = new IndexedDeclaratorAstNode(dadOpt, await constantExpression(), line, column);
                    await Check(RightBracket);
                }
                else
                {
                    var ad = await abstractDeclarator();
                    if (ad != default)
                    {
                        dadOpt = ad;
                    }
                    else
                    {
                        // Optional parameter type list list
                        dadOpt = new ParameterizedDeclaratorAstNode(dadOpt, await parameterTypeList(), line, column);
                    }
                    await Check(RightParen);
                }
            }
            return dadOpt;
        }

        protected async Task<TypedefNameAstNode?> typedefName()
        {
            var (line, column) = await Metrics();
            ValueToken<Symbol>? t;
            if ((t = await CheckAndGet(TypedefName) as ValueToken<Symbol>)?.Kind == TypedefName)
            {
                return new TypedefNameAstNode(t.Value, line, column);
            }
            return default;
        }

        protected async Task<StatementAstNode?> statement()
        {
            return await labeledStatement()
                ?? await expressionStatement()
                ?? await compoundStatement()
                ?? await selectionStatement()
                ?? await iterationStatement()
                ?? await jumpStatement()
                ?? await declarationStatement(); // I added this
        }

        protected async Task<StatementAstNode?> labeledStatement()
        {
            var (line, column) = await Metrics();
            LabelAstNode? label = default;
            switch (await InputStream.Peek())
            {
                case ValueToken<string> ident when ident.Kind == Identifier:
                    label = new IdentifierLabelAstNode(ident.Value, line, column);
                    await AddSymbol(
                        TranslationUnit.Labels, 
                        LabelSymbol,
                        ident.Value, 
                        label);
                    break;
                case Token t when t.Kind == Case:
                    label = new CaseLabelAstNode(await constantExpression(), line, column);
                    break;
                case Token t when t.Kind == Default:
                    label = new DefaultLabelAstNode(line, column);
                    break;
                default:
                    return default;
            }
            await Expect(Colon);
            var s = await statement();
            return new LabeledStatementAstNode(label, await requiredStatement(s), line, column);
        }

        protected async Task<StatementAstNode> declarationStatement()
        {
            var d = await declaration();
            if (d == default)
            {
                await Error($"declaration expected");
                return default!;
            }
            else 
            {
                return new DeclarationStatementAstNode(d);
            }
        }

        protected async Task<StatementAstNode> expressionStatement()
        {
            var (line, column) = await Metrics();
            return new ExpressionStatementAstNode(await expression(), line, column);
        }

        protected async Task<CompoundStatementAstNode?> compoundStatement()
        {
            var (line, column) = await Metrics();
            if (await Check(LeftBrace))
            {
                // declListOpt is redundant, because I added declarations to statementList.
                var declListOpt = await declarationList();
                var list = await statementList();
                await Expect(RightBrace);
                return new CompoundStatementAstNode(declListOpt, list, line, column);
            }
            return default;
        }

        protected async Task<List<StatementAstNode>> statementList()
        {
            var list = new List<StatementAstNode>();
            StatementAstNode? s;
            while (default != (s = await statement()))
            {
                list.Add(s);
            }
            return list;
        }

        protected async Task<StatementAstNode?> selectionStatement()
        {
            var (line, column) = await Metrics();
            if (await Check(If))
            {
                await Expect(LeftParen);
                var e = await requiredExpression(expression);
                await Expect(RightParen);
                var s = await requiredStatement(statement);
                if (await Check(Else))
                {
                    return new IfStatementAstNode(e, s, await statement(), line, column);
                }
                return new IfStatementAstNode(e, s, default, line, column);
            }
            else if (await Check(Switch))
            {
                await Expect(LeftParen);
                var e = await requiredExpression(expression);
                await Expect(RightParen);
                var s = await requiredStatement(statement);
                return new SwitchStatementAstNode(e, s, line, column);
            }
            else
            {
                return default;
            }
        }

        protected async Task<StatementAstNode?> iterationStatement()
        {
            var (line, column) = await Metrics();
            if (await Check(While))
            {
                await Expect(LeftParen);
                var e = await requiredExpression(expression);
                await Expect(RightParen);
                var s = await requiredStatement(statement);
                return new WhileStatementAstNode(e, s, line, column);
            }
            else if (await Check(Do))
            {
                var s = await requiredStatement(statement);
                await Expect(While);
                await Expect(LeftParen);
                var e = await requiredExpression(expression);
                await Expect(RightParen);
                await Expect(Semicolon);
                return new DoStatementAstNode(s, e, line, column);
            }
            else if (await Check(For))
            {
                await Expect(LeftParen);
                var e1 = await expression();
                await Expect(Semicolon);
                var e2 = await expression();
                await Expect(Semicolon);
                var e3 = await expression();
                await Expect(RightParen);
                var s = await requiredStatement(statement);
                return new ForStatementAstNode(e1, e2, e3, s, line, column);
            }
            else
            {
                return default;
            }
        }

        protected async Task<JumpStatementAstNode?> jumpStatement()
        {
            var (line, column) = await Metrics();
            if (await Check(Goto))
            {
                var ident = await ExpectAndGet(Identifier) as ValueToken<string>;
                if (ident == default)
                {
                    return default;
                }
                await Expect(Semicolon);
                return new GotoStatementAstNode(ident.Value, line, column);
            }
            else if (await Check(Continue))
            {
                await Expect(Semicolon);
                return new ContinueStatementAstNode(line, column);
            }
            else if (await Check(Break))
            {
                await Expect(Semicolon);
                return new BreakStatementAstNode(line, column);
            }
            else if (await Check(Return))
            {
                var eOpt = await expression();
                await Expect(Semicolon);
                return new ReturnStatementAstNode(eOpt, line, column);
            }
            else
            {
                return default;
            }
        }

        protected async Task<ExpressionAstNode?> expression()
        {
            var (line, column) = await Metrics();
            var list = new List<ExpressionAstNode>();
            ExpressionAstNode? e = default;
            do
            {
                e = await assignmentExpression();
                if (e != default)
                {
                    list.Add(e);
                }
            } while (e != default && await Check(Comma));
            return list.Count == 1 ? list[0] : list.Count > 1 ? new ExpressionListAstNode(list, line, column) : default;
        }

        protected async Task<ExpressionAstNode?> assignmentExpression()
        {
            var (line, column) = await Metrics();
            var e = await conditionalExpression();
            if (e != default)
            {
                return e;
            }
            var ue = await unaryExpression();
            Token t;
            switch ((await InputStream.Peek()).Kind)
            {
                case Assign:
                case MultiplyAssign:
                case DivideAssign:
                case ModAssign:
                case AddAssign:
                case SubtractAssign:
                case ShiftLeftAssign:
                case ShiftRightAssign:
                case AndAssign:
                case XorAssign:
                case OrAssign:
                    t = await InputStream.Read();
                    break;
                default:
                    await Error("assignment operator expected");
                    return default;
            }
            var ae = await assignmentExpression();
            return new AssignmentExpressionAstNode(await requiredExpression(ue), t.Kind, await requiredExpression(ae), line, column);
        }

        protected async Task<ExpressionAstNode?> conditionalExpression()
        {
            var (line, column) = await Metrics();
            var e1 = await logicalOrExpression();
            if (await Check(Query))
            {
                var conditional = await requiredExpression(e1);
                var trueExpression = await requiredExpression(expression);
                await Expect(Colon);
                var falseExpression = await requiredExpression(conditionalExpression);
                return new ConditionalExpressionAstNode(conditional, trueExpression, falseExpression, line, column);
            }
            return e1;
        }

        protected async Task<ExpressionAstNode> constantExpression()
        {
            return await requiredExpression(conditionalExpression);
        }

        protected async Task<ExpressionAstNode?> logicalOrExpression()
        {
            var (line, column) = await Metrics();
            var e = await logicalAndExpression();
            while (e != default && await Check(LogicalOr))
            {
                e = new LogicalOrExpressionAstNode(e, await requiredExpression(logicalAndExpression), line, column);
            }
            return e;
        }

        protected async Task<ExpressionAstNode?> logicalAndExpression()
        {
            var (line, column) = await Metrics();
            var e = await inclusiveOrExpression();
            while (e != default && await Check(LogicalAnd))
            {
                e = new LogicalAndExpressionAstNode(e, await requiredExpression(inclusiveOrExpression), line, column);
            }
            return e;
        }

        protected async Task<ExpressionAstNode?> inclusiveOrExpression()
        {
            var (line, column) = await Metrics();
            var e = await exclusiveOrExpression();
            while (e != default && await Check(Pipe))
            {
                e = new OrExpressionAstNode(e, await requiredExpression(exclusiveOrExpression), line, column);
            }
            return e;
        }

        protected async Task<ExpressionAstNode?> exclusiveOrExpression()
        {
            var (line, column) = await Metrics();
            var e = await andExpression();
            while (e != default && await Check(Caret))
            {
                e = new XorExpressionAstNode(e, await requiredExpression(andExpression), line, column);
            }
            return e;
        }

        protected async Task<ExpressionAstNode?> andExpression()
        {
            var (line, column) = await Metrics();
            var e = await equalityExpression();
            while (e != default && await Check(Ampersand))
            {
                e = new AndExpressionAstNode(e, await requiredExpression(equalityExpression), line, column);
            }
            return e;
        }

        protected async Task<ExpressionAstNode?> equalityExpression()
        {
            var (line, column) = await Metrics();
            var e = await relationalExpression();
            Token? t = default;
            while (e != default && (t = await CheckAndGet(DoubleEquals) 
                ?? await CheckAndGet(NotEqual)) != default)
            {
                e = new EqualityExpressionAstNode(e, t.Kind, await requiredExpression(relationalExpression), line, column);
            }
            return e;
        }

        protected async Task<ExpressionAstNode?> relationalExpression()
        {
            var (line, column) = await Metrics();
            var e = await shiftExpression();
            Token? t = default;
            while (e != default && (t = await CheckAndGet(LessThan) 
                ?? await CheckAndGet(GreaterThan)
                ?? await CheckAndGet(LessThanOrEqual)
                ?? await CheckAndGet(GreaterThanOrEqual)) != default)
            {
                e = new RelationalExpressionAstNode(e, t.Kind, await requiredExpression(shiftExpression), line, column);
            }
            return e;
        }

        protected async Task<ExpressionAstNode?> shiftExpression()
        {
            var (line, column) = await Metrics();
            var e = await additiveExpression();
            Token? t = default;
            while (e != default && (t = await CheckAndGet(ShiftLeft) 
                ?? await CheckAndGet(ShiftRight)) != default)
            {
                e = new ShiftExpressionAstNode(e, t.Kind, await requiredExpression(additiveExpression), line, column);
            }
            return e;
        }

        protected async Task<ExpressionAstNode?> additiveExpression()
        {
            var (line, column) = await Metrics();
            var e = await multiplicativeExpression();
            Token? t = default;
            while (e != default && (t = await CheckAndGet(Plus) 
                ?? await CheckAndGet(Minus)) != default)
            {
                e = new AdditiveExpressionAstNode(e, t.Kind, await requiredExpression(multiplicativeExpression), line, column);
            }
            return e;
        }

        protected async Task<ExpressionAstNode?> multiplicativeExpression()
        {
            var (line, column) = await Metrics();
            var e = await castExpression();
            Token? t = default;
            while (e != default && (t = await CheckAndGet(Star) 
                ?? await CheckAndGet(Slash)
                ?? await CheckAndGet(Percent)) != default)
            {
                e = new MultiplicativeExpressionAstNode(e, t.Kind, await requiredExpression(castExpression), line, column);
            }
            return e;
        }

        protected async Task<ExpressionAstNode?> castExpression()
        {
            var (line, column) = await Metrics();
            var e = await unaryExpression();
            if (e != default)
            {
                return e;
            }
            await Expect(LeftParen);
            var t = await typeName();
            await Expect(RightParen);
            e = await requiredExpression(castExpression);
            return new CastExpressionAstNode(t, e, line, column);
        }

        protected async Task<ExpressionAstNode?> unaryExpression()
        {
            var (line, column) = await Metrics();
            switch ((await InputStream.Peek()).Kind)
            {
                case Sizeof:
                    await InputStream.Read();
                    if (await Check(LeftParen))
                    {
                        var tn = await typeName();
                        await Expect(RightParen);
                        return new SizeofTypeExpressionAstNode(tn, line, column);
                    }
                    return new UnaryExpressionAstNode(Sizeof, await requiredExpression(unaryExpression), line, column);
                case Increment:
                case Decrement:
                    return new UnaryExpressionAstNode((await InputStream.Read()).Kind, await requiredExpression(unaryExpression), line, column);
                case Ampersand:
                case Star:
                case Plus:
                case Minus:
                case Tilde:
                case Bang:
                    return new UnaryExpressionAstNode((await InputStream.Read()).Kind, await requiredExpression(castExpression), line, column);
                default:
                    return await postfixExpression();
            }
        }

        protected async Task<ExpressionAstNode?> postfixExpression()
        {
            var (line, column) = await Metrics();
            var e = await primaryExpression();
            while (true)
            {
                var t = (await InputStream.Peek()).Kind;
                switch (t)
                {
                    case LeftBracket:
                        e = new PostfixIndexerExpressionAstNode(await requiredExpression(e), await requiredExpression(expression), line, column);
                        break;
                    case LeftParen:
                        e = new PostfixCallExpressionAstNode(await requiredExpression(e), await argumentExpressionList(), line, column);
                        break;
                    case Dot:
                        e = new PostfixMemberAccessExpressionAstNode(await requiredExpression(e), (await CheckAndGet(Identifier) as ValueToken<string>)?.Value!, line, column);
                        break;
                    case Arrow:
                        e = new PostfixPointerAccessExpressionAstNode(await requiredExpression(e), (await CheckAndGet(Identifier) as ValueToken<string>)?.Value!, line, column);
                        break;
                    case Increment:
                    case Decrement:
                        return new PostfixExpressionAstNode(await requiredExpression(e), t, line, column);
                    default:
                        return e;
                }
            }
        }

        protected async Task<ExpressionAstNode?> primaryExpression()
        {
            Token? t;
            var (line, column) = await Metrics();
            if ((t = await CheckAndGet(Identifier)) != default)
            {
                return new IdentifierAstNode((t as ValueToken<string>)!.Value, line, column);
            }
            else if ((t = await CheckAndGet(StringLiteral)) != default)
            {
                return new StringLiteralAstNode((t as ValueToken<string>)!.Value, line, column);
            }
            else if ((t = await CheckAndGet(LeftParen)) != default)
            {
                var e = await expression();
                await Expect(RightParen);
                return e;
            }
            else
            {
                return await constant();
            }
        }

        protected async Task<List<ExpressionAstNode>?> argumentExpressionList()
        {
            var list = new List<ExpressionAstNode>();
            ExpressionAstNode? e;
            while (default != (e = await assignmentExpression()))
            {
                list.Add(e);
                if (!await Check(Comma))
                {
                    break;
                }
            }
            return list.Any() ? list : default;
        }

        protected async Task<ConstantExpressionAstNode?> constant()
        {
            Token? t;
            var (line, column) = await Metrics();
            if ((t = await CheckAndGet(IntegerConstant)) != default)
            {
                return new IntegerConstantAstNode(t! as IntegerToken);
            }
            else if ((t = await CheckAndGet(CharLiteral)) != default)
            {
                return new CharacterConstantAstNode((t as ValueToken<string>)!.Value, line, column);
            }
            else if ((t = await CheckAndGet(FloatingConstant)) != default)
            {
                return new FloatingConstantAstNode(t! as FloatingToken);
            }
            else if ((t = await CheckAndGet(EnumConstant)) != default)
            {
                return new EnumerationConstantAstNode((t as ValueToken<EnumSymbol>)!.Value, line, column);
            }
            else
            {
                return default; // Empty expressions may be used in for loops, empty statements, etc.
            }
        }

        #region IAsyncEnumerable<AstNode> members
        public IAsyncEnumerator<AstNode> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return Parse().GetAsyncEnumerator();
        }
        #endregion
    }
}