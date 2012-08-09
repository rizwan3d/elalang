using System;
using System.Collections.Generic;
using Ela.Linking;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;
using System.Globalization;

namespace Ela.Library.General
{
    public sealed class FormatModule : ForeignModule
    {
        internal static readonly IFormatProvider NumberFormat = CultureInfo.GetCultureInfo("en-US").NumberFormat;

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
                if (val.TypeCode == ElaTypeCode.Long ||
                    val.TypeCode == ElaTypeCode.String ||
                    val.TypeCode == ElaTypeCode.Char ||
                    val.TypeCode == ElaTypeCode.Double ||
                    val.TypeCode == ElaTypeCode.Single ||
                    val.TypeCode == ElaTypeCode.Integer ||
                    val.TypeCode == ElaTypeCode.Boolean ||
                    val.TypeCode == ElaTypeCode.Unit ||
                    val.TypeCode == ElaTypeCode.Function ||
                    val.TypeCode == ElaTypeCode.Module)
                    return val.ToString(format ?? String.Empty, NumberFormat);

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
            var dict = new Dictionary<Int32,Int32>();

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

                    var idx = format.IndexOf('}', ptr - 1);

                    if (idx != -1)
                    {
                        var sub = format.Substring(ptr, idx - ptr);
                        var idx2 = sub.IndexOf(':');

                        if (idx2 != -1)
                            sub = sub.Substring(0, idx2);

                        var i = Int32.Parse(sub);

                        if (!dict.ContainsKey(i))
                        {
                            start = ptr;
                            args++;
                            dict.Add(i, i);
                        }
                    }
                }
            }

            return args;
        }		
    }
}
