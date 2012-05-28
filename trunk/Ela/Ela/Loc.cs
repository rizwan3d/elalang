using System;

namespace Ela
{
    internal struct Loc
    {
        internal readonly int Line;
        internal readonly int Column;

        internal Loc(int line, int col)
        {
            Line = line;
            Column = col;
        }
    }
}
