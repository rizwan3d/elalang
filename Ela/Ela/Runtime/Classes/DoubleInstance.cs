using System;
using System.Globalization;
using Ela.Runtime.ObjectModel;

namespace Ela.Runtime.Classes
{
    internal sealed class DoubleInstance : Class
    {
        internal override ElaValue Successor(ElaValue @this, ExecutionContext ctx)
        {
            return new ElaValue(@this.Ref.AsDouble() + 1);
        }

        internal override ElaValue Predecessor(ElaValue @this, ExecutionContext ctx)
        {
            return new ElaValue(@this.Ref.AsDouble() - 1);
        }

        internal override bool Equal(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.DBL)
            {
                if (right.TypeId == ElaMachine.REA)
                    return left.Ref.AsDouble() == right.DirectGetReal();
                else if (right.TypeId == ElaMachine.INT)
                    return left.Ref.AsDouble() == right.I4;
                else if (right.TypeId == ElaMachine.LNG)
                    return left.Ref.AsDouble() == right.Ref.AsLong();
                else
                {
                    ctx.InvalidRightOperand(left, right, "equal");
                    return false;
                }
            }

            return left.Ref.AsDouble() == right.Ref.AsDouble();
        }

        internal override bool NotEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.DBL)
            {
                if (right.TypeId == ElaMachine.REA)
                    return left.Ref.AsDouble() != right.DirectGetReal();
                else if (right.TypeId == ElaMachine.INT)
                    return left.Ref.AsDouble() != right.I4;
                else if (right.TypeId == ElaMachine.LNG)
                    return left.Ref.AsDouble() != right.Ref.AsLong();
                else
                {
                    ctx.InvalidRightOperand(left, right, "notequal");
                    return false;
                }
            }

            return left.Ref.AsDouble() != right.Ref.AsDouble();
        }

        internal override bool Greater(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.DBL)
            {
                if (right.TypeId == ElaMachine.REA)
                    return left.Ref.AsDouble() > right.DirectGetReal();
                else if (right.TypeId == ElaMachine.LNG)
                    return left.Ref.AsDouble() > right.Ref.AsLong();
                else if (right.TypeId == ElaMachine.INT)
                    return left.Ref.AsDouble() > right.I4;
                else
                {
                    ctx.InvalidRightOperand(left, right, "greater");
                    return false;
                }
            }

            return left.Ref.AsDouble() > right.Ref.AsDouble();
        }

        internal override bool Lesser(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.DBL)
            {
                if (right.TypeId == ElaMachine.REA)
                    return left.Ref.AsDouble() < right.DirectGetReal();
                else if (right.TypeId == ElaMachine.LNG)
                    return left.Ref.AsDouble() < right.Ref.AsLong();
                else if (right.TypeId == ElaMachine.INT)
                    return left.Ref.AsDouble() < right.I4;
                else
                {
                    ctx.InvalidRightOperand(left, right, "lesser");
                    return false;
                }
            }

            return left.Ref.AsDouble() < right.Ref.AsDouble();
        }

        internal override bool GreaterEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.DBL)
            {
                if (right.TypeId == ElaMachine.REA)
                    return left.Ref.AsDouble() >= right.DirectGetReal();
                else if (right.TypeId == ElaMachine.LNG)
                    return left.Ref.AsDouble() >= right.Ref.AsLong();
                else if (right.TypeId == ElaMachine.INT)
                    return left.Ref.AsDouble() >= right.I4;
                else
                {
                    ctx.InvalidRightOperand(left, right, "greaterequal");
                    return false;
                }
            }

            return left.Ref.AsDouble() >= right.Ref.AsDouble();
        }

        internal override bool LesserEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.DBL)
            {
                if (right.TypeId == ElaMachine.REA)
                    return left.Ref.AsDouble() <= right.DirectGetReal();
                else if (right.TypeId == ElaMachine.LNG)
                    return left.Ref.AsDouble() <= right.Ref.AsLong();
                else if (right.TypeId == ElaMachine.INT)
                    return left.Ref.AsDouble() <= right.I4;
                else
                {
                    ctx.InvalidRightOperand(left, right, "lesserequal");
                    return false;
                }
            }

            return left.Ref.AsDouble() <= right.Ref.AsDouble();
        }

        internal override ElaValue Add(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.DBL)
            {
                if (right.TypeId == ElaMachine.INT)
                    return new ElaValue(left.Ref.AsDouble() + right.I4);
                else if (right.TypeId == ElaMachine.LNG)
                    return new ElaValue(left.Ref.AsDouble() + right.Ref.AsLong());
                else if (right.TypeId == ElaMachine.REA)
                    return new ElaValue(left.Ref.AsDouble() + right.DirectGetReal());
                else
                {
                    ctx.InvalidRightOperand(left, right, "add");
                    return Default();
                }
            }

            return new ElaValue(left.Ref.AsDouble() + right.Ref.AsDouble());
        }

        internal override ElaValue Subtract(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.DBL)
            {
                if (right.TypeId == ElaMachine.INT)
                    return new ElaValue(left.Ref.AsDouble() - right.I4);
                else if (right.TypeId == ElaMachine.LNG)
                    return new ElaValue(left.Ref.AsDouble() - right.Ref.AsLong());
                else if (right.TypeId == ElaMachine.REA)
                    return new ElaValue(left.Ref.AsDouble() - right.DirectGetReal());
                else
                {
                    ctx.InvalidRightOperand(left, right, "subtract");
                    return Default();
                }
            }

            return new ElaValue(left.Ref.AsDouble() - right.Ref.AsDouble());
        }

        internal override ElaValue Multiply(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.DBL)
            {
                if (right.TypeId == ElaMachine.INT)
                    return new ElaValue(left.Ref.AsDouble() * right.I4);
                else if (right.TypeId == ElaMachine.LNG)
                    return new ElaValue(left.Ref.AsDouble() * right.Ref.AsLong());
                else if (right.TypeId == ElaMachine.REA)
                    return new ElaValue(left.Ref.AsDouble() * right.DirectGetReal());
                else
                {
                    ctx.InvalidRightOperand(left, right, "multiply");
                    return Default();
                }
            }

            return new ElaValue(left.Ref.AsDouble() * right.Ref.AsDouble());
        }

        internal override ElaValue Divide(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.DBL)
            {
                if (right.TypeId == ElaMachine.INT)
                    return new ElaValue(left.Ref.AsDouble() / right.I4);
                else if (right.TypeId == ElaMachine.LNG)
                    return new ElaValue(left.Ref.AsDouble() / right.Ref.AsLong());
                else if (right.TypeId == ElaMachine.REA)
                    return new ElaValue(left.Ref.AsDouble() / right.DirectGetReal());
                else
                {
                    ctx.InvalidRightOperand(left, right, "divide");
                    return Default();
                }
            }

            if (right.Ref.AsDouble() == 0)
            {
                ctx.DivideByZero(left);
                return Default();
            }

            return new ElaValue(left.Ref.AsDouble() / right.Ref.AsDouble());
        }

        internal override ElaValue Remainder(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.DBL)
            {
                if (right.TypeId == ElaMachine.INT)
                    return new ElaValue(left.Ref.AsDouble() % right.I4);
                else if (right.TypeId == ElaMachine.LNG)
                    return new ElaValue(left.Ref.AsDouble() % right.Ref.AsLong());
                else if (right.TypeId == ElaMachine.REA)
                    return new ElaValue(left.Ref.AsDouble() % right.DirectGetReal());
                else
                {
                    ctx.InvalidRightOperand(left, right, "remainder");
                    return Default();
                }
            }

            if (right.Ref.AsDouble() == 0)
            {
                ctx.DivideByZero(left);
                return Default();
            }

            return new ElaValue(left.Ref.AsDouble() % right.Ref.AsDouble());
        }

        internal override ElaValue Power(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.DBL)
            {
                if (right.TypeId == ElaMachine.INT)
                    return new ElaValue(Math.Pow(left.Ref.AsDouble(), right.I4));
                else if (right.TypeId == ElaMachine.LNG)
                    return new ElaValue(Math.Pow(left.Ref.AsDouble(), right.Ref.AsLong()));
                else if (right.TypeId == ElaMachine.REA)
                    return new ElaValue(Math.Pow(left.Ref.AsDouble(), right.DirectGetReal()));
                else
                {
                    ctx.InvalidRightOperand(left, right, "power");
                    return Default();
                }
            }

            return new ElaValue(Math.Pow(left.Ref.AsDouble(), right.Ref.AsDouble()));
        }
        
        internal override ElaValue Negate(ElaValue value, ExecutionContext ctx)
        {
            return new ElaValue(-value.Ref.AsDouble());
        }

        internal override ElaValue Parse(ElaValue instance, string format, string value, ExecutionContext ctx)
        {
            var res = 0d;

            if (!Double.TryParse(value, NumberStyles.Any, Culture.NumberFormat, out res))
            {
                ctx.UnableRead(instance, value);
                return Default();
            }

            return new ElaValue(res);
        }

        internal override ElaValue CastTo(ElaTypeInfo typeInfo, ElaValue value, ExecutionContext ctx)
        {
            var tc = (ElaTypeCode)typeInfo.ReflectedTypeCode;

            switch (tc)
            {
                case ElaTypeCode.Integer:
                    return new ElaValue((Int32)value.Ref.AsDouble());
                case ElaTypeCode.Long:
                    return new ElaValue((Int64)value.Ref.AsDouble());
                case ElaTypeCode.Single:
                    return new ElaValue((Single)value.Ref.AsDouble());
                default:
                    {
                        ctx.ConversionFailed(value, typeInfo.ReflectedTypeName);
                        return Default();
                    };
            }
        }
    }
}
