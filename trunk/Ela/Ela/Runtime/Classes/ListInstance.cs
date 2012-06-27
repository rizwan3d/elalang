using System;
using Ela.CodeModel;
using Ela.Parsing;
using Ela.Runtime.ObjectModel;

namespace Ela.Runtime.Classes
{
    internal sealed class ListInstance : Class
    {
        internal override ElaValue Parse(ElaValue instance, string format, string value, ExecutionContext ctx)
        {
            try
            {
                var p = new ElaParser();
                var res = p.Parse(value);

                if (!res.Success)
                    return Fail(instance, value, ctx);

                var block = res.Program.TopLevel;

                if (block.Equations.Count != 1 || block.Equations[0].Left.Type != ElaNodeType.ListLiteral)
                    return Fail(instance, value, ctx);

                var lit = (ElaListLiteral)block.Equations[0].Left;
                var list = BuildList(lit);

                if (list != null)
                    return new ElaValue(list);

                return Fail(instance, value, ctx);

            }
            catch (Exception)
            {
                return Fail(instance, value, ctx);
            }
        }
        
        private ElaList BuildList(ElaListLiteral lit)
        {
            var arr = new ElaValue[lit.Values.Count];

            for (var i = 0; i < arr.Length; i++)
            {
                var e = lit.Values[i];

                if (e.Type != ElaNodeType.Primitive)
                    return null;

                var prim = ((ElaPrimitive)e).Value;
                var val = default(ElaValue);

                switch (prim.LiteralType)
                {
                    case ElaTypeCode.Integer:
                        val = new ElaValue(prim.AsInteger());
                        break;
                    case ElaTypeCode.Long:
                        val = new ElaValue(prim.AsLong());
                        break;
                    case ElaTypeCode.Single:
                        val = new ElaValue(prim.AsReal());
                        break;
                    case ElaTypeCode.Double:
                        val = new ElaValue(prim.AsDouble());
                        break;
                    case ElaTypeCode.Char:
                        val = new ElaValue(prim.AsChar());
                        break;
                    case ElaTypeCode.Boolean:
                        val = new ElaValue(prim.AsBoolean());
                        break;
                    case ElaTypeCode.String:
                        val = new ElaValue(prim.AsString());
                        break;
                    default:
                        return null;
                }

                arr[i] = val;
            }

            return ElaList.FromEnumerable(arr);
        }

        private ElaValue Fail(ElaValue instance, string value, ExecutionContext ctx)
        {
            ctx.UnableRead(instance, value);
            return Default();
        }

        internal override ElaValue Head(ElaValue left, ExecutionContext ctx)
        {
            return ((ElaList)left.Ref).InternalValue;
        }

        internal override ElaValue Tail(ElaValue left, ExecutionContext ctx)
        {
            return left.Ref.Tail(ctx);
        }

        internal override bool IsNil(ElaValue left, ExecutionContext ctx)
        {
            return left.Ref == ElaList.Empty;
        }
    }
}
