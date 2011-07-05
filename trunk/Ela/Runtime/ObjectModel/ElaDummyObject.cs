using System;
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
		protected internal override void SetValue(ElaValue index, ElaValue value, ExecutionContext ctx)
		{
			
		}


		protected internal override bool Bool(ElaValue @this, ExecutionContext ctx)
		{
			return false;
		}


		protected internal override ElaValue Generate(ElaValue value, ExecutionContext ctx)
		{
			return Default();
		}


		protected internal override ElaValue GenerateFinalize(ExecutionContext ctx)
		{
			return Default();
		}


		protected internal override ElaValue GetField(string field, ExecutionContext ctx)
		{
			return Default();
		}


		protected internal override void SetField(string field, ElaValue value, ExecutionContext ctx)
		{
			
		}


		protected internal override bool HasField(string field, ExecutionContext ctx)
		{
			return false;
		}


		protected internal override string Show(ElaValue @this, ShowInfo info, ExecutionContext ctx)
		{
			return String.Empty;
		}


		protected internal override ElaValue Call(ElaValue arg, ExecutionContext ctx)
		{
			return Default();
		}


		protected internal override ElaValue Force(ElaValue @this, ExecutionContext ctx)
		{
			return Default();
		}


		protected internal override ElaValue Untag(ElaValue @this, ExecutionContext ctx)
		{
			return @this;
		}
		#endregion
	}
}
