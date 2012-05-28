using System;
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

        private sealed class ArgsFun : ElaFunction
        {
            private readonly ElaValue[] arguments;
            private readonly int num;
            private readonly string format;
            private readonly int act;

            internal ArgsFun(string format, ElaValue[] args, int num, int act) : base(1)
            {
                this.format = format;
                this.arguments = args;
                this.num = num;
                this.act = act;
            }

            public override ElaValue Call(params ElaValue[] args)
            {
                var a = args[0];
                arguments[num] = a;

                if (num == arguments.Length - 1)
                {
                    var objs = new Object[arguments.Length];

                    for (var i = 0; i < arguments.Length; i++)
                        objs[i] = arguments[i];

                    var str = String.Format(format, objs);

                    if (act == 0)
                        return new ElaValue(str);
                    else if (act == 1)
                        Console.Write(str);
                    else if (act == 2)
                        Console.WriteLine(str);

                    return new ElaValue(ElaUnit.Instance);
                }
                else
                    return new ElaValue(new ArgsFun(format, arguments, num + 1, act));                
            }
        }
        
        public override void Initialize()
        {
            Add<String,ElaValue>("format", Format);
            Add<String,ElaValue>("printf", Printf);
            Add<String,ElaValue>("printfn", Printfn);
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
        		
        private ElaValue GetFmt(string format, int act)
        {
            var c = CountArguments(format);

            if (c == 0)
                return new ElaValue(format);
            else
                return new ElaValue(new ArgsFun(format, new ElaValue[c], 0, act));
        }

        public ElaValue Format(string format)
        {
            return GetFmt(format, 0);
        }

        public ElaValue Printf(string format)
        {
            return GetFmt(format, 1);
        }

        public ElaValue Printfn(string format)
        {
            return GetFmt(format, 2);
        }
    }
}
