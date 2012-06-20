using System;
using Elide.CodeEditor.Infrastructure;

namespace Elide.ElaCode.ObjectModel
{
    public sealed class UserType : IType
    {
        internal UserType(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }
    }
}
