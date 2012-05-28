﻿using System;

namespace Elide.ElaObject.ObjectModel
{
    public sealed class LateBound
    {
        internal LateBound(string name, int address, int data, int line, int column)
        {
            Name = name;
            Address = address;
            Data = data;
            Line = line;
            Column = column;
        }

        public string Name { get; private set; }

        public int Address { get; private set; }

        public int Data { get; private set; }

        public int Line { get; private set; }

        public int Column { get; private set; }
    }
}
