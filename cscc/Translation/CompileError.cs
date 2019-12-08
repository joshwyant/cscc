namespace cscc.Translation
{
    class CompileError
    {
        public int Line { get; }
        public int Column { get; }
        public string Message { get; }

        public CompileError(int line, int column, string message)
        {
            Line = line;
            Column = column;
            Message = message;
        }
    }
}