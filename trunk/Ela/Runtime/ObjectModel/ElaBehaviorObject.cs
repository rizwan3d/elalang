using System;
using System.Collections.Generic;
using System.Text;

namespace Ela.Runtime.ObjectModel
{
    public sealed class ElaBehaviorObject : ElaObject
    {
        #region Construction
        private Dictionary<String,ElaFunction> funs;

        public ElaBehaviorObject() : base(ElaTraits.None)
        {
            funs = new Dictionary<String,ElaFunction>();
        }
        #endregion


        #region Methods
        internal ElaFunction GetBehaviorFunction(string behavior, string funName, ExecutionContext ctx)
        {
            var fun = default(ElaFunction);

            if (!funs.TryGetValue(behavior + "." + funName, out fun))
            {
                return null;
            }

            return fun;
        }


        internal bool RunIf(ElaTraits trait, string funName, ElaValue left, ElaValue right, ExecutionContext ctx, out ElaValue res)
        {
            var fun = default(ElaFunction);

            if (funs.TryGetValue(trait.ToString() + "." + funName, out fun))
            {
                try
                {
                    res = fun.Call(left, right);
                }
                catch (Exception ex)
                {
                    ctx.Fail(ex.Message);
                    res = Default();
                    return true;
                }
            }
            
            res = Default();
            return false;
        }
        #endregion

        
        #region Traits
        protected internal override ElaValue Equals(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            ElaValue res;

            if (!RunIf(ElaTraits.Eq, "==", left, right, ctx, out res))
                return base.Equals(left, right, ctx);

            return res;
        }


        //protected internal virtual ElaValue NotEquals(ElaValue left, ElaValue right, ExecutionContext ctx)
        //{
        //    ctx.Fail(ElaRuntimeError.TraitEq, ThisToString(left, right, ctx), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
        //    return Default();
        //}


        //protected internal virtual ElaValue Greater(ElaValue left, ElaValue right, ExecutionContext ctx)
        //{
        //    ctx.Fail(ElaRuntimeError.TraitOrd, ThisToString(left, right, ctx), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
        //    return Default();
        //}


        //protected internal virtual ElaValue Lesser(ElaValue left, ElaValue right, ExecutionContext ctx)
        //{
        //    ctx.Fail(ElaRuntimeError.TraitOrd, ThisToString(left, right, ctx), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
        //    return Default();
        //}


        //protected internal virtual ElaValue GreaterEquals(ElaValue left, ElaValue right, ExecutionContext ctx)
        //{
        //    ctx.Fail(ElaRuntimeError.TraitOrd, ThisToString(left, right, ctx), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
        //    return Default();
        //}


        //protected internal virtual ElaValue LesserEquals(ElaValue left, ElaValue right, ExecutionContext ctx)
        //{
        //    ctx.Fail(ElaRuntimeError.TraitOrd, ThisToString(left, right, ctx), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
        //    return Default();
        //}


        //protected internal virtual ElaValue GetLength(ExecutionContext ctx)
        //{
        //    ctx.Fail(ElaRuntimeError.TraitLen, ToString(), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
        //    return Default();
        //}


        //protected internal virtual ElaValue Successor(ExecutionContext ctx)
        //{
        //    ctx.Fail(ElaRuntimeError.TraitEnum, ToString(), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
        //    return Default();
        //}


        //protected internal virtual ElaValue Predecessor(ExecutionContext ctx)
        //{
        //    ctx.Fail(ElaRuntimeError.TraitEnum, ToString(), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
        //    return Default();
        //}


        //protected internal virtual ElaValue GetValue(ElaValue index, ExecutionContext ctx)
        //{
        //    ctx.Fail(ElaRuntimeError.TraitGet, ToString(), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
        //    return Default();
        //}


        //protected internal virtual void SetValue(ElaValue index, ElaValue value, ExecutionContext ctx)
        //{
        //    ctx.Fail(ElaRuntimeError.TraitSet, ToString(), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
        //}


        //protected internal virtual ElaValue GetMax(ExecutionContext ctx)
        //{
        //    ctx.Fail(ElaRuntimeError.TraitBound, ToString(), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
        //    return Default();
        //}


