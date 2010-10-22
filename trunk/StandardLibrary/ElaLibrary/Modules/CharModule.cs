using System;
using Ela.ModuleGenerator;

namespace Ela.StandardLibrary.Modules
{
	public partial class CharModule
	{
		#region Construction
		public CharModule()
		{

		}
		#endregion


		#region Methods
		[Function("isLower")]
		internal bool IsLower(char c)
		{
			return Char.IsLower(c);
		}


		[Function("isUpper")]
		internal bool IsUpper(char c)
		{
			return Char.IsUpper(c);
		}


		[Function("upper")]
		internal char ToUpper(char c)
		{
			return Char.ToUpper(c);
		}


		[Function("lower")]
		internal char ToLower(char c)
		{
			return Char.ToLower(c);
		}
		#endregion
	}
}
