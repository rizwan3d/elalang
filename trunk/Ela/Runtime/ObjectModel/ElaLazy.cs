using System;

namespace Ela.Runtime.ObjectModel
{
	internal sealed class ElaLazy : ElaObject
	{
		#region Construction
		internal ElaLazy(ElaFunction function) : base(ObjectType.Lazy, ElaTraits.Thunk)
		{
			Function = function;
			_value = default(ElaValue);
		}
		#endregion


		#region Methods
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


		public ElaArray AsArray()
		{
			return Force().AsArray();
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
			switch (Force().DataType)
			{
				case ObjectType.Array: return Value.AsArray();
				case ObjectType.Boolean: return Value.AsBoolean();
				case ObjectType.Char: return Value.AsChar();
				case ObjectType.Double: return Value.AsDouble();
				case ObjectType.Function: return Value.AsFunction();
				case ObjectType.Integer: return Value.AsInteger();
				case ObjectType.List: return Value.AsList();
				case ObjectType.Long: return Value.AsLong();
				case ObjectType.Record: return Value.AsRecord();
				case ObjectType.Single: return Value.AsSingle();
				case ObjectType.String: return Value.AsString();
				case ObjectType.Tuple: return Value.AsTuple();
				case ObjectType.Unit: return null;
				case ObjectType.Lazy: return Value.AsObject();
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
			return new ElaValue(new ElaList(next, value));
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
			return Value.Ref != null ? Value.Ref.Tail(ctx) : Default();
		}


		protected internal override ElaValue Head(ExecutionContext ctx)
		{
			return Value.Ref != null ? 
				((Value.Ref.Traits & ElaTraits.Fold) == ElaTraits.Fold ? Value.Ref.Head(ctx) : Value) : 
				Default();
		}


		protected internal override bool IsNil(ExecutionContext ctx)
		{
			return Value.Ref != null ? Value.Ref.IsNil(ctx) : true;
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