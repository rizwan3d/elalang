using System;
using System.Collections.Generic;
using Ela.CodeModel;

namespace Ela.Compilation
{
    public sealed class ExportVars
    {
        #region Construction
        private Dictionary<String,ElaBuiltinKind> map;

        internal ExportVars()
        {
            map = new Dictionary<String,ElaBuiltinKind>();
        }
        #endregion



        #region Methods
        public void AddBuiltin(string name, ElaBuiltinKind kind)
        {
            map.Remove(name);
            map.Add(name, kind);
        }


        public ElaBuiltinKind FindBuiltin(string name)
        {
            var ret = default(ElaBuiltinKind);
            map.TryGetValue(name, out ret);
            return ret;
        }
        #endregion
    }
}
