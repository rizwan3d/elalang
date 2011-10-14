using System;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;

namespace Ela.Library
{
	public abstract class ElaRefObject : ElaObject
	{
		#region Construction
		private string typeName;

		protected ElaRefObject(string typeName)
		{
			this.typeName = typeName;
		}
		#endregion


        #region Operations
        protected override string GetTypeName()
		{
			return typeName;
		}


		protected override ElaValue Equal(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return new ElaValue(left.ReferenceEquals(right));
		}


		protected override ElaValue NotEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return new ElaValue(!left.ReferenceEquals(right));
		}


		protected override string Show(ElaValue @this, ShowInfo info, ExecutionContext ctx)
		{
			return "[" + typeName + "]";
		}
		#endregion
	}
}
