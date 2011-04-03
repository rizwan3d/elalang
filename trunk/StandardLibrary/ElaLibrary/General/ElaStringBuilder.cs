using System;
using System.Text;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;

namespace Ela.Library.General
{
	public sealed class ElaStringBuilder : ElaObject
	{
		#region Construction
		private const string TYPE_NAME = "stringBuilder";

		public ElaStringBuilder(StringBuilder builder)
		{
			Builder = builder;
		}
		#endregion


		#region Methods
		protected override string GetTypeName()
		{
			return TYPE_NAME;
		}


		public override int GetHashCode()
		{
			return Builder.GetHashCode();
		}


		protected override ElaValue Equal(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return new ElaValue(left.ReferenceEquals(right));
		}


		protected override ElaValue NotEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return new ElaValue(left.ReferenceEquals(right));
		}


		protected override string Show(ElaValue @this, ShowInfo info, ExecutionContext ctx)
		{
			return new ElaValue(Builder.ToString()).Show(info, ctx);
		}


		protected override ElaValue Convert(ElaValue @this, ElaTypeCode type, ExecutionContext ctx)
		{
			if (type == ElaTypeCode.String)
				return new ElaValue(Builder.ToString());

			ctx.ConversionFailed(@this, type);
			return Default();
		}


		protected override ElaValue GetLength(ExecutionContext ctx)
		{
			return new ElaValue(Builder.Length);
		}
		#endregion


		#region Properties
		internal StringBuilder Builder { get; private set; }
		#endregion
	}
}
