using System;
using System.Collections.Generic;
using Ela.Linking;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;

namespace Ela.Library.General
{
    public sealed class FormatModule : ForeignModule
    {
        public FormatModule()
        {

        }

        private sealed class FormatValue : IFormattable
        {
            private readonly ElaFunction showf;
            private readonly ElaValue val;

            public FormatValue(ElaFunction showf, ElaValue val)
            {
                this.showf = showf;
                this.val = val;
            }

            public string ToString(string format, IFormatProvider formatProvider)
            {
                return showf.Call(new ElaValue(format ?? String.Empty), val).AsString();
            }
        }

        public override void Initialize()
        {
            Add<String,Int32>("countArgs", CountArguments);
            Add<String,ElaFunction,IEnumerable<ElaValue>,String>("format", Format);
        }

        private string Format(string format, ElaFunction showf, IEnumerable<ElaValue> values)
        {
            var objs = new List<Object>(10);

            foreach (var v in values)
                objs.Insert(0, new FormatValue(showf, v));

            return String.Format(format, objs.ToArray());
        }

        private int CountArguments(string format)
        {
            var ptr = 0;
            var start = ptr;
            var args = 0;

            while (ptr < format.Length)
            {
                var c = format[ptr++];

                if (c == '{')
                {
                    if (format[ptr] == '{')
                    {
                        start = ptr++;
                        continue;
                    }

                    start = ptr;
                    args++;
                }
            }

            return args;
        }		
    }
}