        //protected internal virtual ElaValue GetMin(ExecutionContext ctx)
        //{
        //    ctx.Fail(ElaRuntimeError.TraitBound, ToString(), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
        //    return Default();
        //}


        //protected internal virtual ElaValue Concatenate(ElaValue left, ElaValue right, ExecutionContext ctx)
        //{
        //    ctx.Fail(ElaRuntimeError.TraitConcat, ThisToString(left, right, ctx), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
        //    return Default();
        //}


        //protected internal virtual ElaValue Add(ElaValue left, ElaValue right, ExecutionContext ctx)
        //{
        //    ctx.Fail(ElaRuntimeError.TraitNum, ThisToString(left, right, ctx), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
        //    return Default();
        //}


        //protected internal virtual ElaValue Subtract(ElaValue left, ElaValue right, ExecutionContext ctx)
        //{
        //    ctx.Fail(ElaRuntimeError.TraitNum, ThisToString(left, right, ctx), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
        //    return Default();
        //}


        //protected internal virtual ElaValue Multiply(ElaValue left, ElaValue right, ExecutionContext ctx)
        //{
        //    ctx.Fail(ElaRuntimeError.TraitNum, ThisToString(left, right, ctx), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
        //    return Default();
        //}


        //protected internal virtual ElaValue Divide(ElaValue left, ElaValue right, ExecutionContext ctx)
        //{
        //    ctx.Fail(ElaRuntimeError.TraitNum, ThisToString(left, right, ctx), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
        //    return Default();
        //}


        //protected internal virtual ElaValue Remainder(ElaValue left, ElaValue right, ExecutionContext ctx)
        //{
        //    ctx.Fail(ElaRuntimeError.TraitNum, ThisToString(left, right, ctx), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
        //    return Default();
        //}


        //protected internal virtual ElaValue Power(ElaValue left, ElaValue right, ExecutionContext ctx)
        //{
        //    ctx.Fail(ElaRuntimeError.TraitNum, ThisToString(left, right, ctx), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
        //    return Default();
        //}


        //protected internal virtual ElaValue BitwiseOr(ElaValue left, ElaValue right, ExecutionContext ctx)
        //{
        //    ctx.Fail(ElaRuntimeError.TraitBit, ThisToString(left, right, ctx), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
        //    return Default();
        //}


        //protected internal virtual ElaValue BitwiseAnd(ElaValue left, ElaValue right, ExecutionContext ctx)
        //{
        //    ctx.Fail(ElaRuntimeError.TraitBit, ThisToString(left, right, ctx), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
        //    return Default();
        //}


        //protected internal virtual ElaValue BitwiseXor(ElaValue left, ElaValue right, ExecutionContext ctx)
        //{
        //    ctx.Fail(ElaRuntimeError.TraitBit, ThisToString(left, right, ctx), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
        //    return Default();
        //}


        //protected internal virtual ElaValue BitwiseNot(ExecutionContext ctx)
        //{
        //    ctx.Fail(ElaRuntimeError.TraitBit, ToString(), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
        //    return Default();
        //}


        //protected internal virtual ElaValue ShiftRight(ElaValue left, ElaValue right, ExecutionContext ctx)
        //{
        //    ctx.Fail(ElaRuntimeError.TraitBit, ThisToString(left, right, ctx), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
        //    return Default();
        //}


        //protected internal virtual ElaValue ShiftLeft(ElaValue left, ElaValue right, ExecutionContext ctx)
        //{
        //    ctx.Fail(ElaRuntimeError.TraitBit, ThisToString(left, right, ctx), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
        //    return Default();
        //}


        //protected internal virtual ElaValue Negate(ExecutionContext ctx)
        //{
        //    ctx.Fail(ElaRuntimeError.TraitNeg, ToString(), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
        //    return Default();
        //}


        //protected internal virtual bool Bool(ExecutionContext ctx)
        //{
        //    ctx.Fail(ElaRuntimeError.TraitBool, ToString(), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
        //    return false;
        //}


        //protected internal virtual ElaValue Head(ExecutionContext ctx)
        //{
        //    ctx.Fail(ElaRuntimeError.TraitFold, ToString(), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
        //    return Default();
        //}


