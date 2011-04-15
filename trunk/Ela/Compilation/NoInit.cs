using System;

namespace Ela.Compilation
{
    internal struct NoInit
    {
        internal readonly int Code;
        internal readonly bool Allow;

        internal NoInit(int code, bool allow)
        {
            Code = code;
            Allow = allow;
        }
    }
}
