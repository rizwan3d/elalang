using System;
using System.Collections.Generic;
using System.Text;

namespace Ela.Runtime.ObjectModel
{
    public sealed class ElaBehaviorObject : ElaObject
    {
        #region Construction
        private Dictionary<String,ElaFunction> funs;
        private ElaTraits overridenTraits;
        private ElaValue wrappedObject;

        public ElaBehaviorObject(ElaValue value) : base(value.Traits)
        {
            funs = new Dictionary<String,ElaFunction>();
            this.wrappedObject = value;
        }
        #endregion


        #region Methods
        internal void AddFunction(string behavior, string funName, ElaFunction fun, ExecutionContext ctx)
        {
            var trait = Builtins.Trait(behavior);

            if (trait != ElaTraits.None)
                overridenTraits |= trait;

            funs.Add(behavior + " " + funName, fun);
        }


        internal ElaFunction GetBehaviorFunction(string funName, ExecutionContext ctx)
        {
            var fun = default(ElaFunction);

            if (!funs.TryGetValue(funName, out fun))
            {
                var idx = funName.IndexOf(' ');

                if (idx != -1)
                {
                    var b = funName.Substring(0, idx - 1);

                    if (!HasBehavior(b))
                    {
                        ctx.Fail(String.Format("Behavior '{0}' is not supported.", b));
                        return null;
                    }
                }
                else
                {
                    ctx.Fail(String.Format("Unknown behavior or function '{0}'.", funName));
                    return null;
                }

                ctx.Fail(String.Format("Function '{0}' is not implemented.", funName));
                return null;
            }

            return fun;
        }


        internal bool HasBehavior(string behavior)
        {
            var b = behavior + " ";

            foreach (var s in funs.Keys)
                if (s.StartsWith(b))
                    return true;

            return false;
        }


        private ElaValue Run(string funName, ExecutionContext ctx, params ElaValue[] args)
        {
            var fun = default(ElaFunction);

            if (funs.TryGetValue(funName, out fun))
            {
                try
                {
                    return fun.Call(args);
                }
                catch (ElaCodeException ex)
                {
                    ctx.Fail(ex.ErrorObject);
                    return Default();
                }
                catch (ElaRuntimeException ex)
                {
                    ctx.Fail(ex.Category, ex.Message);
                    return Default();
                }
                catch (Exception ex)
                {
                    ctx.Fail(ex.Message);
                    return Default();
                }
            }

            ctx.Fail(String.Format("Function '{0}' is not implemented.", funName));
            return Default();
        }
        #endregion

        
        #region Traits
        protected internal override ElaValue Equals(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Eq) == ElaTraits.Eq)
                return Run("Eq ==", ctx, left, right);

