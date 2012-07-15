﻿using System;
using System.Globalization;
using Ela.Runtime.ObjectModel;

namespace Ela.Runtime.Classes
{
    internal sealed class IntegerInstance : Class
    {
        internal override ElaValue BitwiseNot(ElaValue @this, ExecutionContext ctx)
        {
            return new ElaValue(~@this.I4);
        }

        internal override ElaValue BitwiseAnd(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.INT)
            {
                if (right.TypeId == ElaMachine.LNG)
                    return new ElaValue(left.I4 & right.Ref.AsLong());
                else
                {
                    NoOverloadBinary(TCF.INT, right, "bitwiseand", ctx);
                    return Default();
                }
            }

            return new ElaValue(left.I4 & right.I4);
        }

        internal override ElaValue BitwiseOr(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.INT)
            {
                if (right.TypeId == ElaMachine.LNG)
                {
                    var lo = (Int64)left.I4;
                    return new ElaValue(lo | right.Ref.AsLong());
                }
                else
                {
                    NoOverloadBinary(TCF.INT, right, "bitwiseor", ctx);
                    return Default();
                }
            }

            return new ElaValue(left.I4 | right.I4);
        }

        internal override ElaValue BitwiseXor(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.INT)
            {
                if (right.TypeId == ElaMachine.LNG)
                    return new ElaValue(left.I4 ^ right.Ref.AsLong());
                else
                {
                    NoOverloadBinary(TCF.INT, right, "bitwisexor", ctx);
                    return Default();
                }
            }

            return new ElaValue(left.I4 ^ right.I4);
        }

        internal override ElaValue ShiftLeft(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.INT)
            {
                NoOverloadBinary(TCF.INT, right, "shiftleft", ctx);
                return Default();
            }

            return new ElaValue(left.I4 << right.I4);
        }

        internal override ElaValue ShiftRight(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.INT)
            {
                NoOverloadBinary(TCF.INT, right, "shiftright", ctx);
                return Default();
            }

            return new ElaValue(left.I4 >> right.I4);
        }

        internal override ElaValue Successor(ElaValue @this, ExecutionContext ctx)
        {
            return new ElaValue(@this.I4 + 1);
        }

        internal override ElaValue Predecessor(ElaValue @this, ExecutionContext ctx)
        {
            return new ElaValue(@this.I4 - 1);
        }

        internal override bool Equal(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.INT)
            {
                if (right.TypeId == ElaMachine.LNG)
                    return left.I4 == right.Ref.AsLong();
                else
                {
                    NoOverloadBinary(TCF.INT, right, "equal", ctx);
                    return false;
                }
            }

            return left.I4 == right.I4;
        }

        internal override bool NotEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.INT)
            {
                if (right.TypeId == ElaMachine.LNG)
                    return left.I4 != right.Ref.AsLong();
                else
                {
                    NoOverloadBinary(TCF.INT, right, "notequal", ctx);
                    return false;
                }
            }

            return left.I4 != right.I4;
        }

        internal override ElaValue Add(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.INT)
            {
                if (right.TypeId == ElaMachine.LNG)
                    return new ElaValue(left.I4 + right.Ref.AsLong());
                else
                {
                    NoOverloadBinary(TCF.INT, right, "add", ctx);
                    return Default();
                }
            }

            return new ElaValue(left.I4 + right.I4);
        }

        internal override ElaValue Subtract(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.INT)
            {
                if (right.TypeId == ElaMachine.LNG)
                    return new ElaValue(left.I4 - right.Ref.AsLong());
                else
                {
                    NoOverloadBinary(TCF.INT, right, "subtract", ctx);
                    return Default();
                }
            }

            return new ElaValue(left.I4 - right.I4);
        }

        internal override ElaValue Multiply(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.INT)
            {
                if (right.TypeId == ElaMachine.LNG)
                    return new ElaValue(left.I4 * right.Ref.AsLong());
                else
                {
                    NoOverloadBinary(TCF.INT, right, "multiply", ctx);
                    return Default();
                }
            }

            return new ElaValue(left.I4 * right.I4);
        }

        internal override ElaValue Remainder(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.INT)
            {
                if (right.TypeId == ElaMachine.LNG)
                {
                    var r = right.Ref.AsLong();

                    if (r == 0)
                    {
                        ctx.DivideByZero(left);
                        return Default();
                    }

                    return new ElaValue(left.I4 % r);
                }
                else
                {
                    NoOverloadBinary(TCF.INT, right, "remainder", ctx);
                    return Default();
                }
            }

            if (right.I4 == 0)
            {
                ctx.DivideByZero(left);
                return Default();
            }

            return new ElaValue(left.I4 % right.I4);
        }

        internal static ElaValue Modulus(int x, int y, ExecutionContext ctx)
        {
            if (y == 0)
            {
                ctx.DivideByZero(new ElaValue(x));
                return Default();
            }

            var r = x % y;
            return x < 0 && y > 0 || x > 0 && y < 0 ? new ElaValue(r + y) : new ElaValue(r);
        }

        internal override ElaValue Modulus(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.INT)
            {
                if (right.TypeId == ElaMachine.LNG)
                    return LongInstance.Modulus(left.I4, right.Ref.AsLong(), ctx);
                else
                {
                    NoOverloadBinary(TCF.INT, right, "modulus", ctx);
                    return Default();
                }
            }

            return Modulus(left.I4, right.I4, ctx);
        }
        
        internal override ElaValue Power(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.INT)
            {
                if (right.TypeId == ElaMachine.LNG)
                    return new ElaValue(Math.Pow(left.I4, right.Ref.AsLong()));
                else
                {
                    NoOverloadBinary(TCF.INT, right, "power", ctx);
                    return Default();
                }
            }

            return new ElaValue(Math.Pow(left.I4, right.I4));
        }

        internal override ElaValue Negate(ElaValue value, ExecutionContext ctx)
        {
            return new ElaValue(-value.I4);
        }

        internal override bool Greater(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.INT)
            {
                if (right.TypeId == ElaMachine.LNG)
                    return left.I4 > right.Ref.AsLong();
                else
                {
                    NoOverloadBinary(TCF.INT, right, "greater", ctx);
                    return false;
                }
            }

            return left.I4 > right.I4;
        }

        internal override bool Lesser(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.INT)
            {
                if (right.TypeId == ElaMachine.LNG)
                    return left.I4 < right.Ref.AsLong();
                else
                {
                    NoOverloadBinary(TCF.INT, right, "lesser", ctx);
                    return false;
                }
            }

            return left.I4 < right.I4;
        }

        internal override bool GreaterEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.INT)
            {
                if (right.TypeId == ElaMachine.LNG)
                    return left.I4 >= right.Ref.AsLong();
                else
                {
                    NoOverloadBinary(TCF.INT, right, "greaterequal", ctx);
                    return false;
                }
            }

            return left.I4 >= right.I4;
        }

        internal override bool LesserEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.INT)
            {
                if (right.TypeId == ElaMachine.LNG)
                    return left.I4 <= right.Ref.AsLong();
                else
                {
                    NoOverloadBinary(TCF.INT, right, "lesserequal", ctx);
                    return false;
                }
            }

            return left.I4 <= right.I4;
        }
    }
}
