using System;

namespace Elide.CodeEditor.Infrastructure
{
    public interface IClassMember
    {
        string Name { get; }

        int Arguments { get; }

        int Signature { get; }
    }
}
