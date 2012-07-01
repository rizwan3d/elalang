using System;
using Ela.Runtime.ObjectModel;
using Ela.CodeModel;

namespace Ela.Runtime.Classes
{
    public class Class
    {
        private ElaFunction read;

        private ElaFunction or;
        private ElaFunction and;
        private ElaFunction xor;
        private ElaFunction not;
        private ElaFunction shr;
        private ElaFunction shl;
        private ElaFunction cat;
        private ElaFunction succ;
        private ElaFunction pred;
        private ElaFunction eq;
        private ElaFunction neq;
        private ElaFunction len;
        private ElaFunction get;
        private ElaFunction add;
        private ElaFunction sub;
        private ElaFunction mul;
        private ElaFunction div;
        private ElaFunction rem;
        private ElaFunction pow;
        private ElaFunction neg;
        private ElaFunction gt;
        private ElaFunction lt;
        private ElaFunction gteq;
        private ElaFunction lteq;
        private ElaFunction head;
        private ElaFunction tail;
        private ElaFunction isnil;
        private ElaFunction show;
        
        internal Class()
        {

        }

        internal virtual ElaValue BitwiseOr(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (or != null)
            {
                ctx.SetDeffered(or, 2);
                return Default();
            }

            ctx.NoOverload(left.GetTypeName(), "bitwiseor");
            return Default();
        }

        internal virtual ElaValue BitwiseAnd(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (and != null)
            {
                ctx.SetDeffered(and, 2);
                return Default();
            }

            ctx.NoOverload(left.GetTypeName(), "bitwiseand");
            return Default();
        }

        internal virtual ElaValue BitwiseXor(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (xor != null)
            {
                ctx.SetDeffered(xor, 2);
                return Default();
            }

            ctx.NoOverload(left.GetTypeName(), "bitwisexor");
            return Default();
        }

        internal virtual ElaValue BitwiseNot(ElaValue @this, ExecutionContext ctx)
        {
            if (not != null)
            {
                ctx.SetDeffered(not, 1);
                return Default();
            }

            ctx.NoOverload(@this.GetTypeName(), "bitwisenot");
            return Default();
        }

        internal virtual ElaValue ShiftRight(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (shr != null)
            {
                ctx.SetDeffered(shr, 2);
                return Default();
            }

            ctx.NoOverload(left.GetTypeName(), "shiftright");
            return Default();
        }

        internal virtual ElaValue ShiftLeft(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (shl != null)
            {
                ctx.SetDeffered(shl, 2);
                return Default();
            }

            ctx.NoOverload(left.GetTypeName(), "shiftleft");
            return Default();
        }

        internal virtual ElaValue Concatenate(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (cat != null)
            {
                ctx.SetDeffered(cat, 2);
                return Default();
            }

            ctx.NoOverload(left.GetTypeName(), "concatenate");
            return Default();
        }

        internal virtual ElaValue Successor(ElaValue @this, ExecutionContext ctx)
        {
            if (succ != null)
            {
                ctx.SetDeffered(succ, 1);
                return Default();
            }

            ctx.NoOverload(@this.GetTypeName(), "succ");
            return Default();
        }

        internal virtual ElaValue Predecessor(ElaValue @this, ExecutionContext ctx)
        {
            if (pred != null)
            {
                ctx.SetDeffered(pred, 1);
                return Default();
            }

            ctx.NoOverload(@this.GetTypeName(), "pred");
            return Default();
        }

        internal virtual bool Equal(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (eq != null)
            {
                ctx.SetDeffered(eq, 2);
                return false;
            }

            return left.Ref == right.Ref;
        }

        internal virtual bool NotEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (neq != null)
            {
                ctx.SetDeffered(neq, 2);
                return false;
            }

            return left.Ref != right.Ref;
        }

        internal virtual ElaValue GetLength(ElaValue value, ExecutionContext ctx)
        {
            if (len != null)
            {
                ctx.SetDeffered(len, 1);
                return Default();
            }

            ctx.NoOverload(value.GetTypeName(), "length");
            return Default();
        }

        internal virtual ElaValue GetValue(ElaValue value, ElaValue index, ExecutionContext ctx)
        {
            if (get != null)
            {
                ctx.SetDeffered(get, 2);
                return Default();
            }

            ctx.NoOverload(value.GetTypeName(), "get");
            return Default();
        }

        internal virtual ElaValue Add(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (add != null)
            {
                ctx.SetDeffered(add, 2);
                return Default();
            }

            ctx.NoOverload(left.GetTypeName(), "add");
            return Default();
        }

        internal virtual ElaValue Subtract(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (sub != null)
            {
                ctx.SetDeffered(sub, 2);
                return Default();
            }

            ctx.NoOverload(left.GetTypeName(), "subtract");
            return Default();
        }

        internal virtual ElaValue Multiply(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (mul != null)
            {
                ctx.SetDeffered(mul, 2);
                return Default();
            }

            ctx.NoOverload(left.GetTypeName(), "multiply");
            return Default();
        }

        internal virtual ElaValue Divide(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (div != null)
            {
                ctx.SetDeffered(div, 2);
                return Default();
            }

            ctx.NoOverload(left.GetTypeName(), "divide");
            return Default();
        }

        internal virtual ElaValue Remainder(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (rem != null)
            {
                ctx.SetDeffered(rem, 2);
                return Default();
            }

            ctx.NoOverload(left.GetTypeName(), "remainder");
            return Default();
        }

        internal virtual ElaValue Power(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (pow != null)
            {
                ctx.SetDeffered(pow, 2);
                return Default();
            }

            ctx.NoOverload(left.GetTypeName(), "power");
            return Default();
        }

