using System;
using System.Collections.Generic;
using System.Linq;
using Ela.Compilation;
using Elide.CodeEditor.Infrastructure;

namespace Elide.ElaCode.ObjectModel
{
    public sealed class TypeClass : IClass
    {
        internal TypeClass(string className, ClassData classData)
        {
            Name = className;
            Members = classData.GetMembers().Select(m => new TypeClassMember(m.Name, m.Arguments, m.Mask)).ToList();
        }

        public string Name { get; private set; }

        public IEnumerable<IClassMember> Members { get; private set; }
    }
}