            return wrappedObject.Equals(left, right, ctx);
        }


        protected internal override ElaValue NotEquals(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Eq) == ElaTraits.Eq)
                return Run("Eq <>", ctx, left, right);

            return wrappedObject.NotEquals(left, right, ctx);
        }


        protected internal override ElaValue Greater(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Ord) == ElaTraits.Ord)
                return Run("Ord >", ctx, left, right);

            return wrappedObject.Greater(left, right, ctx);
        }


        protected internal override ElaValue Lesser(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Ord) == ElaTraits.Ord)
                return Run("Ord <", ctx, left, right);

            return wrappedObject.Lesser(left, right, ctx);
        }


        protected internal override ElaValue GreaterEquals(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Ord) == ElaTraits.Ord)
                return Run("Ord >=", ctx, left, right);

            return wrappedObject.GreaterEquals(left, right, ctx);
        }


        protected internal override ElaValue LesserEquals(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Ord) == ElaTraits.Ord)
                return Run("Ord <=", ctx, left, right);

            return wrappedObject.LesserEquals(left, right, ctx);
        }


        protected internal override ElaValue GetLength(ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Len) == ElaTraits.Len)
                return Run("Len length", ctx, wrappedObject);

            return wrappedObject.GetLength(ctx);
        }


        protected internal override ElaValue Successor(ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Enum) == ElaTraits.Enum)
                return Run("Enum succ", ctx, wrappedObject);

            return wrappedObject.Successor(ctx);
        }


        protected internal override ElaValue Predecessor(ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Enum) == ElaTraits.Enum)
                return Run("Enum pred", ctx, wrappedObject);

            return wrappedObject.Predecessor(ctx);
        }


        protected internal override ElaValue GetValue(ElaValue index, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Get) == ElaTraits.Get)
                return Run("Get get", ctx, index, wrappedObject);

            return wrappedObject.GetValue(index, ctx);
        }


        protected internal override void SetValue(ElaValue index, ElaValue value, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Set) == ElaTraits.Set)
                Run("Set set", ctx, index, value, wrappedObject);

            value.SetValue(index, value, ctx);
        }


        protected internal override ElaValue GetMax(ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Bound) == ElaTraits.Bound)
                return Run("Bound max", ctx, wrappedObject);

            return wrappedObject.GetMax(ctx);
        }


        protected internal override ElaValue GetMin(ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Bound) == ElaTraits.Bound)
                return Run("Bound min", ctx, wrappedObject);

            return wrappedObject.GetMin(ctx);
        }


        protected internal override ElaValue Concatenate(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Concat) == ElaTraits.Concat)
                return Run("Concat ++", ctx, left, right);

            return wrappedObject.Concatenate(left, right, ctx);
        }


        protected internal override ElaValue Add(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Num) == ElaTraits.Num)
                return Run("Num +", ctx, left, right);

            return wrappedObject.Add(left, right, ctx);
        }


        protected internal override ElaValue Subtract(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Num) == ElaTraits.Num)
                return Run("Num -", ctx, left, right);

            return wrappedObject.Subtract(left, right, ctx);
        }


        protected internal override ElaValue Multiply(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Num) == ElaTraits.Num)
                return Run("Num *", ctx, left, right);

            return wrappedObject.Multiply(left, right, ctx);
        }


        protected internal override ElaValue Divide(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Num) == ElaTraits.Num)
                return Run("Num /", ctx, left, right);

            return wrappedObject.Divide(left, right, ctx);
        }


        protected internal override ElaValue Remainder(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Num) == ElaTraits.Num)
                return Run("Num %", ctx, left, right);

            return wrappedObject.Remainder(left, right, ctx);
        }


        protected internal override ElaValue Power(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Num) == ElaTraits.Num)
                return Run("Num **", ctx, left, right);

            return wrappedObject.Power(left, right, ctx);
        }


        protected internal override ElaValue BitwiseOr(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Bit) == ElaTraits.Bit)
                return Run("Bit |||", ctx, left, right);

            return wrappedObject.BitwiseOr(left, right, ctx);
        }


        protected internal override ElaValue BitwiseAnd(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Bit) == ElaTraits.Bit)
                return Run("Bit &&&", ctx, left, right);

            return wrappedObject.BitwiseAnd(left, right, ctx);
        }


        protected internal override ElaValue BitwiseXor(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Bit) == ElaTraits.Bit)
                return Run("Bit ^^^", ctx, left, right);

            return wrappedObject.BitwiseXor(left, right, ctx);
        }


        protected internal override ElaValue BitwiseNot(ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Bit) == ElaTraits.Bit)
                return Run("Bit ^^^", ctx, wrappedObject);

            return wrappedObject.BitwiseNot(ctx);
        }


        protected internal override ElaValue ShiftRight(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Bit) == ElaTraits.Bit)
                return Run("Bit >>>", ctx, left, right);

            return wrappedObject.ShiftRight(left, right, ctx);
        }


        protected internal override ElaValue ShiftLeft(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Bit) == ElaTraits.Bit)
                return Run("Bit <<<", ctx, left, right);

            return wrappedObject.ShiftLeft(left, right, ctx);
        }


        protected internal override ElaValue Negate(ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Neg) == ElaTraits.Neg)
                return Run("Neg --", ctx, wrappedObject);

            return wrappedObject.Negate(ctx);
        }


        protected internal override bool Bool(ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Bool) == ElaTraits.Bool)
            {
                var ret = Run("Bool bool", ctx, wrappedObject);
                return ret.AsBoolean();
            }

            return wrappedObject.Bool(ctx);
        }


        protected internal override ElaValue Head(ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Seq) == ElaTraits.Seq)
                return Run("Seq head", ctx, wrappedObject);

            return wrappedObject.Head(ctx);
        }


        protected internal override ElaValue Tail(ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Seq) == ElaTraits.Seq)
                return Run("Seq tail", ctx, wrappedObject);

            return wrappedObject.Tail(ctx);
        }


        protected internal override bool IsNil(ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Seq) == ElaTraits.Seq)
            {
                var ret = Run("Seq isNil", ctx, wrappedObject);
                return ret.AsBoolean();
            }

            return wrappedObject.IsNil(ctx);
        }


        protected internal override ElaValue Cons(ElaObject instance, ElaValue value, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Cons) == ElaTraits.Cons)
                return Run("Cons ::", ctx, new ElaValue(instance), value, wrappedObject);

            return value.Cons(instance, value, ctx);
        }


        protected internal override ElaValue Nil(ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Cons) == ElaTraits.Cons)
                return Run("Cons nil", ctx, wrappedObject);

            return wrappedObject.Nil(ctx);
        }


        protected internal override ElaValue Generate(ElaValue value, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Gen) == ElaTraits.Gen)
                return Run("Gen gen", ctx, value, wrappedObject);

            return value.Generate(value, ctx);
        }


        protected internal override ElaValue GenerateFinalize(ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Gen) == ElaTraits.Gen)
                return Run("Gen genFin", ctx, wrappedObject);

            return wrappedObject.GenerateFinalize(ctx);
        }


        protected internal override ElaValue GetField(string field, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.FieldGet) == ElaTraits.FieldGet)
                return Run("FieldGet getField", ctx, new ElaValue(field), wrappedObject);

            return wrappedObject.GetField(field, ctx);
        }


        protected internal override bool HasField(string field, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.FieldGet) == ElaTraits.FieldGet)
            {
                var ret = Run("FieldGet hasField", ctx, new ElaValue(field), wrappedObject);
                return ret.AsBoolean();
            }

            return wrappedObject.HasField(field, ctx);
        }


        protected internal override void SetField(string field, ElaValue value, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.FieldSet) == ElaTraits.FieldSet)
                Run("FieldGet setField", ctx, new ElaValue(field), value, wrappedObject);

            value.SetField(field, value, ctx);
        }


        protected internal override string Show(ExecutionContext ctx, ShowInfo info)
        {
            if ((overridenTraits & ElaTraits.Show) == ElaTraits.Show)
            {
                var ret = Run("Show show", ctx, wrappedObject);
                return ret.AsString();
            }

            return wrappedObject.Show(ctx, info);
        }


        protected internal override ElaValue Convert(ElaTypeCode type, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Convert) == ElaTraits.Convert)
                return Run("Convert convert", ctx, new ElaValue((Int32)type), wrappedObject);

            return wrappedObject.Convert(type, ctx);
        }


        protected internal override ElaValue Call(ElaValue value, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Call) == ElaTraits.Call)
                return Run("Call call", ctx, value);

            return value.Call(value, ctx);
        }


        protected internal override ElaValue Force(ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Thunk) == ElaTraits.Thunk)
                return Run("Thunk force", ctx, wrappedObject);

            return wrappedObject.Force(ctx);
        }


        protected internal override string GetTag(ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Tag) == ElaTraits.Tag)
            {
                var ret = Run("Tag tag", ctx, wrappedObject);
                return ret.AsString();
            }

            return wrappedObject.GetTag(ctx);
        }


        protected internal override ElaValue Untag(ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Tag) == ElaTraits.Tag)
                return Run("Tag untag", ctx, wrappedObject);

            return wrappedObject.Untag(ctx);
        }
        #endregion
    }
}
