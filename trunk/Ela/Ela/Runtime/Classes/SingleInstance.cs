﻿using System;
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
                    NoOverloadBinary(TCF.SINGLE, right, "equal", ctx);
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
                    NoOverloadBinary(TCF.SINGLE, right, "notequal", ctx);
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
                    NoOverloadBinary(TCF.SINGLE, right, "add", ctx);
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
                    NoOverloadBinary(TCF.SINGLE, right, "subtract", ctx);
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
                    NoOverloadBinary(TCF.SINGLE, right, "multiply", ctx);
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
                    NoOverloadBinary(TCF.SINGLE, right, "divide", ctx);
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
                    NoOverloadBinary(TCF.SINGLE, right, "remainder", ctx);
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

        internal static ElaValue Modulus(float x, float y, ExecutionContext ctx)
        {
            var r = x % y;
            return x < 0 && y > 0 || x > 0 && y < 0 ? new ElaValue(r + y) : new ElaValue(r);
        }

        internal override ElaValue Modulus(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.REA)
            {
                if (right.TypeId == ElaMachine.INT)
                    return Modulus(left.DirectGetReal(), right.I4, ctx);
                else if (right.TypeId == ElaMachine.LNG)
                    return Modulus(left.DirectGetReal(), right.Ref.AsLong(), ctx);
                else if (right.TypeId == ElaMachine.DBL)
                    return DoubleInstance.Modulus(left.DirectGetReal(), right.Ref.AsDouble(), ctx);
                else
                {
                    NoOverloadBinary(TCF.SINGLE, right, "modulus", ctx);
                    return Default();
                }
            }

            return Modulus(left.DirectGetReal(), right.DirectGetReal(), ctx);
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
                    NoOverloadBinary(TCF.SINGLE, right, "power", ctx);
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
                    NoOverloadBinary(TCF.SINGLE, right, "greater", ctx);
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
                    NoOverloadBinary(TCF.SINGLE, right, "lesser", ctx);
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
                    NoOverloadBinary(TCF.SINGLE, right, "greaterequal", ctx);
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
                    NoOverloadBinary(TCF.SINGLE, right, "lesserequal", ctx);
                    return false;
                }
            }

            return left.DirectGetReal() <= right.DirectGetReal();
        }
    }
}
