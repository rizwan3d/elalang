﻿using System;
using System.Collections.Generic;

namespace Elide.ElaObject.ObjectModel
{
    public sealed class ElaObjectFile
    {
        internal ElaObjectFile(Header header, IEnumerable<Reference> refs, IEnumerable<Global> globals, IEnumerable<LateBound> lateBounds, IEnumerable<Layout> layouts, IEnumerable<String> strings, IEnumerable<OpCode> opCodes)
        {
            Header = header;
            References = refs;
            Globals = globals;
            LateBounds = lateBounds;
            Layouts = layouts;
            Strings = strings;
            OpCodes = opCodes;
        }

        public Header Header { get; private set; }

        public IEnumerable<Reference> References { get; private set; }

        public IEnumerable<Global> Globals { get; private set; }

        public IEnumerable<LateBound> LateBounds { get; private set; }

        public IEnumerable<Layout> Layouts { get; private set; }

        public IEnumerable<String> Strings { get; private set; }

        public IEnumerable<OpCode> OpCodes { get; private set; }
    }
}
