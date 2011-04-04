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
        private const string TYPENAME = "typeName";
        private const string TYPECODE = "typeCode";
        private const string ISBYREF = "byRef";

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

            internal ElaInvalidObject() { }

            protected internal override string Show(ElaValue @this, ShowInfo info, ExecutionContext ctx)
            {
                return INVALID;
            }
        }
        #endregion


        #region Methods
		public virtual ElaPatterns GetSupportedPatterns()
		{
			return ElaPatterns.None;
		}


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
		#endregion


		#region Operations
		protected internal virtual ElaValue Equal(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			ctx.NoOperator(This(left, right), "equal");
			return Default();
		}


		protected internal virtual ElaValue NotEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			ctx.NoOperator(This(left, right), "notequal");
			return Default();
		}


		protected internal virtual ElaValue Greater(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			ctx.NoOperator(This(left, right), "greater");
			return Default();
		}


		protected internal virtual ElaValue Lesser(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			ctx.NoOperator(This(left, right), "lesser");			
			return Default();
		}


		protected internal virtual ElaValue GreaterEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			ctx.NoOperator(This(left, right), "greaterequal");
			return Default();
		}


		protected internal virtual ElaValue LesserEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			ctx.NoOperator(This(left, right), "lesserequal");
			return Default();
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


		protected internal virtual void SetValue(ElaValue index, ElaValue value, ExecutionContext ctx)
		{
			ctx.NoOperator(new ElaValue(this), "set");						
		}


		protected internal virtual ElaValue GetMax(ElaValue @this, ExecutionContext ctx)
		{
			ctx.NoOperator(@this, "max");
			return Default();
		}


		protected internal virtual ElaValue GetMin(ElaValue @this, ExecutionContext ctx)
		{
			ctx.NoOperator(@this, "min");
			return Default();
		}


		protected internal virtual ElaValue Concatenate(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			ctx.NoOperator(This(left, right), "concat");
			return Default();
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


		protected internal virtual bool Bool(ElaValue @this, ExecutionContext ctx)
		{
			ctx.NoOperator(@this, "bool");
			return false;
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


		protected internal virtual ElaValue GetField(string field, ExecutionContext ctx)
		{
			ctx.NoOperator(new ElaValue(this), "fieldget");
			return Default();
		}


		protected internal virtual void SetField(string field, ElaValue value, ExecutionContext ctx)
		{
			ctx.NoOperator(new ElaValue(this), "fieldset");						
		}


		protected internal virtual bool HasField(string field, ExecutionContext ctx)
		{
			ctx.NoOperator(new ElaValue(this), "fieldhas");
			return false;
		}


		protected internal virtual string Show(ElaValue @this, ShowInfo info, ExecutionContext ctx)
		{
			ctx.NoOperator(@this, "show");
			return String.Empty;
		}


		protected internal virtual ElaValue Convert(ElaValue @this, ElaTypeCode type, ExecutionContext ctx)
		{
			ctx.NoOperator(@this, "convert");
			return Default();
		}


		protected internal virtual ElaValue Call(ElaValue arg, ExecutionContext ctx)
		{
			ctx.NoOperator(new ElaValue(this), "call");
			return Default();
		}


		protected internal virtual ElaValue Force(ElaValue @this, ExecutionContext ctx)
		{
			return @this;
		}


		protected internal virtual string GetTag(ExecutionContext ctx)
		{
			ctx.NoOperator(new ElaValue(this), "gettag");
			return String.Empty;
		}


		protected internal virtual ElaValue Untag(ExecutionContext ctx)
        {
			ctx.NoOperator(new ElaValue(this), "untag");
			return Default();
        }


		protected internal virtual ElaValue Clone(ExecutionContext ctx)
		{
			ctx.NoOperator(new ElaValue(this), "clone");
			return Default();
		}
		#endregion


		#region Properties
		internal int TypeId { get; set; }
		#endregion
	}
}