        //protected internal virtual ElaValue Tail(ExecutionContext ctx)
        //{
        //    ctx.Fail(ElaRuntimeError.TraitFold, ToString(), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
        //    return Default();
        //}


        //protected internal virtual bool IsNil(ExecutionContext ctx)
        //{
        //    ctx.Fail(ElaRuntimeError.TraitFold, ToString(), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
        //    return false;
        //}


        //protected internal virtual ElaValue Cons(ElaObject instance, ElaValue value, ExecutionContext ctx)
        //{
        //    ctx.Fail(ElaRuntimeError.TraitCons, ToString(), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
        //    return Default();
        //}


        //protected internal virtual ElaValue Nil(ExecutionContext ctx)
        //{
        //    ctx.Fail(ElaRuntimeError.TraitCons, ToString(), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
        //    return Default();
        //}


        //protected internal virtual ElaValue Generate(ElaValue value, ExecutionContext ctx)
        //{
        //    ctx.Fail(ElaRuntimeError.TraitGen, ToString(), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
        //    return Default();
        //}


        //protected internal virtual ElaValue GenerateFinalize(ExecutionContext ctx)
        //{
        //    ctx.Fail(ElaRuntimeError.TraitGen, ToString(), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
        //    return Default();
        //}


        //protected internal virtual ElaValue GetField(string field, ExecutionContext ctx)
        //{
        //    ctx.Fail(ElaRuntimeError.TraitFieldGet, ToString(), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
        //    return Default();
        //}


        //protected internal virtual void SetField(string field, ElaValue value, ExecutionContext ctx)
        //{
        //    ctx.Fail(ElaRuntimeError.TraitFieldSet, ToString(), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
        //}


        //protected internal virtual bool HasField(string field, ExecutionContext ctx)
        //{
        //    ctx.Fail(ElaRuntimeError.TraitFieldGet, ToString(), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
        //    return false;
        //}


        //protected internal virtual string Show(ExecutionContext ctx, ShowInfo info)
        //{
        //    ctx.Fail(ElaRuntimeError.TraitShow, String.Empty, TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
        //    return String.Empty;
        //}


        //protected internal virtual ElaValue Convert(ElaTypeCode type, ExecutionContext ctx)
        //{
        //    ctx.Fail(ElaRuntimeError.TraitConvert, ToString(), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
        //    return Default();
        //}


        //protected internal virtual ElaValue Call(ElaValue value, ExecutionContext ctx)
        //{
        //    ctx.Fail(ElaRuntimeError.TraitCall, ToString(), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
        //    return Default();
        //}


        //protected internal virtual ElaValue Force(ExecutionContext ctx)
        //{
        //    ctx.Fail(ElaRuntimeError.TraitThunk, ToString(), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
        //    return Default();
        //}


        //protected internal virtual string GetTag(ExecutionContext ctx)
        //{
        //    ctx.Fail(ElaRuntimeError.TraitTag, ToString(), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
        //    return String.Empty;
        //}


        //protected internal virtual ElaValue Untag(ExecutionContext ctx)
        //{
        //    ctx.Fail(ElaRuntimeError.TraitTag, ToString(), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
        //    return Default();
        //}


        //internal virtual ElaValue Successor(ElaValue @this, ExecutionContext ctx)
        //{
        //    return Successor(ctx);
        //}


        //internal virtual ElaValue Predecessor(ElaValue @this, ExecutionContext ctx)
        //{
        //    return Predecessor(ctx);
        //}


        //internal virtual ElaValue BitwiseNot(ElaValue @this, ExecutionContext ctx)
        //{
        //    return BitwiseNot(ctx);
        //}


        //internal virtual ElaValue Negate(ElaValue @this, ExecutionContext ctx)
        //{
        //    return Negate(ctx);
        //}


        //internal virtual bool Bool(ElaValue @this, ExecutionContext ctx)
        //{
        //    return Bool(ctx);
        //}


        //internal virtual string Show(ElaValue @this, ExecutionContext ctx, ShowInfo info)
        //{
        //    return Show(ctx, info);
        //}


        //internal virtual ElaValue Convert(ElaValue @this, ElaTypeCode type, ExecutionContext ctx)
        //{
        //    return Convert(type, ctx);
        //}
        #endregion
    }
}
