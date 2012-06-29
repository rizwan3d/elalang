using System;
using Ela.CodeModel;
using Ela.Parsing;
using Ela.Runtime.ObjectModel;

namespace Ela.Runtime.Classes
{
    internal sealed class RecordInstance : Class
    {
        internal override ElaValue Concatenate(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            left = left.Ref.Force(left, ctx);
            right = right.Ref.Force(right, ctx);

            if (right.TypeId != ElaMachine.REC)
            {
                ctx.InvalidOperand(left, right, "concatenate");
                return Default();
            }

            return new ElaValue(ElaRecord.Concat((ElaRecord)left.Ref, (ElaRecord)right.Ref));
        }

        internal override ElaValue GetLength(ElaValue value, ExecutionContext ctx)
        {
            return new ElaValue(((ElaRecord)value.Ref).Length);
        }

        internal override ElaValue GetValue(ElaValue value, ElaValue index, ExecutionContext ctx)
        {
            return ((ElaRecord)value.Ref).GetValue(index, ctx);
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

                if (block.Equations.Count != 1 || block.Equations[0].Left.Type != ElaNodeType.RecordLiteral)
                    return Fail(instance, value, ctx);

                var lit = (ElaRecordLiteral)block.Equations[0].Left;
                var rec = BuildRecord(lit);

                if (rec != null)
                    return new ElaValue(rec);

                return Fail(instance, value, ctx);

            }
            catch (Exception)
            {
                return Fail(instance, value, ctx);
            }
        }
        
        private ElaRecord BuildRecord(ElaRecordLiteral lit)
        {
            var arr = new ElaRecordField[lit.Fields.Count];

            for (var i = 0; i < arr.Length; i++)
            {
                var f = lit.Fields[i];

                if (f.FieldValue.Type != ElaNodeType.Primitive)
                    return null;

                var prim = ((ElaPrimitive)f.FieldValue).Value;
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

                arr[i] = new ElaRecordField(f.FieldName, val);
            }

            return new ElaRecord(arr);
        }

        private ElaValue Fail(ElaValue instance, string value, ExecutionContext ctx)
        {
            ctx.UnableRead(instance, value);
            return Default();
        }
    }
}
