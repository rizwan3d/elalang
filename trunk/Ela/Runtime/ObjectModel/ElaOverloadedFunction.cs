using System;
using System.Collections.Generic;
using Ela.CodeModel;
using System.Text;

namespace Ela.Runtime.ObjectModel
{
    internal sealed class ElaOverloadedFunction : ElaFunction
    {
        #region Construction
        internal Dictionary<String,ElaFunction> overloads;
        private string fname;
        
        internal ElaOverloadedFunction(string fname, int pars, Dictionary<String,ElaFunction> funs, FastList<ElaValue[]> captures, ElaMachine vm) :
            base(0, 0, pars, captures, vm)
        {
            this.fname = fname;
            overloads = funs;
            Overloaded = true;
        }
        #endregion


        #region Methods
        public override ElaValue Call(params ElaValue[] args)
        {
            var fun = (ElaFunction)this;
            var ctx = new ExecutionContext();

            for (var i = 0; i < args.Length; i++)
            {
                var a = args[i];
                var ret = fun.Call(a, ctx);

                if (i == args.Length - 1)
                    return ret;

                if (ret.TypeId == ElaMachine.FUN)
                    fun = (ElaFunction)ret.Ref;
                else
                {
                    var e = new ElaError(ElaRuntimeError.TooManyParams);
                    throw new ElaRuntimeException(e.Tag, e.Message);
                }
               
                if (ctx.Failed)
                    throw new ElaRuntimeException(ctx.Error.Tag, ctx.Error.Message);
            }

            return Default();
        }


        internal override ElaFunction Resolve(ElaValue arg, ExecutionContext ctx)
        {
            var tag = arg.GetTag();

            if (ctx.Failed)
                return null;

            var fun = default(ElaFunction);

            if (!overloads.TryGetValue(tag, out fun))
            {
                if (tag == null || !overloads.TryGetValue("$Any", out fun))
                {
                    ctx.Fail(ElaRuntimeError.OverloadNotFound, BuildSingature(tag), fname.Replace("$", String.Empty));
                    return null;
                }
            }

            fun = fun.CloneFast();
            fun.AppliedParameters = AppliedParameters;

            for (var i = 0; i < AppliedParameters; i++)
                fun.Parameters[i] = Parameters[i];

            return fun;
        }


        private string BuildSingature(string tag)
        {
            var sb = new StringBuilder();

            for (var i = 0; i < AppliedParameters; i++)
            {
                sb.Append(Parameters[i].GetTag());
                sb.Append("->");
            }

            sb.Replace("$Any", "*");
            sb.Append(tag);
            return sb.ToString();
        }


        public override string ToString()
		{
			return (Format.IsSymbolic(fname) ? "(" + fname + ")" : fname) + ":*->*";
		}


        public override ElaFunction Clone()
        {
            var newInst = new ElaOverloadedFunction(fname, Parameters.Length + 1, overloads, Captures, Machine);
            return CloneFast(newInst);
        }
        #endregion
    }
}
