using System;
using System.Collections.Generic;

namespace Ela.Runtime.ObjectModel
{
	public abstract class ElaObject
	{
		#region Construction
		internal static readonly ExecutionContext DummyContext = new ExecutionContext();
		internal const string UNDEF = "<unknown>";
        private const string TYPENAME = "typeName";
        private const string TYPECODE = "typeCode";
        private const string ISBYREF = "byRef";

		protected ElaObject(ElaTraits traits) : this(ElaTypeCode.Object, traits)
		{
			
		}

		
		internal ElaObject(ElaTypeCode type, ElaTraits traits)
		{
			TypeId = (Int32)type;
			Traits = traits;
		} 
		#endregion


		#region Methods
		public virtual ElaTypeInfo GetTypeInfo()
		{
            var info = new ElaTypeInfo();
            info.AddField(TYPENAME, GetTypeName());
            info.AddField(TYPECODE, TypeId);
            info.AddField(ISBYREF, 
                TypeId == ElaMachine.INT ||
                TypeId == ElaMachine.CHR ||
                TypeId == ElaMachine.REA ||
                TypeId == ElaMachine.BYT);
            return info;
		}


		internal protected virtual int Compare(ElaValue @this, ElaValue other)
		{
			return @this.Ref == other.Ref ? 0 : -1;
		}


		public override string ToString()
		{
			if (TypeId == ElaMachine.INT ||
				TypeId == ElaMachine.REA ||
				TypeId == ElaMachine.CHR ||
				TypeId == ElaMachine.BYT)
				return UNDEF;
			else
				return Show(DummyContext, ShowInfo.Debug);
		}


		protected ElaValue Default()
		{
			return new ElaValue(ElaUnit.Instance);
		}


		protected internal virtual string GetTypeName()
		{
			return TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId);
		}


		private string ThisToString(ElaValue first, ElaValue second, ExecutionContext ctx)
		{
			return first.Ref == this ? first.Ref.Show(first, ctx, ShowInfo.Default) :
				second.Ref.Show(second, ctx, ShowInfo.Default);
		}
		#endregion


