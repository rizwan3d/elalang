﻿using System;
using System.Collections.Generic;

namespace Ela.Runtime.ObjectModel
{
	internal sealed class ElaDummyObject : ElaObject
	{
		#region Construction
		internal static readonly ElaDummyObject Instance = new ElaDummyObject();

		private ElaDummyObject() : base(ElaTypeCode.Object)
		{

		}
		#endregion


		#region Methods
		protected internal override bool Is<T>(ElaValue value)
		{
			return false;
		}


		protected internal override T As<T>(ElaValue value)
		{
			return default(T);
		}


		protected internal override int AsInteger(ElaValue value)
		{
			return default(Int32);
		}


		protected internal override float AsSingle(ElaValue value)
		{
			return default(Single);
		}


		protected internal override bool AsBoolean(ElaValue value)
		{
			return default(Boolean);
		}


		protected internal override char AsChar(ElaValue value)
		{
			return default(Char);
		}


		public override ElaPatterns GetSupportedPatterns()
		{
			return ElaPatterns.None;
		}


		public override ElaTypeInfo GetTypeInfo()
		{
			return null;
		}


		internal protected override int Compare(ElaValue @this, ElaValue other)
		{
			return 0;
		}


		public override string ToString()
		{
			return String.Empty;
		}
		#endregion


		#region Operations
		protected internal override bool Equal(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
            return false;
		}


		protected internal override bool NotEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
            return false;
		}


		protected internal override bool Greater(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
            return false;
		}


		protected internal override bool Lesser(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
            return false;
		}


		protected internal override bool GreaterEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
            return false;
		}


		protected internal override bool LesserEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
            return false;
		}


		protected internal override ElaValue GetLength(ExecutionContext ctx)
		{
			return Default();
		}


		protected internal override ElaValue Successor(ElaValue @this, ExecutionContext ctx)
		{
			return Default();
		}


		protected internal override ElaValue Predecessor(ElaValue @this, ExecutionContext ctx)
		{
			return Default();
		}


		protected internal override ElaValue GetValue(ElaValue index, ExecutionContext ctx)
		{
			return Default();
		}


		protected internal override ElaValue GetMax(ElaValue @this, ExecutionContext ctx)
		{
			return Default();
		}


		protected internal override ElaValue GetMin(ElaValue @this, ExecutionContext ctx)
		{
			return Default();
		}


		protected internal override ElaValue Concatenate(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return Default();
		}


		protected internal override ElaValue Add(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return Default();
		}


		protected internal override ElaValue Subtract(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return Default();
		}


		protected internal override ElaValue Multiply(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return Default();
		}


		protected internal override ElaValue Divide(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return Default();
		}


		protected internal override ElaValue Remainder(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return Default();
		}


		protected internal override ElaValue Power(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return Default();
		}


		protected internal override ElaValue BitwiseOr(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return Default();
		}


		protected internal override ElaValue BitwiseAnd(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return Default();
		}


		protected internal override ElaValue BitwiseXor(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return Default();
		}


		protected internal override ElaValue BitwiseNot(ElaValue @this, ExecutionContext ctx)
		{
			return Default();
		}


		protected internal override ElaValue ShiftRight(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return Default();
		}


		protected internal override ElaValue ShiftLeft(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return Default();
		}


		protected internal override ElaValue Negate(ElaValue @this, ExecutionContext ctx)
		{
			return Default();
		}


		protected internal override ElaValue Head(ExecutionContext ctx)
		{
			return Default();
		}


		protected internal override ElaValue Tail(ExecutionContext ctx)
		{
			return Default();
		}


		protected internal override bool IsNil(ExecutionContext ctx)
		{
			return false;
		}


		protected internal override ElaValue Cons(ElaObject instance, ElaValue value, ExecutionContext ctx)
		{
			return Default();
		}


		protected internal override ElaValue Nil(ExecutionContext ctx)
		{
			return Default();
		}


		protected internal override ElaValue Generate(ElaValue value, ExecutionContext ctx)
		{
			return Default();
		}


		protected internal override ElaValue GenerateFinalize(ExecutionContext ctx)
		{
			return Default();
		}


		protected internal override bool Has(string field, ExecutionContext ctx)
		{
			return false;
		}


		protected internal override string Show(ElaValue @this, ShowInfo info, ExecutionContext ctx)
		{
			return String.Empty;
		}


        protected internal override ElaValue Convert(ElaValue @this, ElaTypeInfo type, ExecutionContext ctx)
		{
			return Default();
		}


		protected internal override ElaValue Call(ElaValue arg, ExecutionContext ctx)
		{
			return Default();
		}


		internal override ElaValue Force(ElaValue @this, ExecutionContext ctx)
		{
			return Default();
		}


		protected internal override string GetTag(ExecutionContext ctx)
		{
			return String.Empty;
		}


		protected internal override ElaValue Untag(ExecutionContext ctx)
		{
			return Default();
		}
		#endregion
	}
}
