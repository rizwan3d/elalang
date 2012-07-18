using System;

namespace Elide.CodeEditor.Infrastructure
{
    public sealed class CodeName : ILocationBounded
    {
        public CodeName(string name, int line, int column)
        {
            Name = name;
            Location = new Location(line, column);
        }

        public string Name { get; private set; }

        public Location Location { get; private set; }
    }
}
