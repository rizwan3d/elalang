using System;
using Ela.CodeModel;
using Ela.Parsing;
using Ela.Runtime.ObjectModel;

namespace Ela.Runtime.Classes
{
    internal sealed class TupleInstance : Class
    {
        internal override ElaValue Concatenate(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            left = left.Ref.Force(left, ctx);
            right = right.Ref.Force(right, ctx);

            if (right.TypeId != ElaMachine.TUP)
            {
                ctx.InvalidRightOperand(left, right, "concatenate");
                return Default();
            }

            return new ElaValue(ElaTuple.Concat((ElaTuple)left.Ref, (ElaTuple)right.Ref));
        }

        internal override ElaValue GetLength(ElaValue value, ExecutionContext ctx)
        {
            return new ElaValue(((ElaTuple)value.Ref).Length);
        }

        internal override ElaValue GetValue(ElaValue value, ElaValue key, ExecutionContext ctx)
        {
            if (key.TypeId != ElaMachine.INT)
            {
                ctx.InvalidIndexType(key);
                return Default();
            }

            var tup = (ElaTuple)value.Ref;

            if (key.I4 < tup.Length && key.I4 > -1)
                return tup.Values[key.I4];

            ctx.IndexOutOfRange(key, value);
            return Default();
        }

        internal override ElaValue Parse(ElaValue instance, string format, string value, ExecutionContext ctx)
        {
            try
            {
                var p = new ElaParser();
                var res = p.Parse(value);

                if (!res.Success)
                    return Fail(instance, value, ctx);

                var block = res.Program.TopLevel;

                if (block.Equations.Count != 1 || block.Equations[0].Left.Type != ElaNodeType.TupleLiteral)
                    return Fail(instance, value, ctx);

                var lit = (ElaTupleLiteral)block.Equations[0].Left;
                var tup = BuildTuple(lit);

                if (tup != null)
                    return new ElaValue(tup);

                return Fail(instance, value, ctx);

            }
            catch (Exception)
            {
                return Fail(instance, value, ctx);
            }
        }

        private ElaTuple BuildTuple(ElaTupleLiteral lit)
        {
            var arr = new ElaValue[lit.Parameters.Count];

            for (var i = 0; i < arr.Length; i++)
            {
                var e = lit.Parameters[i];

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

            return new ElaTuple(arr);
        }

        private ElaValue Fail(ElaValue instance, string value, ExecutionContext ctx)
        {
            ctx.UnableRead(instance, value);
            return Default();
        }
    }
}
