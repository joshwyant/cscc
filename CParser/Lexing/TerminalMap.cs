using System.Collections.Generic;
using static CParser.Lexing.Terminal;

namespace CParser.Lexing
{
    static class TerminalMap
    {
        public static Dictionary<Terminal, string> Names { get; }
            = new Dictionary<Terminal, string>
        {
            { Newline, "newline" },
            { Whitespace, " " },
            { Eof, "end-of-file" },
            { Identifier, "identifier" },
            { TypedefName, "type name" },
            { EnumConstant, "enumeration constant" },
            { Pound, "#" },
            { DoublePound, "##" },
            { Filename, "filename" },
            { Tilde, "~" },
            { IntegerConstant, "integer constant" },
            { FloatingConstant, "floating point constant" },
            { Bang, "!" },
            { NotEqual, "!=" },
            { Percent, "%" },
            { ModAssign, "%=" },
            { Caret, "^" },
            { XorAssign, "^=" },
            { Ampersand, "&" },
            { LogicalAnd, "&&" },
            { AndAssign, "&=" },
            { Star, "*" },
            { MultiplyAssign, "*=" },
            { LeftParen, "(" },
            { RightParen, ")" },
            { Minus, "-" },
            { Arrow, "->" },
            { Decrement, "--" },
            { SubtractAssign, "-=" },
            { Plus, "+" },
            { Increment, "++" },
            { AddAssign, "+=" },
            { Assign, "=" },
            { DoubleEquals, "==" },
            { LeftBrace, "{" },
            { RightBrace, "}" },
            { LeftBracket, "[" },
            { RightBracket, "]" },
            { Pipe, "|" },
            { LogicalOr, "||" },
            { OrAssign, "|=" },
            { Colon, ":" },
            { Semicolon, ";" },
            { StringLiteral, "string literal" },
            { CharLiteral, "character constant" },
            { LessThan, "<" },
            { LessThanOrEqual, "<=" },
            { ShiftLeft, "<<" },
            { ShiftLeftAssign, "<<=" },
            { Comma, "," },
            { Dot, "." },
            { Ellipsis, "..." },
            { GreaterThan, ">" },
            { GreaterThanOrEqual, ">=" },
            { ShiftRight, ">>" },
            { ShiftRightAssign, ">>=" },
            { Query, "?" },
            { Slash, "/" },
            { DivideAssign, "/=" },
            { Auto, "auto" },
            { Asm, "asm" },
            { Break, "break" },
            { Case, "case" },
            { Char, "char" },
            { Const, "const" },
            { Continue, "continue" },
            { Default, "default" },
            { Do, "do" },
            { Double, "double" },
            { Else, "else" },
            { Enum, "enum" },
            { Extern, "extern" },
            { Float, "float" },
            { For, "for" },
            { Goto, "goto" },
            { If, "if" },
            { Int, "int" },
            { Long, "long" },
            { Register, "register" },
            { Return, "return" },
            { Short, "short" },
            { Signed, "signed" },
            { Sizeof, "sizeof" },
            { Static, "static" },
            { Struct, "struct" },
            { Switch, "switch" },
            { Typedef, "typedef" },
            { Union, "union" },
            { Unsigned, "unsigned" },
            { Void, "void" },
            { Volatile, "volatile" },
            { While, "while" },
        };

        public static string Name(this Terminal t)
        {
            if (Names.ContainsKey(t))
            {
                return Names[t];
            }
            return System.Enum.GetName(typeof(Terminal), t)!;
        }
    }
}