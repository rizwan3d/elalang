using System;
using Ela.CodeModel;

namespace Ela.Compilation
{
    internal sealed class ClassData
    {
        internal ClassData(ElaClassMember[] members)
        {
            Members = members;
        }

        internal ElaClassMember[] Members { get; private set; }

        internal int Code { get; set; }
    }
}
