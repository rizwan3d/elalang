using System;
using Elide.CodeEditor.Infrastructure;

namespace Elide.ElaCode.ObjectModel
{
    public sealed class TypeClassInstance : IClassInstance
    {
        internal TypeClassInstance(string @class, string type)
        {
            Class = @class;
            Type = type;
        }

        public string Class { get; private set; }

        public string Type { get; private set; }
    }
}
