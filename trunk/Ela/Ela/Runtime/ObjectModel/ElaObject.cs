using System;
using System.Collections.Generic;

namespace Ela.Runtime.ObjectModel
{
	public abstract class ElaObject
	{
		#region Construction
		internal static readonly ExecutionContext DummyContext = new ExecutionContext();
		internal const string UNDEF = "<unknown>";
        internal const string INVALID = "<INVALID>";

		protected ElaObject() : this(ElaTypeCode.Object)
		{
			
		}

		
		internal ElaObject(ElaTypeCode type)
		{
			TypeId = (Int32)type;
		} 
		#endregion


        #region Nested Classes
        private sealed class ElaInvalidObject : ElaObject
        {
            internal static readonly ElaInvalidObject Instance = new ElaInvalidObject();

            internal ElaInvalidObject() 
			{ 
			
			}

            protected internal override string Show(ElaValue @this, ShowInfo info, ExecutionContext ctx)
            {
                return INVALID;
            }
        }
        #endregion


        #region Methods
        protected internal virtual bool Is<T>(ElaValue value) where T : ElaObject
        {
            return this is T;
        }
        
        
        protected internal virtual T As<T>(ElaValue value) where T : ElaObject
        {
            return this as T;
        }


        protected internal virtual int AsInteger(ElaValue value)
        {
            throw InvalidCast(ElaTypeCode.Integer, value.GetTypeName());
        }


        protected internal virtual float AsSingle(ElaValue value)
        {
            throw InvalidCast(ElaTypeCode.Single, value.GetTypeName());
        }


        protected internal virtual bool AsBoolean(ElaValue value)
        {
            throw InvalidCast(ElaTypeCode.Boolean, value.GetTypeName());
        }


        protected internal virtual char AsChar(ElaValue value)
        {
            throw InvalidCast(ElaTypeCode.Char, value.GetTypeName());
        }


        protected InvalidCastException InvalidCast(ElaTypeCode target, string source)
        {
            return InvalidCast(TypeCodeFormat.GetShortForm(target), source);
        }


        protected InvalidCastException InvalidCast(string target, string source)
        {
            return new InvalidCastException(Strings.GetMessage("InvalidCast", source, target));
        }


		public virtual ElaPatterns GetSupportedPatterns()
		{
			return ElaPatterns.None;
		}


        public virtual ElaTypeInfo GetTypeInfo()
        {
            return CreateTypeInfo();
        }


        protected ElaTypeInfo CreateTypeInfo(params ElaRecordField[] fields)
        {
            return new ElaTypeInfo(GetTypeName(), TypeId,
                TypeId != ElaMachine.INT ||
                TypeId != ElaMachine.CHR ||
                TypeId != ElaMachine.REA ||
                TypeId != ElaMachine.BYT,
                GetType(),
                fields);		
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
                return Show(new ElaValue(this), ShowInfo.Debug, DummyContext);
		}


		protected ElaValue Default()
		{
			return new ElaValue(ElaInvalidObject.Instance);
		}


		protected internal virtual string GetTypeName()
		{
			return TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId);
		}


		private string ThisToString(ElaValue first, ElaValue second, ExecutionContext ctx)
		{
			return first.Ref == this ? first.Ref.Show(first, ShowInfo.Default, ctx) :
				second.Ref.Show(second, ShowInfo.Default, ctx);
		}


		private ElaValue This(ElaValue first, ElaValue second)
		{
			return first.Ref == this ? first : second;
		}


        //Returns true for external function and FastCall functions
        internal virtual bool IsExternFun()
        {
            return false;
        }
		#endregion


		#region Operations
        protected internal virtual bool Equal(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
            return left.Ref == right.Ref;
		}


