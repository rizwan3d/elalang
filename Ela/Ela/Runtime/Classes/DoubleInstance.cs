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
                    ctx.InvalidOperand(left, right, "equal");
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
                    ctx.InvalidOperand(left, right, "notequal");
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
                    ctx.InvalidOperand(left, right, "greater");
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
                    ctx.InvalidOperand(left, right, "lesser");
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
                    ctx.InvalidOperand(left, right, "greaterequal");
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
                    ctx.InvalidOperand(left, right, "lesserequal");
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
                    ctx.InvalidOperand(left, right, "add");
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
                    ctx.InvalidOperand(left, right, "subtract");
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
                    ctx.InvalidOperand(left, right, "multiply");
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
                    ctx.InvalidOperand(left, right, "divide");
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
                    ctx.InvalidOperand(left, right, "remainder");
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
                    ctx.InvalidOperand(left, right, "power");
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
            if (right.TypeId != ElaMachine.REA)
            {
                if (right.TypeId == ElaMachine.INT)
                    return Modulus(left.Ref.AsDouble(), right.I4, ctx);
                else if (right.TypeId == ElaMachine.LNG)
                    return Modulus(left.Ref.AsDouble(), right.Ref.AsLong(), ctx);
                else if (right.TypeId == ElaMachine.REA)
                    return DoubleInstance.Modulus(left.Ref.AsDouble(), right.DirectGetReal(), ctx);
                else
                {
                    ctx.InvalidOperand(left, right, "remainder");
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
