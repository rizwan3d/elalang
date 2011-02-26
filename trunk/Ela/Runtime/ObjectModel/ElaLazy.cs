using System;

namespace Ela.Runtime.ObjectModel
{
	internal sealed class ElaLazy : ElaObject
	{
		#region Construction
		internal ElaLazy(ElaFunction function) : base(ElaTypeCode.Lazy, ElaTraits.Thunk)
		{
			Function = function;
			_value = default(ElaValue);
		}
		#endregion


		#region Methods
        public override int GetHashCode()
        {
            return Value.Ref != null ? Value.GetHashCode() : 0;
        }


		public string AsString()
		{
			return Force().AsString();
		}


		public bool AsBoolean()
		{
			return Force().AsBoolean();
		}


		public char AsChar()
		{
			return Force().AsChar();
		}


		public double AsDouble()
		{
			return Force().AsDouble();
		}


		public float AsSingle()
		{
			return Force().AsSingle();
		}


		public int AsInteger()
		{
			return Force().AsInteger();
		}


		public long AsLong()
		{
			return Force().AsLong();
		}


		public ElaList AsList()
		{
			return Force().AsList();
		}


		public ElaTuple AsTuple()
		{
			return Force().AsTuple();
		}


		public ElaRecord AsRecord()
		{
			return Force().AsRecord();
		}


		public ElaFunction AsFunction()
		{
			return Force().AsFunction();
		}


		public object AsObject()
		{
			switch (Force().TypeCode)
			{
				case ElaTypeCode.Boolean: return Value.AsBoolean();
				case ElaTypeCode.Char: return Value.AsChar();
				case ElaTypeCode.Double: return Value.AsDouble();
				case ElaTypeCode.Function: return Value.AsFunction();
				case ElaTypeCode.Integer: return Value.AsInteger();
				case ElaTypeCode.List: return Value.AsList();
				case ElaTypeCode.Long: return Value.AsLong();
				case ElaTypeCode.Record: return Value.AsRecord();
				case ElaTypeCode.Single: return Value.AsSingle();
				case ElaTypeCode.String: return Value.AsString();
				case ElaTypeCode.Tuple: return Value.AsTuple();
				case ElaTypeCode.Unit: return null;
				case ElaTypeCode.Lazy: return Value.AsObject();
				default:
					if (Value.Ref != null)
						return Value.Ref;
					else
						throw new NotSupportedException();
			}
		}


		internal ElaValue Force()
		{
			if (Value.Ref == null)
				Value = Function.CallWithThrow(
					Function.LastParameter.Ref != null ? Function.LastParameter :
					new ElaValue(ElaUnit.Instance));

			return Value;
		}


		protected internal override ElaValue Force(ExecutionContext ctx)
		{
			if (Value.Ref == null)
				Value = Function.Call(
					Function.LastParameter.Ref != null ? Function.LastParameter :
					new ElaValue(ElaUnit.Instance), ctx);

			return Value;
		}


		protected internal override ElaValue Cons(ElaObject next, ElaValue value, ExecutionContext ctx)
		{
			return new ElaValue(new ElaList(this, value));
		}


		protected internal override ElaValue Generate(ElaValue value, ExecutionContext ctx)
		{
			return new ElaValue(new ElaList(this, value));
		}


		protected internal override ElaValue GenerateFinalize(ExecutionContext ctx)
		{
			return new ElaValue(this);
		}


		protected internal override ElaValue Tail(ExecutionContext ctx)
		{
			return Value.Ref != null && (Value.Ref.Traits & ElaTraits.Fold) == ElaTraits.Fold ? Value.Ref.Tail(ctx) : 
				new ElaValue(ElaList.Nil);
		}


		protected internal override ElaValue Head(ExecutionContext ctx)
		{
			return Value.Ref != null ? 
				((Value.Ref.Traits & ElaTraits.Fold) == ElaTraits.Fold ? Value.Ref.Head(ctx) : Value) : 
				Default();
		}


		protected internal override bool IsNil(ExecutionContext ctx)
		{
			return false;
		}
		#endregion


		#region Properties
		internal ElaFunction Function;

		private ElaValue _value;
		internal ElaValue Value
		{
			get { return _value; }
			set
			{
				Function = null;
				_value = value;
			}
		}
		#endregion
	}
}