		protected internal virtual bool NotEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
            return left.Ref != right.Ref;
		}


		protected internal virtual bool Greater(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
            ctx.NoOperator(This(left, right), "greater"); 
            return false;
		}


		protected internal virtual bool Lesser(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			ctx.NoOperator(This(left, right), "lesser");
            return false;
		}


		protected internal virtual bool GreaterEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
            ctx.NoOperator(This(left, right), "greaterequal");
            return false;
		}


		protected internal virtual bool LesserEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
            ctx.NoOperator(This(left, right), "lesserequal");
            return false;
		}


		protected internal virtual ElaValue GetLength(ExecutionContext ctx)
		{
			ctx.NoOperator(new ElaValue(this), "length");
			return Default();
		}


		protected internal virtual ElaValue Successor(ElaValue @this, ExecutionContext ctx)
		{
			ctx.NoOperator(@this, "succ");
			return Default();
		}


		protected internal virtual ElaValue Predecessor(ElaValue @this, ExecutionContext ctx)
		{
			ctx.NoOperator(@this, "pred");
			return Default();
		}


		protected internal virtual ElaValue GetValue(ElaValue index, ExecutionContext ctx)
		{
			ctx.NoOperator(new ElaValue(this), "get");
			return Default();
		}


		protected internal virtual ElaValue Concatenate(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
            var str1 = left.Force(ctx).Show(ShowInfo.Default, ctx);

            if (ctx.Failed)
                return Default();

            var str2 = right.Force(ctx).Show(ShowInfo.Default, ctx);

            if (ctx.Failed)
                return Default();

            return new ElaValue(str1 + str2);
		}


		protected internal virtual ElaValue Add(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			ctx.NoOperator(This(left, right), "add");
			return Default();
		}


		protected internal virtual ElaValue Subtract(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			ctx.NoOperator(This(left, right), "subtract");
			return Default();
		}


		protected internal virtual ElaValue Multiply(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			ctx.NoOperator(This(left, right), "multiply");
			return Default();
		}


		protected internal virtual ElaValue Divide(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			ctx.NoOperator(This(left, right), "divide");
			return Default();
		}


		protected internal virtual ElaValue Remainder(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			ctx.NoOperator(This(left, right), "remainder");
			return Default();
		}


		protected internal virtual ElaValue Power(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			ctx.NoOperator(This(left, right), "power");
			return Default();
		}


		protected internal virtual ElaValue BitwiseOr(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			ctx.NoOperator(This(left, right), "bitwiseor");
			return Default();
		}


		protected internal virtual ElaValue BitwiseAnd(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			ctx.NoOperator(This(left, right), "bitwiseand");
			return Default();
		}


		protected internal virtual ElaValue BitwiseXor(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			ctx.NoOperator(This(left, right), "bitwisexor");
			return Default();
		}


		protected internal virtual ElaValue BitwiseNot(ElaValue @this, ExecutionContext ctx)
		{
			ctx.NoOperator(@this, "bitwisenot");
			return Default();
		}


		protected internal virtual ElaValue ShiftRight(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			ctx.NoOperator(This(left, right), "shiftright");
			return Default();
		}


		protected internal virtual ElaValue ShiftLeft(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			ctx.NoOperator(This(left, right), "shiftleft");
			return Default();
		}


		protected internal virtual ElaValue Negate(ElaValue @this, ExecutionContext ctx)
		{
			ctx.NoOperator(@this, "negate");
			return Default();
		}


		protected internal virtual ElaValue Head(ExecutionContext ctx)
		{
			ctx.NoOperator(new ElaValue(this), "head");
			return Default();
		}


		protected internal virtual ElaValue Tail(ExecutionContext ctx)
		{
			ctx.NoOperator(new ElaValue(this), "tail");
			return Default();
		}


		protected internal virtual bool IsNil(ExecutionContext ctx)
		{
			ctx.NoOperator(new ElaValue(this), "isnil");
			return false;
		}


		protected internal virtual ElaValue Cons(ElaObject instance, ElaValue value, ExecutionContext ctx)
		{
			ctx.NoOperator(new ElaValue(instance), "cons");
			return Default();
		}


		protected internal virtual ElaValue Nil(ExecutionContext ctx)
		{
			return new ElaValue(ElaList.Empty);
		}


		protected internal virtual ElaValue Generate(ElaValue value, ExecutionContext ctx)
		{
			ctx.NoOperator(new ElaValue(this), "gen");
			return Default();
		}


		protected internal virtual ElaValue GenerateFinalize(ExecutionContext ctx)
		{
			ctx.NoOperator(new ElaValue(this), "genfin");
			return Default();
		}


		protected internal virtual bool Has(string field, ExecutionContext ctx)
		{
			ctx.NoOperator(new ElaValue(this), "has");
			return false;
		}


		protected internal virtual string Show(ElaValue @this, ShowInfo info, ExecutionContext ctx)
		{
			//ctx.NoOperator(@this, "show");
			return String.Empty;
		}


		protected internal virtual ElaValue Convert(ElaValue @this, ElaTypeInfo type, ExecutionContext ctx)
		{
			ctx.NoOperator(@this, "convert");
			return Default();
		}


		protected internal virtual ElaValue Call(ElaValue arg, ExecutionContext ctx)
		{
			ctx.NoOperator(new ElaValue(this), "call");
			return Default();
        }


        protected internal virtual ElaValue Call(ElaValue arg1, ElaValue arg2, ExecutionContext ctx)
        {
            ctx.NoOperator(new ElaValue(this), "call");
            return Default();
        }


		internal virtual ElaValue Force(ElaValue @this, ExecutionContext ctx)
		{
			return @this;
		}


		protected internal virtual string GetTag(ExecutionContext ctx)
		{
			return String.Empty;
		}


		protected internal virtual ElaValue Untag(ExecutionContext ctx)
        {
			ctx.NoOperator(new ElaValue(this), "untag");
			return Default();
        }
		#endregion


		#region Properties
		internal int TypeId { get; set; }
		#endregion
	}
}
