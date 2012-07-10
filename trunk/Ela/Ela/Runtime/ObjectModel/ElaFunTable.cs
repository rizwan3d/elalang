using System;
using System.Collections.Generic;
using System.Text;

namespace Ela.Runtime.ObjectModel
{
    public sealed class ElaFunTable : ElaFunction
    {
        private Dictionary<Int32,ElaFunction> funs;
        private readonly string name;
        private readonly int mask;
        private int curType;

        internal ElaFunTable(string name, int mask, int pars, int curType, ElaMachine vm) : base(pars)
        {
            this.name = name;
            this.mask = mask;
            this.curType = curType;
            base.table = true;
            base.Machine = vm;
            funs = new Dictionary<Int32,ElaFunction>();
        }

        internal ElaFunction GetFunction(ElaValue val, ExecutionContext ctx)
        {
            var m = 1 << AppliedParameters;
            var bit = (mask & m) == m;
            
            if (bit && val.TypeId == ElaMachine.LAZ)
            {
                val = val.Ref.Force(val, ctx);

                if (ctx.Failed)
                    return null;
            }

            if (curType == 0)
            {
                if (bit)
                    curType = val.TypeId;
            }
            else
            {
                if (bit && val.TypeId != curType)
                {
                    if (funs.ContainsKey(curType))
                        ctx.NoOverload(ToStringWithType(curType, null, AppliedParameters), ToStringWithType(curType, val.TypeId, AppliedParameters), name);
                    else
                        ctx.NoOverload(val.GetTypeName(), name);
                    return null;
                }
            }

            if (AppliedParameters == Parameters.Length)
            {
                var ret = default(ElaFunction);

                if (!funs.TryGetValue(curType, out ret))
                {
                    if (funs.ContainsKey(curType))
                        ctx.NoOverload(ToStringWithType(curType, null, AppliedParameters), ToStringWithType(curType, val.TypeId, AppliedParameters), name);
                    else
                        ctx.NoOverload(val.GetTypeName(), name); 
                    return null;
                }

                ret = ret.CloneFast();
                ret.AppliedParameters = AppliedParameters;
                
                if (ret.Parameters.Length != Parameters.Length)
                {
                    for (var i = 0; i < AppliedParameters; i++)
                        ret.Parameters[i] = Parameters[i];
                }
                else
                    ret.Parameters = Parameters;

                return ret;
            }

            return this;
        }

        public override ElaValue Call(params ElaValue[] args)
        {
            if (args == null || args.Length == 0)
                throw new ElaRuntimeException("Unable to call an overloaded function without arguments.");

            var ctx = new ExecutionContext();
            var fn = default(ElaFunction);
            
            for (var i = 0; i < args.Length - 1; i++)
            {
                fn = GetFunction(args[i], ctx);

                if (!(fn is ElaFunTable))
                    throw new ElaRuntimeException("Incorrect argument number.");

                if (ctx.Failed)
                    throw new ElaRuntimeException(ctx.Error.Message);
            }

            return fn.Call(args);
        }

        internal void AddFunction(int typeId, ElaFunction fun)
        {
            funs.Remove(typeId);
            funs.Add(typeId, fun);
        }

        public override ElaFunction Clone()
        {
            var f = new ElaFunTable(name, mask, Parameters.Length + 1, curType, Machine);
            f.funs = funs;
            return base.CloneFast(f);
        }

        public override string ToString(string format, IFormatProvider provider)
        {
            var sb = new StringBuilder();

            for (var i = 0; i < Parameters.Length + 1; i++)
            {
                if (i > 0)
                    sb.Append("->");

                var m = (1 << i);

                if ((mask & m) == m)
                    sb.Append('a');
                else
                    sb.Append('*');
            }

            sb.Append("->*");
            return GetFunctionName() + ":" + sb.ToString();
        }

        private string ToStringWithType(int type, int? cur, int arg)
        {
            var sb = new StringBuilder();
            var tn = base.Machine.Assembly.Types[type].TypeName;
            var curn = cur != null ? base.Machine.Assembly.Types[cur.Value].TypeName : null;

            for (var i = 0; i < Parameters.Length + 1; i++)
            {
                if (i > 0)
                    sb.Append("->");

                var m = (1 << i);

                if (i == arg && curn != null)
                    sb.Append(curn);
                else if ((mask & m) == m)
                    sb.Append(tn);
                else
                    sb.Append('*');
            }

            sb.Append("->*");
            return sb.ToString();
        }


        public override string GetFunctionName()
        {
            return name;
        }
    }
}
