using System;
using System.Collections.Generic;
using Ela.CodeModel;

namespace Ela.Compilation
{
    public sealed class ExportVars
    {
        #region Construction
        private Dictionary<String,ExportVarData> map;

        public ExportVars()
        {
            map = new Dictionary<String,ExportVarData>();
        }
        #endregion


		#region Methods
        public void AddName(string name)
        {
            AddName(name, ElaBuiltinKind.None, CallConv.Standard);
        }


		public void AddName(string name, ElaBuiltinKind kind)
        {
            map.Remove(name);
            map.Add(name, new ExportVarData(kind, CallConv.Standard));
        }
        
        public void AddName(string name, ElaBuiltinKind kind, CallConv conv)
        {
            map.Remove(name);
            map.Add(name, new ExportVarData(kind, conv));
        }

        public bool FindName(string name, out ExportVarData data)
        {
            return map.TryGetValue(name, out data);
        }


		internal Dictionary<String,ExportVarData> GetMap()
		{
			return map;
		}
        #endregion
    }
}
