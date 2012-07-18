using System;
using System.Collections.Generic;

namespace Ela.Compilation
{
    internal sealed class ConstructorData
    {
        public string TypeName { get; internal set; }

        public int TypeModuleId { get; internal set; }

        public int Code { get; internal set; }

        public string Name { get; internal set; }

        internal List<String> Parameters { get; set; }
    }
}
