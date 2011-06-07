using System;
using System.Collections.Generic;
using Ela.CodeModel;

namespace Ela.Compilation
{
    public sealed class ExportVars
    {
        #region Construction
        private Dictionary<String,ElaBuiltinKind> map;

        public ExportVars()
        {
            map = new Dictionary<String,ElaBuiltinKind>();
        }
        #endregion


		#region Methods
		public void AddName(string name, ElaBuiltinKind kind)
        {
            map.Remove(name);
            map.Add(name, kind);
        }


        public bool FindName(string name, out ElaBuiltinKind kind)
        {
            return map.TryGetValue(name, out kind);
        }


		internal Dictionary<String,ElaBuiltinKind> GetMap()
		{
			return map;
		}
        #endregion
    }
}