        internal virtual ElaValue Negate(ElaValue @this, ExecutionContext ctx)
        {
            if (neg != null)
            {
                ctx.SetDeffered(neg, 1);
                return Default();
            }

            ctx.NoOverload(@this.GetTypeName(), "negate");
            return Default();
        }

        internal virtual bool Greater(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (gt != null)
            {
                ctx.SetDeffered(gt, 2);
                return false;
            }

            ctx.NoOverload(left.GetTypeName(), "greater");
            return false;
        }

        internal virtual bool Lesser(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (lt != null)
            {
                ctx.SetDeffered(lt, 2);
                return false;
            }

            ctx.NoOverload(left.GetTypeName(), "lesser");
            return false;
        }

        internal virtual bool GreaterEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (gteq != null)
            {
                ctx.SetDeffered(gteq, 2);
                return false;
            }

            ctx.NoOverload(left.GetTypeName(), "greaterequal");
            return false;
        }

        internal virtual bool LesserEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (lteq != null)
            {
                ctx.SetDeffered(lteq, 2);
                return false;
            }

            ctx.NoOverload(left.GetTypeName(), "lesserequal");
            return false;
        }

        internal virtual ElaValue Parse(ElaValue instance, string format, string value, ExecutionContext ctx)
        {
            if (read != null)
            {
                ctx.SetDeffered(read, 2);
                return Default();
            }

            ctx.NoOverload(instance.GetTypeName(), "read");
            return Default();
        }

        internal virtual ElaValue Head(ElaValue left, ExecutionContext ctx)
        {
            if (head != null)
            {
                ctx.SetDeffered(head, 1);
                return Default();
            }

            ctx.NoOverload(left.GetTypeName(), "head");
            return Default();
        }

        internal virtual ElaValue Tail(ElaValue left, ExecutionContext ctx)
        {
            if (tail != null)
            {
                ctx.SetDeffered(tail, 1);
                return Default();
            }

            ctx.NoOverload(left.GetTypeName(), "tail");
            return Default();
        }

        internal virtual bool IsNil(ElaValue left, ExecutionContext ctx)
        {
            if (isnil != null)
            {
                ctx.SetDeffered(isnil, 1);
                return false;
            }

            ctx.NoOverload(left.GetTypeName(), "isnil");
            return false;
        }

        internal virtual string Showf(string format, ElaValue value, ExecutionContext ctx)
        {
            if (show != null)
            {
                ctx.SetDeffered(show, 2);
                return String.Empty;
            }

            if (value.Ref is ElaUserType)
            {
                return ((ElaUserType)value.Ref).ToString(format, Culture.NumberFormat);
            }

            try
            {
                return value.ToString(format, Culture.NumberFormat);
            }
            catch (Exception)
            {
                ctx.InvalidFormat(format, value);
                return String.Empty;
            }
        }

        protected ElaValue Default()
        {
            return new ElaValue(ElaObject.ElaInvalidObject.Instance);
        }

        internal void AddFunction(ElaBuiltinKind builtin, ElaFunction fun)
        {
            switch (builtin)
            {
                case ElaBuiltinKind.BitwiseAnd:
                    and = fun;
                    break;
                case ElaBuiltinKind.BitwiseOr:
                    or = fun;
                    break;
                case ElaBuiltinKind.BitwiseXor:
                    xor = fun;
                    break;
                case ElaBuiltinKind.BitwiseNot:
                    not = fun;
                    break;
                case ElaBuiltinKind.ShiftLeft:
                    shl = fun;
                    break;
                case ElaBuiltinKind.ShiftRight:
                    shr = fun;
                    break;
                case ElaBuiltinKind.Concat:
                    cat = fun;
                    break;
                case ElaBuiltinKind.Succ:
                    succ = fun;
                    break;
                case ElaBuiltinKind.Pred:
                    pred = fun;
                    break;
                case ElaBuiltinKind.Equal:
                    eq = fun;
                    break;
                case ElaBuiltinKind.NotEqual:
                    neq = fun;
                    break;
                case ElaBuiltinKind.Length:
                    len = fun;
                    break;
                case ElaBuiltinKind.Get:
                    get = fun;
                    break;
                case ElaBuiltinKind.Add:
                    add = fun;
                    break;
                case ElaBuiltinKind.Subtract:
                    sub = fun;
                    break;
                case ElaBuiltinKind.Multiply:
                    mul = fun;
                    break;
                case ElaBuiltinKind.Divide:
                    div = fun;
                    break;
                case ElaBuiltinKind.Power:
                    pow = fun;
                    break;
                case ElaBuiltinKind.Remainder:
                    rem = fun;
                    break;
                case ElaBuiltinKind.Negate:
                    neg = fun;
                    break;
                case ElaBuiltinKind.Greater:
                    gt = fun;
                    break;
                case ElaBuiltinKind.Lesser:
                    lt = fun;
                    break;
                case ElaBuiltinKind.LesserEqual:
                    lteq = fun;
                    break;
                case ElaBuiltinKind.GreaterEqual:
                    gteq = fun;
                    break;
                case ElaBuiltinKind.Head:
                    head = fun;
                    break;
                case ElaBuiltinKind.Tail:
                    tail = fun;
                    break;
                case ElaBuiltinKind.IsNil:
                    isnil = fun;
                    break;
                case ElaBuiltinKind.Showf:
                    show = fun;
                    break;
                
                case ElaBuiltinKind.Readf:
                    read = fun;
                    break;
            }            
        }
    }
}
