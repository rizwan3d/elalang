using System;

namespace Ela.Runtime.ObjectModel
{
	internal sealed class ElaComposition : ElaFunction
	{
		#region Construction
		private ElaFunction firstFunction;
		private ElaFunction secondFunction;
		
		internal ElaComposition(ElaFunction first, ElaFunction sec) : base(first.ParameterCount)
		{
			this.firstFunction = first;
			this.secondFunction = sec;
		}
		#endregion


		#region Methods
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			var ret = firstFunction.Call(args);
			return secondFunction.Call(ret);
		}
		#endregion
	}
}