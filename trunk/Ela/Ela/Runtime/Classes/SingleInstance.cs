using System;
using System.Globalization;
using Ela.Runtime.ObjectModel;

namespace Ela.Runtime.Classes
{
    internal sealed class SingleInstance : Class
    {
        internal override ElaValue Successor(ElaValue @this, ExecutionContext ctx)
        {
            return new ElaValue(@this.DirectGetReal() + 1);
        }

        internal override ElaValue Predecessor(ElaValue @this, ExecutionContext ctx)
        {
            return new ElaValue(@this.DirectGetReal() - 1);
        }

        internal override bool Equal(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.REA)
            {
                if (right.TypeId == ElaMachine.INT)
                    return left.DirectGetReal() == right.I4;
                else if (right.TypeId == ElaMachine.LNG)
                    return left.DirectGetReal() == right.Ref.AsLong();
                else if (right.TypeId == ElaMachine.DBL)
                    return left.DirectGetReal() == right.Ref.AsDouble();
                else
                {
                    ctx.InvalidOperand(left, right, "equal");
                    return false;
                }
            }

            return left.DirectGetReal() == right.DirectGetReal();
        }

        internal override bool NotEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.REA)
            {
                if (right.TypeId == ElaMachine.INT)
                    return left.DirectGetReal() != right.I4;
                else if (right.TypeId == ElaMachine.LNG)
                    return left.DirectGetReal() != right.Ref.AsLong();
                else if (right.TypeId == ElaMachine.DBL)
                    return left.DirectGetReal() != right.Ref.AsDouble();
                else
                {
                    ctx.InvalidOperand(left, right, "notequal");
                    return false;
                }
            }

            return left.DirectGetReal() != right.DirectGetReal();
        }

        internal override ElaValue Add(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.REA)
            {
                if (right.TypeId == ElaMachine.INT)
                    return new ElaValue(left.DirectGetReal() + right.I4);
                else if (right.TypeId == ElaMachine.LNG)
                    return new ElaValue(left.DirectGetReal() + right.Ref.AsLong());
                else if (right.TypeId == ElaMachine.DBL)
                    return new ElaValue(left.DirectGetReal() + right.Ref.AsDouble());
                else
                {
                    ctx.InvalidOperand(left, right, "add");
                    return Default();
                }
            }

            return new ElaValue(left.DirectGetReal() + right.DirectGetReal());
        }

        internal override ElaValue Subtract(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.REA)
            {
                if (right.TypeId == ElaMachine.INT)
                    return new ElaValue(left.DirectGetReal() - right.I4);
                else if (right.TypeId == ElaMachine.LNG)
                    return new ElaValue(left.DirectGetReal() - right.Ref.AsLong());
                else if (right.TypeId == ElaMachine.DBL)
                    return new ElaValue(left.DirectGetReal() - right.Ref.AsDouble());
                else
                {
                    ctx.InvalidOperand(left, right, "subtract");
                    return Default();
                }
            }

            return new ElaValue(left.DirectGetReal() - right.DirectGetReal());
        }

        internal override ElaValue Multiply(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.REA)
            {
                if (right.TypeId == ElaMachine.INT)
                    return new ElaValue(left.DirectGetReal() * right.I4);
                else if (right.TypeId == ElaMachine.LNG)
                    return new ElaValue(left.DirectGetReal() * right.Ref.AsLong());
                else if (right.TypeId == ElaMachine.DBL)
                    return new ElaValue(left.DirectGetReal() * right.Ref.AsDouble());
                else
                {
                    ctx.InvalidOperand(left, right, "multiply");
                    return Default();
                }
            }

            return new ElaValue(left.DirectGetReal() * right.DirectGetReal());
        }

        internal override ElaValue Divide(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.REA)
            {
                if (right.TypeId == ElaMachine.INT)
                    return new ElaValue(left.DirectGetReal() / right.I4);
                else if (right.TypeId == ElaMachine.LNG)
                    return new ElaValue(left.DirectGetReal() / right.Ref.AsLong());
                else if (right.TypeId == ElaMachine.DBL)
                    return new ElaValue(left.DirectGetReal() / right.Ref.AsDouble());
                else
                {
                    ctx.InvalidOperand(left, right, "divide");
                    return Default();
                }
            }

            if (right.DirectGetReal() == 0)
            {
                ctx.DivideByZero(left);
                return Default();
            }

            return new ElaValue(left.DirectGetReal() / right.DirectGetReal());
        }

        internal override ElaValue Remainder(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.REA)
            {
                if (right.TypeId == ElaMachine.INT)
                    return new ElaValue(left.DirectGetReal() % right.I4);
                else if (right.TypeId == ElaMachine.LNG)
                    return new ElaValue(left.DirectGetReal() % right.Ref.AsLong());
                else if (right.TypeId == ElaMachine.DBL)
                    return new ElaValue(left.DirectGetReal() % right.Ref.AsDouble());
                else
                {
                    ctx.InvalidOperand(left, right, "remainder");
                    return Default();
                }
            }

            if (right.DirectGetReal() == 0)
            {
                ctx.DivideByZero(left);
                return Default();
            }

            return new ElaValue(left.DirectGetReal() % right.DirectGetReal());
        }
        
        internal override ElaValue Power(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.REA)
            {
                if (right.TypeId == ElaMachine.INT)
                    return new ElaValue(Math.Pow(left.DirectGetReal(), right.I4));
                else if (right.TypeId == ElaMachine.LNG)
                    return new ElaValue(Math.Pow(left.DirectGetReal(), right.Ref.AsLong()));
                else if (right.TypeId == ElaMachine.DBL)
                    return new ElaValue(Math.Pow(left.DirectGetReal(), right.Ref.AsDouble()));
                else
                {
                    ctx.InvalidOperand(left, right, "power");
                    return Default();
                }
            }

            return new ElaValue(Math.Pow(left.DirectGetReal(), right.DirectGetReal()));
        }
        
        internal override ElaValue Negate(ElaValue value, ExecutionContext ctx)
        {
            return new ElaValue(-value.DirectGetReal());
        }

        internal override bool Greater(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.REA)
            {
                if (right.TypeId == ElaMachine.INT)
                    return left.DirectGetReal() > right.I4;
                else if (right.TypeId == ElaMachine.LNG)
                    return left.DirectGetReal() > right.Ref.AsLong();
                else if (right.TypeId == ElaMachine.DBL)
                    return left.DirectGetReal() > right.Ref.AsDouble();
                else
                {
                    ctx.InvalidOperand(left, right, "greater");
                    return false;
                }
            }

            return left.DirectGetReal() > right.DirectGetReal();
        }

        internal override bool Lesser(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.REA)
            {
                if (right.TypeId == ElaMachine.INT)
                    return left.DirectGetReal() < right.I4;
                else if (right.TypeId == ElaMachine.LNG)
                    return left.DirectGetReal() < right.Ref.AsLong();
                else if (right.TypeId == ElaMachine.DBL)
                    return left.DirectGetReal() < right.Ref.AsDouble();
                else
                {
                    ctx.InvalidOperand(left, right, "lesser");
                    return false;
                }
            }

            return left.DirectGetReal() < right.DirectGetReal();
        }

        internal override bool GreaterEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.REA)
            {
                if (right.TypeId == ElaMachine.INT)
                    return left.DirectGetReal() >= right.I4;
                else if (right.TypeId == ElaMachine.LNG)
                    return left.DirectGetReal() >= right.Ref.AsLong();
                else if (right.TypeId == ElaMachine.DBL)
                    return left.DirectGetReal() >= right.Ref.AsDouble();
                else
                {
                    ctx.InvalidOperand(left, right, "greaterequal");
                    return false;
                }
            }

            return left.DirectGetReal() >= right.DirectGetReal();
        }

        internal override bool LesserEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.REA)
            {
                if (right.TypeId == ElaMachine.INT)
                    return left.DirectGetReal() <= right.I4;
                else if (right.TypeId == ElaMachine.LNG)
                    return left.DirectGetReal() <= right.Ref.AsLong();
                else if (right.TypeId == ElaMachine.DBL)
                    return left.DirectGetReal() <= right.Ref.AsDouble();
                else
                {
                    ctx.InvalidOperand(left, right, "lesserequal");
                    return false;
                }
            }

            return left.DirectGetReal() <= right.DirectGetReal();
        }

        internal override ElaValue Parse(ElaValue instance, string format, string value, ExecutionContext ctx)
        {
            var res = 0f;

            if (!Single.TryParse(value, NumberStyles.Any, Culture.NumberFormat, out res))
            {
                ctx.UnableRead(instance, value);
                return Default();
            }

            return new ElaValue(res);
        }
    }
}
