using System;
using System.Collections.Generic;
using System.Text;

namespace Ela.Runtime.ObjectModel
{
    public sealed class ElaTraitObject : ElaObject
    {
        #region Construction
        private Dictionary<String,ElaFunction> funs;
        private ElaTraits overridenTraits;
        private ElaValue wrappedObject;

		public ElaTraitObject(ElaValue value) : base((ElaTypeCode)ElaMachine.PRX, value.Traits)
        {
            funs = new Dictionary<String,ElaFunction>();
            this.wrappedObject = value;
        }
        #endregion


        #region Methods
        internal void AddFunction(string trait, string funName, ElaFunction fun, ExecutionContext ctx)
        {
            var t = Builtins.Trait(trait);

            if (t != ElaTraits.None)
                overridenTraits |= t;

            funs.Add(trait + " " + funName, fun);
        }


        internal ElaFunction GetTraitFunction(string funName, ExecutionContext ctx)
        {
            var fun = default(ElaFunction);

            if (!funs.TryGetValue(funName, out fun))
            {
                var idx = funName.IndexOf(' ');

                if (idx != -1)
                {
                    var b = funName.Substring(0, idx);

                    if (!HasTrait(b))
                    {
                        ctx.Fail(String.Format("Trait '{0}' is not supported.", b));
                        return null;
                    }
                }
                else
                {
                    ctx.Fail(String.Format("Unknown trait or function '{0}'.", funName));
                    return null;
                }

                ctx.Fail(String.Format("Function '{0}' is not implemented.", funName));
                return null;
            }

            return fun;
        }


        internal bool HasTrait(string trait)
        {
            var b = trait + " ";

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
                return Run("Eq equals", ctx, Self(left), Self(right));

            return wrappedObject.Ref.Equals(Self(left), Self(right), ctx);
        }


        protected internal override ElaValue NotEquals(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Eq) == ElaTraits.Eq)
                return Run("Eq notequals", ctx, Self(left), Self(right));

