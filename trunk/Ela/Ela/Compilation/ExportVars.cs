using System;
using System.Collections.Generic;
using Ela.CodeModel;
using Ela.Linking;
using Ela.Runtime.Classes;

namespace Ela.Compilation
{
    public sealed class ExportVars
    {
        #region Construction
        private readonly Dictionary<String,ExportVarData> variables;
        private readonly CodeAssembly asm;

        public ExportVars(CodeAssembly asm)
        {
            this.asm = asm;
            variables = new Dictionary<String,ExportVarData>();
        }
        #endregion


		#region Methods
        public void AddVariable(string name, ElaBuiltinKind kind, ElaVariableFlags flags, int moduleHandle, int address)
        {
            variables.Remove(name);
            variables.Add(name, new ExportVarData(kind, flags, moduleHandle, address));
        }

        public bool FindVariable(string name, out ExportVarData data)
        {
            return variables.TryGetValue(name, out data);
        }

        public bool HasVariable(string name)
        {
            return variables.ContainsKey(name);
        }
        #endregion
    }
}
