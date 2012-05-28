using System;

namespace Elide.CodeEditor.Infrastructure
{
    public sealed class CodeName
    {
        public CodeName(string name, int line, int column)
        {
            Name = name;
            Line = line;
            Column = column;
        }

        public string Name { get; private set; }

        public int Line { get; private set; }

        public int Column { get; private set; }
    }
}