		#region Traits
		protected internal virtual ElaValue Equals(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			ctx.Fail(ElaRuntimeError.TraitEq, ThisToString(left, right, ctx), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
			return Default();
		}


		protected internal virtual ElaValue NotEquals(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
            ctx.Fail(ElaRuntimeError.TraitEq, ThisToString(left, right, ctx), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
			return Default();
		}


		protected internal virtual ElaValue Greater(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
            ctx.Fail(ElaRuntimeError.TraitOrd, ThisToString(left, right, ctx), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
			return Default();
		}


		protected internal virtual ElaValue Lesser(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
            ctx.Fail(ElaRuntimeError.TraitOrd, ThisToString(left, right, ctx), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
			return Default();
		}


		protected internal virtual ElaValue GreaterEquals(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
            ctx.Fail(ElaRuntimeError.TraitOrd, ThisToString(left, right, ctx), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
			return Default();
		}


		protected internal virtual ElaValue LesserEquals(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
            ctx.Fail(ElaRuntimeError.TraitOrd, ThisToString(left, right, ctx), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
			return Default();
		}


		protected internal virtual ElaValue GetLength(ExecutionContext ctx)
		{
            ctx.Fail(ElaRuntimeError.TraitLen, ToString(), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
			return Default();
		}


		protected internal virtual ElaValue Successor(ExecutionContext ctx)
		{
            ctx.Fail(ElaRuntimeError.TraitEnum, ToString(), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
			return Default();
		}


		protected internal virtual ElaValue Predecessor(ExecutionContext ctx)
		{
            ctx.Fail(ElaRuntimeError.TraitEnum, ToString(), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
			return Default();
		}


		protected internal virtual ElaValue GetValue(ElaValue index, ExecutionContext ctx)
		{
            ctx.Fail(ElaRuntimeError.TraitGet, ToString(), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
			return Default();
		}


		protected internal virtual void SetValue(ElaValue index, ElaValue value, ExecutionContext ctx)
		{
            ctx.Fail(ElaRuntimeError.TraitSet, ToString(), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));			
		}


		protected internal virtual ElaValue GetMax(ExecutionContext ctx)
		{
            ctx.Fail(ElaRuntimeError.TraitBound, ToString(), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
			return Default();
		}


		protected internal virtual ElaValue GetMin(ExecutionContext ctx)
		{
			ctx.Fail(ElaRuntimeError.TraitBound, ToString(), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));			
			return Default();
		}


		protected internal virtual ElaValue Concatenate(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
            ctx.Fail(ElaRuntimeError.TraitConcat, ThisToString(left, right, ctx), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
			return Default();
		}


		protected internal virtual ElaValue Add(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
            ctx.Fail(ElaRuntimeError.TraitNum, ThisToString(left, right, ctx), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
			return Default();
		}


		protected internal virtual ElaValue Subtract(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
            ctx.Fail(ElaRuntimeError.TraitNum, ThisToString(left, right, ctx), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
			return Default();
		}


		protected internal virtual ElaValue Multiply(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
            ctx.Fail(ElaRuntimeError.TraitNum, ThisToString(left, right, ctx), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
			return Default();
		}


		protected internal virtual ElaValue Divide(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
            ctx.Fail(ElaRuntimeError.TraitNum, ThisToString(left, right, ctx), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
			return Default();
		}


		protected internal virtual ElaValue Modulus(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
            ctx.Fail(ElaRuntimeError.TraitNum, ThisToString(left, right, ctx), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
			return Default();
		}


		protected internal virtual ElaValue Power(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
            ctx.Fail(ElaRuntimeError.TraitNum, ThisToString(left, right, ctx), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
			return Default();
		}


		protected internal virtual ElaValue BitwiseOr(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
            ctx.Fail(ElaRuntimeError.TraitBit, ThisToString(left, right, ctx), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
			return Default();
		}


		protected internal virtual ElaValue BitwiseAnd(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
            ctx.Fail(ElaRuntimeError.TraitBit, ThisToString(left, right, ctx), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
			return Default();
		}


		protected internal virtual ElaValue BitwiseXor(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			ctx.Fail(ElaRuntimeError.TraitBit, ThisToString(left, right, ctx), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
			return Default();
		}


		protected internal virtual ElaValue BitwiseNot(ExecutionContext ctx)
		{
			ctx.Fail(ElaRuntimeError.TraitBit, ToString(), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
			return Default();
		}


		protected internal virtual ElaValue ShiftRight(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			ctx.Fail(ElaRuntimeError.TraitBit, ThisToString(left, right, ctx), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
			return Default();
		}


		protected internal virtual ElaValue ShiftLeft(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			ctx.Fail(ElaRuntimeError.TraitBit, ThisToString(left, right, ctx), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
			return Default();
		}


		protected internal virtual ElaValue Negate(ExecutionContext ctx)
		{
			ctx.Fail(ElaRuntimeError.TraitNeg, ToString(), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
			return Default();
		}


		protected internal virtual bool Bool(ExecutionContext ctx)
		{
			ctx.Fail(ElaRuntimeError.TraitBool, ToString(), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
			return false;
		}


		protected internal virtual ElaValue Head(ExecutionContext ctx)
		{
			ctx.Fail(ElaRuntimeError.TraitFold, ToString(), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId)); 
			return Default();
		}


		protected internal virtual ElaValue Tail(ExecutionContext ctx)
		{
			ctx.Fail(ElaRuntimeError.TraitFold, ToString(), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
			return Default();
		}


		protected internal virtual bool IsNil(ExecutionContext ctx)
		{
			ctx.Fail(ElaRuntimeError.TraitFold, ToString(), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
			return false;
		}


		protected internal virtual ElaValue Cons(ElaObject instance, ElaValue value, ExecutionContext ctx)
		{
			ctx.Fail(ElaRuntimeError.TraitCons, ToString(), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
			return Default();
		}


		protected internal virtual ElaValue Nil(ExecutionContext ctx)
		{
			ctx.Fail(ElaRuntimeError.TraitCons, ToString(), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
			return Default();
		}


		protected internal virtual ElaValue Generate(ElaValue value, ExecutionContext ctx)
		{
			ctx.Fail(ElaRuntimeError.TraitGen, ToString(), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
			return Default();
		}


		protected internal virtual ElaValue GenerateFinalize(ExecutionContext ctx)
		{
			ctx.Fail(ElaRuntimeError.TraitGen, ToString(), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
			return Default();
		}


		protected internal virtual ElaValue GetField(string field, ExecutionContext ctx)
		{
			ctx.Fail(ElaRuntimeError.TraitFieldGet, ToString(), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
			return Default();
		}


		protected internal virtual void SetField(string field, ElaValue value, ExecutionContext ctx)
		{
			ctx.Fail(ElaRuntimeError.TraitFieldSet, ToString(), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));			
		}


		protected internal virtual bool HasField(string field, ExecutionContext ctx)
		{
			ctx.Fail(ElaRuntimeError.TraitFieldGet, ToString(), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
			return false;
		}


		protected internal virtual string Show(ExecutionContext ctx, ShowInfo info)
		{
			ctx.Fail(ElaRuntimeError.TraitShow, String.Empty, TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
			return String.Empty;
		}


		protected internal virtual ElaValue Convert(ElaTypeCode type, ExecutionContext ctx)
		{
			ctx.Fail(ElaRuntimeError.TraitConvert, ToString(), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
			return Default();
		}


		protected internal virtual ElaValue Call(ElaValue value, ExecutionContext ctx)
		{
			ctx.Fail(ElaRuntimeError.TraitCall, ToString(), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
			return Default();
		}


		protected internal virtual ElaValue Force(ExecutionContext ctx)
		{
			ctx.Fail(ElaRuntimeError.TraitThunk, ToString(), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
			return Default();
		}


		protected internal virtual string GetTag(ExecutionContext ctx)
		{
			ctx.Fail(ElaRuntimeError.TraitTag, ToString(), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
			return String.Empty;
		}


        protected internal virtual ElaValue Untag(ExecutionContext ctx)
        {
            ctx.Fail(ElaRuntimeError.TraitTag, ToString(), TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId));
            return Default();
        }
		#endregion


		#region Singleton Traits
		internal virtual ElaValue Successor(ElaValue @this, ExecutionContext ctx)
		{
			return Successor(ctx);
		}


		internal virtual ElaValue Predecessor(ElaValue @this, ExecutionContext ctx)
		{
			return Predecessor(ctx);
		}


		internal virtual ElaValue BitwiseNot(ElaValue @this, ExecutionContext ctx)
		{
			return BitwiseNot(ctx);
		}


		internal virtual ElaValue Negate(ElaValue @this, ExecutionContext ctx)
		{
			return Negate(ctx);
		}


		internal virtual bool Bool(ElaValue @this, ExecutionContext ctx)
		{
			return Bool(ctx);
		}


		internal virtual string Show(ElaValue @this, ExecutionContext ctx, ShowInfo info)
		{
			return Show(ctx, info);
		}


		internal virtual ElaValue Convert(ElaValue @this, ElaTypeCode type, ExecutionContext ctx)
		{
			return Convert(type, ctx);
		}
		#endregion


		#region Properties
		internal int TypeId { get; private set; }

		internal protected ElaTraits Traits { get; protected set; }
		#endregion
	}
}
