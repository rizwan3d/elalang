using System;

namespace Ela.Runtime.ObjectModel
{
	internal sealed class ElaDummyObject : ElaObject
	{
		internal static readonly ElaDummyObject Instance = new ElaDummyObject();

		private ElaDummyObject() : base(ElaTypeCode.Object)
		{

		}       
        
		public override string  ToString(string format, IFormatProvider formatProvider)
		{
			return String.Empty;
		}

		protected internal override ElaValue Tail(ExecutionContext ctx)
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

		internal override ElaValue Force(ElaValue @this, ExecutionContext ctx)
		{
			return Default();
		}
        
		internal override string GetTag(ExecutionContext ctx)
		{
			return String.Empty;
		}
        
		internal override ElaValue Untag(ExecutionContext ctx)
		{
			return Default();
		}
	}
}
