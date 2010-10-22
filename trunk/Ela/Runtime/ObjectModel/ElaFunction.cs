using System;
using Ela.Runtime.Reflection;

namespace Ela.Runtime.ObjectModel
{
	public abstract class ElaFunction : ElaObject
	{
		#region Construction
		protected ElaFunction(int parCount) : base(ObjectType.Function)
		{
			ParameterCount = parCount;
		}
		#endregion


		#region Methods
		public abstract RuntimeValue Call(params RuntimeValue[] args);


		public override ElaTypeInfo GetTypeInfo()
		{
			return new ElaFunctionInfo(null, 0, null, ParameterCount, false, false, false, AppliedParameters != null);
		}


		internal ElaFunction Clone()
		{
			return (ElaFunction)MemberwiseClone();
		}
		#endregion


		#region Properties
		internal int ParameterCount { get; set; }

		internal RuntimeValue[] AppliedParameters { get; set; }
		#endregion
	}
}
