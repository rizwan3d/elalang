using System;
using System.Collections.Generic;
using Ela.CodeModel;

namespace Ela.Compilation
{
	public sealed class BuiltinVars
	{
		#region Construction
		private Dictionary<String,ElaBuiltinKind> map;

		internal BuiltinVars()
		{
			map = new Dictionary<String,ElaBuiltinKind>();
		}
		#endregion


		#region Methods
		public void AddVar(string name, ElaBuiltinKind var)
		{
			map.Remove(name);
			map.Add(name, var);
		}


		public ElaBuiltinKind FindVar(string name)
		{
			ElaBuiltinKind sv;

			if (!map.TryGetValue(name, out sv))
				sv = ElaBuiltinKind.None;

			return sv;
		}
		#endregion
	}
}
