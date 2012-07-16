using System;
using System.Globalization;
using Ela.Runtime.ObjectModel;

namespace Ela.Runtime.Classes
{
    internal sealed class DoubleInstance : Class
    {
        internal override string Showf(string format, ElaValue value, ExecutionContext ctx)
        {
            return value.ToString(format, Culture.NumberFormat);
        }

        internal override bool Equal(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.DBL)
            {
                if (right.TypeId == ElaMachine.REA)
                    return left.Ref.AsDouble() == right.DirectGetReal();
                else
                {
                    NoOverloadBinary(TCF.DOUBLE, right, "equal", ctx);
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
                else
                {
                    NoOverloadBinary(TCF.DOUBLE, right, "notequal", ctx);
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
                else
                {
                    NoOverloadBinary(TCF.DOUBLE, right, "greater", ctx);
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
                else
                {
                    NoOverloadBinary(TCF.DOUBLE, right, "lesser", ctx);
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
                else
                {
                    NoOverloadBinary(TCF.DOUBLE, right, "greaterequal", ctx);
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
                else
                {
                    NoOverloadBinary(TCF.DOUBLE, right, "lesserequal", ctx);
                    return false;
                }
            }

            return left.Ref.AsDouble() <= right.Ref.AsDouble();
        }

        internal override ElaValue Add(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.DBL)
            {
                if (right.TypeId == ElaMachine.REA)
                    return new ElaValue(left.Ref.AsDouble() + right.DirectGetReal());
                else
                {
                    NoOverloadBinary(TCF.DOUBLE, right, "add", ctx);
                    return Default();
                }
            }

            return new ElaValue(left.Ref.AsDouble() + right.Ref.AsDouble());
        }

        internal override ElaValue Subtract(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.DBL)
            {
                if (right.TypeId == ElaMachine.REA)
                    return new ElaValue(left.Ref.AsDouble() - right.DirectGetReal());
                else
                {
                    NoOverloadBinary(TCF.DOUBLE, right, "subtract", ctx);
                    return Default();
                }
            }

            return new ElaValue(left.Ref.AsDouble() - right.Ref.AsDouble());
        }

        internal override ElaValue Multiply(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.DBL)
            {
                if (right.TypeId == ElaMachine.REA)
                    return new ElaValue(left.Ref.AsDouble() * right.DirectGetReal());
                else
                {
                    NoOverloadBinary(TCF.DOUBLE, right, "multiply", ctx);
                    return Default();
                }
            }

            return new ElaValue(left.Ref.AsDouble() * right.Ref.AsDouble());
        }

        internal override ElaValue Divide(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.DBL)
            {
                if (right.TypeId == ElaMachine.REA)
                    return new ElaValue(left.Ref.AsDouble() / right.DirectGetReal());
                else
                {
                    NoOverloadBinary(TCF.DOUBLE, right, "divide", ctx);
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
                if (right.TypeId == ElaMachine.REA)
                    return new ElaValue(left.Ref.AsDouble() % right.DirectGetReal());
                else
                {
                    NoOverloadBinary(TCF.DOUBLE, right, "remainder", ctx);
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
                if (right.TypeId == ElaMachine.REA)
                    return new ElaValue(Math.Pow(left.Ref.AsDouble(), right.DirectGetReal()));
                else
                {
                    NoOverloadBinary(TCF.DOUBLE, right, "power", ctx);
                    return Default();
                }
            }

            return new ElaValue(Math.Pow(left.Ref.AsDouble(), right.Ref.AsDouble()));
        }

        internal static ElaValue Modulus(double x, double y, ExecutionContext ctx)
        {
            var r = x % y;
            return x < 0 && y > 0 || x > 0 && y < 0 ? new ElaValue(r + y) : new ElaValue(r);
        }

        internal override ElaValue Modulus(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.DBL)
            {
                if (right.TypeId == ElaMachine.REA)
                    return DoubleInstance.Modulus(left.Ref.AsDouble(), right.DirectGetReal(), ctx);
                else
                {
                    NoOverloadBinary(TCF.DOUBLE, right, "modulus", ctx);
                    return Default();
                }
            }

            return Modulus(left.Ref.AsDouble(), right.Ref.AsDouble(), ctx);
        }
        
        internal override ElaValue Negate(ElaValue value, ExecutionContext ctx)
        {
            return new ElaValue(-value.Ref.AsDouble());
        }
    }
}
