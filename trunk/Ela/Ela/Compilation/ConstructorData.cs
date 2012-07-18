using System;
using System.Collections.Generic;

namespace Ela.Compilation
{
    internal sealed class ConstructorData
    {
        public int Code { get; internal set; }

        public string Name { get; internal set; }

        internal List<String> Parameters { get; set; }
    }
}
