using System;
using Ela.Runtime.ObjectModel;

namespace Ela.Runtime.Classes
{
    internal sealed class TypeInfoInstance : Class
    {
        internal override bool Equal(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.TYP)
            {
                ctx.InvalidRightOperand(left, right, "equal");
                return false;
            }

            return ((ElaTypeInfo)left.Ref).ReflectedTypeCode == ((ElaTypeInfo)right.Ref).ReflectedTypeCode;
        }

        internal override bool NotEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.TYP)
            {
                ctx.InvalidRightOperand(left, right, "notequal");
                return false;
            }

            return ((ElaTypeInfo)left.Ref).ReflectedTypeCode != ((ElaTypeInfo)right.Ref).ReflectedTypeCode;
        }

        internal override ElaValue GetLength(ElaValue value, ExecutionContext ctx)
        {
            return new ElaValue(3);
        }

        internal override ElaValue GetValue(ElaValue value, ElaValue key, ExecutionContext ctx)
        {
            var ti = (ElaTypeInfo)value.Ref;

            if (key.TypeId == ElaMachine.STR)
            {
                var str = key.DirectGetString();

                switch (str)
                {
                    case "typeCode": return new ElaValue(ti.ReflectedTypeCode);
                    case "specName": return new ElaValue(ti.ReflectedTypeName);
                    case "instances": return new ElaValue(new ElaTuple(ti.ReflectedInstances));
                    default:
                        ctx.IndexOutOfRange(key, value);
                        return Default();
                }
            }
            else if (key.TypeId == ElaMachine.INT)
            {
                var i = key.I4;

                switch (i)
                {
                    case 0: return new ElaValue(ti.ReflectedTypeCode);
                    case 1: return new ElaValue(ti.ReflectedTypeName);
                    case 2: return new ElaValue(new ElaTuple(ti.ReflectedInstances));
                    default:
                        ctx.IndexOutOfRange(key, value);
                        return Default();
                }
            }

            ctx.InvalidIndexType(key);
            return Default();
        }
    }
}
