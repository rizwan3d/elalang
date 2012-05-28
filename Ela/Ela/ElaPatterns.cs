using System;

namespace Ela
{
    [Flags]
    public enum ElaPatterns
    {
        None = 0x00,

        Tuple = 0x02,

        HeadTail = 0x04,

        Record = 0x08,

        Variant = 0x10
    }
}