            return wrappedObject.Ref.NotEquals(Self(left), Self(right), ctx);
        }


        protected internal override ElaValue Greater(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Ord) == ElaTraits.Ord)
                return Run("Ord greater", ctx, Self(left), Self(right));

            return wrappedObject.Ref.Greater(Self(left), Self(right), ctx);
        }


        protected internal override ElaValue Lesser(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Ord) == ElaTraits.Ord)
                return Run("Ord lesser", ctx, Self(left), Self(right));

            return wrappedObject.Ref.Lesser(Self(left), Self(right), ctx);
        }


        protected internal override ElaValue GreaterEquals(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Ord) == ElaTraits.Ord)
                return Run("Ord greaterequal", ctx, Self(left), Self(right));

            return wrappedObject.Ref.GreaterEquals(Self(left), Self(right), ctx);
        }


        protected internal override ElaValue LesserEquals(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Ord) == ElaTraits.Ord)
                return Run("Ord lesserequal", ctx, Self(left), Self(right));

            return wrappedObject.Ref.LesserEquals(Self(left), Self(right), ctx);
        }


        protected internal override ElaValue GetLength(ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Len) == ElaTraits.Len)
                return Run("Len length", ctx, wrappedObject);

            return wrappedObject.Ref.GetLength(ctx);
        }


        protected internal override ElaValue Successor(ElaValue @this, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Enum) == ElaTraits.Enum)
                return Run("Enum succ", ctx, wrappedObject);

            return wrappedObject.Ref.Successor(wrappedObject, ctx);
        }


        protected internal override ElaValue Predecessor(ElaValue @this, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Enum) == ElaTraits.Enum)
                return Run("Enum pred", ctx, wrappedObject);

            return wrappedObject.Ref.Predecessor(wrappedObject, ctx);
        }


        protected internal override ElaValue GetValue(ElaValue index, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Get) == ElaTraits.Get)
                return Run("Get get", ctx, index, wrappedObject);

            return wrappedObject.Ref.GetValue(index, ctx);
        }


        protected internal override void SetValue(ElaValue index, ElaValue value, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Set) == ElaTraits.Set)
                Run("Set set", ctx, index, value, wrappedObject);

            wrappedObject.Ref.SetValue(index, value, ctx);
        }


        protected internal override ElaValue GetMax(ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Bound) == ElaTraits.Bound)
                return Run("Bound max", ctx, wrappedObject);

            return wrappedObject.Ref.GetMax(ctx);
        }


        protected internal override ElaValue GetMin(ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Bound) == ElaTraits.Bound)
                return Run("Bound min", ctx, wrappedObject);

            return wrappedObject.Ref.GetMin(ctx);
        }


        protected internal override ElaValue Concatenate(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Concat) == ElaTraits.Concat)
                return Run("Concat concat", ctx, Self(left), Self(right));

            return wrappedObject.Ref.Concatenate(Self(left), Self(right), ctx);
        }


        protected internal override ElaValue Add(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Num) == ElaTraits.Num)
                return Run("Num add", ctx, Self(left), Self(right));

            return wrappedObject.Ref.Add(Self(left), Self(right), ctx);
        }


        protected internal override ElaValue Subtract(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Num) == ElaTraits.Num)
                return Run("Num subtract", ctx, Self(left), Self(right));

            return wrappedObject.Ref.Subtract(Self(left), Self(right), ctx);
        }


        protected internal override ElaValue Multiply(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Num) == ElaTraits.Num)
                return Run("Num multiply", ctx, Self(left), Self(right));

            return wrappedObject.Ref.Multiply(Self(left), Self(right), ctx);
        }


        protected internal override ElaValue Divide(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Num) == ElaTraits.Num)
                return Run("Num divide", ctx, Self(left), Self(right));

            return wrappedObject.Ref.Divide(Self(left), Self(right), ctx);
        }


        protected internal override ElaValue Remainder(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Num) == ElaTraits.Num)
                return Run("Num remainder", ctx, Self(left), Self(right));

            return wrappedObject.Ref.Remainder(Self(left), Self(right), ctx);
        }


        protected internal override ElaValue Power(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Num) == ElaTraits.Num)
                return Run("Num power", ctx, Self(left), Self(right));

            return wrappedObject.Ref.Power(Self(left), Self(right), ctx);
        }


        protected internal override ElaValue BitwiseOr(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Bit) == ElaTraits.Bit)
                return Run("Bit bitwiseor", ctx, Self(left), Self(right));

            return wrappedObject.Ref.BitwiseOr(Self(left), Self(right), ctx);
        }


        protected internal override ElaValue BitwiseAnd(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Bit) == ElaTraits.Bit)
                return Run("Bit bitwiseand", ctx, Self(left), Self(right));

            return wrappedObject.Ref.BitwiseAnd(Self(left), Self(right), ctx);
        }


        protected internal override ElaValue BitwiseXor(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Bit) == ElaTraits.Bit)
                return Run("Bit bitwisexor", ctx, Self(left), Self(right));

            return wrappedObject.Ref.BitwiseXor(Self(left), Self(right), ctx);
        }


        protected internal override ElaValue BitwiseNot(ElaValue @this, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Bit) == ElaTraits.Bit)
                return Run("Bit bitwisenot", ctx, wrappedObject);

            return wrappedObject.Ref.BitwiseNot(wrappedObject, ctx);
        }


        protected internal override ElaValue ShiftRight(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Bit) == ElaTraits.Bit)
                return Run("Bit shiftright", ctx, Self(left), Self(right));

            return wrappedObject.Ref.ShiftRight(Self(left), Self(right), ctx);
        }


        protected internal override ElaValue ShiftLeft(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Bit) == ElaTraits.Bit)
                return Run("Bit shiftleft", ctx, Self(left), Self(right));

            return wrappedObject.Ref.ShiftLeft(Self(left), Self(right), ctx);
        }


        protected internal override ElaValue Negate(ElaValue @this, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Neg) == ElaTraits.Neg)
                return Run("Neg negate", ctx, wrappedObject);

            return wrappedObject.Ref.Negate(wrappedObject, ctx);
        }


        protected internal override bool Bool(ElaValue @this, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Bool) == ElaTraits.Bool)
            {
                var ret = Run("Bool bool", ctx, wrappedObject);
                return ret.AsBoolean();
            }

            return wrappedObject.Ref.Bool(wrappedObject, ctx);
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

            return wrappedObject.Ref.Tail(ctx);
        }


        protected internal override bool IsNil(ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Seq) == ElaTraits.Seq)
            {
                var ret = Run("Seq isNil", ctx, wrappedObject);
                return ret.AsBoolean();
            }

            return wrappedObject.Ref.IsNil(ctx);
        }


        protected internal override ElaValue Cons(ElaObject instance, ElaValue value, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Cons) == ElaTraits.Cons)
                return Run("Cons cons", ctx, new ElaValue(instance), value, wrappedObject);

            return wrappedObject.Ref.Cons(instance, value, ctx);
        }


        protected internal override ElaValue Nil(ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Cons) == ElaTraits.Cons)
                return Run("Cons nil", ctx, wrappedObject);

            return wrappedObject.Ref.Nil(ctx);
        }


        protected internal override ElaValue Generate(ElaValue value, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Gen) == ElaTraits.Gen)
                return Run("Gen gen", ctx, value, wrappedObject);

            return wrappedObject.Ref.Generate(value, ctx);
        }


        protected internal override ElaValue GenerateFinalize(ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Gen) == ElaTraits.Gen)
                return Run("Gen genFin", ctx, wrappedObject);

            return wrappedObject.Ref.GenerateFinalize(ctx);
        }


        protected internal override ElaValue GetField(string field, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.FieldGet) == ElaTraits.FieldGet)
                return Run("FieldGet getField", ctx, new ElaValue(field), wrappedObject);

            return wrappedObject.Ref.GetField(field, ctx);
        }


        protected internal override bool HasField(string field, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.FieldGet) == ElaTraits.FieldGet)
            {
                var ret = Run("FieldGet hasField", ctx, new ElaValue(field), wrappedObject);
                return ret.AsBoolean();
            }

            return wrappedObject.Ref.HasField(field, ctx);
        }


        protected internal override void SetField(string field, ElaValue value, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.FieldSet) == ElaTraits.FieldSet)
                Run("FieldGet setField", ctx, new ElaValue(field), value, wrappedObject);

            wrappedObject.Ref.SetField(field, value, ctx);
        }


        protected internal override string Show(ElaValue @this, ShowInfo info, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Show) == ElaTraits.Show)
            {
                var ret = Run("Show show", ctx, wrappedObject);
                return ret.AsString();
            }

            return wrappedObject.Ref.Show(wrappedObject, info, ctx);
        }


        protected internal override ElaValue Convert(ElaValue @this, ElaTypeCode type, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Convert) == ElaTraits.Convert)
                return Run("Convert convert", ctx, new ElaValue((Int32)type), wrappedObject);

            return wrappedObject.Ref.Convert(wrappedObject, type, ctx);
        }


        protected internal override ElaValue Call(ElaValue value, ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Call) == ElaTraits.Call)
                return Run("Call call", ctx, value, wrappedObject);

            return wrappedObject.Ref.Call(value, ctx);
        }


        protected internal override ElaValue Force(ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Thunk) == ElaTraits.Thunk)
                return Run("Thunk force", ctx, wrappedObject);

            return wrappedObject.Ref.Force(ctx);
        }


        protected internal override string GetTag(ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Tag) == ElaTraits.Tag)
            {
                var ret = Run("Tag tag", ctx, wrappedObject);
                return ret.AsString();
            }

            return wrappedObject.Ref.GetTag(ctx);
        }


        protected internal override ElaValue Untag(ExecutionContext ctx)
        {
            if ((overridenTraits & ElaTraits.Tag) == ElaTraits.Tag)
                return Run("Tag untag", ctx, wrappedObject);

            return wrappedObject.Ref.Untag(ctx);
        }


        private ElaValue Self(ElaValue val)
        {
            return val.Ref == this ? wrappedObject : val;
        }
        #endregion


		#region Properties
		public ElaValue Value
		{
			get { return wrappedObject; }
		}
		#endregion
	}
}
