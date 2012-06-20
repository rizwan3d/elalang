using System;
using System.Collections.Generic;

namespace Elide.CodeEditor.Infrastructure
{
    public interface IClass
    {
        string Name { get; }

        IEnumerable<IClassMember> Members { get; }
    }
}
