using System;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;

namespace Ela.Library
{
	public abstract class ElaRefObject : ElaObject
	{
		#region Construction
		private string typeName;

		protected ElaRefObject(string typeName, ElaTraits traits) : base(traits | ElaTraits.Eq | ElaTraits.Show)
		{
			this.typeName = typeName;
		}
		#endregion


		#region Traits
		protected override string GetTypeName()
		{
			return typeName;
		}


		protected override ElaValue Equals(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return new ElaValue(left.ReferenceEquals(right));
		}


		protected override ElaValue NotEquals(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return new ElaValue(!left.ReferenceEquals(right));
		}


		protected override string Show(ExecutionContext ctx, ShowInfo info)
		{
			return "[" + typeName + "]";
		}
		#endregion
	}
}
