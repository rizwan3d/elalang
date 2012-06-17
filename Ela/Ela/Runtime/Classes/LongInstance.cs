using System;
using System.Globalization;
using Ela.Runtime.ObjectModel;

namespace Ela.Runtime.Classes
{
    internal sealed class LongInstance : Class
    {
        internal override ElaValue BitwiseNot(ElaValue @this, ExecutionContext ctx)
        {
            return new ElaValue(~@this.Ref.AsLong());
        }

        internal override ElaValue BitwiseAnd(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.LNG)
            {
                if (right.TypeId == ElaMachine.INT)
                    return new ElaValue(left.Ref.AsLong() & right.I4);
                else
                {
                    ctx.InvalidRightOperand(left, right, "bitwiseand");
                    return Default();
                }
            }

            return new ElaValue(left.Ref.AsLong() & right.Ref.AsLong());
        }

        internal override ElaValue BitwiseOr(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.LNG)
            {
                if (right.TypeId == ElaMachine.INT)
                {
                    var lo = (Int64)right.I4;
                    return new ElaValue(left.Ref.AsLong() | lo);
                }
                else
                {
                    ctx.InvalidRightOperand(left, right, "bitwiseor");
                    return Default();
                }
            }

            return new ElaValue(left.Ref.AsLong() | right.Ref.AsLong());
        }

        internal override ElaValue BitwiseXor(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.LNG)
            {
                if (right.TypeId == ElaMachine.INT)
                    return new ElaValue(left.Ref.AsLong() ^ right.I4);
                else
                {
                    ctx.InvalidRightOperand(left, right, "bitwisexor");
                    return Default();
                }
            }

            return new ElaValue(left.Ref.AsLong() ^ right.Ref.AsLong());
        }

        internal override ElaValue ShiftLeft(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.INT)
            {
                ctx.InvalidRightOperand(left, right, "shiftleft");
                return Default();
            }

            return new ElaValue(left.Ref.AsLong() << right.I4);
        }

        internal override ElaValue ShiftRight(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.INT)
            {
                ctx.InvalidRightOperand(left, right, "shiftright");
                return Default();
            }

            return new ElaValue(left.Ref.AsLong() >> right.I4);
        }

        internal override ElaValue Successor(ElaValue @this, ExecutionContext ctx)
        {
            return new ElaValue(@this.Ref.AsLong() + 1);
        }

        internal override ElaValue Predecessor(ElaValue @this, ExecutionContext ctx)
        {
            return new ElaValue(@this.Ref.AsLong() - 1);
        }

        internal override bool Equal(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.LNG)
            {
                if (right.TypeId == ElaMachine.REA)
                    return left.Ref.AsLong() == right.DirectGetReal();
                else if (right.TypeId == ElaMachine.INT)
                    return left.Ref.AsLong() == right.I4;
                else if (right.TypeId == ElaMachine.DBL)
                    return left.Ref.AsLong() == right.Ref.AsDouble();
                else
                {
                    ctx.InvalidRightOperand(left, right, "equal");
                    return false;
                }
            }

            return left.Ref.AsLong() == right.Ref.AsLong();
        }

        internal override bool NotEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.LNG)
            {
                if (right.TypeId == ElaMachine.REA)
                    return left.Ref.AsLong() != right.DirectGetReal();
                else if (right.TypeId == ElaMachine.INT)
                    return left.Ref.AsLong() != right.I4;
                else if (right.TypeId == ElaMachine.DBL)
                    return left.Ref.AsLong() != right.Ref.AsDouble();
                else
                {
                    ctx.InvalidRightOperand(left, right, "notequal");
                    return false;
                }
            }

            return left.Ref.AsLong() != right.Ref.AsLong();
        }

        internal override ElaValue Add(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.LNG)
            {
                if (right.TypeId == ElaMachine.REA)
                    return new ElaValue(left.Ref.AsLong() + right.DirectGetReal());
                else if (right.TypeId == ElaMachine.INT)
                    return new ElaValue(left.Ref.AsLong() + right.I4);
                else if (right.TypeId == ElaMachine.DBL)
                    return new ElaValue(left.Ref.AsLong() + right.Ref.AsDouble());
                else
                {
                    ctx.InvalidRightOperand(left, right, "add");
                    return Default();
                }
            }

            return new ElaValue(left.Ref.AsLong() + right.Ref.AsLong());
        }

        internal override ElaValue Subtract(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.LNG)
            {
                if (right.TypeId == ElaMachine.REA)
                    return new ElaValue(left.Ref.AsLong() - right.DirectGetReal());
                else if (right.TypeId == ElaMachine.INT)
                    return new ElaValue(left.Ref.AsLong() + right.I4);
                else if (right.TypeId == ElaMachine.DBL)
                    return new ElaValue(left.Ref.AsLong() - right.Ref.AsDouble());
                else
                {
                    ctx.InvalidRightOperand(left, right, "subtract");
                    return Default();
                }
            }

            return new ElaValue(left.Ref.AsLong() - right.Ref.AsLong());
        }

        internal override ElaValue Multiply(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.LNG)
            {
                if (right.TypeId == ElaMachine.REA)
                    return new ElaValue(left.Ref.AsLong() * right.DirectGetReal());
                else if (right.TypeId == ElaMachine.INT)
                    return new ElaValue(left.Ref.AsLong() + right.I4);
                else if (right.TypeId == ElaMachine.DBL)
                    return new ElaValue(left.Ref.AsLong() * right.Ref.AsDouble());
                else
                {
                    ctx.InvalidRightOperand(left, right, "multiply");
                    return Default();
                }
            }

            return new ElaValue(left.Ref.AsLong() * right.Ref.AsLong());
        }

        internal override ElaValue Divide(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.LNG)
            {
                if (right.TypeId == ElaMachine.REA)
                    return new ElaValue(left.Ref.AsLong() / right.DirectGetReal());
                else if (right.TypeId == ElaMachine.INT)
                {
                    if (right.I4 == 0)
                    {
                        ctx.DivideByZero(left);
                        return Default();
                    }

                    return new ElaValue(left.Ref.AsLong() / right.I4);
                }
                else if (right.TypeId == ElaMachine.DBL)
                    return new ElaValue(left.Ref.AsLong() / right.Ref.AsDouble());
                else
                {
                    ctx.InvalidRightOperand(left, right, "divide");
                    return Default();
                }
            }

            if (right.Ref.AsLong() == 0)
            {
                ctx.DivideByZero(left);
                return Default();
            }

            return new ElaValue(left.Ref.AsLong() / right.Ref.AsLong());
        }

        internal override ElaValue Remainder(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.LNG)
            {
                if (right.TypeId == ElaMachine.REA)
                    return new ElaValue(left.Ref.AsLong() % right.DirectGetReal());
                else if (right.TypeId == ElaMachine.INT)
                {
                    if (right.I4 == 0)
                    {
                        ctx.DivideByZero(left);
                        return Default();
                    }

                    return new ElaValue(left.Ref.AsLong() % right.I4);
                }
                else if (right.TypeId == ElaMachine.DBL)
                    return new ElaValue(left.Ref.AsLong() % right.Ref.AsDouble());
                else
                {
                    ctx.InvalidRightOperand(left, right, "remainder");
                    return Default();
                }
            }

            if (right.Ref.AsLong() == 0)
            {
                ctx.DivideByZero(left);
                return Default();
            }

            return new ElaValue(left.Ref.AsLong() % right.Ref.AsLong());
        }
        
        internal override ElaValue Power(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.LNG)
            {
                if (right.TypeId == ElaMachine.REA)
                    return new ElaValue(Math.Pow(left.Ref.AsLong(), right.DirectGetReal()));
                else if (right.TypeId == ElaMachine.INT)
                    return new ElaValue(Math.Pow(left.Ref.AsLong(), right.I4));
                else if (right.TypeId == ElaMachine.DBL)
                    return new ElaValue(Math.Pow(left.Ref.AsLong(), right.Ref.AsDouble()));
                else
                {
                    ctx.InvalidRightOperand(left, right, "power");
                    return Default();
                }
            }

            return new ElaValue(Math.Pow(left.Ref.AsLong(), right.Ref.AsLong()));
        }

        internal override ElaValue Negate(ElaValue value, ExecutionContext ctx)
        {
            return new ElaValue(-value.Ref.AsLong());
        }

        internal override bool Greater(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.LNG)
            {
                if (right.TypeId == ElaMachine.REA)
                    return left.Ref.AsLong() > right.DirectGetReal();
                else if (right.TypeId == ElaMachine.INT)
                    return left.Ref.AsLong() > right.I4;
                else if (right.TypeId == ElaMachine.DBL)
                    return left.Ref.AsLong() > right.Ref.AsDouble();
                else
                {
                    ctx.InvalidRightOperand(left, right, "greater");
                    return false;
                }
            }

            return left.Ref.AsLong() > right.Ref.AsLong();
        }

        internal override bool Lesser(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.LNG)
            {
                if (right.TypeId == ElaMachine.REA)
                    return left.Ref.AsLong() < right.DirectGetReal();
                else if (right.TypeId == ElaMachine.INT)
                    return left.Ref.AsLong() < right.I4;
                else if (right.TypeId == ElaMachine.DBL)
                    return left.Ref.AsLong() < right.Ref.AsDouble();
                else
                {
                    ctx.InvalidRightOperand(left, right, "lesser");
                    return false;
                }
            }

            return left.Ref.AsLong() < right.Ref.AsLong();
        }

        internal override bool GreaterEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.LNG)
            {
                if (right.TypeId == ElaMachine.REA)
                    return left.Ref.AsLong() >= right.DirectGetReal();
                else if (right.TypeId == ElaMachine.INT)
                    return left.Ref.AsLong() >= right.I4;
                else if (right.TypeId == ElaMachine.DBL)
                    return left.Ref.AsLong() >= right.Ref.AsDouble();
                else
                {
                    ctx.InvalidRightOperand(left, right, "greaterequal");
                    return false;
                }
            }

            return left.Ref.AsLong() >= right.Ref.AsLong();
        }

        internal override bool LesserEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.LNG)
            {
                if (right.TypeId == ElaMachine.REA)
                    return left.Ref.AsLong() <= right.DirectGetReal();
                else if (right.TypeId == ElaMachine.INT)
                    return left.Ref.AsLong() <= right.I4;
                else if (right.TypeId == ElaMachine.DBL)
                    return left.Ref.AsLong() <= right.Ref.AsDouble();
                else
                {
                    ctx.InvalidRightOperand(left, right, "lesserequal");
                    return false;
                }
            }

            return left.Ref.AsLong() <= right.Ref.AsLong();
        }

        internal override ElaValue Parse(ElaValue instance, string format, string value, ExecutionContext ctx)
        {
            var res = 0L;

            if (!Int64.TryParse(value, NumberStyles.Any, Culture.NumberFormat, out res))
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
                case ElaTypeCode.Double:
                    return new ElaValue((Int32)value.Ref.AsLong());
                case ElaTypeCode.Long:
                    return new ElaValue((Int64)value.Ref.AsLong());
                case ElaTypeCode.Single:
                    return new ElaValue((Single)value.Ref.AsLong());
                default:
                    {
                        ctx.ConversionFailed(value, typeInfo.ReflectedTypeName);
                        return Default();
                    };
            }
        }
    }
}
