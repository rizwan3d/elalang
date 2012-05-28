using System;
using System.Collections.Generic;

namespace Elide.CodeEditor.Infrastructure
{
    public interface ICompiledUnit
    {
        string Name { get; }

        IEnumerable<CodeName> Globals { get; }

        IEnumerable<IReference> References { get; }
    }
}
