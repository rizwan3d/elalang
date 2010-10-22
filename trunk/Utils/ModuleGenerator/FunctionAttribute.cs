using System;

namespace Ela.ModuleGenerator
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	public sealed class FunctionAttribute : Attribute
	{
		#region Construction
		public FunctionAttribute(string name)
		{
			Name = name;
		}
		#endregion


		#region Properties
		public string Name { get; private set; }

		public bool UnlimitedParameters { get; set; }
		#endregion
	}
}
